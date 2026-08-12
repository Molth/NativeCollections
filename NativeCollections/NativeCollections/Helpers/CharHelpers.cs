using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a character as a UTF-16 code unit.
    /// </summary>
    internal static class CharHelpers
    {
        /// <summary>
        ///     Indicates whether a character is categorized as an ASCII digit.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>
        ///     true if <paramref name="c" /> is an ASCII digit;
        ///     otherwise, false.
        /// </returns>
        /// <remarks>
        ///     This determines whether the character is in the range '0' through '9', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiDigit(char c)
        {
#if NET7_0_OR_GREATER
            return char.IsAsciiDigit(c);
#else
            return IsBetween(c, '0', '9');
#endif
        }

#if !NET7_0_OR_GREATER
        /// <summary>
        ///     Indicates whether a character is within the specified inclusive range.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <param name="minInclusive">The lower bound, inclusive.</param>
        /// <param name="maxInclusive">The upper bound, inclusive.</param>
        /// <returns>true if <paramref name="c" /> is within the specified range; otherwise, false.</returns>
        /// <remarks>
        ///     The method does not validate that <paramref name="maxInclusive" /> is greater than or equal
        ///     to <paramref name="minInclusive" />.  If <paramref name="maxInclusive" /> is less than
        ///     <paramref name="minInclusive" />, the behavior is undefined.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(char c, char minInclusive, char maxInclusive) => (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);
#endif
    }
}