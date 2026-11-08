using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to characters.
    /// </summary>
    internal static class Char
    {
        /// <summary>
        ///     Converts a 16-bit unsigned integer to a character using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char FromU16Unchecked(ushort value) => unchecked((char)value);

        /// <summary>
        ///     Converts a 32-bit signed integer to a character using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char FromI32Unchecked(int value) => unchecked((char)value);
    }
}