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
    [Analyzer(014)]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class Analyzer014_StructGetEnumeratorMissingCollection : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = SR.DIAGNOSTIC_ID_DESIGN + "014";

        private static readonly LocalizableString Title = "Struct with GetEnumerator should implement IReadOnlyCollection<T>";
        private static readonly LocalizableString MessageFormat = "Struct '{0}' has a GetEnumerator method but does not implement IReadOnlyCollection<{1}>";
        private static readonly DiagnosticDescriptor Rule = new(DIAGNOSTIC_ID, Title, MessageFormat, SR.CATEGORY_SAFETY, DiagnosticSeverity.Error, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
        }

        private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
        {
            var structDecl = (StructDeclarationSyntax)context.Node;
            var structSymbol = context.SemanticModel.GetDeclaredSymbol(structDecl);
            if (structSymbol == null)
                return;

            var getEnumeratorMethod = structSymbol.GetMembers("GetEnumerator")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    !m.IsStatic && m.Parameters.IsEmpty &&
                    !m.ReturnsVoid &&
                    m.ReturnType.TypeKind != TypeKind.Interface &&
                    m.TypeParameters.IsEmpty);

            if (getEnumeratorMethod == null)
                return;

            var enumerableGeneric = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");
            var enumerableNonGeneric = context.Compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
            if (enumerableGeneric != null && structSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, enumerableGeneric)))
                return;

            if (enumerableNonGeneric != null && structSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, enumerableNonGeneric)))
                return;

            var elementType = InferElementType(getEnumeratorMethod.ReturnType, context.Compilation);
            if (elementType == null)
                return;

            var readOnlyCollectionType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1");
            if (readOnlyCollectionType == null)
                return;

            var expectedInterface = readOnlyCollectionType.Construct(elementType);
            if (structSymbol.AllInterfaces.Contains(expectedInterface, SymbolEqualityComparer.Default))
                return;

            var elementTypeName = elementType.ToMinimalDisplayString(context.SemanticModel, structDecl.SpanStart);
            var diagnostic = Diagnostic.Create(Rule, structDecl.Identifier.GetLocation(), structDecl.Identifier.ValueText, elementTypeName);
            context.ReportDiagnostic(diagnostic);
        }

        private static ITypeSymbol? InferElementType(ITypeSymbol enumeratorType, Compilation compilation)
        {
            var iteratorType = compilation.GetTypeByMetadataName("NativeCollections.IIterator`1");
            if (iteratorType != null)
            {
                foreach (var item in enumeratorType.AllInterfaces)
                {
                    if (item.IsGenericType && SymbolEqualityComparer.Default.Equals(item.OriginalDefinition, iteratorType) && item.TypeArguments.Length == 1)
                        return item.TypeArguments[0];
                }
            }

            var currentProperty = enumeratorType.GetMembers("Current")
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => !p.IsStatic && p.GetMethod != null);

            return currentProperty?.Type;
        }
    }
}