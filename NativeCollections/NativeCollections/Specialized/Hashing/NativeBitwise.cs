using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides bitwise comparison utilities for unmanaged types.
    /// </summary>
    public static class NativeBitwise
    {
        /// <summary>
        ///     Determines whether the raw bytes of two values are equal.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if the byte representations are equal; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Equals(ref left, ref right);

        /// <summary>
        ///     Compares the raw bytes of two values in lexicographic order.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     A negative value if <paramref name="left" /> is less than <paramref name="right" />,
        ///     zero if they are equal, or a positive value if <paramref name="left" /> is greater than
        ///     <paramref name="right" />, based on byte-wise comparison.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Compare(ref left, ref right);
    }
}