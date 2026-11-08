using System;
using Microsoft.CodeAnalysis;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the predicates that recognize the arithmetic helper members of the analyzed library.
    /// </summary>
    /// <remarks>
    ///     The helpers are the only members that are allowed to contain <see langword="unchecked" />, so every
    ///     analyzer and code fix has to agree on which members count as helpers. Recognition is done on the resolved
    ///     symbol rather than on syntax, so a member with a similar name in another assembly is still analyzed.
    /// </remarks>
    internal static class HelperFacts
    {
        /// <summary>
        ///     Determines whether the method belongs to the group of the arithmetic
        ///     helpers and conversions of <c>NativeCollections</c> that may contain
        ///     <see langword="checked" /> or <see langword="unchecked" /> arithmetic themselves.
        /// </summary>
        /// <param name="method">The method to classify.</param>
        /// <returns><see langword="true" /> when the method is a helper; otherwise, <see langword="false" />.</returns>
        /// <remarks>
        ///     A wrapping helper is a <c>Wrapping*</c> method of an <c>*Extensions</c> type, an
        ///     unchecked conversion helper is a <c>From*Unchecked</c> method of a numeric wrapper type,
        ///     and a wrapper conversion is any <c>From*</c> method of a numeric wrapper type — the
        ///     checked conversions are included, because they state the throwing behavior explicitly
        ///     and therefore never need a wrapping helper. The signature is deliberately not inspected
        ///     any further, because the helper matrix of the library is partial and differs per type.
        /// </remarks>
        internal static bool IsWrappingHelper(IMethodSymbol? method)
        {
            if (method is not { IsStatic: true } || method.ContainingType is not { } containingType || containingType.ContainingNamespace.ToDisplayString() != SR.NATIVE_COLLECTIONS_NAMESPACE)
                return false;

            var isWrappingHelper = method.Name.StartsWith("Wrapping", StringComparison.Ordinal) && containingType.Name.EndsWith("Extensions", StringComparison.Ordinal);
            var isWrapperConversion = method.Name.StartsWith("From", StringComparison.Ordinal) && NumericFacts.IsWrapperTypeName(containingType.Name);
            return isWrappingHelper || isWrapperConversion;
        }
    }
}