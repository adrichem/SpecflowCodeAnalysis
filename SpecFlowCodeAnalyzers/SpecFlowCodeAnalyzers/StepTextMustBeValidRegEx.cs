﻿namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StepTextMustBeValidRegEx : DiagnosticAnalyzer
    {
        public const string DiagnosticId = nameof(StepTextMustBeValidRegEx);
        private static readonly string Title = "Invalid Regex";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "Text provided to Given, When and Then attributes must be valid Regular expression.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId
            , Title
            , MessageFormat
            , Category
            , DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule); 

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private static void AnalyzeMethod(SymbolAnalysisContext Context)
        {
            IMethodSymbol MethodSymbol = Context.Symbol as IMethodSymbol;

            var AttributeTypesToCheck = new List<INamedTypeSymbol>()
            {
                Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.GivenAttribute"),
                Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.WhenAttribute"),
                Context.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.ThenAttribute"),
            };
            
            foreach(var Attribute in MethodSymbol.GetAttributes()
                .Where(a => AttributeTypesToCheck.Any( x => SymbolEqualityComparer.Default.Equals(a.AttributeClass, x)))
                .Where(a => a.ConstructorArguments.Any()))
            {
                string RegExPattern = Attribute.ConstructorArguments.First().Value.ToString();
                try
                {
                    new System.Text.RegularExpressions.Regex(RegExPattern);
                }
                catch (Exception e)
                {
                    var LocationofString = Attribute
                        .ApplicationSyntaxReference
                        .GetSyntax()
                        .DescendantNodes()
                        .OfType<AttributeArgumentSyntax>()
                        .FirstOrDefault()
                        ?.GetLocation()
                    ;
                    Context.ReportDiagnostic(Diagnostic.Create(Rule, LocationofString, e.Message));
                }
            }
        }
    }
}
