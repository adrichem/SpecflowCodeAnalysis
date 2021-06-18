namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StepTextMustBeValidRegEx : DiagnosticAnalyzer
    {
        public const string DiagnosticId = nameof(StepTextMustBeValidRegEx);
        private static readonly string Title = "Invalid Regex";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "The text of a binding step must be a valid Regular expression.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId
            , Title
            , MessageFormat
            , Category
            , DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        { 
            get 
            { 
                return ImmutableArray.Create(Rule); 
            } 
        }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext Context)
        {
            IMethodSymbol MethodSymbol = Context.Symbol as IMethodSymbol;

            var Given = Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.GivenAttribute");
            var When = Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.WhenAttribute");
            var Then = Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.ThenAttribute");
            
            var Attributes = MethodSymbol.GetAttributes();

            foreach(var Attribute in Attributes
                .Where(a =>
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, Given) |
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, When) |
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, Then)
                 ))
            {
                if(!Attribute.ConstructorArguments.Any())
                {
                    continue;
                }
                string RegExPattern = Attribute.ConstructorArguments.First().Value.ToString();

                try
                {
                    new System.Text.RegularExpressions.Regex(RegExPattern);
                }
                catch (Exception e)
                {
                    var LocationofMethod = MethodSymbol
                        .DeclaringSyntaxReferences
                        .FirstOrDefault()
                        .GetSyntax()
                        .GetLocation()
                    ;
                    Context.ReportDiagnostic(Diagnostic.Create(Rule, LocationofMethod, $"{Attribute.AttributeClass.Name} => {e.Message}"));
                }
            }
        }
    }
}
