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
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocList<T> : IIsCreated, IEquatable<StackallocList<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

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
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of
        ///     elements, taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var nativeCapacity = Usize.FromI32Unchecked(capacity);
            var byteCount = nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()) + NativeMemoryAllocator.AlignOf<T>().WrappingSub(Usize.FromI32Unchecked(1));
            return I32.FromUsize(byteCount);
        }

        /// <summary>
        ///     Initializes a new instance of this class that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage. It must be large enough
        ///     to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the list can hold. Must be non-negative.
        /// </param>
        /// <returns>
        ///     A list that uses <paramref name="buffer" /> as its storage
        ///     and holds up to <paramref name="capacity" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StackallocList<T> FromBytes([MustBePinned] Span<byte> buffer, int capacity) => new(buffer, capacity);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the list can hold.
        /// </param>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        private StackallocList([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            _buffer = NativeArray<T>.FromBytes(buffer).Buffer;
            _capacity = capacity;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(StackallocList<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocList<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("StackallocList<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocList<T> left, StackallocList<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(StackallocList<T> left, StackallocList<T> right) => !left.Equals(right);

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
        ///     Attempts to add an object to the end of the list.
        /// </summary>
        /// <param name="item">The object to add.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the item was successfully
        ///     added to the list; <see cref="InsertResult.InsufficientCapacity" />
        ///     if the list is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(T item)
        {
            var size = _count;
            if (U32.FromI32Unchecked(size) < U32.FromI32Unchecked(_capacity))
            {
                _version = _version.WrappingAdd(1);
                _count = size.WrappingAdd(1);
                Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(size)) = item;
                return InsertResult.Success;
            }

            return InsertResult.InsufficientCapacity;
        }

        /// <summary>
        ///     Attempts to add the elements of the specified collection to the end of this.
        /// </summary>
        /// <param name="buffer">The collection whose elements should be added to the end of this.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the items were successfully
        ///     added to the list; <see cref="InsertResult.InsufficientCapacity" />
        ///     if the list is already full and the items could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddRange(ReadOnlySpan<T> buffer)
        {
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    return InsertResult.InsufficientCapacity;

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(_count))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }

            return InsertResult.Success;
        }

        /// <summary>
        ///     Attempts to insert an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the item was successfully
        ///     added to the list; <see cref="InsertResult.InsufficientCapacity" />
        ///     if the list is already full and the item could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsert(int index, T item)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            if (_count == _capacity)
                return InsertResult.InsufficientCapacity;

            if (index < _count)
                SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(1)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

            Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index)) = item;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
        }

        /// <summary>
        ///     Attempts to insert the elements of a collection into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="buffer">The collection whose elements should be inserted into this.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the item was successfully
        ///     added to the list; <see cref="InsertResult.InsufficientCapacity" />
        ///     if the list is already full and the item could not be added.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsertRange(int index, [MustNotSelfAlias] ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            var count = buffer.Length;
            if (count > 0)
            {
                if (_capacity.WrappingSub(_count) < count)
                    return InsertResult.InsufficientCapacity;

                if (index < _count)
                    SpanHelpers.Move(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index.WrappingAdd(count)))), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));

                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), Isize.FromI32Unchecked(index))), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
                _count = _count.WrappingAdd(count);
                _version = _version.WrappingAdd(1);
            }

            return InsertResult.Success;
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
        ///     Removes the first occurrence of a specific object from this using a swap-based removal,
        ///     which maintains O(1) time complexity by moving the last element into the position of
        ///     the removed element. This operation does not preserve the original order of elements.
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
        ///     Thrown when <paramref name="index" /> or <paramref name="count" /> is negative,
        ///     or <paramref name="index" /> and <paramref name="count" />
        ///     do not denote a valid range of elements in this.
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
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            AsSpan().Reverse();
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Reverses the sequence of the elements in the specified span.
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
        ///     Reverses the sequence of the elements in the specified span.
        /// </summary>
        /// <param name="index">The zero-based starting index.</param>
        /// <param name="count">The number of elements.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
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
        /// <param name="count">The value to set the list's count to.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the count was set;
        ///     <see cref="InsertResult.InsufficientCapacity" /> if <paramref name="count" /> exceeds the capacity.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        /// <remarks>
        ///     When increasing the count, uninitialized data is being exposed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TrySetCount(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (_capacity < count)
                return InsertResult.InsufficientCapacity;

            _count = count;
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
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
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
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
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
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
        ///     Thrown when <paramref name="index" /> or the end index
        ///     is not in range (&lt;0 or &gt;<see cref="Count" />).
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
        ///     Returns a span over a portion of this.
        /// </summary>
        /// <returns>A span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(_buffer), _count);

        /// <summary>
        ///     Returns a read-only span over a portion of this.
        /// </summary>
        /// <returns>A read-only span over a portion of this.</returns>
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
            private readonly StackallocList<T>* _handle;

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
            private Enumerator(StackallocList<T>* handle)
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
            internal static Enumerator From(StackallocList<T>* handle) => new(handle);

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