using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeFrozenSet;
#if !NET5_0_OR_GREATER
using System.Buffers;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632
#pragma warning disable CS9082
#pragma warning disable CS9092

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenSet<T> : IDisposable, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeFrozenSetHandle<T> _handle;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Items
        /// </summary>
        public ReadOnlySpan<T> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Items(ref handle.Value);
            }
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Count(ref handle.Value);
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create<TReadOnlyCollection>(in TReadOnlyCollection source) where TReadOnlyCollection : IReadOnlyCollection<T>
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(in UnsafeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSet<T> Create(in StackallocHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenSet(ReadOnlySpan<T> source)
        {
            if (source.Length == 0)
            {
                var handle = GetUnsafeHandle<EmptyFrozenSet<T>, T>();
                Unsafe.As<UnsafeFrozenSetValue, EmptyFrozenSet<T>>(ref handle.Value) = new EmptyFrozenSet<T>();
                _handle = handle;
                return;
            }

            if (source.Length <= 10)
            {
                var items = new NativeArray<T>(source.Length);
                var index = 0;
                if (FrozenHelpers.IsKnownComparable<T>())
                {
#if !NET5_0_OR_GREATER
                    var array = ArrayPool<T>.Shared.Rent(source.Length);
#endif
                    foreach (var item in source)
                    {
#if NET5_0_OR_GREATER
                        items[index] = item;
#else
                        array[index] = item;
#endif
                        ++index;
                    }

#if NET5_0_OR_GREATER
                    items.AsSpan().Sort();
#else
                    Array.Sort(array, 0, source.Length);
                    array.AsSpan(0, source.Length).CopyTo(items);
                    ArrayPool<T>.Shared.Return(array);
#endif
                    var handle = GetUnsafeHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.As<UnsafeFrozenSetValue, SmallComparableFrozenSet<T>>(ref handle.Value) = new SmallComparableFrozenSet<T>(items);
                    _handle = handle;
                }
                else
                {
                    foreach (var item in source)
                    {
                        items[index] = item;
                        ++index;
                    }

                    var handle = GetUnsafeHandle<SmallFrozenSet<T>, T>();
                    Unsafe.As<UnsafeFrozenSetValue, SmallFrozenSet<T>>(ref handle.Value) = new SmallFrozenSet<T>(items);
                    _handle = handle;
                }
            }
            else
            {
                using var buffer = new NativeArray<T>(source.Length);
                var index = 0;
                foreach (var kvp in source)
                {
                    buffer[index] = kvp;
                    ++index;
                }

                if (typeof(T) == typeof(int))
                {
                    var handle = GetUnsafeHandle<Int32FrozenSet, int>();
                    Unsafe.As<UnsafeFrozenSetValue, Int32FrozenSet>(ref handle.Value) = new Int32FrozenSet(buffer.Cast<int>());
                    _handle = Unsafe.As<UnsafeFrozenSetHandle<int>, UnsafeFrozenSetHandle<T>>(ref handle);
                }
                else
                {
                    var handle = GetUnsafeHandle<DefaultFrozenSet<T>, T>();
                    Unsafe.As<UnsafeFrozenSetValue, DefaultFrozenSet<T>>(ref handle.Value) = new DefaultFrozenSet<T>(buffer);
                    _handle = handle;
                }
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
        }

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.FindItemIndex(ref handle.Value, item) >= 0;
        }

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in T equalValue, out T actualValue)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            var index = handle.FindItemIndex(ref handle.Value, equalValue);
            if (index >= 0)
            {
                actualValue = handle.Items(ref handle.Value)[index];
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFrozenSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenSet<T>.Enumerator GetEnumerator()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.GetEnumerator(ref handle.Value);
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}