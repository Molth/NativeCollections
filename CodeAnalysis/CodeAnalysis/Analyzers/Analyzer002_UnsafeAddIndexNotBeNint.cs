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
    [Analyzer(002)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer002_UnsafeAddIndexNotBeNint : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "002";

        private static readonly LocalizableString Title = "Unsafe.Add index must be nint";
        private static readonly LocalizableString MessageFormat = "Index passed to Unsafe.Add must be 'nint', but found '{0}'";
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
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "Add")
            {
                var symbol = context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                if (symbol == null || !symbol.ToDisplayString().Contains("System.Runtime.CompilerServices.Unsafe"))
                    return;

                var args = invocation.ArgumentList.Arguments;
                if (args.Count != 2)
                    return;

                var indexExpr = args[1].Expression;
                var typeInfo = context.SemanticModel.GetTypeInfo(indexExpr);
                var indexType = typeInfo.ConvertedType ?? typeInfo.Type;

                if (indexType == null || indexType.SpecialType != SpecialType.System_IntPtr)
                {
                    var diagnostic = Diagnostic.Create(Rule, indexExpr.GetLocation(), indexType?.ToDisplayString() ?? "unknown");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}