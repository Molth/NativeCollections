using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires a public struct that exposes <c>IsCreated</c> to implement <c>NativeCollections.IIsCreated</c>.
    /// </summary>
    /// <remarks>
    ///     <c>IIsCreated</c> is the contract used to test whether a native collection still owns its
    ///     allocation. A struct that declares the property without the interface cannot be validated
    ///     through the shared generic helpers and hides that it participates in the ownership contract.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer012_IIsCreatedMissing : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "012";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Struct with IsCreated property should implement IIsCreated";

        /// <summary>
        ///     The message of the rule; the placeholder is the struct name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a public bool IsCreated property but does not implement NativeCollections.IIsCreated";

        /// <summary>
        ///     Flags a public struct with a get-only <c>bool IsCreated</c> property but without the interface.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the struct callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        }

        /// <summary>
        ///     Reports a public struct that declares <c>IsCreated</c> but does not implement <c>IIsCreated</c>.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     Reference structs are skipped. Only a get-only property counts, because the
        ///     interface exposes a read-only member and a settable property would not satisfy it.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (structDeclaration.Modifiers.Any(SyntaxKind.RefKeyword) || !structDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            if (context.SemanticModel.GetDeclaredSymbol(structDeclaration) is not { } structSymbol || context.Compilation.GetTypeByMetadataName(SR.NATIVE_COLLECTIONS_NAMESPACE + ".IIsCreated") is not { } isCreatedType)
                return;

            if (structSymbol.AllInterfaces.Any(item => SymbolEqualityComparer.Default.Equals(item, isCreatedType)))
                return;

            var isCreatedProperty = structSymbol.GetMembers("IsCreated").OfType<IPropertySymbol>().FirstOrDefault(property => property.DeclaredAccessibility == Accessibility.Public && !property.IsStatic && property.Type.SpecialType == SpecialType.System_Boolean && property.GetMethod != null && property.SetMethod == null);
            if (isCreatedProperty != null)
                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText));
        }
    }
}