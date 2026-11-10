using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the literal suffixes and the constant conversion rewrites that the numeric
    ///     analyzers and code fixes share.
    /// </summary>
    /// <remarks>
    ///     A constant conversion helper is only needed because the method call states the target type explicitly.
    ///     When the produced value is a compile-time constant that the C# literal syntax can write directly,
    ///     the literal suffix states the same type without a call, so the analyzer reports the helper and the
    ///     code fix replaces it with the suffixed literal.
    /// </remarks>
    internal static class LiteralFacts
    {
        /// <summary>
        ///     Gets the literal suffix of the wrapper type or of the C# keyword type.
        /// </summary>
        /// <param name="typeName">The simple name of the wrapper type or the keyword of the C# type.</param>
        /// <returns>The suffix that writes a literal of the type, or <see langword="null" /> when it has none.</returns>
        /// <remarks>
        ///     The mapping covers exactly the types whose underlying type has a literal suffix.
        ///     The small integral types and the native-sized types have no suffix of their own, so they are
        ///     rejected instead of being written with a suffix that would change their type.
        /// </remarks>
        public static string? GetLiteralSuffix(string? typeName) => typeName switch
        {
            "uint" or "U32" => "U",
            "long" or "I64" => "L",
            "ulong" or "U64" => "UL",
            "float" or "F32" => "F",
            "double" or "F64" => "D",
            "decimal" => "M",
            _ => null
        };

        /// <summary>
        ///     Gets the literal suffix of the type that the semantic model resolves.
        /// </summary>
        /// <param name="type">The special type of the declaration.</param>
        /// <returns>The suffix that writes a literal of the type, or <see langword="null" /> when it has none.</returns>
        /// <remarks>
        ///     The overload resolves a declaration whose type is written as an alias or as a qualified name,
        ///     which the name based mapping cannot classify.
        /// </remarks>
        public static string? GetLiteralSuffix(SpecialType type) => type switch
        {
            SpecialType.System_UInt32 => "U",
            SpecialType.System_Int64 => "L",
            SpecialType.System_UInt64 => "UL",
            SpecialType.System_Single => "F",
            SpecialType.System_Double => "D",
            SpecialType.System_Decimal => "M",
            _ => null
        };

        /// <summary>
        ///     Appends a literal suffix to a numeric literal or to the shifted literal of a constant expression.
        /// </summary>
        /// <param name="value">The expression to rewrite.</param>
        /// <param name="suffix">The suffix of the target type.</param>
        /// <param name="result">The literal with the suffix.</param>
        /// <returns>
        ///     <see langword="true" /> when the suffix was appended; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A literal that already carries an unsigned or long suffix is not modified, because the suffix of
        ///     the target type would change the value instead of the type. A floating-point or decimal suffix
        ///     is only recognized when the literal has a decimal spelling, because the hexadecimal digits
        ///     <c>A</c> to <c>F</c> would otherwise hide the suffix that is still missing.
        /// </remarks>
        public static bool TryAppendLiteralSuffix(ExpressionSyntax value, string suffix, out ExpressionSyntax result)
        {
            switch (value)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NumericLiteralExpression):
                    var text = literal.Token.Text;
                    if (HasLiteralSuffix(text))
                        break;

                    result = SyntaxFactory.ParseExpression(text + suffix).WithTriviaFrom(literal);
                    return true;

                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.LeftShiftExpression) && TryAppendLiteralSuffix(binary.Left, suffix, out var left):
                    result = binary.WithLeft(left);
                    return true;
            }

            result = null!;
            return false;
        }

        /// <summary>
        ///     Determines whether the text of a numeric literal already carries a type suffix.
        /// </summary>
        /// <param name="text">The text of the literal.</param>
        /// <returns>
        ///     <see langword="true" /> when the literal carries a suffix; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The unsigned and long suffixes cannot be spelled by a digit of a prefixed literal, so they are
        ///     recognized in every spelling. The floating-point and decimal suffixes share their letters with
        ///     the hexadecimal digits, so they are only recognized in a literal without a radix prefix.
        /// </remarks>
        private static bool HasLiteralSuffix(string text)
        {
            if (text.Length == 0)
                return false;

            var last = text[text.Length - 1];
            if ("uUlL".IndexOf(last) >= 0)
                return true;

            var prefixed = text.Length > 1 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X' || text[1] == 'b' || text[1] == 'B');
            return !prefixed && "fFdDmM".IndexOf(last) >= 0;
        }

        /// <summary>
        ///     Writes a constant expression as a literal of the target wrapper type.
        /// </summary>
        /// <param name="value">The constant expression to rewrite.</param>
        /// <param name="targetTypeName">The name of the wrapper type that the expression converts to.</param>
        /// <param name="result">The constant conversion that replaces the expression.</param>
        /// <returns>
        ///     <see langword="true" /> when a suffixed literal was written; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A helper call is not a compile-time constant, so a constant can only be made explicit by
        ///     the literal suffix of its target type. A type without a suffix is left to the caller, which
        ///     keeps it as written in a constant position and uses a helper anywhere else.
        /// </remarks>
        public static bool TryCreateConstantConversion(ExpressionSyntax value, string targetTypeName, out ExpressionSyntax result)
        {
            result = null!;
            var suffix = GetLiteralSuffix(targetTypeName);
            return suffix != null && TryAppendLiteralSuffix(value, suffix, out result);
        }

        /// <summary>
        ///     Determines whether the invocation is a constant conversion helper that a suffixed literal can replace.
        /// </summary>
        /// <param name="invocation">The invocation to classify.</param>
        /// <param name="semanticModel">The semantic model used to resolve the invoked method.</param>
        /// <param name="replacement">The literal that replaces the invocation.</param>
        /// <returns>
        ///     <see langword="true" /> when the invocation can be replaced; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The invocation has to be a <c>FromI32Unchecked</c> method of one of the wrapper types of the
        ///     analyzed library, because the same method name in another type keeps the helper call. Only a
        ///     numeric literal argument is rewritten: a variable, a constant field and a folded expression
        ///     cannot state their type with a suffix without changing the written value or its type.
        /// </remarks>
        public static bool TryGetReplacement(InvocationExpressionSyntax invocation, SemanticModel semanticModel, out ExpressionSyntax replacement)
        {
            replacement = null!;
            if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "FromI32Unchecked" } || invocation.ArgumentList.Arguments.Count != 1)
                return false;

            if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Parameters.Length: 1 } method || method.Parameters[0].Type.SpecialType != SpecialType.System_Int32)
                return false;

            if (method.ContainingType.ContainingNamespace.ToDisplayString() != SR.NATIVE_COLLECTIONS_NAMESPACE)
                return false;

            var suffix = GetLiteralSuffix(method.ContainingType.Name);
            return suffix != null && invocation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax { } literal && literal.IsKind(SyntaxKind.NumericLiteralExpression) && TryAppendLiteralSuffix(literal, suffix, out replacement);
        }
    }
}