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
    /// A unit test for the <see cref="ClassMustBePublicAnalyzer"/>  
    /// For top level classes we test these access modifiers:
    /// <list type="number">
    ///    <item>none</item>
    ///    <item><see langword="public"/></item>
    ///    <item><see langword="internal"/></item>
    /// </list>
    /// 
    /// For nested classes we test:
    /// <list type="number">
    ///    <item>none</item>
    ///    <item><see langword="public"/></item>
    ///    <item><see langword="private"/></item>
    ///    <item><see langword="protected"/></item>
    ///    <item><see langword="internal"/></item>
    ///    <item><see langword="protected"/> <see langword="internal"/></item>
    ///    <item><see langword="protected"/> <see langword="private"/></item>
    /// </list>
    /// 
    /// For <see langword="partial"/> classes we test 1 and 2 class declarations.
    /// </summary>
    [TestClass]
    public class ClassMustBePublicAnalyzerUnitTest
    {

        private readonly Func<int, int, int, int, DiagnosticResult> ExpectedDiagnostic = (x1, y1, x2, y2) =>
               new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicClass, DiagnosticSeverity.Warning)
                   .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task TopLevel_None()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]    
    class MyTestCode
    {   
        [Given, When, Then(""here is my regex""),StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 5, 5, 10))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task TopLevel_Public()
        {
            string CodeTemplate = @"
                using TechTalk.SpecFlow;
    
                namespace ConsoleApplication1
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given, When, Then(""here is my regex""),StepDefinition]
                        public void Method1() { }
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                 .WithCode(CodeTemplate)
                 .RunAsync()
            ;
        }

        [TestMethod]
        public async Task TopLevel_Internal()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]    
    internal class MyTestCode
    {   
        [Given, When, Then(""here is my regex""),StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 14, 5, 19))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_Public()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        public class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_Private()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        private class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 17, 7, 22))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_Protected()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        protected class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 19, 7, 24))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_Internal()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        internal class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 18, 7, 23))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_ProtectedInternal()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        protected internal class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 28, 7, 33))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Nested_ProtectedPrivate()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]    
        protected private class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 27, 7, 32))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Partial_One()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]    
    partial class MyTestCode
    {   
        [Given, When, Then(""here is my regex""),StepDefinition]
        public void Method1() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 13, 5, 18))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Partial_Two()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    [Binding]    
    partial class MyTestCode
    {   
        [Given, When, Then(""here is my regex""),StepDefinition]
        public void Method1() {}
    }

    partial class MyTestCode
    {   
        [Given, When, Then(""here is my regex""),StepDefinition]
        public void Method2() {}
    }
}";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(5, 13, 5, 18))
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 13, 11, 18))
                .RunAsync()
            ;
        }
    }
}
