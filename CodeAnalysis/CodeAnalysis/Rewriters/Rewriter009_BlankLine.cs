using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Inserts the blank lines used to separate early exits, control-flow
    ///     blocks, switch sections, and the final exit of a <c>while (true)</c> loop.
    /// </summary>
    /// <remarks>
    ///     The rewriter changes trivia only. It never changes the statement structure, and it inserts at
    ///     most one blank line at a boundary even when more than one rule selects the same boundary. A
    ///     directive or an existing blank line immediately before the target statement is preserved as-is.
    /// </remarks>
    internal sealed class Rewriter009_BlankLine : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Rewrites the complete compilation unit after all structural rewriters have run.
        /// </summary>
        /// <param name="node">The compilation unit to rewrite.</param>
        /// <returns>The compilation unit with the requested blank lines.</returns>
        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var result = (CompilationUnitSyntax)base.VisitCompilationUnit(node)!;
            var source = SourceText.From(result.ToFullString());
            var targets = new HashSet<int>();
            var lineEnding = TriviaHelpers.GetLineEnding(result.ToFullString());

            AddEarlyReturnTargets(result, source, targets);
            AddSwitchSectionTargets(result, source, targets);
            AddControlFlowTargets(result, source, targets);
            AddWhileExitTargets(result, source, targets);

            if (targets.Count == 0)
                return result;

            var tokens = result.DescendantTokens().Where(token => targets.Contains(token.SpanStart)).ToArray();
            return result.ReplaceTokens(tokens, (_, token) => InsertBlankLine(token, lineEnding));
        }

        /// <summary>
        ///     Selects the statement after each return that has a following sibling in the same statement list.
        /// </summary>
        /// <param name="root">The root to inspect.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddEarlyReturnTargets(SyntaxNode root, SourceText source, ISet<int> targets)
        {
            foreach (var returnStatement in root.DescendantNodes().OfType<ReturnStatementSyntax>())
            {
                if (TryGetFollowingSibling(returnStatement, out var nextStatement))
                    AddTarget(nextStatement, returnStatement, source, targets);
            }
        }

        /// <summary>
        ///     Selects the first statement of every switch section after a non-empty preceding section.
        /// </summary>
        /// <param name="root">The root to inspect.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddSwitchSectionTargets(SyntaxNode root, SourceText source, ISet<int> targets)
        {
            foreach (var switchStatement in root.DescendantNodes().OfType<SwitchStatementSyntax>())
            {
                for (var index = 1; index < switchStatement.Sections.Count; ++index)
                {
                    var previous = switchStatement.Sections[index - 1];
                    var current = switchStatement.Sections[index];
                    if (previous.Statements.Count > 0 && current.Statements.Count > 0)
                        AddTarget(current.GetFirstToken(), previous.GetLastToken(), source, targets);
                }
            }
        }

        /// <summary>
        ///     Selects the statement after a control-flow block when another statement follows it in the same block.
        /// </summary>
        /// <param name="root">The root to inspect.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddControlFlowTargets(SyntaxNode root, SourceText source, ISet<int> targets)
        {
            foreach (var block in root.DescendantNodes().OfType<BlockSyntax>())
            {
                for (var index = 0; index + 1 < block.Statements.Count; ++index)
                {
                    var previous = block.Statements[index];
                    if (IsControlFlowStatement(previous))
                        AddTarget(block.Statements[index + 1], previous, source, targets);
                }
            }
        }

        /// <summary>
        ///     Selects the statement immediately before the final exit of every <c>while (true)</c> block.
        /// </summary>
        /// <param name="root">The root to inspect.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddWhileExitTargets(SyntaxNode root, SourceText source, ISet<int> targets)
        {
            foreach (var whileStatement in root.DescendantNodes().OfType<WhileStatementSyntax>())
            {
                if (!IsTrueExpression(whileStatement.Condition) || whileStatement.Statement is not BlockSyntax block || block.Statements.Count < 2)
                    continue;

                var exit = block.Statements[block.Statements.Count - 1];
                if (IsExitStatement(exit))
                    AddTarget(exit, block.Statements[block.Statements.Count - 2], source, targets);
            }
        }

        /// <summary>
        ///     Adds a target when the boundary has no blank line and is not immediately preceded by a directive.
        /// </summary>
        /// <param name="next">The statement before which the blank line is inserted.</param>
        /// <param name="previous">The statement on the preceding side of the boundary.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddTarget(StatementSyntax next, StatementSyntax previous, SourceText source, ISet<int> targets) => AddTarget(next.GetFirstToken(), previous.GetLastToken(), source, targets);

        /// <summary>
        ///     Adds a token to the target set when its line follows the preceding token without an existing blank line.
        /// </summary>
        /// <param name="nextToken">The token before which the blank line is inserted.</param>
        /// <param name="previousToken">The token on the preceding side of the boundary.</param>
        /// <param name="source">The source text of the root.</param>
        /// <param name="targets">The target token positions.</param>
        private static void AddTarget(SyntaxToken nextToken, SyntaxToken previousToken, SourceText source, ISet<int> targets)
        {
            var previousLine = source.Lines.GetLineFromPosition(Math.Min(previousToken.Span.End, source.Length));
            var nextLine = source.Lines.GetLineFromPosition(Math.Min(nextToken.SpanStart, source.Length));
            if (nextLine.LineNumber <= previousLine.LineNumber || nextLine.LineNumber == 0)
                return;

            var lineBeforeNext = source.Lines[nextLine.LineNumber - 1].ToString();
            if (string.IsNullOrWhiteSpace(lineBeforeNext) || lineBeforeNext.TrimStart().StartsWith("#", StringComparison.Ordinal))
                return;

            targets.Add(nextToken.SpanStart);
        }

        /// <summary>
        ///     Inserts one line ending after the last existing line ending in the target token's leading trivia.
        /// </summary>
        /// <param name="token">The token that starts the selected statement.</param>
        /// <param name="lineEnding">The line ending used by the source.</param>
        /// <returns>The token with one additional blank line.</returns>
        private static SyntaxToken InsertBlankLine(SyntaxToken token, string lineEnding)
        {
            var leading = token.LeadingTrivia;
            var lastLineBreak = -1;
            for (var index = 0; index < leading.Count; ++index)
            {
                if (leading[index].IsKind(SyntaxKind.EndOfLineTrivia))
                    lastLineBreak = index;
            }

            var insertion = lastLineBreak + 1;
            return token.WithLeadingTrivia(leading.Insert(insertion, SyntaxFactory.EndOfLine(lineEnding)));
        }

        /// <summary>
        ///     Finds a following sibling in a block or switch section.
        /// </summary>
        /// <param name="returnStatement">The return statement to inspect.</param>
        /// <param name="nextStatement">The following sibling, when one exists.</param>
        /// <returns><see langword="true" /> when a sibling was found; otherwise, <see langword="false" />.</returns>
        private static bool TryGetFollowingSibling(ReturnStatementSyntax returnStatement, out StatementSyntax nextStatement)
        {
            if (returnStatement.Parent is BlockSyntax block && block.Statements.IndexOf(returnStatement) is var blockIndex && blockIndex >= 0 && blockIndex + 1 < block.Statements.Count)
            {
                nextStatement = block.Statements[blockIndex + 1];
                return true;
            }

            if (returnStatement.Parent is SwitchSectionSyntax section && section.Statements.IndexOf(returnStatement) is var sectionIndex && sectionIndex >= 0 && sectionIndex + 1 < section.Statements.Count)
            {
                nextStatement = section.Statements[sectionIndex + 1];
                return true;
            }

            nextStatement = null!;
            return false;
        }

        /// <summary>
        ///     Determines whether the statement is one of the structural control-flow
        ///     statements that separates the surrounding block into visual sections.
        /// </summary>
        /// <param name="statement">The statement to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the statement is structural; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsControlFlowStatement(StatementSyntax statement) => statement is IfStatementSyntax or SwitchStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax or UsingStatementSyntax or FixedStatementSyntax or LockStatementSyntax or TryStatementSyntax or CheckedStatementSyntax or UnsafeStatementSyntax;

        /// <summary>
        ///     Determines whether an expression is the literal <see langword="true" />.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is the literal; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsTrueExpression(ExpressionSyntax expression) => expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.TrueLiteralExpression);

        /// <summary>
        ///     Determines whether a statement always exits the current loop body.
        /// </summary>
        /// <param name="statement">The statement to inspect.</param>
        /// <returns><see langword="true" /> when every path exits; otherwise, <see langword="false" />.</returns>
        private static bool IsExitStatement(StatementSyntax statement) => statement switch
        {
            BreakStatementSyntax or ReturnStatementSyntax or ThrowStatementSyntax => true,
            IfStatementSyntax { Else: null, Statement: var thenStatement } => IsExitStatement(thenStatement),
            IfStatementSyntax { Else.Statement: var elseStatement, Statement: var thenStatement } => IsExitStatement(thenStatement) && IsExitStatement(elseStatement),
            _ => false
        };
    }
}