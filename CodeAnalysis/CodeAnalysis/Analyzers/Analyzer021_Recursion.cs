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
    [Analyzer(021)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer021_Recursion : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "021";

        private static readonly LocalizableString Title = "Direct recursion is prohibited";
        private static readonly LocalizableString MessageFormat = "'{0}' calls itself recursively";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);

            context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.GetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.SetAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.AddAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            if (method.Body == null && method.ExpressionBody == null)
                return;

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
                return;

            CheckForRecursion(context, method.Body ?? (CSharpSyntaxNode?)method.ExpressionBody, methodSymbol, method.Identifier.ValueText);
        }

        private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            var ctor = (ConstructorDeclarationSyntax)context.Node;
            if (ctor.Body == null && ctor.ExpressionBody == null)
                return;

            var ctorSymbol = context.SemanticModel.GetDeclaredSymbol(ctor);
            if (ctorSymbol == null)
                return;

            CheckForRecursion(context, ctor.Body ?? (CSharpSyntaxNode?)ctor.ExpressionBody, ctorSymbol, ctor.Identifier.ValueText);
        }

        private static void AnalyzeOperator(SyntaxNodeAnalysisContext context)
        {
            var op = (OperatorDeclarationSyntax)context.Node;
            if (op.Body == null && op.ExpressionBody == null)
                return;

            var opSymbol = context.SemanticModel.GetDeclaredSymbol(op);
            if (opSymbol == null)
                return;

            CheckForRecursion(context, op.Body ?? (CSharpSyntaxNode?)op.ExpressionBody, opSymbol, op.OperatorToken.Text);
        }

        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
        {
            var conv = (ConversionOperatorDeclarationSyntax)context.Node;
            if (conv.Body == null && conv.ExpressionBody == null)
                return;

            var convSymbol = context.SemanticModel.GetDeclaredSymbol(conv);
            if (convSymbol == null)
                return;

            CheckForRecursion(context, conv.Body ?? (CSharpSyntaxNode?)conv.ExpressionBody, convSymbol, conv.ImplicitOrExplicitKeyword.Text);
        }

        private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
        {
            var accessor = (AccessorDeclarationSyntax)context.Node;
            if (accessor.Body == null && accessor.ExpressionBody == null)
                return;

            var accessorSymbol = context.SemanticModel.GetDeclaredSymbol(accessor);
            if (accessorSymbol == null)
                return;

            var propertyOrEvent = accessor.Parent?.Parent;
            var memberName = propertyOrEvent switch
            {
                PropertyDeclarationSyntax prop => prop.Identifier.ValueText,
                EventDeclarationSyntax evt => evt.Identifier.ValueText,
                _ => "accessor"
            };

            CheckForRecursion(context, accessor.Body ?? (CSharpSyntaxNode?)accessor.ExpressionBody, accessorSymbol, memberName);
        }

        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            var localFunc = (LocalFunctionStatementSyntax)context.Node;
            if (localFunc.Body == null && localFunc.ExpressionBody == null)
                return;

            var localFuncSymbol = context.SemanticModel.GetDeclaredSymbol(localFunc);
            if (localFuncSymbol == null)
                return;

            CheckForRecursion(context, localFunc.Body ?? (CSharpSyntaxNode?)localFunc.ExpressionBody, localFuncSymbol, localFunc.Identifier.ValueText);
        }

        private static void CheckForRecursion(SyntaxNodeAnalysisContext context, CSharpSyntaxNode? node, IMethodSymbol currentMethod, string displayName)
        {
            if (node == null)
                return;

            foreach (var invocation in node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedSymbol)
                {
                    if (SymbolEqualityComparer.Default.Equals(invokedSymbol, currentMethod))
                    {
                        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), displayName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}