namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassMustHaveBindingAttributeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Title = "Class must have [Binding] attribute.";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "A step definition's class must have the [Binding] attribute.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.BindingAttributeMissing
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
             * - Is a class.
             * - Contains SpecFlow step definition methods.
             * - Does not have the [Binding] attribute
             */
            var nt = c.Symbol as INamedTypeSymbol;
            var ClassKeywords = Helpers.ClassKeywordsOf(nt);
            if (ClassKeywords.IsEmpty)
                return;

            if (!Helpers.HasStepDefinitionMethods(nt, c.Compilation))
                return;
            
            var BindingAttr = c.Compilation.GetTypeByMetadataName("TechTalk.SpecFlow.BindingAttribute");

            if ( nt.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, BindingAttr)))
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
