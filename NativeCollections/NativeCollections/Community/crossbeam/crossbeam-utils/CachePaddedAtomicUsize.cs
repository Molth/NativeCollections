using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable All

namespace crossbeam
{
    [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
    internal unsafe struct CachePaddedAtomicUsize
    {
        public UnsafeAtomicUsize data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref nuint get_mut() => ref data.get_mut();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint load(Ordering order) => data.load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void store(nuint @new, Ordering order) => data.store(@new, order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint compare_exchange(nuint current, nuint @new) => data.compare_exchange(current, @new);
    }
}