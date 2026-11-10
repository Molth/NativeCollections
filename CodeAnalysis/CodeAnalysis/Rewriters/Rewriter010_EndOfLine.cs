using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Normalizes every line break of the compilation unit to <c>\r\n</c>.
    /// </summary>
    /// <remarks>
    ///     The rewriter replaces only the <c>EndOfLine</c> trivia and the <c>XmlTextLiteralNewLine</c>
    ///     tokens of the documentation comments, which exist at real line breaks. The raw line breaks
    ///     inside a multi-line verbatim or raw string literal belong to the literal token instead, so
    ///     they are preserved; rewriting them would change the value that the program computes. The
    ///     step runs last, because the earlier rewriters create trivia from the line ending of the
    ///     source and the normalization must also cover what they inserted.
    /// </remarks>
    internal sealed class Rewriter010_EndOfLine : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Initializes a new instance that visits into structured trivia.
        /// </summary>
        /// <remarks>
        ///     Structured trivia holds the documentation comments, so the visit must descend into them
        ///     for the normalization to cover their line breaks.
        /// </remarks>
        public Rewriter010_EndOfLine() : base(true)
        {
        }

        /// <summary>
        ///     Replaces the line break with <c>\r\n</c> when it uses another style.
        /// </summary>
        /// <param name="trivia">The trivia to rewrite.</param>
        /// <returns>The normalized line break, or the original trivia.</returns>
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia) && trivia.ToFullString() != "\r\n")
                return SyntaxFactory.EndOfLine("\r\n");

            return base.VisitTrivia(trivia);
        }

        /// <summary>
        ///     Replaces the line break of a documentation comment with <c>\r\n</c> when it uses another style.
        /// </summary>
        /// <param name="token">The token to rewrite.</param>
        /// <returns>The normalized line break, or the original token.</returns>
        public override SyntaxToken VisitToken(SyntaxToken token)
        {
            var result = base.VisitToken(token);
            if (result.IsKind(SyntaxKind.XmlTextLiteralNewLineToken) && result.Text != "\r\n")
                return SyntaxFactory.Token(result.LeadingTrivia, SyntaxKind.XmlTextLiteralNewLineToken, "\r\n", "\r\n", result.TrailingTrivia);

            return result;
        }
    }
}