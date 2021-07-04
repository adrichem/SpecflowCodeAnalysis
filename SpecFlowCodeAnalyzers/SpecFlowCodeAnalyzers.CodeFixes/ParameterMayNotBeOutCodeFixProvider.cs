﻿namespace SpecFlowCodeAnalyzers.CodeFixes
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
    public class ParameterMayNotBeOutCodeFixProvider : CodeFixProvider
    {
        private static readonly string Title = "Remove 'out' keyword";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(SpecFlowCodeAnalyzersDiagnosticIds.NoOutParameters); }
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
                    createChangedDocument: c => RemoveOutAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private async Task<Document> RemoveOutAsync(Document document
            , ParameterSyntax param
            , CancellationToken cancellationToken)
        {
            var newModifiers = param
                .Modifiers
                .Where(m => !m.IsKind(SyntaxKind.OutKeyword))
            ;

            ParameterSyntax newParameterDeclaration = param.WithModifiers(new SyntaxTokenList(newModifiers));

            SyntaxNode oldRoot = await document
                .GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);
            return document.WithSyntaxRoot(oldRoot.ReplaceNode(param, newParameterDeclaration));
        }
    }
}
