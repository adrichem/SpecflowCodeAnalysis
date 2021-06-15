namespace BindingAnalyzer
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MoreLinq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Find all instances of where one of the Attributes are applied on methods
    /// </summary>
    public class AttributeUsageFinder : CSharpSyntaxWalker
    {
        /// <summary>
        /// Which attributes to search for.
        /// </summary>
        public IEnumerable<INamedTypeSymbol> Attributes { get; set; }

        /// <summary>
        /// Called when an attribute is found on a method.
        /// </summary>
        public Action<MethodDeclarationSyntax, IMethodSymbol, AttributeData> ReportAttr { get; set; }

        /// <summary>
        /// The semantic model of the <see cref="Compilation"/>
        /// </summary>
        public SemanticModel SemanticModel { get; set; }
        
        public override void VisitMethodDeclaration(MethodDeclarationSyntax MethodSyntax)
        {
            if (null == SemanticModel) throw new InvalidOperationException($"{nameof(SemanticModel)} is null");
            if (null == Attributes) throw new InvalidOperationException($"{nameof(Attributes)} is null");
            if (null == ReportAttr) throw new InvalidOperationException($"{nameof(ReportAttr)} is null");

            IMethodSymbol MethodSymbol = SemanticModel.GetDeclaredSymbol(MethodSyntax);
            MethodSymbol
                .GetAttributes()
                .Where(a => Attributes.Any(x => SymbolEqualityComparer.Default.Equals(x, a.AttributeClass)))
                .ForEach(attr => ReportAttr(MethodSyntax, MethodSymbol, attr)) 
            ;
            base.VisitMethodDeclaration(MethodSyntax);
        }
    }
}
