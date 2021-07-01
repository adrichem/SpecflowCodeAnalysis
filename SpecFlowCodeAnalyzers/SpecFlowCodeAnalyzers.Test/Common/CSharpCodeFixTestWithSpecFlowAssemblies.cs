namespace SpecFlowCodeAnalyzers.Test.Common
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;

    public class CSharpCodeFixTestWithSpecFlowAssemblies<TAnalyzer, TCodeFix> 
        : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        /// <summary>
        /// A <see cref="CSharpAnalyzerTest"/> with references to the SpecFlow assembly
        /// </summary>
        public CSharpCodeFixTestWithSpecFlowAssemblies()
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

        public CSharpCodeFixTestWithSpecFlowAssemblies<TAnalyzer, TCodeFix> WithCode(string x)
        {
            TestCode = x;
            return this;
        }

        public CSharpCodeFixTestWithSpecFlowAssemblies<TAnalyzer, TCodeFix> WithFixCode(string x)
        {
            FixedCode = x;
            return this;
        }

        public CSharpCodeFixTestWithSpecFlowAssemblies<TAnalyzer, TCodeFix> WithExpectedDiagnostic(DiagnosticResult x)
        {
            ExpectedDiagnostics.Add(x);
            return this;
        }
        
    }
}
