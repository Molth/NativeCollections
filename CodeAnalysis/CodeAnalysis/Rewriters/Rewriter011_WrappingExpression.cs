using System;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Replaces the arithmetic, conversion and <see langword="unchecked" />
    ///     constructs of a document with the helpers of the analyzed library.
    /// </summary>
    /// <remarks>
    ///     The rewriter works on a single semantic model of the original tree, so types are always resolved
    ///     from the original nodes and never from the nodes it creates. It also tracks the enclosing
    ///     <see langword="checked" /> context, because an operation that the author explicitly marked as checked
    ///     must keep its throwing behavior. Nodes that the rewriter creates are not added back to the tree before
    ///     they are returned, so the caller has to replace the root instead of reusing the model afterwards.
    /// </remarks>
    internal sealed class Rewriter011_WrappingExpression : CSharpSyntaxRewriter
    {
        /// <summary>
        ///     The semantic model that resolves types, symbols, operations and constant values of the original tree.
        /// </summary>
        private readonly SemanticModel _semanticModel;

        /// <summary>
        ///     A value indicating whether the current position is inside a <see langword="checked" />
        ///     context, in which the wrapping helpers must not be introduced.
        /// </summary>
        private bool _checkedContext;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rewriter011_WrappingExpression" /> class.
        /// </summary>
        /// <param name="semanticModel">The semantic model of the document to rewrite.</param>
        public Rewriter011_WrappingExpression(SemanticModel semanticModel) => _semanticModel = semanticModel;

        /// <summary>
        ///     Rewrites an implicit numeric conversion into an explicit call of the conversion helper.
        /// </summary>
        /// <param name="node">The node to rewrite.</param>
        /// <returns>The rewritten node, or the node visited by the base implementation.</returns>
        /// <remarks>
        ///     A conversion that has to stay a compile-time constant is kept as written, because a helper
        ///     call is never a constant. A constant anywhere else is written with the literal suffix of its
        ///     target type when one exists. A conversion with no available helper keeps its implicit form,
        ///     because the helper matrix of the library is partial. A conversion inside an explicit
        ///     <see langword="checked" /> context is kept as written, because replacing it with a helper
        ///     would suppress the overflow exception the context asks for.
        /// </remarks>
        public override SyntaxNode? Visit(SyntaxNode? node)
        {
            if (node is not ExpressionSyntax expression || _checkedContext || !CanReplaceImplicitConversion(expression) || IsWrappingHelper(expression) || !TryGetImplicitNumericConversion(expression, out var targetTypeName, out var sourceTypeName))
                return base.Visit(node);

            var visitedExpression = (ExpressionSyntax?)base.Visit(node);
            if (visitedExpression == null)
                return null;

            if (ArithmeticFacts.IsRequiredConstantContext(expression))
                return visitedExpression.WithTriviaFrom(expression);

            if (LiteralFacts.TryCreateConstantConversion(visitedExpression, targetTypeName, out var constantExpression))
                return constantExpression.WithTriviaFrom(expression);

            return CanConvert(targetTypeName, sourceTypeName) ? CreateFromUncheckedCall(visitedExpression, targetTypeName, sourceTypeName).WithTriviaFrom(expression) : visitedExpression.WithTriviaFrom(expression);
        }

        /// <summary>
        ///     Determines whether the implicit conversion of the expression may be replaced with a helper call.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the conversion may be replaced; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The name part of a member access or member binding is bound against
        ///     the type of its receiver, so it must never be wrapped in a helper call.
        /// </remarks>
        private static bool CanReplaceImplicitConversion(ExpressionSyntax expression) => !(expression.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Name == expression) && !(expression.Parent is MemberBindingExpressionSyntax memberBinding && memberBinding.Name == expression);

        /// <summary>
        ///     Rewrites a member access without replacing the implicit conversion of its name part.
        /// </summary>
        /// <param name="node">The member access to rewrite.</param>
        /// <returns>The rewritten member access.</returns>
        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var expression = (ExpressionSyntax)Visit(node.Expression)!;
            var name = (SimpleNameSyntax)base.Visit(node.Name);
            return node.Update(expression, node.OperatorToken, name);
        }

        /// <summary>
        ///     Rewrites a member binding without replacing the implicit conversion of its name part.
        /// </summary>
        /// <param name="node">The member binding to rewrite.</param>
        /// <returns>The rewritten member binding.</returns>
        public override SyntaxNode VisitMemberBindingExpression(MemberBindingExpressionSyntax node) => node.Update(node.OperatorToken, (SimpleNameSyntax)base.Visit(node.Name));

        /// <summary>
        ///     Rewrites the operand of a <see langword="checked" /> or <see langword="unchecked" /> expression.
        /// </summary>
        /// <param name="node">
        ///     The <see langword="checked" /> or <see langword="unchecked" /> expression to rewrite.
        /// </param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     An <see langword="unchecked" /> expression is removed, because the wrapping helpers
        ///     replace it and the project-wide checked context has to apply to everything else. The
        ///     parentheses that are required to keep the precedence of the rewritten operand are added
        ///     by this method, as the rewriter cannot know the context that the removed wrapper provided.
        /// </remarks>
        public override SyntaxNode? VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            if (IsWrappingHelper(node))
                return base.VisitCheckedExpression(node);

            var previous = _checkedContext;
            _checkedContext = node.IsKind(SyntaxKind.CheckedExpression);
            var expression = (ExpressionSyntax)Visit(node.Expression)!;
            _checkedContext = previous;

            if (node.IsKind(SyntaxKind.CheckedExpression))
                return node.WithExpression(expression);

            return IsPrimaryExpression(expression) ? expression.WithTriviaFrom(node) : SyntaxFactory.ParenthesizedExpression(expression).WithTriviaFrom(node);
        }

        /// <summary>
        ///     Rewrites the block of a <see langword="checked" /> or <see langword="unchecked" /> statement.
        /// </summary>
        /// <param name="node">
        ///     The <see langword="checked" /> or <see langword="unchecked" /> statement to rewrite.
        /// </param>
        /// <returns>The rewritten statement.</returns>
        /// <remarks>
        ///     An <see langword="unchecked" /> block is replaced with its content, because the wrapping helpers
        ///     replace the block and the project-wide checked context has to apply to the statements around it as well.
        /// </remarks>
        public override SyntaxNode? VisitCheckedStatement(CheckedStatementSyntax node)
        {
            if (IsWrappingHelper(node))
                return base.VisitCheckedStatement(node);

            var previous = _checkedContext;
            _checkedContext = node.IsKind(SyntaxKind.CheckedStatement);
            var block = (BlockSyntax)Visit(node.Block)!;
            _checkedContext = previous;

            return node.IsKind(SyntaxKind.CheckedStatement) ? node.WithBlock(block) : block.WithTriviaFrom(node);
        }

        /// <summary>
        ///     Replaces an integral addition, subtraction or multiplication with the wrapping helper.
        /// </summary>
        /// <param name="node">The binary expression to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     The operands are converted to the type of the operation first, because
        ///     the helper that is called is the one of that type. The operation is left
        ///     untouched when the helper, or one of the conversions it needs, does not exist.
        /// </remarks>
        public override SyntaxNode? VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (_checkedContext || IsWrappingHelper(node) || ArithmeticFacts.IsCheckedSizeArithmetic(node, _semanticModel) || !TryGetWrappingOperation(node, out var methodName, out var resultType))
                return base.VisitBinaryExpression(node);

            var resultTypeName = GetWrappingTypeName(resultType)!;
            var leftTypeName = GetReceiverTypeName(node.Left);
            var rightTypeName = GetWrappingTypeName(node.Right);
            if (!NumericFacts.HasBinaryWrappingHelper(_semanticModel.Compilation, resultTypeName, methodName) || !CanConvert(resultTypeName, leftTypeName) || !CanConvert(resultTypeName, rightTypeName))
                return base.VisitBinaryExpression(node);

            var left = (ExpressionSyntax)Visit(node.Left)!;
            var right = (ExpressionSyntax)Visit(node.Right)!;
            return CreateCall(ConvertOperand(left, resultTypeName, leftTypeName), methodName, ConvertOperand(right, resultTypeName, rightTypeName)).WithTriviaFrom(node);
        }

        /// <summary>
        ///     Replaces a unary minus or a prefix increment with the wrapping helper.
        /// </summary>
        /// <param name="node">The prefix unary expression to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     The operand of a unary minus is cast to the type of the operation when the language promotes
        ///     it, because the negation of a small integral type is performed on its promoted type.
        /// </remarks>
        public override SyntaxNode? VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (_checkedContext || IsWrappingHelper(node))
                return base.VisitPrefixUnaryExpression(node);

            if (node.IsKind(SyntaxKind.UnaryMinusExpression) && !ArithmeticFacts.IsCheckedSizeArithmetic(node, _semanticModel) && TryGetWrappingNeg(node, out var resultType))
            {
                var resultTypeName = GetWrappingTypeName(resultType)!;
                if (!NumericFacts.HasWrappingNegHelper(_semanticModel.Compilation, resultTypeName))
                    return base.VisitPrefixUnaryExpression(node);

                var operand = (ExpressionSyntax)Visit(node.Operand)!;
                var operandType = _semanticModel.GetTypeInfo(node.Operand).Type;
                if (operandType != null && operandType.SpecialType != resultType.SpecialType)
                    operand = SyntaxFactory.CastExpression(GetTypeSyntax(resultType), operand);

                return CreateCall(operand, "WrappingNeg").WithTriviaFrom(node);
            }

            if ((node.IsKind(SyntaxKind.PreIncrementExpression) || node.IsKind(SyntaxKind.PreDecrementExpression)) && !ArithmeticFacts.IsCheckedSizeArithmetic(node, _semanticModel))
                return RewriteIncrement(node, node.Operand, true, node.IsKind(SyntaxKind.PreIncrementExpression));

            return base.VisitPrefixUnaryExpression(node);
        }

        /// <summary>
        ///     Replaces a postfix increment with the wrapping helper.
        /// </summary>
        /// <param name="node">The postfix unary expression to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        public override SyntaxNode? VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            if (_checkedContext || IsWrappingHelper(node))
                return base.VisitPostfixUnaryExpression(node);

            if ((node.IsKind(SyntaxKind.PostIncrementExpression) || node.IsKind(SyntaxKind.PostDecrementExpression)) && !ArithmeticFacts.IsCheckedSizeArithmetic(node, _semanticModel))
                return RewriteIncrement(node, node.Operand, false, node.IsKind(SyntaxKind.PostIncrementExpression));

            return base.VisitPostfixUnaryExpression(node);
        }

        /// <summary>
        ///     Replaces an integral compound assignment with an assignment to the same target and a wrapping call.
        /// </summary>
        /// <param name="node">The assignment expression to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     The left operand is written twice, so the target must be a stable location. The arithmetic is performed
        ///     in the promoted type of the target, and the result is converted back to the target type afterwards.
        /// </remarks>
        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (_checkedContext || IsWrappingHelper(node) || ArithmeticFacts.IsCheckedSizeArithmetic(node, _semanticModel) || !TryGetWrappingAssignment(node, out var methodName, out var targetType) || !CanDuplicateLValue(node.Left))
                return base.VisitAssignmentExpression(node);

            var targetTypeName = GetWrappingTypeName(targetType)!;
            var arithmeticType = GetArithmeticType(targetType);
            var arithmeticTypeName = GetWrappingTypeName(arithmeticType)!;
            var rightTypeName = GetWrappingTypeName(node.Right);
            if (!NumericFacts.HasBinaryWrappingHelper(_semanticModel.Compilation, arithmeticTypeName, methodName) || !CanConvert(arithmeticTypeName, targetTypeName) || !CanConvert(arithmeticTypeName, rightTypeName) || !CanConvert(targetTypeName, arithmeticTypeName))
                return base.VisitAssignmentExpression(node);

            var left = ((ExpressionSyntax)Visit(node.Left)!).WithoutTrivia();
            var right = ((ExpressionSyntax)Visit(node.Right)!).WithoutTrivia();
            ExpressionSyntax value = CreateCall(ConvertOperand(left, arithmeticTypeName, targetTypeName), methodName, ConvertOperand(right, arithmeticTypeName, rightTypeName));
            if (arithmeticType.SpecialType != targetType.SpecialType)
                value = CreateFromUncheckedCall(value, targetTypeName, arithmeticTypeName);

            return CreateAssignment(left, value).WithTriviaFrom(node);
        }

        /// <summary>
        ///     Rewrites a cast into a call of the matching conversion helper.
        /// </summary>
        /// <param name="node">The cast expression to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     A pointer is converted through <c>Isize.FromPtrUnchecked</c> and, when the target is not
        ///     the native-sized signed type, through the conversion helper of the target type. A cast
        ///     whose value has to stay a compile-time constant is written as a constant expression, and
        ///     every other cast is converted between the two wrapper types. A cast inside an explicit
        ///     <see langword="checked" /> context is kept as written so that it keeps throwing on overflow.
        /// </remarks>
        public override SyntaxNode? VisitCastExpression(CastExpressionSyntax node)
        {
            if (_checkedContext || IsWrappingHelper(node))
                return base.VisitCastExpression(node);

            if (TryGetPointerConversion(node, out var pointerTargetTypeName))
            {
                var pointerValue = CreateFromPtrUncheckedCall((ExpressionSyntax)Visit(node.Expression)!);
                return pointerTargetTypeName == SR.ISIZE_TYPE_NAME ? pointerValue.WithTriviaFrom(node) : CreateFromUncheckedCall(pointerValue, pointerTargetTypeName, SR.ISIZE_TYPE_NAME).WithTriviaFrom(node);
            }

            if (_semanticModel.GetConstantValue(node) is { HasValue: true, Value: { } constantValue } && ArithmeticFacts.IsRequiredConstantContext(node))
                return _semanticModel.GetOperation(node) is IConversionOperation { Type: { } constantTargetType } && TryCreateConstantExpression(constantValue, constantTargetType, out var constantExpression) ? constantExpression.WithTriviaFrom(node) : base.VisitCastExpression(node);

            if (!TryGetWrappingConversion(node, out var targetTypeName, out var sourceTypeName))
                return base.VisitCastExpression(node);

            return CreateFromUncheckedCall((ExpressionSyntax)Visit(node.Expression)!, targetTypeName, sourceTypeName).WithTriviaFrom(node);
        }

        /// <summary>
        ///     Rewrites a call of a helper whose argument does not have the type the helper expects.
        /// </summary>
        /// <param name="node">The invocation to rewrite.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     A nested call of the same helper is reduced to a single call, because the outer one only
        ///     repeats the conversion. A call whose argument already has the return type of the helper
        ///     is removed as redundant. Finally, the arguments of a wrapping call are converted so that
        ///     both operands use the type of the operation, which is what the helper is defined for.
        /// </remarks>
        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.Expression is not MemberAccessExpressionSyntax memberAccess)
                return base.VisitInvocationExpression(node);

            var name = memberAccess.Name.Identifier.ValueText;
            if (name.StartsWith("From", StringComparison.Ordinal) && name.EndsWith("Unchecked", StringComparison.Ordinal) && node.ArgumentList.Arguments.Count == 1 && node.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax nestedInvocation && nestedInvocation.Expression is MemberAccessExpressionSyntax nestedMemberAccess && nestedMemberAccess.Name.Identifier.ValueText == name && nestedMemberAccess.Expression.ToString() == memberAccess.Expression.ToString())
                return Visit(nestedInvocation)?.WithTriviaFrom(node);

            if (name.StartsWith("From", StringComparison.Ordinal) && name.EndsWith("Unchecked", StringComparison.Ordinal) && node.ArgumentList.Arguments.Count == 1 && _semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol conversionMethod && _semanticModel.GetTypeInfo(node.ArgumentList.Arguments[0].Expression).Type is { } conversionArgumentType && SymbolEqualityComparer.Default.Equals(conversionMethod.ReturnType, conversionArgumentType))
                return Visit(node.ArgumentList.Arguments[0].Expression)?.WithTriviaFrom(node);

            if (_checkedContext || IsWrappingHelper(node))
                return base.VisitInvocationExpression(node);

            if (name is not ("WrappingAdd" or "WrappingSub" or "WrappingMul") || node.ArgumentList.Arguments.Count != 1 || !TryGetWrappingBinaryResultType(memberAccess.Expression, node.ArgumentList.Arguments[0].Expression, name, out var binaryResultType))
                return base.VisitInvocationExpression(node);

            var resultTypeName = GetWrappingTypeName(binaryResultType);
            var leftTypeName = GetReceiverTypeName(memberAccess.Expression);
            var rightTypeName = GetWrappingTypeName(node.ArgumentList.Arguments[0].Expression);
            if (resultTypeName == null || leftTypeName == null || rightTypeName == null || (leftTypeName == resultTypeName && rightTypeName == resultTypeName) || !NumericFacts.HasBinaryWrappingHelper(_semanticModel.Compilation, resultTypeName, name) || !CanConvert(resultTypeName, leftTypeName) || !CanConvert(resultTypeName, rightTypeName))
                return base.VisitInvocationExpression(node);

            var left = (ExpressionSyntax)Visit(memberAccess.Expression)!;
            var right = (ExpressionSyntax)Visit(node.ArgumentList.Arguments[0].Expression)!;
            return CreateCall(ConvertOperand(left, resultTypeName, leftTypeName), name, ConvertOperand(right, resultTypeName, rightTypeName)).WithTriviaFrom(node);
        }

        /// <summary>
        ///     Rewrites an increment or decrement operator that is an operation on a stable target.
        /// </summary>
        /// <param name="node">The increment or decrement node.</param>
        /// <param name="operand">The operand of the operator.</param>
        /// <param name="isPrefix">A value indicating whether the operator is written before its operand.</param>
        /// <param name="isIncrement">A value indicating whether the operator increments rather than decrements.</param>
        /// <returns>The rewritten expression.</returns>
        private SyntaxNode? RewriteIncrement(SyntaxNode node, ExpressionSyntax operand, bool isPrefix, bool isIncrement) => _semanticModel.GetOperation(node) is IIncrementOrDecrementOperation { OperatorMethod: null } operation ? RewriteIncrementCore(node, operand, isPrefix, isIncrement, operation.Type!) : VisitIncrementWithoutRewrite(node);

        /// <summary>
        ///     Visits an increment or decrement with the base implementation of the matching node kind.
        /// </summary>
        /// <param name="node">The prefix or postfix increment or decrement node.</param>
        /// <returns>
        ///     The node visited by the base implementation, or
        ///     <see langword="null" /> when the base visit removes the node.
        /// </returns>
        /// <remarks>
        ///     Calling <c>base.Visit(node)</c> would dispatch back to the override that requested the rewrite
        ///     and recurse until the stack overflows, so the base visit is selected by the node kind instead.
        /// </remarks>
        private SyntaxNode? VisitIncrementWithoutRewrite(SyntaxNode node) => node is PrefixUnaryExpressionSyntax prefix ? base.VisitPrefixUnaryExpression(prefix) : base.VisitPostfixUnaryExpression((PostfixUnaryExpressionSyntax)node);

        /// <summary>
        ///     Builds the assignment that implements an increment or decrement with a wrapping helper.
        /// </summary>
        /// <param name="node">The increment or decrement node.</param>
        /// <param name="operand">The operand of the operator.</param>
        /// <param name="isPrefix">A value indicating whether the operator is written before its operand.</param>
        /// <param name="isIncrement">A value indicating whether the operator increments rather than decrements.</param>
        /// <param name="targetType">The type of the operand.</param>
        /// <returns>The rewritten expression.</returns>
        /// <remarks>
        ///     When the value of the expression is used, the postfix form reconstructs the value before
        ///     the update by applying the inverse operation to the updated value, and both forms return
        ///     the updated value for a prefix operator. The target is written twice, so it must be stable,
        ///     and the reference that has to be read back is evaluated again for every reconstruction.
        /// </remarks>
        private SyntaxNode? RewriteIncrementCore(SyntaxNode node, ExpressionSyntax operand, bool isPrefix, bool isIncrement, ITypeSymbol targetType)
        {
            if (GetWrappingTypeName(targetType) == null || !CanDuplicateLValue(operand))
                return VisitIncrementWithoutRewrite(node);

            var arithmeticType = GetArithmeticType(targetType);
            var targetTypeName = GetWrappingTypeName(targetType)!;
            var arithmeticTypeName = GetWrappingTypeName(arithmeticType)!;
            var operationName = isIncrement ? "WrappingAdd" : "WrappingSub";
            if (!NumericFacts.HasBinaryWrappingHelper(_semanticModel.Compilation, arithmeticTypeName, operationName) || !CanConvert(arithmeticTypeName, targetTypeName) || !CanConvert(arithmeticTypeName, SR.INT32_TYPE_NAME) || !CanConvert(targetTypeName, arithmeticTypeName))
                return VisitIncrementWithoutRewrite(node);

            var left = ((ExpressionSyntax)Visit(operand)!).WithoutTrivia();
            ExpressionSyntax one = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));
            one = ConvertOperand(one, arithmeticTypeName, SR.INT32_TYPE_NAME);
            ExpressionSyntax value = CreateCall(ConvertOperand(left, arithmeticTypeName, targetTypeName), operationName, one);
            if (arithmeticType.SpecialType != targetType.SpecialType)
                value = CreateFromUncheckedCall(value, targetTypeName, arithmeticTypeName);

            var assignment = CreateAssignment(left, value);
            if (IsValueDiscarded(node))
                return assignment.WithTriviaFrom(node);

            if (isPrefix)
                return SyntaxFactory.ParenthesizedExpression(assignment).WithTriviaFrom(node);

            ExpressionSyntax oldValue = SyntaxFactory.ParenthesizedExpression(assignment);
            if (arithmeticType.SpecialType != targetType.SpecialType)
                oldValue = CreateFromUncheckedCall(oldValue, arithmeticTypeName, targetTypeName);

            oldValue = CreateCall(oldValue, isIncrement ? "WrappingSub" : "WrappingAdd", one);
            if (arithmeticType.SpecialType != targetType.SpecialType)
                oldValue = CreateFromUncheckedCall(oldValue, targetTypeName, arithmeticTypeName);

            return oldValue.WithTriviaFrom(node);
        }

        /// <summary>
        ///     Gets the wrapping helper name and the result type of a binary arithmetic expression.
        /// </summary>
        /// <param name="node">The binary expression to classify.</param>
        /// <param name="methodName">The name of the wrapping helper that implements the operation.</param>
        /// <param name="resultType">The type of the operation.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is a wrappable
        ///     integral operation; otherwise, <see langword="false" />.
        /// </returns>
        private bool TryGetWrappingOperation(BinaryExpressionSyntax node, out string methodName, out ITypeSymbol resultType)
        {
            methodName = node.Kind() switch
            {
                SyntaxKind.AddExpression => "WrappingAdd",
                SyntaxKind.SubtractExpression => "WrappingSub",
                SyntaxKind.MultiplyExpression => "WrappingMul",
                _ => string.Empty
            };

            resultType = null!;
            if (methodName.Length == 0 || ArithmeticFacts.MustKeepConstantExpression(node, _semanticModel) || _semanticModel.GetOperation(node) is not IBinaryOperation { OperatorMethod: null, Type: { } type } || GetWrappingTypeName(type) == null)
                return false;

            resultType = type;
            return true;
        }

        /// <summary>
        ///     Gets the result type of a unary minus that can be wrapping.
        /// </summary>
        /// <param name="node">The unary expression to classify.</param>
        /// <param name="resultType">The type of the operation.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is a wrappable
        ///     negation; otherwise, <see langword="false" />.
        /// </returns>
        private bool TryGetWrappingNeg(PrefixUnaryExpressionSyntax node, out ITypeSymbol resultType)
        {
            resultType = null!;
            if (ArithmeticFacts.MustKeepConstantExpression(node, _semanticModel) || _semanticModel.GetOperation(node) is not IUnaryOperation { OperatorMethod: null, Type: { } type } || GetWrappingTypeName(type) == null)
                return false;

            resultType = type;
            return true;
        }

        /// <summary>
        ///     Gets the wrapping helper name and the target type of a compound assignment.
        /// </summary>
        /// <param name="node">The assignment expression to classify.</param>
        /// <param name="methodName">
        ///     The name of the wrapping helper that implements the arithmetic of the assignment.
        /// </param>
        /// <param name="targetType">The type of the assignment result.</param>
        /// <returns>
        ///     <see langword="true" /> when the assignment is a wrappable
        ///     integral operation; otherwise, <see langword="false" />.
        /// </returns>
        private bool TryGetWrappingAssignment(AssignmentExpressionSyntax node, out string methodName, out ITypeSymbol targetType)
        {
            methodName = node.Kind() switch
            {
                SyntaxKind.AddAssignmentExpression => "WrappingAdd",
                SyntaxKind.SubtractAssignmentExpression => "WrappingSub",
                SyntaxKind.MultiplyAssignmentExpression => "WrappingMul",
                _ => string.Empty
            };

            targetType = null!;
            if (methodName.Length == 0 || _semanticModel.GetOperation(node) is not ICompoundAssignmentOperation { OperatorMethod: null, Type: { } type } || GetWrappingTypeName(type) == null)
                return false;

            targetType = type;
            return true;
        }

        /// <summary>
        ///     Determines whether the expression can be written twice without changing the behavior of the code.
        /// </summary>
        /// <param name="expression">The expression to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression may be duplicated; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Only a stable local, parameter or writable field, and a field or array element
        ///     whose receiver is itself side effect free, are considered stable, because a
        ///     property access or an indexer call may run arbitrary code for every evaluation.
        /// </remarks>
        private bool CanDuplicateLValue(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case ParenthesizedExpressionSyntax parenthesized:
                    return CanDuplicateLValue(parenthesized.Expression);

                case IdentifierNameSyntax identifier:
                    return IsStableVariable(_semanticModel.GetSymbolInfo(identifier).Symbol);

                case MemberAccessExpressionSyntax memberAccess when memberAccess.IsKind(SyntaxKind.PointerMemberAccessExpression):
                    return IsSideEffectFree(memberAccess.Expression);

                case MemberAccessExpressionSyntax memberAccess:
                    if (_semanticModel.GetSymbolInfo(memberAccess).Symbol is not IFieldSymbol)
                        return false;
                    return memberAccess.Expression.IsKind(SyntaxKind.ThisExpression) || memberAccess.Expression.IsKind(SyntaxKind.BaseExpression) || IsSideEffectFree(memberAccess.Expression);

                case PrefixUnaryExpressionSyntax pointerDereference when pointerDereference.IsKind(SyntaxKind.PointerIndirectionExpression):
                    return IsSideEffectFree(pointerDereference.Operand);

                case ElementAccessExpressionSyntax elementAccess:
                    return _semanticModel.GetTypeInfo(elementAccess.Expression).Type is IArrayTypeSymbol && IsSideEffectFree(elementAccess.Expression) && elementAccess.ArgumentList.Arguments.All(argument => IsSideEffectFree(argument.Expression));

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Determines whether evaluating the expression has no observable side effect.
        /// </summary>
        /// <param name="expression">The expression to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the evaluation is free of
        ///     side effects; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Literals, stable variables, fields, the instance reference and expressions built only
        ///     from those are side effect free. Calls, object creations, increments and assignments are
        ///     not, and neither are property accesses or indexer calls, because they may run user code.
        /// </remarks>
        private bool IsSideEffectFree(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case IdentifierNameSyntax identifier:
                    return IsStableVariable(_semanticModel.GetSymbolInfo(identifier).Symbol);

                case LiteralExpressionSyntax:
                    return true;

                case ThisExpressionSyntax:
                case BaseExpressionSyntax:
                    return true;

                case ParenthesizedExpressionSyntax parenthesized:
                    return IsSideEffectFree(parenthesized.Expression);

                case CastExpressionSyntax cast:
                    return IsSideEffectFree(cast.Expression);

                case PrefixUnaryExpressionSyntax prefix:
                    return !prefix.IsKind(SyntaxKind.PreIncrementExpression) && !prefix.IsKind(SyntaxKind.PreDecrementExpression) && IsSideEffectFree(prefix.Operand);

                case BinaryExpressionSyntax binary:
                    return IsSideEffectFree(binary.Left) && IsSideEffectFree(binary.Right);

                case MemberAccessExpressionSyntax member:
                    return _semanticModel.GetSymbolInfo(member).Symbol is IFieldSymbol && IsSideEffectFree(member.Expression);

                case ElementAccessExpressionSyntax elementAccess:
                    return _semanticModel.GetTypeInfo(elementAccess.Expression).Type is IArrayTypeSymbol && IsSideEffectFree(elementAccess.Expression) && elementAccess.ArgumentList.Arguments.All(argument => IsSideEffectFree(argument.Expression));

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Determines whether the symbol denotes a variable that can be read twice with the same value.
        /// </summary>
        /// <param name="symbol">The symbol to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the symbol is a local, a parameter
        ///     or a writable field; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsStableVariable(ISymbol? symbol) => symbol is ILocalSymbol or IParameterSymbol || symbol is IFieldSymbol { IsReadOnly: false };

        /// <summary>
        ///     Determines whether the value of the expression is discarded by the surrounding statement.
        /// </summary>
        /// <param name="node">The expression to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the value is not observed; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsValueDiscarded(SyntaxNode node) => (node.Parent is ExpressionStatementSyntax statement && statement.Expression == node) || (node.Parent is ForStatementSyntax forStatement && forStatement.Incrementors.Contains((ExpressionSyntax)node));

        /// <summary>
        ///     Gets the type in which the arithmetic on the given type is performed.
        /// </summary>
        /// <param name="type">The type of the value that the arithmetic is applied to.</param>
        /// <returns>The type of the arithmetic operation.</returns>
        /// <remarks>
        ///     The language promotes the small integral types to <see cref="int" /> for binary
        ///     arithmetic, and the library declares the wrapping helpers for exactly those promoted types.
        /// </remarks>
        private ITypeSymbol GetArithmeticType(ITypeSymbol type) => type.SpecialType is SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 ? _semanticModel.Compilation.GetSpecialType(SpecialType.System_Int32) : type;

        /// <summary>
        ///     Gets the name of the wrapper type that declares the wrapping helpers for the type of the expression.
        /// </summary>
        /// <param name="expression">The expression whose wrapper type is requested.</param>
        /// <returns>
        ///     The wrapper type name, or <see langword="null" /> when the expression has no wrapper type.
        /// </returns>
        /// <remarks>
        ///     The type of the expression is resolved from the converted type first, because an argument or an
        ///     operand already has the type it is converted to. When that fails, the initializer of the local
        ///     variable that the expression refers to and the return type of the called helper are used, and
        ///     finally the type that the equivalent binary operation would have is computed speculatively.
        /// </remarks>
        private string? GetWrappingTypeName(ExpressionSyntax expression)
        {
            var typeInfo = _semanticModel.GetTypeInfo(expression);
            var typeName = GetWrappingTypeName(typeInfo.ConvertedType) ?? GetWrappingTypeName(typeInfo.Type);
            if (typeName != null)
                return typeName;

            if (expression is ParenthesizedExpressionSyntax parenthesized)
                return GetWrappingTypeName(parenthesized.Expression);

            if (expression is IdentifierNameSyntax identifier && _semanticModel.GetSymbolInfo(identifier).Symbol is ILocalSymbol local && local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                return GetWrappingTypeName(initializer.Value);

            if (expression is not InvocationExpressionSyntax invocation || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return null;

            if (_semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method && GetWrappingTypeName(method.ReturnType) is { } returnTypeName)
                return returnTypeName;

            var methodName = memberAccess.Name.Identifier.ValueText;
            return methodName is "WrappingAdd" or "WrappingSub" or "WrappingMul" && invocation.ArgumentList.Arguments.Count == 1 && TryGetWrappingBinaryResultType(memberAccess.Expression, invocation.ArgumentList.Arguments[0].Expression, methodName, out var binaryResultType) ? GetWrappingTypeName(binaryResultType) : null;
        }

        /// <summary>
        ///     Gets the name of the wrapper type that declares the wrapping helpers for the given type.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <returns>The wrapper type name, or <see langword="null" /> for a type without wrapping helpers.</returns>
        /// <remarks>
        ///     Only the integral types are mapped, because the library declares no wrapping helpers
        ///     for the floating point types, for <see cref="decimal" /> and for <see cref="char" />.
        /// </remarks>
        private static string? GetWrappingTypeName(ITypeSymbol? type) => type?.SpecialType switch
        {
            SpecialType.System_Byte => "U8",
            SpecialType.System_UInt16 => "U16",
            SpecialType.System_UInt32 => "U32",
            SpecialType.System_UInt64 => "U64",
            SpecialType.System_UIntPtr => "Usize",
            SpecialType.System_SByte => "I8",
            SpecialType.System_Int16 => "I16",
            SpecialType.System_Int32 => "I32",
            SpecialType.System_Int64 => "I64",
            SpecialType.System_IntPtr => "Isize",
            _ => null
        };

        /// <summary>
        ///     Gets the type that the binary operation behind a wrapping call would have.
        /// </summary>
        /// <param name="left">The left operand of the wrapping call.</param>
        /// <param name="right">The right operand of the wrapping call.</param>
        /// <param name="methodName">The name of the wrapping helper.</param>
        /// <param name="resultType">The type of the equivalent binary operation.</param>
        /// <returns>
        ///     <see langword="true" /> when the type could be determined; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The operation is built speculatively from the two operands, so the result reflects
        ///     the conversions that the language would apply. When the speculative operation has no
        ///     wrapper type, the promoted <see cref="int" /> of two small integral operands is assumed.
        /// </remarks>
        private bool TryGetWrappingBinaryResultType(ExpressionSyntax left, ExpressionSyntax right, string methodName, out ITypeSymbol resultType)
        {
            resultType = null!;
            var binaryKind = methodName switch
            {
                "WrappingAdd" => SyntaxKind.AddExpression,
                "WrappingSub" => SyntaxKind.SubtractExpression,
                "WrappingMul" => SyntaxKind.MultiplyExpression,
                _ => SyntaxKind.None
            };

            if (binaryKind == SyntaxKind.None)
                return false;

            var binary = SyntaxFactory.BinaryExpression(binaryKind, left.WithoutTrivia(), right.WithoutTrivia());
            var speculativeType = _semanticModel.GetSpeculativeTypeInfo(left.SpanStart, binary, SpeculativeBindingOption.BindAsExpression).Type;
            if (GetWrappingTypeName(speculativeType) != null)
            {
                resultType = speculativeType!;
                return true;
            }

            if (!NumericFacts.IsSmallIntegral(_semanticModel.GetTypeInfo(left).Type) || !NumericFacts.IsSmallIntegral(_semanticModel.GetTypeInfo(right).Type))
                return false;

            resultType = _semanticModel.Compilation.GetSpecialType(SpecialType.System_Int32);
            return true;
        }

        /// <summary>
        ///     Gets the two wrapper types of a cast that converts between numbers.
        /// </summary>
        /// <param name="node">The cast expression to classify.</param>
        /// <param name="targetTypeName">The name of the wrapper type that the cast converts to.</param>
        /// <param name="sourceTypeName">The name of the wrapper type that the cast converts from.</param>
        /// <returns>
        ///     <see langword="true" /> when the cast is a numeric conversion; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The conversion is only accepted when both types have a wrapper type, when the cast can
        ///     lose information, and when the target wrapper type declares the matching conversion
        ///     helper. A widening cast and a cast of a constant never wrap and therefore keep their form.
        /// </remarks>
        private bool TryGetWrappingConversion(CastExpressionSyntax node, out string targetTypeName, out string sourceTypeName)
        {
            targetTypeName = string.Empty;
            sourceTypeName = string.Empty;
            if (_semanticModel.GetOperation(node) is not IConversionOperation { OperatorMethod: null, Operand.Type: { } sourceType, Type: { } targetType })
                return false;

            var targetName = NumericFacts.GetHelperTypeName(targetType);
            var sourceName = NumericFacts.GetHelperTypeName(sourceType);
            if (sourceName == null || targetName == null || sourceName == targetName || !NumericFacts.CanOverflow(sourceType, targetType) || !NumericFacts.HasUncheckedConversionHelper(_semanticModel.Compilation, targetName, sourceName))
                return false;

            targetTypeName = targetName;
            sourceTypeName = sourceName;
            return true;
        }

        /// <summary>
        ///     Gets the wrapper type that a pointer cast converts to.
        /// </summary>
        /// <param name="node">The cast expression to classify.</param>
        /// <param name="targetTypeName">The name of the wrapper type that the cast converts to.</param>
        /// <returns>
        ///     <see langword="true" /> when the cast converts a pointer
        ///     into a numeric type; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The conversion is expressed with <c>Isize.FromPtrUnchecked</c> and, for a target other than
        ///     <c>Isize</c>, with the conversion helper of that target, so both helpers have to exist.
        /// </remarks>
        private bool TryGetPointerConversion(CastExpressionSyntax node, out string targetTypeName)
        {
            targetTypeName = string.Empty;
            if (_semanticModel.GetOperation(node) is not IConversionOperation { OperatorMethod: null, Operand.Type: { TypeKind: TypeKind.Pointer }, Type: { } targetType })
                return false;

            var targetName = NumericFacts.GetHelperTypeName(targetType);
            if (targetName == null || !NumericFacts.HasUncheckedConversionHelper(_semanticModel.Compilation, SR.ISIZE_TYPE_NAME, SR.PTR_TYPE_NAME) || (targetName != SR.ISIZE_TYPE_NAME && !NumericFacts.HasUncheckedConversionHelper(_semanticModel.Compilation, targetName, SR.ISIZE_TYPE_NAME)))
                return false;

            targetTypeName = targetName;
            return true;
        }

        /// <summary>
        ///     Gets the two wrapper types of an implicit conversion between numbers.
        /// </summary>
        /// <param name="expression">The expression to classify.</param>
        /// <param name="targetTypeName">The name of the wrapper type that the expression is converted to.</param>
        /// <param name="sourceTypeName">The name of the wrapper type of the expression itself.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is implicitly
        ///     converted between numbers; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The expression is described by its own type and by the type it is converted to, because a conversion
        ///     of an expression that is not a numeric conversion in the language sense is still a numeric conversion
        ///     of the analyzed library. A cast operand is excluded, since the cast is rewritten on its own, and
        ///     so is a conversion that cannot lose information, because it never wraps. A constant expression is
        ///     included, because a folded conversion is still a numeric conversion that the library writes
        ///     explicitly; the caller decides whether it can use a suffix or has to stay constant.
        /// </remarks>
        private bool TryGetImplicitNumericConversion(ExpressionSyntax expression, out string targetTypeName, out string sourceTypeName)
        {
            targetTypeName = string.Empty;
            sourceTypeName = string.Empty;
            if (expression.Parent is CastExpressionSyntax cast && cast.Expression == expression)
                return false;

            var typeInfo = _semanticModel.GetTypeInfo(expression);
            var conversion = _semanticModel.GetConversion(expression);
            var targetName = NumericFacts.GetHelperTypeName(typeInfo.ConvertedType);
            var sourceName = NumericFacts.GetHelperTypeName(typeInfo.Type);
            if (sourceName == null || targetName == null || sourceName == targetName || !conversion.IsImplicit || conversion.IsIdentity)
                return false;

            targetTypeName = targetName;
            sourceTypeName = sourceName;
            return true;
        }

        /// <summary>
        ///     Writes the value of a constant cast as a literal of the target type.
        /// </summary>
        /// <param name="value">The constant value to write.</param>
        /// <param name="targetType">The type that the value is converted to.</param>
        /// <param name="expression">The literal that represents the value.</param>
        /// <returns>
        ///     <see langword="true" /> when the literal was written; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The value is rendered with the invariant culture, so the literal is independent from the culture
        ///     of the build. The small integral types have no literal suffix and are therefore written as a cast
        ///     of the literal, and a native-sized target is rejected because it has no constant representation.
        /// </remarks>
        private static bool TryCreateConstantExpression(object value, ITypeSymbol targetType, out ExpressionSyntax expression)
        {
            var suffix = targetType.SpecialType switch
            {
                SpecialType.System_UInt32 => "U",
                SpecialType.System_Int64 => "L",
                SpecialType.System_UInt64 => "UL",
                SpecialType.System_Single => "F",
                SpecialType.System_Double => "D",
                SpecialType.System_Decimal => "M",
                _ => string.Empty
            };

            if (targetType.SpecialType == SpecialType.System_Char && value is char character)
            {
                expression = SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(character));
                return true;
            }

            var valueText = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (valueText == null || targetType.SpecialType is SpecialType.System_IntPtr or SpecialType.System_UIntPtr)
            {
                expression = null!;
                return false;
            }

            var literal = SyntaxFactory.ParseExpression(valueText + suffix);
            if (NumericFacts.IsSmallIntegral(targetType))
                literal = SyntaxFactory.CastExpression(GetTypeSyntax(targetType), SyntaxFactory.ParenthesizedExpression(literal));

            expression = literal;
            return true;
        }

        /// <summary>
        ///     Determines whether the node is inside the body of a wrapping helper or an unchecked conversion helper.
        /// </summary>
        /// <param name="node">The node whose enclosing method is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the node is part of a helper body; otherwise, <see langword="false" />.
        /// </returns>
        private bool IsWrappingHelper(SyntaxNode node) => node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } method && HelperFacts.IsWrappingHelper(_semanticModel.GetDeclaredSymbol(method));

        /// <summary>
        ///     Determines whether the operand can be converted to the target wrapper type.
        /// </summary>
        /// <param name="targetTypeName">The name of the wrapper type that the operand has to be converted to.</param>
        /// <param name="sourceTypeName">The name of the wrapper type of the operand, which may be unknown.</param>
        /// <returns>
        ///     <see langword="true" /> when the operand needs no conversion
        ///     or the conversion exists; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     An operand without a wrapper type, or one that already has the target type, needs
        ///     no conversion. A conversion that would need a missing helper makes the caller
        ///     keep the original expression instead of emitting a call that does not compile.
        /// </remarks>
        private bool CanConvert(string targetTypeName, string? sourceTypeName) => sourceTypeName == null || sourceTypeName == targetTypeName || NumericFacts.HasUncheckedConversionHelper(_semanticModel.Compilation, targetTypeName, sourceTypeName);

        /// <summary>
        ///     Wraps the expression in the conversion helper of the target wrapper type when its type differs.
        /// </summary>
        /// <param name="expression">The expression to convert.</param>
        /// <param name="targetTypeName">
        ///     The name of the wrapper type that the expression has to be converted to.
        /// </param>
        /// <param name="sourceTypeName">The name of the wrapper type of the expression, which may be unknown.</param>
        /// <returns>The converted expression, or the original expression when no conversion is required.</returns>
        /// <remarks>
        ///     An operand that the visitor already converted is returned unchanged. The
        ///     visitor rewrites an implicit conversion of the operand before it is passed
        ///     here, so a second conversion call would nest the same helper inside itself.
        /// </remarks>
        private static ExpressionSyntax ConvertOperand(ExpressionSyntax expression, string targetTypeName, string? sourceTypeName) => sourceTypeName == null || sourceTypeName == targetTypeName || IsSameConversionCall(expression, targetTypeName, sourceTypeName) ? expression : CreateFromUncheckedCall(expression, targetTypeName, sourceTypeName);

        /// <summary>
        ///     Determines whether the expression is already the conversion call of the target wrapper type.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <param name="targetTypeName">
        ///     The name of the wrapper type that the expression has to be converted to.
        /// </param>
        /// <param name="sourceTypeName">The name of the wrapper type of the expression.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is the matching
        ///     conversion call; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsSameConversionCall(ExpressionSyntax expression, string targetTypeName, string sourceTypeName) => expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax receiver, Name: IdentifierNameSyntax name } } && receiver.Identifier.Text == targetTypeName && name.Identifier.Text == "From" + sourceTypeName + "Unchecked";

        /// <summary>
        ///     Gets the wrapper type name of the value that the expression actually has.
        /// </summary>
        /// <param name="expression">The expression whose declared wrapper type is requested.</param>
        /// <returns>
        ///     The wrapper type name, or <see langword="null" /> when the expression has no wrapper type.
        /// </returns>
        /// <remarks>
        ///     The conversion the language would apply is deliberately ignored. A wrapping helper is invoked on
        ///     the receiver, and an extension receiver does not take an implicit numeric conversion, so a receiver
        ///     of a different type would not compile. The declared type also keeps the promoted arithmetic of
        ///     the small integral types visible: their operands have no conversion helper to the promoted type,
        ///     so the fix leaves those expressions untouched instead of emitting a call on a promoted receiver.
        /// </remarks>
        private string? GetReceiverTypeName(ExpressionSyntax expression) => GetWrappingTypeName(_semanticModel.GetTypeInfo(expression).Type);

        /// <summary>
        ///     Builds a simple assignment whose equals token is surrounded by single spaces.
        /// </summary>
        /// <param name="left">The assignment target.</param>
        /// <param name="value">The value assigned to the target.</param>
        /// <returns>The created assignment expression.</returns>
        /// <remarks>
        ///     The operands are stripped of trivia, because the same location is often written twice and a receiver
        ///     copy would otherwise repeat the indentation of the original expression. The caller attaches the trivia
        ///     of the replaced node afterwards, so the comments and the layout around the assignment are preserved.
        /// </remarks>
        private static AssignmentExpressionSyntax CreateAssignment(ExpressionSyntax left, ExpressionSyntax value) => SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left.WithoutTrivia(), SyntaxFactory.Token(SyntaxKind.EqualsToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space), value.WithoutTrivia());

        /// <summary>
        ///     Builds a call of the named helper on the receiver.
        /// </summary>
        /// <param name="receiver">The expression that the helper is called on.</param>
        /// <param name="methodName">The name of the helper.</param>
        /// <param name="argument">
        ///     The single argument of the helper, or <see langword="null" /> for a parameterless helper.
        /// </param>
        /// <returns>The created invocation.</returns>
        /// <remarks>
        ///     The receiver is parenthesized unless it is a primary expression,
        ///     because the helper is accessed through a member access.
        /// </remarks>
        private static InvocationExpressionSyntax CreateCall(ExpressionSyntax receiver, string methodName, ExpressionSyntax? argument = null)
        {
            var receiverExpression = IsPrimaryExpression(receiver) ? receiver.WithoutTrivia() : SyntaxFactory.ParenthesizedExpression(receiver.WithoutTrivia());
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiverExpression, SyntaxFactory.IdentifierName(methodName));
            var arguments = argument == null ? SyntaxFactory.ArgumentList() : SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argument)));
            return SyntaxFactory.InvocationExpression(memberAccess, arguments).WithLeadingTrivia(receiver.GetLeadingTrivia());
        }

        /// <summary>
        ///     Builds a call of the conversion helper of the target wrapper type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetTypeName">The name of the wrapper type that receives the value.</param>
        /// <param name="sourceTypeName">The name of the wrapper type that the value converts from.</param>
        /// <returns>The conversion call, or the original value when no conversion is needed.</returns>
        /// <remarks>
        ///     The caller has to verify that the helper exists, because no validation is done here. The conversion is
        ///     skipped entirely when the source and the target type are the same, so a redundant call is never emitted.
        /// </remarks>
        private static ExpressionSyntax CreateFromUncheckedCall(ExpressionSyntax value, string targetTypeName, string sourceTypeName)
        {
            if (targetTypeName == sourceTypeName)
                return value;

            var method = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(targetTypeName), SyntaxFactory.IdentifierName("From" + sourceTypeName + "Unchecked"));
            return SyntaxFactory.InvocationExpression(method, SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(value.WithoutTrivia())))).WithLeadingTrivia(value.GetLeadingTrivia());
        }

        /// <summary>
        ///     Builds a call of <c>Isize.FromPtrUnchecked</c>.
        /// </summary>
        /// <param name="value">The pointer value to convert.</param>
        /// <returns>The conversion call.</returns>
        private static InvocationExpressionSyntax CreateFromPtrUncheckedCall(ExpressionSyntax value) => SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(SR.ISIZE_TYPE_NAME), SyntaxFactory.IdentifierName("FromPtrUnchecked")), SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(value.WithoutTrivia())))).WithLeadingTrivia(value.GetLeadingTrivia());

        /// <summary>
        ///     Gets the keyword type syntax of an integral type.
        /// </summary>
        /// <param name="type">The type to write.</param>
        /// <returns>The type syntax of the type.</returns>
        private static TypeSyntax GetTypeSyntax(ITypeSymbol type) => SyntaxFactory.ParseTypeName(type.SpecialType switch
        {
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Byte => "byte",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_IntPtr => "nint",
            SpecialType.System_UIntPtr => "nuint",
            _ => type.ToDisplayString()
        });

        /// <summary>
        ///     Determines whether the expression can be the receiver of a member access without parentheses.
        /// </summary>
        /// <param name="expression">The expression to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is primary; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsPrimaryExpression(ExpressionSyntax expression) => expression.Kind() is SyntaxKind.IdentifierName or SyntaxKind.GenericName or SyntaxKind.SimpleMemberAccessExpression or SyntaxKind.InvocationExpression or SyntaxKind.ElementAccessExpression or SyntaxKind.ParenthesizedExpression or SyntaxKind.StringLiteralExpression or SyntaxKind.CharacterLiteralExpression or SyntaxKind.NumericLiteralExpression or SyntaxKind.TrueLiteralExpression or SyntaxKind.FalseLiteralExpression or SyntaxKind.NullLiteralExpression or SyntaxKind.ThisExpression or SyntaxKind.BaseExpression or SyntaxKind.ObjectCreationExpression or SyntaxKind.ImplicitObjectCreationExpression or SyntaxKind.DefaultExpression or SyntaxKind.TypeOfExpression;
    }
}