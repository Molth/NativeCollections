using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable ALL

namespace crossbeam
{
    public static unsafe class AtomicExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint fetch_update<TClosure>(ref this UnsafeAtomicUsize value, ref TClosure closure, delegate* managed<ref TClosure, nuint, nuint> func) => value.update(ref closure, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<nuint> try_update<TClosure>(ref this UnsafeAtomicUsize value, ref TClosure closure, delegate* managed<ref TClosure, nuint, Option<nuint>> func) => AtomicHelpers.TryUpdate(ref value.AsRef(), ref closure, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint update<TClosure>(ref this UnsafeAtomicUsize value, ref TClosure closure, delegate* managed<ref TClosure, nuint, nuint> func) => AtomicHelpers.Update<TClosure>(ref value.AsRef(), ref closure, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* load<T>(ref this UnsafeAtomicPtr<T> value, Ordering order) where T : unmanaged => value.Load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store<T>(ref this UnsafeAtomicPtr<T> value, T* v, Ordering order) where T : unmanaged => value.Store(v, order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint load(ref this UnsafeAtomicUsize value, Ordering order) => value.Load(order);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void store(ref this UnsafeAtomicUsize value, nuint v, Ordering order) => value.Store(v, order);
    }
}