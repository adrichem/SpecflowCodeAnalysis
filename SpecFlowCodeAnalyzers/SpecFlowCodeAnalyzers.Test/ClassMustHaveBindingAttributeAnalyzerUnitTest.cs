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
    /// A unit test for the <see cref="ClassMustHaveBindingAttributeAnalyzer"/>
    /// Tests the following situations:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Is partial class?</term>
    ///         <term>Has [Binding] attribute?</term>
    ///         <term>Has any stepdefinition methods?</term>
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
    public class ClassMustHaveBindingAttributeAnalyzerUnitTest
    {

        private readonly Func<int, int, int, int, DiagnosticResult> ExpectedDiagnostic = (x1, y1, x2, y2) =>
               new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.BindingAttributeMissing, DiagnosticSeverity.Warning)
                   .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task Situation01()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    public class MyTestCode
    {   
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }


        [TestMethod]
        public async Task Situation02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    public class MyTestCode
    {   
        [StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(4, 12, 4, 17))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]
    public class MyTestCode
    {   
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task Situation04()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]
    public class MyTestCode
    {   
        [StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task Situation05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{

    public partial class MyTestCode
    {   
        public void Method1() {}
    }

    public partial class MyTestCode
    {   
        public void Method2() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task Situation06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    public partial class MyTestCode
    {   
        [StepDefinition]
        public void Method1() {}
    }

public partial class MyTestCode
{   

    public void Method2() {}
}
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(4, 20, 4, 25))
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 16, 10, 21))
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task Situation07()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [TechTalk.SpecFlow.Binding]
    public partial class MyTestCode
    {   
        public void Method1() {}
    }

public partial class MyTestCode
{   

    public void Method2() {}
}
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task Situation08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [TechTalk.SpecFlow.Binding]
    public partial class MyTestCode
    {   
        public void Method1() {}
    }

public partial class MyTestCode
{   
    [Given]
    public void Method2() {}
}
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustHaveBindingAttributeAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }
        

    }
}
