using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Prohibits mixing <see cref="UIntPtr" /> and <see cref="uint" />
    ///     operands in a single binary or compound assignment operation.
    /// </summary>
    /// <remarks>
    ///     The common type of <see cref="UIntPtr" /> and <see cref="uint" /> is <see cref="UIntPtr" />,
    ///     so an intended 32-bit computation silently widens to the platform pointer size and
    ///     then narrows again at the assignment. On a 64-bit platform that hides the wrap-around
    ///     at <c>2^32</c> and makes the result differ between 32-bit and 64-bit builds.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer020_NuintUIntMixedOperation : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "020";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Mixing nuint and uint in an operation may cause truncation";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Do not mix 'nuint' and 'uint' in operations";

        /// <summary>
        ///     Flags a binary or compound assignment operation with one
        ///     <see cref="UIntPtr" /> and one <see cref="uint" /> operand.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The binary operator kinds that are inspected.
        /// </summary>
        private static readonly SyntaxKind[] BinaryKinds =
        [
            SyntaxKind.AddExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression
        ];

        /// <summary>
        ///     The compound assignment operator kinds that are inspected.
        /// </summary>
        private static readonly SyntaxKind[] AssignmentKinds =
        [
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression
        ];

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the binary and assignment callbacks.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, BinaryKinds);
            context.RegisterSyntaxNodeAction(AnalyzeAssignmentExpression, AssignmentKinds);
        }

        /// <summary>
        ///     Checks the operands of a binary expression.
        /// </summary>
        /// <param name="context">The syntax context of the binary expression.</param>
        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            CheckOperands(binary.Left, binary.Right, context, binary.GetLocation());
        }

        /// <summary>
        ///     Checks the operands of a compound assignment expression.
        /// </summary>
        /// <param name="context">The syntax context of the assignment expression.</param>
        private static void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            CheckOperands(assignment.Left, assignment.Right, context, assignment.GetLocation());
        }

        /// <summary>
        ///     Reports the operation when one operand is typed
        ///     <see cref="UIntPtr" /> and the other is typed <see cref="uint" />.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <param name="context">The syntax context of the operation.</param>
        /// <param name="location">The location reported for the diagnostic.</param>
        /// <remarks>
        ///     The types are taken from the syntax of each operand rather than
        ///     from the operation, because the converted type of both operands is
        ///     <see cref="UIntPtr" /> and would hide the original <see cref="uint" /> operand.
        /// </remarks>
        private static void CheckOperands(ExpressionSyntax left, ExpressionSyntax right, SyntaxNodeAnalysisContext context, Location location)
        {
            var leftType = context.SemanticModel.GetTypeInfo(left).Type;
            var rightType = context.SemanticModel.GetTypeInfo(right).Type;
            if (leftType == null || rightType == null)
                return;

            var leftIsNativeUnsigned = leftType.SpecialType == SpecialType.System_UIntPtr;
            var leftIsUnsigned32 = leftType.SpecialType == SpecialType.System_UInt32;
            var rightIsNativeUnsigned = rightType.SpecialType == SpecialType.System_UIntPtr;
            var rightIsUnsigned32 = rightType.SpecialType == SpecialType.System_UInt32;
            if ((leftIsNativeUnsigned && rightIsUnsigned32) || (leftIsUnsigned32 && rightIsNativeUnsigned))
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }
    }
}