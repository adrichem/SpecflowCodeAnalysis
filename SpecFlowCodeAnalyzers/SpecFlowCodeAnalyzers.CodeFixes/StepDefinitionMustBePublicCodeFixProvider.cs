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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StepDefinitionMustBePublicCodeFixProvider)), Shared]
    public class StepDefinitionMustBePublicCodeFixProvider : CodeFixProvider
    {
        private static readonly string Title = "Make method public";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(SpecFlowCodeAnalyzersDiagnosticIds.MustBePublicMethod); }
        }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
        
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

        private async Task<Document> MakePublicAsync(Document document
            , MethodDeclarationSyntax methodDecl
            , CancellationToken cancellationToken)
        {
            SyntaxTriviaList leadingTrivia = methodDecl
                .ReturnType
                .GetLeadingTrivia();

            SyntaxToken publicToken = SyntaxFactory.Token(leadingTrivia
                , SyntaxKind.PublicKeyword
                , SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

            var newModifiers = methodDecl
                .Modifiers
                .Where(m => !m.IsKind(SyntaxKind.PrivateKeyword))
                .Where(m => !m.IsKind(SyntaxKind.ProtectedKeyword))
                .Where(m => !m.IsKind(SyntaxKind.InternalKeyword))
                .Concat(new List<SyntaxToken> { publicToken })
            ;
            MethodDeclarationSyntax newMethodDeclaration = methodDecl.WithModifiers(new SyntaxTokenList(newModifiers));

            SyntaxNode oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            return document.WithSyntaxRoot(oldRoot.ReplaceNode(methodDecl, newMethodDeclaration));
        }
    }
}
