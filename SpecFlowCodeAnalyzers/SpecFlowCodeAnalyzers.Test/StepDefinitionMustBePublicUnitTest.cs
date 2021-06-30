﻿namespace SpecFlowCodeAnalyzers.Test
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
                        [ACCESS] 
                        void Abcdefg() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }";

            var Tasks = AccessModifiers
                .Select(AccessModifier => new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                        .WithCode(CodeTemplate.Replace("[ACCESS]", AccessModifier))
                        .WithExpectedDiagnostic(ExpectedDiagnostic(11, 30, 11, 37))
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
                        [ACCESS] 
                        void A() {}

                        [Given(""blah blah"")]
                        [ACCESS] 
                        void ABC() {}

                        public void NotAStepDefinition() {}

                        private void NotAStepDefinitionPrivate() {}
                    }
                }"
            ;

            var Tasks = AccessModifiers
                .Select(AccessModifier => new TestWithSpecFlowAssemblies<StepDefinitionMustBePublic>()
                    .WithCode(CodeTemplate.Replace("[ACCESS]", AccessModifier))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(14, 30, 14, 31))
                    .WithExpectedDiagnostic(ExpectedDiagnostic(18, 30, 18, 33))
                    .RunAsync())
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
