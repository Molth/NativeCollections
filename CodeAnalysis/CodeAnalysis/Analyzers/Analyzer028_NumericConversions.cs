using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires every implicit numeric conversion of the analyzed library to be written with
    ///     the unchecked conversion helper of its target wrapper type when the helper exists.
    ///     Explicit narrowing and pointer conversions retain the existing helper requirement.
    /// </summary>
    /// <remarks>
    ///     Spelling the conversion out through the helper keeps the numeric type choice visible at
    ///     the call site and makes the conversion follow the wrapper matrix used by the rest of the
    ///     library. Compile-time constants are still excluded because a method call cannot be used
    ///     where C# requires a constant expression. The target wrapper type must declare a matching
    ///     <c>From*Unchecked</c> method, because the helper matrix of the library is partial.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer028_NumericConversions : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "028";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Use source-specific unchecked numeric conversion helpers";

        /// <summary>
        ///     The message of the rule, formatted with the name of the wrapper type that the value converts from.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Replace the numeric conversion with the target type's From{0}Unchecked method";

        /// <summary>
        ///     Flags a numeric or pointer conversion that is not written with a conversion helper.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the conversion callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(AnalyzeConversion, OperationKind.Conversion);
        }

        /// <summary>
        ///     Reports the conversion when it must be written with a conversion helper.
        /// </summary>
        /// <param name="context">The operation context of the conversion.</param>
        /// <remarks>
        ///     The conversion is not reported when it is an identity conversion, when it is performed by a user-defined
        ///     operator, when it happens inside the body of a wrapper conversion, when either type has no wrapper
        ///     type, when the result is a compile-time constant, when the conversion cannot overflow, when it
        ///     happens inside an explicit <see langword="checked" /> context, or when the required helper does not
        ///     exist. A call is never a constant, and the compiler already rejects a constant that does not fit
        ///     the target type, so a folded conversion needs no helper. A checked conversion is written on purpose
        ///     to throw on overflow, so a helper would change its behavior and the code fix keeps it as written.
        /// </remarks>
        private static void AnalyzeConversion(OperationAnalysisContext context)
        {
            if (context.Operation is not IConversionOperation conversion || conversion.OperatorMethod != null || conversion.Operand.Type is not { } sourceType || conversion.Type is not { } targetType)
                return;

            if (context.ContainingSymbol is IMethodSymbol containingMethod && HelperFacts.IsWrappingHelper(containingMethod))
                return;

            if (conversion.ConstantValue.HasValue)
                return;

            if (IsInsideCheckedContext(conversion.Syntax))
                return;

            if (!conversion.IsImplicit && conversion.Syntax is not CastExpressionSyntax)
                return;

            var isPointerConversion = sourceType.TypeKind == TypeKind.Pointer;
            var sourceTypeName = isPointerConversion ? SR.PTR_TYPE_NAME : NumericFacts.GetHelperTypeName(sourceType);
            var targetTypeName = NumericFacts.GetHelperTypeName(targetType);
            if (sourceTypeName == null || targetTypeName == null || sourceTypeName == targetTypeName || (!isPointerConversion && !conversion.IsImplicit && !NumericFacts.CanOverflow(sourceType, targetType)))
                return;

            if (HasConversionHelpers(context.Compilation, targetTypeName, sourceTypeName, isPointerConversion))
                context.ReportDiagnostic(Diagnostic.Create(Rule, conversion.Syntax.GetLocation(), sourceTypeName));
        }

        /// <summary>
        ///     Determines whether the syntax node is inside an explicit <see langword="checked" /> context.
        /// </summary>
        /// <param name="node">The node whose lexical context is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the nearest overflow context is
        ///     <see langword="checked" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The nearest checked or unchecked ancestor decides, so an inner
        ///     <see langword="unchecked" /> expression keeps the wrapping behavior and is still reported.
        /// </remarks>
        private static bool IsInsideCheckedContext(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                switch (ancestor)
                {
                    case CheckedExpressionSyntax expression:
                        return expression.IsKind(SyntaxKind.CheckedExpression);

                    case CheckedStatementSyntax statement:
                        return statement.IsKind(SyntaxKind.CheckedStatement);
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the helpers that the code fix would emit exist.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="targetTypeName">The name of the wrapper type that receives the value.</param>
        /// <param name="sourceTypeName">The name of the wrapper type that the value converts from.</param>
        /// <param name="isPointerConversion">A value indicating whether the value converts from a pointer.</param>
        /// <returns>
        ///     <see langword="true" /> when the code fix can express
        ///     the conversion; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A pointer conversion is first converted to the native-sized signed wrapper
        ///     type and from there to the target type, so both steps have to be available.
        /// </remarks>
        private static bool HasConversionHelpers(Compilation compilation, string targetTypeName, string sourceTypeName, bool isPointerConversion) => !isPointerConversion ? NumericFacts.HasUncheckedConversionHelper(compilation, targetTypeName, sourceTypeName) : NumericFacts.HasUncheckedConversionHelper(compilation, SR.ISIZE_TYPE_NAME, sourceTypeName) && (targetTypeName == SR.ISIZE_TYPE_NAME || NumericFacts.HasUncheckedConversionHelper(compilation, targetTypeName, SR.ISIZE_TYPE_NAME));
    }
}