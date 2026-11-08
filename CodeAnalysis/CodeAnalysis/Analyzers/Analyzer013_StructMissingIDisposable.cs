using System;
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
    ///     Requires a struct that exposes <c>Dispose()</c> to implement <see cref="IDisposable" />.
    /// </summary>
    /// <remarks>
    ///     A struct with a <c>Dispose()</c> method but no interface is not released by <see langword="using" />
    ///     statements, <see langword="foreach" /> disposal or generic helpers constrained to
    ///     <see cref="IDisposable" />, so its native allocation leaks whenever it is passed to such a helper.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer013_StructMissingIDisposable : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "013";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Struct with Dispose() should implement IDisposable";

        /// <summary>
        ///     The message of the rule; the placeholder is the struct name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a public Dispose() method but does not implement IDisposable";

        /// <summary>
        ///     Flags a struct with a public parameterless <c>Dispose()</c> but without <see cref="IDisposable" />.
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
        ///     Reports a struct that declares <c>Dispose()</c> but does not implement <see cref="IDisposable" />.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     Reference structs are skipped. The method must be public, instance and
        ///     parameterless, because any other shape cannot satisfy the interface member.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (structDeclaration.Modifiers.Any(SyntaxKind.RefKeyword))
                return;

            if (context.SemanticModel.GetDeclaredSymbol(structDeclaration) is not { } structSymbol || context.Compilation.GetTypeByMetadataName("System.IDisposable") is not { } disposableType)
                return;

            if (structSymbol.AllInterfaces.Any(item => SymbolEqualityComparer.Default.Equals(item, disposableType)))
                return;

            var disposeMethod = structSymbol.GetMembers("Dispose").OfType<IMethodSymbol>().FirstOrDefault(method => method.DeclaredAccessibility == Accessibility.Public && !method.IsStatic && method.ReturnsVoid && method.Parameters.Length == 0);
            if (disposeMethod != null)
                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText));
        }
    }
}