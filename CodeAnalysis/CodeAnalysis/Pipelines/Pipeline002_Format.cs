using Microsoft.CodeAnalysis;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Runs the rewriters that produce the community source style.
    /// </summary>
    /// <remarks>
    ///     The order is significant. Attributes are restored before any header is flattened so that the declaration
    ///     keeps its own line. Body comments are removed before a body can be converted into an expression body,
    ///     and using declarations are converted before braces are removed, because the conversion creates the
    ///     block that the brace rewriter expects. The expression body, brace, and single-line rewriters then
    ///     operate on the already cleaned tree, and the final expression line rewriter flattens the remaining
    ///     inline expressions. The numeric suffix of a rewriter mirrors the position of its step: the rewriters are
    ///     applied in ascending order, with the end-of-line step deferred to the end, so a step keeps its ordinal
    ///     when another step is inserted or removed around it. This type composes the steps instead of being one
    ///     of them, so it lives in its own <c>Pipelines</c> folder rather than in the <c>Rewriters</c> folder,
    ///     which holds only the rewriters themselves. The literal suffix rewriter runs after the code has been
    ///     restructured, because it changes the text of a token in place. The end-of-line rewriter runs last,
    ///     because the earlier rewriters create trivia from the line ending of the source and the normalization
    ///     must also cover what they inserted.
    /// </remarks>
    internal static class Pipeline002_Format
    {
        /// <summary>
        ///     Applies the community style rewriters to a syntax tree.
        /// </summary>
        /// <param name="root">The root of the syntax tree to rewrite.</param>
        /// <returns>The rewritten root.</returns>
        public static SyntaxNode Rewrite(SyntaxNode root)
        {
            var result = new Rewriter001_AttributeLineRestorer().Visit(root);
            result = new Rewriter002_BodyCommentRemover().Visit(result);
            result = new Rewriter003_UsingDeclaration().Visit(result);
            result = new Rewriter004_ExpressionBody().Visit(result);
            result = new Rewriter005_SingleStatementBrace().Visit(result);
            result = new Rewriter006_NestedControlBrace().Visit(result);
            result = new Rewriter007_SingleLine().Visit(result);
            result = new Rewriter008_ExpressionLine().Visit(result);
            result = new Rewriter009_BlankLine().Visit(result);
            result = new Rewriter012_SuffixedLiteral().Visit(result);
            result = new Rewriter010_EndOfLine().Visit(result);
            return result ?? root;
        }
    }
}