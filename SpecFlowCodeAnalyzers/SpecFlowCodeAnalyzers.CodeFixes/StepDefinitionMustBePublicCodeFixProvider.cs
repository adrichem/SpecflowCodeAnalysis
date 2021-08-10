namespace SpecFlowCodeAnalyzers.CodeFixes
{
    using Adrichem.Test.SpecFlowCodeAnalyzers.CodeFixes.Helpers;
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

        private readonly ImmutableList<SyntaxKind> ReplaceAbleAccessModifiers = new List<SyntaxKind>
        {
            SyntaxKind.InternalKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword
        }.ToImmutableList();

        private async Task<Document> MakePublicAsync(Document document
            , MethodDeclarationSyntax methodDecl
            , CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax newMethodDecl;
            bool hasReplaceableAccessModifiers = methodDecl
                .Modifiers
                .Where(m => ReplaceAbleAccessModifiers.Any(r => r == m.Kind()))
                .Any()
            ;

            if (hasReplaceableAccessModifiers)
            {
                newMethodDecl = ReplaceAccessModifiersHelper.ReplaceModifiers(methodDecl
                    , SyntaxKind.PublicKeyword
                    , ReplaceAbleAccessModifiers);
            }
            else if (methodDecl.Modifiers.Any())
            {
                newMethodDecl = InsertPublicModifierHelper.InsertPublicModifier(methodDecl) as MethodDeclarationSyntax;
            }
            else
            {
                //Insert a 'public' token as the only modifier
                //The leading trivia of return type needs to move to the new 'public' token
                SyntaxToken publicToken = SyntaxFactory.Token(methodDecl.ReturnType.GetLeadingTrivia()
                    , SyntaxKind.PublicKeyword
                    , SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))
                );

                newMethodDecl = methodDecl
                    .WithReturnType(methodDecl.ReturnType.WithoutTrivia().WithTrailingTrivia(methodDecl.ReturnType.GetTrailingTrivia()))
                    .WithModifiers(methodDecl.Modifiers.Insert(0, publicToken))
                ;
            }

            return document.WithSyntaxRoot((await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false)).ReplaceNode(methodDecl, newMethodDecl));
        }
     
    }
}
