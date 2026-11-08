using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires every explicit <see langword="unchecked" /> context of the analyzed
    ///     library to be replaced with a wrapping helper or an unchecked conversion helper.
    /// </summary>
    /// <remarks>
    ///     The library builds with <c>CheckForOverflowUnderflow</c>, so an inline <see langword="unchecked" />
    ///     block silently disables the property-wide protection for the operations it contains, and the
    ///     wraparound becomes indistinguishable from an unintended overflow. The expression body of a
    ///     canonical wrapping or conversion helper is the only place where <see langword="unchecked" />
    ///     is still expected, because that body is what implements the wrapping behavior for every caller.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer024_ExplicitOverflowContext : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "024";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Use project-wide checked arithmetic";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Replace the explicit unchecked context with Wrapping helpers";

        /// <summary>
        ///     Flags an explicit <see langword="unchecked" /> context
        ///     that is not the expression body of a canonical helper.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the callbacks for both forms of the <see langword="unchecked" /> context.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeContext, SyntaxKind.UncheckedExpression, SyntaxKind.UncheckedStatement);
        }

        /// <summary>
        ///     Reports the <see langword="unchecked" /> keyword of the context.
        /// </summary>
        /// <param name="context">The syntax context of the <see langword="unchecked" /> context.</param>
        /// <remarks>
        ///     Only the keyword is reported, because that is the part the developer
        ///     has to remove. An <see langword="unchecked" /> statement is never
        ///     a helper body, since the canonical helpers are expression bodied.
        /// </remarks>
        private static void AnalyzeContext(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is CheckedExpressionSyntax expression && IsHelperBody(context, expression))
                return;

            var location = context.Node switch
            {
                CheckedExpressionSyntax expressionNode => expressionNode.Keyword.GetLocation(),
                CheckedStatementSyntax statementNode => statementNode.Keyword.GetLocation(),
                _ => context.Node.GetLocation()
            };

            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }

        /// <summary>
        ///     Determines whether the <see langword="unchecked" />
        ///     expression is the complete body of a canonical helper.
        /// </summary>
        /// <param name="context">The syntax context of the <see langword="unchecked" /> expression.</param>
        /// <param name="expression">The <see langword="unchecked" /> expression to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is a helper body; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A helper body must be exactly one <see langword="unchecked" /> expression and must
        ///     not contain a nested <see langword="checked" /> context, so that the exemption
        ///     covers the wrapping implementation only and never a larger block of user code.
        /// </remarks>
        private static bool IsHelperBody(SyntaxNodeAnalysisContext context, CheckedExpressionSyntax expression) => expression.Parent is ArrowExpressionClauseSyntax { Parent: MethodDeclarationSyntax method } arrow && arrow.Expression == expression && !expression.Expression.DescendantNodesAndSelf().Any(node => node is CheckedExpressionSyntax) && HelperFacts.IsWrappingHelper(context.SemanticModel.GetDeclaredSymbol(method));
    }
}