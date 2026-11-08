using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Removes the comments that are written inside the body of a callable member.
    /// </summary>
    /// <remarks>
    ///     Comments outside a body describe the member and are preserved. Comments inside a body are removed
    ///     from methods, constructors, destructors, operators, conversions, local functions, accessors, anonymous
    ///     methods, and block-bodied lambdas. Expression-bodied lambdas have no block and are returned unchanged.
    /// </remarks>
    internal sealed class Rewriter002_BodyCommentRemover : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     Removes the body comments of a method declaration.
        /// </summary>
        /// <param name="node">The method declaration to rewrite.</param>
        /// <returns>The method declaration without body comments.</returns>
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) => RemoveBodyComments((MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of a constructor declaration.
        /// </summary>
        /// <param name="node">The constructor declaration to rewrite.</param>
        /// <returns>The constructor declaration without body comments.</returns>
        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node) => RemoveBodyComments((ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of a destructor declaration.
        /// </summary>
        /// <param name="node">The destructor declaration to rewrite.</param>
        /// <returns>The destructor declaration without body comments.</returns>
        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node) => RemoveBodyComments((DestructorDeclarationSyntax)base.VisitDestructorDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of an operator declaration.
        /// </summary>
        /// <param name="node">The operator declaration to rewrite.</param>
        /// <returns>The operator declaration without body comments.</returns>
        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node) => RemoveBodyComments((OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of a conversion operator declaration.
        /// </summary>
        /// <param name="node">The conversion operator declaration to rewrite.</param>
        /// <returns>The conversion operator declaration without body comments.</returns>
        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node) => RemoveBodyComments((ConversionOperatorDeclarationSyntax)base.VisitConversionOperatorDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of a local function declaration.
        /// </summary>
        /// <param name="node">The local function declaration to rewrite.</param>
        /// <returns>The local function declaration without body comments.</returns>
        public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node) => RemoveBodyComments((LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of an accessor declaration.
        /// </summary>
        /// <param name="node">The accessor declaration to rewrite.</param>
        /// <returns>The accessor declaration without body comments.</returns>
        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node) => RemoveBodyComments((AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node)!, static value => value.Body, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of an anonymous method expression.
        /// </summary>
        /// <param name="node">The anonymous method expression to rewrite.</param>
        /// <returns>The anonymous method expression without body comments.</returns>
        public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) => RemoveBodyComments((AnonymousMethodExpressionSyntax)base.VisitAnonymousMethodExpression(node)!, static value => value.Block, static (value, body) => value.WithBlock(body));

        /// <summary>
        ///     Removes the body comments of a lambda expression with a parenthesized parameter list.
        /// </summary>
        /// <param name="node">The lambda expression to rewrite.</param>
        /// <returns>The lambda expression without body comments.</returns>
        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => RemoveBodyComments((ParenthesizedLambdaExpressionSyntax)base.VisitParenthesizedLambdaExpression(node)!, static value => value.Body as BlockSyntax, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the body comments of a lambda expression with a single parameter.
        /// </summary>
        /// <param name="node">The lambda expression to rewrite.</param>
        /// <returns>The lambda expression without body comments.</returns>
        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => RemoveBodyComments((SimpleLambdaExpressionSyntax)base.VisitSimpleLambdaExpression(node)!, static value => value.Body as BlockSyntax, static (value, body) => value.WithBody(body));

        /// <summary>
        ///     Removes the comments from the body of the rewritten node.
        /// </summary>
        /// <typeparam name="T">The type of the callable node.</typeparam>
        /// <param name="node">The rewritten callable node.</param>
        /// <param name="getBody">
        ///     Gets the block body of the node, or <see langword="null" /> when the body is an expression.
        /// </param>
        /// <param name="withBody">Creates a copy of the node with the given block body.</param>
        /// <returns>The node with the body comments removed, or the original node when it has no block body.</returns>
        /// <remarks>
        ///     A node without a block body has no comments to remove. The getter and setter are passed
        ///     as delegates so that every callable kind can use the same null and rewrite handling.
        /// </remarks>
        private static T RemoveBodyComments<T>(T node, Func<T, BlockSyntax?> getBody, Func<T, BlockSyntax, T> withBody)
        {
            var body = getBody(node);
            return body is null ? node : withBody(node, TriviaHelpers.RemoveBodyComments(body));
        }
    }
}