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
    /// A unit test for 
    /// <see cref="ClassMustBePublicAnalyzer"/> and
    /// <see cref="ClassMustBePublicCodeFixProvider"/>  
    ///
    /// We test the following situations regarding access modifiers.
    /// <list type="table">
    ///    <item><term>0</term><term><see langword="public"/></term></item>
    ///    <item><term>1</term><term>none</term></item>
    ///    <item><term>2</term><term><see langword="private"/></term></item>
    ///    <item><term>3</term><term><see langword="protected"/></term></item>
    ///    <item><term>4</term><term><see langword="internal"/></term></item>
    ///    <item><term>5</term><term><see langword="protected"/> <see langword="internal"/></term></item>
    ///    <item><term>7</term><term><see langword="static"/></term></item>
    ///    <item><term>8</term><term>class is <see langword="partial"/> but has only 1 declaration</term></item>
    ///    <item><term>9</term><term>class is <see langword="partial"/> and has multiple declarations</term></item>
    ///    <item><term>10</term><term>class is a nested class</term></item>
    /// </list>
    /// 
    /// For trivia we test the following situations:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Num modifiers (0|1|MORE)</term>
    ///         <term>Are there access modifiers? (YES|NO)</term>
    ///         <term>Leading trivia on 1st modifier or class keyword? (YES|NO)</term>
    ///     </listheader>
    ///    <item>
    ///         <term>1</term>
    ///         <term>0</term>
    ///         <term>NO</term>
    ///         <term>NO</term>
    ///    </item> 
    ///    <item>
    ///         <term>2</term>
    ///         <term>0</term>
    ///         <term>NO</term>
    ///         <term>YES</term>
    ///     </item>   
    ///    <item>
    ///         <term>(N/A)</term>
    ///         <term>0</term>
    ///         <term>YES</term>
    ///         <term>NO</term>
    ///    </item>  
    ///    <item>
    ///         <term>(N/A)</term>
    ///         <term>0</term>
    ///         <term>YES</term>
    ///         <term>YES</term>
    ///    </item>     
    ///    <item>
    ///         <term>5</term>
    ///         <term>1</term>
    ///         <term>NO</term>
    ///         <term>NO</term>
    ///    </item>   
    ///    <item>
    ///         <term>6</term>
    ///         <term>1</term>
    ///         <term>NO</term>
    ///         <term>YES</term>
    ///    </item>  
    ///    <item>
    ///         <term>7</term>
    ///         <term>1</term>
    ///         <term>YES</term>
    ///         <term>NO</term>
    ///    </item>      
    ///    <item>
    ///         <term>8</term>
    ///         <term>1</term>
    ///         <term>YES</term>
    ///         <term>YES</term>
    ///    </item>  
    ///    <item>
    ///         <term>9</term>
    ///         <term>MORE</term>
    ///         <term>NO</term>
    ///         <term>NO</term>
    ///    </item>  
    ///    <item>
    ///         <term>10</term>
    ///         <term>MORE</term>
    ///         <term>NO</term>
    ///         <term>YES</term>
    ///    </item> 
    ///    <item>
    ///         <term>11</term>
    ///         <term>MORE</term>
    ///         <term>YES</term>
    ///         <term>NO</term>
    ///    </item> 
    ///    <item>
    ///         <term>12</term>
    ///         <term>MORE</term>
    ///         <term>YES</term>
    ///         <term>YES</term>
    ///    </item>  
    /// </list>
    /// </summary>
    [TestClass]
    public class ClassMustBePublicUnitTest
    {
        private readonly Func<int, int, int, int, DiagnosticResult> ExpectedDiagnostic = (x1, y1, x2, y2) =>
               new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicClass
                   , DiagnosticSeverity.Warning
                   )
                   .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task Modifiers00()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
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
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Modifiers01()
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
        public async Task Modifiers02()
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
        public async Task Modifiers03()
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
        public async Task Modifiers04()
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
        public async Task Modifiers05()
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
        public async Task Modifiers06()
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
        public async Task Modifiers07()
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
        public async Task Modifiers08()
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
        public async Task Modifiers09()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]
        ///
        ///
        ///
        private class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace ConsoleApplication1
{
    class outer
    {
        [Binding]
        ///
        ///
        ///
        public class inner
        {   
            [Given, When, Then(""here is my regex""),StepDefinition]
            public void Method1() {}
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 17, 10, 22))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }


        [TestMethod]
        public async Task Trivia01()
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
        public async Task Trivia02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
///<summary>
   ///</summary>
     class MyTestCode
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
///<summary>
   ///</summary>
     public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 6, 9, 11))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        static class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        public static class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 16, 7, 21))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        /**
        * Leading trivia
        */
        static class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        /**
        * Leading trivia
        */
        public static class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 16, 10, 21))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia07()
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
        public async Task Trivia08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        [Binding]
        ///<summary>
        ///this is my leading trivia
        ///</summary>
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
        ///<summary>
        ///this is my leading trivia
        ///</summary>
        public class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 19, 10, 24))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia09()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public static void TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    public static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public static void TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(4, 20, 4, 25))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia10()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
        ///<summary>
        ///This is a comment
        ///</summary>
        static partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
        ///<summary>
        ///This is a comment
        ///</summary>
        public static partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(7, 24, 7, 29))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia11()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        protected internal partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        public partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(6, 36, 6, 41))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Trivia12()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        ///<summary>
        /// Leading trivia on 1st modifier
        ///</summary
        protected internal partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    class foo
    {
        ///<summary>
        /// Leading trivia on 1st modifier
        ///</summary
        public partial class MyTestCode
        {   
            [Given,When,Then,StepDefinition]
            public static void TestMethod() {  }
        }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ClassMustBePublicAnalyzer, ClassMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 36, 9, 41))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }
    }
}
