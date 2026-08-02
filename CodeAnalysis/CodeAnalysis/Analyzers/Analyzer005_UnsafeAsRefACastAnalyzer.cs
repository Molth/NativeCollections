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
    [Analyzer(005)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer005_UnsafeAsRefACastAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "005";

        private static readonly LocalizableString Title = "Casting is prohibited in Unsafe.AsRef<T> argument";
        private static readonly LocalizableString MessageFormat = "Unsafe.AsRef<{0}> does not allow any explicit cast in its argument";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

            if (memberAccess.Name is not GenericNameSyntax genericName)
                return;

            if (genericName.Identifier.Text != "AsRef" || genericName.TypeArgumentList.Arguments.Count != 1)
                return;

            if (memberAccess.Expression is not IdentifierNameSyntax identifier || identifier.Identifier.Text != "Unsafe")
                return;

            if (invocation.ArgumentList.Arguments.Count != 1)
                return;

            var argExpr = invocation.ArgumentList.Arguments[0].Expression;
            if (UnwrapParentheses(argExpr) is CastExpressionSyntax)
            {
                var typeArg = genericName.TypeArgumentList.Arguments[0].ToString();
                context.ReportDiagnostic(Diagnostic.Create(Rule, argExpr.GetLocation(), typeArg));
            }
        }

        private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expr)
        {
            while (expr is ParenthesizedExpressionSyntax paren)
                expr = paren.Expression;
            return expr;
        }
    }
}