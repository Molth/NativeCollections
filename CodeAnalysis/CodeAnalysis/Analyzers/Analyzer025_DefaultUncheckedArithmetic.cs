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
    ///     Requires arithmetic that the old code performed in an unchecked
    ///     context to be written with an explicit <c>Wrapping</c> helper.
    /// </summary>
    /// <remarks>
    ///     The analyzed library compiles with <c>CheckForOverflowUnderflow</c>, so every unchecked operation
    ///     of the old code now throws. Index, counter, step and hash arithmetic that used to wrap silently
    ///     must therefore state its wrapping intent. Size arithmetic keeps the project-wide checked semantics
    ///     on purpose, because an overflowing allocation size or capacity must throw instead of wrapping.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer025_DefaultUncheckedArithmetic : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "025";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Make unchecked arithmetic explicit";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Use a Wrapping helper to preserve this operation's original unchecked behavior";

        /// <summary>
        ///     Flags an integral arithmetic operation that is neither size arithmetic nor a constant expression.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the callbacks for every arithmetic operation that the project-wide checked context changes.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        /// <remarks>
        ///     Conversions are not registered, because <see cref="Analyzer028_NumericConversions" />
        ///     owns them and reports only the ones that can overflow.
        /// </remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeOperation, SyntaxKind.AddExpression, SyntaxKind.SubtractExpression, SyntaxKind.MultiplyExpression, SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression, SyntaxKind.MultiplyAssignmentExpression, SyntaxKind.UnaryMinusExpression, SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression, SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression);
        }

        /// <summary>
        ///     Reports the operation when it must be made explicit.
        /// </summary>
        /// <param name="context">The syntax context of the operation.</param>
        /// <remarks>
        ///     Operations inside a <see langword="checked" /> context, constant
        ///     expressions and size arithmetic are not reported, and neither are the
        ///     bodies of the <c>Wrapping</c> and <c>From*Unchecked</c> helpers themselves.
        /// </remarks>
        private static void AnalyzeOperation(SyntaxNodeAnalysisContext context)
        {
            if (SyntaxHelpers.IsInsideCheckedContext(context.Node) || IsWrappingHelper(context) || ArithmeticFacts.MustKeepConstantExpression(context.Node, context.SemanticModel) || ArithmeticFacts.IsCheckedSizeArithmetic(context.Node, context.SemanticModel))
                return;

            var operation = context.SemanticModel.GetOperation(context.Node);
            if (operation != null && RequiresWrapping(operation))
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }

        /// <summary>
        ///     Determines whether the operation changes its behavior when the project-wide checked context is enabled.
        /// </summary>
        /// <param name="operation">The operation to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the operation needs an explicit
        ///     wrapping helper; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Only the arithmetic operations are classified, because the conversion
        ///     rule decides on its own which conversions have to be made explicit.
        /// </remarks>
        private static bool RequiresWrapping(IOperation operation)
        {
            switch (operation)
            {
                case IBinaryOperation binary:
                    return binary.OperatorMethod == null && binary.OperatorKind is BinaryOperatorKind.Add or BinaryOperatorKind.Subtract or BinaryOperatorKind.Multiply && NumericFacts.IsIntegral(binary.Type);

                case ICompoundAssignmentOperation compound:
                    return compound.OperatorMethod == null && compound.OperatorKind is BinaryOperatorKind.Add or BinaryOperatorKind.Subtract or BinaryOperatorKind.Multiply && NumericFacts.IsIntegral(compound.Type);

                case IUnaryOperation unary:
                    return unary.OperatorMethod == null && unary.OperatorKind == UnaryOperatorKind.Minus && NumericFacts.IsIntegral(unary.Type);

                case IIncrementOrDecrementOperation increment:
                    return increment.OperatorMethod == null && NumericFacts.IsIntegral(increment.Type);

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Determines whether the node is inside the body of a helper
        ///     that implements wrapping arithmetic or a numeric conversion.
        /// </summary>
        /// <param name="context">The syntax context of the operation.</param>
        /// <returns>
        ///     <see langword="true" /> when the operation belongs to a helper; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsWrappingHelper(SyntaxNodeAnalysisContext context) => context.Node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is { } method && HelperFacts.IsWrappingHelper(context.SemanticModel.GetDeclaredSymbol(method));
    }
}