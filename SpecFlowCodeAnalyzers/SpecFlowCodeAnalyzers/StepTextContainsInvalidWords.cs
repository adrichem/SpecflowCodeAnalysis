namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StepTextContainsInvalidWords: DiagnosticAnalyzer
    {
        private readonly XmlSchemaSet Schemas = new XmlSchemaSet();
        public const string BannedWordsFileName = "BannedStepTextPatterns.xml";
        private const string mySchema = @"<?xml version='1.0' encoding='utf-8'?>
            <xs:schema targetNamespace='{0}' elementFormDefault='qualified' xmlns='{0}' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	            <xs:complexType name='itemType'>
		            <xs:attribute name='phrase' type='xs:string' use='required'/>
		            <xs:attribute name='severity' use='required'>
			            <xs:simpleType>
				            <xs:restriction base='xs:string'>
					            <xs:enumeration value='Error'/>
					            <xs:enumeration value='Warning'/>
					            <xs:enumeration value='Info'/>
				            </xs:restriction>
				            </xs:simpleType>
			            </xs:attribute>
		            <xs:attribute name='message' type='xs:string' use='optional'/>
		            <xs:attribute name='ignoreCase' type='xs:boolean' default='false' use='optional'/>
	            </xs:complexType>
	            <xs:element name='banlist'>
		            <xs:complexType>
			            <xs:sequence>
				            <xs:element name='items' minOccurs='1' maxOccurs='1'>
					            <xs:complexType>
						            <xs:sequence>
							            <xs:element name='item' type='itemType' minOccurs='0' maxOccurs='unbounded'/>
						            </xs:sequence>
					            </xs:complexType>
				            </xs:element>
			            </xs:sequence>
		            </xs:complexType>
	            </xs:element>
            </xs:schema>
            ";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.RegExContainsBannedWord
            , "StepText contains banned phrase"
            , "{0}"
            , Helpers.DiagnosticCategory
            , DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "This phrase is banned."
        );

        private static readonly DiagnosticDescriptor InvalidFileRule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.RegExContainsBannedWordInvalidFileFormat
            , "Invalid file"
            , "{0}"
            , Helpers.DiagnosticCategory
            , DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The contents of the file are invalid."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, InvalidFileRule); 

        public StepTextContainsInvalidWords()
        {
            Schemas.Add(null
                , XmlReader.Create(
                    new StringReader(
                        string.Format(mySchema, BannedItems.MyNamespace)
                    )
                )
            );
        }
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(x =>
            {
                var BannedWords = ReadBannedWords(context, x.Options.AdditionalFiles);
                if (!BannedWords.Any()) return;
                if (!x.Compilation.SpecFlowIsReferenced()) return;
                x.RegisterSymbolAction(ctxt => AnalyzeMethod(ctxt, BannedWords), SymbolKind.Method);
            });
        }

        /// <summary>
        /// Analyzes the attributes of a method
        /// </summary>
        /// <param name="Context">The analysis context.</param>
        /// <param name="BannedWords">The banned phrases.</param>
        private void AnalyzeMethod(SymbolAnalysisContext Context, ImmutableArray<BannedItem> BannedWords)
        {
            var AttributeTypesToCheck = Helpers.GetStepDefinitionTypeSymbols(Context.Compilation);
            (Context.Symbol as IMethodSymbol)
                .GetAttributes()
                .Where(a => AttributeTypesToCheck.Any(x => SymbolEqualityComparer.Default.Equals(a.AttributeClass, x)))
                .Where(a => a.ConstructorArguments.Count() == 1)
                .ForEach(a => AnalyzeAttribute(a, Context, BannedWords))
             ; 
        }

        /// <summary>
        /// Analyzes a single attribute
        /// </summary>
        /// <param name="Attribute">The attribute to analyze.</param>
        /// <param name="Context">The analysis context</param>
        /// <param name="BannedWords">The banned phrases.</param>
        private void AnalyzeAttribute(AttributeData Attribute, SymbolAnalysisContext Context, ImmutableArray<BannedItem> BannedWords)
        {
            string StepText = Attribute
                    .ConstructorArguments
                    .First()
                    .Value
                    .ToString()
            ;

            var LocationofString = Attribute
                    .ApplicationSyntaxReference
                    .GetSyntax()
                    .DescendantNodes()
                    .OfType<AttributeArgumentSyntax>()
                    .FirstOrDefault()
                    ?.GetLocation()
           ;

            BannedWords
                .Select(word => new { Word = word, Matches = word.Matches(StepText) })
                .Where(x => x.Matches.Count() > 0)
                .SelectMany(matchesOfBannedWord => matchesOfBannedWord.Matches, (a, b) => new { a.Word, Match = b })
                .ForEach(instanceOfBannedWord => {
                    Context.ReportDiagnostic(Diagnostic.Create(Rule
                        , LocationofString
                        , instanceOfBannedWord.Word.SeverityAsEnum
                        , null
                        , null
                        , string.IsNullOrEmpty(instanceOfBannedWord.Word.Message) ? $"Banned phrase: {instanceOfBannedWord.Word.Phrase}" : instanceOfBannedWord.Word.Message)
                    );
                })
            ;
        }

        /// <summary>
        /// Reads all banned phrases from relevant files.
        /// </summary>
        /// <param name="Context">The context to report any parsing/validation errors.</param>
        /// <param name="AdditionalFiles">The set of <see cref="AdditionalText"/> items to check for any relevant files.</param>
        /// <returns>A set of <see cref="BannedItem"/> taken from all relevant files in the set of <paramref name="AdditionalFiles"/></returns>
        private ImmutableArray<BannedItem> ReadBannedWords(AnalysisContext Context, ImmutableArray<AdditionalText> AdditionalFiles)
        {
            return AdditionalFiles
                .Where(f => Path.GetFileName(f.Path) == BannedWordsFileName)
                .Select(f => new { path = f.Path, srcText = f.GetText() })
                .Where(x => x.srcText != null)
                .Select(x => new {
                    x.path,
                    x.srcText,
                    encoding = x.srcText.Encoding ?? Encoding.UTF8, //When encoding is missing, we should assume UTF-8
                    stream = new MemoryStream()
                })
               //Ensure we have a stream containing the content of the SourceText with the expected encoding.
               //We cannot use .ToString()  as it will return a set of bytes in a potentially different encoding
               //or byte order mark than specified in the content of the file
               .Select(x => {
                   using (var Writer = new StreamWriter(x.stream, x.encoding, 1, true))
                   {
                       x.srcText.Write(Writer);
                   }
                   x.stream.Seek(0, SeekOrigin.Begin);
                   return x;
               })
                .Select(x => new { x.path, list = ParseAndReportSyntaxErrors(Context,x.path,x.stream, x.encoding) })
                .Where(x => x.list != null)
                .Select(x => new { x.path, list = RemoveAndReportInvalidRegExes(Context,x.path,x.list)})
                .SelectMany(x => x.list.Items)
                .ToImmutableArray()
            ;
        }

        /// <summary>
        /// Deserializes a <see cref="BannedItems"/> and reports invalid content as diagnostics. 
        /// </summary>
        /// <param name="Context">The context to report any parsing/validation errors</param>
        /// <param name="Path">The path of the file where stream came from</param>
        /// <param name="xmlDataStream">The stream containing the content</param>
        /// <param name="encoding">The encoding of the characters in the stream</param>
        /// <returns></returns>
        private BannedItems ParseAndReportSyntaxErrors(AnalysisContext Context, string Path, Stream xmlDataStream, Encoding encoding)
        {
            try
            {
                var MyXmlReaderSettings = new XmlReaderSettings
                {
                    Schemas = Schemas,
                    ValidationType = ValidationType.Schema,
                    ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings
                };
                
                /// Based on https://stackoverflow.com/questions/4034207/ignoring-specified-encoding-when-deserializing-xml
                /// If the XmlReader is initialized with a TextReader, XML-header encoding is ignored.
                /// If a StringReader is used, the XmlReader fails if a unicode byte-order mark exists.
                /// If a StreamReader is used, a unicode byte-order mark overrides the StreamReader encoding.
                /// XmlReaderSettings.IgnoreProcessingInstructions = true doesn't make a difference when using a TextReader.
                ///In conclusion, the most robust solution seems to be using a StreamReader, since it uses the byte-order mark, if present.
                using (var reader = new StreamReader(xmlDataStream, encoding))
                {
                    using (var xmlReader = XmlReader.Create(reader, MyXmlReaderSettings))
                    {
                        return (new XmlSerializer(typeof(BannedItems)).Deserialize(xmlReader) as BannedItems);
                    }
                }
            }
            catch (Exception e)
            {
                Context.RegisterCompilationAction(endctxt =>
                   endctxt.ReportDiagnostic(Diagnostic.Create(InvalidFileRule
                       , null
                       , DiagnosticSeverity.Warning
                       , null
                       , null
                       , $"{Path} {e}"
                       )
                   )
                );
                return null;
            }
        }

        /// <summary>
        /// Check that each <see cref="BannedItem.Phrase"/> is a valid Regex
        /// </summary>
        /// <param name="Context">The context to report any parsing/validation errors</param>
        /// <param name="Path">The path of the file where stream came from</param>
        /// <param name="Banlist">The items to check</param>
        /// <returns>The BannedItems object where invalid regexes have been removed from <see cref="BannedItems.Items"/></returns>
        private BannedItems RemoveAndReportInvalidRegExes(AnalysisContext Context, string Path, BannedItems Banlist)
        {
            var invalidItems = Banlist.Items.Where(item => !IsValidRegEx(item.Phrase));

            invalidItems.ForEach(item => Context.RegisterCompilationAction(endctxt =>
                   endctxt.ReportDiagnostic(Diagnostic.Create(InvalidFileRule
                       , null
                       , DiagnosticSeverity.Warning
                       , null
                       , null
                       , $"{Path} Invalid Regex: {item.Phrase}"
                       )
                   )
                )
            );

            Banlist.Items = Banlist.Items.Except(invalidItems).ToList();
            return Banlist;


            bool IsValidRegEx(string pattern)
            {
                try { new Regex(pattern); return true; } 
                catch { return false; }

            }
        }
       
    }


    [XmlRoot("banlist", Namespace = MyNamespace)]
    public class BannedItems
    {
        public const string MyNamespace = "dec2ed27-578d-445b-8bfa-43f4fbe150b6";

        [XmlArray("items", Namespace = MyNamespace)]
        [XmlArrayItem("item", Namespace = MyNamespace)]
        public List<BannedItem> Items { get; set; }
    }

    public class BannedItem
    {
        [XmlAttribute(AttributeName = "phrase")]
        public string Phrase { get; set; }

        [XmlAttribute(AttributeName = "severity")]
        public string Severity { get; set;}

        [XmlAttribute(AttributeName = "message")]
        public string Message { get; set; }
        
        [XmlAttribute(AttributeName = "ignoreCase")]
        public bool IgnoreCase { get; set; }

        public DiagnosticSeverity SeverityAsEnum => (DiagnosticSeverity)Enum.Parse(typeof(DiagnosticSeverity), Severity);

        public IEnumerable<BannedItemMatch> Matches(string stepText)
        {
            RegexOptions Opts =  IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            return Regex
                .Matches(stepText, Phrase, Opts)
                .OfType<Match>()
                .Select(match => new BannedItemMatch { Index = match.Index, Length = match.Length, Value = match.Value })
            ;
        }
    }

    public class BannedItemMatch
    {
        /// <summary>
        /// <inheritdoc cref="Capture.Index"/>
        /// </summary>
        public int Index { get; set; }
       
        /// <summary>
        /// <inheritdoc cref="Capture.Length"/>
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// <inheritdoc cref="Capture.Value"/>
        /// </summary>
        public string Value { get; set; }
    }
}
