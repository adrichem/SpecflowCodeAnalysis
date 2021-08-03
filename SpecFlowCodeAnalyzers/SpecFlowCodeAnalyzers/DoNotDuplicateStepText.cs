namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotDuplicateStepTextAnalyzer : DiagnosticAnalyzer
    {
        private static readonly string Title = "Replace with [StepDefinition(....)]";
        private static readonly string MessageFormat = "{0}";
        private static readonly string Description = "Instead of multiple attributes with the same regex, use a single [StepDefinition(...)] attribute.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(SpecFlowCodeAnalyzersDiagnosticIds.DoNotDuplicateStepText
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

        private void AnalyzeMethod(SymbolAnalysisContext c)
        {
            (c.Symbol as IMethodSymbol)
                .SpecFlowStepDefinitionAttributes(c.Compilation)
                .Select( attr => new { 
                    attr, 
                    Keyword = attr.AttributeClass.Name, 
                    Text = attr.ConstructorArguments.Select(arg => arg.Value).FirstOrDefault() 
                })
                .GroupBy(attr => attr.Text)
                .Where(grp => grp.Count() > 1)
                .SelectMany(grp=>grp)
                .Where(attr => attr.Keyword != "StepDefinitionAttribute")
                .ForEach(attr =>RaiseDiagnostic(attr.attr))
            ;

            void RaiseDiagnostic(AttributeData a)
            {
                c.ReportDiagnostic(Diagnostic.Create(Rule
                    ,a.ApplicationSyntaxReference.GetSyntax().GetLocation()
                    , string.Empty))
                ;
            }
        }
    }
}
