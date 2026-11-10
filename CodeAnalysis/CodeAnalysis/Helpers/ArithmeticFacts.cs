using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the arithmetic classifications that the checked-arithmetic analyzer
    ///     and the wrapping code fix share.
    /// </summary>
    /// <remarks>
    ///     The helpers decide which operations the project-wide checked context changes. Size arithmetic keeps
    ///     the checked semantics on purpose, because an overflowing allocation size or capacity must throw
    ///     instead of wrapping, and a compile-time constant has to stay a constant because a helper call is
    ///     never one. Both decisions are shared, so the analyzer and the code fix cannot disagree on them.
    /// </remarks>
    internal static class ArithmeticFacts
    {
        /// <summary>
        ///     The lower-case name suffixes that identify a value as
        ///     a byte count, length, capacity, alignment or stride.
        /// </summary>
        /// <remarks>
        ///     Matching by word suffix makes <c>bucketsByteCount</c>, <c>nativeSize</c> and <c>minimumCapacity</c>
        ///     size names while an unrelated identifier that merely contains those letters is not matched.
        /// </remarks>
        private static readonly string[] SizeNameSuffixes = ["bytecount", "byteoffset", "bytesize", "bytes", "size", "length", "capacity", "alignment", "overhead", "stride", "elementcount"];

        /// <summary>
        ///     Determines whether the node participates in size
        ///     arithmetic and must therefore keep the checked semantics.
        /// </summary>
        /// <param name="node">The expression to classify.</param>
        /// <param name="semanticModel">The semantic model used to resolve calls and names.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is size arithmetic; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     An expression is size arithmetic when it reads a value that is named like a size, when it
        ///     contains <see langword="sizeof" />, when it feeds a size-like parameter of an allocator or
        ///     of a method whose parameter is named like a size, when it initializes or assigns a size-named
        ///     variable, when it is returned from a size-named member, or when it is an array length. The
        ///     decision is made per operation, so a mixed expression still wraps the parts that are not sizes.
        /// </remarks>
        public static bool IsCheckedSizeArithmetic(SyntaxNode node, SemanticModel semanticModel)
        {
            if (ContainsSizeOf(node) || ReadsSizeNamedValue(node, semanticModel))
                return true;

            for (var current = node.Parent; current != null; current = current.Parent)
            {
                if (current is ExpressionSyntax && (ContainsSizeOf(current) || ReadsSizeNamedValue(current, semanticModel)))
                    return true;

                switch (current)
                {
                    case ArgumentSyntax { Parent.Parent: InvocationExpressionSyntax invocation } argument:
                        if (IsSizeApi(invocation, argument, semanticModel))
                            return true;
                        break;

                    case EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator }:
                        return IsSizeName(declarator.Identifier.ValueText);

                    case AssignmentExpressionSyntax assignment:
                        return IsSizeName(GetAssignedName(assignment.Left));

                    case ReturnStatementSyntax:
                    case ArrowExpressionClauseSyntax:
                        return IsSizeName(GetContainingMemberName(node));

                    case ArrayRankSpecifierSyntax:
                        return true;

                    case StatementSyntax:
                    case MemberDeclarationSyntax:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether an expression is or contains a
        ///     <see langword="sizeof" /> or <c>Unsafe.SizeOf</c> operation.
        /// </summary>
        /// <param name="node">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when a size operation is present; otherwise, <see langword="false" />.
        /// </returns>
        private static bool ContainsSizeOf(SyntaxNode node) => node.DescendantNodesAndSelf().Any(child => child is SizeOfExpressionSyntax || child is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "SizeOf" } });

        /// <summary>
        ///     Determines whether an expression reads a value whose name
        ///     denotes a size, length, capacity, alignment or offset.
        /// </summary>
        /// <param name="node">The expression to inspect.</param>
        /// <param name="semanticModel">The semantic model used to classify the identifiers.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression reads a
        ///     size-named value; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The name decides on its own, so a parameter such as <c>alignment</c> or a local such as <c>byteCount</c>
        ///     marks the arithmetic that uses it as size arithmetic even when the result is assigned to a name
        ///     that does not read like a size. Identifiers that resolve to a type are skipped, because the wrapper
        ///     type names <c>Isize</c> and <c>Usize</c> end with the <c>size</c> suffix without describing a size.
        /// </remarks>
        private static bool ReadsSizeNamedValue(SyntaxNode node, SemanticModel semanticModel) => node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(identifier => semanticModel.GetSymbolInfo(identifier).Symbol is not INamespaceOrTypeSymbol && IsSizeName(identifier.Identifier.ValueText));

        /// <summary>
        ///     Determines whether the name denotes a size, length, capacity, alignment or offset.
        /// </summary>
        /// <param name="name">The identifier or parameter name to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the name ends with one of the
        ///     size suffixes; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsSizeName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var lowerInvariant = name!.ToLowerInvariant();
            return SizeNameSuffixes.Any(suffix => lowerInvariant.EndsWith(suffix, StringComparison.Ordinal));
        }

        /// <summary>
        ///     Determines whether the argument of the invocation is a size argument.
        /// </summary>
        /// <param name="invocation">The invocation that receives the argument.</param>
        /// <param name="argument">The argument to classify.</param>
        /// <param name="semanticModel">The semantic model used to resolve the invoked method.</param>
        /// <returns>
        ///     <see langword="true" /> when the argument is a size argument; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Every numeric argument of a memory allocator is a byte count, an alignment or an
        ///     element count, so the allocator types are recognized as a whole. Other calls are
        ///     classified by the name of the bound parameter or of the explicitly named argument.
        /// </remarks>
        private static bool IsSizeApi(InvocationExpressionSyntax invocation, ArgumentSyntax argument, SemanticModel semanticModel)
        {
            if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
                return false;

            if (method.ContainingType.Name.EndsWith("MemoryAllocator", StringComparison.Ordinal))
                return true;

            var argumentIndex = invocation.ArgumentList.Arguments.IndexOf(argument);
            if (argumentIndex < 0 || method.Parameters.Length == 0)
                return false;

            var parameter = method.Parameters[Math.Min(argumentIndex, method.Parameters.Length - 1)];
            return IsSizeName(argument.NameColon?.Name.Identifier.ValueText ?? parameter.Name);
        }

        /// <summary>
        ///     Gets the name that an assignment writes to.
        /// </summary>
        /// <param name="expression">The left side of the assignment.</param>
        /// <returns>
        ///     The identifier or member name, or <see langword="null" /> when the target has no simple name.
        /// </returns>
        private static string? GetAssignedName(ExpressionSyntax expression) => expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null
        };

        /// <summary>
        ///     Gets the name of the method, property, constructor or operator that contains the node.
        /// </summary>
        /// <param name="node">The node whose containing member is searched.</param>
        /// <returns>The member name, or <see langword="null" /> when the node is not inside a named member.</returns>
        private static string? GetContainingMemberName(SyntaxNode node)
        {
            for (var current = node; current != null; current = current.Parent)
            {
                switch (current)
                {
                    case MethodDeclarationSyntax method:
                        return method.Identifier.ValueText;

                    case PropertyDeclarationSyntax property:
                        return property.Identifier.ValueText;

                    case ConstructorDeclarationSyntax:
                        return ".ctor";

                    case OperatorDeclarationSyntax:
                    case ConversionOperatorDeclarationSyntax:
                        return "operator";
                }
            }

            return null;
        }

        /// <summary>
        ///     Determines whether the constant expression must stay a compile-time
        ///     constant and therefore must not be replaced by a helper call.
        /// </summary>
        /// <param name="node">The expression to inspect.</param>
        /// <param name="semanticModel">The semantic model used to evaluate the constant.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression must stay constant; otherwise, <see langword="false" />.
        /// </returns>
        public static bool MustKeepConstantExpression(SyntaxNode node, SemanticModel semanticModel) => semanticModel.GetConstantValue(node) is { HasValue: true } && (IsRequiredConstantContext(node) || IsSimpleConstantExpression(node));

        /// <summary>
        ///     Determines whether the node is a single literal, constant reference or sign applied to one.
        /// </summary>
        /// <param name="node">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is a simple constant; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Such an expression has nothing to wrap: the operation is entirely performed on literals and
        ///     the compiler folds it, so replacing it with a helper call would only obscure the constant.
        /// </remarks>
        public static bool IsSimpleConstantExpression(SyntaxNode node) => node switch
        {
            LiteralExpressionSyntax => true,
            ParenthesizedExpressionSyntax parenthesized => IsSimpleConstantExpression(parenthesized.Expression),
            PrefixUnaryExpressionSyntax prefix when prefix.IsKind(SyntaxKind.UnaryMinusExpression) || prefix.IsKind(SyntaxKind.UnaryPlusExpression) => IsSimpleConstantExpression(prefix.Operand),
            IdentifierNameSyntax => true,
            MemberAccessExpressionSyntax memberAccess => IsSimpleConstantExpression(memberAccess.Expression),
            _ => false
        };

        /// <summary>
        ///     Determines whether the node is in a position where the C# language requires a compile-time constant.
        /// </summary>
        /// <param name="syntax">The expression whose position is inspected.</param>
        /// <returns><see langword="true" /> when a constant is required; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     A helper call is never a constant, so an operation in one of these positions can only
        ///     be made explicit with a literal suffix or an explicit cast, which the code fix decides.
        ///     The positions are attribute arguments, switch labels without a <see langword="when" />
        ///     guard, <see langword="goto" /> <see langword="case" /> labels, constant and relational
        ///     patterns, enum member values, parameter default values, fixed buffer sizes, and the
        ///     initializers of <see langword="const" /> members.
        /// </remarks>
        public static bool IsRequiredConstantContext(SyntaxNode syntax)
        {
            for (var node = syntax; node != null; node = node.Parent)
            {
                if (node is AttributeArgumentSyntax or ConstantPatternSyntax or RelationalPatternSyntax or EnumMemberDeclarationSyntax or GotoStatementSyntax { Expression: not null })
                    return true;

                if (node is CaseSwitchLabelSyntax)
                    return true;

                if (node is CasePatternSwitchLabelSyntax patternLabel && (patternLabel.WhenClause == null || !patternLabel.WhenClause.Span.Contains(syntax.Span)))
                    return true;

                if (node is ParameterSyntax { Default: not null })
                    return true;

                if (node is VariableDeclaratorSyntax { ArgumentList: not null })
                    return true;

                if (node is VariableDeclaratorSyntax declarator && declarator.Initializer != null && declarator.Initializer.Value.Span.Contains(syntax.Span) && declarator.Parent?.Parent is { } declarationParent && declarationParent.DescendantTokens().Any(token => token.IsKind(SyntaxKind.ConstKeyword)))
                    return true;

                if (node is ArrowExpressionClauseSyntax arrow && arrow.Expression.Span.Contains(syntax.Span) && arrow.Parent is PropertyDeclarationSyntax property && property.Modifiers.Any(SyntaxKind.ConstKeyword))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the node initializes an element of an array or a collection expression
        ///     whose values are emitted as constant data.
        /// </summary>
        /// <param name="syntax">The expression whose position is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression initializes a constant data element;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A helper call is never a constant, so replacing such an element would prevent the compiler
        ///     and the JIT from materializing the values as constant data.
        /// </remarks>
        public static bool IsConstantDataElement(SyntaxNode syntax)
        {
            for (var node = syntax.Parent; node != null; node = node.Parent)
            {
                if (node is ExpressionElementSyntax)
                    continue;

                if (node is InitializerExpressionSyntax initializer)
                    return initializer.IsKind(SyntaxKind.ArrayInitializerExpression);

                return node is CollectionExpressionSyntax;
            }

            return false;
        }
    }
}