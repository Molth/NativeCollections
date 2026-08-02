using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable All

namespace crossbeam
{
    internal static class AtomicExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint load(ref this UnsafeAtomicUsize value, Ordering order) => value.Load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store(ref this UnsafeAtomicUsize value, nuint @new, Ordering order) => value.Store(@new, order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint compare_exchange(ref this UnsafeAtomicUsize value, nuint current, nuint @new) => value.CompareExchange(@new, current);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref nuint get_mut(ref this UnsafeAtomicUsize value) => ref value.AsRef();
    }
}