using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an implementation of the XxHash32 hash algorithm for generating a 32-bit hash.
    /// </summary>
    /// <remarks>
    ///     The implementation is based on <see href="https://github.com/DoumanAsh/xxhash-rust">xxhash-rust</see>.
    /// </remarks>
    internal static partial class XxHash32
    {
        /// <summary>
        ///     The number of bytes processed by one main xxHash32 loop iteration.
        /// </summary>
        private const int CHUNK_SIZE = sizeof(uint) * 4;

        /// <summary>
        ///     The first xxHash32 prime constant.
        /// </summary>
        private const uint PRIME_1 = 0x9E3779B1U;

        /// <summary>
        ///     The second xxHash32 prime constant.
        /// </summary>
        private const uint PRIME_2 = 0x85EBCA77U;

        /// <summary>
        ///     The third xxHash32 prime constant.
        /// </summary>
        private const uint PRIME_3 = 0xC2B2AE3DU;

        /// <summary>
        ///     The fourth xxHash32 prime constant.
        /// </summary>
        private const uint PRIME_4 = 0x27D4EB2FU;

        /// <summary>
        ///     The fifth xxHash32 prime constant.
        /// </summary>
        private const uint PRIME_5 = 0x165667B1U;

        /// <summary>
        ///     Performs one xxHash32 round and returns the updated accumulator.
        /// </summary>
        /// <param name="acc">The current accumulator.</param>
        /// <param name="input">The input value for the round.</param>
        /// <returns>The updated accumulator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Round(uint acc, uint input) => acc.WrappingAdd(input.WrappingMul(PRIME_2)).RotateLeft(13).WrappingMul(PRIME_1);

        /// <summary>
        ///     Applies the final avalanche mixing steps to an xxHash32 accumulator.
        /// </summary>
        /// <param name="input">The accumulator to mix.</param>
        /// <returns>The mixed accumulator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Avalanche(uint input)
        {
            input ^= input >> 15;
            input = input.WrappingMul(PRIME_2);
            input ^= input >> 13;
            input = input.WrappingMul(PRIME_3);
            input ^= input >> 16;
            return input;
        }
    }
}