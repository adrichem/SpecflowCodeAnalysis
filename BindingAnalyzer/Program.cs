namespace BindingAnalyzer
{
    using Microsoft.Build.Locator;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.MSBuild;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MoreLinq;
    using Gherkin;
    using Gherkin.Ast;
    using System.Text.RegularExpressions;
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
                         ReportAttr = (attr) => GivenWhenThens.Add(attr)
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
                   var Result = new UnusedGivenWhenThenFinder { 
                       DiscoveredAttributes = GivenWhenThens, 
                       FeatureFiles = FeatureFiles 
                   }.Analyze();

                    Console.WriteLine(JsonConvert.SerializeObject(Result.Result, Formatting.Indented));
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
        private class DiscoveredAttributeUsage
        {
            /// <summary>
            /// The method symbol on which the attribute was found
            /// </summary>
            public IMethodSymbol Method { get; set; }

            /// <summary>
            /// The method syntax on which the attribute was found
            /// </summary>
            public MethodDeclarationSyntax MethodSyntax { get; set; }

            /// <summary>
            /// The Gherkin keyword of this attribute (Given/When/Then)
            /// </summary>
            public string Keyword { get; set; }

            /// <summary>
            /// The string provided to the constructor of the attribute
            /// </summary>
            public string Text { get; set; }
        }
        private class AttributeUsageFinder : CSharpSyntaxWalker
        {
            public SemanticModel SemanticModel { get; set; }
            public IEnumerable<INamedTypeSymbol> Attributes { get; set; }
            public Action<DiscoveredAttributeUsage> ReportAttr { get; set; }
            public override void VisitMethodDeclaration(MethodDeclarationSyntax MethodSyntax)
            {
                IMethodSymbol MethodSymbol = SemanticModel.GetDeclaredSymbol(MethodSyntax);

                MethodSymbol
                    .GetAttributes()
                    .Where(a => Attributes.Any(x => SymbolEqualityComparer.Default.Equals(x, a.AttributeClass)))
                    .ForEach(attr => ReportAttr(new DiscoveredAttributeUsage
                    {
                         Keyword = attr.AttributeClass.Name,
                         Text = attr.ConstructorArguments.First().Value.ToString(),
                         Method = MethodSymbol,
                         MethodSyntax = MethodSyntax
                    }))
                ;
                base.VisitMethodDeclaration(MethodSyntax);
            }
        }
        private class UnusedGivenWhenThenFinder
        {
            public IEnumerable<DiscoveredAttributeUsage> DiscoveredAttributes { get; set; }
            public IEnumerable<string> FeatureFiles{ get; set; }
            public object Result { get; private set; }
            public UnusedGivenWhenThenFinder Analyze()
            {
                IDictionary<DiscoveredAttributeUsage, int> BindingsUsage = DiscoveredAttributes.ToDictionary(x => x, x => 0);
                FeatureFiles.ForEach(f =>
                {
                    GherkinDocument gherkinDocument = new Parser().Parse(f);
                    gherkinDocument.Feature.Children.OfType<StepsContainer>().ForEach(c => {
                            string StepKeyword = string.Empty;
                            c.Steps.ForEach(step => {
  
                                if(step.Keyword.Trim().ToLower() != "and") {
                                    StepKeyword  = step.Keyword.Trim() + "Attribute";
                                }
                                string StepText = step.Text.Trim();
                               
                                DiscoveredAttributes
                                    .Where(attr => attr.Keyword == StepKeyword)
                                    .Where(attr => Regex.IsMatch(StepText, attr.Text))
                                    .ForEach(attr => { BindingsUsage[attr] = BindingsUsage[attr] + 1;})
                               ;
                            });
                        }
                    );
                });
                var UsagePerMethod = BindingsUsage
                    .GroupBy(x => x.Key.Method)
                    .Select(grp => new { Method = grp.Key, Usage = grp.Sum(x => x.Value), Items = grp })
                ;


                var UnusedBindings = UsagePerMethod.Where(x => x.Usage == 0);

                Result = new
                {
                    UnusedBindingMethods = UnusedBindings.Select(x => x.Method.ToString()),
                    UnusedAttributes = BindingsUsage.Where(x => x.Value == 0).Select( x => $"{x.Key.Keyword} {x.Key.Text}")
                };
                return this;

            }
        }

    }
}
