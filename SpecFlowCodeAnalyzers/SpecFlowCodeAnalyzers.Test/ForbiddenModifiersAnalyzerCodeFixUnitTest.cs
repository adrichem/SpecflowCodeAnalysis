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
    /// A unit test for <see cref="ForbiddenModifiersAnalyzerCodeFixProvider"/>.
    /// 
    /// Tests the following situations on removing the out keyword from a parameter list:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation</term>
    ///         <term>leading trivia</term>
    ///         <term>trailing trivia</term>
    ///         <term>Amount of replaced keywords on same parameter list</term>
    ///     </listheader>
    ///     <item><term>1</term><term>NO</term><term>1 space</term><term>1</term></item>
    ///     <item><term>2</term><term>NO</term><term>1 space</term><term>Multiple</term></item>
    ///     <item><term>3</term><term>NO</term><term>many whitespace</term><term>1</term></item>
    ///     <item><term>4</term><term>NO</term><term>many whitespace</term><term>Multiple</term></item>
    ///     <item><term>5</term><term>YES</term><term>1 space</term><term>1</term></item>
    ///     <item><term>6</term><term>YES</term><term>1 space</term><term>Multiple</term></item>
    ///     <item><term>7</term><term>YES</term><term>many whitespace</term><term>1</term></item>
    ///     <item><term>8</term><term>YES</term><term>many whitespace</term><term>Multiple</term></item>
    /// </list>
    /// </summary>
    [TestClass]
    public class ForbiddenModifiersAnalyzerCodeFixUnitTest
    {
        private readonly Func<int,int,int,int, DiagnosticResult> ExpectedDiagnostic = (x1,y1,x2,y2) => 
            new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.ForbiddenModifier
                , DiagnosticSeverity.Warning
                )
                .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task CodeFixSituation01()
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 48, 8, 51))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
               .WithCode(CodeTemplate.Replace("out","ref"))
               .WithExpectedDiagnostic(ExpectedDiagnostic(8, 48, 8, 51))
               .WithFixCode(ExpectedResult)
               .RunAsync()
           ;
        }

        [TestMethod]
        public async Task CodeFixSituation02()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,out int b,out int c, int d) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,int b,int c, int d) { b = 1; c = 1; }
                    }
                }";                
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 54, 8, 57))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 64, 8, 67))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 54, 8, 57))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 64, 8, 67))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation03()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out         int a) { a = 1; }
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
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 48, 8, 51))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 48, 8, 51))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task CodeFixSituation04()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,out     int b,out    int c, int d) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,int b,int c, int d) { b = 1; c = 1; }
                    }
                }";                
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 54, 8, 57))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 68, 8, 71))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 54, 8, 57))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 68, 8, 71))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task CodeFixSituation05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(   out int a) { a = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(   int a) { a = 1; }
                    }
                }";                
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 51, 8, 54))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 51, 8, 54))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task CodeFixSituation06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,   out int b, out int c, int d) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a,   int b, int c, int d) { b = 1; c = 1; }
                    }
                }";                
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 57, 8, 60))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 68, 8, 71))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 57, 8, 60))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 68, 8, 71))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task CodeFixSituation07()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(     out         int a) { a = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(     int a) { a = 1; }
                    }
                }";                
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer,ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

        }

        [TestMethod]
        public async Task CodeFixSituation08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a, out     int b,        out    int c, int d) { b = 1; c = 1; }
                    }
                }";
            string ExpectedResult = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(int a, int b,        int c, int d) { b = 1; c = 1; }
                    }
                }";
            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 55, 8, 58))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 77, 8, 80))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;

            await new CSharpCodeFixTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer, ForbiddenModifiersAnalyzerCodeFixProvider>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 55, 8, 58))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 77, 8, 80))
                .WithFixCode(ExpectedResult)
                .RunAsync()
            ;
            
        }

    }
}
