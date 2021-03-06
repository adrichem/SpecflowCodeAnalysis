namespace Adrichem.Test.SpecFlowCodeAnalyzers.CodeFixes.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;

    public static class InsertPublicModifierHelper
    {
        /// <summary>
        /// Inserts the public modifier as 1st modifier ensuring 
        ///  trivia of the previous 1st modifier is moved to the public modifier
        /// </summary>
        /// <param name="decl">The declaration whose modifiers need to change</param>
        /// <returns>a new object of same type as <paramref name="decl"/> whose modifiers contain public as 1st modifier</returns>
        public static ClassDeclarationSyntax InsertPublicModifier(ClassDeclarationSyntax decl) 
        {
            if (null == decl)
                throw new ArgumentNullException(nameof(decl));

            if (!decl.Modifiers.Any()) 
                throw new InvalidOperationException($"{nameof(decl)} must have at least other modifier.");

            SyntaxToken TokenToInsertBefore = decl.Modifiers.First();
            SyntaxToken NewTokenToInsertBefore = TokenToInsertBefore
                .WithoutTrivia()
                .WithTrailingTrivia(TokenToInsertBefore.TrailingTrivia)
            ;

            SyntaxToken publicToken = SyntaxFactory.Token(TokenToInsertBefore.LeadingTrivia
                , SyntaxKind.PublicKeyword
                , SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))
            );

            return decl.WithModifiers(decl
                .Modifiers
                .Replace(TokenToInsertBefore, NewTokenToInsertBefore)
                .Insert(0, publicToken)
            );
           
        }

        /// <inheritdoc cref="InsertPublicModifierHelper.InsertPublicModifier(ClassDeclarationSyntax)"/>
        /// We have to duplicate functionality as in Roslyn 2.9.0 the ClassDeclarationSyntax and MethodDeclarationSyntax classes
        /// dont derive from a shared base types as in Roslyn 3.10. 
        /// We cannot use a generic method with type contrainsts as these classes are sealed.
        public static MethodDeclarationSyntax InsertPublicModifier(MethodDeclarationSyntax decl)
        {
            if (null == decl)
                throw new ArgumentNullException(nameof(decl));

            if (!decl.Modifiers.Any())
                throw new InvalidOperationException($"{nameof(decl)} must have at least other modifier.");

            SyntaxToken TokenToInsertBefore = decl.Modifiers.First();
            SyntaxToken NewTokenToInsertBefore = TokenToInsertBefore
                .WithoutTrivia()
                .WithTrailingTrivia(TokenToInsertBefore.TrailingTrivia)
            ;

            SyntaxToken publicToken = SyntaxFactory.Token(TokenToInsertBefore.LeadingTrivia
                , SyntaxKind.PublicKeyword
                , SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))
            );

            return decl.WithModifiers(decl
                .Modifiers
                .Replace(TokenToInsertBefore, NewTokenToInsertBefore)
                .Insert(0, publicToken)
            );

        }
    }
}
