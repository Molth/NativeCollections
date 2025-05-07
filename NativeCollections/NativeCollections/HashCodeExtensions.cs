#if !NET6_0_OR_GREATER
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
    ///     HashCode extensions
    /// </summary>
    public static class HashCodeExtensions
    {
        /// <summary>
        ///     Add bytes
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddBytes(ref this HashCode hashCode, ReadOnlySpan<byte> buffer)
        {
            for (; buffer.Length >= 4; buffer = buffer.Slice(4))
                hashCode.Add(Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference(buffer)));
            for (var i = 0; i < buffer.Length; ++i)
                hashCode.Add((int)buffer[i]);
        }
    }
}
#endif