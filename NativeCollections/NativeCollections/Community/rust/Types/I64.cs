using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 64-bit signed integers.
    /// </summary>
    internal static class I64
    {
        /// <summary>
        ///     Converts a 32-bit signed integer to 64-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromI32Unchecked(int value) => unchecked(value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to 64-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromU32Unchecked(uint value) => unchecked(value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to 64-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromU64Unchecked(ulong value) => unchecked((long)value);

        /// <summary>
        ///     Converts a native-sized signed integer to 64-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromIsizeUnchecked(nint value) => unchecked(value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to 64-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromUsizeUnchecked(nuint value) => unchecked((long)value);

        /// <summary>
        ///     Converts a 64-bit double-precision floating-point number
        ///     to a 64-bit signed integer using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FromF64Unchecked(double value) => unchecked((long)value);
    }
}