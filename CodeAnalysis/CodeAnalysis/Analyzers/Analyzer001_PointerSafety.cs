using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Enforces that pointer dereferencing, pointer arithmetic and pointer indexing are
    ///     expressed through the <see cref="Unsafe" /> helpers instead of the C# operators.
    /// </summary>
    /// <remarks>
    ///     The rules exist so that every raw memory access is visible and searchable
    ///     in the source. The same access written with operators is easy to overlook
    ///     during review, and it does not state the element type that is being addressed.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer001_PointerSafety : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The prefix shared by the four rules of this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_PTR + "001";

        /// <summary>
        ///     The title of <c>PTR001_1</c>, which flags a plain pointer dereference.
        /// </summary>
        private static readonly LocalizableString Title001 = "Pointer dereference detected";

        /// <summary>
        ///     The message of <c>PTR001_1</c>; the placeholder is the dereferenced pointer expression.
        /// </summary>
        private static readonly LocalizableString MessageFormat001 = "Pointer dereference: '*{0}'";

        /// <summary>
        ///     Flags <c>*pointer</c> when the operand is not a pointer arithmetic sub-expression.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule001 = new(DIAGNOSTIC_ID + "_1", Title001, MessageFormat001, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>PTR001_2</c>, which flags a pointer arithmetic assignment.
        /// </summary>
        private static readonly LocalizableString Title002 = "Pointer arithmetic assignment is prohibited";

        /// <summary>
        ///     The message of <c>PTR001_2</c>; the placeholder is the mutated pointer expression.
        /// </summary>
        private static readonly LocalizableString MessageFormat002 = "Use Unsafe.Add<T>() instead of arithmetic assignment on pointer '{0}'";

        /// <summary>
        ///     Flags <c>pointer += offset</c>, <c>pointer -= offset</c> and <c>pointer = pointer + offset</c>.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule002 = new(DIAGNOSTIC_ID + "_2", Title002, MessageFormat002, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>PTR001_3</c>, which flags pointer indexing and pointer-arithmetic dereferences.
        /// </summary>
        private static readonly LocalizableString Title003 = "Convert pointer arithmetic to unsafe methods";

        /// <summary>
        ///     The message of <c>PTR001_3</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat003 = "Pointer arithmetic should be converted to Unsafe methods";

        /// <summary>
        ///     Flags pointer element access, pointer indexer parameters and <c>*(pointer + offset)</c> dereferences.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule003 = new(DIAGNOSTIC_ID + "_3", Title003, MessageFormat003, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     The title of <c>PTR001_4</c>, which flags any other operator applied to a pointer operand.
        /// </summary>
        private static readonly LocalizableString Title004 = "Pointer arithmetic or indexing is prohibited";

        /// <summary>
        ///     The message of <c>PTR001_4</c>.
        /// </summary>
        private static readonly LocalizableString MessageFormat004 = "Pointer type cannot be used with operators or indexing";

        /// <summary>
        ///     Flags binary operators with at least one pointer operand, except the forms handled by the other rules.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule004 = new(DIAGNOSTIC_ID + "_4", Title004, MessageFormat004, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the four rules reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule001, Rule002, Rule003, Rule004];

        /// <summary>
        ///     Registers the syntax callbacks.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        /// <remarks>
        ///     Generated code is analyzed as well, because the pointer rules apply to
        ///     every compilation unit of the solution regardless of how it was produced.
        /// </remarks>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeDereference, SyntaxKind.PointerIndirectionExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.AddExpression, SyntaxKind.SubtractExpression, SyntaxKind.MultiplyExpression);
            context.RegisterSyntaxNodeAction(AnalyzeElementAccess, SyntaxKind.ElementAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.AddAssignmentExpression, SyntaxKind.SubtractAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
        }

        /// <summary>
        ///     Reports a pointer dereference.
        /// </summary>
        /// <param name="context">The syntax context of the dereference expression.</param>
        /// <remarks>
        ///     When the operand is a parenthesized binary expression the dereference is a pointer arithmetic
        ///     access, so the dedicated arithmetic rule is reported instead of the plain dereference rule.
        /// </remarks>
        private static void AnalyzeDereference(SyntaxNodeAnalysisContext context)
        {
            var dereference = (PrefixUnaryExpressionSyntax)context.Node;
            if (!dereference.OperatorToken.IsKind(SyntaxKind.AsteriskToken) || context.SemanticModel.GetTypeInfo(dereference.Operand).Type is not IPointerTypeSymbol)
                return;

            if (dereference.Operand is ParenthesizedExpressionSyntax { Expression: BinaryExpressionSyntax })
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule003, dereference.GetLocation()));
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule001, dereference.GetLocation(), dereference.Operand.ToString()));
        }

        /// <summary>
        ///     Reports a binary operator that has a pointer operand.
        /// </summary>
        /// <param name="context">The syntax context of the binary expression.</param>
        /// <remarks>
        ///     Dereference expressions and the right side of a simple assignment are skipped, because they are already
        ///     reported by <see cref="AnalyzeDereference" /> and <see cref="AnalyzeAssignment" /> respectively.
        /// </remarks>
        private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binaryExpression = (BinaryExpressionSyntax)context.Node;
            if (binaryExpression.Parent is ParenthesizedExpressionSyntax { Parent: PrefixUnaryExpressionSyntax dereference } && dereference.IsKind(SyntaxKind.PointerIndirectionExpression))
                return;

            if (binaryExpression.Parent is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left).Type;
            var rightType = context.SemanticModel.GetTypeInfo(binaryExpression.Right).Type;
            if (leftType is IPointerTypeSymbol || rightType is IPointerTypeSymbol)
                context.ReportDiagnostic(Diagnostic.Create(Rule004, binaryExpression.GetLocation()));
        }

        /// <summary>
        ///     Reports an element access whose receiver is a pointer.
        /// </summary>
        /// <param name="context">The syntax context of the element access expression.</param>
        private static void AnalyzeElementAccess(SyntaxNodeAnalysisContext context)
        {
            var elementAccess = (ElementAccessExpressionSyntax)context.Node;
            if (context.SemanticModel.GetTypeInfo(elementAccess.Expression).Type is IPointerTypeSymbol)
                context.ReportDiagnostic(Diagnostic.Create(Rule003, elementAccess.GetLocation()));
        }

        /// <summary>
        ///     Reports an indexer that declares a pointer parameter.
        /// </summary>
        /// <param name="context">The syntax context of the indexer declaration.</param>
        /// <remarks>
        ///     Pointer element accesses inside the accessor bodies are already reported by
        ///     <see cref="AnalyzeElementAccess" />, so this callback only inspects the parameter list.
        /// </remarks>
        private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
        {
            var indexer = (IndexerDeclarationSyntax)context.Node;
            if (indexer.ParameterList.Parameters.Any(parameter => parameter.Type is PointerTypeSyntax))
                context.ReportDiagnostic(Diagnostic.Create(Rule003, indexer.GetLocation()));
        }

        /// <summary>
        ///     Reports pointer arithmetic that mutates a pointer variable.
        /// </summary>
        /// <param name="context">The syntax context of the assignment expression.</param>
        /// <remarks>
        ///     Only identifiers are matched, because the rule targets the <c>pointer += offset</c>
        ///     idiom; member accesses and dereferences fall under the binary expression rule.
        /// </remarks>
        private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (context.SemanticModel.GetTypeInfo(assignment.Left).Type is not IPointerTypeSymbol)
                return;

            if (assignment.IsKind(SyntaxKind.AddAssignmentExpression) || assignment.IsKind(SyntaxKind.SubtractAssignmentExpression))
            {
                ReportPointerArithmetic(context, assignment);
                return;
            }

            if (assignment is { Left: IdentifierNameSyntax leftIdentifier, Right: BinaryExpressionSyntax binary } && (binary.IsKind(SyntaxKind.AddExpression) || binary.IsKind(SyntaxKind.SubtractExpression)) && binary.Left is IdentifierNameSyntax binaryLeft && binaryLeft.Identifier.ValueText == leftIdentifier.Identifier.ValueText)
                ReportPointerArithmetic(context, assignment);
        }

        /// <summary>
        ///     Reports <c>PTR001_2</c> on the given assignment.
        /// </summary>
        /// <param name="context">The syntax context of the assignment expression.</param>
        /// <param name="assignment">The assignment that mutates a pointer.</param>
        private static void ReportPointerArithmetic(SyntaxNodeAnalysisContext context, AssignmentExpressionSyntax assignment) => context.ReportDiagnostic(Diagnostic.Create(Rule002, assignment.GetLocation(), assignment.Left.ToString()));
    }
}