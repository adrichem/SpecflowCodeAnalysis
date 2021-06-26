namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using Microsoft.CodeAnalysis;
    using System.Collections.Generic;

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
}
