﻿namespace SpecFlowCodeAnalyzers.Test
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Microsoft.CodeAnalysis;
    using System.Threading;

    public class Test : CSharpAnalyzerTest<StepTextMustBeValidRegEx, MSTestVerifier>
    {
        public Test()
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
                    .WithProjectMetadataReferences(projectId,tmp.MetadataReferences)
                ;
                
                return solution;
            });
        }
    }


    [TestClass]
    public class StepTextMustBeValidRegExUnitTest
    {
        private readonly string CodeTemplate = @"
            using TechTalk.SpecFlow;
    
            namespace ConsoleApplication1
            {
                class Foo
                {   [ATTR]
                    public void Test(int a) {}
                }
            }"
        ;

        class RegexTestSituation
        {
            public string RegexPattern { get; set; }
            public string ExpectedError { get; set; }
        }

        [TestMethod]
        public async Task ValidRegexesMayNotCauseDiagnostics()
        {
            var ValidRegexes = new List<string>
            {
                "]",
                "{",
                "}",
                "{}",
                ".*",
                "(.*)"
            };
            foreach (var RegexPattern in ValidRegexes)
            {
                foreach (var Attribute in new List<string> { "Given", "When", "Then" })
                {
                    string tmp = CodeTemplate.Replace("[ATTR]", $"[{Attribute}(@\"{RegexPattern}\")]");
                    var t = new Test()
                    {
                        TestCode = tmp,
                    };
                    await t.RunAsync(CancellationToken.None);
                }
            }
        }
        
        [TestMethod]
        public async Task InvalidRegExes()
        {
            var InvalidRegExes = new List<RegexTestSituation> {
                new RegexTestSituation {
                    RegexPattern = "\\",
                    ExpectedError = "ATTR => Invalid pattern '\\' at offset 1. Illegal \\\\ at end of pattern."
                },
                new RegexTestSituation {
                    RegexPattern = "[",
                    ExpectedError = "ATTR => Invalid pattern '[' at offset 1. Unterminated [] set."
                },
                new RegexTestSituation {
                    RegexPattern = "*",
                    ExpectedError = "ATTR => Invalid pattern '*' at offset 1. Quantifier {x,y} following nothing."
                },
                new RegexTestSituation
                {
                    RegexPattern = "?",
                    ExpectedError = "ATTR => Invalid pattern '?' at offset 1. Quantifier {x,y} following nothing."
                },
                new RegexTestSituation
                {
                    RegexPattern = "+",
                    ExpectedError = "ATTR => Invalid pattern '+' at offset 1. Quantifier {x,y} following nothing."
                },
                new RegexTestSituation
                {
                    RegexPattern = "(",
                    ExpectedError = "ATTR => Invalid pattern '(' at offset 1. Not enough )'s."
                },
                new RegexTestSituation
                {
                    RegexPattern = ")",
                    ExpectedError = "ATTR => Invalid pattern ')' at offset 1. Too many )'s."
                },
            };


            foreach (var Situation in InvalidRegExes)
            {
                foreach (var Attribute in new List<string> { "Given", "When", "Then" })
                {
                    string tmp = CodeTemplate.Replace("[ATTR]", $"[{Attribute}(@\"{Situation.RegexPattern}\")]");
                    var t = new Test()
                    {
                        TestCode = tmp,
                    };

                    t.ExpectedDiagnostics.Add(new DiagnosticResult(StepTextMustBeValidRegEx.DiagnosticId, DiagnosticSeverity.Error)
                      .WithSpan(7, 21, 8, 47)
                      .WithArguments(Situation.ExpectedError.Replace("ATTR", $"{Attribute}Attribute")))
                    ;
                    await t.RunAsync(CancellationToken.None);
                }
                 
            }
            
        }
    }
}
