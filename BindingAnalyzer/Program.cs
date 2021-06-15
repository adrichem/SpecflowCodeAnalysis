namespace BindingAnalyzer
{
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.MSBuild;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MoreLinq;

    using Newtonsoft.Json;
    class Program
    {
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => {
                    if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure) {
                        Console.Error.WriteLine(e.Diagnostic.Message);
                    }
                };

                var solutionPath = args.First();

                var solution = await workspace.OpenSolutionAsync(solutionPath);

                var GivenWhenThens = new List<DiscoveredAttributeUsage>();

                foreach (var Project in solution.Projects)
                {
                    var compilation = Project.GetCompilationAsync().Result;
                    var Walker = new AttributeUsageFinder
                    {
                        Attributes = new List<INamedTypeSymbol>
                        {
                            compilation.GetTypeByMetadataName("TechTalk.SpecFlow.GivenAttribute"),
                            compilation.GetTypeByMetadataName("TechTalk.SpecFlow.WhenAttribute"),
                            compilation.GetTypeByMetadataName("TechTalk.SpecFlow.ThenAttribute"),
                        },
                        ReportAttr = (syntax, symbol, attr) =>
                        {
                            GivenWhenThens.Add(new DiscoveredAttributeUsage
                            {
                                Keyword = attr.AttributeClass.Name,
                                Text = attr.ConstructorArguments.First().Value.ToString(),
                                Method = symbol,
                                MethodSyntax = syntax
                            });
                        }
                    };

                    foreach(var Tree in compilation.SyntaxTrees)
                    {
                        Walker.SemanticModel = compilation.GetSemanticModel(Tree);
                        Walker.Visit(Tree.GetRoot());
                    }
                }

                if (GivenWhenThens.Any())
                {
                    List<string> FeatureFiles = new List<string>();
                    solution.Projects.ForEach(Project => {
                        FeatureFiles
                            .AddRange(
                                Directory.GetFiles(
                                    Path.GetDirectoryName(Project.FilePath),
                                    "*.feature",
                                    SearchOption.AllDirectories
                                )
                            ); });
                    var Counter = new GivenWhenThenCounter { 
                        DiscoveredAttributes = GivenWhenThens, 
                        FeatureFiles = FeatureFiles 
                    }.Analyze();


                    var Result = new
                    {
                        UnusedBindingMethods = Counter
                            .BindingsUsage
                            .GroupBy(x => x.Key.Method)
                            .Where(grp => grp.Sum(x => x.Value) == 0)
                            .Select(grp => grp.Key.ToString())
                            .ToList(),

                        UnusedAttributes = Counter
                            .BindingsUsage
                            .Where(x => x.Value == 0)
                            .Select( x => new
                            {
                                File = x.Key.MethodSyntax.GetLocation().SourceTree.FilePath,
                                Line = x.Key.MethodSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                                Keyword = x.Key.Keyword.Substring(0, x.Key.Keyword.LastIndexOf("Attribute")),
                                StepText = x.Key.Text
                            })
                    };

                    Console.WriteLine(JsonConvert.SerializeObject(Result, Formatting.Indented));
                }
            }
        }
        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }
    }
}
