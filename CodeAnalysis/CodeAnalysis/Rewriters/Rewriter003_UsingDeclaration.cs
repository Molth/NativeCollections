using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Converts a using declaration into a using statement whose block contains the following statements.
    /// </summary>
    /// <remarks>
    ///     A using declaration disposes its resource when the enclosing scope ends. Replacing it with a using
    ///     statement keeps the same lifetime because every following statement in the same block becomes
    ///     the body of the statement. A declaration that is followed by no statements is left unchanged,
    ///     since an empty using statement would dispose the resource earlier than the declaration did.
    /// </remarks>
    internal sealed class Rewriter003_UsingDeclaration : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Converts a using declaration at the start of a block.
        /// </summary>
        /// <param name="node">The block to rewrite.</param>
        /// <returns>The block with the using declaration converted to a using statement.</returns>
        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var result = (BlockSyntax)base.VisitBlock(node)!;
            var statements = RewriteStatements(result.Statements, node);
            return statements == result.Statements ? result : result.WithStatements(statements);
        }

        /// <summary>
        ///     Converts a using declaration at the start of a switch section.
        /// </summary>
        /// <param name="node">The switch section to rewrite.</param>
        /// <returns>The switch section with the using declaration converted to a using statement.</returns>
        public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
        {
            var result = (SwitchSectionSyntax)base.VisitSwitchSection(node)!;
            var statements = RewriteStatements(result.Statements, node);
            return statements == result.Statements ? result : result.WithStatements(statements);
        }

        /// <summary>
        ///     Replaces the first using declaration of the statement list with a using statement.
        /// </summary>
        /// <param name="statements">The statements of the block or switch section.</param>
        /// <param name="sourceNode">The original node whose layout supplies the indentation and line ending.</param>
        /// <returns>The rewritten statement list.</returns>
        /// <remarks>
        ///     Only the first using declaration has to be considered. The conversion moves every statement after the
        ///     declaration into the using statement, so a later declaration is processed as part of that nested block
        ///     on a subsequent visit. A labeled declaration is converted while keeping its label on the new statement.
        /// </remarks>
        private static SyntaxList<StatementSyntax> RewriteStatements(SyntaxList<StatementSyntax> statements, SyntaxNode sourceNode)
        {
            for (var index = 0; index < statements.Count; ++index)
            {
                var declaration = statements[index] as LocalDeclarationStatementSyntax;
                if (declaration is null || !declaration.UsingKeyword.IsKind(SyntaxKind.UsingKeyword))
                    continue;

                var body = statements.Skip(index + 1).ToArray();
                if (body.Length == 0)
                    return statements;

                var usingStatement = CreateUsingStatement(declaration, body, sourceNode);
                var replacement = statements[index] is LabeledStatementSyntax labeled ? (StatementSyntax)labeled.WithStatement(usingStatement) : usingStatement;
                return SyntaxFactory.List(statements.Take(index).Append(replacement));
            }

            return statements;
        }

        /// <summary>
        ///     Builds the using statement that contains the declaration and the following statements.
        /// </summary>
        /// <param name="declaration">The using declaration to convert.</param>
        /// <param name="body">The statements that become the body of the using statement.</param>
        /// <param name="sourceNode">The original node whose layout supplies the indentation and line ending.</param>
        /// <returns>The using statement.</returns>
        /// <remarks>
        ///     The statement is parsed from text so that the comments and the trailing trivia of the
        ///     declaration and the body are preserved. The declaration itself is taken from its full text,
        ///     and the body statements are re-indented one level deeper than the declaration. An
        ///     <c>
        ///         await
        ///         using
        ///     </c>
        ///     declaration keeps the <see langword="await" /> keyword in the generated statement.
        /// </remarks>
        private static UsingStatementSyntax CreateUsingStatement(LocalDeclarationStatementSyntax declaration, StatementSyntax[] body, SyntaxNode sourceNode)
        {
            var lineEnding = TriviaHelpers.GetLineEnding(sourceNode);
            var ownerIndent = TriviaHelpers.GetLineIndentation(sourceNode, declaration.GetFirstToken());
            var bodyIndent = ownerIndent + "    ";
            var bodyText = string.Concat(body.Select(static statement => statement.ToFullString())).Trim();
            var declarationText = declaration.Declaration.ToFullString();
            var isAwait = declaration.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword);
            var statementText = string.Concat(isAwait ? "await using (" : "using (", declarationText, ")", lineEnding, ownerIndent, "{", lineEnding, IndentLines(bodyText, bodyIndent, lineEnding), lineEnding, ownerIndent, "}");
            var generated = (UsingStatementSyntax)SyntaxFactory.ParseStatement(statementText, 0, new CSharpParseOptions(LanguageVersion.Preview));
            var nested = (UsingStatementSyntax)new Rewriter003_UsingDeclaration().Visit(generated)!;
            return nested.WithLeadingTrivia(declaration.GetLeadingTrivia()).WithTrailingTrivia(declaration.GetTrailingTrivia());
        }

        /// <summary>
        ///     Indents every line of the body text.
        /// </summary>
        /// <param name="text">The body text to indent.</param>
        /// <param name="indent">The indentation to add to the body lines.</param>
        /// <param name="lineEnding">The line ending to use.</param>
        /// <returns>The indented body text.</returns>
        /// <remarks>
        ///     The original indentation of the body statements is removed before the new indentation
        ///     is added, because the statements were written one level deeper than the enclosing scope
        ///     and the generated using statement adds another level. A directive is not indented,
        ///     because preprocessor directives have to start at a column that is independent of the
        ///     surrounding code. A blank line stays empty and does not receive trailing whitespace.
        /// </remarks>
        private static string IndentLines(string text, string indent, string lineEnding)
        {
            var lines = text.Replace("\r", string.Empty).Split('\n');
            for (var index = 0; index < lines.Length; ++index)
            {
                var line = lines[index].TrimStart();
                lines[index] = line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal) ? line : indent + line;
            }

            return string.Join(lineEnding, lines);
        }
    }
}