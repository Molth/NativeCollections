using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Computes hash codes for unmanaged values and byte buffers.
    /// </summary>
    [Customizable("public static int GetHashCode(ReadOnlySpan<byte> buffer)")]
    public static unsafe class NativeHashCode
    {
        /// <summary>
        ///     Default seed value used for hash code calculation.
        /// </summary>
        private static readonly ulong DefaultSeed = NativeRandom.Next<ulong>();

        /// <summary>
        ///     Custom get hash code handler.
        /// </summary>
        private static delegate* managed<ReadOnlySpan<byte>, int> _getHashCode;

        /// <summary>
        ///     Configures custom get hash code handler.
        /// </summary>
        /// <param name="getHashCode">The function used to compute hash codes from byte spans.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<byte>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to hash.</typeparam>
        /// <param name="obj">The value to hash.</param>
        /// <returns>The hash code for <paramref name="obj" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(in T obj) where T : unmanaged => GetHashCode(MemoryMarshalHelpers.AsReadOnlyBytes(ref Unsafe.AsRef(in obj)));

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the elements to hash.</typeparam>
        /// <param name="buffer">The elements to hash.</param>
        /// <returns>The hash code for <paramref name="buffer" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(ReadOnlySpan<T> buffer) where T : unmanaged => GetHashCode(MemoryMarshal.AsBytes(buffer));

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        /// <param name="ptr">A pointer to the bytes to hash.</param>
        /// <param name="byteCount">The number of bytes to hash.</param>
        /// <returns>The hash code for the specified bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="byteCount" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(void* ptr, int byteCount)
        {
            ThrowHelpers.ThrowIfNegative(byteCount, ExceptionArgument.byteCount);
            return GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(ptr), byteCount));
        }

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        /// <param name="buffer">The bytes to hash.</param>
        /// <returns>The hash code for the specified bytes.</returns>
        [Customizable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

            return Environment.Is64BitProcess ? XxHash64.HashToU64(buffer, DefaultSeed).GetHashCode() : I32.FromU32Unchecked(XxHash32.HashToU32(buffer, U32.FromU64Unchecked(DefaultSeed)));
        }
    }
}