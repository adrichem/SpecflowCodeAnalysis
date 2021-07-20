namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using Microsoft.CodeAnalysis;
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
        /// Indicates which SpecFlow attributes exist on the method.
        /// </summary>
        /// <param name="m">The method to check</param>
        /// <param name="c">The compilation.</param>
        /// <returns>A sequence of <see cref="AttributeData"/> found on the <see cref="IMethodSymbol"/></returns>
        public static IEnumerable<AttributeData> SpecFlowAttributes(this IMethodSymbol m, Compilation c)
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
