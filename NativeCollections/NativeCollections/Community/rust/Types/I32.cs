using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 32-bit signed integers.
    /// </summary>
    internal static class I32
    {
        /// <summary>
        ///     Converts an 8-bit unsigned integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromU8Unchecked(byte value) => unchecked(value);

        /// <summary>
        ///     Converts a 16-bit unsigned integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromU16Unchecked(ushort value) => unchecked(value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromU32Unchecked(uint value) => unchecked((int)value);

        /// <summary>
        ///     Converts a 64-bit signed integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromI64Unchecked(long value) => unchecked((int)value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromU64Unchecked(ulong value) => unchecked((int)value);

        /// <summary>
        ///     Converts a native-sized signed integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromIsizeUnchecked(nint value) => unchecked((int)value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to 32-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromUsizeUnchecked(nuint value) => unchecked((int)value);

        /// <summary>
        ///     Converts a 64-bit double-precision floating-point number
        ///     to a 32-bit signed integer using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromF64Unchecked(double value) => unchecked((int)value);

        /// <summary>
        ///     Converts a character to a 32-bit signed integer using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromCharUnchecked(char value) => unchecked(value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when <paramref name="value" /> is greater than <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromUsize(nuint value) => checked((int)value);
    }
}