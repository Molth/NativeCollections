using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires the pointer arguments of <c>Unsafe.CopyBlockUnaligned</c>
    ///     to be converted to <c>ref Unsafe.AsRef&lt;byte&gt;</c> references.
    /// </summary>
    /// <remarks>
    ///     The <see langword="ref" /> overload keeps the byte count independent of the element type and makes
    ///     the copy explicitly a byte-wise operation. Passing raw pointers selects the pointer overload, which
    ///     is easy to confuse with the element-wise overloads of the other <see cref="Unsafe" /> copy helpers.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer003_UnsafeCopyBlockUnaligned : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "003";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Convert Unsafe.CopyBlockUnaligned pointer parameters";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Convert pointer parameters in Unsafe.CopyBlockUnaligned to ref Unsafe.AsRef<byte>";

        /// <summary>
        ///     Flags the pointer overload of <c>Unsafe.CopyBlockUnaligned</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the invocation callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        ///     Reports the <c>Unsafe.CopyBlockUnaligned(void*, void*, uint)</c> overload.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     The overload is identified by its resolved signature rather than by the
        ///     argument syntax, so an argument that happens to be a pointer but binds to the
        ///     <see langword="ref" /> overload through an implicit conversion is not reported.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "CopyBlockUnaligned", Parameters.Length: 3 } method || method.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            var destination = method.Parameters[0].Type;
            var source = method.Parameters[1].Type;
            var byteCount = method.Parameters[2].Type;
            if (destination.TypeKind != TypeKind.Pointer || source.TypeKind != TypeKind.Pointer || byteCount.SpecialType != SpecialType.System_UInt32)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}