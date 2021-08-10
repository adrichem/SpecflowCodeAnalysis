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
    /// <see cref="StepDefinitionMustBePublicAnalyzer"/> and
    /// <see cref="StepDefinitionMustBePublicCodeFixProvider"/>  
    ///
    /// We test the following situations regarding modifiers.
    /// <list type="table">
    ///    <item><term>0</term><term><see langword="public"/></term></item>
    ///    <item><term>1</term><term>none</term></item>
    ///    <item><term>2</term><term><see langword="private"/></term></item>
    ///    <item><term>3</term><term><see langword="protected"/></term></item>
    ///    <item><term>4</term><term><see langword="internal"/></term></item>
    ///    <item><term>5</term><term><see langword="protected"/> <see langword="private"/></term></item>
    ///    <item><term>6</term><term><see langword="static"/></term></item>
    ///    <item><term>7</term><term><see langword="partial"/></term></item>
    /// </list>
    /// 
    /// For trivia we test the following situations:
    /// 
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Num modifiers (0|1|MORE)</term>
    ///         <term>Are there access modifiers? (YES|NO)</term>
    ///         <term>Leading trivia on 1st modifier or return type? (YES|NO)</term>
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
    public class StepDefinitionMustBePublicUnitTest
    {

        private readonly Func<int,int,int,int, DiagnosticResult> ExpectedDiagnostic = (x1,y1,x2,y2) => 
            new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicMethod, DiagnosticSeverity.Warning)
                .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task Modifiers00()
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

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer>()
                .WithCode(CodeTemplate)
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
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        void TestMethod() {  }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 14, 8, 24))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        private void TestMethod() {  }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 22, 8, 32))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        protected void TestMethod() {  }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 24, 8, 34))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        internal void TestMethod() {  }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 23, 8, 33))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        protected private void TestMethod() {  }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 32, 8, 42))
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
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        static void TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public static void TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 21, 8, 31))
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
    [Binding]
    public partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        partial void TestMethod();
    }

    public partial class MyTestCode
    {   
        partial void TestMethod() { }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public partial void TestMethod();
    }

    public partial class MyTestCode
    {   
        public partial void TestMethod() { }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 22, 8, 32))
                .WithExpectedDiagnostic(ExpectedDiagnostic(13, 22, 13, 32))
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
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        void /* trailing */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public void /* trailing */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 29, 8, 39))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///<summary>
        ///
        ///</summary>
        void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///<summary>
        ///
        ///</summary>
        public void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 36, 11, 46))
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
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        static void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public static void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 43, 8, 53))
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
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
///<summary>
///</summary>
        static void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public static class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
///<summary>
///</summary>
        public static void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 43, 10, 53))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        protected void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 46, 8, 56))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///
        ///
        ///
        protected void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///
        ///
        ///
        public void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 46, 11, 56))
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
    [Binding]
    public static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        static partial void /* trailing trivia */ TestMethod();

        static partial void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        public static partial void /* trailing trivia */ TestMethod();

        public static partial void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 51, 8, 61))
                .WithExpectedDiagnostic(ExpectedDiagnostic(10, 51, 10, 61))
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
    [Binding]
    public static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
/*
leading trivia
*/
        static partial void /* trailing trivia */ TestMethod();

        static partial void /* trailing trivia */ TestMethod() {  }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public static partial class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
/*
leading trivia
*/
        public static partial void /* trailing trivia */ TestMethod();

        public static partial void /* trailing trivia */ TestMethod() {  }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 51, 11, 61))
                .WithExpectedDiagnostic(ExpectedDiagnostic(13, 51, 13, 61))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        static protected void /* trailing trivia */ TestMethod() { }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        static public void /* trailing trivia */ TestMethod() { }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 63))
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
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///
        ///
        ///
        protected /* trailing trivia */ static void /* trailing trivia */ TestMethod() { }
    }
}";
            string ExpectedResult = @"using TechTalk.SpecFlow;
namespace a
{
    [Binding]
    public class MyTestCode
    {   
        [Given,When,Then,StepDefinition]
        ///
        ///
        ///
        public /* trailing trivia */ static void /* trailing trivia */ TestMethod() { }
    }
}";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<StepDefinitionMustBePublicAnalyzer, StepDefinitionMustBePublicCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 75, 11, 85))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }
    }
}
