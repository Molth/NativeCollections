using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS1038
#pragma warning disable RS2008

// ReSharper disable ALL

namespace Roslyn
{
    [Analyzer(009)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer009_PublicStructEquals : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "009";

        private static readonly LocalizableString Title = "Public struct should override Equals(object?)";
        private static readonly LocalizableString MessageFormat = "Public struct '{0}' does not override Equals(object?)";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDecl = (StructDeclarationSyntax)context.Node;
            if (!structDecl.Modifiers.Any(SyntaxKind.PublicKeyword) || structDecl.Parent is TypeDeclarationSyntax)
                return;

            var hasEqualsOverride = structDecl.Members
                .OfType<MethodDeclarationSyntax>()
                .Any(m =>
                    m.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                    m.Modifiers.Any(SyntaxKind.OverrideKeyword) &&
                    m.Identifier.ValueText == "Equals" &&
                    m.ParameterList.Parameters.Count == 1 &&
                    m.ParameterList.Parameters[0].Type is NullableTypeSyntax nullableType &&
                    nullableType.ElementType is PredefinedTypeSyntax predefined &&
                    predefined.Keyword.IsKind(SyntaxKind.ObjectKeyword) &&
                    m.ReturnType is PredefinedTypeSyntax retPredefined &&
                    retPredefined.Keyword.IsKind(SyntaxKind.BoolKeyword));

            if (!hasEqualsOverride)
            {
                var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(), structDecl.Identifier.ValueText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}