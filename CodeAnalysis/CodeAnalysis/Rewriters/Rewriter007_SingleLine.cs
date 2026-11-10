using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Flattens the headers and the call-like expressions that are allowed to occupy a single line.
    /// </summary>
    /// <remarks>
    ///     A declaration header is flattened by replacing the whitespace between its first
    ///     signature token and its closing parenthesis or bracket with a single space. Comments
    ///     and directives inside the header are left untouched, because replacing them would remove
    ///     information that belongs to the declaration. A switch expression keeps its arms on
    ///     separate lines, and an arrow expression clause receives exactly one space after the arrow.
    /// </remarks>
    internal sealed class Rewriter007_SingleLine : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Flattens the receiver and the argument list of an invocation expression.
        /// </summary>
        /// <param name="node">The invocation expression to rewrite.</param>
        /// <returns>The flattened invocation expression.</returns>
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var result = (InvocationExpressionSyntax)base.VisitInvocationExpression(node)!;
            return result.WithExpression(Flatten(result.Expression)).WithArgumentList(Flatten(result.ArgumentList));
        }

        /// <summary>
        ///     Flattens the argument list of an object creation expression.
        /// </summary>
        /// <param name="node">The object creation expression to rewrite.</param>
        /// <returns>The object creation expression with a flattened argument list.</returns>
        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var result = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node)!;
            return result.ArgumentList is null ? result : result.WithArgumentList(Flatten(result.ArgumentList));
        }

        /// <summary>
        ///     Flattens the argument list of an implicit object creation expression.
        /// </summary>
        /// <param name="node">The implicit object creation expression to rewrite.</param>
        /// <returns>The implicit object creation expression with a flattened argument list.</returns>
        public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
        {
            var result = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node)!;
            return result.WithArgumentList(Flatten(result.ArgumentList));
        }

        /// <summary>
        ///     Flattens the argument list of a constructor initializer.
        /// </summary>
        /// <param name="node">The constructor initializer to rewrite.</param>
        /// <returns>The constructor initializer with a flattened argument list.</returns>
        public override SyntaxNode VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            var result = (ConstructorInitializerSyntax)base.VisitConstructorInitializer(node)!;
            return result.WithArgumentList(Flatten(result.ArgumentList));
        }

        /// <summary>
        ///     Formats the expression of an arrow expression clause and keeps the arrow on the declaration line.
        /// </summary>
        /// <param name="node">The arrow expression clause to rewrite.</param>
        /// <returns>The formatted arrow expression clause.</returns>
        /// <remarks>
        ///     A switch expression is rebuilt with one arm per line. Any other expression that contains an
        ///     invocation loses a leading line break, because the invocation itself is already flattened
        ///     and the leading break would leave the arrow with an empty line. Clauses whose expression
        ///     contains a comment or a directive are returned unchanged: a directive owns its line, and
        ///     replacing the trivia of the arrow would either move it next to the arrow or delete it.
        /// </remarks>
        public override SyntaxNode VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
        {
            var result = (ArrowExpressionClauseSyntax)base.VisitArrowExpressionClause(node)!;
            var expression = result.Expression;
            if (TriviaHelpers.HasCommentOrDirective(expression))
                return result;

            if (expression is SwitchExpressionSyntax switchExpression)
                expression = FormatSwitchExpression(switchExpression);
            else if (expression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Any())
                expression = expression.WithLeadingTrivia(default(SyntaxTriviaList));

            return result.WithArrowToken(result.ArrowToken.WithTrailingTrivia(SyntaxFactory.Space)).WithExpression(expression);
        }

        /// <summary>
        ///     Flattens the header of a method declaration.
        /// </summary>
        /// <param name="node">The method declaration to rewrite.</param>
        /// <returns>The method declaration with a flattened header.</returns>
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var result = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of a constructor declaration.
        /// </summary>
        /// <param name="node">The constructor declaration to rewrite.</param>
        /// <returns>The constructor declaration with a flattened header.</returns>
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var result = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of an operator declaration.
        /// </summary>
        /// <param name="node">The operator declaration to rewrite.</param>
        /// <returns>The operator declaration with a flattened header.</returns>
        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            var result = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of a conversion operator declaration.
        /// </summary>
        /// <param name="node">The conversion operator declaration to rewrite.</param>
        /// <returns>The conversion operator declaration with a flattened header.</returns>
        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            var result = (ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of a local function declaration.
        /// </summary>
        /// <param name="node">The local function declaration to rewrite.</param>
        /// <returns>The local function declaration with a flattened header.</returns>
        public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var result = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of a delegate declaration.
        /// </summary>
        /// <param name="node">The delegate declaration to rewrite.</param>
        /// <returns>The delegate declaration with a flattened header.</returns>
        public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            var result = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseParenToken);
        }

        /// <summary>
        ///     Flattens the header of an indexer declaration.
        /// </summary>
        /// <param name="node">The indexer declaration to rewrite.</param>
        /// <returns>The indexer declaration with a flattened header.</returns>
        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            var result = (IndexerDeclarationSyntax)base.VisitIndexerDeclaration(node)!;
            return FlattenHeader(result, result.ParameterList.CloseBracketToken);
        }

        /// <summary>
        ///     Puts every arm of a switch expression on its own line.
        /// </summary>
        /// <param name="node">The switch expression to format.</param>
        /// <returns>The formatted switch expression.</returns>
        /// <remarks>
        ///     The indentation is derived from the line on which the switch keyword starts. The opening
        ///     brace starts the following line at the owner indentation, the arms are indented one
        ///     level, and the closing brace returns to the owner indentation. The leading trivia of the
        ///     expression is removed because the arrow clause supplies the position of the expression.
        /// </remarks>
        private static SwitchExpressionSyntax FormatSwitchExpression(SwitchExpressionSyntax node)
        {
            var lineEnding = TriviaHelpers.GetLineEnding(node);
            var ownerIndent = TriviaHelpers.GetLineIndentation(node, node.SwitchKeyword);
            var armIndent = ownerIndent + "    ";
            var lineBreak = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(lineEnding));
            var openBrace = node.OpenBraceToken.WithLeadingTrivia(lineBreak.Add(SyntaxFactory.Whitespace(ownerIndent))).WithTrailingTrivia(default(SyntaxTriviaList));
            var closeBrace = node.CloseBraceToken.WithLeadingTrivia(lineBreak.Add(SyntaxFactory.Whitespace(ownerIndent)));
            var arms = node.Arms.Select(arm => arm.WithLeadingTrivia(lineBreak.Add(SyntaxFactory.Whitespace(armIndent))).WithTrailingTrivia(default(SyntaxTriviaList)));
            return node.WithLeadingTrivia(default(SyntaxTriviaList)).WithSwitchKeyword(node.SwitchKeyword.WithTrailingTrivia(default(SyntaxTriviaList))).WithOpenBraceToken(openBrace).WithArms(SyntaxFactory.SeparatedList(arms)).WithCloseBraceToken(closeBrace);
        }

        /// <summary>
        ///     Flattens the whitespace between the first signature token and the closing token of a header.
        /// </summary>
        /// <typeparam name="T">The type of the declaration.</typeparam>
        /// <param name="node">The declaration whose header is flattened.</param>
        /// <param name="endToken">The closing parenthesis or bracket of the header.</param>
        /// <returns>The declaration with the flattened header.</returns>
        /// <remarks>
        ///     The first token is taken after the last attribute list, so the line breaks between attributes and
        ///     the declaration are not flattened. Only whitespace trivia is replaced; a comment or a directive
        ///     in the header is preserved, and the header is left unchanged when a directive is present.
        /// </remarks>
        private static T FlattenHeader<T>(T node, SyntaxToken endToken) where T : SyntaxNode
        {
            if (node.DescendantTrivia(descendIntoTrivia: false).Any(static trivia => trivia.IsDirective))
                return node;

            var first = node.ChildNodes().OfType<AttributeListSyntax>().LastOrDefault()?.CloseBracketToken.GetNextToken() ?? node.GetFirstToken();
            var trivia = node.DescendantTrivia(descendIntoTrivia: false).Where(triviaItem => triviaItem.SpanStart >= first.Span.End && triviaItem.Span.End <= endToken.SpanStart && IsWhitespace(triviaItem)).ToArray();
            return trivia.Length == 0 ? node : node.ReplaceTrivia(trivia, static (_, _) => SyntaxFactory.Whitespace(" "));
        }

        /// <summary>
        ///     Flattens a node that spans several lines.
        /// </summary>
        /// <typeparam name="T">The type of the node.</typeparam>
        /// <param name="node">The node to flatten.</param>
        /// <returns>The flattened node, or the original node when it already occupies one line.</returns>
        /// <remarks>
        ///     The leading and trailing trivia are restored after normalization so that the
        ///     indentation and the comments around the node stay in place. A node that contains
        ///     a comment or a directive is not normalized, because normalization moves the
        ///     directive to the line of another token and produces source that does not compile.
        ///     A node that contains a construct with a meaningful multi-line layout is not normalized
        ///     either, because collapsing it would erase the indentation of its own lines.
        /// </remarks>
        private static T Flatten<T>(T node) where T : SyntaxNode
        {
            var firstLine = node.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line;
            var lastLine = node.GetLastToken().GetLocation().GetLineSpan().EndLinePosition.Line;
            if (firstLine == lastLine || TriviaHelpers.HasCommentOrDirective(node) || SyntaxHelpers.ContainsMultilineLayoutConstruct(node))
                return node;

            var leading = node.GetLeadingTrivia();
            var trailing = node.GetTrailingTrivia();
            var result = node.NormalizeWhitespace(" ", " ");
            return result.WithLeadingTrivia(leading).WithTrailingTrivia(trailing);
        }

        /// <summary>
        ///     Determines whether the trivia is whitespace or a line break.
        /// </summary>
        /// <param name="trivia">The trivia to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the trivia is whitespace or
        ///     a line break; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsWhitespace(SyntaxTrivia trivia) => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia);
    }
}