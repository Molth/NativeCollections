using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to native-sized unsigned integers.
    /// </summary>
    internal static class Usize
    {
        /// <summary>
        ///     Converts a 32-bit signed integer to native-sized unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromI32Unchecked(int value) => unchecked((nuint)value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to native-sized unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromU32Unchecked(uint value) => unchecked(value);

        /// <summary>
        ///     Converts a 64-bit signed integer to native-sized unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromI64Unchecked(long value) => unchecked((nuint)value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to native-sized unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromU64Unchecked(ulong value) => unchecked((nuint)value);

        /// <summary>
        ///     Converts a native-sized signed integer to native-sized unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromIsizeUnchecked(nint value) => unchecked((nuint)value);

        /// <summary>
        ///     Converts a 32-bit signed integer to a native-sized unsigned integer.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="OverflowException">Thrown when <paramref name="value" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint FromI32(int value) => checked((nuint)value);
    }
}