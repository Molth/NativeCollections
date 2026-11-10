using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the trivia and layout helpers that the rewriters share.
    /// </summary>
    /// <remarks>
    ///     Token-level layout is read from the trivia attached to the node that is currently being
    ///     rewritten. A rewritten node keeps the syntax tree of the original source even though its
    ///     spans have moved, so looking the node up in that text can observe unrelated characters.
    ///     Source text is only used for the line ending and for the indentation of a whole line, because
    ///     trivia does not describe the beginning of a line when the token follows another token.
    /// </remarks>
    internal static class TriviaHelpers
    {
        /// <summary>
        ///     Determines whether the trivia is a comment that the body cleanup has to remove.
        /// </summary>
        /// <param name="trivia">The trivia to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the trivia is a comment; otherwise, <see langword="false" />.
        /// </returns>
        public static bool IsComment(SyntaxTrivia trivia) => trivia.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia or SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia;

        /// <summary>
        ///     Determines whether the trivia contains a preprocessor directive.
        /// </summary>
        /// <param name="trivia">The trivia list to inspect.</param>
        /// <returns><see langword="true" /> when a directive is present; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     A directive owns the line it is written on, so layout normalization must not move it,
        ///     merge it with surrounding tokens, or drop it together with the trivia of a replaced token.
        /// </remarks>
        public static bool HasDirective(SyntaxTriviaList trivia) => trivia.Any(static triviaItem => triviaItem.IsDirective);

        /// <summary>
        ///     Determines whether the node contains a comment or a preprocessor directive.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when a comment or a directive is present; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The check includes the leading trivia of the first token and the trailing trivia of the last token
        ///     of the node. Both kinds of trivia carry information that a whitespace normalization would destroy.
        /// </remarks>
        public static bool HasCommentOrDirective(SyntaxNode node) => node.DescendantTrivia(descendIntoTrivia: false).Any(static triviaItem => triviaItem.IsDirective || IsComment(triviaItem));

        /// <summary>
        ///     Removes every comment from the body and collapses the blank lines that the removal leaves behind.
        /// </summary>
        /// <param name="body">The block whose comments are removed.</param>
        /// <returns>The block without comments.</returns>
        /// <remarks>
        ///     A comment that occupies a whole line is removed together with its indentation and line break.
        ///     A trailing comment after code is removed on its own, because deleting the surrounding line
        ///     would also delete the statement. Directives are not comments and are therefore preserved.
        /// </remarks>
        public static BlockSyntax RemoveBodyComments(BlockSyntax body)
        {
            var trivia = body.DescendantTrivia(descendIntoTrivia: true).ToArray();
            var comments = trivia.Where(IsComment).ToArray();
            if (comments.Length == 0)
                return body;

            var remove = new HashSet<int>();
            var source = body.SyntaxTree?.GetText();
            for (var index = 0; index < trivia.Length; ++index)
            {
                if (!IsComment(trivia[index]))
                    continue;

                remove.Add(index);
                if (!IsFullLineComment(source, trivia[index]))
                    continue;

                var previous = index - 1;
                while (previous >= 0 && trivia[previous].IsKind(SyntaxKind.WhitespaceTrivia))
                    previous--;

                for (var indexBeforeComment = previous + 1; indexBeforeComment < index; ++indexBeforeComment)
                    remove.Add(indexBeforeComment);

                if (index + 1 < trivia.Length && trivia[index + 1].IsKind(SyntaxKind.EndOfLineTrivia))
                    remove.Add(index + 1);
            }

            CollapseBlankLines(trivia, remove, source);
            return remove.Count == 0 ? body : body.ReplaceTrivia(trivia.Where((_, index) => remove.Contains(index)), static (_, _) => default);
        }

        /// <summary>
        ///     Determines whether the comment is the first non-whitespace content on its line.
        /// </summary>
        /// <param name="source">The original source text, or <see langword="null" /> when it is unavailable.</param>
        /// <param name="comment">The comment to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the comment starts a line; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The check uses the original source text because the trivia sequence does not contain
        ///     the code tokens between two line breaks. Walking backwards through trivia alone
        ///     cannot distinguish a full-line comment from a trailing comment, since the code before
        ///     the trailing comment is not part of the trivia sequence. When the source text is
        ///     unavailable, the comment is treated as trailing and only the comment itself is removed.
        /// </remarks>
        private static bool IsFullLineComment(SourceText? source, SyntaxTrivia comment)
        {
            if (source is null)
                return false;

            var line = source.Lines.GetLineFromPosition(comment.SpanStart);
            return IsWhitespaceOnly(source, line.Start, comment.SpanStart);
        }

        /// <summary>
        ///     Marks the whitespace and line breaks of the blank lines that surround the comments.
        /// </summary>
        /// <param name="trivia">The original trivia sequence.</param>
        /// <param name="remove">The set that receives the indexes of the trivia to remove.</param>
        /// <param name="source">The original source text, or <see langword="null" /> when it is unavailable.</param>
        /// <remarks>
        ///     The check runs on the original trivia and source text, before the comments are removed. The
        ///     spans of a rewritten node no longer describe the original text, so the check cannot run after
        ///     the replacement. Lines that contain code are not blank even when the only trivia between their
        ///     line breaks is whitespace, because code tokens are not trivia and do not appear in the sequence.
        /// </remarks>
        private static void CollapseBlankLines(SyntaxTrivia[] trivia, HashSet<int> remove, SourceText? source)
        {
            if (source is null)
                return;

            var previousEndOfLine = -1;
            for (var index = 0; index < trivia.Length; ++index)
            {
                if (!trivia[index].IsKind(SyntaxKind.EndOfLineTrivia))
                    continue;

                if (previousEndOfLine >= 0 && IsWhitespaceOnly(source, trivia[previousEndOfLine].Span.End, trivia[index].SpanStart))
                {
                    for (var removeIndex = previousEndOfLine + 1; removeIndex <= index; ++removeIndex)
                        remove.Add(removeIndex);

                    continue;
                }

                previousEndOfLine = index;
            }
        }

        /// <summary>
        ///     Determines whether the source range contains only whitespace.
        /// </summary>
        /// <param name="source">The source text to inspect.</param>
        /// <param name="start">The inclusive start position of the range.</param>
        /// <param name="end">The exclusive end position of the range.</param>
        /// <returns>
        ///     <see langword="true" /> when the range contains only whitespace; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsWhitespaceOnly(SourceText source, int start, int end)
        {
            for (var position = start; position < end; ++position)
            {
                if (!char.IsWhiteSpace(source[position]))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Collapses consecutive line breaks that have nothing but whitespace between them.
        /// </summary>
        /// <param name="trivia">The trivia list to collapse.</param>
        /// <returns>The trivia list without blank lines.</returns>
        /// <remarks>
        ///     Only the given trivia list is inspected. A line that contains a comment or a
        ///     directive is therefore preserved, even when the surrounding lines are empty.
        /// </remarks>
        public static SyntaxTriviaList CollapseBlankLines(SyntaxTriviaList trivia)
        {
            var result = new List<SyntaxTrivia>(trivia.Count);
            var previousEndOfLine = -1;
            foreach (var current in trivia)
            {
                if (current.IsKind(SyntaxKind.EndOfLineTrivia) && previousEndOfLine >= 0 && IsWhitespaceOnly(result, previousEndOfLine + 1))
                {
                    result.RemoveRange(previousEndOfLine + 1, result.Count - previousEndOfLine - 1);
                    continue;
                }

                if (current.IsKind(SyntaxKind.EndOfLineTrivia))
                    previousEndOfLine = result.Count;

                result.Add(current);
            }

            return SyntaxFactory.TriviaList(result);
        }

        /// <summary>
        ///     Gets the line ending used by the source that contains the node.
        /// </summary>
        /// <param name="node">The node whose source is inspected.</param>
        /// <returns><c>\r\n</c> when the source uses CRLF; otherwise, <c>\n</c>.</returns>
        public static string GetLineEnding(SyntaxNode node) => GetLineEnding(node.SyntaxTree?.GetText().ToString() ?? string.Empty);

        /// <summary>
        ///     Gets the first line ending style used by the source, defaulting to LF.
        /// </summary>
        /// <param name="source">The source text to inspect.</param>
        /// <returns><c>\r\n</c> when the source uses CRLF; otherwise, <c>\n</c>.</returns>
        public static string GetLineEnding(string source) => source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";

        /// <summary>
        ///     Gets the indentation of the line on which the token described by the trivia starts.
        /// </summary>
        /// <param name="trivia">The leading trivia of the token.</param>
        /// <returns>The indentation text of the current line.</returns>
        /// <remarks>
        ///     The search walks backwards until the previous line break. Any non-whitespace trivia
        ///     on the current line clears the accumulated indentation, so the result describes
        ///     the token that follows the trivia rather than an earlier token on the same line.
        /// </remarks>
        public static string GetIndentation(SyntaxTriviaList trivia)
        {
            var result = new StringBuilder();
            foreach (var current in trivia.Reverse())
            {
                if (current.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    result.Insert(0, current.ToFullString());
                    continue;
                }

                if (current.IsKind(SyntaxKind.EndOfLineTrivia))
                    break;

                result.Clear();
            }

            return result.ToString();
        }

        /// <summary>
        ///     Gets the indentation of the line on which the token is located.
        /// </summary>
        /// <param name="node">The node whose syntax tree contains the token.</param>
        /// <param name="token">The token whose line is inspected.</param>
        /// <returns>The indentation text of the line on which the token is located.</returns>
        /// <remarks>
        ///     The leading trivia of a token that follows another token on the same line does not contain the
        ///     indentation of the whole line, because that whitespace belongs to the leading trivia of the first
        ///     token on the line. This helper therefore reads the line from the source text; it is only used
        ///     when the line indentation, rather than the trivia immediately before the token, is required.
        /// </remarks>
        public static string GetLineIndentation(SyntaxNode node, SyntaxToken token)
        {
            var source = node.SyntaxTree?.GetText();
            if (source is null)
                return GetIndentation(token.LeadingTrivia);

            var position = Math.Min(token.SpanStart, source.Length);
            var line = source.Lines.GetLineFromPosition(position).ToString();
            var length = 0;
            while (length < line.Length && char.IsWhiteSpace(line[length]))
                ++length;

            return line.Substring(0, length);
        }

        /// <summary>
        ///     Adds the indentation to a token that starts a line without any leading whitespace.
        /// </summary>
        /// <param name="token">The token to indent.</param>
        /// <param name="indentation">The indentation text to insert.</param>
        /// <returns>The token with the indentation, or the original token when the line already has content.</returns>
        /// <remarks>
        ///     The whitespace is inserted after the last line break rather than in front of the
        ///     leading trivia, because the leading trivia can itself begin with the line break.
        ///     Prefixing it would indent the empty line before the token instead of the token. A
        ///     line that already contains whitespace, a comment, or a directive is left untouched.
        /// </remarks>
        public static SyntaxToken WithIndentation(SyntaxToken token, string indentation)
        {
            if (indentation.Length == 0)
                return token;

            var leading = token.LeadingTrivia;
            var lastLineBreak = -1;
            for (var index = 0; index < leading.Count; ++index)
            {
                if (leading[index].IsKind(SyntaxKind.EndOfLineTrivia))
                    lastLineBreak = index;
            }

            if (lastLineBreak < 0)
                return leading.Count == 0 ? token.WithLeadingTrivia(leading.Insert(0, SyntaxFactory.Whitespace(indentation))) : token;

            return lastLineBreak + 1 == leading.Count ? token.WithLeadingTrivia(leading.Insert(lastLineBreak + 1, SyntaxFactory.Whitespace(indentation))) : token;
        }

        /// <summary>
        ///     Wraps a statement in a block that uses the indentation of the owner and of the statement.
        /// </summary>
        /// <param name="owner">The statement that owns the body.</param>
        /// <param name="statement">The statement to wrap.</param>
        /// <param name="ownerIndent">The indentation of the owner; derived from the owner when omitted.</param>
        /// <returns>The statement inside a block.</returns>
        /// <remarks>
        ///     The statement loses its own trivia because the block supplies the line breaks and indentation. The
        ///     original trailing trivia is kept on the closing brace so that comments and line breaks that follow
        ///     the statement survive the rewrite. A directive that precedes the statement is moved in front of
        ///     the opening brace instead of being discarded, because a directive owns the line it is written on.
        /// </remarks>
        public static BlockSyntax WrapInBlock(StatementSyntax owner, StatementSyntax statement, string? ownerIndent = null)
        {
            var lineEnding = GetLineEnding(owner);
            ownerIndent ??= GetIndentation(owner.GetFirstToken().LeadingTrivia);
            var statementIndent = GetIndentation(statement.GetFirstToken().LeadingTrivia);
            if (statementIndent.Length == 0)
                statementIndent = ownerIndent + "    ";

            var body = statement.WithoutTrivia();
            var openBraceLeading = HasDirective(statement.GetLeadingTrivia()) ? statement.GetLeadingTrivia() : SyntaxFactory.TriviaList(SyntaxFactory.Whitespace(ownerIndent));
            var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(openBraceLeading).WithTrailingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(lineEnding), SyntaxFactory.Whitespace(statementIndent)));
            var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(lineEnding), SyntaxFactory.Whitespace(ownerIndent))).WithTrailingTrivia(statement.GetTrailingTrivia());
            return SyntaxFactory.Block(body).WithOpenBraceToken(openBrace).WithCloseBraceToken(closeBrace);
        }

        /// <summary>
        ///     Determines whether every trivia after the index is whitespace.
        /// </summary>
        /// <param name="trivia">The trivia list to inspect.</param>
        /// <param name="start">The inclusive start index of the range.</param>
        /// <returns>
        ///     <see langword="true" /> when the range contains only whitespace; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsWhitespaceOnly(List<SyntaxTrivia> trivia, int start)
        {
            for (var index = start; index < trivia.Count; ++index)
            {
                if (!trivia[index].IsKind(SyntaxKind.WhitespaceTrivia))
                    return false;
            }

            return true;
        }
    }
}