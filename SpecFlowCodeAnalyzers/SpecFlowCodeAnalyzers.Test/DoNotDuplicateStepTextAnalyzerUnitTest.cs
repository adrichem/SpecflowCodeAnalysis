namespace SpecFlowCodeAnalyzers.Test
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using SpecFlowCodeAnalyzers.Test.Common;
    using System;
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;

    /// <summary>
    /// A unit test for the <see cref="DoNotDuplicateStepTextAnalyzer"/>  
    /// <br/>
    /// It has the following test situations:
    /// <list type = "table" >
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Attribute(s) have step text?</term>
    ///         <term>Method has attributes with duplicated step text value?</term>
    ///         <term>One of the attributes is a [StepDefinition]?</term>
    ///     </listheader>
    ///     <item><term>1</term><term>NO</term><term>NO</term><term>NO</term></item>
    ///     <item><term>2</term><term>NO</term><term>NO</term><term>YES</term></item>
    ///     <item><term>3</term><term>NO</term><term>YES</term><term>NO</term></item>
    ///     <item><term>4</term><term>NO</term><term>YES</term><term>YES</term></item>
    ///     <item><term>5</term><term>YES</term><term>NO</term><term>NO</term></item>
    ///     <item><term>6</term><term>YES</term><term>NO</term><term>YES</term></item>
    ///     <item><term>7</term><term>YES</term><term>YES</term><term>NO</term></item>
    ///     <item><term>8</term><term>YES</term><term>YES</term><term>YES</term></item>
    /// </list>
    /// </summary>
    [TestClass]
    public class DoNotDuplicateStepTextAnalyzerUnitTest
    {

        private readonly Func<int, int, int, int, DiagnosticResult> ExpectedDiagnostic = (x1, y1, x2, y2) =>
               new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.DoNotDuplicateStepText, DiagnosticSeverity.Warning)
                   .WithSpan(x1, y1, x2, y2)
        ;


        [TestMethod]
        public async Task Situation01()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given()]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [StepDefinition()]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(), When()]
        [Then]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 10, 7, 17))
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 19, 7, 25))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 10, 8, 14))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation04()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(), When()]
        [Then]
        [StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 10, 7, 17))
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 19, 7, 25))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 10, 8, 14))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(""1""), When(""2"")]
        [Then(""3"")]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(""1""), When(""2"")]
        [StepDefinition(""3"")]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation07()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(""1""), When(""1"")]
        [Then(""1"")]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 10, 7, 20))
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 22, 7, 31))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 10, 8, 19))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace test
{
    [Binding]    
    class MyTestCode
    {   
        [Given(""1"")]
        [StepDefinition(""1"")]
        [Given(""2"")]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<DoNotDuplicateStepTextAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 10, 7, 20))
                .RunAsync()
            ;
        }

    }
}
