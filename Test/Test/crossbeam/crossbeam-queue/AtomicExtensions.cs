using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable ALL

namespace crossbeam
{
    public static unsafe class AtomicExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* load<T>(ref this UnsafeAtomicReference<T> value, Ordering order) where T : unmanaged
        {
            return value.Load(order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store<T>(ref this UnsafeAtomicReference<T> value, T* v, Ordering order) where T : unmanaged
        {
            value.Store(v, order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint load(ref this UnsafeAtomicUIntPtr value, Ordering order)
        {
            return value.Load(order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store(ref this UnsafeAtomicUIntPtr value, nuint v, Ordering order)
        {
            value.Store(v, order);
        }
    }
}