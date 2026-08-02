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
    [Analyzer(019)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer019_CopyBlockUnalignedMaybeOverlap : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "019";

        private static readonly LocalizableString Title001 = "Destination is higher than source";
        private static readonly LocalizableString MessageFormat001 = "Unsafe.CopyBlockUnaligned with destination > source";
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly LocalizableString Title002 = "Potential overlapping copy";
        private static readonly LocalizableString MessageFormat002 = "Unsafe.CopyBlockUnaligned may overlap";
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            if (memberAccess.Name.Identifier.Text != "CopyBlockUnaligned")
                return;

            if (!(memberAccess.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == "Unsafe"))
                return;

            var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (symbol?.ContainingType?.ToDisplayString() != "System.Runtime.CompilerServices.Unsafe")
                return;

            var args = invocation.ArgumentList.Arguments;
            if (args.Count < 2)
                return;

            var destExpr = args[0].Expression;
            var srcExpr = args[1].Expression;

            var destInfo = ExtractBaseAndOffset(destExpr);
            var srcInfo = ExtractBaseAndOffset(srcExpr);
            if (destInfo == null || srcInfo == null)
                return;

            var (destBase, destOffset) = destInfo.Value;
            var (srcBase, srcOffset) = srcInfo.Value;
            if (!SyntaxFactory.AreEquivalent(destBase, srcBase))
                return;

            var destGreater = IsOffsetGreater(destOffset, srcOffset, context.SemanticModel);
            if (destGreater == true)
                context.ReportDiagnostic(Diagnostic.Create(Rule001, invocation.GetLocation()));
            else if (destGreater == null)
                context.ReportDiagnostic(Diagnostic.Create(Rule002, invocation.GetLocation()));
        }

        private static (ExpressionSyntax Base, ExpressionSyntax Offset)? ExtractBaseAndOffset(ExpressionSyntax expr)
        {
            expr = UnwrapRefAndParens(expr);
            if (expr is InvocationExpressionSyntax asInvoke && asInvoke.Expression is MemberAccessExpressionSyntax asMa && asMa.Name.Identifier.Text == "As")
            {
                if (asMa.Expression is IdentifierNameSyntax { Identifier.Text: "Unsafe" })
                {
                    var inner = UnwrapRefAndParens(asInvoke.ArgumentList.Arguments[0].Expression);
                    if (inner is InvocationExpressionSyntax addInvoke && addInvoke.Expression is MemberAccessExpressionSyntax addMa)
                    {
                        if (addMa.Name.Identifier.Text == "Add" && addMa.Expression is IdentifierNameSyntax { Identifier.Text: "Unsafe" })
                        {
                            var addArgs = addInvoke.ArgumentList.Arguments;
                            if (addArgs.Count >= 2)
                            {
                                var baseExpr = UnwrapRefAndParens(addArgs[0].Expression);
                                var offsetExpr = addArgs[1].Expression;
                                return (baseExpr, offsetExpr);
                            }
                        }
                    }

                    return (inner, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));
                }
            }

            return null;
        }

        private static ExpressionSyntax UnwrapRefAndParens(ExpressionSyntax expr)
        {
            while (expr is ParenthesizedExpressionSyntax paren)
                expr = paren.Expression;
            return expr;
        }

        private static bool? IsOffsetGreater(ExpressionSyntax left, ExpressionSyntax right, SemanticModel model)
        {
            left = UnwrapCastAndParens(left);
            right = UnwrapCastAndParens(right);

            var leftConst = model.GetConstantValue(left);
            var rightConst = model.GetConstantValue(right);

            if (leftConst.HasValue && rightConst.HasValue)
            {
                if (leftConst.Value is int lInt && rightConst.Value is int rInt)
                    return lInt > rInt;
            }

            if (right is BinaryExpressionSyntax rightBin && rightBin.Kind() == SyntaxKind.AddExpression)
            {
                if (SyntaxFactory.AreEquivalent(rightBin.Left, left) || SyntaxFactory.AreEquivalent(rightBin.Right, left))
                    return false;
            }

            if (left is BinaryExpressionSyntax leftBin && leftBin.Kind() == SyntaxKind.AddExpression)
            {
                if (SyntaxFactory.AreEquivalent(leftBin.Left, right) || SyntaxFactory.AreEquivalent(leftBin.Right, right))
                {
                    var other = SyntaxFactory.AreEquivalent(leftBin.Left, right) ? leftBin.Right : leftBin.Left;
                    var val = model.GetConstantValue(UnwrapCastAndParens(other));
                    if (val.HasValue && val.Value is int v && v > 0)
                        return true;
                }
            }

            return null;
        }

        private static ExpressionSyntax UnwrapCastAndParens(ExpressionSyntax expr)
        {
            while (true)
            {
                if (expr is ParenthesizedExpressionSyntax paren)
                    expr = paren.Expression;
                else if (expr is CastExpressionSyntax cast)
                    expr = cast.Expression;
                else
                    break;
            }

            return expr;
        }
    }
}