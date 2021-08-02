# SpecflowCodeAnalysis

## Code analyzers for Visual Studio and VS Code


Analyzes your [SpecFlow](https://specflow.org) step definition code for common code smells. 

Install the NuGet package into your project. Visual Studio / VS Code will start raising errors and warnings. For some of the reported issues, you can use the light bulb to automatically fix them.

|Issue|Reported when|
|-|-|
|SPECFLOW0001| The string of the `Given()/When()/Then()/StepDefinition` attribute is not a valid regular expression. |
|SPECFLOW0002| The step definition is not a `public` method.|
|SPECFLOW0003| The step definition has `out` or `ref` modifier on a parameter.|
|SPECFLOW0004| The step definition's class is not `public`.|
|SPECFLOW0005| The step definition's class does not have the `[Binding]` attribute.|

## BindingAnalyzer

Finds step definitions and Given/When/Then attributes that are never used by any of the .feature files. Usage:

```ps1
$ .\BindingAnalyzer.exe C:\Repos\TestAutomation\VisualStudioSolution.sln
```
Example output
```json
{
  "UnusedBindingMethods": [
    "MyNamespace.BindingClass.Method1(string)",
    "MyNamespace.BindingClass.Method2(string, TechTalk.SpecFlow.Table)",
  ],
 "UnusedAttributes": [
    {
      "File": "C:\\Repos\\TestAutomation\\Bindings\\File1.cs",
      "Line": 58,
      "Keyword": "Given",
      "StepText": "I wait for '(.*)'"
    },
    {
      "File": "C:\\Repos\\TestAutomation\\Bindings\\File1.cs",
      "Line": 59,
      "Keyword": "When",
      "StepText": "I Wait for '(.*)'"
    },
 ]
 ```