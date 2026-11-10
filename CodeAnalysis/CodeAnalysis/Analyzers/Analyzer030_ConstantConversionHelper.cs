using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires a constant that a <c>FromI32Unchecked</c> helper converts to be written as a suffixed literal.
    /// </summary>
    /// <remarks>
    ///     The helper exists to state the checked or unchecked conversion of a value whose type is not visible
    ///     at the call site. A numeric literal already has the type that its suffix spells, so the call only
    ///     restates the type of the constant. Writing the literal with the suffix of the target type removes the
    ///     call without changing the value or the type of the expression.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer030_ConstantConversionHelper : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "030";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Use a suffixed literal instead of a constant conversion helper";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Replace the constant conversion helper with a suffixed literal";

        /// <summary>
        ///     Flags a constant conversion helper call whose argument is a numeric literal.
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
        ///     Reports the helper call when the literal suffix can replace it.
        /// </summary>
        /// <param name="context">The syntax context of the invocation.</param>
        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "FromI32Unchecked" })
                return;

            if (LiteralFacts.TryGetReplacement(invocation, context.SemanticModel, out _))
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}