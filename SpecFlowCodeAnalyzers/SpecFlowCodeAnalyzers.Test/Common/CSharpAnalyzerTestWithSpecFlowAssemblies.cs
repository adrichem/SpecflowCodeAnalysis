namespace SpecFlowCodeAnalyzers.Test.Common
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Microsoft.CodeAnalysis.Text;
    using System.IO;
    using System.Text;

    public class CSharpAnalyzerTestWithSpecFlowAssemblies<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <summary>
        /// A <see cref="CSharpAnalyzerTest"/> with references to the SpecFlow assembly
        /// </summary>
        public CSharpAnalyzerTestWithSpecFlowAssemblies()
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

        public CSharpAnalyzerTestWithSpecFlowAssemblies<TAnalyzer> WithAdditionalFile(string Name, string Content, string Path = null)
        {
            SolutionTransforms.Add((solution, projectId) => solution.AddAdditionalDocument(
                DocumentId.CreateNewId(projectId)
                , Name
                , Content
                , (Path ?? System.IO.Path.GetTempPath()).Split(System.IO.Path.DirectorySeparatorChar))
            );
            return this;
        }

        public CSharpAnalyzerTestWithSpecFlowAssemblies<TAnalyzer> WithAdditionalFile(string Name, Stream Content, Encoding Enc, string Path = null)
        {
            SolutionTransforms.Add((solution, projectId) => solution.AddAdditionalDocument(
                DocumentId.CreateNewId(projectId)
                , Name
                , SourceText.From(Content, Enc)
                , (Path ?? System.IO.Path.GetTempPath()).Split(System.IO.Path.DirectorySeparatorChar))
            );
            return this;
        }


        public CSharpAnalyzerTestWithSpecFlowAssemblies<TAnalyzer> WithCode(string x)
        {
            TestCode = x;
            return this;
        }

        public CSharpAnalyzerTestWithSpecFlowAssemblies<TAnalyzer> WithExpectedDiagnostic(DiagnosticResult x)
        {
            ExpectedDiagnostics.Add(x);
            return this;
        }
        
    }
}
