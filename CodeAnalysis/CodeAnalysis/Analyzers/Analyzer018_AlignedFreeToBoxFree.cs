using System.Collections.Immutable;
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
    [Analyzer(018)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer018_AlignedFreeToBoxFree : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "018";

        private static readonly LocalizableString Title = "Use Box.Free instead of AlignedFree";
        private static readonly LocalizableString MessageFormat = "Replace AlignedFree pattern with Box.Free";
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
            if (statements.Count < 3)
                return;

            for (var i = 0; i <= statements.Count - 3; i++)
            {
                var stmt1 = statements[i];
                var stmt2 = statements[i + 1];
                var stmt3 = statements[i + 2];

                if (!TryGetSimpleAssignment(stmt1, out var localName, out var fieldExpression))
                    continue;

                if (!IsNullCheckWithReturn(stmt2, localName!, context.SemanticModel))
                    continue;

                if (!IsAlignedFreeCall(stmt3, localName!, context.SemanticModel))
                    continue;

                if (fieldExpression is not IdentifierNameSyntax && fieldExpression is not MemberAccessExpressionSyntax)
                    continue;

                var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(stmt1.SpanStart, stmt3.Span.End));
                var diagnostic = Diagnostic.Create(Rule, location);
                context.ReportDiagnostic(diagnostic);

                i += 2;
            }
        }

        private static bool TryGetSimpleAssignment(StatementSyntax stmt, out string? localName, out ExpressionSyntax? assignedExpression)
        {
            localName = null;
            assignedExpression = null;
            if (stmt is LocalDeclarationStatementSyntax localDecl && localDecl.Declaration.Variables.Count == 1)
            {
                var variable = localDecl.Declaration.Variables[0];
                localName = variable.Identifier.Text;
                assignedExpression = variable.Initializer?.Value;
                return assignedExpression != null;
            }

            if (stmt is ExpressionStatementSyntax exprStmt && exprStmt.Expression is AssignmentExpressionSyntax assign)
            {
                if (assign.Kind() == SyntaxKind.SimpleAssignmentExpression)
                {
                    if (assign.Left is IdentifierNameSyntax id)
                    {
                        localName = id.Identifier.Text;
                        assignedExpression = assign.Right;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsNullCheckWithReturn(StatementSyntax stmt, string variableName, SemanticModel model)
        {
            if (stmt is not IfStatementSyntax ifStmt)
                return false;

            if (ifStmt.Else != null)
                return false;

            if (ifStmt.Condition is not InvocationExpressionSyntax invocation)
                return false;

            var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol?.Name != "IsNull" || methodSymbol.ContainingType?.Name != "UnsafeHelpers")
                return false;

            if (invocation.ArgumentList.Arguments.Count != 1)
                return false;

            if (invocation.ArgumentList.Arguments[0].Expression is not IdentifierNameSyntax argId || argId.Identifier.Text != variableName)
                return false;

            if (ifStmt.Statement is BlockSyntax thenBlock)
            {
                if (thenBlock.Statements.Count != 1)
                    return false;

                return thenBlock.Statements[0] is ReturnStatementSyntax returnStmt && returnStmt.Expression == null;
            }

            if (ifStmt.Statement is ReturnStatementSyntax simpleReturn)
                return simpleReturn.Expression == null;

            return false;
        }

        private static bool IsAlignedFreeCall(StatementSyntax stmt, string variableName, SemanticModel model)
        {
            if (stmt is not ExpressionStatementSyntax exprStmt)
                return false;

            if (exprStmt.Expression is not InvocationExpressionSyntax invocation)
                return false;

            var methodSymbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            if (methodSymbol?.Name != "AlignedFree" || methodSymbol.ContainingType?.Name != "NativeMemoryAllocator")
                return false;

            if (invocation.ArgumentList.Arguments.Count != 1)
                return false;

            return invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax argId && argId.Identifier.Text == variableName;
        }
    }
}