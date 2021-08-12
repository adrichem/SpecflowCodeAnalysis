namespace SpecFlowCodeAnalyzers.CodeFixes
{
    using Adrichem.Test.SpecFlowCodeAnalyzers;
    using Adrichem.Test.SpecFlowCodeAnalyzers.Common;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ForbiddenModifiersAnalyzer)), Shared]
    public class ForbiddenModifiersCodeFixProvider : CodeFixProvider
    {
        private static readonly string Title = "Remove 'out' or 'ref' modifiers";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(SpecFlowCodeAnalyzersDiagnosticIds.ForbiddenModifier); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
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
                .OfType<ParameterSyntax>()
                .First()
             ;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => RemoveForbiddenModifiers(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> RemoveForbiddenModifiers(Document document
            , ParameterSyntax param
            , CancellationToken cancellationToken)
        {
            var newModifiers = param
                .Modifiers
                .Where(m => !m.IsKind(SyntaxKind.OutKeyword) && !m.IsKind(SyntaxKind.RefKeyword))
            ;

            ParameterSyntax newParameterDeclaration = param.WithModifiers(new SyntaxTokenList(newModifiers));

             return document.WithSyntaxRoot((await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false))
                .ReplaceNode(param, newParameterDeclaration)
             );
        }
    }
}
