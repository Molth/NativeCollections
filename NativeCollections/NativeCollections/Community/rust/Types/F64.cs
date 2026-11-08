using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer
    ///     values to 64-bit double-precision floating-point numbers.
    /// </summary>
    internal static class F64
    {
        /// <summary>
        ///     Converts a 32-bit signed integer to a 64-bit double-precision
        ///     floating-point number using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FromI32Unchecked(int value) => unchecked(value);

        /// <summary>
        ///     Converts a 64-bit signed integer to a 64-bit double-precision
        ///     floating-point number using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FromI64Unchecked(long value) => unchecked(value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to a 64-bit double-precision
        ///     floating-point number using an unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FromU64Unchecked(ulong value) => unchecked(value);
    }
}