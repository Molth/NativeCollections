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
    [Analyzer(003)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer003_UnsafeCopyBlockUnaligned : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "003";

        private static readonly LocalizableString Title = "Convert Unsafe.CopyBlockUnaligned pointer parameters";
        private static readonly LocalizableString MessageFormat = "Convert pointer parameters in Unsafe.CopyBlockUnaligned to ref Unsafe.AsRef<byte>";
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
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess?.Name.Identifier.Text != "CopyBlockUnaligned")
                return;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol?.ContainingType?.ToString() != "System.Runtime.CompilerServices.Unsafe")
                return;

            if (methodSymbol.Parameters.Length != 3)
                return;

            var p0 = methodSymbol.Parameters[0].Type;
            var p1 = methodSymbol.Parameters[1].Type;
            var p2 = methodSymbol.Parameters[2].Type;

            if (p0.TypeKind != TypeKind.Pointer || p1.TypeKind != TypeKind.Pointer)
                return;

            if (p2.SpecialType != SpecialType.System_UInt32)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}