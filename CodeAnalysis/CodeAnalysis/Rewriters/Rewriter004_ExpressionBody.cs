using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Converts a callable whose block body contains a single expression into an expression body.
    /// </summary>
    /// <remarks>
    ///     The conversion is only applied when the body contains exactly one expression, return,
    ///     or throw statement and the statement occupies one line. A body with directives, local
    ///     declarations, or several statements is preserved. The parameter list is kept, and a trailing
    ///     space is inserted after the closing parenthesis so that the arrow is separated from it.
    /// </remarks>
    internal sealed class Rewriter004_ExpressionBody : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Rewrites a method declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The method declaration to rewrite.</param>
        /// <returns>The rewritten method declaration.</returns>
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) => RewriteMethod((MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!);

        /// <summary>
        ///     Rewrites a constructor declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The constructor declaration to rewrite.</param>
        /// <returns>The rewritten constructor declaration.</returns>
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => RewriteConstructor((ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node)!);

        /// <summary>
        ///     Rewrites a destructor declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The destructor declaration to rewrite.</param>
        /// <returns>The rewritten destructor declaration.</returns>
        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node) => RewriteDestructor((DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node)!);

        /// <summary>
        ///     Rewrites an operator declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The operator declaration to rewrite.</param>
        /// <returns>The rewritten operator declaration.</returns>
        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node) => RewriteOperator((OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node)!);

        /// <summary>
        ///     Rewrites a conversion operator declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The conversion operator declaration to rewrite.</param>
        /// <returns>The rewritten conversion operator declaration.</returns>
        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node) => RewriteConversionOperator((ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node)!);

        /// <summary>
        ///     Rewrites a local function declaration into an expression body when possible.
        /// </summary>
        /// <param name="node">The local function declaration to rewrite.</param>
        /// <returns>The rewritten local function declaration.</returns>
        public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) => RewriteLocalFunction((LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!);

        /// <summary>
        ///     Converts the body of a method declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The method declaration to rewrite.</param>
        /// <returns>The rewritten method declaration.</returns>
        private static MethodDeclarationSyntax RewriteMethod(MethodDeclarationSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Converts the body of a constructor declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The constructor declaration to rewrite.</param>
        /// <returns>The rewritten constructor declaration.</returns>
        private static ConstructorDeclarationSyntax RewriteConstructor(ConstructorDeclarationSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Converts the body of a destructor declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The destructor declaration to rewrite.</param>
        /// <returns>The rewritten destructor declaration.</returns>
        private static DestructorDeclarationSyntax RewriteDestructor(DestructorDeclarationSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Converts the body of an operator declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The operator declaration to rewrite.</param>
        /// <returns>The rewritten operator declaration.</returns>
        private static OperatorDeclarationSyntax RewriteOperator(OperatorDeclarationSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Converts the body of a conversion operator declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The conversion operator declaration to rewrite.</param>
        /// <returns>The rewritten conversion operator declaration.</returns>
        private static ConversionOperatorDeclarationSyntax RewriteConversionOperator(ConversionOperatorDeclarationSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Converts the body of a local function declaration when it contains a single expression.
        /// </summary>
        /// <param name="node">The local function declaration to rewrite.</param>
        /// <returns>The rewritten local function declaration.</returns>
        private static LocalFunctionStatementSyntax RewriteLocalFunction(LocalFunctionStatementSyntax node) => node.Body is BlockSyntax body && TryGetExpression(body, out var expression) ? node.WithParameterList(WithInlineCloseParen(node.ParameterList)).WithBody(null).WithExpressionBody(CreateArrow(expression)).WithSemicolonToken(CreateSemicolon(body)) : node;

        /// <summary>
        ///     Adds the space between the closing parenthesis and the arrow.
        /// </summary>
        /// <param name="parameterList">The parameter list to adjust.</param>
        /// <returns>The parameter list with a trailing space.</returns>
        private static ParameterListSyntax WithInlineCloseParen(ParameterListSyntax parameterList) => parameterList.WithCloseParenToken(parameterList.CloseParenToken.WithTrailingTrivia(SyntaxFactory.Whitespace(" ")));

        /// <summary>
        ///     Creates the arrow clause that replaces the block body.
        /// </summary>
        /// <param name="expression">The expression of the original statement.</param>
        /// <returns>The arrow clause for the expression.</returns>
        /// <remarks>
        ///     The expression loses its trivia so that the arrow and the expression are separated
        ///     by exactly one space and no line break is inherited from the original statement.
        /// </remarks>
        private static ArrowExpressionClauseSyntax CreateArrow(ExpressionSyntax expression) => SyntaxFactory.ArrowExpressionClause(SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken).WithTrailingTrivia(SyntaxFactory.Whitespace(" ")), expression.WithoutTrivia());

        /// <summary>
        ///     Creates the semicolon that terminates the expression body.
        /// </summary>
        /// <param name="body">The block body that is replaced.</param>
        /// <returns>The terminating semicolon token.</returns>
        /// <remarks>
        ///     The trailing trivia of the original closing brace is moved to the
        ///     semicolon, so comments and line breaks that followed the body are preserved.
        /// </remarks>
        private static SyntaxToken CreateSemicolon(BlockSyntax body) => SyntaxFactory.Token(SyntaxKind.SemicolonToken).WithTrailingTrivia(body.CloseBraceToken.TrailingTrivia);

        /// <summary>
        ///     Extracts the expression that can replace the block body.
        /// </summary>
        /// <param name="body">The block body to inspect.</param>
        /// <param name="expression">
        ///     The expression that can form the body; unused when the method returns <see langword="false" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> when the body can be converted; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Only one statement is accepted, and it has to occupy one line. Directives are rejected because
        ///     moving the expression around a directive would change which code the directive applies to.
        /// </remarks>
        private static bool TryGetExpression(BlockSyntax body, out ExpressionSyntax expression)
        {
            expression = null!;
            if (body.Statements.Count != 1 || body.DescendantTrivia(descendIntoTrivia: true).Any(static trivia => trivia.IsDirective))
                return false;

            var statement = body.Statements[0];
            var first = statement.GetFirstToken();
            var last = statement.GetLastToken();
            if (first.GetLocation().GetLineSpan().StartLinePosition.Line != last.GetLocation().GetLineSpan().EndLinePosition.Line)
                return false;

            switch (statement)
            {
                case ExpressionStatementSyntax expressionStatement:
                    expression = expressionStatement.Expression;
                    return true;

                case ReturnStatementSyntax { Expression: not null } returnStatement:
                    expression = returnStatement.Expression;
                    return true;

                case ThrowStatementSyntax { Expression: not null } throwStatement:
                    expression = SyntaxFactory.ThrowExpression(throwStatement.Expression);
                    return true;

                default:
                    return false;
            }
        }
    }
}