using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Removes the braces around a single-line statement body when the braces do not declare a scope.
    /// </summary>
    /// <remarks>
    ///     A body that spans several lines keeps its braces, because removing them would change the grouping of the
    ///     statements. The <see langword="using" />, <see langword="do" />, and <see langword="fixed" /> statements
    ///     are deliberately not handled here: <see cref="Rewriter006_NestedControlBrace" /> adds braces to them later
    ///     in the pipeline, so removing braces from them would only make the two rewriters undo each other's work.
    /// </remarks>
    internal sealed class Rewriter005_SingleStatementBrace : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Removes the braces from the branches of an if statement when neither branch needs a block.
        /// </summary>
        /// <param name="node">The if statement to rewrite.</param>
        /// <returns>The if statement with the removable braces removed or with the required braces added.</returns>
        /// <remarks>
        ///     When any branch contains a multi-line statement the whole chain keeps or
        ///     receives braces, so an <c>else if</c> chain is always formatted consistently.
        /// </remarks>
        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            var chainNeedsBraces = ChainNeedsBraces(node);
            var result = (IfStatementSyntax)base.VisitIfStatement(node)!;
            if (chainNeedsBraces)
                return EnsureChainBraces(result);

            if (result.Statement is BlockSyntax statementBlock && CanUnbrace(statementBlock))
                result = result.WithStatement(Unbrace(statementBlock));

            if (result.Else is not null && result.Else.Statement is BlockSyntax elseBlock && CanUnbrace(elseBlock))
                result = result.WithElse(result.Else.WithStatement(Unbrace(elseBlock)));

            return result;
        }

        /// <summary>
        ///     Removes the braces from the body of a for statement when the body is a single-line statement.
        /// </summary>
        /// <param name="node">The for statement to rewrite.</param>
        /// <returns>The for statement with removable braces removed.</returns>
        public override SyntaxNode VisitForStatement(ForStatementSyntax node) => RemoveSimpleBlock((ForStatementSyntax)base.VisitForStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Removes the braces from the body of a foreach statement when the body is a single-line statement.
        /// </summary>
        /// <param name="node">The foreach statement to rewrite.</param>
        /// <returns>The foreach statement with removable braces removed.</returns>
        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node) => RemoveSimpleBlock((ForEachStatementSyntax)base.VisitForEachStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Removes the braces from the body of a foreach statement
        ///     with a variable when the body is a single-line statement.
        /// </summary>
        /// <param name="node">The foreach statement to rewrite.</param>
        /// <returns>The foreach statement with removable braces removed.</returns>
        public override SyntaxNode VisitForEachVariableStatement(ForEachVariableStatementSyntax node) => RemoveSimpleBlock((ForEachVariableStatementSyntax)base.VisitForEachVariableStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Removes the braces from the body of a while statement when the body is a single-line statement.
        /// </summary>
        /// <param name="node">The while statement to rewrite.</param>
        /// <returns>The while statement with removable braces removed.</returns>
        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node) => RemoveSimpleBlock((WhileStatementSyntax)base.VisitWhileStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Removes the braces from the body of a lock statement when the body is a single-line statement.
        /// </summary>
        /// <param name="node">The lock statement to rewrite.</param>
        /// <returns>The lock statement with removable braces removed.</returns>
        public override SyntaxNode VisitLockStatement(LockStatementSyntax node) => RemoveSimpleBlock((LockStatementSyntax)base.VisitLockStatement(node)!, static value => value.Statement, static (value, statement) => value.WithStatement(statement));

        /// <summary>
        ///     Replaces the body of the node with the unwrapped statement when the block can be removed.
        /// </summary>
        /// <typeparam name="T">The type of the statement that owns the body.</typeparam>
        /// <param name="node">The rewritten statement.</param>
        /// <param name="get">Gets the current body of the statement.</param>
        /// <param name="set">Creates a copy of the statement with the given body.</param>
        /// <returns>
        ///     The statement with the braces removed, or the original statement when the block has to stay.
        /// </returns>
        private static T RemoveSimpleBlock<T>(T node, Func<T, StatementSyntax> get, Func<T, StatementSyntax, T> set) where T : StatementSyntax
        {
            var statement = get(node);
            return statement is BlockSyntax block && CanUnbrace(block) ? set(node, Unbrace(block)) : node;
        }

        /// <summary>
        ///     Determines whether the block can lose its braces.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the block contains a single
        ///     line statement; otherwise, <see langword="false" />.
        /// </returns>
        private static bool CanUnbrace(BlockSyntax block) => !BlockNeedsBraces(block);

        /// <summary>
        ///     Determines whether the block has to keep its braces.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the braces are required; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A block with anything other than one statement cannot be unwrapped. A single
        ///     statement that spans more than one line also keeps the block, because an
        ///     unbraced multi-line body would attach only its first line to the owner.
        /// </remarks>
        private static bool BlockNeedsBraces(BlockSyntax block)
        {
            if (block.Statements.Count != 1)
                return true;

            var statement = block.Statements[0];
            var lineSpan = statement.GetLocation().GetLineSpan();
            return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
        }

        /// <summary>
        ///     Removes the braces from a block that contains a single statement.
        /// </summary>
        /// <param name="block">The block to unwrap.</param>
        /// <returns>The statement that the block contained.</returns>
        /// <remarks>
        ///     The indentation that belonged to the opening brace is transferred to the statement, and the
        ///     trivia of the closing brace is transferred to the trailing trivia of the statement. Blank lines
        ///     left by the removed braces are collapsed so that the surrounding code keeps its original spacing.
        /// </remarks>
        private static StatementSyntax Unbrace(BlockSyntax block)
        {
            var statement = block.Statements[0];
            var leading = statement.GetLeadingTrivia();
            if (!block.OpenBraceToken.GetPreviousToken().TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                leading = block.OpenBraceToken.TrailingTrivia.AddRange(leading);

            leading = TriviaHelpers.CollapseBlankLines(leading);
            var trailing = TriviaHelpers.CollapseBlankLines(statement.GetTrailingTrivia().AddRange(block.CloseBraceToken.LeadingTrivia).AddRange(block.CloseBraceToken.TrailingTrivia));
            return statement.WithLeadingTrivia(leading).WithTrailingTrivia(trailing);
        }

        /// <summary>
        ///     Determines whether the branches of an if statement need braces.
        /// </summary>
        /// <param name="node">The if statement to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when any branch needs braces; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The else branch is inspected recursively, so a multi-line branch
        ///     anywhere in an <c>else if</c> chain keeps the whole chain braced.
        /// </remarks>
        private static bool ChainNeedsBraces(IfStatementSyntax node)
        {
            if (BranchNeedsBraces(node.Statement))
                return true;

            return node.Else?.Statement switch
            {
                null => false,
                IfStatementSyntax nested => ChainNeedsBraces(nested),
                var statement => BranchNeedsBraces(statement)
            };
        }

        /// <summary>
        ///     Determines whether a branch is a block that has to keep its braces.
        /// </summary>
        /// <param name="statement">The branch to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the branch is a block with a
        ///     multi-line body; otherwise, <see langword="false" />.
        /// </returns>
        private static bool BranchNeedsBraces(StatementSyntax statement) => statement is BlockSyntax block && BlockNeedsBraces(block);

        /// <summary>
        ///     Adds the braces that a branch of an if chain requires.
        /// </summary>
        /// <param name="node">The if statement to rewrite.</param>
        /// <param name="ownerIndent">The indentation of the owner; derived from the statement when omitted.</param>
        /// <returns>The if statement whose branches all have the required braces.</returns>
        /// <remarks>
        ///     The indentation of the outermost if statement is passed down the chain, so every branch uses
        ///     the same owner indentation even when the branches were originally written on different columns.
        /// </remarks>
        private static IfStatementSyntax EnsureChainBraces(IfStatementSyntax node, string? ownerIndent = null)
        {
            ownerIndent ??= TriviaHelpers.GetIndentation(node.GetFirstToken().LeadingTrivia);
            var result = node.Statement is BlockSyntax ? node : node.WithStatement(TriviaHelpers.WrapInBlock(node, node.Statement, ownerIndent));
            if (result.Else is null)
                return result;

            if (result.Else.Statement is IfStatementSyntax nested)
                return result.WithElse(result.Else.WithStatement(EnsureChainBraces(nested, ownerIndent)));

            return result.Else.Statement is BlockSyntax ? result : result.WithElse(result.Else.WithStatement(TriviaHelpers.WrapInBlock(result, result.Else.Statement, ownerIndent)));
        }
    }
}