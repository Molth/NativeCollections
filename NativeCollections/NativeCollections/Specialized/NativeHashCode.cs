using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Combines the hash code for multiple values into a single hash code.
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
        ///     Configures custom get hash code handler.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<ReadOnlySpan<byte>, int> getHashCode) => _getHashCode = getHashCode;

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(in T obj) where T : unmanaged => GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode<T>(ReadOnlySpan<T> buffer) where T : unmanaged => GetHashCode(MemoryMarshal.AsBytes(buffer));

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="byteCount" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(void* ptr, int byteCount)
        {
            ThrowHelpers.ThrowIfNegative(byteCount, ExceptionArgument.byteCount);
            return GetHashCode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(ptr), byteCount));
        }

        /// <summary>
        ///     Diffuses the hash code returned by the specified bytes.
        /// </summary>
        [Customizable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(ReadOnlySpan<byte> buffer)
        {
            var getHashCode = _getHashCode;
            if (getHashCode != null)
                return getHashCode(buffer);

#if NET10_0_OR_GREATER
            var hashCode = new HashCode();
            hashCode.AddBytes(buffer);
            return hashCode.ToHashCode() + (int)DefaultSeed;
#else
            return XxHash32.HashToInt32(ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length, DefaultSeed);
#endif
        }
    }
}