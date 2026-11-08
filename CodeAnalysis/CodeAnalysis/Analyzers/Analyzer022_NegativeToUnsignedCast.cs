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
    ///     Requires the cast of a negated value to an unsigned type to
    ///     be written inside an <see langword="unchecked" /> context.
    /// </summary>
    /// <remarks>
    ///     The analyzed library compiles with <c>CheckForOverflowUnderflow</c>, so a negated signed value that
    ///     is cast to an unsigned type throws an <see cref="OverflowException" /> at run time. The cast is only
    ///     safe when the wraparound is intended, and that intent must be stated with <see langword="unchecked" />.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer022_NegativeToUnsignedCast : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "022";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Cast of negated value to unsigned type should be unchecked";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Wrap the conversion in 'unchecked' to avoid overflow exception";

        /// <summary>
        ///     Flags an unsigned cast of a unary minus expression outside an <see langword="unchecked" /> context.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the unary minus callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeUnaryMinus, SyntaxKind.UnaryMinusExpression);
        }

        /// <summary>
        ///     Reports a cast to an unsigned type whose operand is a unary minus expression.
        /// </summary>
        /// <param name="context">The syntax context of the unary minus expression.</param>
        /// <remarks>
        ///     Both <see langword="unchecked" /> expressions and <see langword="unchecked" />
        ///     statement blocks are honored, and the innermost checked or unchecked context wins.
        ///     A <see langword="checked" /> context nested inside an <see langword="unchecked" />
        ///     one keeps its throwing behavior and is therefore still reported.
        /// </remarks>
        private static void AnalyzeUnaryMinus(SyntaxNodeAnalysisContext context)
        {
            var unaryMinus = (PrefixUnaryExpressionSyntax)context.Node;
            if (unaryMinus.Parent is not CastExpressionSyntax cast || context.SemanticModel.GetTypeInfo(cast.Type).Type is not { } targetType || !IsUnsignedInteger(targetType) || IsInsideUncheckedContext(cast))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Rule, cast.GetLocation()));
        }

        /// <summary>
        ///     Determines whether the node is inside the scope of an
        ///     <see langword="unchecked" /> expression or statement block.
        /// </summary>
        /// <param name="node">The node whose lexical context is inspected.</param>
        /// <returns>
        ///     <see langword="true" /> when the nearest overflow context is
        ///     <see langword="unchecked" />; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsInsideUncheckedContext(SyntaxNode node)
        {
            foreach (var ancestor in node.Ancestors())
            {
                switch (ancestor)
                {
                    case CheckedExpressionSyntax expression:
                        return expression.IsKind(SyntaxKind.UncheckedExpression);

                    case CheckedStatementSyntax statement:
                        return statement.IsKind(SyntaxKind.UncheckedStatement);
                }
            }

            return false;
        }

        /// <summary>
        ///     Determines whether the type is one of the unsigned integral types.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><see langword="true" /> when the type is unsigned; otherwise, <see langword="false" />.</returns>
        private static bool IsUnsignedInteger(ITypeSymbol type) => type.SpecialType is SpecialType.System_Byte or SpecialType.System_UInt16 or SpecialType.System_UInt32 or SpecialType.System_UInt64 or SpecialType.System_UIntPtr;
    }
}