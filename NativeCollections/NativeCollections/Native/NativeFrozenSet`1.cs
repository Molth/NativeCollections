using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;
using static NativeCollections.NativeFrozenSet;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenSet<T> : IIsCreated, IDisposable, IEquatable<NativeFrozenSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFrozenSetHandle<T>* _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

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
        public static NativeFrozenSet<T> Create(HashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            var index = 0;
            foreach (var item in source)
                items[index++] = item;
            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(in UnsafeHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create(in StackallocHashSet<T> source)
        {
            using var items = new NativeArray<T>(source.Count);
            source.CopyTo(items);
            return new NativeFrozenSet<T>(items);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenSet([MustBeDistinct] ReadOnlySpan<T> source) => _handle = Initialize(source);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSetHandle<T>* Initialize(ReadOnlySpan<T> source)
        {
            NativeFrozenSetHandle<T>* handle;
            if (source.IsEmpty)
            {
                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<EmptyFrozenSet<T>>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<EmptyFrozenSet<T>, T>();
                Unsafe.AsRef<EmptyFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new EmptyFrozenSet<T>();
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<T>())
                {
                    handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<SmallComparableFrozenSet<T>>()), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.AsRef<SmallComparableFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallComparableFrozenSet<T>(source);
                    return handle;
                }

                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<SmallFrozenSet<T>>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<SmallFrozenSet<T>, T>();
                Unsafe.AsRef<SmallFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallFrozenSet<T>(source);
                return handle;
            }

            if (typeof(T) == typeof(int))
            {
                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<Int32FrozenSet>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenSetHandle<int>>(handle) = GetNativeHandle<Int32FrozenSet, int>();
                Unsafe.AsRef<Int32FrozenSet>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new Int32FrozenSet(MemoryMarshal.Cast<T, int>(source));
                return handle;
            }

            handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<DefaultFrozenSet<T>>()), CACHE_LINE_SIZE);
            Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<DefaultFrozenSet<T>, T>();
            Unsafe.AsRef<DefaultFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new DefaultFrozenSet<T>(source);
            return handle;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFrozenSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFrozenSet<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeFrozenSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
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
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue)
        {
            var handle = _handle;
            var index = handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), equalValue);
            if (index >= 0)
            {
                actualValue = new NativeReference<T>(UnsafeHelpers.AsPointer(ref handle->Items(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE))[index]));
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
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     Items
            /// </summary>
            private readonly NativeArray<T> _handle;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<T> handle)
            {
                _handle = handle;
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
                if ((uint)_index < (uint)_handle.Length)
                    return true;
                _index = _handle.Length;
                return false;
            }

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if ((uint)_index >= (uint)_handle.Length)
                        ThrowHelpers.ThrowInvalidOperationException();
                    return _handle[_index];
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}