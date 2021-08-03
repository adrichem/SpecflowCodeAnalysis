namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassMustBePublicAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Title = "Parent class must be public";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition's class must be public.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicClass
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
                    x.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
                }
            });
        }

        private void AnalyzeNamedType(SymbolAnalysisContext c)
        {
            /*
             * Raise diagnostic on each of the named type's 'class' keywords when named type:
             * - Is not public.
             * - Is a class.
             * - contains SpecFlow step definition methods.
             */
            if (c.Symbol.DeclaredAccessibility == Accessibility.Public)
                return;

            var ClassKeywords = Helpers.ClassKeywordsOf(c.Symbol as INamedTypeSymbol);
            if (ClassKeywords.IsEmpty)
                return;

            if (!Helpers.HasStepDefinitionMethods(c.Symbol as INamedTypeSymbol, c.Compilation))
                return;

            ClassKeywords
                .ForEach(classKeyword => c.ReportDiagnostic(Diagnostic.Create(Rule
                       , classKeyword.GetLocation()
                       , $"Classes with step definitions must be public."))
                )
            ;

        }
    }
}
