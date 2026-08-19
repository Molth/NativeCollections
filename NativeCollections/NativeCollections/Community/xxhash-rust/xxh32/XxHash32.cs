using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an implementation of the XxHash32 hash algorithm for generating a 32-bit hash.
    /// </summary>
    /// <remarks>https://github.com/DoumanAsh/xxhash-rust</remarks>
    internal static class XxHash32
    {
        /// <summary>
        ///     Computes the hash of the provided data.
        /// </summary>
        /// <param name="source">The data to hash. The default is zero.</param>
        /// <param name="seed">The seed value for this hash computation.</param>
        /// <returns>The computed hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint HashToUInt32(ReadOnlySpan<byte> source, uint seed = default) => xxhash_rust.XxHash32.xxh32(source, seed);
    }
}