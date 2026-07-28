using System.Runtime.CompilerServices;
using NativeCollections;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable All

namespace crossbeam
{
    internal static unsafe class AtomicExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* load<T>(ref this UnsafeAtomicPtr<T> value, Ordering order) where T : unmanaged => value.Load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store<T>(ref this UnsafeAtomicPtr<T> value, T* @new, Ordering order) where T : unmanaged => value.Store(@new, order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* swap<T>(ref this UnsafeAtomicPtr<T> value, T* @new) where T : unmanaged => value.Swap(@new);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* compare_exchange<T>(ref this UnsafeAtomicPtr<T> value, T* current, T* @new) where T : unmanaged => value.CompareExchange(@new, current);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T* get_mut<T>(ref this UnsafeAtomicPtr<T> value) where T : unmanaged => ref value.AsRef();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint fetch_or(ref this UnsafeAtomicUsize value, nuint @new) => value.Or(@new);

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