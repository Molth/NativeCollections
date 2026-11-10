using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 64-bit unsigned integers.
    /// </summary>
    internal static class U64
    {
        /// <summary>
        ///     Converts an 8-bit unsigned integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromU8Unchecked(byte value) => unchecked(value);

        /// <summary>
        ///     Converts a 32-bit signed integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromI32Unchecked(int value) => unchecked((ulong)value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromU32Unchecked(uint value) => unchecked(value);

        /// <summary>
        ///     Converts a 64-bit signed integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromI64Unchecked(long value) => unchecked((ulong)value);

        /// <summary>
        ///     Converts a native-sized signed integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromIsizeUnchecked(nint value) => unchecked((ulong)value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to 64-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong FromUsizeUnchecked(nuint value) => unchecked(value);
    }
}