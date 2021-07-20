namespace SpecFlowCodeAnalyzers.CodeFixes
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassMustBePublicCodeFixProvider)), Shared]
    public class ClassMustBePublicCodeFixProvider : CodeFixProvider
    {
        private static readonly string Title = "Make class public";
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicClass); 
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .First()
            ;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MakePublicAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private readonly Func<SyntaxToken, bool> IsReplaceableAccessModifier = (m) => m.IsKind(SyntaxKind.ProtectedKeyword)
                    || m.IsKind(SyntaxKind.PrivateKeyword)
                    || m.IsKind(SyntaxKind.InternalKeyword)
        ;


        private async Task<Document> MakePublicAsync(Document document
            , ClassDeclarationSyntax classDecl
            , CancellationToken cancellationToken)
        {

            var accessModifiers = classDecl
                .Modifiers
                .Where(m => IsReplaceableAccessModifier(m))
            ;

            if (accessModifiers.Any())
            {
                return await ReplaceAccessModifiers(document
                    , classDecl
                    , cancellationToken
                    , accessModifiers)
                ;
            }

            return await InsertPublicModifier(document
                , classDecl
                , cancellationToken)
            ;
        }


        private async Task<Document> ReplaceAccessModifiers(Document document
            , ClassDeclarationSyntax classDecl
            , CancellationToken cancellationToken
            , IEnumerable<SyntaxToken> tokensToReplace)
        {
            //The public keyword replaces 1 or more access modifiers
            //Ensure it has leading trivia if first replaced modifier
            //and trailing trivia of last replaced modifier
            SyntaxToken publicToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(tokensToReplace.First().LeadingTrivia)
                   , SyntaxKind.PublicKeyword
                   , SyntaxFactory.TriviaList(tokensToReplace.Last().TrailingTrivia)
            );

            //Get the index of the 1st removeable token
            int index = classDecl
                .Modifiers
                .Where(m => IsReplaceableAccessModifier(m))
                .Select(m => classDecl.Modifiers.IndexOf(m))
                .OrderBy(i => i)
                .First()
            ;

            var newModifiers = classDecl
                .Modifiers
                .Replace(tokensToReplace.First(), publicToken)
            ;

            //now remove each remaining access modifiers
            foreach (var tokenToRemove in tokensToReplace.Skip(1))
            {
                newModifiers = newModifiers.Remove(newModifiers.Single(m => m.Kind() == tokenToRemove.Kind()));
            }

            var newClassDecl = classDecl.WithModifiers(new SyntaxTokenList(newModifiers));

            SyntaxNode oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            return document.WithSyntaxRoot(oldRoot.ReplaceNode(classDecl, newClassDecl));
        }

        private async Task<Document> InsertPublicModifier(Document document
            , ClassDeclarationSyntax classDecl
            , CancellationToken cancellationToken)
        {
            //insert public modifier behind the static keyword (if any) 
            int index = classDecl
                .Modifiers
                .Where(m => m.IsKind(SyntaxKind.StaticKeyword))
                .Select(staticKeyWord => classDecl.Modifiers.IndexOf(staticKeyWord) + 1)
                .FirstOrDefault()
            ;

            SyntaxToken publicToken;

            if (index != 0 )
            {
                //the new keyword is inserted directly after the static keyword
                //there's no need to give it leading/trailing whitespace
                publicToken = SyntaxFactory.Token(SyntaxFactory.TriviaList()
                     , SyntaxKind.PublicKeyword
                     , SyntaxFactory.TriviaList()
                );
            }
            else
            {
                //the public keyword is inserted at beginning of the class declaration
                //We need to ensure its leading trivia is taken from the 1st token of the class declaration
                publicToken = SyntaxFactory.Token(classDecl.ChildTokens().First().LeadingTrivia
                     , SyntaxKind.PublicKeyword
                     , SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker)
                );
            }
           

            var newClassDecl = classDecl
                .WithModifiers(new SyntaxTokenList(classDecl
                    .Modifiers
                    .Insert(index, publicToken)));

            SyntaxNode oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false)
            ;

            return document.WithSyntaxRoot(oldRoot.ReplaceNode(classDecl, newClassDecl));
        }

    }
}
