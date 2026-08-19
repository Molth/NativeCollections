using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable All

namespace rust
{
    internal static class ReadOnlySpanExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly byte get_unchecked(this ReadOnlySpan<byte> buffer, int index) => ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> split(this ReadOnlySpan<byte> buffer, int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), start), buffer.Length - start);
    }
}