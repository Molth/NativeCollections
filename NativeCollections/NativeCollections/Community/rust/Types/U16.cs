using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 16-bit unsigned integers.
    /// </summary>
    internal static class U16
    {
        /// <summary>
        ///     Converts a 16-bit signed integer to 16-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FromI16Unchecked(short value) => unchecked((ushort)value);

        /// <summary>
        ///     Converts a 32-bit signed integer to 16-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FromI32Unchecked(int value) => unchecked((ushort)value);

        /// <summary>
        ///     Converts a 32-bit unsigned integer to 16-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FromU32Unchecked(uint value) => unchecked((ushort)value);

        /// <summary>
        ///     Converts a character to a 16-bit unsigned integer using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort FromCharUnchecked(char value) => unchecked(value);
    }
}