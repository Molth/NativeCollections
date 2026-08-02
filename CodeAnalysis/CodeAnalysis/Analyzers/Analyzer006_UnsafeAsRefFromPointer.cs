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
    [Analyzer(006)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer006_UnsafeAsRefFromPointer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "006";

        private static readonly LocalizableString Title = "Unsafe.AsRef(Unsafe.AsPointer(...)) pattern is prohibited";
        private static readonly LocalizableString MessageFormat = "Use Unsafe.As<TSource, TTarget>(ref ...) instead of Unsafe.AsRef<TTarget>(Unsafe.AsPointer(...))";
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
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            if (methodSymbol.Name == "AsRef" && methodSymbol.ContainingType?.ToDisplayString() == "System.Runtime.CompilerServices.Unsafe")
            {
                if (methodSymbol.TypeArguments.Length == 1 && invocation.ArgumentList.Arguments.Count == 1)
                {
                    var innerArg = invocation.ArgumentList.Arguments[0].Expression;
                    if (innerArg is InvocationExpressionSyntax innerInvocation)
                    {
                        if (context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol is IMethodSymbol innerSymbolInfo)
                        {
                            if (innerSymbolInfo.Name == "AsPointer" && innerSymbolInfo.ContainingType?.ToDisplayString() == "System.Runtime.CompilerServices.Unsafe")
                            {
                                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
                                context.ReportDiagnostic(diagnostic);
                            }
                        }
                    }
                }
            }
        }
    }
}