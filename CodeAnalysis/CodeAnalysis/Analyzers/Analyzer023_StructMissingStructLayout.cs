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
    [Analyzer(023)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer023_StructMissingStructLayout : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "023";

        private static readonly LocalizableString Title = "Struct should have StructLayout attribute";
        private static readonly LocalizableString MessageFormat = "Struct '{0}' is missing [StructLayout] attribute";
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
            var hasStructLayout = structDecl.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                {
                    var attrName = attr.Name.ToString();
                    return attrName is "StructLayout" or "StructLayoutAttribute";
                });

            if (hasStructLayout)
                return;

            var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(), structDecl.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}