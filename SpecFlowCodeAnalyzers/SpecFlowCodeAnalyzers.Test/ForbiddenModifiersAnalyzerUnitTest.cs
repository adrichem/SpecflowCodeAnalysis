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
    /// A unit test for the <see cref="ForbiddenModifiersAnalyzer"/> analyzer.
    /// 
    /// Tests the following situations on methods:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Num params with forbidden modifier</term>
    ///         <term>Num paramet without forbidden modifier</term>
    ///     </listheader>
    ///     <item><term>1</term><term>0</term><term>0</term></item>
    ///     <item><term>2</term><term>1</term><term>0</term></item>
    ///     <item><term>3</term><term>2</term><term>0</term></item>
    ///     <item><term>4</term><term>0</term><term>1</term></item>
    ///     <item><term>5</term><term>1</term><term>1</term></item>
    ///     <item><term>6</term><term>2</term><term>1</term></item>
    ///     <item><term>7</term><term>0</term><term>2</term></item>
    ///     <item><term>8</term><term>1</term><term>2</term></item>
    ///     <item><term>9</term><term>2</term><term>2</term></item>
    /// </list>
    ///
    /// Tests the following situations w.r.t. location of forbidden modifier:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Situation#</term>
    ///         <term>Relative location of parameter with forbidden modifier</term>
    ///         <term>Number of lines in parameter list</term>
    ///     </listheader>
    ///     <item><term>A</term><term>First</term><term>1</term></item>
    ///     <item><term>B</term><term>First</term><term>multiple</term></item>
    ///     <item><term>C</term><term>Middle</term><term>1</term></item>
    ///     <item><term>D</term><term>Middle</term><term>multiple</term></item>
    ///     <item><term>E</term><term>Last</term><term>1</term></item>
    ///     <item><term>F</term><term>Last</term><term>multiple</term></item>
    /// </list>
    /// </summary>
    [TestClass]
    public class ForbiddenModifiersAnalyzerUnitTest
    {
        private readonly Func<int,int,int,int, DiagnosticResult> ExpectedDiagnostic = (x1,y1,x2,y2) => 
            new DiagnosticResult(SpecFlowCodeAnalyzersDiagnosticIds.ForbiddenModifier
                , DiagnosticSeverity.Warning
                )
                .WithSpan(x1, y1, x2, y2)
        ;

        [TestMethod]
        public async Task Situation01()
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
        public async Task Situation02()
        {

            string CodeTemplate = @"
                using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void TestMethod(out int a) { a = 1;}
                    }
                }";

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 48, 9, 51))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 48, 9, 51))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation03()
        {

            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a, out string b) 
                        { 
                            a = 1;
                            b = string.Empty;
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation04()
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
        public async Task Situation05()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a, out string b) { 
                            a = 1; b = string.Empty;

                         }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 53, 8, 56))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation06()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a, string b, out object c) { 
                            a = 1; c = null;

                         }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 63, 8, 66))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 63, 8, 66))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation07()
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
        public async Task Situation08()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a, string b, out object c) { c = null; }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 59, 8, 62))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 59, 8, 62))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation09()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a, string b, out object c, string d) { a = 1; c = null; }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 63, 8, 66))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 63, 8, 66))
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Situation_A()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a, string b, object c, string d) { a = 1; }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Situation_B()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(out int a
                            , string b
                            , object c, string d) 
                        { 
                            a = 1; 
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 42, 8, 45))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation_C()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a, out string b, object c, string d) 
                        { 
                            b = string.Empty; 
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 49, 8, 52))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 49, 8, 52))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation_D()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a, 
                            out string b
                            , object c, string d) 
                        { 
                            b = string.Empty; 
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 29, 9, 32))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(9, 29, 9, 32))
                .RunAsync()
            ;
            
        }

        [TestMethod]
        public async Task Situation_E()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a, string b, object c, out string d) 
                        { 
                            d = string.Empty; 
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 69, 8, 72))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(8, 69, 8, 72))
                .RunAsync()
            ;
        }

        [TestMethod]
        public async Task Situation_F()
        {
            string CodeTemplate = @"using TechTalk.SpecFlow;
                namespace UnitTestOfParametersMayNotBeOut
                {
                    [Binding]    
                    public class MyTestCode
                    {   
                        [Given,When,Then,StepDefinition]
                        public void Test(int a
                        , string b
                        , object c
                        , out string d) 
                        { 
                            d = string.Empty; 
                        }
                    }
                }";
            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate)
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 27, 11, 30))
                .RunAsync()
            ;

            await new CSharpAnalyzerTestWithSpecFlowAssemblies<ForbiddenModifiersAnalyzer>()
                .WithCode(CodeTemplate.Replace("out", "ref"))
                .WithExpectedDiagnostic(ExpectedDiagnostic(11, 27, 11, 30))
                .RunAsync()
            ;
            
        }
    }
}
