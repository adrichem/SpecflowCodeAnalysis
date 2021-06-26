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
    using System;
    using System.Linq;

    [TestClass]
    public class StepDefinitionMustBePublicUnitTest
    {
        private readonly IEnumerable<string> AccessModifiers = new List<string>
        {
            "private",
            "protected",
            "internal",
            "protected internal",
            "private protected"
        };

        private readonly Func<int,int,int,int, DiagnosticResult> ExpectedDiagnostic = (x1,y1,x2,y2) => 
            new DiagnosticResult(StepDefinitionMustBePublic.DiagnosticId, DiagnosticSeverity.Warning)
                .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public void CombiningAttrbutesInSingleAttributeList()
        {
            string CodeTemplate = @"
                using TechTalk.SpecFlow;
    
                namespace ConsoleApplication1
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given, When, Then(""here is my regex""),StepDefinition]
                        [ACCESS] void A() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }";

            var Tasks = AccessModifiers
                .Select(AccessModifier => new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                        .WithCode(CodeTemplate.Replace("[ACCESS]", AccessModifier))
                        .WithExpectedDiagnostic(ExpectedDiagnostic(9, 26, 9, 31))
                        .WithExpectedDiagnostic(ExpectedDiagnostic(9, 33, 9, 37))
                        .WithExpectedDiagnostic(ExpectedDiagnostic(9, 39, 9, 63))
                        .WithExpectedDiagnostic(ExpectedDiagnostic(9, 64, 9, 78))
                        .RunAsync())
                .ToArray()
            ;
            Task.WaitAll(Tasks);

        }

        [TestMethod]
        public void MultipleBindingsOnsameMethodMustStillRaiseDiagnostic()
        {
            string CodeTemplate = @"
                using TechTalk.SpecFlow;
    
                namespace ConsoleApplication1
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given]
                        [When]
                        [Then(""here is my regex"")]
                           [StepDefinition("""")]
                        [ACCESS] void A() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }"
            ;

            var Tasks = AccessModifiers
                .Select(AccessModifier => new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                    .WithCode(CodeTemplate.Replace("[ACCESS]", AccessModifier))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(9, 26, 9, 31))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(10, 26, 10, 30))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(11, 26, 11, 50))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(12, 29, 12, 47))
                    .RunAsync())
                .ToArray()
           ;
            Task.WaitAll(Tasks);
        }

        [TestMethod]
        public void NonPublicStepDefinitionsMustCauseDiagnostic()
        {
            string CodeTemplate = @"
                using TechTalk.SpecFlow;
    
                namespace ConsoleApplication1
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given]
                        [ACCESS] void A() {}

                        [When]
                        [ACCESS] void B() {}

                        [Then]
                        [ACCESS] void C() {}

                        [    StepDefinition]
                        [ACCESS] void D() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }"
            ;
            var Tasks = AccessModifiers.Select(AccessModifier => new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                    .WithCode(CodeTemplate.Replace("[ACCESS]", AccessModifier))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(9, 26, 9, 31))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(12, 26, 12, 30))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(15, 26, 15, 30))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(18, 30, 18, 44)).RunAsync())
                    .ToArray()
            ;
            Task.WaitAll(Tasks);
        }

        [TestMethod]
        public void PublicStepDefinitionMayNotRaiseDiagnostic()
        {
            string CodeTemplate = @"
                using TechTalk.SpecFlow;
    
                namespace ConsoleApplication1
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given]
                        public void A() {}

                        [When]
                        public void B() {}

                        [Then]
                        public void C() {}

                        [StepDefinition]
                        public void D() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }";

            Task.WaitAll(new Task[] {new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                .WithCode(CodeTemplate)
                .RunAsync(CancellationToken.None) })
            ;
        }

    }
}
