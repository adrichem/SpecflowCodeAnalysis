namespace SpecFlowCodeAnalyzers.Test
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using System.Threading;
    using SpecFlowCodeAnalyzers.Test.Common;
    using System.Linq;

    [TestClass]
    public class StepTextMustBeValidRegExUnitTest
    {
        private readonly IEnumerable<string> StepDefinitionAttributes = new List<string> 
        { 
            "Given", 
            "When", 
            "Then",
            "StepDefinition"
        };

        private readonly string CodeTemplate = @"
            using TechTalk.SpecFlow;
    
            namespace ConsoleApplication1
            {
                [Binding]
                public class Foo
                {   
                    [ATTR]
                    public void Test(int a) {}
                }
            }"
        ;

        [TestMethod]
        public async Task ValidRegexesMayNotCauseDiagnostic()
        {
            var ValidRegexes = new List<string>
            {
                "]", "{","}","{}",".*","(.*)","there must be [1-9][0-9]* items?"
            };

            var Tests = ValidRegexes
                .SelectMany(vr => StepDefinitionAttributes, (RegexPattern, Attribute) => new { RegexPattern, Attribute })
            ;
            foreach(var x in Tests)
            {
                await new TestWithSpecFlowAssemblies<StepTextMustBeValidRegEx>()
                        .WithCode(CodeTemplate.Replace("[ATTR]", $"[{x.Attribute}(@\"{x.RegexPattern}\")]"))
                        .RunAsync(CancellationToken.None);
            }
        }

        [TestMethod]
        public async Task InvalidRegExesMustCauseDiagnostic()
        {
            IEnumerable<InvalidRegexTestSituation> InvalidRegExes = new List<InvalidRegexTestSituation> {
                new InvalidRegexTestSituation {
                    RegexPattern = "\\",
                    ExpectedError = "Invalid pattern '\\' at offset 1. Illegal \\\\ at end of pattern.",
                },
                new InvalidRegexTestSituation {
                    RegexPattern = "[",
                    ExpectedError = "Invalid pattern '[' at offset 1. Unterminated [] set."
                },
                new InvalidRegexTestSituation {
                    RegexPattern = "*",
                    ExpectedError = "Invalid pattern '*' at offset 1. Quantifier {x,y} following nothing."
                },
                new InvalidRegexTestSituation
                {
                    RegexPattern = "?",
                    ExpectedError = "Invalid pattern '?' at offset 1. Quantifier {x,y} following nothing."
                },
                new InvalidRegexTestSituation
                {
                    RegexPattern = "+",
                    ExpectedError = "Invalid pattern '+' at offset 1. Quantifier {x,y} following nothing."
                },
                new InvalidRegexTestSituation
                {
                    RegexPattern = "(",
                    ExpectedError = "Invalid pattern '(' at offset 1. Not enough )'s."
                },
                new InvalidRegexTestSituation
                {
                    RegexPattern = ")",
                    ExpectedError = "Invalid pattern ')' at offset 1. Too many )'s."
                },
            };
            
            //Combine each invalid regex with each type of stepdefinition to create a bunch of situations to test
            //Calculate the expected span for each sitiation
            //and create a Task we can await
            var Situations = InvalidRegExes
                .SelectMany(Situation => StepDefinitionAttributes, (S,A) => CombineSituationWithAttribute(S, A) )
                .Select(Situation => CalculateLineStartAndEnd(Situation))
                .Select(Situation => CalculateSpanStart(Situation))
                .Select(Situation => CalculateSpanEnd(Situation))
                .Select(Situation => CreateTestTask(Situation))
                .ToArray()
            ;

            foreach(var S in Situations)
            {
                await S;
            }

            InvalidRegexTestSituation CalculateLineStartAndEnd(InvalidRegexTestSituation Situation)
            {
                Situation.LineStart = 9;
                Situation.LineEnd = 9;
                return Situation;
            }

            InvalidRegexTestSituation CalculateSpanStart(InvalidRegexTestSituation Situation)
            {
                switch (Situation.Attribute)
                {
                    case "Given":
                        Situation.ColumnStart = 28;
                        break;
                    case "StepDefinition":
                        Situation.ColumnStart = 37;
                        break;
                    default:
                        Situation.ColumnStart = 27;
                        break;

                }
                return Situation;
            }

            InvalidRegexTestSituation CalculateSpanEnd(InvalidRegexTestSituation Situation)
            {
                //Dont forget to include
                // The @ char
                // First "
                // Last "
                Situation.ColumnEnd = Situation.ColumnStart + Situation.RegexPattern.Length + 1 + 1 + 1;
                return Situation;
            }

            InvalidRegexTestSituation CombineSituationWithAttribute(InvalidRegexTestSituation Situation, string Attribute)
            {
                Situation.Attribute = Attribute;
                return Situation;
            }

            Task CreateTestTask(InvalidRegexTestSituation Situation)
            {
                return new TestWithSpecFlowAssemblies<StepTextMustBeValidRegEx>()
                    .WithCode(CodeTemplate.Replace("[ATTR]", $"[{Situation.Attribute}(@\"{Situation.RegexPattern}\")]"))
                    .WithExpectedDiagnostic(new DiagnosticResult(StepTextMustBeValidRegEx.DiagnosticId, DiagnosticSeverity.Error)
                        .WithSpan(Situation.LineStart, Situation.ColumnStart, Situation.LineEnd, Situation.ColumnEnd)
                        .WithArguments(Situation.ExpectedError)
                    )
                    .RunAsync()
                ;
            }
        }

        /// <summary>
        /// Represents a test sitation for an invalid regex used in a SpecFlow attribute.
        /// </summary>
        class InvalidRegexTestSituation
        {
            /// <summary>
            /// The attribute (Given/When/Then) to test with.
            /// </summary>
            public string Attribute { get; set; }

            /// <summary>
            /// The regex pattern to test with.
            /// </summary>
            public string RegexPattern { get; set; }

            /// <summary>
            /// The expected message from analyzer.
            /// </summary>
            public string ExpectedError { get; set; }

            /// <summary>
            /// The expected start line of the analyzers span.
            /// </summary>
            public int LineStart { get; set; }

            /// <summary>
            /// The expected end line of the analyzers span.
            /// </summary>
            public int LineEnd { get; set; }

            /// <summary>
            /// The expected start column of the analyzers span.
            /// </summary>
            public int ColumnStart { get; set; }

            /// <summary>
            /// The expected end column of the analyzers span.
            /// </summary>
            public int ColumnEnd { get; set; }
        }

    }
}
