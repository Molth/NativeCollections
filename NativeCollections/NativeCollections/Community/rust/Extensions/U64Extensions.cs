using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides wrapping arithmetic helpers for 64-bit unsigned integers.
    /// </summary>
    internal static class U64Extensions
    {
        /// <summary>
        ///     Adds two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to add.</param>
        /// <returns>The wrapped sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong WrappingAdd(this ulong value, ulong other) => unchecked(value + other);

        /// <summary>
        ///     Subtracts one value from another using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value from which to subtract.</param>
        /// <param name="other">The value to subtract.</param>
        /// <returns>The wrapped difference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong WrappingSub(this ulong value, ulong other) => unchecked(value - other);

        /// <summary>
        ///     Multiplies two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to multiply.</param>
        /// <returns>The wrapped product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong WrappingMul(this ulong value, ulong other) => unchecked(value * other);

        /// <summary>
        ///     Negates a value using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The wrapped negation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong WrappingNeg(this ulong value) => unchecked(0UL - value);

        /// <summary>
        ///     Rotates the bits in a 64-bit unsigned integer to the left by the specified offset.
        /// </summary>
        /// <param name="value">The value whose bits are rotated.</param>
        /// <param name="offset">The number of bits to rotate.</param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(this ulong value, int offset) => BitOperationsHelpers.RotateLeft(value, offset);
    }
}