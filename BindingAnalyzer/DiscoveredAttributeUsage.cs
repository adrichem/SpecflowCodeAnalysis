namespace BindingAnalyzer
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    
    /// <summary>
    /// Represents a Given/When/Then found on a method
    /// </summary>
    public class DiscoveredAttributeUsage
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
        /// The Gherkin keyword of this attribute (GivenAttribute/WhenAttribute/ThenAttribute)
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// The string provided to the constructor of the attribute
        /// </summary>
        public string Text { get; set; }
    }
}
