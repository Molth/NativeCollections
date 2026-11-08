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
        ///     Determines whether two values are equal.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Equals(ref left, ref right);

        /// <summary>
        ///     Determines the relative order of the values.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to compare.</typeparam>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>A value that indicates the relative order of the values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Compare(ref left, ref right);
    }
}