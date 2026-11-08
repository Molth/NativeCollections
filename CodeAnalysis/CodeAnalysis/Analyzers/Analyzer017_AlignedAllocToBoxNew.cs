using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Requires the manual "allocate one element, copy the value into
    ///     it, store the pointer" sequence to be replaced by <c>Box.New</c>.
    /// </summary>
    /// <remarks>
    ///     The manual sequence repeats the allocation and the copy at every collection, and it
    ///     is easy to allocate a pointer without ever copying the value into it. <c>Box.New</c>
    ///     performs both steps in one place and keeps the ownership of the boxed value visible.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer017_AlignedAllocToBoxNew : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "017";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Use Box.New instead of manual alloc and copy";

        /// <summary>
        ///     The message of the rule.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Replace AlignedAlloc + Unsafe.AsRef assignment with Box.New";

        /// <summary>
        ///     Flags the four-statement manual boxing sequence.
        /// </summary>
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        /// <summary>
        ///     Gets the rule reported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        /// <summary>
        ///     Registers the block callback.
        /// </summary>
        /// <param name="context">The analysis context of the compilation.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        /// <summary>
        ///     Reports a block that allocates a single element, copies a value into
        ///     it with <c>Unsafe.AsRef</c> and then stores the resulting pointer.
        /// </summary>
        /// <param name="context">The syntax context of the block.</param>
        /// <remarks>
        ///     The sequence is matched statement by statement: a value declaration, a handle declaration initialised
        ///     by <c>NativeMemoryAllocator.AlignedAlloc&lt;T&gt;</c>, an
        ///     <c>
        ///         Unsafe.AsRef&lt;T&gt;(handle) =
        ///         value
        ///     </c>
        ///     assignment and a field or property assignment of the handle. The handle must not be
        ///     referenced anywhere else in the block, because otherwise the rewrite would drop its remaining uses.
        /// </remarks>
        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;
            var statements = block.Statements;
            if (statements.Count < 4)
                return;

            var semanticModel = context.SemanticModel;
            for (var index = 0; index <= statements.Count - 4; index++)
            {
                if (!TryGetLocalDeclaration(statements[index], out var valueName, out var valueInitializer) || !TryGetLocalDeclaration(statements[index + 1], out var handleName, out var handleInitializer))
                    continue;

                if (statements[index + 2] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax asRefAssignment } || statements[index + 3] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax handleAssignment })
                    continue;

                if (valueInitializer is not ObjectCreationExpressionSyntax creation || semanticModel.GetTypeInfo(creation).Type is not { IsValueType: true } valueType)
                    continue;

                if (!IsSingleElementAllocation(handleInitializer, valueType, semanticModel))
                    continue;

                if (!IsUnsafeAsRefAssignment(asRefAssignment, valueType, handleName, valueName, semanticModel))
                    continue;

                if (handleAssignment.Left is not IdentifierNameSyntax fieldIdentifier || !IsSameIdentifier(handleAssignment.Right, handleName))
                    continue;

                var fieldSymbol = semanticModel.GetSymbolInfo(fieldIdentifier).Symbol;
                var fieldType = fieldSymbol switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => null
                };

                if (fieldType == null || !SymbolEqualityComparer.Default.Equals(fieldType, semanticModel.GetTypeInfo(handleInitializer).Type))
                    continue;

                if (HasOtherReferences(block, handleName, statements[index + 1], statements[index + 2], statements[index + 3]))
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(Rule, Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(statements[index].SpanStart, statements[index + 3].Span.End))));
                index += 3;
            }
        }

        /// <summary>
        ///     Reads a single-variable local declaration statement.
        /// </summary>
        /// <param name="statement">The statement to inspect.</param>
        /// <param name="name">
        ///     The declared variable name, or <see langword="null" /> when the statement does not match.
        /// </param>
        /// <param name="initializer">
        ///     The initializer expression, or <see langword="null" /> when the statement does not match.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> when the statement is a single-variable local
        ///     declaration with an initializer; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Multi-variable declarations are skipped, because the statement index would then cover
        ///     more than one value and the four-statement window would no longer describe the sequence.
        /// </remarks>
        private static bool TryGetLocalDeclaration(StatementSyntax statement, out string name, out ExpressionSyntax initializer)
        {
            name = string.Empty;
            initializer = null!;
            if (statement is not LocalDeclarationStatementSyntax { Declaration.Variables.Count: 1 } declaration)
                return false;

            var variable = declaration.Declaration.Variables[0];
            if (variable.Initializer == null)
                return false;

            name = variable.Identifier.ValueText;
            initializer = variable.Initializer.Value;
            return true;
        }

        /// <summary>
        ///     Determines whether the initializer is
        ///     <c>NativeMemoryAllocator.AlignedAlloc&lt;T&gt;</c> for one element of the given type.
        /// </summary>
        /// <param name="initializer">The initializer of the handle declaration.</param>
        /// <param name="valueType">The type of the value that is copied into the allocation.</param>
        /// <param name="semanticModel">The semantic model used to resolve the allocation and the type argument.</param>
        /// <returns>
        ///     <see langword="true" /> when the initializer allocates exactly one element
        ///     of <paramref name="valueType" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The element count must be the constant <c>1</c>, either written directly or wrapped in
        ///     the corresponding <c>From*Unchecked</c> conversion helper that <c>Box.New</c> itself uses.
        /// </remarks>
        private static bool IsSingleElementAllocation(ExpressionSyntax initializer, ITypeSymbol valueType, SemanticModel semanticModel)
        {
            if (initializer is not InvocationExpressionSyntax { ArgumentList.Arguments.Count: 1 } allocation || semanticModel.GetSymbolInfo(allocation).Symbol is not IMethodSymbol { Name: "AlignedAlloc", IsGenericMethod: true, TypeArguments.Length: 1 } allocator)
                return false;

            if (allocator.ContainingType.Name != "NativeMemoryAllocator" || !SymbolEqualityComparer.Default.Equals(allocator.TypeArguments[0], valueType))
                return false;

            return IsConstantOne(allocation.ArgumentList.Arguments[0].Expression, semanticModel);
        }

        /// <summary>
        ///     Determines whether the expression denotes the numeric value <c>1</c>.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <param name="semanticModel">The semantic model used to evaluate constants.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression denotes <c>1</c>; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A conversion helper such as <c>Usize.FromI32Unchecked(1)</c> is not a constant expression,
        ///     so single-argument invocations and casts are unwrapped before the constant is evaluated.
        /// </remarks>
        private static bool IsConstantOne(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var constant = semanticModel.GetConstantValue(expression);
            if (constant.HasValue)
            {
                return constant.Value switch
                {
                    int value => value == 1,
                    uint value => value == 1,
                    long value => value == 1,
                    ulong value => value == 1,
                    short value => value == 1,
                    ushort value => value == 1,
                    byte value => value == 1,
                    sbyte value => value == 1,
                    _ => false
                };
            }

            return expression switch
            {
                InvocationExpressionSyntax { ArgumentList.Arguments.Count: 1 } invocation => IsConstantOne(invocation.ArgumentList.Arguments[0].Expression, semanticModel),
                CastExpressionSyntax cast => IsConstantOne(cast.Expression, semanticModel),
                ParenthesizedExpressionSyntax parenthesized => IsConstantOne(parenthesized.Expression, semanticModel),
                _ => false
            };
        }

        /// <summary>
        ///     Determines whether the assignment is <c>Unsafe.AsRef&lt;T&gt;(handle) = value</c>.
        /// </summary>
        /// <param name="assignment">The assignment to inspect.</param>
        /// <param name="valueType">The type that <c>Unsafe.AsRef</c> must request.</param>
        /// <param name="handleName">The name of the pointer variable that receives the allocation.</param>
        /// <param name="valueName">The name of the value variable that is copied into the allocation.</param>
        /// <param name="semanticModel">
        ///     The semantic model used to resolve the invoked method and the assigned identifiers.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> when the assignment copies <paramref name="valueName" />
        ///     into the allocation; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsUnsafeAsRefAssignment(AssignmentExpressionSyntax assignment, ITypeSymbol valueType, string handleName, string valueName, SemanticModel semanticModel)
        {
            if (assignment.Left is not InvocationExpressionSyntax { ArgumentList.Arguments.Count: 1 } invocation || semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol { Name: "AsRef", IsGenericMethod: true, TypeArguments.Length: 1 } asRefMethod)
                return false;

            if (asRefMethod.ContainingType.Name != "Unsafe" || !SymbolEqualityComparer.Default.Equals(asRefMethod.TypeArguments[0], valueType))
                return false;

            return IsSameIdentifier(invocation.ArgumentList.Arguments[0].Expression, handleName) && IsSameIdentifier(assignment.Right, valueName);
        }

        /// <summary>
        ///     Determines whether the expression is the identifier with the given name.
        /// </summary>
        /// <param name="expression">The expression to inspect.</param>
        /// <param name="name">The expected identifier text.</param>
        /// <returns>
        ///     <see langword="true" /> when the expression is an identifier
        ///     with the given name; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsSameIdentifier(ExpressionSyntax expression, string name) => expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == name;

        /// <summary>
        ///     Determines whether the handle variable is referenced by statements other than the matched sequence.
        /// </summary>
        /// <param name="block">The block that contains the sequence.</param>
        /// <param name="handleName">The name of the handle variable.</param>
        /// <param name="declaration">The statement that declares the handle.</param>
        /// <param name="asRefUse">The statement that copies the value into the allocation.</param>
        /// <param name="assignmentUse">The statement that stores the handle.</param>
        /// <returns>
        ///     <see langword="true" /> when the handle is referenced elsewhere; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Any additional reference means the pointer outlives the matched statements,
        ///     so replacing them with <c>Box.New</c> would break the remaining code.
        /// </remarks>
        private static bool HasOtherReferences(BlockSyntax block, string handleName, StatementSyntax declaration, StatementSyntax asRefUse, StatementSyntax assignmentUse)
        {
            foreach (var identifier in block.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (identifier.Identifier.ValueText != handleName)
                    continue;

                var parentStatement = identifier.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
                if (parentStatement == declaration || parentStatement == asRefUse || parentStatement == assignmentUse)
                    continue;

                return true;
            }

            return false;
        }
    }
}