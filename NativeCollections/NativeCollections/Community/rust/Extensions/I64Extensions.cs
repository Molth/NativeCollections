using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides wrapping arithmetic helpers for 64-bit signed integers.
    /// </summary>
    internal static class I64Extensions
    {
        /// <summary>
        ///     Adds two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to add.</param>
        /// <returns>The wrapped sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long WrappingAdd(this long value, long other) => unchecked(value + other);

        /// <summary>
        ///     Subtracts one value from another using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value from which to subtract.</param>
        /// <param name="other">The value to subtract.</param>
        /// <returns>The wrapped difference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long WrappingSub(this long value, long other) => unchecked(value - other);

        /// <summary>
        ///     Multiplies two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to multiply.</param>
        /// <returns>The wrapped product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long WrappingMul(this long value, long other) => unchecked(value * other);

        /// <summary>
        ///     Negates a value using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The wrapped negation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long WrappingNeg(this long value) => unchecked(-value);
    }
}