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
    [Analyzer(010)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer010_PublicTypeMissingToString : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "010";

        private static readonly LocalizableString Title = "Public type should override ToString() correctly";
        private static readonly LocalizableString MessageFormat = "Type '{0}' does not override ToString() or the implementation is incorrect";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        private static readonly string[] IgnoredNames =
        [
            "Enumerator", "KeyCollection", "ValueCollection",
            "UnorderedItemsCollection", "OrderedKeyCollection",
            "OrderedValueCollection", "OrderedKeyValuePairCollection"
        ];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
        {
            var typeDecl = (TypeDeclarationSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetDeclaredSymbol(typeDecl);
            if (typeSymbol == null || typeSymbol.IsStatic || typeSymbol.DeclaredAccessibility != Accessibility.Public)
                return;

            var attrType = context.Compilation.GetTypeByMetadataName("System.Attribute");
            if (attrType != null && SymbolEqualityComparer.Default.Equals(typeSymbol.BaseType, attrType))
                return;

            if (IgnoredNames.Contains(typeSymbol.Name))
                return;

            if (typeDecl.Parent is TypeDeclarationSyntax parentTypeDecl)
            {
                var parentSymbol = context.SemanticModel.GetDeclaredSymbol(parentTypeDecl);
                if (parentSymbol != null && parentSymbol.DeclaredAccessibility != Accessibility.Public)
                    return;
            }

            var toStringMethod = typeSymbol.GetMembers("ToString")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    m is { IsStatic: false, IsOverride: true, Parameters.Length: 0, ReturnType.SpecialType: SpecialType.System_String } &&
                    m.DeclaredAccessibility == Accessibility.Public);

            if (toStringMethod == null)
            {
                Report(context, typeDecl);
                return;
            }

            var methodSyntax = toStringMethod.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
            if (methodSyntax == null)
                return;

            ExpressionSyntax? expression = null;
            if (methodSyntax.ExpressionBody != null)
                expression = methodSyntax.ExpressionBody.Expression;
            else if (methodSyntax.Body?.Statements.Count == 1 && methodSyntax.Body.Statements[0] is ReturnStatementSyntax returnStmt)
                expression = returnStmt.Expression;

            if (expression == null || !IsToStringCorrect(expression, typeSymbol))
                Report(context, typeDecl);
        }

        private static bool IsToStringCorrect(ExpressionSyntax expression, INamedTypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.Name;
            var isGeneric = typeSymbol.TypeParameters.Length > 0;
            if (isGeneric)
            {
                if (expression is InvocationExpressionSyntax invocation && invocation.Expression is MemberAccessExpressionSyntax ma)
                {
                    if (ma.Expression is IdentifierNameSyntax { Identifier.ValueText: "SR" } && ma.Name.Identifier.ValueText == "Format" && invocation.ArgumentList.Arguments.Count > 0)
                    {
                        var firstArg = invocation.ArgumentList.Arguments[0].Expression;
                        if (firstArg is LiteralExpressionSyntax literal1 && literal1.IsKind(SyntaxKind.StringLiteralExpression) && literal1.Token.ValueText.StartsWith(typeName))
                            return true;
                    }
                }

                return false;
            }

            if (expression is LiteralExpressionSyntax literal2 && literal2.IsKind(SyntaxKind.StringLiteralExpression) && literal2.Token.ValueText == typeName)
                return true;

            if (expression is InvocationExpressionSyntax nameofInv && nameofInv.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" })
                return true;

            return false;
        }

        private static void Report(SyntaxNodeAnalysisContext context, TypeDeclarationSyntax typeDecl)
        {
            var diagnostic = Diagnostic.Create(Rule, typeDecl.Identifier.GetLocation(), typeDecl.Identifier.ValueText);
            context.ReportDiagnostic(diagnostic);
        }
    }
}