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
    [Analyzer(007)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer007_UnsafeAddOrSubtractWithNegative : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "007";

        private static readonly LocalizableString Title001 = "Unsafe.Add / AddByteOffset with negative offset is prohibited";
        private static readonly LocalizableString MessageFormat001 = "Use Unsafe.Subtract{0} instead of negative offset call to Unsafe.Add{0}";
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly LocalizableString Title002 = "Unsafe.Subtract / SubtractByteOffset with unary minus expression is prohibited";
        private static readonly LocalizableString MessageFormat002 = "The offset argument of Unsafe.Subtract{0} should not be a negative expression";
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002];

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                return;

            var methodName = memberAccess.Name.Identifier.Text;
            var isAdd = methodName == "Add";
            var isAddByteOffset = methodName == "AddByteOffset";
            var isSubtract = methodName == "Subtract";
            var isSubtractByteOffset = methodName == "SubtractByteOffset";

            if (!(isAdd || isAddByteOffset || isSubtract || isSubtractByteOffset))
                return;

            if (memberAccess.Expression is not IdentifierNameSyntax classIdent)
                return;

            if (classIdent.Identifier.Text != "Unsafe" && classIdent.Identifier.Text != "UnsafeHelpers")
                return;

            if (invocation.ArgumentList.Arguments.Count != 2)
                return;

            var offsetArg = invocation.ArgumentList.Arguments[1].Expression;
            var coreExpr = UnwrapParentheses(offsetArg);

            if (isAdd || isAddByteOffset)
            {
                if (IsNegativeConstant(coreExpr, context.SemanticModel) || IsNegativeUnary(coreExpr))
                {
                    var suffix = isAddByteOffset ? "ByteOffset" : "";
                    context.ReportDiagnostic(Diagnostic.Create(Rule001, offsetArg.GetLocation(), suffix));
                }
            }
            else if (isSubtract || isSubtractByteOffset)
            {
                if (IsNegativeUnary(coreExpr))
                {
                    var suffix = isSubtractByteOffset ? "ByteOffset" : "";
                    context.ReportDiagnostic(Diagnostic.Create(Rule002, offsetArg.GetLocation(), suffix));
                }
            }
        }

        private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expr)
        {
            while (expr is ParenthesizedExpressionSyntax paren)
                expr = paren.Expression;
            return expr;
        }

        private static bool IsNegativeUnary(ExpressionSyntax expr) => expr is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.UnaryMinusExpression);

        private static bool IsNegativeConstant(ExpressionSyntax expr, SemanticModel semanticModel)
        {
            var constVal = semanticModel.GetConstantValue(expr);
            if (!constVal.HasValue)
                return false;

            switch (constVal.Value)
            {
                case int i:
                    return i < 0;
                case long l:
                    return l < 0;
                case short s:
                    return s < 0;
                case sbyte sb:
                    return sb < 0;
                case byte _:
                case uint _:
                case ulong _:
                case ushort _:
                    return false;
                case null:
                    return false;
                default:
                    return false;
            }
        }
    }
}