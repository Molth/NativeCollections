using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;
using rust;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable All

namespace crossbeam
{
    [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
    internal unsafe struct CachePaddedAtomicPtr<T> where T : unmanaged
    {
        public UnsafeAtomicPtr<T> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T* get_mut() => ref data.get_mut();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* load(Ordering order) => data.load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* swap(T* @new) => data.swap(@new);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* compare_exchange(T* current, T* @new) => data.compare_exchange(current, @new);
    }
}