using System;
using System.Collections.Generic;
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
    ///     Requires a public struct that exposes <c>Equals(T)</c> to implement <see cref="IEquatable{T}" />.
    /// </summary>
    /// <remarks>
    ///     Without the interface the strongly typed overload is invisible to generic
    ///     containers and to <see cref="EqualityComparer{T}.Default" />, so every
    ///     comparison silently falls back to the boxing <c>Equals(object?)</c> path.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer011_StructIEquatable : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "011";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Struct should implement IEquatable<T>";

        /// <summary>
        ///     The message of the rule; the single placeholder is the struct name used three times.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has public Equals({0}) but does not implement IEquatable<{0}>";

        /// <summary>
        ///     Flags a public top-level struct with <c>Equals(T)</c> but without <see cref="IEquatable{T}" />.
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
        ///     Reports a public top-level struct that declares <c>Equals(T)</c>
        ///     but does not implement <see cref="IEquatable{T}" />.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     Reference structs and nested structs are skipped. The interface is matched through
        ///     <c>AllInterfaces</c>, so an implementation inherited through a base interface still counts.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (structDeclaration.Modifiers.Any(SyntaxKind.RefKeyword) || !structDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) || structDeclaration.Parent is TypeDeclarationSyntax)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(structDeclaration) is not { } structSymbol || context.Compilation.GetTypeByMetadataName("System.IEquatable`1") is not { } equatableType)
                return;

            var implementsEquatable = structSymbol.AllInterfaces.Any(item => SymbolEqualityComparer.Default.Equals(item.OriginalDefinition, equatableType) && item.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(item.TypeArguments[0], structSymbol));
            if (implementsEquatable)
                return;

            var hasTypedEquals = structSymbol.GetMembers("Equals").OfType<IMethodSymbol>().Any(method => method.DeclaredAccessibility == Accessibility.Public && !method.IsStatic && method.ReturnType.SpecialType == SpecialType.System_Boolean && method.Parameters.Length == 1 && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, structSymbol));
            if (hasTypedEquals)
                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText));
        }
    }
}