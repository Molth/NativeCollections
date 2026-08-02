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
    [Analyzer(013)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer013_StructMissingIDisposable : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "013";

        private static readonly LocalizableString Title = "Struct with Dispose() should implement IDisposable";
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a public Dispose() method but does not implement IDisposable";
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
            if (structDecl.Modifiers.Any(SyntaxKind.RefKeyword))
                return;

            var semanticModel = context.SemanticModel;
            var structSymbol = semanticModel.GetDeclaredSymbol(structDecl);
            if (structSymbol == null)
                return;

            var disposableType = context.Compilation.GetTypeByMetadataName("System.IDisposable");
            if (disposableType == null)
                return;

            var alreadyImplements = structSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, disposableType));
            if (alreadyImplements)
                return;

            var disposeMethod = structSymbol.GetMembers("Dispose")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    m.DeclaredAccessibility == Accessibility.Public &&
                    !m.IsStatic &&
                    m.ReturnsVoid &&
                    m.Parameters.Length == 0);

            if (disposeMethod == null)
                return;

            var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(), structDecl.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}