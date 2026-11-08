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
    ///     Requires a public type to override <c>ToString()</c> with its own implementation.
    /// </summary>
    /// <remarks>
    ///     A type without an override prints its namespace-qualified metadata name, which makes logs and failure
    ///     messages hard to read. The override may name the type — as a literal, a <see langword="nameof" />
    ///     or an <c>SR.Format</c> format that starts with the type name — or it may describe the value
    ///     that the type wraps, such as the text of a string type or the elapsed time of a stopwatch type.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer010_PublicTypeMissingToString : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "010";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Public type should override ToString()";

        /// <summary>
        ///     The message of the rule; the placeholder is the type name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Type '{0}' does not override ToString() or the override does not describe the type";

        /// <summary>
        ///     Flags a public type without a usable <c>ToString()</c> override.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The nested collection type names that are exempt from the rule.
        /// </summary>
        /// <remarks>
        ///     These types are implementation details of their owning collection and are
        ///     never formatted directly, so requiring an override would only add dead code.
        /// </remarks>
        private static readonly string[] IgnoredNames = ["Enumerator", "KeyCollection", "ValueCollection", "UnorderedItemsCollection", "OrderedKeyCollection", "OrderedValueCollection", "OrderedKeyValuePairCollection"];

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the class and struct callbacks.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        /// <summary>
        ///     Reports a public type whose <c>ToString()</c> override is missing or does not describe the type.
        /// </summary>
        /// <param name="context">The syntax context of the type declaration.</param>
        /// <remarks>
        ///     Static types, attribute types and nested types of non-public types are skipped, because they
        ///     are not part of the public surface or are already covered by the rule on their containing type.
        /// </remarks>
        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDeclaration = (TypeDeclarationSyntax)context.Node;
            if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not { IsStatic: false, DeclaredAccessibility: Accessibility.Public } typeSymbol)
                return;

            var attributeType = context.Compilation.GetTypeByMetadataName("System.Attribute");
            if (attributeType != null && SymbolEqualityComparer.Default.Equals(typeSymbol.BaseType, attributeType))
                return;

            if (IgnoredNames.Contains(typeSymbol.Name))
                return;

            if (typeDeclaration.Parent is TypeDeclarationSyntax parentDeclaration && context.SemanticModel.GetDeclaredSymbol(parentDeclaration) is { DeclaredAccessibility: not Accessibility.Public })
                return;

            var toStringMethod = typeSymbol.GetMembers("ToString").OfType<IMethodSymbol>().FirstOrDefault(method => method is { IsStatic: false, IsOverride: true, Parameters.Length: 0, ReturnType.SpecialType: SpecialType.System_String } && method.DeclaredAccessibility == Accessibility.Public);
            if (toStringMethod?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
            {
                Report(context, typeDeclaration);
                return;
            }

            if (GetReturnedExpressions(methodSyntax).Any(expression => !IsAcceptableRepresentation(expression, typeSymbol)))
                Report(context, typeDeclaration);
        }

        /// <summary>
        ///     Gets the expressions that the <c>ToString()</c> body returns.
        /// </summary>
        /// <param name="methodSyntax">The <c>ToString()</c> declaration.</param>
        /// <returns>The returned expressions, which may be empty when the body has no return statement.</returns>
        /// <remarks>
        ///     Both the expression body and every return statement of a block body are inspected, because
        ///     an implementation may branch on the type generic argument before it composes the result.
        /// </remarks>
        private static ImmutableArray<ExpressionSyntax> GetReturnedExpressions(MethodDeclarationSyntax methodSyntax)
        {
            var expressionBody = methodSyntax.ExpressionBody?.Expression;
            if (expressionBody != null)
                return [expressionBody];

            return methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>().Select(statement => statement.Expression).OfType<ExpressionSyntax>().ToImmutableArray();
        }

        /// <summary>
        ///     Determines whether the returned expression is a usable representation of the value.
        /// </summary>
        /// <param name="expression">The expression returned by <c>ToString()</c>.</param>
        /// <param name="typeSymbol">The type that declares <c>ToString()</c>.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression describes the value; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The inherited implementation is rejected, and so is a literal or a format string
        ///     that does not start with the type name, because both read as the name of another
        ///     type. Any other expression — a delegation to the wrapped value, an interpolation
        ///     or a conditional composition — is accepted as an intentional representation.
        /// </remarks>
        private static bool IsAcceptableRepresentation(ExpressionSyntax expression, INamedTypeSymbol typeSymbol)
        {
            if (expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } })
                return false;

            if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                return literal.Token.ValueText.StartsWith(typeSymbol.Name, StringComparison.Ordinal);

            if (expression is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "SR" }, Name.Identifier.ValueText: "Format" } } invocation)
                return true;

            return invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression is not LiteralExpressionSyntax formatLiteral || !formatLiteral.IsKind(SyntaxKind.StringLiteralExpression) || formatLiteral.Token.ValueText.StartsWith(typeSymbol.Name, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Reports <c>DESIGN010</c> on the identifier of the type.
        /// </summary>
        /// <param name="context">The syntax context of the type declaration.</param>
        /// <param name="typeDeclaration">The type that misses or mistypes the override.</param>
        private static void Report(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDeclaration) => context.ReportDiagnostic(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(), typeDeclaration.Identifier.ValueText));
    }
}