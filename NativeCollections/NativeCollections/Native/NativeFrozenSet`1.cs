using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.ArchitectureHelpers;
using static NativeCollections.NativeFrozenSet;
#if !NET5_0_OR_GREATER
using System.Buffers;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenSet<T> : IDisposable, IEquatable<NativeFrozenSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFrozenSetHandle<T>* _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

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
                var handle = _handle;
                return handle->Items(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
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
                var handle = _handle;
                return handle->Count(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create<TReadOnlyCollection>(TReadOnlyCollection source) where TReadOnlyCollection : IReadOnlyCollection<T>
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(in UnsafeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(in StackallocHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                items[index] = kvp;
                ++index;
            }

            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenSet(ReadOnlySpan<T> source)
        {
            if (source.Length == 0)
            {
                var handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(EmptyFrozenSet<T>)), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetHandle<EmptyFrozenSet<T>, T>();
                Unsafe.AsRef<EmptyFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new EmptyFrozenSet<T>();
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
                    var handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(SmallComparableFrozenSet<T>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.AsRef<SmallComparableFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallComparableFrozenSet<T>(items);
                    _handle = handle;
                }
                else
                {
                    foreach (var item in source)
                    {
                        items[index] = item;
                        ++index;
                    }

                    var handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(SmallFrozenSet<T>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetHandle<SmallFrozenSet<T>, T>();
                    Unsafe.AsRef<SmallFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallFrozenSet<T>(items);
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
                    var handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(Int32FrozenSet)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenSetHandle<int>>(handle) = GetHandle<Int32FrozenSet, int>();
                    Unsafe.AsRef<Int32FrozenSet>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new Int32FrozenSet(buffer.Cast<int>());
                    _handle = handle;
                }
                else
                {
                    var handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(DefaultFrozenSet<T>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetHandle<DefaultFrozenSet<T>, T>();
                    Unsafe.AsRef<DefaultFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new DefaultFrozenSet<T>(buffer);
                    _handle = handle;
                }
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFrozenSet<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFrozenSet<T> nativeHashSet && nativeHashSet == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeFrozenSet<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item)
        {
            var handle = _handle;
            return handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), item) >= 0;
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
            var handle = _handle;
            var index = handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), equalValue);
            if (index >= 0)
            {
                actualValue = handle->Items(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE))[index];
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeFrozenSet<T> Empty => new();

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            /// <summary>
            ///     Items
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<T> items)
            {
                _items = items;
                _index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <returns>
            ///     <code data-dev-comment-type="langword">true</code> if the enumerator was successfully advanced to the next element;
            ///     <code data-dev-comment-type="langword">false</code> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ++_index;
                if ((uint)_index < (uint)_items.Length)
                    return true;
                _index = _items.Length;
                return false;
            }

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if ((uint)_index >= (uint)_items.Length)
                        ThrowHelpers.ThrowInvalidOperationException();
                    return _items[_index];
                }
            }
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
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