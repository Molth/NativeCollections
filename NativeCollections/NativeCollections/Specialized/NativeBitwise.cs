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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Equals(ref left, ref right);

        /// <summary>
        ///     Determines the relative order of the values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare<T>(ref T left, ref T right) where T : unmanaged => SpanHelpers.Compare(ref left, ref right);
    }
}