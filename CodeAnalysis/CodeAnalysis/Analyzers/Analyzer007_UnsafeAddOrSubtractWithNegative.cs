using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Prohibits passing a negative offset to <c>Unsafe.Add</c> / <c>Unsafe.AddByteOffset</c>,
    ///     or a negated expression to <c>Unsafe.Subtract</c> / <c>Unsafe.SubtractByteOffset</c>.
    /// </summary>
    /// <remarks>
    ///     The negative forms hide the direction of the movement and are easy to double negate. The matching
    ///     positive helper states the direction explicitly, so the call site reads as a forward or backward offset.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer007_UnsafeAddOrSubtractWithNegative : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The prefix shared by the two rules of this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "007";

        /// <summary>
        ///     The title of <c>PTR007_1</c>.
        /// </summary>
        private static readonly LocalizableString Title001 = "Unsafe.Add / AddByteOffset with negative offset is prohibited";

        /// <summary>
        ///     The message of <c>PTR007_1</c>; the placeholder is the
        ///     method suffix (<c>ByteOffset</c> or an empty string).
        /// </summary>
        private static readonly LocalizableString MessageFormat001 = "Use Unsafe.Subtract{0} instead of negative offset call to Unsafe.Add{0}";

        /// <summary>
        ///     Flags <c>Unsafe.Add</c> and <c>Unsafe.AddByteOffset</c> with a negative offset.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>PTR007_2</c>.
        /// </summary>
        private static readonly LocalizableString Title002 = "Unsafe.Subtract / SubtractByteOffset with unary minus expression is prohibited";

        /// <summary>
        ///     The message of <c>PTR007_2</c>; the placeholder is the
        ///     method suffix (<c>ByteOffset</c> or an empty string).
        /// </summary>
        private static readonly LocalizableString MessageFormat002 = "The offset argument of Unsafe.Subtract{0} should not be a negative expression";

        /// <summary>
        ///     Flags <c>Unsafe.Subtract</c> and <c>Unsafe.SubtractByteOffset</c> with a unary minus offset.
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
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        ///     Reports a negative offset passed to an <c>Add</c> helper
        ///     or a negated offset passed to a <c>Subtract</c> helper.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     Both the BCL <see cref="Unsafe" /> type and the <c>UnsafeHelpers</c>
        ///     wrapper of the analyzed library are matched by syntax, because the wrapper
        ///     forwards to the same BCL methods and must follow the same convention.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Expression is not IdentifierNameSyntax hostIdentifier)
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            if ((hostIdentifier.Identifier.Text != "Unsafe" && hostIdentifier.Identifier.Text != "UnsafeHelpers") || invocation.ArgumentList.Arguments.Count != 2)
                return;

            if (methodName is not ("Add" or "AddByteOffset" or "Subtract" or "SubtractByteOffset"))
                return;

            var offset = SyntaxHelpers.UnwrapParentheses(invocation.ArgumentList.Arguments[1].Expression);
            if (methodName is "Add" or "AddByteOffset")
            {
                if (IsNegativeConstant(offset, context.SemanticModel) || IsNegativeUnary(offset))
                    context.ReportDiagnostic(Diagnostic.Create(Rule001, invocation.ArgumentList.Arguments[1].GetLocation(), methodName == "AddByteOffset" ? "ByteOffset" : string.Empty));

                return;
            }

            if (IsNegativeUnary(offset))
                context.ReportDiagnostic(Diagnostic.Create(Rule002, invocation.ArgumentList.Arguments[1].GetLocation(), methodName == "SubtractByteOffset" ? "ByteOffset" : string.Empty));
        }

        /// <summary>
        ///     Determines whether the expression is a unary minus expression.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is <c>-x</c>; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsNegativeUnary(ExpressionSyntax expression) => expression is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.UnaryMinusExpression);

        /// <summary>
        ///     Determines whether the expression is a compile-time constant with a negative signed value.
        /// </summary>
        /// <param name="expression">The offset expression to inspect.</param>
        /// <param name="semanticModel">The semantic model used to evaluate the constant.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is a negative constant; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Unsigned constants are never negative, so they are reported as non-negative.
        ///     A constant that is not a numeric value, such as <see langword="null" />,
        ///     is treated as non-negative because no negative offset is proven.
        /// </remarks>
        private static bool IsNegativeConstant(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var constant = semanticModel.GetConstantValue(expression);
            return constant.HasValue && constant.Value switch
            {
                int value => value < 0,
                long value => value < 0,
                short value => value < 0,
                sbyte value => value < 0,
                _ => false
            };
        }
    }
}