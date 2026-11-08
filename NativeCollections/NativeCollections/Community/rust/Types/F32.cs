using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer
    ///     values to 32-bit single-precision floating-point numbers.
    /// </summary>
    internal static class F32
    {
        /// <summary>
        ///     Converts a 32-bit unsigned integer to a 32-bit single-precision
        ///     floating-point number using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float FromU32Unchecked(uint value) => unchecked(value);
    }
}