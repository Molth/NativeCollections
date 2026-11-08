using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides wrapping arithmetic helpers for 8-bit unsigned integers.
    /// </summary>
    internal static class U8Extensions
    {
        /// <summary>
        ///     Negates a value using unchecked arithmetic and allows the result to wrap around.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The wrapped negation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte WrappingNeg(this byte value) => unchecked((byte)-value);
    }
}