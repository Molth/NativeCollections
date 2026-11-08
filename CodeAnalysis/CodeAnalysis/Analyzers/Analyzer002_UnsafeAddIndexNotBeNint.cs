using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires the element offset passed to <c>Unsafe.Add</c> to be a <see cref="IntPtr" />.
    /// </summary>
    /// <remarks>
    ///     Picking the <see cref="int" /> overload silently widens an offset to the element size times the offset, and
    ///     it hides that the value is an element offset rather than a byte offset. The <see cref="IntPtr" /> overload
    ///     also removes the <c>PositionIndependent</c> branch that the JIT emits for the <see cref="int" /> overload.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer002_UnsafeAddIndexNotBeNint : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "002";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Unsafe.Add index must be nint";

        /// <summary>
        ///     The message of the rule; the placeholder is the actual type of the offset argument.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Index passed to Unsafe.Add must be 'nint', but found '{0}'";

        /// <summary>
        ///     Flags <c>Unsafe.Add</c> invocations whose second argument is not typed as <see cref="IntPtr" />.
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
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        /// <summary>
        ///     Reports an <c>Unsafe.Add</c> call whose resolved second parameter is not <see cref="IntPtr" />.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     The check runs on the resolved method symbol, so a local type named
        ///     <c>Unsafe</c> is not reported and only the two-argument overloads are considered.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "Add", Parameters.Length: 2 } method || method.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            var indexExpression = invocation.ArgumentList.Arguments[1].Expression;
            var typeInfo = context.SemanticModel.GetTypeInfo(indexExpression);
            var indexType = typeInfo.ConvertedType ?? typeInfo.Type;
            if (indexType == null || indexType.SpecialType != SpecialType.System_IntPtr)
                context.ReportDiagnostic(Diagnostic.Create(Rule, indexExpression.GetLocation(), indexType?.ToDisplayString() ?? "unknown"));
        }
    }
}