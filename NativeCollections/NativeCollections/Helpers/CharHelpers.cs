using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Char helpers
    /// </summary>
    internal static class CharHelpers
    {
        /// <summary>
        ///     Is ascii digit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiDigit(char c)
        {
#if NET7_0_OR_GREATER
            return char.IsAsciiDigit(c);
#else
            return IsBetween(c, '0', '9');
#endif
        }

        /// <summary>
        ///     Is between
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(char c, char minInclusive, char maxInclusive)
        {
#if NET7_0_OR_GREATER
            return char.IsBetween(c, minInclusive, maxInclusive);
#else
            return (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);
#endif
        }
    }
}