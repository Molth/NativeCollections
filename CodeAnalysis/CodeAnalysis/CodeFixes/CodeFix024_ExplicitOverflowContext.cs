using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Rewrites the diagnostics of the overflow and numeric conversion rules with the
    ///     wrapping helpers and the unchecked conversion helpers of the analyzed library.
    /// </summary>
    /// <remarks>
    ///     The fix rewrites the whole document instead of replacing a single node, because one expression can
    ///     need several coordinated conversions: an operand has to be converted to the arithmetic type of the
    ///     operation, the result has to be converted back to the type of the assignment target, and a postfix
    ///     increment has to reproduce the value before the update. Every replacement is only made when it is
    ///     provably equivalent, so an expression that cannot be duplicated, that has to stay a compile-time
    ///     constant, that is already written with a helper, or that has no helper available is left untouched.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFix024_ExplicitOverflowContext))]
    [Shared]
    internal sealed class CodeFix024_ExplicitOverflowContext : CodeFixProvider
    {
        /// <summary>
        ///     The identifiers of the diagnostics that this provider fixes.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => [Analyzer024_ExplicitOverflowContext.DIAGNOSTIC_ID, Analyzer025_DefaultUncheckedArithmetic.DIAGNOSTIC_ID, Analyzer028_NumericConversions.DIAGNOSTIC_ID];

        /// <summary>
        ///     Gets the provider that fixes every occurrence of a diagnostic in a single pass.
        /// </summary>
        /// <returns>The fix-all provider backed by the whole-document rewrite.</returns>
        public override FixAllProvider GetFixAllProvider() => FixAllProvider.Create(FixDocumentAsync);

        /// <summary>
        ///     Registers a single code action, because the rewrite is document
        ///     wide and therefore identical for every diagnostic of this provider.
        /// </summary>
        /// <param name="context">The context of the diagnostic the fix is offered for.</param>
        /// <returns>A task that completes when the code action is registered.</returns>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(CodeAction.Create("Use the wrapping and unchecked conversion helpers", cancellationToken => ApplyDocumentFixAsync(context.Document, cancellationToken), nameof(CodeFix024_ExplicitOverflowContext)), context.Diagnostics);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Applies the whole-document rewrite for a fix-all operation.
        /// </summary>
        /// <param name="context">The context of the fix-all operation.</param>
        /// <param name="document">The document that contains the diagnostics.</param>
        /// <param name="diagnostics">
        ///     The diagnostics to fix, which are unused because the rewrite covers the document.
        /// </param>
        /// <returns>The rewritten document, or the original document when it cannot be analyzed.</returns>
        private static async Task<Document?> FixDocumentAsync(FixAllContext context, Document document, ImmutableArray<Diagnostic> diagnostics) => await ApplyDocumentFixAsync(document, context.CancellationToken).ConfigureAwait(false);

        /// <summary>
        ///     Rewrites the document with the wrapping expression rewriter.
        /// </summary>
        /// <param name="document">The document to rewrite.</param>
        /// <param name="cancellationToken">The token that cancels the operation.</param>
        /// <returns>
        ///     The rewritten document, or the original document when
        ///     the syntax tree or the semantic model is unavailable.
        /// </returns>
        private static async Task<Document> ApplyDocumentFixAsync(Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (root == null || semanticModel == null)
                return document;

            var rewrittenRoot = new Rewriter011_WrappingExpression(semanticModel).Visit(root);
            return rewrittenRoot == null ? document : document.WithSyntaxRoot(rewrittenRoot);
        }
    }
}