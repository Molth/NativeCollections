using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Adds braces to the control statements that are excluded from brace removal.
    /// </summary>
    /// <remarks>
    ///     A <see langword="using" />, <see langword="do" />, or <see langword="fixed" />
    ///     statement receives a block body even when it contains a single statement.
    ///     The block is created by <see cref="TriviaHelpers.WrapInBlock" />, so the
    ///     produced indentation and line endings match the ones of the other rewriters.
    /// </remarks>
    internal sealed class Rewriter006_NestedControlBrace : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Adds a block to the body of a using statement.
        /// </summary>
        /// <param name="node">The using statement to rewrite.</param>
        /// <returns>The using statement with a block body.</returns>
        public override SyntaxNode VisitUsingStatement(UsingStatementSyntax node) => AddBody((UsingStatementSyntax)base.VisitUsingStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Adds a block to the body of a do statement.
        /// </summary>
        /// <param name="node">The do statement to rewrite.</param>
        /// <returns>The do statement with a block body.</returns>
        public override SyntaxNode VisitDoStatement(DoStatementSyntax node) => AddBody((DoStatementSyntax)base.VisitDoStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Adds a block to the body of a fixed statement.
        /// </summary>
        /// <param name="node">The fixed statement to rewrite.</param>
        /// <returns>The fixed statement with a block body.</returns>
        public override SyntaxNode VisitFixedStatement(FixedStatementSyntax node) => AddBody((FixedStatementSyntax)base.VisitFixedStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Wraps the body of the node in a block when the body is not already a block.
        /// </summary>
        /// <typeparam name="T">The type of the statement that owns the body.</typeparam>
        /// <param name="node">The rewritten statement.</param>
        /// <param name="get">Gets the current body of the statement.</param>
        /// <param name="set">Creates a copy of the statement with the given body.</param>
        /// <returns>The statement with a block body.</returns>
        private static T AddBody<T>(T node, Func<T, StatementSyntax> get, Func<T, StatementSyntax, T> set) where T : StatementSyntax
        {
            var statement = get(node);
            return statement is not BlockSyntax ? set(node, TriviaHelpers.WrapInBlock(node, statement)) : node;
        }
    }
}