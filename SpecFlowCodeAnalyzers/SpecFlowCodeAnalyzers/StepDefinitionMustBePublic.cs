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
        public const string DiagnosticId = nameof(StepDefinitionMustBePublic);
        private static readonly string Title = "Must be public";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition method must be public.";
        private const string Category = "SpecFlow";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId
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

        private static void AnalyzeMethod(SymbolAnalysisContext Context)
        {
            IMethodSymbol MethodSymbol = Context.Symbol as IMethodSymbol;
            if (MethodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                var AttributeTypesToCheck = Helpers.GetStepDefinitionTypeSymbols(Context.Compilation);
                bool MethodHasSpecFlowAtributes = MethodSymbol
                    .GetAttributes()
                    .Where(a => AttributeTypesToCheck.Any(x => SymbolEqualityComparer.Default.Equals(a.AttributeClass, x)))
                    .Any()
                ;

                if (MethodHasSpecFlowAtributes)
                {
                    Context.ReportDiagnostic(Diagnostic.Create(Rule
                        , MethodSymbol.Locations.First()
                        , $"{MethodSymbol.ReceiverType}.{MethodSymbol.Name}")
                    );
                }
            }
        }
    }
}
