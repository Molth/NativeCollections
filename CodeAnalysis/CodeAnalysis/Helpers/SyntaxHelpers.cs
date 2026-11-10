using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the syntax predicates and rewrites that the analyzers and the code fix share.
    /// </summary>
    /// <remarks>
    ///     The helpers here work on the syntax tree only, so every caller observes the same
    ///     classification of parentheses and of the lexical overflow context.
    /// </remarks>
    internal static class SyntaxHelpers
    {
        /// <summary>
        ///     Removes any number of redundant parentheses around the given expression.
        /// </summary>
        /// <param name="expression">The expression to unwrap.</param>
        /// <returns>The innermost expression after removing all parenthesized wrappers.</returns>
        public static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
        {
            while (expression is ParenthesizedExpressionSyntax parenthesized)
                expression = parenthesized.Expression;

            return expression;
        }

        /// <summary>
        ///     Determines whether the node is inside a <see langword="checked" /> context.
        /// </summary>
        /// <param name="node">The node whose lexical context is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the nearest overflow context is
        ///     <see langword="checked" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The nearest checked or unchecked ancestor decides, so an explicit <see langword="checked" /> block
        ///     nested inside an <see langword="unchecked" /> block keeps its throwing semantics and is not reported.
        /// </remarks>
        public static bool IsInsideCheckedContext(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                switch (ancestor)
                {
                    case CheckedExpressionSyntax expression:
                        return expression.IsKind(SyntaxKind.CheckedExpression);

                    case CheckedStatementSyntax statement:
                        return statement.IsKind(SyntaxKind.CheckedStatement);
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the node is inside the scope of an
        ///     <see langword="unchecked" /> expression or statement block.
        /// </summary>
        /// <param name="node">The node whose lexical context is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the nearest overflow context is
        ///     <see langword="unchecked" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The nearest checked or unchecked ancestor decides, so an inner <see langword="checked" />
        ///     expression keeps its throwing behavior and is not reported as unchecked.
        /// </remarks>
        public static bool IsInsideUncheckedContext(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                switch (ancestor)
                {
                    case CheckedExpressionSyntax expression:
                        return expression.IsKind(SyntaxKind.UncheckedExpression);

                    case CheckedStatementSyntax statement:
                        return statement.IsKind(SyntaxKind.UncheckedStatement);
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the node contains a construct that must keep its multi-line layout.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the node contains such a construct; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A switch expression, a lambda, an initializer, or a query expression is excluded because
        ///     normalizing it would either erase the layout that makes it readable or produce nested
        ///     inline text that the later rewriters cannot format safely.
        /// </remarks>
        public static bool ContainsMultilineLayoutConstruct(SyntaxNode node) => node.DescendantNodesAndSelf().Any(static descendant => descendant is SwitchExpressionSyntax or AnonymousFunctionExpressionSyntax or InitializerExpressionSyntax or QueryExpressionSyntax);
    }
}