namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForbiddenModifiersAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Title = "Parameter may not have out or ref modifier";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition may not have out or ref parameters.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            SpecFlowCodeAnalyzersDiagnosticIds.ForbiddenModifier
            , Title
            , MessageFormat
            , Helpers.DiagnosticCategory
            , DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: Description
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule); 

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(x =>
            {
                if (x.Compilation.SpecFlowIsReferenced())
                {
                    x.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
                }
            });
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            IMethodSymbol m = context.Symbol as IMethodSymbol;
            if (m.SpecFlowStepDefinitionAttributes(context.Compilation).Any())
            {
                m.DeclaringSyntaxReferences
                    .First()
                    .GetSyntax()
                    .DescendantNodes()
                    .OfType<ParameterSyntax>()
                    .ForEach(p => AnalyzeParameter(p, context))
                ;
            }
        }

        private void AnalyzeParameter(ParameterSyntax p, SymbolAnalysisContext c)
        {
             p.ChildTokens()
                .Where(token => token.IsKind(SyntaxKind.OutKeyword) || token.IsKind(SyntaxKind.RefKeyword))
                .ForEach(o => c.ReportDiagnostic(Diagnostic.Create(Rule
                    , o.GetLocation()
                    , Title))
                )
            ;
        }
    }
}
