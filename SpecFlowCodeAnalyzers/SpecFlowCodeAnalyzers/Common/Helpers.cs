namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Helpers
    {
        public static string DiagnosticCategory { get; } = "SpecFlow";
        public static IEnumerable<INamedTypeSymbol> GetStepDefinitionTypeSymbols(Compilation C)
        {
            return new List<INamedTypeSymbol>()
            {
                C.GetTypeByMetadataName("TechTalk.SpecFlow.GivenAttribute"),
                C.GetTypeByMetadataName("TechTalk.SpecFlow.WhenAttribute"),
                C.GetTypeByMetadataName("TechTalk.SpecFlow.ThenAttribute"),
                C.GetTypeByMetadataName("TechTalk.SpecFlow.StepDefinitionAttribute"),
            };
        }

         /// <summary>
        /// Indicates if the named type contains SpecFlow step definition methods.
        /// </summary>
        /// <param name="nt">The named type to check</param>
        /// <param name="c">The compilation</param>
        /// <returns> <see langword="true"/> if the <see cref="INamedTypeSymbol"/> contains methods with specflow attributes; 
        /// otherwise, <see langword="false"/>.</returns>
        public static bool HasStepDefinitionMethods(INamedTypeSymbol nt, Compilation c) => nt
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(method => method.SpecFlowStepDefinitionAttributes(c).Any())
            .Any()
        ;

        /// <summary>
        /// The class keywords of the named type
        /// </summary>
        /// <param name="nt">The named type to check</param>
        /// <returns>A sequence of <see cref="SyntaxToken"/>s corresponding to each of the
        /// type's <see langword="class"/> keywords in the source code.
        /// A class can be <see langword="partial"/> and have multiple <see langword="class"/> keywords</returns>
        public static ImmutableArray<SyntaxToken> ClassKeywordsOf(INamedTypeSymbol nt)
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

    public static class ExtensionMethods
    {
        /// <summary>
        /// Indicates if SpecFlow is present in the compilation.
        /// </summary>
        /// <param name="c">The compilation to check.</param>
        /// <returns><see langword="true"/> if the <see cref="Compilation"/> references SpecFlow; 
        /// otherwise, <see langword="false"/></returns>
        public static bool SpecFlowIsReferenced(this Compilation c)
        {
            return c.GetTypeByMetadataName("TechTalk.SpecFlow.GivenAttribute") != null;
        }

        /// <summary>
        /// Indicates which SpecFlow step definition attributes exist on the method.
        /// </summary>
        /// <param name="m">The method to check</param>
        /// <param name="c">The compilation.</param>
        /// <returns>A sequence of <see cref="AttributeData"/> found on the <see cref="IMethodSymbol"/></returns>
        public static IEnumerable<AttributeData> SpecFlowStepDefinitionAttributes(this IMethodSymbol m, Compilation c)
        {
            var AttributeTypesToCheck = Helpers.GetStepDefinitionTypeSymbols(c);
            return m
                .GetAttributes()
                .Where(a => AttributeTypesToCheck.Any(x => SymbolEqualityComparer.Default.Equals(a.AttributeClass, x)))
                .ToImmutableArray()
            ; 
        }
    }
}
