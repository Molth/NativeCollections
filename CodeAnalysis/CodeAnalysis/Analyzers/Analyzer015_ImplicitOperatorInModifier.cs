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
    [Analyzer(015)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer015_ImplicitOperatorInModifier : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "015";

        private static readonly LocalizableString Title = "Implicit operator should not use 'in' modifier";
        private static readonly LocalizableString MessageFormat = "The implicit operator '{0}' has an 'in' parameter modifier, which is not allowed";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
        }

        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
        {
            var operatorDecl = (ConversionOperatorDeclarationSyntax)context.Node;
            if (!operatorDecl.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword))
                return;

            var parameterList = operatorDecl.ParameterList;
            if (parameterList == null || parameterList.Parameters.Count == 0)
                return;

            var parameter = parameterList.Parameters[0];
            if (parameter.Modifiers.Any(SyntaxKind.InKeyword))
            {
                var operatorName = $"implicit operator {operatorDecl.Type}";
                var diagnostic = Diagnostic.Create(Rule, parameter.GetLocation(), operatorName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}