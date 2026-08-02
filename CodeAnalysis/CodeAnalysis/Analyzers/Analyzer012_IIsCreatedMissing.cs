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
    [Analyzer(012)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer012_IIsCreatedMissing : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "012";

        private static readonly LocalizableString Title = "Struct with IsCreated property should implement IIsCreated";
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a public bool IsCreated property but does not implement NativeCollections.IIsCreated";
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

            if (!structDecl.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            var semanticModel = context.SemanticModel;
            var structSymbol = semanticModel.GetDeclaredSymbol(structDecl);
            if (structSymbol == null)
                return;

            var iisCreatedType = context.Compilation.GetTypeByMetadataName("NativeCollections.IIsCreated");
            if (iisCreatedType == null)
                return;

            var alreadyImplements = structSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iisCreatedType));
            if (alreadyImplements)
                return;

            var isCreatedProperty = structSymbol.GetMembers("IsCreated")
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p =>
                    p.DeclaredAccessibility == Accessibility.Public &&
                    !p.IsStatic &&
                    p.Type.SpecialType == SpecialType.System_Boolean &&
                    p.GetMethod != null &&
                    p.SetMethod == null);

            if (isCreatedProperty == null)
                return;

            var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(), structDecl.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}