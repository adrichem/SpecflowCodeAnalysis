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
    public class ParameterMayNotBeOutAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Title = "Parameter may not have out keyword";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition may not have out parameters.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            SpecFlowCodeAnalyzersDiagnosticIds.NoOutParameters
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
            context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        }

        private void AnalyzeMethod(SymbolAnalysisContext context)
        {
            IMethodSymbol m = context.Symbol as IMethodSymbol;
            if (Helpers.MethodHasSpecFlowAtributes(m,context.Compilation))
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
                .Where(token => token.IsKind(SyntaxKind.OutKeyword))
                .ForEach(o => c.ReportDiagnostic(Diagnostic.Create(Rule
                    , o.GetLocation()
                    , $"out keyword not allowed on step definition method"))
                )
            ;
        }
    }
}
