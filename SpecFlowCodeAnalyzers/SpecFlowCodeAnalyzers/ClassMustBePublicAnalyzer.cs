namespace Adrichem.Test.SpecFlowCodeAnalyzers
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System.Collections.Immutable;
    using System.Linq;

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

            var ClassKeywords = ClassKeywordsOf(c.Symbol as INamedTypeSymbol);
            if (ClassKeywords.IsEmpty)
                return;

            if (!HasStepDefinitionMethods(c.Symbol as INamedTypeSymbol, c.Compilation))
                return;

            ClassKeywords
                .ForEach(classKeyword => c.ReportDiagnostic(Diagnostic.Create(Rule
                       , classKeyword.GetLocation()
                       , $"Classes with step definitions must be public."))
                )
            ;

        }

        /// <summary>
        /// Indicates if the named type contains SpecFlow step definition methods.
        /// </summary>
        /// <param name="nt">The named type to check</param>
        /// <param name="c">The compilation</param>
        /// <returns> <see langword="true"/> if the <see cref="INamedTypeSymbol"/> contains methods with specflow attributes; 
        /// otherwise, <see langword="false"/>.</returns>
        private bool HasStepDefinitionMethods(INamedTypeSymbol nt, Compilation c) => nt
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => method.SpecFlowAttributes(c).Any())
            .Any()
        ;

        /// <summary>
        /// The class keywords of the named type
        /// </summary>
        /// <param name="nt">The named type to check</param>
        /// <returns>A sequence of <see cref="SyntaxToken"/>s corresponding to each of the
        /// type's <see langword="class"/> keywords in the source code.
        /// A class can be <see langword="partial"/> and have multiple <see langword="class"/> keywords</returns>
        private ImmutableArray<SyntaxToken> ClassKeywordsOf(INamedTypeSymbol nt)
        {
            return nt
                .DeclaringSyntaxReferences
                .Select(sref => sref
                    .GetSyntax()
                    .ChildTokens()
                    .Where(t => t.IsKind(SyntaxKind.ClassKeyword))
                )
                .SelectMany(x => x)
                .ToImmutableArray()
            ;
        }
    }
}
