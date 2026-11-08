using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Prohibits the <see langword="in" /> modifier on the parameter of an implicit conversion operator.
    /// </summary>
    /// <remarks>
    ///     An implicit conversion is consumed by the compiler at an arbitrary use site, where the argument is not
    ///     guaranteed to be a variable, so the <see langword="in" /> modifier cannot be satisfied reliably. It also
    ///     prevents the compiler from treating the conversion as a simple value conversion in some generic contexts.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer015_ImplicitOperatorInModifier : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "015";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Implicit operator should not use 'in' modifier";

        /// <summary>
        ///     The message of the rule; the placeholder is the signature of the conversion operator.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "The implicit operator '{0}' has an 'in' parameter modifier, which is not allowed";

        /// <summary>
        ///     Flags an implicit conversion operator whose parameter uses the <see langword="in" /> modifier.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the conversion operator callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
        }

        /// <summary>
        ///     Reports an implicit conversion operator whose first parameter has the <see langword="in" /> modifier.
        /// </summary>
        /// <param name="context">The syntax context of the conversion operator declaration.</param>
        /// <remarks>
        ///     Explicit conversion operators are skipped on purpose; they are always written
        ///     explicitly at the call site, where the argument is stable enough for the modifier.
        /// </remarks>
        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (ConversionOperatorDeclarationSyntax)context.Node;
            if (!operatorDeclaration.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword))
                return;

            var parameter = operatorDeclaration.ParameterList.Parameters.FirstOrDefault();
            if (parameter is { } firstParameter && firstParameter.Modifiers.Any(SyntaxKind.InKeyword))
                context.ReportDiagnostic(Diagnostic.Create(Rule, firstParameter.GetLocation(), $"implicit operator {operatorDeclaration.Type}"));
        }
    }
}