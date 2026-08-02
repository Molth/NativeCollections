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
    [Analyzer(008)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer008_PointerNullCheck : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "008";

        private static readonly LocalizableString Title = "Pointer null check should use UnsafeHelpers.IsNull";
        private static readonly LocalizableString MessageFormat = "Replace '{0}' with '{1}'";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
        }

        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpr = (BinaryExpressionSyntax)context.Node;
            var left = binaryExpr.Left;
            var right = binaryExpr.Right;

            var leftIsNull = left.IsKind(SyntaxKind.NullLiteralExpression);
            var rightIsNull = right.IsKind(SyntaxKind.NullLiteralExpression);
            if (!leftIsNull && !rightIsNull)
                return;

            var pointerOperand = leftIsNull ? right : left;
            if (pointerOperand is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression is IdentifierNameSyntax identifier)
                {
                    if (identifier.Identifier.ValueText == "UnsafeHelpers" && memberAccess.Name.Identifier.ValueText == "IsNull")
                        return;
                }
            }

            var typeInfo = context.SemanticModel.GetTypeInfo(pointerOperand);
            var type = typeInfo.Type;
            if (type == null || type.TypeKind != TypeKind.Pointer)
                return;

            var expressionText = pointerOperand.ToString();
            var isEquals = binaryExpr.IsKind(SyntaxKind.EqualsExpression);
            var replacement = isEquals ? $"UnsafeHelpers.IsNull({expressionText})" : $"!UnsafeHelpers.IsNull({expressionText})";

            var diagnostic = Diagnostic.Create(Rule, binaryExpr.GetLocation(), binaryExpr.ToString(), replacement);
            context.ReportDiagnostic(diagnostic);
        }
    }
}