using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Prohibits an explicit cast in the argument of <c>Unsafe.AsRef&lt;T&gt;(void*)</c>.
    /// </summary>
    /// <remarks>
    ///     The cast is a no-op for the pointer overload and only obscures which type is being
    ///     reinterpreted. The intended source type is expressed by the type argument, and a genuine
    ///     reinterpretation must use <c>Unsafe.As&lt;TFrom, TTo&gt;</c> so that both types stay visible.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer004_UnsafeAsRefCast : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "004";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Explicit cast is prohibited in Unsafe.AsRef<T>() argument";

        /// <summary>
        ///     The message of the rule; the placeholder is the type argument of <c>Unsafe.AsRef</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Unsafe.AsRef<{0}> does not allow explicit casts in its argument";

        /// <summary>
        ///     Flags a cast expression passed to the pointer overload of <c>Unsafe.AsRef</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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
        ///     Reports a cast expression used as the argument of <c>Unsafe.AsRef&lt;T&gt;(void*)</c>.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     The invocation is resolved semantically, so a user-declared type named <c>Unsafe</c>
        ///     is ignored. Redundant parentheses around the cast are unwrapped before the check.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "AsRef", TypeArguments.Length: 1, Parameters.Length: 1 } method || method.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            if (method.Parameters[0].Type is not IPointerTypeSymbol { PointedAtType.SpecialType: SpecialType.System_Void } || invocation.ArgumentList.Arguments.Count != 1)
                return;

            var argument = invocation.ArgumentList.Arguments[0].Expression;
            if (UnwrapParentheses(argument) is CastExpressionSyntax)
                context.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation(), method.TypeArguments[0].ToDisplayString()));
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
    }
}