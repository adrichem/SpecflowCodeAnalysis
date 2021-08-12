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
    /// A unit test for <see cref="ForbiddenModifiersCodeFixProvider"/> and <see cref="ForbiddenModifiersAnalyzer"/>.
    /// 
    /// The following situations cover both the analyzer and the codefixer w.r.t. to keywords:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation</term>
    ///         <term>Amount of replaced keywords in same parameter list</term>
    ///         <term>Amount of non relevant parameters  in same parameter list</term>
    ///     </listheader>
    ///     <item><term>1</term><term>0</term>0</item>
    ///     <item><term>2</term><term>0</term>1</item>
    ///     <item><term>3</term><term>0</term>2</item>
    ///     <item><term>4</term><term>1</term>0</item>
    ///     <item><term>5</term><term>1</term>1</item>
    ///     <item><term>6</term><term>1</term>2</item>
    ///     <item><term>7</term><term>2</term>0</item>
    ///     <item><term>8</term><term>2</term>1</item>
    ///     <item><term>9</term><term>2</term>2</item>
    /// </list>
    /// </summary>
    [TestClass]
    public class ForbiddenModifiersUnitTest
    {
        private readonly Func<int,int,int,int, DiagnosticResult> ExpectedDiagnostic = (x1,y1,x2,y2) => 
            new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.ForbiddenModifier
                , DiagnosticSeverity.Warning
                )
                .WithSpan(x1, y1, x2, y2)
        ;



        [TestMethod]
        public async Task Keyword01()
        {

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod() { }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Keyword02()
        {

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a) { }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Keyword03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a, string b) { }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Keyword04()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out int a) { a = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a) { a = 1; }
                    }
                }";

            var d1 = ExpectedDiagnostic(8, 48, 8, 51);
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
               .WithCode(CodeTemplate.Replace("out","ref"))
               .WithExpectedDiagnostic(d1)
               .WithFixCode(ExpectedResult)
               .RunAsync()
           ;
        }

        [TestMethod]
        public async Task Keyword05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,ref int b) { b = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,int b) { b = 1; }
                    }
                }";
            var d1 = ExpectedDiagnostic(8, 54, 8, 57);

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(d1)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Keyword06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out int a, string b, string c) { a = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a, string b, string c) { a = 1; }
                    }
                }";

            var d1 = ExpectedDiagnostic(8, 48, 8, 51);

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(d1)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Keyword07()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out int b,out int c) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int b,int c) { b = 1; c = 1; }
                    }
                }";

            var d1 = ExpectedDiagnostic(8, 48, 8, 51);
            var d2 = ExpectedDiagnostic(8, 58, 8, 61);

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Keyword08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out int a,out int b, int c) { b = a = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,int b, int c) { b = a = 1; }
                    }
                }";

            var d1 = ExpectedDiagnostic(8, 48, 8, 51);
            var d2 = ExpectedDiagnostic(8, 58, 8, 61);

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Keyword09()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a, out int b, out int c, int d) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a, int b, int c, int d) { b = 1; c = 1; }
                    }
                }";

            var d1 = ExpectedDiagnostic(8, 55, 8, 58);
            var d2 = ExpectedDiagnostic(8, 66, 8, 69);
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(d1)
                .WithExpectedDiagnostic(d2)
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

       
    }
}
