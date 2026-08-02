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
    [Analyzer(016)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer016_SingleParameterOperatorParameterName : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "016";

        private static readonly LocalizableString Title = "Single-parameter operator should name its parameter 'value'";
        private static readonly LocalizableString MessageFormat = "Rename parameter '{0}' to 'value' for clarity";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
        }

        private static void AnalyzeOperator(SyntaxNodeAnalysisContext context)
        {
            var opDecl = (OperatorDeclarationSyntax)context.Node;
            CheckParameter(opDecl.ParameterList, context, opDecl.OperatorToken);
        }

        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
        {
            var convDecl = (ConversionOperatorDeclarationSyntax)context.Node;
            CheckParameter(convDecl.ParameterList, context, convDecl.ImplicitOrExplicitKeyword);
        }

        private static void CheckParameter(BaseParameterListSyntax? parameterList, SyntaxNodeAnalysisContext context, SyntaxToken keywordToken)
        {
            if (parameterList == null || parameterList.Parameters.Count != 1)
                return;

            var parameter = parameterList.Parameters[0];
            var paramName = parameter.Identifier.ValueText;
            if (paramName == "value")
                return;

            var diagnostic = Diagnostic.Create(Rule, parameter.Identifier.GetLocation(), paramName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}