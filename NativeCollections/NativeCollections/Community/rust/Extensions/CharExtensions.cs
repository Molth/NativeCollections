using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides arithmetic helpers for characters.
    /// </summary>
    internal static class CharExtensions
    {
        /// <summary>
        ///     Subtracts one character value from another.
        /// </summary>
        /// <param name="value">The value from which to subtract.</param>
        /// <param name="other">The value to subtract.</param>
        /// <returns>The difference as a 32-bit signed integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WrappingSub(this char value, char other) => unchecked(value - other);
    }
}