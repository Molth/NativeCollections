using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces
    ///     a sequence of numbers that meet certain statistical requirements for randomness.
    /// </summary>
    internal interface IRandomState
    {
        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        uint Next32();

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        ulong Next64();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        void NextBytes(Span<byte> buffer);

        /// <summary>
        ///     Generates a random <see cref="bool" /> value.
        /// </summary>
        /// <returns><see langword="true" /> or <see langword="false" />.</returns>
        bool NextBool();
    }
}