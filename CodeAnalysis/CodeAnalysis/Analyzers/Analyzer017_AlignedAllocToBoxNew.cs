using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RS1038
#pragma warning disable RS2008

// ReSharper disable ALL

namespace Roslyn
{
    [Analyzer(017)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer017_AlignedAllocToBoxNew : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "017";

        private static readonly LocalizableString Title = "Use Box.New instead of manual alloc and copy";
        private static readonly LocalizableString MessageFormat = "Replace AlignedAlloc + Unsafe.AsRef assignment with Box.New";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;
            var statements = block.Statements;
            if (statements.Count < 4)
                return;

            var model = context.SemanticModel;
            for (var i = 0; i <= statements.Count - 4; i++)
            {
                if (!IsLocalDeclaration(statements[i], out var valueVarName, out var valueInitExpr))
                    continue;

                if (!IsLocalDeclaration(statements[i + 1], out var handleVarName, out var handleInitExpr))
                    continue;

                if (!IsExpressionStatement(statements[i + 2], out var assignExpr2) || assignExpr2 is not AssignmentExpressionSyntax assign2)
                    continue;

                if (!IsExpressionStatement(statements[i + 3], out var assignExpr3) || assignExpr3 is not AssignmentExpressionSyntax assign3)
                    continue;

                if (valueInitExpr is not ObjectCreationExpressionSyntax creation)
                    continue;

                var valueTypeSymbol = model.GetTypeInfo(creation).Type;
                if (valueTypeSymbol == null || !valueTypeSymbol.IsValueType)
                    continue;

                if (handleInitExpr is not InvocationExpressionSyntax allocInvocation)
                    continue;

                var allocMethodSymbol = model.GetSymbolInfo(allocInvocation).Symbol as IMethodSymbol;
                if (allocMethodSymbol?.Name != "AlignedAlloc" || allocMethodSymbol.ContainingType?.Name != "NativeMemoryAllocator")
                    continue;

                if (!allocMethodSymbol.IsGenericMethod || allocMethodSymbol.TypeArguments.Length != 1)
                    continue;

                if (!SymbolEqualityComparer.Default.Equals(allocMethodSymbol.TypeArguments[0], valueTypeSymbol))
                    continue;

                if (allocInvocation.ArgumentList.Arguments.Count != 1)
                    continue;

                if (allocInvocation.ArgumentList.Arguments[0].Expression.ToString() != "1")
                    continue;

                var handleTypeSymbol = model.GetTypeInfo(handleInitExpr).Type;
                if (handleTypeSymbol?.TypeKind != TypeKind.Pointer)
                    continue;

                if (!IsUnsafeAsRefAssignment(assign2, valueTypeSymbol, handleVarName!, valueVarName!, model))
                    continue;

                if (assign3.Left is not IdentifierNameSyntax fieldIdentifier)
                    continue;

                if (assign3.Right is not IdentifierNameSyntax rightId || rightId.Identifier.Text != handleVarName)
                    continue;

                var fieldSymbol = model.GetSymbolInfo(fieldIdentifier).Symbol;
                if (fieldSymbol is not IFieldSymbol && fieldSymbol is not IPropertySymbol)
                    continue;

                var fieldType = fieldSymbol is IFieldSymbol f ? f.Type : ((IPropertySymbol)fieldSymbol).Type;
                if (!SymbolEqualityComparer.Default.Equals(fieldType, handleTypeSymbol))
                    continue;

                if (HasOtherReferences(block, handleVarName, statements[i + 1], statements[i + 2], statements[i + 3]))
                    continue;

                var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(statements[i].SpanStart, statements[i + 3].Span.End));
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);

                i += 3;
            }
        }

        private static bool IsLocalDeclaration(StatementSyntax statement, out string? name, out ExpressionSyntax? initExpr)
        {
            name = null;
            initExpr = null;
            if (statement is not LocalDeclarationStatementSyntax localDecl || localDecl.Declaration.Variables.Count != 1)
                return false;

            var variable = localDecl.Declaration.Variables[0];
            name = variable.Identifier.Text;
            initExpr = variable.Initializer?.Value;
            return initExpr != null;
        }

        private static bool IsExpressionStatement(StatementSyntax statement, out ExpressionSyntax? expr)
        {
            if (statement is ExpressionStatementSyntax exprStmt)
            {
                expr = exprStmt.Expression;
                return true;
            }

            expr = null;
            return false;
        }

        private static bool IsUnsafeAsRefAssignment(AssignmentExpressionSyntax assignment, ITypeSymbol valueType, string handleVarName, string valueVarName, SemanticModel model)
        {
            if (assignment.Left is not InvocationExpressionSyntax invocation)
                return false;
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.Text != "AsRef")
                return false;

            if (memberAccess.Expression is not IdentifierNameSyntax unsafeIdentifier || unsafeIdentifier.Identifier.Text != "Unsafe")
                return false;

            var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol?.Name != "AsRef" || methodSymbol.ContainingType?.Name != "Unsafe")
                return false;

            if (!methodSymbol.IsGenericMethod || methodSymbol.TypeArguments.Length != 1)
                return false;

            if (!SymbolEqualityComparer.Default.Equals(methodSymbol.TypeArguments[0], valueType))
                return false;

            if (invocation.ArgumentList.Arguments.Count != 1)
                return false;

            if (invocation.ArgumentList.Arguments[0].Expression is not IdentifierNameSyntax argId || argId.Identifier.Text != handleVarName)
                return false;

            if (assignment.Right is not IdentifierNameSyntax rightId || rightId.Identifier.Text != valueVarName)
                return false;

            return true;
        }

        private static bool HasOtherReferences(BlockSyntax block, string varName, StatementSyntax definition, StatementSyntax use1, StatementSyntax use2)
        {
            foreach (var node in block.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (node.Identifier.Text != varName)
                    continue;

                var parentStatement = node.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (parentStatement == definition || parentStatement == use1 || parentStatement == use2)
                    continue;

                return true;
            }

            return false;
        }
    }
}