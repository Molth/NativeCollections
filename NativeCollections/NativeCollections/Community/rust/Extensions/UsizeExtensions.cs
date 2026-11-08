using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides wrapping arithmetic helpers for native-sized unsigned integers.
    /// </summary>
    internal static class UsizeExtensions
    {
        /// <summary>
        ///     Adds two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to add.</param>
        /// <returns>The wrapped sum.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint WrappingAdd(this nuint value, nuint other) => unchecked(value + other);

        /// <summary>
        ///     Subtracts one value from another using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value from which to subtract.</param>
        /// <param name="other">The value to subtract.</param>
        /// <returns>The wrapped difference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint WrappingSub(this nuint value, nuint other) => unchecked(value - other);

        /// <summary>
        ///     Multiplies two values using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The first value.</param>
        /// <param name="other">The value to multiply.</param>
        /// <returns>The wrapped product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint WrappingMul(this nuint value, nuint other) => unchecked(value * other);

        /// <summary>
        ///     Negates a value using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The wrapped negation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint WrappingNeg(this nuint value) => unchecked(new UIntPtr(0) - value);
    }
}