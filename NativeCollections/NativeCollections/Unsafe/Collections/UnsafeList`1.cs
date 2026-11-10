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
    ///     Represents a strongly typed list of objects that can be accessed
    ///     by index. Provides methods to search, sort, and manipulate lists.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeList<T> : IIsCreated, IDisposable, IEquatable<UnsafeList<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Initializes an instance from its final storage state.
        /// </summary>
        /// <param name="buffer">The allocated element buffer.</param>
        /// <param name="capacity">The buffer capacity.</param>
        /// <param name="count">The number of stored elements.</param>
        /// <param name="version">The current mutation version.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeList(T* buffer, int capacity, int count, int version)
        {
            _buffer = buffer;
            _capacity = capacity;
            _count = count;
            _version = version;
        }

        /// <summary>
        ///     Creates a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>An unsafe list with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeList<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var buffer = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(capacity));
            return new UnsafeList<T>(buffer, capacity, 0, 0);
        }

        /// <summary>
        ///     Gets a reference to the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index));
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeList<T> left, UnsafeList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeList<T> left, UnsafeList<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _version = _version.WrappingAdd(1);
            _count = 0;
        }

        /// <summary>
        ///     Adds the given object to the end of this list. The size of this is increased by
        ///     one. If required, the capacity of this is doubled before adding the new element.
        /// </summary>
        /// <param name="item">The object to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            _version = _version.WrappingAdd(1);
            var size = _count;
            if (U32.FromI32Unchecked(size) < U32.FromI32Unchecked(_capacity))
            {
                _count = size.WrappingAdd(1);
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
            }
            else
            {
                Grow(size.WrappingAdd(1));
                _count = size.WrappingAdd(1);
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
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
        public bool TryAdd(T item)
        {
            var size = _count;
            if (U32.FromI32Unchecked(size) < U32.FromI32Unchecked(_capacity))
            {
                _version = _version.WrappingAdd(1);
                _count = size.WrappingAdd(1);
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <exception cref="OverflowException">
        ///     Thrown when the resulting number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddRange([MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    Grow(_count + count);

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Attempts to add the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <returns>
        ///     <see langword="true" /> if the items were successfully added to this;
        ///     <see langword="false" /> if the remaining capacity is less than the number of items
        ///     and the items could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAddRange(ReadOnlySpan<T> buffer)
        {
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    return false;

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }

            return true;
        }

        /// <summary>
        ///     Inserts an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            if (_count == _capacity)
                Grow(_count.WrappingAdd(1));

            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(1)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Attempts to insert an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsert(int index, T item)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            if (_count == _capacity)
                return false;

            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(1)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Inserts the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the resulting number of elements exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, [MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    Grow(_count + count);

                if (index < _count)
                    SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(count)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Attempts to insert the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <returns>
        ///     <see langword="true" /> if the items were successfully added to this;
        ///     <see langword="false" /> if the remaining capacity is less than the number of items
        ///     and the items could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInsertRange(int index, [MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    return false;

                if (index < _count)
                    SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(count)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }

            return true;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
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
        ///     Searches for the first occurrence of a specific object in this and, if found, removes it
        ///     using a swap-based removal; the search is O(n), while the swap itself is O(1) because
        ///     the last element is moved into the position of the removed element. This operation does
        ///     not preserve the original order of elements.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(T item)
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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            _count = _count.WrappingSub(1);
            if (index < _count)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the element at the specified index using a swap-based removal,
        ///     which is O(1) and does not preserve the order of elements. The last
        ///     element of the list is moved to the position of the removed element.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <remarks>
        ///     This method is more efficient than <see cref="RemoveAt" /> when
        ///     order is not important, as it avoids shifting subsequent elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            _count = _count.WrappingSub(1);
            if (index != _count)
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index)) = Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count));

            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes a range of elements from this.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative, or
        ///     <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            var offset = _count.WrappingSub(index);
            ThrowHelpers.ThrowIfGreaterThan(count, offset, ExceptionArgument.count);
            if (count > 0)
            {
                _count = _count.WrappingSub(count);
                if (index < _count)
                    SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(count)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

                _version = _version.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Reverses the order of the elements in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            AsSpan().Reverse();
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Reverses the order of the elements in this from <paramref name="index" />
        ///     to the end of this.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index)
        {
            AsSpan().Slice(index).Reverse();
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Reverses the order of the elements in this in the range specified by
        ///     <paramref name="index" /> and <paramref name="count" />.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative, or
        ///     <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count)
        {
            AsSpan().Slice(index, count).Reverse();
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Sets the count of this to the specified value.
        /// </summary>
        /// <param name="count">The value to set the count of this to.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
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
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (_capacity < capacity)
                Grow(capacity);

            return _capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = I32.FromF64Unchecked(F64.FromI32Unchecked(_capacity) * 0.9);
            if (_count < threshold)
                SetCapacity(_count);

            return _capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 If <paramref name="capacity" /> is less than <see cref="Count" /> or is greater than
        ///                 or equal to the current capacity, the capacity is left unchanged.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The returned value is the capacity after the call.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
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
        ///     Increases the capacity of this to a new size that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = CollectionHelpers.GrowCapacity(_capacity, capacity);
            SetCapacity(newCapacity);
        }

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item) => AsReadOnlySpan().IndexOf(item);

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item, int index)
        {
            var offset = AsReadOnlySpan().Slice(index).IndexOf(item);
            return offset < 0 ? -1 : offset.WrappingAdd(index);
        }

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the search.</param>
        /// <param name="count">The number of elements to search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative, or
        ///     <paramref name="index" /> and <paramref name="count" /> do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item, int index, int count)
        {
            var offset = AsReadOnlySpan().Slice(index, count).IndexOf(item);
            return offset < 0 ? -1 : offset.WrappingAdd(index);
        }

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(T item) => AsReadOnlySpan().LastIndexOf(item);

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(T item, int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            return AsReadOnlySpan().Slice(0, index.WrappingAdd(1)).LastIndexOf(item);
        }

        /// <summary>
        ///     Determines the last index of a specific element in this.
        /// </summary>
        /// <param name="item">The object to locate in this.</param>
        /// <param name="index">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative, or when
        ///     this is not empty and <paramref name="index" /> and <paramref name="count" />
        ///     do not denote a valid range of elements in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int LastIndexOf(T item, int index, int count)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (_count == 0)
                return -1;

            var start = index.WrappingSub(count).WrappingAdd(1);
            var offset = AsReadOnlySpan().Slice(start, count).LastIndexOf(item);
            return offset < 0 ? -1 : offset.WrappingAdd(start);
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current count.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _count, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var newItems = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(capacity));
                if (_count > 0)
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newItems), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

                NativeMemoryAllocator.AlignedFree(_buffer);
                _buffer = newItems;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _count);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(_buffer), _count);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => Enumerator.From(UnsafeHelpers.AsPointer(ref this));

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
            ///     Used to keep the enumerator synchronized with the collection.
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
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeList<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = 0;
                _current = default;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeList<T>* handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            ///     Thrown when the collection was modified after the enumerator was created.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _handle;
                if (_version == handle->_version && U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                {
                    _current = Unsafe.Add(ref Unsafe.AsRef<T>(handle->_buffer), Isize.FromI32Unchecked(_index));
                    _index = _index.WrappingAdd(1);
                    return true;
                }

                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                _index = handle->_count.WrappingAdd(1);
                _current = default;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}