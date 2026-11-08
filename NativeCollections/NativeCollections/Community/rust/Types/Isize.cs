using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to native-sized signed integers.
    /// </summary>
    internal static class Isize
    {
        /// <summary>
        ///     Converts a 32-bit signed integer to native-sized signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromI32Unchecked(int value) => unchecked(value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to native-sized signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromU32Unchecked(uint value) => unchecked((nint)value);

        /// <summary>
        ///     Converts a 64-bit signed integer to native-sized signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromI64Unchecked(long value) => unchecked((nint)value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to native-sized signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromU64Unchecked(ulong value) => unchecked((nint)value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to native-sized signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromUsizeUnchecked(nuint value) => unchecked((nint)value);

        /// <summary>
        ///     Converts a pointer to a native-sized signed integer using an unchecked conversion.
        /// </summary>
        /// <param name="value">The pointer to convert.</param>
        /// <returns>The converted address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe nint FromPtrUnchecked(void* value) => unchecked((nint)value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to a native-sized signed integer.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when <paramref name="value" /> is greater than the maximum native-sized signed integer.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint FromUsize(nuint value) => checked((nint)value);
    }
}