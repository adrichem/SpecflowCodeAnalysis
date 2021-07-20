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
    using SpecFlowCodeAnalyzers.CodeFixes;


    /// <summary>
    /// A unit test for the <see cref="ClassMustBePublicCodeFixProvider"/>  
    ///
    /// We test the following situations regarding access modifiers.
    /// <list type="number">
    ///    <item>none</item>
    ///    <item><see langword="private"/></item>
    ///    <item><see langword="protected"/></item>
    ///    <item><see langword="internal"/></item>
    ///    <item><see langword="protected"/> <see langword="internal"/></item>
    ///    <item><see langword="protected"/> <see langword="private"/></item>
    ///    <item>class is <see langword="static"/></item>
    ///    <item>class is <see langword="partial"/> but has only 1 declaration</item>
    ///    <item>class is <see langword="partial"/> and has multiple declarations</item>
    /// </list>
    /// 
    /// For whitespaceing we test the following situations:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Leading trivia on first replaced modifier</term>
    ///         <term>trailing trivia on last replaced modifier</term>
    ///     </listheader>
    ///     <item><term>1</term><term>n/a</term><term>n/a</term></item>
    ///     <item><term>2</term><term>none</term><term>many</term></item>
    ///     <item><term>3</term><term>many</term><term>many</term></item>
    /// </list>
    /// </summary>
    [TestClass]
    public class ClassMustBePublicCodeFixUnitTest
    {
        private readonly Func<int, int, int, int, DiagnosticResult> ExpectedDiagnostic = (x1, y1, x2, y2) =>
               new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicClass
                   , DiagnosticSeverity.Warning
                   )
                   .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task CodeFixSituation01()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    class MyTestCode
        {   
        [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
        {   
        [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 5, 5, 10))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        private class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 17, 7, 22))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }
        
        [TestMethod]
        public async Task CodeFixSituation03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        protected class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 19, 7, 24))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation04()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        internal class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 18, 7, 23))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        protected internal class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 28, 7, 33))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        protected private class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 27, 7, 32))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation07()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        static protected internal class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            static public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        static public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            static public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 35, 7, 40))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }


        [TestMethod]
        public async Task CodeFixSituation08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        internal partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 26, 7, 31))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation09()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        internal partial class A
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethodA() {  }
        }
        
        internal partial class B
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethodB() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public partial class A
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethodA() {  }
        }
        
        public partial class B
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethodB() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 26, 7, 31))
                .WithExpectedDiagnostic(ExpectedDiagnostic(13, 26, 13, 31))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }


        [TestMethod]
        public async Task TriviaSituation01()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
class MyTestCode
        {   
        [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
        {   
        [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 1, 5, 6))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task TriviaSituation02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
protected private         class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
public         class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 27, 7, 32))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task TriviaSituation03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
   protected private         class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
   public         class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 30, 7, 35))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }
    }
}
