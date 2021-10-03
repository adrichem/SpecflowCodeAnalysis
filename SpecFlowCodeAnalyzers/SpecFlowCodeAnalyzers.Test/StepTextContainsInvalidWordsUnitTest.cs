namespace SpecFlowCodeAnalyzers.Test
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using SpecFlowCodeAnalyzers.Test.Common;
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.IO;
    using System.Text;

    [TestClass]
    public class StepTextContainsInvalidWordsUnitTest
    {
        private readonly Encoding DefaultEncoding = Encoding.UTF8;
        private readonly Func<DiagnosticSeverity,int, int, int, int, DiagnosticResult> BannedPhraseDiagnostic = (sev, x1, y1, x2, y2) =>
            new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.RegExContainsBannedWord
                , sev
                )
                .WithSpan(x1, y1, x2, y2)
        ;


        private readonly Func<DiagnosticResult> InvalidFileDiagnostic = () =>
             new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.RegExContainsBannedWordInvalidFileFormat
                 , DiagnosticSeverity.Warning
                 )
        ;


        /// <summary>
        /// analyzer must detecteven when argument is not string literal.
        /// </summary>
        [TestMethod]
        public async Task ArgumentIsNotAStringLiteral()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    private const string foo = ""Hello"";
                    [Given(foo)]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 9, 28, 9, 31))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with 1 stepdefinition(s) and multiple match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsOneStepMultipleMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""bad Hello World Hello more text"")]
                        void TestMethod1() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 67 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 67 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 8, 32, 8, 67 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with 1 stepdefinition(s) and 1 match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsOneStepOneMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""some text Hello more text"")]
                        void TestMethod1() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 61 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with 1 stepdefinition(s) and 0 match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsOneStepZeroMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""this is fine"")]
                        void TestMethod1() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with multiple stepdefinition(s) and multiple match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsMultipleStepsMultipleMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""bad Hello World"")]
                        void TestMethod1() {  }
        
                        [Given(""World bad"")]
                        void TestMethod2() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 51 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 8, 32, 8, 51 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 11, 32, 11, 45 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with multiple stepdefinition(s) and multiple match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsMultipleStepsOneMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""bad Hello"")]
                        void TestMethod1() {  }
        
                        [Given(""this is fine, no diagnostic here"")]
                        void TestMethod2() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 45 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with multiple stepdefinition(s) and 0 match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsMultipleStepsZeroMatches()
        {
            Stream content =new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""good"")]
                        void TestMethod1() {  }
        
                        [Given(""fine"")]
                        void TestMethod2() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// Verifies analyzer detecting with 0 stepdefinition(s) and 0 match(es)
        /// </summary>
        [TestMethod]
        public async Task BindingsZeroStepZeroMatches()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                using System.Diagnostics;

                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    { 
                        [Conditional(""Hello"")]
                        void TestMethod1() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// By default matching must be case sensitive.
        /// </summary>
        [TestMethod]
        public async Task CaseSensitityRuleDefault()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error"},
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""Hello"")]
                        void TestMethod1() {  }

                        [Given(""hello"")]
                        void TestMethod2() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 41 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Each item can hava a different case-sensitivity.
        /// </summary>
        [TestMethod]
        public async Task CaseSensitityRuleSpecified()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error", IgnoreCase = true },
                        new BannedItem { Phrase = "World", Severity = "Warning", IgnoreCase = false },
                        new BannedItem { Phrase = "Foo", Severity = "Warning",  }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""Hello"")]
                        void TestMethod1() {  }

                        [Given(""hello"")]
                        void TestMethod2() {  }
        
                        [Given(""World"")]
                        void TestMethod3() {  }

                        [Given(""world"")]
                        void TestMethod4() {  }

                        [Given(""Foo"")]
                        void TestMethod5() {  }

                        [Given(""foo"")]
                        void TestMethod6() {  }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 41 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 11, 32, 11, 41 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 14, 32, 14, 41 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 20, 32, 20, 39 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Analyzer must be able to parse configuration encoded in ASCII.
        /// </summary>
        [TestMethod]
        public async Task FileEncodingAscii()
        {
            await FileEncodingTest(Encoding.ASCII);
        }

        /// <summary>
        /// Analyzer must be able to parse configuration encoded in Latin1.
        /// </summary>
        [TestMethod]
        public async Task FileEncodingLatin1()
        {
            await FileEncodingTest(Encoding.Latin1);
        }

        /// <summary>
        /// Analyzer must be able to parse configuration encoded in UTF8.
        /// </summary>
        [TestMethod]
        public async Task FileEncodingUTF8()
        {
            await FileEncodingTest(new UTF8Encoding(true));
            await FileEncodingTest(new UTF8Encoding(false));
        }

        /// <summary>
        /// Analyzer must be able to parse configuration encoded in Unicode.
        /// </summary>
        [TestMethod]
        public async Task FileEncodingUnicode()
        {
            await FileEncodingTest(new UnicodeEncoding(false, false));
            await FileEncodingTest(new UnicodeEncoding(false, true));
            await FileEncodingTest(new UnicodeEncoding(true, false));
            await FileEncodingTest(new UnicodeEncoding(true, true));
        }

        /// <summary>
        /// Analyzer must raise diagnostic when regex is invalid, but continue processing valid ones from same file
        /// </summary>
        [TestMethod]
        public async Task FileInvalidRegExInFile()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "?", Severity = "Warning" },
                        new BannedItem { Phrase = "+", Severity = "Warning" },
                        new BannedItem { Phrase = "hello", Severity = "Warning" },
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""hello"")]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithExpectedDiagnostic(InvalidFileDiagnostic().WithMessage("BannedStepTextPatterns.xml Invalid Regex: ?"))
                .WithExpectedDiagnostic(InvalidFileDiagnostic().WithMessage("BannedStepTextPatterns.xml Invalid Regex: +"))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Warning, 8, 28, 8, 37 - 2))
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// Analyzer must raise diagnostic when Severity is invalid. Its allowed to reject entire file.
        /// </summary>
        [TestMethod]
        public async Task FileInvalidSeverity()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Invalid" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }.ToXmlStream(DefaultEncoding)
            ;

            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""hello"")]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                .WithExpectedDiagnostic(InvalidFileDiagnostic())
                .WithCode(CodeTemplate)
                .RunAsync()
            ;

        }

        /// <summary>
        /// Analyzer must raise diagnostic when file is malformed XML.
        /// </summary>
        [TestMethod]
        public async Task FileMalformedEmpty()
        {
            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""hello"")]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, "")
                .WithExpectedDiagnostic(InvalidFileDiagnostic())
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// Analyzer must support multiple files in the solution and apply all their rules.
        /// </summary>
        [TestMethod]
        public async Task FileMultiplePresent()
        {
            Stream content = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error" },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(DefaultEncoding)
            ;


            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""Hello"")]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding, "c:\\first\\")
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding, "c:\\second\\")
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 28, 8, 37 - 2))
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 28, 8, 37 - 2))
                .RunAsync()
            ;
        }

        /// <summary>
        /// Analyzer may not raise diagnostics when no files are present.
        /// </summary>
        [TestMethod]
        public async Task FileNotPresent()
        {
            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""hello"")]
                    void TestMethod1() {  }
                }
            }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        /// <summary>
        /// Analyzer must raise diagnostic when file is valid XML, but not from our namespace.
        /// </summary>
        [TestMethod]
        public async Task FileValidNoRelevantContentForAnalyzerXml()
        {
            string CodeTemplate = @"
            using TechTalk.SpecFlow;
            namespace a
            {
                [Binding]
                public class MyTestCode
                {   
                    [Given(""hello"")]
                    void TestMethod1() {  }
                }
            }";

            string RandomXmlFile = @"<table><items><item phrase=""hello""></item></items></table>";


            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, RandomXmlFile)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(InvalidFileDiagnostic())
                .RunAsync()
            ;
        }

        /// <summary>
        ///Analyzer must be able to return a configurable message in its diagnostics:
        /// </summary>
        [TestMethod]
        public async Task MessageAttributeMustBeSupported()
        {

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""Hello"")]
                        void TestMethod1() {  }
                    }
                }";

            var messages = new List<string>()
            {
                "plain text",
                "😀",
                "😀a",
                "a😀",
                "😀😀",
             };

            foreach(var message in messages)
            {
                Stream content = new BannedItems()
                    {
                        Items = new List<BannedItem>()
                        {
                            new BannedItem { Phrase = "Hello", Severity = "Error", Message = message },
                        }
                    }
                    .ToXmlStream(DefaultEncoding)
                ;

                await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                    .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, content, DefaultEncoding)
                    .WithCode(CodeTemplate)
                    .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 41 - 2).WithMessage(message))
                    .RunAsync()
                ;

            }
        }

        private async Task FileEncodingTest(Encoding enc)
        {

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace a
                {
                    [Binding]
                    public class MyTestCode
                    {   
                        [Given(""some text Hello more text"")]
                        void TestMethod1() {  }
                    }
                }";

            Stream stream = new BannedItems()
                {
                    Items = new List<BannedItem>()
                    {
                        new BannedItem { Phrase = "Hello", Severity = "Error", },
                        new BannedItem { Phrase = "World", Severity = "Warning" }
                    }
                }
                .ToXmlStream(enc)
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepTextContainsInvalidWords>()
                .WithAdditionalFile(StepTextContainsInvalidWords.BannedWordsFileName, stream, enc)
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(BannedPhraseDiagnostic(DiagnosticSeverity.Error, 8, 32, 8, 61 - 2))
                .RunAsync()
            ;
        }

    }


    internal static class MyExt
    {
        public static Stream ToXmlStream<T>(this T toSerialize, Encoding enc)
        {
            var stream = new MemoryStream();
            using var Writer = new StreamWriter(stream, enc, 1024, true);
            new XmlSerializer(toSerialize.GetType()).Serialize(Writer, toSerialize);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static string ToXmlString<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new(toSerialize.GetType());

            using StringWriter textWriter = new();
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }
  
}
