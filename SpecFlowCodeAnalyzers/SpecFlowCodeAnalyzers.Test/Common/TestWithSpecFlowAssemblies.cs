namespace SpecFlowCodeAnalyzers.Test.Common
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;

    public class TestWithSpecFlowAssemblies<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <summary>
        /// A <seealso cref="CSharpAnalyzerTest"/> with references to the SpecFlow assembly
        /// </summary>
        public TestWithSpecFlowAssemblies()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                var specFlowAssembly = MetadataReference.CreateFromFile(typeof(TechTalk.SpecFlow.AfterAttribute).Assembly.Location);

                var tmp = solution
                    .GetProject(projectId)
                    .AddMetadataReference(specFlowAssembly)
                 ;

                var compilationOptions = solution.GetProject(projectId).CompilationOptions;
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));

                solution = solution
                    .WithProjectCompilationOptions(projectId, compilationOptions)
                    .WithProjectMetadataReferences(projectId, tmp.MetadataReferences)
                ;

                return solution;
            });
        }

        public TestWithSpecFlowAssemblies<TAnalyzer> WithCode(string x)
        {
            TestCode = x;
            return this;
        }

        public TestWithSpecFlowAssemblies<TAnalyzer> WithExpectedDiagnostic(DiagnosticResult x)
        {
            ExpectedDiagnostics.Add(x);
            return this;
        }
        
    }
}
