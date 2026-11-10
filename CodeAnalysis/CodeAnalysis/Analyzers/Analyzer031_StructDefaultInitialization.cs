using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires a struct value that is already zero-initialized to be written with the <c>default</c> literal.
    /// </summary>
    /// <remarks>
    ///     A struct whose parameterless constructor is the implicit one is zero-initialized by <c>new S()</c>,
    ///     and <c>default(S)</c> is that same value, so both spellings repeat what <c>default</c> already states.
    ///     A local declaration also states the type instead of hiding it behind <c>var</c>, which is the type the
    ///     reader needs to reason about the layout of the value. A struct that declares its own parameterless
    ///     constructor is skipped, because <c>new S()</c> then runs that constructor and no longer produces the
    ///     default value.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer031_StructDefaultInitialization : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The prefix shared by the two rules of this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "031";

        /// <summary>
        ///     The format used to write the name of the struct type in the message.
        /// </summary>
        private static readonly SymbolDisplayFormat TypeNameFormat = SymbolDisplayFormat.MinimallyQualifiedFormat;

        /// <summary>
        ///     The title of <c>DESIGN031_1</c>.
        /// </summary>
        private static readonly LocalizableString Title001 = "Struct variable is initialized with a redundant expression";

        /// <summary>
        ///     The message of <c>DESIGN031_1</c>, formatted with the type name and the variable name.
        /// </summary>
        private static readonly LocalizableString MessageFormat001 = "Replace the initialization with '{0} {1} = default;'";

        /// <summary>
        ///     Flags a <c>var</c> declaration whose initializer is <c>new S()</c> or <c>default(S)</c> for a struct <c>S</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>DESIGN031_2</c>.
        /// </summary>
        private static readonly LocalizableString Title002 = "Struct value is created instead of written as default";

        /// <summary>
        ///     The message of <c>DESIGN031_2</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat002 = "Replace the struct creation with 'default'";

        /// <summary>
        ///     Flags an expression body whose value is <c>new S()</c> or <c>default(S)</c> for a struct <c>S</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the two rules reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002];

        /// <summary>
        ///     Registers the object creation and the default expression callbacks.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
        }

        /// <summary>
        ///     Reports an object creation that produces the default value of a struct.
        /// </summary>
        /// <param name="context">The syntax context of the object creation.</param>
        /// <remarks>
        ///     The creation must not pass an argument, because an argument changes the produced value. A creation whose
        ///     constructor is declared by the struct itself is skipped, because that constructor can assign a value
        ///     that differs from <c>default</c>.
        /// </remarks>
        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var creation = (BaseObjectCreationExpressionSyntax)context.Node;
            if (creation.ArgumentList is { Arguments.Count: > 0 } || creation.Initializer != null)
                return;

            if (context.SemanticModel.GetSymbolInfo(creation).Symbol is IMethodSymbol { IsImplicitlyDeclared: false })
                return;

            if (context.SemanticModel.GetTypeInfo(creation).Type is not { } type || !IsStructType(type))
                return;

            Report(context, creation, type);
        }

        /// <summary>
        ///     Reports a <c>default(T)</c> expression whose type is a struct.
        /// </summary>
        /// <param name="context">The syntax context of the default expression.</param>
        private static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var expression = (DefaultExpressionSyntax)context.Node;
            if (context.SemanticModel.GetTypeInfo(expression).Type is not { } type || !IsStructType(type))
                return;

            Report(context, expression, type);
        }

        /// <summary>
        ///     Reports the expression when it is the initializer of a <c>var</c> declaration
        ///     or the value of an expression body that returns the struct.
        /// </summary>
        /// <param name="context">The syntax context of the analyzed expression.</param>
        /// <param name="expression">The expression that produces the default value.</param>
        /// <param name="type">The struct type of the produced value.</param>
        /// <remarks>
        ///     The declaration form is reported first, because it also replaces the implicit type of <c>var</c>
        ///     with the struct type. Any other position keeps the declaration untouched and is reported only when
        ///     the expression body converts the value to exactly its own type, so that <c>default</c> states the
        ///     same value instead of a value of another type.
        /// </remarks>
        private static void Report(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, ITypeSymbol type)
        {
            if (GetVarDeclarator(expression) is { } declarator)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule001, expression.GetLocation(), type.ToDisplayString(TypeNameFormat), declarator.Identifier.ValueText));
                return;
            }

            if (IsDefaultLiteralTarget(context.SemanticModel, expression))
                context.ReportDiagnostic(Diagnostic.Create(Rule002, expression.GetLocation()));
        }

        /// <summary>
        ///     Gets the declarator of the <c>var</c> declaration that the expression initializes.
        /// </summary>
        /// <param name="expression">The initializer expression of a declaration.</param>
        /// <returns>
        ///     The declarator, or <see langword="null" /> when the expression does not initialize a <c>var</c> variable.
        /// </returns>
        private static VariableDeclaratorSyntax? GetVarDeclarator(ExpressionSyntax expression) => expression.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } declarator } && declaration.Type.IsVar ? declarator : null;

        /// <summary>
        ///     Determines whether the expression is the value of an expression body that returns
        ///     exactly the type of the expression.
        /// </summary>
        /// <param name="semanticModel">The semantic model used to read the conversion of the expression.</param>
        /// <param name="expression">The expression to inspect.</param>
        /// <returns>
        ///     <see langword="true" /> when the enclosing expression body converts the value
        ///     to its own type; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The equality of the type and the converted type excludes a body that converts the struct to
        ///     another type, such as <c>object</c>, where <c>default</c> would describe a different value.
        /// </remarks>
        private static bool IsDefaultLiteralTarget(SemanticModel semanticModel, ExpressionSyntax expression)
        {
            if (expression.Parent is not (ArrowExpressionClauseSyntax or LambdaExpressionSyntax))
                return false;

            var info = semanticModel.GetTypeInfo(expression);
            return info.Type != null && SymbolEqualityComparer.Default.Equals(info.Type, info.ConvertedType);
        }

        /// <summary>
        ///     Determines whether the type is a struct or a type parameter constrained to a value type.
        /// </summary>
        /// <param name="type">The type to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the type is a value type that <c>new</c> zero-initializes;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsStructType(ITypeSymbol type) => type switch
        {
            INamedTypeSymbol { TypeKind: TypeKind.Struct } => true,
            ITypeParameterSymbol { HasValueTypeConstraint: true } => true,
            _ => false
        };
    }
}