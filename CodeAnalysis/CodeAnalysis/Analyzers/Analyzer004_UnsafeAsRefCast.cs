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
    [Analyzer(004)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer004_UnsafeAsRefCast : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "004";

        private static readonly LocalizableString Title = "Explicit cast is prohibited in Unsafe.AsRef<T>() argument";
        private static readonly LocalizableString MessageFormat = "Unsafe.AsRef<{0}> does not allow explicit casts in its argument";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol == null)
                return;

            if (methodSymbol.Name == "AsRef" && methodSymbol.ContainingType?.ToDisplayString() == "System.Runtime.CompilerServices.Unsafe")
            {
                if (methodSymbol.TypeArguments.Length == 1 && methodSymbol.Parameters.Length == 1)
                {
                    var paramType = methodSymbol.Parameters[0].Type;
                    if (paramType is IPointerTypeSymbol pointerType && pointerType.PointedAtType.SpecialType == SpecialType.System_Void)
                    {
                        if (invocation.ArgumentList.Arguments.Count == 1)
                        {
                            var argExpr = invocation.ArgumentList.Arguments[0].Expression;
                            if (argExpr is CastExpressionSyntax)
                            {
                                var genericType = methodSymbol.TypeArguments[0];
                                var diagnostic = Diagnostic.Create(Rule, argExpr.GetLocation(), genericType.ToDisplayString());
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }
    }
}