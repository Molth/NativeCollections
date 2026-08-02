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
    [Analyzer(011)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer011_StructIEquatable : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "011";

        private static readonly LocalizableString Title = "Struct should implement IEquatable<T>";
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has public Equals({0}) but does not implement IEquatable<{0}>";
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

            if (!structDecl.Modifiers.Any(SyntaxKind.PublicKeyword) || structDecl.Parent is TypeDeclarationSyntax)
                return;

            var semanticModel = context.SemanticModel;
            var structSymbol = semanticModel.GetDeclaredSymbol(structDecl);
            if (structSymbol == null)
                return;

            var equatableGeneric = context.Compilation.GetTypeByMetadataName("System.IEquatable`1");
            if (equatableGeneric == null)
                return;

            var alreadyImplements = structSymbol.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, equatableGeneric) &&
                i.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], structSymbol));

            if (alreadyImplements)
                return;

            var hasTypedEquals = structSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(m =>
                    m.DeclaredAccessibility == Accessibility.Public &&
                    !m.IsStatic &&
                    m.Name == "Equals" &&
                    m.ReturnType.SpecialType == SpecialType.System_Boolean &&
                    m.Parameters.Length == 1 &&
                    SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, structSymbol));

            if (!hasTypedEquals)
                return;

            var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(),
                structDecl.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}