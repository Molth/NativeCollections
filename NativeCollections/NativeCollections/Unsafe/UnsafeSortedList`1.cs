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
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeSortedList<T> : IIsCreated, IDisposable, IEquatable<UnsafeSortedList<T>>, IReadOnlyCollection<T> where T : unmanaged, IComparable<T>
    {
        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        private T* _items;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _size;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_items);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeSortedList(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _items = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
            _size = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeSortedList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeSortedList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeSortedList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeSortedList<T> left, UnsafeSortedList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeSortedList<T> left, UnsafeSortedList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_items);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ++_version;
            _size = 0;
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item)
        {
            var num = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_items), _size).BinarySearch(item);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Adds the given object to the end of this list. The size of the list is
        ///     increased by one. If required, the capacity of the list is doubled
        ///     before adding the new element.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            var num = IndexOf(item);
            if (num >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(item);
            Insert(~num, item);
        }

        /// <summary>
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the list;
        ///     <see langword="false" /> if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item)
        {
            var num = IndexOf(item);
            if (num >= 0)
                return false;
            Insert(~num, item);
            return true;
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
                --_size;
                if (index < _size)
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _size, ExceptionArgument.index);
            --_size;
            if (index < _size)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _size, ExceptionArgument.index);
            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            --_size;
            if (index < _size)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                return false;
            --_size;
            if (index < _size)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
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
            if ((uint)index >= (uint)_size)
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            --_size;
            if (index < _size)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
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
            ThrowHelpers.ThrowIfGreaterThan(index + count, _size, ExceptionArgument.count);
            _size -= count;
            if (index < _size)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + count))), (uint)((_size - index) * Unsafe.SizeOf<T>()));
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _size, ExceptionArgument.index);
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
            if ((uint)index >= (uint)_size)
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index);
            return true;
        }

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (_capacity < capacity)
            {
                var newCapacity = 2 * _capacity;
                if ((uint)newCapacity > ArrayHelpers.MaxLength)
                    newCapacity = ArrayHelpers.MaxLength;
                var expected = _capacity + 4;
                newCapacity = Math.Max(newCapacity, expected);
                newCapacity = Math.Max(newCapacity, capacity);
                SetCapacity(newCapacity);
            }

            return _capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_capacity * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _size || capacity >= _capacity)
                return _capacity;
            SetCapacity(capacity);
            return _capacity;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _size, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var items = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
                if (_size > 0)
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(items), ref Unsafe.AsRef<byte>(_items), (uint)(_size * Unsafe.SizeOf<T>()));
                NativeMemoryAllocator.AlignedFree(_items);
                _items = items;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_items), _size);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)start), _size - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnsafeSortedList<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, in T item)
        {
            if (_size == _capacity)
                EnsureCapacity(_size + 1);
            if (index < _size)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)(index + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index)), (uint)((_size - index) * Unsafe.SizeOf<T>()));
            Unsafe.Add(ref Unsafe.AsRef<T>(_items), (nint)index) = item;
            ++_size;
            ++_version;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSortedList<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly UnsafeSortedList<T>* _handle;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeSortedList<T>* handle)
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
                if ((uint)_index < (uint)handle->_size)
                {
                    _current = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_items), (nint)_index);
                    ++_index;
                    return true;
                }

                _index = handle->_size + 1;
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