using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;
using static NativeCollections.PaddingHelpers;

// ReSharper disable All

namespace crossbeam
{
    [StructLayout(LayoutKind.Sequential, Size = 1 * CACHE_LINE_SIZE)]
    internal struct CachePaddedAtomicU32
    {
        private UnsafeAtomicU32 _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint fetch_add(uint value) => _data.Add(value).wrapping_sub(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint fetch_sub(uint value) => _data.Sub(value).wrapping_add(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint load(Ordering order) => _data.Load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint compare_exchange(uint current, uint @new) => _data.CompareExchange(@new, current);
    }
}