using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Flattens the expressions that are allowed to occupy a single line.
    /// </summary>
    /// <remarks>
    ///     Only the expression kinds that the formatter treats as inline expressions are
    ///     rewritten. Constructs whose layout carries meaning or that cannot be flattened without
    ///     losing readability, such as switch expressions, lambdas, initializers, and query
    ///     expressions, are preserved. Directives inside an expression also prevent the rewrite.
    /// </remarks>
    internal sealed class Rewriter008_ExpressionLine : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Flattens a multi-line assignment expression.
        /// </summary>
        /// <param name="node">The assignment expression to rewrite.</param>
        /// <returns>The flattened assignment expression.</returns>
        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) => Flatten((AssignmentExpressionSyntax)base.VisitAssignmentExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line binary expression.
        /// </summary>
        /// <param name="node">The binary expression to rewrite.</param>
        /// <returns>The flattened binary expression.</returns>
        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) => Flatten((BinaryExpressionSyntax)base.VisitBinaryExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line conditional expression.
        /// </summary>
        /// <param name="node">The conditional expression to rewrite.</param>
        /// <returns>The flattened conditional expression.</returns>
        public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node) => Flatten((ConditionalExpressionSyntax)base.VisitConditionalExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line is-pattern expression.
        /// </summary>
        /// <param name="node">The is-pattern expression to rewrite.</param>
        /// <returns>The flattened is-pattern expression.</returns>
        public override SyntaxNode VisitIsPatternExpression(IsPatternExpressionSyntax node) => Flatten((IsPatternExpressionSyntax)base.VisitIsPatternExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line cast expression.
        /// </summary>
        /// <param name="node">The cast expression to rewrite.</param>
        /// <returns>The flattened cast expression.</returns>
        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node) => Flatten((CastExpressionSyntax)base.VisitCastExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line member access expression.
        /// </summary>
        /// <param name="node">The member access expression to rewrite.</param>
        /// <returns>The flattened member access expression.</returns>
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node) => Flatten((MemberAccessExpressionSyntax)base.VisitMemberAccessExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line element access expression.
        /// </summary>
        /// <param name="node">The element access expression to rewrite.</param>
        /// <returns>The flattened element access expression.</returns>
        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node) => Flatten((ElementAccessExpressionSyntax)base.VisitElementAccessExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line element binding expression.
        /// </summary>
        /// <param name="node">The element binding expression to rewrite.</param>
        /// <returns>The flattened element binding expression.</returns>
        public override SyntaxNode VisitElementBindingExpression(ElementBindingExpressionSyntax node) => Flatten((ElementBindingExpressionSyntax)base.VisitElementBindingExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line range expression.
        /// </summary>
        /// <param name="node">The range expression to rewrite.</param>
        /// <returns>The flattened range expression.</returns>
        public override SyntaxNode VisitRangeExpression(RangeExpressionSyntax node) => Flatten((RangeExpressionSyntax)base.VisitRangeExpression(node)!);

        /// <summary>
        ///     Flattens a multi-line tuple expression.
        /// </summary>
        /// <param name="node">The tuple expression to rewrite.</param>
        /// <returns>The flattened tuple expression.</returns>
        public override SyntaxNode VisitTupleExpression(TupleExpressionSyntax node) => Flatten((TupleExpressionSyntax)base.VisitTupleExpression(node)!);

        /// <summary>
        ///     Flattens the node when it spans several lines and contains no unsupported construct.
        /// </summary>
        /// <typeparam name="T">The type of the expression.</typeparam>
        /// <param name="node">The expression to flatten.</param>
        /// <returns>The flattened expression, or the original expression when it cannot be flattened.</returns>
        /// <remarks>
        ///     The leading and trailing trivia of the node are saved before normalization, because they contain
        ///     the indentation and the comments that belong to the surrounding statement. Keeping them also
        ///     prevents the normalization from moving the expression away from the statement that owns it.
        /// </remarks>
        private static T Flatten<T>(T node) where T : ExpressionSyntax
        {
            if (!HasLineBreak(node) || SyntaxHelpers.ContainsMultilineLayoutConstruct(node) || node.DescendantTrivia(descendIntoTrivia: true).Any(static trivia => trivia.IsDirective))
                return node;

            var leading = node.GetLeadingTrivia();
            var trailing = node.GetTrailingTrivia();
            var result = (T)node.NormalizeWhitespace(" ", " ");
            return (T)result.WithLeadingTrivia(leading).WithTrailingTrivia(trailing);
        }

        /// <summary>
        ///     Determines whether the node spans more than one line.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the node spans several lines; otherwise, <see langword="false" />.
        /// </returns>
        private static bool HasLineBreak(SyntaxNode node)
        {
            var span = node.GetLocation().GetLineSpan();
            return span.StartLinePosition.Line != span.EndLinePosition.Line;
        }
    }
}