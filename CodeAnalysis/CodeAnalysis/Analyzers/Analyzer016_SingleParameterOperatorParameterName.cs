using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires the parameter of a single-parameter operator or conversion operator to be named <c>value</c>.
    /// </summary>
    /// <remarks>
    ///     Unary operators and conversion operators are attribute targets, so their parameter name
    ///     is part of the metadata that documentation and callers refer to. A single consistent
    ///     name avoids signature churn and keeps the generated operator documentation uniform.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer016_SingleParameterOperatorParameterName : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "016";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Single-parameter operator should name its parameter 'value'";

        /// <summary>
        ///     The message of the rule; the placeholder is the current parameter name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Rename parameter '{0}' to 'value' for clarity";

        /// <summary>
        ///     Flags an operator whose only parameter is not named <c>value</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the operator and conversion operator callbacks.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
        }

        /// <summary>
        ///     Checks the parameter list of a unary operator declaration.
        /// </summary>
        /// <param name="context">The syntax context of the operator declaration.</param>
        private static void AnalyzeOperator(SyntaxNodeAnalysisContext context) => CheckParameter(((OperatorDeclarationSyntax)context.Node).ParameterList, context);

        /// <summary>
        ///     Checks the parameter list of a conversion operator declaration.
        /// </summary>
        /// <param name="context">The syntax context of the conversion operator declaration.</param>
        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context) => CheckParameter(((ConversionOperatorDeclarationSyntax)context.Node).ParameterList, context);

        /// <summary>
        ///     Reports the single parameter of the given list when it is not named <c>value</c>.
        /// </summary>
        /// <param name="parameterList">The parameter list of the operator declaration.</param>
        /// <param name="context">The syntax context of the operator declaration.</param>
        /// <remarks>
        ///     Binary operators are skipped because their two parameters conventionally
        ///     describe the left and right operands rather than a single input value.
        /// </remarks>
        private static void CheckParameter(BaseParameterListSyntax? parameterList, SyntaxNodeAnalysisContext context)
        {
            if (parameterList == null || parameterList.Parameters.Count != 1)
                return;

            var parameter = parameterList.Parameters[0];
            if (parameter.Identifier.ValueText != "value")
                context.ReportDiagnostic(Diagnostic.Create(Rule, parameter.Identifier.GetLocation(), parameter.Identifier.ValueText));
        }
    }
}