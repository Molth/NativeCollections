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
    [Analyzer(022)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer022_NegativeToUnsignedCast : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "022";

        private static readonly LocalizableString Title = "Cast of negated value to unsigned type should be unchecked";
        private static readonly LocalizableString MessageFormat = "Wrap the conversion in 'unchecked' to avoid overflow exception";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeUnaryMinus, SyntaxKind.UnaryMinusExpression);
        }

        private static void AnalyzeUnaryMinus(SyntaxNodeAnalysisContext context)
        {
            var unaryMinus = (PrefixUnaryExpressionSyntax)context.Node;
            if (unaryMinus.Parent is not CastExpressionSyntax castExpr)
                return;

            var targetType = context.SemanticModel.GetTypeInfo(castExpr.Type).Type;
            if (targetType == null || !IsUnsignedInteger(targetType))
                return;

            if (castExpr.Parent is CheckedExpressionSyntax checkedExpr && checkedExpr.Kind() == SyntaxKind.UncheckedExpression)
                return;

            var diagnostic = Diagnostic.Create(Rule, castExpr.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsUnsignedInteger(ITypeSymbol type) =>
            type.SpecialType is
                SpecialType.System_Byte or
                SpecialType.System_UInt16 or
                SpecialType.System_UInt32 or
                SpecialType.System_UInt64 or
                SpecialType.System_UIntPtr;
    }
}