using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 16-bit signed integers.
    /// </summary>
    internal static class I16
    {
        /// <summary>
        ///     Converts a 16-bit unsigned integer to 16-bit signed integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short FromU16Unchecked(ushort value) => unchecked((short)value);
    }
}