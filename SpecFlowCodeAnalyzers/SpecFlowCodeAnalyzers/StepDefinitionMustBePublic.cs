namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StepDefinitionMustBePublic : DiagnosticAnalyzer
    {
        private static readonly string Title = "Must be public";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition must be public.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicMethod
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

        private static void AnalyzeMethod(SymbolAnalysisContext c)
        {
            IMethodSymbol m = c.Symbol as IMethodSymbol;
            if (m.DeclaredAccessibility != Accessibility.Public &&
                Helpers.MethodHasSpecFlowAtributes(m, c.Compilation))
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule
                    , m.Locations.First()
                    , $"Must be public: {m.Name}")
                );
            }
        }
    }
}
