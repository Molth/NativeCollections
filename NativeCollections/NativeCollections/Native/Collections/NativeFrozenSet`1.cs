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
    ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenSet<T> : IIsCreated, IDisposable, IEquatable<NativeFrozenSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeFrozenSetHandle<T>* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        public ReadOnlySpan<T> Items
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                return handle->Items(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))).AsReadOnlySpan();
            }
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                return handle->Count(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        private NativeFrozenSet(NativeFrozenSetHandle<T>* handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenSet<T> Create(HashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                var index = 0;
                foreach (var item in source)
                    items[(index = index.WrappingAdd(1)).WrappingSub(1)] = item;

                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenSet<T> Create(NativeHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenSet<T> Create(UnsafeHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The set from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenSet<T> Create(StackallocHashSet<T> source)
        {
            using (var items = NativeArray<T>.Create(source.Count))
            {
                source.CopyTo(items.AsSpan());
                return Create(items.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="source">The span of items to initialize from.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSet<T> Create([MustBeDistinct] ReadOnlySpan<T> source) => new(Initialize(source));

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="source">The span of items to initialize from.</param>
        /// <returns>A pointer to the newly allocated frozen set handle.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NativeFrozenSetHandle<T>* Initialize(ReadOnlySpan<T> source)
        {
            NativeFrozenSetHandle<T>* handle;
            if (source.IsEmpty)
            {
                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<EmptyFrozenSet<T>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<EmptyFrozenSet<T>, T>();
                Unsafe.AsRef<EmptyFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = default;
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<T>())
                {
                    handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<SmallComparableFrozenSet<T>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                    Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<SmallComparableFrozenSet<T>, T>();
                    Unsafe.AsRef<SmallComparableFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = SmallComparableFrozenSet<T>.Create(source);
                    return handle;
                }

                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<SmallFrozenSet<T>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<SmallFrozenSet<T>, T>();
                Unsafe.AsRef<SmallFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = SmallFrozenSet<T>.Create(source);
                return handle;
            }

            if (typeof(T) == typeof(int))
            {
                handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<Int32FrozenSet>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenSetHandle<int>>(handle) = GetNativeHandle<Int32FrozenSet, int>();
                Unsafe.AsRef<Int32FrozenSet>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = Int32FrozenSet.Create(MemoryMarshal.Cast<T, int>(source));
                return handle;
            }

            handle = (NativeFrozenSetHandle<T>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<DefaultFrozenSet<T>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
            Unsafe.AsRef<NativeFrozenSetHandle<T>>(handle) = GetNativeHandle<DefaultFrozenSet<T>, T>();
            Unsafe.AsRef<DefaultFrozenSet<T>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = DefaultFrozenSet<T>.Create(source);
            return handle;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeFrozenSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeFrozenSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeFrozenSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeFrozenSet<T> left, NativeFrozenSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;

            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item)
        {
            var handle = _handle;
            return handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), item) >= 0;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(T equalValue, out T actualValue)
        {
            var handle = _handle;
            var index = handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), equalValue);
            if (index >= 0)
            {
                actualValue = handle->Items(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)))[index];
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(T equalValue, out NativePtr<T> actualValue)
        {
            var handle = _handle;
            var index = handle->FindItemIndex(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), equalValue);
            if (index >= 0)
            {
                actualValue = NativePtr<T>.From(UnsafeHelpers.AsPointer(ref handle->Items(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)))[index]));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeArray<T> _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeArray<T> handle)
            {
                _handle = handle;
                _index = -1;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeArray<T> handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                _index = _index.WrappingAdd(1);
                if (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(_handle.Length))
                    return true;

                _index = _handle.Length;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (U32.FromI32Unchecked(_index) >= U32.FromI32Unchecked(_handle.Length))
                        ThrowHelpers.ThrowInvalidOperationException();

                    return _handle[_index];
                }
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}