using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Puts consecutive attribute lists on separate lines and keeps the annotated declaration on its own line.
    /// </summary>
    /// <remarks>
    ///     The rewriter only changes the line breaks between attribute lists and the declaration that
    ///     follows them. The attribute arguments, the indentation of the first attribute, and the
    ///     attribute order are preserved. It runs before the other rewriters so that later header
    ///     flattening cannot join an attribute list with the declaration or with another attribute list.
    /// </remarks>
    internal sealed class Rewriter001_AttributeLineRestorer : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Restores the line breaks around the attribute lists of a member or a local function.
        /// </summary>
        /// <param name="node">The node to rewrite.</param>
        /// <returns>The node with restored attribute line breaks.</returns>
        /// <remarks>
        ///     A local function can carry attributes, so it is handled together with the member
        ///     declarations. Other nodes are returned unchanged, because an attribute list there
        ///     cannot be placed on a separate line without changing the declaration it belongs to.
        /// </remarks>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            var result = base.Visit(node);
            if (result is null || (result is not MemberDeclarationSyntax && result is not LocalFunctionStatementSyntax))
                return result;

            var lists = result.ChildNodes().OfType<AttributeListSyntax>().ToArray();
            if (lists.Length == 0)
                return result;

            var lineEnding = TriviaHelpers.GetLineEnding(result);
            var indentation = TriviaHelpers.GetLineIndentation(result, lists[0].OpenBracketToken);
            var lineBreak = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(lineEnding));
            var replacements = new Dictionary<int, AttributeListSyntax>();
            for (var index = 0; index < lists.Length; ++index)
            {
                var current = lists[index];
                if (index + 1 < lists.Length && current.CloseBracketToken.GetLocation().GetLineSpan().EndLinePosition.Line == lists[index + 1].OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Line)
                    current = current.WithCloseBracketToken(current.CloseBracketToken.WithTrailingTrivia(lineBreak));

                if (index == lists.Length - 1)
                {
                    var next = current.CloseBracketToken.GetNextToken();
                    if (next.RawKind != 0 && current.CloseBracketToken.GetLocation().GetLineSpan().EndLinePosition.Line == next.GetLocation().GetLineSpan().StartLinePosition.Line)
                        current = current.WithCloseBracketToken(current.CloseBracketToken.WithTrailingTrivia(lineBreak));
                }

                replacements[lists[index].SpanStart] = current;
            }

            var restored = result.ReplaceNodes(lists, (oldNode, _) => replacements[oldNode.SpanStart]);
            if (indentation.Length > 0)
            {
                for (var index = 1; index < lists.Length; ++index)
                {
                    var currentLists = restored.ChildNodes().OfType<AttributeListSyntax>().ToArray();
                    restored = restored.ReplaceToken(currentLists[index].OpenBracketToken, TriviaHelpers.WithIndentation(currentLists[index].OpenBracketToken, indentation));
                }
            }

            var last = restored.ChildNodes().OfType<AttributeListSyntax>().Last();
            var nextToken = last.CloseBracketToken.GetNextToken();
            if (nextToken.RawKind != 0)
                restored = restored.ReplaceToken(nextToken, TriviaHelpers.WithIndentation(nextToken, indentation));

            return restored;
        }
    }
}