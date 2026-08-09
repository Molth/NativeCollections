using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
    ///     that meet certain statistical requirements for randomness.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <typeparam name="TRandom">The type of pseudo-random number generator.</typeparam>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stringLength" /> is not zero or a positive number.</exception>
        /// <seealso cref="IRandom.GetItems{T}(ReadOnlySpan{T}, Span{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<TRandom>(ref this TRandom random, ReadOnlySpan<char> source, int stringLength) where TRandom : struct, IRandom
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(source, ExceptionArgument.source);
            if (stringLength <= 0)
            {
                ThrowHelpers.ThrowIfNegative(stringLength, ExceptionArgument.stringLength);
                return "";
            }

            var destination = new string((char)0, stringLength);
            random.GetItems(source, MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(destination.AsSpan()), destination.Length));
            return destination;
        }

        /// <summary>
        ///     Creates a string filled with random hexadecimal characters.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="stringLength">The length of string to create.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        /// <typeparam name="TRandom">The type of pseudo-random number generator.</typeparam>
        /// <returns>A string populated with random hexadecimal characters.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHexString<TRandom>(ref this TRandom random, int stringLength, bool lowercase = false) where TRandom : struct, IRandom => random.GetString(GetHexChoices(lowercase), stringLength);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        /// <typeparam name="TRandom">The type of pseudo-random number generator.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetHexString<TRandom>(ref this TRandom random, Span<char> destination, bool lowercase = false) where TRandom : struct, IRandom => random.GetItems(GetHexChoices(lowercase), destination);

        /// <summary>
        ///     Gets all possible hex characters for the specified casing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetHexChoices(bool lowercase) => lowercase ? "0123456789abcdef" : "0123456789ABCDEF";
    }
}