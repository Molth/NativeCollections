using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Prohibits the <c>Unsafe.AsRef&lt;T&gt;(Unsafe.AsPointer(...))</c> round trip.
    /// </summary>
    /// <remarks>
    ///     Converting a reference to a pointer and back bypasses the type system without any benefit;
    ///     the same reinterpretation is expressed by <c>Unsafe.As&lt;TFrom, TTo&gt;(ref ...)</c>, which
    ///     keeps both types in the source and lets the compiler verify that the sizes are compatible.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer006_UnsafeAsRefFromPointer : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "006";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Unsafe.AsRef(Unsafe.AsPointer(...)) pattern is prohibited";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Use Unsafe.As<TSource, TTarget>(ref ...) instead of Unsafe.AsRef<TTarget>(Unsafe.AsPointer(...))";

        /// <summary>
        ///     Flags an <c>Unsafe.AsRef</c> whose only argument is an <c>Unsafe.AsPointer</c> invocation.
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
        ///     Reports an <c>Unsafe.AsRef</c> invocation whose argument is an <c>Unsafe.AsPointer</c> invocation.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        /// <remarks>
        ///     Both calls are resolved semantically, so a user-declared type named <c>Unsafe</c> is
        ///     not matched. Only the single-argument <c>AsRef</c> overload can form this pattern.
        /// </remarks>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "AsRef", TypeArguments.Length: 1 } method || method.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            if (invocation.ArgumentList.Arguments.Count != 1 || invocation.ArgumentList.Arguments[0].Expression is not InvocationExpressionSyntax innerInvocation)
                return;

            if (context.SemanticModel.GetSymbolInfo(innerInvocation).Symbol is not IMethodSymbol { Name: "AsPointer" } innerMethod || innerMethod.ContainingType.ToDisplayString() != SR.UNSAFE_TYPE_NAME)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}