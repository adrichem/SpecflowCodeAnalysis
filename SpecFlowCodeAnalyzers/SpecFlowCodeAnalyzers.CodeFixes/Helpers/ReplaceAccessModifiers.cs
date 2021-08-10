namespace Adrichem.Test.SpecFlowCodeAnalyzers.CodeFixes.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Generic;
    using System.Linq;

    public static class ReplaceAccessModifiersHelper
    {
        public static ClassDeclarationSyntax ReplaceModifiers(ClassDeclarationSyntax decl
            , SyntaxKind WhatToInsert
            , IEnumerable<SyntaxKind> kindsToReplace)
        {
            return DoReplaceModifiers(decl, WhatToInsert, kindsToReplace) as ClassDeclarationSyntax;
        }

        public static MethodDeclarationSyntax ReplaceModifiers(MethodDeclarationSyntax decl
            , SyntaxKind WhatToInsert
            , IEnumerable<SyntaxKind> kindsToReplace)
        {
            return DoReplaceModifiers(decl, WhatToInsert, kindsToReplace) as MethodDeclarationSyntax;
        }
        private static MemberDeclarationSyntax DoReplaceModifiers(MemberDeclarationSyntax decl
            , SyntaxKind WhatToInsert
            , IEnumerable<SyntaxKind> kindsToReplace)
        {
            var tokensToReplace = decl
                .Modifiers
                .Where(m => kindsToReplace.Any(kind => kind == m.Kind()))
            ;

            //The new modifier replaces 1 or more modifiers
            //Ensure it has leading trivia of first replaced modifier
            //and trailing trivia of last replaced modifier
            SyntaxToken publicToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(tokensToReplace.First().LeadingTrivia)
                , WhatToInsert
                , SyntaxFactory.TriviaList(tokensToReplace.Last().TrailingTrivia)
            );

            var newModifiers = decl
                .Modifiers
                .Replace(tokensToReplace.First(), publicToken)
            ;

            //now remove each remaining modifiers
            foreach (var tokenToRemove in tokensToReplace.Skip(1))
            {
                newModifiers = newModifiers.Remove(newModifiers.Single(m => m.Kind() == tokenToRemove.Kind()));
            }

            return decl.WithModifiers(new SyntaxTokenList(newModifiers));
        }

    }
}
