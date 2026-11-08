using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Detects <c>Unsafe.CopyBlockUnaligned</c> calls whose source and destination ranges may overlap.
    /// </summary>
    /// <remarks>
    ///     <c>CopyBlockUnaligned</c> is a forward byte copy with no overlap handling. If the destination starts
    ///     after the source inside the same allocation, the tail of the source is overwritten before it is read,
    ///     which silently corrupts the copied data. An overlap must be handled explicitly or by copying backwards.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer019_CopyBlockUnalignedMaybeOverlap : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The prefix shared by the two rules of this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "019";

        /// <summary>
        ///     The title of <c>DESIGN019_1</c>.
        /// </summary>
        private static readonly LocalizableString Title001 = "Destination is higher than source";

        /// <summary>
        ///     The message of <c>DESIGN019_1</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat001 = "Unsafe.CopyBlockUnaligned with destination > source";

        /// <summary>
        ///     Flags a copy whose destination offset is provably greater than the source offset.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>DESIGN019_2</c>.
        /// </summary>
        private static readonly LocalizableString Title002 = "Potential overlapping copy";

        /// <summary>
        ///     The message of <c>DESIGN019_2</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat002 = "Unsafe.CopyBlockUnaligned may overlap";

        /// <summary>
        ///     Flags a copy whose relative offset cannot be proven safe.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the two rules reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002];

        /// <summary>
        ///     Registers the invocation callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        ///     Compares the offsets of the two
        ///     <c>
        ///         Unsafe.As&lt;T&gt;(Unsafe.Add(base,
        ///         offset))
        ///     </c>
        ///     arguments of a <c>CopyBlockUnaligned</c> call.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     The comparison only applies when both arguments address the same base
        ///     expression; different allocations cannot overlap. A proven greater destination
        ///     is reported as an error, an unprovable relation as a potential overlap.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "Unsafe" }, Name.Identifier.ValueText: "CopyBlockUnaligned" } || invocation.ArgumentList.Arguments.Count < 2)
                return;

            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method || method.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            var destination = ExtractBaseAndOffset(invocation.ArgumentList.Arguments[0].Expression);
            var source = ExtractBaseAndOffset(invocation.ArgumentList.Arguments[1].Expression);
            if (destination == null || source == null || !SyntaxFactory.AreEquivalent(destination.Value.Base, source.Value.Base))
                return;

            var destinationIsGreater = IsOffsetGreater(destination.Value.Offset, source.Value.Offset, context.SemanticModel);
            if (destinationIsGreater == true)
                context.ReportDiagnostic(Diagnostic.Create(Rule001, invocation.GetLocation()));
            else if (destinationIsGreater == null)
                context.ReportDiagnostic(Diagnostic.Create(Rule002, invocation.GetLocation()));
        }

        /// <summary>
        ///     Extracts the base expression and the element offset from a
        ///     <c>Unsafe.As&lt;T&gt;(Unsafe.Add(base, offset))</c> argument.
        /// </summary>
        /// <param name="expression">The argument expression of the copy call.</param>
        /// <returns>
        ///     The base expression with its offset, or <see langword="null" /> when the shape is not recognized.
        /// </returns>
        /// <remarks>
        ///     An argument without the <c>Unsafe.Add</c> step is treated as offset zero of
        ///     the inner expression. Any other shape returns <see langword="null" />, because
        ///     the relation between two such addresses cannot be compared by this analyzer.
        /// </remarks>
        private static (ExpressionSyntax Base, ExpressionSyntax Offset)? ExtractBaseAndOffset(ExpressionSyntax expression)
        {
            var inner = UnwrapParentheses(expression);
            if (inner is not InvocationExpressionSyntax asInvocation || asInvocation.Expression is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "Unsafe" }, Name.Identifier.ValueText: "As" } || asInvocation.ArgumentList.Arguments.Count != 1)
                return null;

            var reference = UnwrapParentheses(asInvocation.ArgumentList.Arguments[0].Expression);
            if (reference is InvocationExpressionSyntax addInvocation && addInvocation.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "Unsafe" }, Name.Identifier.ValueText: "Add" } && addInvocation.ArgumentList.Arguments.Count >= 2)
                return (UnwrapParentheses(addInvocation.ArgumentList.Arguments[0].Expression), addInvocation.ArgumentList.Arguments[1].Expression);

            return (reference, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));
        }

        /// <summary>
        ///     Removes any number of redundant parentheses around the given expression.
        /// </summary>
        /// <param name="expression">The expression to unwrap.</param>
        /// <returns>The innermost expression after removing all parenthesized wrappers.</returns>
        private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
        {
            while (expression is ParenthesizedExpressionSyntax parenthesized)
                expression = parenthesized.Expression;

            return expression;
        }

        /// <summary>
        ///     Determines whether the destination offset is greater than the source offset.
        /// </summary>
        /// <param name="destination">The destination offset.</param>
        /// <param name="source">The source offset.</param>
        /// <param name="semanticModel">The semantic model used to evaluate the offsets.</param>
        /// <returns>
        ///     <see langword="true" /> when the destination is greater, <see langword="false" />
        ///     when it is not, and <see langword="null" /> when the relation is unknown.
        /// </returns>
        /// <remarks>
        ///     Both offsets are reduced to their constant values first. When that fails, the <c>base + offset</c>
        ///     shapes are compared: building one offset from the other proves the direction, and the sign of the added
        ///     constant resolves the remaining case. A <see langword="null" /> result means the relation is unknown.
        /// </remarks>
        private static bool? IsOffsetGreater(ExpressionSyntax destination, ExpressionSyntax source, SemanticModel semanticModel)
        {
            var destinationValue = FindConstantOffset(UnwrapCastsAndParentheses(destination), semanticModel);
            var sourceValue = FindConstantOffset(UnwrapCastsAndParentheses(source), semanticModel);
            if (destinationValue.HasValue && sourceValue.HasValue)
                return destinationValue.Value > sourceValue.Value;

            if (source is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } sourceSum && (SyntaxFactory.AreEquivalent(sourceSum.Left, destination) || SyntaxFactory.AreEquivalent(sourceSum.Right, destination)))
                return false;

            if (destination is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AddExpression } destinationSum && (SyntaxFactory.AreEquivalent(destinationSum.Left, source) || SyntaxFactory.AreEquivalent(destinationSum.Right, source)))
            {
                var other = SyntaxFactory.AreEquivalent(destinationSum.Left, source) ? destinationSum.Right : destinationSum.Left;
                var otherValue = FindConstantOffset(UnwrapCastsAndParentheses(other), semanticModel);
                if (otherValue.HasValue && otherValue.Value > 0)
                    return true;
            }

            return null;
        }

        /// <summary>
        ///     Finds the constant <see cref="int" /> value of an offset
        ///     expression, or <see langword="null" /> when it is not a constant.
        /// </summary>
        /// <param name="expression">The offset expression to evaluate.</param>
        /// <param name="semanticModel">The semantic model used to evaluate the constant.</param>
        /// <returns>
        ///     The constant value converted to <see cref="long" />, or
        ///     <see langword="null" /> when no integral constant is available.
        /// </returns>
        private static long? FindConstantOffset(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var constant = semanticModel.GetConstantValue(expression);
            if (!constant.HasValue)
                return null;

            return constant.Value switch
            {
                int value => value,
                uint value => value,
                long value => value,
                ulong value => value <= long.MaxValue ? (long)value : null,
                short value => value,
                ushort value => value,
                byte value => value,
                sbyte value => value,
                _ => null
            };
        }

        /// <summary>
        ///     Removes any number of redundant parentheses and casts around the given expression.
        /// </summary>
        /// <param name="expression">The expression to unwrap.</param>
        /// <returns>The innermost expression after removing all parenthesized and cast wrappers.</returns>
        private static ExpressionSyntax UnwrapCastsAndParentheses(ExpressionSyntax expression)
        {
            while (true)
            {
                switch (expression)
                {
                    case ParenthesizedExpressionSyntax parenthesized:
                        expression = parenthesized.Expression;
                        continue;

                    case CastExpressionSyntax cast:
                        expression = cast.Expression;
                        continue;

                    default:
                        return expression;
                }
            }
        }
    }
}