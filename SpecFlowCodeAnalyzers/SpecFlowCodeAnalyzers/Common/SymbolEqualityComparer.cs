namespace Adrichem.Test.SpecFlowCodeAnalyzers.Common
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Roslyn version 2.9.0 does not have the Microsoft.CodeAnalysis.SymbolEqualityComparer class
    /// This is our own implementation.
    /// </summary>
    internal class SymbolEqualityComparer
    {
        public static SymbolEqualityComparer Default => new SymbolEqualityComparer();

        public bool Equals(ISymbol x, ISymbol y)
        {
            return x is null ? y is null : x.GetHashCode() == y.GetHashCode();
        }
    }
}
