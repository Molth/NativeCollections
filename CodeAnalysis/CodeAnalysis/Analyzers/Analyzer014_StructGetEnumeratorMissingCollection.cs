using System.Collections.Generic;
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
    ///     Requires a struct that exposes a strongly typed <c>GetEnumerator()</c>
    ///     to implement <see cref="IReadOnlyCollection{T}" />.
    /// </summary>
    /// <remarks>
    ///     A struct that can be enumerated but is not a collection cannot answer <c>Count</c>, so
    ///     callers cannot preallocate buffers and cannot state the ownership contract they rely on.
    ///     The interface also makes the element type discoverable without inspecting the enumerator.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer014_StructGetEnumeratorMissingCollection : DiagnosticAnalyzer
    {
        /// <summary>
        ///     The identifier of the rule reported by this analyzer.
        /// </summary>
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "014";

        /// <summary>
        ///     The title of the rule.
        /// </summary>
        private static readonly LocalizableString Title = "Struct with GetEnumerator should implement IReadOnlyCollection<T>";

        /// <summary>
        ///     The message of the rule; the placeholders are the struct name and the inferred element type name.
        /// </summary>
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a GetEnumerator method but does not implement IReadOnlyCollection<{1}>";

        /// <summary>
        ///     Flags a struct with a typed <c>GetEnumerator()</c> but without <see cref="IReadOnlyCollection{T}" />.
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
        ///     Reports a struct with a typed <c>GetEnumerator()</c> that implements neither
        ///     <c>IEnumerable</c> nor <see cref="IReadOnlyCollection{T}" /> for the inferred element type.
        /// </summary>
        /// <param name="context">The syntax context of the struct declaration.</param>
        /// <remarks>
        ///     An enumerator that returns an interface type is skipped, because such a method is a forwarding
        ///     member rather than evidence of a newly introduced collection. The expected element type is taken
        ///     from <c>IIterator&lt;T&gt;</c> when available and from the <c>Current</c> property otherwise.
        ///     A <c>ref struct</c> is skipped when the language version does not support ref struct
        ///     interfaces, because such a type cannot implement the required interface there.
        /// </remarks>
        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDeclaration = (StructDeclarationSyntax)context.Node;
            if (context.SemanticModel.GetDeclaredSymbol(structDeclaration) is not { } structSymbol)
                return;

            if (structSymbol.IsRefLikeType && !SupportsRefStructInterfaces(structDeclaration))
                return;

            var getEnumeratorMethod = structSymbol.GetMembers("GetEnumerator").OfType<IMethodSymbol>().FirstOrDefault(method => !method.IsStatic && method.Parameters.IsEmpty && !method.ReturnsVoid && method.ReturnType.TypeKind != TypeKind.Interface && method.TypeParameters.IsEmpty);
            if (getEnumeratorMethod == null)
                return;

            var enumerableType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            var nonGenericEnumerableType = context.Compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            if (enumerableType != null && structSymbol.AllInterfaces.Any(item => SymbolEqualityComparer.Default.Equals(item.OriginalDefinition, enumerableType)))
                return;

            if (nonGenericEnumerableType != null && structSymbol.AllInterfaces.Any(item => SymbolEqualityComparer.Default.Equals(item, nonGenericEnumerableType)))
                return;

            var elementType = InferElementType(getEnumeratorMethod.ReturnType, context.Compilation);
            var readOnlyCollectionType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
            if (elementType == null || readOnlyCollectionType == null)
                return;

            var expectedInterface = readOnlyCollectionType.Construct(elementType);
            if (structSymbol.AllInterfaces.Contains(expectedInterface, SymbolEqualityComparer.Default))
                return;

            var elementTypeName = elementType.ToMinimalDisplayString(context.SemanticModel, structDeclaration.SpanStart);
            context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), structDeclaration.Identifier.ValueText, elementTypeName));
        }

        /// <summary>
        ///     Determines whether the language version of the analyzed declaration supports ref struct interfaces.
        /// </summary>
        /// <param name="structDeclaration">The declaration of the analyzed struct.</param>
        /// <returns>
        ///     <see langword="true" /> when a <c>ref struct</c> may implement an interface; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The feature was introduced in C# 13. Earlier language versions reject a <c>ref struct</c> with a base
        ///     list, so the required <see cref="IReadOnlyCollection{T}" /> cannot be implemented there and reporting
        ///     the rule would flag code that cannot be fixed.
        /// </remarks>
        private static bool SupportsRefStructInterfaces(StructDeclarationSyntax structDeclaration) => structDeclaration.SyntaxTree.Options is CSharpParseOptions options && options.LanguageVersion >= LanguageVersion.CSharp13;

        /// <summary>
        ///     Infers the element type produced by the given enumerator type.
        /// </summary>
        /// <param name="enumeratorType">The return type of the <c>GetEnumerator()</c> method.</param>
        /// <param name="compilation">
        ///     The compilation used to resolve <c>NativeCollections.IIterator&lt;T&gt;</c>.
        /// </param>
        /// <returns>The inferred element type, or <see langword="null" /> when it cannot be determined.</returns>
        /// <remarks>
        ///     <c>IIterator&lt;T&gt;</c> is preferred, because the library's enumerators expose their
        ///     element type there even when the <c>Current</c> property is inherited from a base
        ///     enumerator. When the interface is absent the type of the <c>Current</c> property is used.
        /// </remarks>
        private static ITypeSymbol? InferElementType(ITypeSymbol enumeratorType, Compilation compilation)
        {
            var iteratorType = compilation.GetTypeByMetadataName(SR.NATIVE_COLLECTIONS_NAMESPACE + ".IIterator`1");
            if (iteratorType != null)
            {
                foreach (var item in enumeratorType.AllInterfaces)
                {
                    if (item.IsGenericType && item.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(item.OriginalDefinition, iteratorType))
                        return item.TypeArguments[0];
                }
            }

            var currentProperty = enumeratorType.GetMembers("Current").OfType<IPropertySymbol>().FirstOrDefault(property => !property.IsStatic && property.GetMethod != null);
            return currentProperty?.Type;
        }
    }
}