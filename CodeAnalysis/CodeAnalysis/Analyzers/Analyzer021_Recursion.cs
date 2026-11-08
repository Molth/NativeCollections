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
    ///     Prohibits direct recursion in methods, constructors, operators, accessors and local functions.
    /// </summary>
    /// <remarks>
    ///     Every native collection in this library is written to be allocation free and to run in constant
    ///     stack depth. A self call is almost always an accidental copy of the intended loop body and cannot
    ///     be bounded at the call site, so it is treated as an error rather than as a style suggestion.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer021_Recursion : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "021";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Direct recursion is prohibited";

        /// <summary>
        ///     The message of the rule; the placeholder is the name of the recursive member.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "'{0}' calls itself recursively";

        /// <summary>
        ///     Flags an invocation that resolves to the member that contains it.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the callbacks for every member kind that can contain an invocation.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeConversionOperator, SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.GetAccessorDeclaration, SyntaxKind.SetAccessorDeclaration, SyntaxKind.AddAccessorDeclaration, SyntaxKind.RemoveAccessorDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
        }

        /// <summary>
        ///     Checks a method body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the method declaration.</param>
        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            if (method.Body == null && method.ExpressionBody == null)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(method) is { } methodSymbol)
                CheckForRecursion(context, method.Body ?? (CSharpSyntaxNode?)method.ExpressionBody, methodSymbol, method.Identifier.ValueText);
        }

        /// <summary>
        ///     Checks a constructor body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the constructor declaration.</param>
        /// <remarks>
        ///     A constructor initializer such as <c>this(...)</c> is not part of the
        ///     body and is therefore not visited; it is a delegation, not recursion.
        /// </remarks>
        private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            var constructor = (ConstructorDeclarationSyntax)context.Node;
            if (constructor.Body == null && constructor.ExpressionBody == null)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(constructor) is { } constructorSymbol)
                CheckForRecursion(context, constructor.Body ?? (CSharpSyntaxNode?)constructor.ExpressionBody, constructorSymbol, constructor.Identifier.ValueText);
        }

        /// <summary>
        ///     Checks an operator body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the operator declaration.</param>
        private static void AnalyzeOperator(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;
            if (operatorDeclaration.Body == null && operatorDeclaration.ExpressionBody == null)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(operatorDeclaration) is { } operatorSymbol)
                CheckForRecursion(context, operatorDeclaration.Body ?? (CSharpSyntaxNode?)operatorDeclaration.ExpressionBody, operatorSymbol, operatorDeclaration.OperatorToken.Text);
        }

        /// <summary>
        ///     Checks a conversion operator body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the conversion operator declaration.</param>
        private static void AnalyzeConversionOperator(SyntaxNodeAnalysisContext context)
        {
            var conversion = (ConversionOperatorDeclarationSyntax)context.Node;
            if (conversion.Body == null && conversion.ExpressionBody == null)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(conversion) is { } conversionSymbol)
                CheckForRecursion(context, conversion.Body ?? (CSharpSyntaxNode?)conversion.ExpressionBody, conversionSymbol, conversion.ImplicitOrExplicitKeyword.Text);
        }

        /// <summary>
        ///     Checks an accessor body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the accessor declaration.</param>
        /// <remarks>
        ///     The reported name is the name of the property or event that owns the accessor,
        ///     because the accessor itself has no identifier that a user could recognize.
        /// </remarks>
        private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
        {
            var accessor = (AccessorDeclarationSyntax)context.Node;
            if (accessor.Body == null && accessor.ExpressionBody == null)
                return;

            var memberName = accessor.Parent?.Parent switch
            {
                PropertyDeclarationSyntax property => property.Identifier.ValueText,
                EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.ValueText,
                _ => "accessor"
            };

            if (context.SemanticModel.GetDeclaredSymbol(accessor) is { } accessorSymbol)
                CheckForRecursion(context, accessor.Body ?? (CSharpSyntaxNode?)accessor.ExpressionBody, accessorSymbol, memberName);
        }

        /// <summary>
        ///     Checks a local function body for a self call.
        /// </summary>
        /// <param name="context">The syntax context of the local function statement.</param>
        private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            var localFunction = (LocalFunctionStatementSyntax)context.Node;
            if (localFunction.Body == null && localFunction.ExpressionBody == null)
                return;

            if (context.SemanticModel.GetDeclaredSymbol(localFunction) is { } localFunctionSymbol)
                CheckForRecursion(context, localFunction.Body ?? (CSharpSyntaxNode?)localFunction.ExpressionBody, localFunctionSymbol, localFunction.Identifier.ValueText);
        }

        /// <summary>
        ///     Reports every invocation inside the given body that resolves to the declaring member.
        /// </summary>
        /// <param name="context">The syntax context of the member declaration.</param>
        /// <param name="body">The body or expression body to search.</param>
        /// <param name="member">The symbol of the member that must not call itself.</param>
        /// <param name="displayName">The member name reported in the message.</param>
        /// <remarks>
        ///     Symbol equality is used instead of a name comparison, so an overload with a different
        ///     signature is not reported and a call through a receiver of the same type is.
        /// </remarks>
        private static void CheckForRecursion(SyntaxNodeAnalysisContext context, CSharpSyntaxNode? body, IMethodSymbol member, string displayName)
        {
            if (body == null)
                return;

            foreach (var invocation in body.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedSymbol && SymbolEqualityComparer.Default.Equals(invokedSymbol, member))
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), displayName));
            }
        }
    }
}