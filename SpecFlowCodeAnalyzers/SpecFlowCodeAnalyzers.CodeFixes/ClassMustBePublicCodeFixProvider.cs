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


        private readonly ImmutableList<SyntaxKind> ReplaceAbleAccessModifiers = new List<SyntaxKind>
        {
            SyntaxKind.InternalKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword
        }
        .ToImmutableList();

        private async Task<Document> MakePublicAsync(Document document
            , ClassDeclarationSyntax classDecl
            , CancellationToken cancellationToken)
        {
            ClassDeclarationSyntax newClassDecl;
            bool hasReplaceableAccessModifiers = classDecl
                .Modifiers
                .Where(m => ReplaceAbleAccessModifiers.Any(r => r == m.Kind()))
                .Any()
            ;

            if (hasReplaceableAccessModifiers)
            {
                newClassDecl = ReplaceAccessModifiersHelper.ReplaceModifiers(classDecl
                     , SyntaxKind.PublicKeyword
                     , ReplaceAbleAccessModifiers);
            }
            else if (classDecl.Modifiers.Any())
            {
                newClassDecl = InsertPublicModifierHelper.InsertPublicModifier(classDecl) as ClassDeclarationSyntax;
            }
            else
            {
                //Insert a 'public' token as the only modifier
                //The leading trivia of 'class' token needs to move to the new 'public' token
                SyntaxToken classToken = classDecl.ChildTokens().Single(t => t.IsKind(SyntaxKind.ClassKeyword));

                SyntaxToken publicToken = SyntaxFactory.Token(classToken.LeadingTrivia
                    , SyntaxKind.PublicKeyword
                    , SyntaxFactory.TriviaList(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "))
                );

                newClassDecl = classDecl
                    .WithKeyword(classToken.WithoutTrivia().WithTrailingTrivia(classToken.TrailingTrivia))
                    .WithModifiers(classDecl.Modifiers.Insert(0, publicToken))
                ;
            }
            return document.WithSyntaxRoot((await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false)).ReplaceNode(classDecl, newClassDecl));
        }
    }
}
