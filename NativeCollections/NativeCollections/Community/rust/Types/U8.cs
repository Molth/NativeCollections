using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 8-bit unsigned integers.
    /// </summary>
    internal static class U8
    {
        /// <summary>
        ///     Converts a 8-bit signed integer to 8-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FromI8Unchecked(sbyte value) => unchecked((byte)value);

        /// <summary>
        ///     Converts a 32-bit signed integer to 8-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FromI32Unchecked(int value) => unchecked((byte)value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to 8-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte FromU32Unchecked(uint value) => unchecked((byte)value);
    }
}