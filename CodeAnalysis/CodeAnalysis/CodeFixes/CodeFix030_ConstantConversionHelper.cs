using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Replaces a constant conversion helper with the suffixed literal of its target type.
    /// </summary>
    /// <remarks>
    ///     The fix is only offered for the diagnostics of <see cref="Analyzer030_ConstantConversionHelper" />,
    ///     whose argument is a numeric literal. The literal keeps the text of the original number, so a
    ///     decimal, hexadecimal or binary spelling is preserved and only the type suffix is added.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFix030_ConstantConversionHelper))]
    [Shared]
    internal sealed class CodeFix030_ConstantConversionHelper : CodeFixProvider
    {
        /// <summary>
        ///     The identifiers of the diagnostics that this provider fixes.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => [Analyzer030_ConstantConversionHelper.DIAGNOSTIC_ID];

        /// <summary>
        ///     Gets the provider that fixes every occurrence of a diagnostic in a single pass.
        /// </summary>
        /// <returns>The fix-all provider backed by the batch fixer.</returns>
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        ///     Registers the literal suffix code action.
        /// </summary>
        /// <param name="context">The context of the diagnostic the fix is offered for.</param>
        /// <returns>A task that completes when the code action is registered.</returns>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(CodeAction.Create("Use a suffixed literal", cancellationToken => ApplyFixAsync(context.Document, context.Span, cancellationToken), nameof(CodeFix030_ConstantConversionHelper)), context.Diagnostics);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Replaces the helper call with the suffixed literal.
        /// </summary>
        /// <param name="document">The document that contains the invocation.</param>
        /// <param name="span">The span of the reported invocation.</param>
        /// <param name="cancellationToken">The token that cancels the operation.</param>
        /// <returns>The rewritten document, or the original document when the fix cannot be applied.</returns>
        private static async Task<Document> ApplyFixAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (root?.FindNode(span) is not InvocationExpressionSyntax invocation || semanticModel == null || !LiteralFacts.TryGetReplacement(invocation, semanticModel, out var replacement))
                return document;

            return document.WithSyntaxRoot(root.ReplaceNode(invocation, replacement.WithTriviaFrom(invocation)));
        }
    }
}