using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS1591
#pragma warning disable CS8625
#pragma warning disable CS8632

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
        public static int GetHashCode<T>(in T obj) where T : unmanaged => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), sizeof(T)));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(ReadOnlySpan<T> buffer) where T : unmanaged => GetHashCode(MemoryMarshal.Cast<T, byte>(buffer));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(void* ptr, int byteCount) => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref *(byte*)ptr, byteCount));

        /// <summary>
        ///     Compute hash 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> buffer)
        {
            if (_getHashCode != null)
                return _getHashCode(buffer);

            var hashCode = new HashCode();
#if NET6_0_OR_GREATER
            hashCode.AddBytes(buffer);
#else
            for (; buffer.Length >= 4; buffer = buffer.Slice(4))
                hashCode.Add(Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(buffer)));
            for (var i = 0; i < buffer.Length; ++i)
                hashCode.Add((int)buffer[i]);
#endif
            return hashCode.ToHashCode();
        }
    }
}