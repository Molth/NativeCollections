using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS1038
#pragma warning disable RS2008

// ReSharper disable ALL

namespace Roslyn
{
    [Analyzer(001)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer001_PointerSafety : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "001";

        private static readonly LocalizableString Title001 = "Pointer dereference detected";
        private static readonly LocalizableString MessageFormat001 = "Pointer dereference: '*{0}'";
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly LocalizableString Title002 = "Pointer arithmetic assignment is prohibited";
        private static readonly LocalizableString MessageFormat002 = "Use Unsafe.Add<T>() instead of arithmetic assignment on pointer '{0}'";
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly LocalizableString Title003 = "Convert pointer arithmetic to unsafe methods";
        private static readonly LocalizableString MessageFormat003 = "Pointer arithmetic should be converted to Unsafe methods";
        private static readonly DiagnosticDescriptor Rule003 = new(DIAGNOSTIC_ID + "_3", Title003, MessageFormat003, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly LocalizableString Title004 = "Pointer arithmetic or indexing is prohibited";
        private static readonly LocalizableString MessageFormat004 = "Pointer type cannot be used with operators or indexing";
        private static readonly DiagnosticDescriptor Rule004 = new(DIAGNOSTIC_ID + "_4", Title004, MessageFormat004, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002, Rule003, Rule004];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeDereference, SyntaxKind.PointerIndirectionExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.AddExpression, SyntaxKind.SubtractExpression, SyntaxKind.MultiplyExpression);
            context.RegisterSyntaxNodeAction(AnalyzeElementAccess, SyntaxKind.ElementAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
        }

        private static void AnalyzeDereference(SyntaxNodeAnalysisContext context)
        {
            var dereference = (PrefixUnaryExpressionSyntax)context.Node;
            if (!dereference.OperatorToken.IsKind(SyntaxKind.AsteriskToken))
                return;

            var operandType = context.SemanticModel.GetTypeInfo(dereference.Operand).Type;
            if (operandType is not IPointerTypeSymbol)
                return;

            if (dereference.Operand is ParenthesizedExpressionSyntax paren && paren.Expression is BinaryExpressionSyntax)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule003, dereference.GetLocation()));
                return;
            }

            var diagnostic = Diagnostic.Create(Rule001, dereference.GetLocation(), dereference.Operand.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpr = (BinaryExpressionSyntax)context.Node;
            if (binaryExpr.Parent is ParenthesizedExpressionSyntax paren)
            {
                if (paren.Parent is PrefixUnaryExpressionSyntax prefix && prefix.IsKind(SyntaxKind.PointerIndirectionExpression))
                    return;
            }

            if (binaryExpr.Parent is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            var leftType = context.SemanticModel.GetTypeInfo(binaryExpr.Left).Type;
            var rightType = context.SemanticModel.GetTypeInfo(binaryExpr.Right).Type;

            if (leftType is IPointerTypeSymbol || rightType is IPointerTypeSymbol)
                context.ReportDiagnostic(Diagnostic.Create(Rule004, binaryExpr.GetLocation()));
        }

        private static void AnalyzeElementAccess(SyntaxNodeAnalysisContext context)
        {
            var elementAccess = (ElementAccessExpressionSyntax)context.Node;
            var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression);
            if (typeInfo.Type is IPointerTypeSymbol)
                context.ReportDiagnostic(Diagnostic.Create(Rule003, elementAccess.GetLocation()));
        }

        private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
        {
            var indexer = (IndexerDeclarationSyntax)context.Node;
            foreach (var parameter in indexer.ParameterList.Parameters)
            {
                if (parameter.Type is PointerTypeSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule003, indexer.GetLocation()));
                    return;
                }
            }

            foreach (var accessor in indexer.AccessorList?.Accessors ?? Enumerable.Empty<AccessorDeclarationSyntax>())
            {
                if (accessor.Body != null)
                {
                    foreach (var elementAccess in accessor.Body.DescendantNodes().OfType<ElementAccessExpressionSyntax>())
                    {
                        var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression);
                        if (typeInfo.Type is IPointerTypeSymbol)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule003, indexer.GetLocation()));
                            return;
                        }
                    }
                }

                if (accessor.ExpressionBody != null)
                {
                    var expr = accessor.ExpressionBody.Expression;
                    if (expr is ElementAccessExpressionSyntax elementAccess)
                    {
                        var typeInfo = context.SemanticModel.GetTypeInfo(elementAccess.Expression);
                        if (typeInfo.Type is IPointerTypeSymbol)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule003, indexer.GetLocation()));
                            return;
                        }
                    }
                    else if (expr is RefExpressionSyntax refExpr && refExpr.Expression is ElementAccessExpressionSyntax refElementAccess)
                    {
                        var typeInfo = context.SemanticModel.GetTypeInfo(refElementAccess.Expression);
                        if (typeInfo.Type is IPointerTypeSymbol)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule003, indexer.GetLocation()));
                            return;
                        }
                    }
                }
            }
        }

        private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            var leftType = context.SemanticModel.GetTypeInfo(assignment.Left).Type;
            if (leftType is not IPointerTypeSymbol)
                return;

            if (assignment.IsKind(SyntaxKind.AddAssignmentExpression) || assignment.IsKind(SyntaxKind.SubtractAssignmentExpression))
            {
                ReportAssignment(context, assignment, assignment.Left);
                return;
            }

            if (assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) && assignment.Right is BinaryExpressionSyntax binary)
            {
                var leftIdent = assignment.Left as IdentifierNameSyntax;
                if (leftIdent == null)
                    return;

                if (binary.Left is IdentifierNameSyntax binaryLeftIdent && binaryLeftIdent.Identifier.Text == leftIdent.Identifier.Text)
                {
                    if (binary.IsKind(SyntaxKind.AddExpression) || binary.IsKind(SyntaxKind.SubtractExpression))
                        ReportAssignment(context, assignment, assignment.Left);
                }
            }

            static void ReportAssignment(SyntaxNodeAnalysisContext context, AssignmentExpressionSyntax assignment, ExpressionSyntax pointerExpr) => context.ReportDiagnostic(Diagnostic.Create(Rule002, assignment.GetLocation(), pointerExpr.ToString()));
        }
    }
}