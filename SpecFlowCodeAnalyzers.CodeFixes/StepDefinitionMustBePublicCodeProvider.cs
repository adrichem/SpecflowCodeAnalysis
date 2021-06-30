namespace SpecFlowCodeAnalyzers.CodeFixes
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Rename;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StepDefinitionMustBePublicCodeProvider)), Shared]
    public class StepDefinitionMustBePublicCodeProvider : CodeFixProvider
    {
        private static readonly string Title = "Make public";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(StepDefinitionMustBePublic.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the method declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MakePublicAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> MakePublicAsync(Document document
            , MethodDeclarationSyntax methodDecl
            , CancellationToken cancellationToken)
        {
            var accessModifier = methodDecl
                .ChildNodes()
                .Where(child => child.IsKind(SyntaxKind.PrivateKeyword) || child.IsKind(SyntaxKind.ProtectedKeyword) || child.IsKind(SyntaxKind.InternalKeyword))
                .FirstOrDefault()
            ;
            if (null == accessModifier) return document;

            SyntaxToken token = accessModifier.GetFirstToken();
            var leadingTrivia = accessModifier.GetLeadingTrivia();
            var trailingTrivia = accessModifier.GetTrailingTrivia();

            // Remove the leading trivia from the method declaration.
            var trimmedLocal = methodDecl.ReplaceToken(token, token.WithLeadingTrivia(SyntaxTriviaList.Empty));
            SyntaxToken publicToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.PublicKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            // Insert the public token into the modifiers list, creating a new modifiers list.
            SyntaxTokenList newModifiers = trimmedLocal.Modifiers.Insert(0, publicToken);

            // Produce the new local declaration.
            var newLocal = trimmedLocal.WithModifiers(newModifiers);

            // Add an annotation to format the new local declaration.
            var formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

            // Replace the old local declaration with the new local declaration.
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(methodDecl, formattedLocal);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);


        }
    }
}
