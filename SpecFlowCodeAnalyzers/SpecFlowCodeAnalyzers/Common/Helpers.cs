namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;
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


        public static bool MethodHasSpecFlowAtributes(IMethodSymbol M, Compilation C)
        {
            var AttributeTypesToCheck = GetStepDefinitionTypeSymbols(C);
            bool result = M
                .GetAttributes()
                .Any(a => AttributeTypesToCheck.Any(x => SymbolEqualityComparer.Default.Equals(a.AttributeClass, x)))
            ;
            return result;
        }
    }
}
