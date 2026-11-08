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
    ///     Requires every struct declaration to carry a <c>StructLayout</c> attribute.
    /// </summary>
    /// <remarks>
    ///     Native collections depend on the exact field order and packing of their structs. An implicit
    ///     layout is chosen by the runtime and may differ between target frameworks and platforms,
    ///     which changes the meaning of the raw memory that the collections address directly.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer023_StructMissingStructLayout : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "023";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Struct should have StructLayout attribute";

        /// <summary>
        ///     The message of the rule; the placeholder is the struct name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Struct '{0}' is missing [StructLayout] attribute";

        /// <summary>
        ///     Flags a struct declaration that has no <c>StructLayout</c> attribute.
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
        ///     Reports a struct declaration without a <c>StructLayout</c> attribute.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     The attribute is matched on the rightmost identifier of its name, so both the short form and
        ///     a namespace-qualified form are accepted, together with the optional <c>Attribute</c> suffix.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            var hasStructLayout = structDeclaration.AttributeLists.SelectMany(list => list.Attributes).Any(attribute => GetRightmostIdentifier(attribute.Name) is "StructLayout" or "StructLayoutAttribute");
            if (!hasStructLayout)
                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText));
        }

        /// <summary>
        ///     Gets the rightmost identifier of an attribute name.
        /// </summary>
        /// <param name="name">The attribute name, which may be qualified or alias qualified.</param>
        /// <returns>The rightmost identifier text, or the full name text when the shape is not recognized.</returns>
        private static string GetRightmostIdentifier(NameSyntax name) => name switch
        {
            QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name.Identifier.ValueText,
            SimpleNameSyntax simple => simple.Identifier.ValueText,
            _ => name.ToString()
        };
    }
}