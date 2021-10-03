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
|SPECFLOW0006| The step definition has attributes with duplicated step text.|
|SPECFLOW0007|  The string of the `Given()/When()/Then()/StepDefinition` attribute contains words banned by `BannedStepTextPatterns.xml`.|
|SPECFLOW0008| `BannedStepTextPatterns.xml` has an error in it.|

### Configuring banned words
To check your reg-exes for invalid words:
1. Add a file named `BannedStepTextPatterns.xml` to your project.
2. In the solution explorer, set the build action to 'AdditionalFiles' on `BannedStepTextPatterns.xml`
3. Put content like this into `BannedStepTextPatterns.xml`: 
```xml
<?xml version="1.0" encoding="utf-16"?>
<banlist xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
         xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
         xmlns="dec2ed27-578d-445b-8bfa-43f4fbe150b6">
  <items>
    <item phrase="Hello" severity="Error" />
    <item phrase="World" severity="Warning" ignoreCase="true" message="try to use 'everyone' instead of 'World'" />
  </items>
</banlist>
```
# BindingAnalyzer

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
