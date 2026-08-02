using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS1038
#pragma warning disable RS2008

// ReSharper disable ALL

namespace Roslyn
{
    [Analyzer(020)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer020_NuintUIntMixedOperation : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "020";

        private static readonly LocalizableString Title = "Mixing nuint and uint in an operation may cause truncation";
        private static readonly LocalizableString MessageFormat = "Do not mix 'nuint' and 'uint' in operations";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, BinaryKinds);
            context.RegisterSyntaxNodeAction(AnalyzeAssignmentExpression, AssignmentKinds);
        }

        private void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;
            CheckOperands(binary.Left, binary.Right, context, binary.GetLocation());
        }

        private void AnalyzeAssignmentExpression(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            CheckOperands(assignment.Left, assignment.Right, context, assignment.GetLocation());
        }

        private void CheckOperands(ExpressionSyntax left, ExpressionSyntax right, SyntaxNodeAnalysisContext context, Location location)
        {
            var model = context.SemanticModel;
            var leftType = model.GetTypeInfo(left).Type;
            var rightType = model.GetTypeInfo(right).Type;

            if (leftType == null || rightType == null)
                return;

            var leftIsNuInt = leftType.SpecialType == SpecialType.System_UIntPtr;
            var leftIsUInt = leftType.SpecialType == SpecialType.System_UInt32;
            var rightIsNuInt = rightType.SpecialType == SpecialType.System_UIntPtr;
            var rightIsUInt = rightType.SpecialType == SpecialType.System_UInt32;

            if ((leftIsNuInt && rightIsUInt) || (leftIsUInt && rightIsNuInt))
            {
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}