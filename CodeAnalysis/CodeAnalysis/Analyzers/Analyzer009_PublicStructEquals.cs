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
    ///     Requires every top-level public struct to override <c>Equals(object?)</c>.
    /// </summary>
    /// <remarks>
    ///     A struct that does not override <c>Equals(object?)</c> falls back to
    ///     <see cref="ValueType.Equals(object)" />, which boxes both operands and uses
    ///     reflection, so it is both allocating and slow. Overriding the method also keeps
    ///     the equality semantics identical to the strongly typed <c>Equals(T)</c> overload.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer009_PublicStructEquals : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "009";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Public struct should override Equals(object?)";

        /// <summary>
        ///     The message of the rule; the placeholder is the struct name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Public struct '{0}' does not override Equals(object?)";

        /// <summary>
        ///     Flags a public top-level struct without an <c>Equals(object?)</c> override.
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
        ///     Reports a public top-level struct that has no <c>public override bool Equals(object?)</c> declaration.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     Nested structs are skipped because a nested type is only reachable
        ///     through its containing type. The override is matched by syntax, which is
        ///     sufficient here because a valid override must repeat the exact signature.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (!structDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) || structDeclaration.Parent is TypeDeclarationSyntax)
                return;

            var hasEqualsOverride = structDeclaration.Members.OfType<MethodDeclarationSyntax>().Any(method => method.Modifiers.Any(SyntaxKind.PublicKeyword) && method.Modifiers.Any(SyntaxKind.OverrideKeyword) && method.Identifier.ValueText == "Equals" && method.ParameterList.Parameters.Count == 1 && method.ParameterList.Parameters[0].Type is NullableTypeSyntax { ElementType: PredefinedTypeSyntax predefinedParameter } && predefinedParameter.Keyword.IsKind(SyntaxKind.ObjectKeyword) && method.ReturnType is PredefinedTypeSyntax predefinedReturn && predefinedReturn.Keyword.IsKind(SyntaxKind.BoolKeyword));

            if (!hasEqualsOverride)
                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText));
        }
    }
}