using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an implementation of the XxHash64 hash algorithm for generating a 64-bit hash.
    /// </summary>
    /// <remarks>
    ///     The implementation is based on <see href="https://github.com/DoumanAsh/xxhash-rust">xxhash-rust</see>.
    /// </remarks>
    internal static partial class XxHash64
    {
        /// <summary>
        ///     The number of bytes processed by one main xxHash64 loop iteration.
        /// </summary>
        private const int CHUNK_SIZE = sizeof(ulong) * 4;

        /// <summary>
        ///     The first xxHash64 prime constant.
        /// </summary>
        private const ulong PRIME_1 = 0x9E3779B185EBCA87;

        /// <summary>
        ///     The second xxHash64 prime constant.
        /// </summary>
        private const ulong PRIME_2 = 0xC2B2AE3D27D4EB4F;

        /// <summary>
        ///     The third xxHash64 prime constant.
        /// </summary>
        private const ulong PRIME_3 = 0x165667B19E3779F9UL;

        /// <summary>
        ///     The fourth xxHash64 prime constant.
        /// </summary>
        private const ulong PRIME_4 = 0x85EBCA77C2B2AE63;

        /// <summary>
        ///     The fifth xxHash64 prime constant.
        /// </summary>
        private const ulong PRIME_5 = 0x27D4EB2F165667C5UL;

        /// <summary>
        ///     Performs one xxHash64 round and returns the updated accumulator.
        /// </summary>
        /// <param name="acc">The current accumulator.</param>
        /// <param name="input">The input value for the round.</param>
        /// <returns>The updated accumulator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Round(ulong acc, ulong input) => acc.WrappingAdd(input.WrappingMul(PRIME_2)).RotateLeft(31).WrappingMul(PRIME_1);

        /// <summary>
        ///     Merges one completed round into an xxHash64 accumulator.
        /// </summary>
        /// <param name="acc">The current accumulator.</param>
        /// <param name="val">The value to merge.</param>
        /// <returns>The updated accumulator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MergeRound(ulong acc, ulong val)
        {
            acc ^= Round(U64.FromI32Unchecked(0), val);
            return acc.WrappingMul(PRIME_1).WrappingAdd(PRIME_4);
        }

        /// <summary>
        ///     Applies the final avalanche mixing steps to an xxHash64 accumulator.
        /// </summary>
        /// <param name="input">The accumulator to mix.</param>
        /// <returns>The mixed accumulator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Avalanche(ulong input)
        {
            input ^= input >> 33;
            input = input.WrappingMul(PRIME_2);
            input ^= input >> 29;
            input = input.WrappingMul(PRIME_3);
            input ^= input >> 32;
            return input;
        }
    }
}