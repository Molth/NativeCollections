using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native hashCode
    /// </summary>
    [Customizable("public static int GetHashCode(ReadOnlySpan<byte> buffer)")]
    public static unsafe class NativeHashCode
    {
        /// <summary>
        ///     Default seed
        /// </summary>
        private static readonly uint DefaultSeed = NativeRandom.NextUInt32();

        /// <summary>
        ///     GetHashCode
        /// </summary>
        private static delegate* managed<ReadOnlySpan<byte>, int> _getHashCode;

        /// <summary>
        ///     Custom GetHashCode
        /// </summary>
        /// <param name="getHashCode">GetHashCode</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<byte>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(in T obj) where T : unmanaged => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(ReadOnlySpan<T> buffer) where T : unmanaged => GetHashCode(MemoryMarshal.AsBytes(buffer));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(void* ptr, int byteCount)
        {
            ThrowHelpers.ThrowIfNegative(byteCount, ExceptionArgument.byteCount);
            return GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(ptr), byteCount));
        }

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [Customizable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

            return XxHash32.HashToInt32(ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length, DefaultSeed);
        }
    }
}