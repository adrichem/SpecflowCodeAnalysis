namespace SpecFlowCodeAnalyzers.CodeFixes
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            get { return ImmutableArray.Create(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicMethod); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var declaration = root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .First()
             ;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => MakePublicAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private bool IsAnAccessibilityKeyword(SyntaxToken child)
        {
            return child.IsKind(SyntaxKind.PrivateKeyword)
                || child.IsKind(SyntaxKind.ProtectedKeyword)
                || child.IsKind(SyntaxKind.InternalKeyword);
        }

        private async Task<Document> MakePublicAsync(Document document
            , MethodDeclarationSyntax methodDecl
            , CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia = methodDecl
                .ReturnType
                .GetLeadingTrivia();

            SyntaxToken publicToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.PublicKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            var newModifiers = methodDecl
                .Modifiers
                .Where(m => !IsAnAccessibilityKeyword(m))
                .Concat(new List<SyntaxToken> { publicToken })
            ;
            MethodDeclarationSyntax newMethodDeclaration = methodDecl.WithModifiers(new SyntaxTokenList(newModifiers));

            SyntaxNode oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot
                .ReplaceNode(methodDecl, newMethodDeclaration);



            return document.WithSyntaxRoot(newRoot);
        }
    }
}
