using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the type-name mappings and the helper lookups that
    ///     the numeric conversion analyzer and the code fix share.
    /// </summary>
    /// <remarks>
    ///     The library converts numbers through the static <c>From*Unchecked</c> methods of its wrapper
    ///     types and performs wrapping arithmetic through the <c>Wrapping*</c> extension methods of
    ///     the matching <c>*Extensions</c> type. The helper matrix is partial, so both the analyzer and
    ///     the code fix ask the compilation whether the helper they intend to emit exists instead of
    ///     assuming that it does, which keeps the code fix from producing calls that do not compile.
    /// </remarks>
    internal static class NumericFacts
    {
        /// <summary>
        ///     The wrapper types of the analyzed library, paired with the types they represent.
        /// </summary>
        /// <remarks>
        ///     The table is the single source of both the type-to-name mapping and the name-to-type
        ///     classification, so the analyzer and the code fix cannot drift apart when the library
        ///     gains a wrapper type. <see cref="decimal" /> is deliberately absent, because the library
        ///     has no <c>Decimal</c> wrapper type and no conversion helper can be emitted for it.
        /// </remarks>
        private static readonly (Microsoft.CodeAnalysis.SpecialType SpecialType, string TypeName)[] WrapperTypes =
        [
            (SpecialType.System_SByte, "I8"),
            (SpecialType.System_Byte, "U8"),
            (SpecialType.System_Int16, "I16"),
            (SpecialType.System_UInt16, "U16"),
            (SpecialType.System_Char, "Char"),
            (SpecialType.System_Int32, "I32"),
            (SpecialType.System_UInt32, "U32"),
            (SpecialType.System_Int64, "I64"),
            (SpecialType.System_UInt64, "U64"),
            (SpecialType.System_IntPtr, "Isize"),
            (SpecialType.System_UIntPtr, "Usize"),
            (SpecialType.System_Single, "F32"),
            (SpecialType.System_Double, "F64")
        ];

        /// <summary>
        ///     Gets the name of the wrapper type that represents the numeric type.
        /// </summary>
        /// <param name="type">The type to map.</param>
        /// <returns>The wrapper type name, or <see langword="null" /> for a type without a wrapper type.</returns>
        public static string? GetHelperTypeName(ITypeSymbol? type)
        {
            foreach (var wrapper in WrapperTypes)
            {
                if (wrapper.SpecialType == type?.SpecialType)
                    return wrapper.TypeName;
            }

            return null;
        }

        /// <summary>
        ///     Determines whether the name is the name of a numeric wrapper type of the library.
        /// </summary>
        /// <param name="typeName">The simple type name to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the name belongs to a wrapper type; otherwise, <see langword="false" />.
        /// </returns>
        public static bool IsWrapperTypeName(string? typeName) => WrapperTypes.Any(wrapper => wrapper.TypeName == typeName);

        /// <summary>
        ///     Determines whether the type is an integral type whose
        ///     arithmetic and conversions the checked context changes.
        /// </summary>
        /// <param name="type">The type to classify.</param>
        /// <returns><see langword="true" /> when the type is integral; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     <see cref="char" /> is included, because a conversion from or to a character
        ///     can truncate even though the character itself is not used in arithmetic.
        /// </remarks>
        public static bool IsIntegral(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Char or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_IntPtr or SpecialType.System_UIntPtr;

        /// <summary>
        ///     Determines whether the type is a small integral type
        ///     whose arithmetic is performed on <see cref="int" />.
        /// </summary>
        /// <param name="type">The type to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the type is promoted to
        ///     <see cref="int" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The small integral types are promoted to <see cref="int" /> when they
        ///     take part in arithmetic, so the result type of an operation on them is
        ///     the wrapper type of <see cref="int" /> rather than their own wrapper type.
        /// </remarks>
        public static bool IsSmallIntegral(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Char;

        /// <summary>
        ///     Determines whether the conversion of the source type to the target type
        ///     can lose information and therefore throws when the conversion is checked.
        /// </summary>
        /// <param name="source">The type of the value that is converted.</param>
        /// <param name="target">The type that receives the value.</param>
        /// <returns>
        ///     <see langword="true" /> when the conversion can overflow; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A narrowing conversion can always lose information. A signed-to-unsigned conversion
        ///     can as well, and so can an unsigned-to-signed conversion whose target is not wider than
        ///     the source. A floating point value can lose information when it converts to an integral
        ///     type, while the other direction only loses precision and never throws. An unknown native
        ///     size is treated as potentially overflowing, because the width depends on the platform.
        /// </remarks>
        public static bool CanOverflow(ITypeSymbol? source, ITypeSymbol? target)
        {
            if (IsFloatingPoint(source))
                return IsIntegral(target);

            if (!IsIntegral(source) || !IsIntegral(target) || source!.SpecialType == target!.SpecialType)
                return false;

            var sourceBits = GetBitWidth(source.SpecialType);
            var targetBits = GetBitWidth(target.SpecialType);
            if (sourceBits == 0 || targetBits == 0)
                return true;

            var sourceIsUnsigned = IsUnsigned(source.SpecialType);
            var targetIsUnsigned = IsUnsigned(target.SpecialType);
            return targetBits < sourceBits || (!sourceIsUnsigned && targetIsUnsigned) || (sourceIsUnsigned && !targetIsUnsigned && targetBits <= sourceBits);
        }

        /// <summary>
        ///     Determines whether the type is a floating point type.
        /// </summary>
        /// <param name="type">The type to classify.</param>
        /// <returns>
        ///     <see langword="true" /> when the type is floating point; otherwise, <see langword="false" />.
        /// </returns>
        private static bool IsFloatingPoint(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_Single or SpecialType.System_Double;

        /// <summary>
        ///     Gets the bit width of a fixed-size integral type.
        /// </summary>
        /// <param name="type">The integral type to inspect.</param>
        /// <returns>The bit width, or <c>0</c> for the native-sized types.</returns>
        /// <remarks>
        ///     The native-sized types return <c>0</c>, because their width depends on the platform.
        /// </remarks>
        private static int GetBitWidth(SpecialType type) => type switch
        {
            SpecialType.System_SByte or SpecialType.System_Byte => 8,
            SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Char => 16,
            SpecialType.System_Int32 or SpecialType.System_UInt32 => 32,
            SpecialType.System_Int64 or SpecialType.System_UInt64 => 64,
            _ => 0
        };

        /// <summary>
        ///     Determines whether the type is an unsigned integral type.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns><see langword="true" /> when the type is unsigned; otherwise, <see langword="false" />.</returns>
        private static bool IsUnsigned(SpecialType type) => type is SpecialType.System_Byte or SpecialType.System_UInt16 or SpecialType.System_Char or SpecialType.System_UInt32 or SpecialType.System_UInt64 or SpecialType.System_UIntPtr;

        /// <summary>
        ///     Determines whether the target wrapper type declares the <c>From{source}Unchecked</c> conversion helper.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="targetTypeName">The name of the wrapper type that receives the value.</param>
        /// <param name="sourceTypeName">The name of the wrapper type that the value converts from.</param>
        /// <returns><see langword="true" /> when the helper exists; otherwise, <see langword="false" />.</returns>
        public static bool HasUncheckedConversionHelper(Compilation compilation, string targetTypeName, string sourceTypeName) => GetMembers(compilation, targetTypeName, "From" + sourceTypeName + "Unchecked").Any(member => member is IMethodSymbol { Parameters.Length: 1 });

        /// <summary>
        ///     Determines whether the wrapping type declares one of the binary wrapping helpers.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="wrappingTypeName">The name of the wrapper type the helper operates on.</param>
        /// <param name="methodName">The name of the helper.</param>
        /// <returns><see langword="true" /> when the helper exists; otherwise, <see langword="false" />.</returns>
        public static bool HasBinaryWrappingHelper(Compilation compilation, string? wrappingTypeName, string methodName) => HasWrappingHelper(compilation, wrappingTypeName, methodName, 2);

        /// <summary>
        ///     Determines whether the wrapping type declares <c>WrappingNeg</c>.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="wrappingTypeName">The name of the wrapper type the helper operates on.</param>
        /// <returns><see langword="true" /> when the helper exists; otherwise, <see langword="false" />.</returns>
        public static bool HasWrappingNegHelper(Compilation compilation, string? wrappingTypeName) => HasWrappingHelper(compilation, wrappingTypeName, "WrappingNeg", 1);

        /// <summary>
        ///     Determines whether the wrapping extension type declares the helper with the expected parameter count.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="wrappingTypeName">The name of the wrapper type the helper operates on.</param>
        /// <param name="methodName">The name of the helper.</param>
        /// <param name="parameterCount">
        ///     The number of parameters the helper must declare, including the receiver.
        /// </param>
        /// <returns><see langword="true" /> when the helper exists; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     The wrapping helpers are extension methods, so the helper is looked up in the type whose name is
        ///     the wrapper type name with the <c>Extensions</c> suffix rather than in the wrapper type itself. The
        ///     parameter count includes the receiver parameter, because the helper is emitted as an extension call.
        /// </remarks>
        private static bool HasWrappingHelper(Compilation compilation, string? wrappingTypeName, string methodName, int parameterCount) => wrappingTypeName != null && GetMembers(compilation, wrappingTypeName + "Extensions", methodName).Any(member => member is IMethodSymbol method && method.IsExtensionMethod && method.Parameters.Length == parameterCount);

        /// <summary>
        ///     Gets the members with the given name of the library type with the given simple name.
        /// </summary>
        /// <param name="compilation">The compilation that declares or references the analyzed library.</param>
        /// <param name="typeName">The simple name of the type in the <c>NativeCollections</c> namespace.</param>
        /// <param name="memberName">The name of the members to get.</param>
        /// <returns>The matching members, or an empty list when the type does not exist.</returns>
        private static ImmutableArray<ISymbol> GetMembers(Compilation compilation, string typeName, string memberName)
        {
            var type = compilation.GetTypeByMetadataName(SR.NATIVE_COLLECTIONS_NAMESPACE + "." + typeName);
            return type?.GetMembers(memberName) ?? ImmutableArray<ISymbol>.Empty;
        }
    }
}