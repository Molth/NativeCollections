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
    ///     Represents a strongly typed list of objects that can be accessed by index.
    ///     Provides methods to search, sort, and manipulate lists.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeList<T> : IIsCreated, IDisposable, IEquatable<UnsafeList<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private T* _buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

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
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeList(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _buffer = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
            _capacity = capacity;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeList<T> left, UnsafeList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeList<T> left, UnsafeList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _version++;
            _count = 0;
        }

        /// <summary>
        ///     Adds the given object to the end of this list. The size of this is
        ///     increased by one. If required, the capacity of this is doubled
        ///     before adding the new element.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            _version++;
            var size = _count;
            if ((uint)size < (uint)_capacity)
            {
                _count = size + 1;
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size) = item;
            }
            else
            {
                Grow(size + 1);
                _count = size + 1;
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size) = item;
            }
        }

        /// <summary>
        ///     Attempts to add an object to the end of this.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item)
        {
            var size = _count;
            if ((uint)size < (uint)_capacity)
            {
                _version++;
                _count = size + 1;
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)size) = item;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange(ReadOnlySpan<T> buffer)
        {
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity - _count < count)
                    Grow(checked(_count + count));
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_count)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(count * Unsafe.SizeOf<T>()));
                _count += count;
                _version++;
            }
        }

        /// <summary>
        ///     Attempts to add the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <returns>
        ///     <see langword="true" /> if the items were successfully added to this;
        ///     <see langword="false" /> if this is already full and the items could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAddRange(ReadOnlySpan<T> buffer)
        {
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity - _count < count)
                    return false;
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_count)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(count * Unsafe.SizeOf<T>()));
                _count += count;
                _version++;
            }

            return true;
        }

        /// <summary>
        ///     Inserts an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            if (_count == _capacity)
                Grow(_count + 1);
            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index) = item;
            _count++;
            _version++;
        }

        /// <summary>
        ///     Attempts to insert an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsert(int index, in T item)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            if (_count == _capacity)
                return false;
            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + 1))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index) = item;
            _count++;
            _version++;
            return true;
        }

        /// <summary>
        ///     Inserts the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity - _count < count)
                    Grow(checked(_count + count));
                if (index < _count)
                    SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + count))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<T>()));
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(count * Unsafe.SizeOf<T>()));
                _count += count;
                _version++;
            }
        }

        /// <summary>
        ///     Attempts to insert the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0. -or-
        ///     <paramref name="index" /> is greater than <see cref="Count" />.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsertRange(int index, ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity - _count < count)
                    return false;
                if (index < _count)
                    SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + count))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<T>()));
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), (uint)(count * Unsafe.SizeOf<T>()));
                _count += count;
                _version++;
            }

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
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from this using a swap-based removal,
        ///     which maintains O(1) time complexity by moving the last element into the position of the removed element.
        ///     This operation does not preserve the original order of elements.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />. This method also returns <see langword="false" /> if
        ///     <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(in T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                SwapRemoveAt(index);
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            _count--;
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
            _version++;
        }

        /// <summary>
        ///     Removes the element at the specified index using a swap-based removal,
        ///     which is O(1) and does not preserve the order of elements.
        ///     The last element of the list is moved to the position of the removed element.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <remarks>
        ///     This method is more efficient than <see cref="RemoveAt" /> when order is not important,
        ///     as it avoids shifting subsequent elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            _count--;
            if (index != _count)
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index) = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)_count);
            _version++;
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
            var offset = _count - index;
            ThrowHelpers.ThrowIfGreaterThan(count, offset, ExceptionArgument.count);
            if (count > 0)
            {
                _count -= count;
                if (index < _count)
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index)), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)(index + count))), (uint)((_count - index) * Unsafe.SizeOf<T>()));
                _version++;
            }
        }

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            AsSpan().Reverse();
            _version++;
        }

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index)
        {
            AsSpan().Slice(index).Reverse();
            _version++;
        }

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count)
        {
            AsSpan().Slice(index, count).Reverse();
            _version++;
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>true if this contains the specified element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Sets the count of this to the specified value.
        /// </summary>
        /// <param name="count">The value to set this's count to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="count" /> is negative.
        /// </exception>
        /// <remarks>
        ///     When increasing the count, uninitialized data is being exposed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCount(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (_capacity < count)
                Grow(count);
            _count = count;
            _version++;
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (_capacity < capacity)
                Grow(capacity);
            return _capacity;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_capacity * 0.9);
            if (_count < threshold)
                SetCapacity(_count);
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
            if (capacity < _count || capacity >= _capacity)
                return _capacity;
            SetCapacity(capacity);
            return _capacity;
        }

        /// <summary>
        ///     Increases the capacity of this to a new size
        ///     that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = CollectionHelpers.EnsureCapacity(_capacity, capacity);
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item) => AsReadOnlySpan().IndexOf(item);

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item, int index) => AsReadOnlySpan().Slice(index).IndexOf(item);

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item, int index, int count) => AsReadOnlySpan().Slice(index, count).IndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(in T item) => AsReadOnlySpan().LastIndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(in T item, int index) => AsReadOnlySpan().Slice(index).LastIndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified <paramref name="index" /> or end index is not in range (&lt;0 or &gt;Count).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(in T item, int index, int count) => AsReadOnlySpan().Slice(index, count).LastIndexOf(item);

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _count, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var newItems = NativeMemoryAllocator.AlignedAlloc<T>((uint)capacity);
                if (_count > 0)
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newItems), ref Unsafe.AsRef<byte>(_buffer), (uint)(_count * Unsafe.SizeOf<T>()));
                NativeMemoryAllocator.AlignedFree(_buffer);
                _buffer = newItems;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _count);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _count - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _count);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), _count - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)start), length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(UnsafeList<T> value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(UnsafeList<T> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeList<T> Empty => default;

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
            private readonly UnsafeList<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeList<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = 0;
                _current = default;
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
                if (_version == handle->_version && (uint)_index < (uint)handle->_count)
                {
                    _current = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), (nint)_index);
                    _index++;
                    return true;
                }

                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                _index = handle->_count + 1;
                _current = default;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = 0;
                _current = default;
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