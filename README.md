# SpecflowCodeAnalysis

Finds bindings methods and Given/When/Then attributes that are never used by feature files.

Usage:
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
      "Line": 58,
      "Keyword": "When",
      "StepText": "I Wait for '(.*)'"
    },
 ]
 ```