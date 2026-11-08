using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides conversions from signed and unsigned integer values to 32-bit unsigned integers.
    /// </summary>
    internal static class U32
    {
        /// <summary>
        ///     Converts a 8-bit unsigned integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromU8Unchecked(byte value) => unchecked(value);

        /// <summary>
        ///     Converts a 16-bit unsigned integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromU16Unchecked(ushort value) => unchecked(value);

        /// <summary>
        ///     Converts a 32-bit signed integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromI32Unchecked(int value) => unchecked((uint)value);

        /// <summary>
        ///     Converts a 64-bit unsigned integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromU64Unchecked(ulong value) => unchecked((uint)value);

        /// <summary>
        ///     Converts a native-sized signed integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromIsizeUnchecked(nint value) => unchecked((uint)value);

        /// <summary>
        ///     Converts a native-sized unsigned integer to 32-bit unsigned integer using unchecked conversion.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FromUsizeUnchecked(nuint value) => unchecked((uint)value);
    }
}