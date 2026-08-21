using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a collection of items that are sorted by the items
    ///     and are accessible by item and by index.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedList<T> : IIsCreated, IEquatable<StackallocSortedList<T>>, IReadOnlyCollection<T> where T : unmanaged, IComparable<T>
    {
        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        private readonly T* _items;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_items);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of elements,
        ///     taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            return capacity * Unsafe.SizeOf<T>() + (int)NativeMemoryAllocator.AlignOf<T>() - 1;
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        ///     It must be large enough to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the stack can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="capacity" /> is negative, or if <paramref name="buffer" /> is too small
        ///     to hold the required number of elements (including alignment padding).
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSortedList([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            _items = NativeArray<T>.Create(buffer).Buffer;
            _count = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocSortedList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocSortedList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocSortedList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocSortedList<T> left, StackallocSortedList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocSortedList<T> left, StackallocSortedList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ++_version;
            _count = 0;
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item)
        {
            var num = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_items), _count).BinarySearch(item);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        /// <param name="item">The object to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in T item)
        {
            var num = IndexOf(item);
            return num >= 0 ? InsertResult.AlreadyExists : Insert(~num, item);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                --_count;
                if (index < _count)
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
                ++_version;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            --_count;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="item">The removed element.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out T item)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            --_count;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_count)
                return false;
            --_count;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="item">The removed element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out T item)
        {
            if ((uint)index >= (uint)_count)
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            --_count;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes a range of elements from this.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="count" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (count == 0)
                return;
            ThrowHelpers.ThrowIfGreaterThan(index + count, _count, ExceptionArgument.count);
            _count -= count;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + count))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            ++_version;
        }

        /// <summary>
        ///     Get item at index
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
        }

        /// <summary>
        ///     Contains item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="item">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="item" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out T item)
        {
            if ((uint)index >= (uint)_count)
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            return true;
        }

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_items), _count);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)start), _count - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(StackallocSortedList<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult Insert(int index, in T item)
        {
            if (_count == _capacity)
                return InsertResult.InsufficientCapacity;
            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index) = item;
            ++_count;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static StackallocSortedList<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
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
            private readonly StackallocSortedList<T>* _handle;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocSortedList<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _current = default;
                _index = 0;
            }

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
                var handle = _handle;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if ((uint)_index < (uint)handle->_count)
                {
                    _current = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_items), (nint)_index);
                    ++_index;
                    return true;
                }

                _index = handle->_count + 1;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _current = default;
                _index = 0;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}