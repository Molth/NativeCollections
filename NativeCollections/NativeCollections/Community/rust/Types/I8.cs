using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 8-bit signed integers.
    /// </summary>
    internal static class I8
    {
        /// <summary>
        ///     Converts an 8-bit unsigned integer to 8-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte FromU8Unchecked(byte value) => unchecked((sbyte)value);
    }
}