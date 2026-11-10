using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires every implicit numeric conversion of the analyzed library to be written explicitly.
    ///     Explicit narrowing and pointer conversions retain the existing helper requirement.
    /// </summary>
    /// <remarks>
    ///     Spelling the conversion out keeps the numeric type choice visible at the call site and makes
    ///     the conversion follow the wrapper matrix used by the rest of the library. A compile-time
    ///     constant is reported like any other conversion, except in a position where C# requires a
    ///     constant expression and in an element of an array or a collection expression, because a
    ///     method call cannot be used in the former and would prevent the values from being emitted
    ///     as constant data in the latter.
    ///     A conversion whose target wrapper type declares no matching <c>From*Unchecked</c> method is still
    ///     reported, because falling back to the implicit conversion would hide the numeric type choice at
    ///     exactly the places where the helper matrix of the library is incomplete.
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
        private static readonly LocalizableString MessageFormat = "Replace the numeric conversion with the target type's From{0}Unchecked helper when the helper exists";

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
        ///     Reports the conversion when it must not be written implicitly.
        /// </summary>
        /// <param name="context">The operation context of the conversion.</param>
        /// <remarks>
        ///     The conversion is not reported when it is an identity conversion, when it is performed by a user-defined
        ///     operator, when it happens inside the body of a wrapper conversion, when either type has no wrapper
        ///     type, when it is a compile-time constant in a position where C# requires a constant expression or
        ///     in an element of an array or a collection expression, when the conversion cannot overflow, or when
        ///     it happens inside an explicit <see langword="checked" /> context. A helper call is never a constant,
        ///     so such a position keeps its implicit conversion. A checked conversion is written on purpose to
        ///     throw on overflow, so a helper would change its behavior.
        /// </remarks>
        private static void AnalyzeConversion(OperationAnalysisContext context)
        {
            if (context.Operation is not IConversionOperation conversion || conversion.OperatorMethod != null || conversion.Operand.Type is not { } sourceType || conversion.Type is not { } targetType)
                return;

            if (context.ContainingSymbol is IMethodSymbol containingMethod && HelperFacts.IsWrappingHelper(containingMethod))
                return;

            if (conversion.ConstantValue.HasValue && (ArithmeticFacts.IsRequiredConstantContext(conversion.Syntax) || ArithmeticFacts.IsConstantDataElement(conversion.Syntax)))
                return;

            if (SyntaxHelpers.IsInsideCheckedContext(conversion.Syntax))
                return;

            if (!conversion.IsImplicit && conversion.Syntax is not CastExpressionSyntax)
                return;

            var isPointerConversion = sourceType.TypeKind == TypeKind.Pointer;
            var sourceTypeName = isPointerConversion ? SR.PTR_TYPE_NAME : NumericFacts.GetHelperTypeName(sourceType);
            var targetTypeName = NumericFacts.GetHelperTypeName(targetType);
            if (sourceTypeName == null || targetTypeName == null || sourceTypeName == targetTypeName || (!isPointerConversion && !conversion.IsImplicit && !NumericFacts.CanOverflow(sourceType, targetType)))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, conversion.Syntax.GetLocation(), sourceTypeName));
        }
    }
}