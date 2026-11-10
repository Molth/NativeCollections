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
    ///     Represents a collection of key/value pairs that are
    ///     sorted by the keys and are accessible by key and by index.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedList<TKey, TValue> : IIsCreated, IEquatable<StackallocSortedList<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Gets a collection containing the keys in the sorted list.
        /// </summary>
        private readonly TKey* _keys;

        /// <summary>
        ///     Gets a collection containing the values in the sorted list.
        /// </summary>
        private readonly TValue* _values;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets a collection containing the keys in the sorted list.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => KeyCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the sorted list.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => ValueCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _keys != null;

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
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
            var keysByteCount = NativeMemoryAllocator.AlignUp(nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>()), alignment);
            var byteCount = keysByteCount + nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>()) + alignment.WrappingSub(Usize.FromI32Unchecked(1));
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
        ///     A sorted list that uses <paramref name="buffer" /> as its
        ///     storage and holds up to <paramref name="capacity" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static StackallocSortedList<TKey, TValue> FromBytes([MustBePinned] Span<byte> buffer, int capacity) => new(buffer, capacity);

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
        private StackallocSortedList([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
            var keysByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>()), alignment);
            _keys = (TKey*)NativeArray<byte>.FromBytes(buffer, alignment).Buffer;
            _values = UnsafeHelpers.AddByteOffset<TValue>(_keys, Isize.FromUsizeUnchecked(keysByteCount));
            _count = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(StackallocSortedList<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocSortedList<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("StackallocSortedList<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocSortedList<TKey, TValue> left, StackallocSortedList<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(StackallocSortedList<TKey, TValue> left, StackallocSortedList<TKey, TValue> right) => !left.Equals(right);

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
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>The index of <paramref name="key" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(TKey key)
        {
            var num = Search(key);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Searches for the specified key using binary search.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///     The index of the key if found; otherwise, the bitwise complement of the insertion point.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int Search(TKey key) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TKey>(_keys), _count).BinarySearch(key);

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the key/value pair was added to this;
        ///     <see cref="InsertResult.AlreadyExists" /> if the key is already present;
        ///     <see cref="InsertResult.InsufficientCapacity" /> if this has no available capacity.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(TKey key, TValue value)
        {
            var num = Search(key);
            return num >= 0 ? InsertResult.AlreadyExists : Insert(~num, key, value);
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add or update.</param>
        /// <param name="value">The value of the element to add or update.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the key/value pair was added to this;
        ///     <see cref="InsertResult.Overwritten" /> if an existing value was replaced;
        ///     <see cref="InsertResult.InsufficientCapacity" /> if this has no available capacity for a new key.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddOrUpdate(TKey key, TValue value)
        {
            var num = Search(key);
            if (num >= 0)
            {
                Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(num)) = value;
                _version = _version.WrappingAdd(1);
                return InsertResult.Overwritten;
            }

            return Insert(~num, key, value);
        }

        /// <summary>
        ///     Removes the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and
        ///     removed; otherwise, <see langword="false" />. This method returns
        ///     <see langword="false" /> if <paramref name="key" /> is not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                _count = _count.WrappingSub(1);
                if (index < _count)
                {
                    SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                    SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
                }

                _version = _version.WrappingAdd(1);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes the value with the specified key from this, and
        ///     copies the element to the <paramref name="value" /> parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     found and removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index));
                _count = _count.WrappingSub(1);
                if (index < _count)
                {
                    SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                    SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
                }

                _version = _version.WrappingAdd(1);
                return true;
            }

            value = default;
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
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            _count = _count.WrappingSub(1);
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">The removed element.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)));
            _count = _count.WrappingSub(1);
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     found and removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
                return false;

            _count = _count.WrappingSub(1);
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item is successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)));
            _count = _count.WrappingSub(1);
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            _version = _version.WrappingAdd(1);
            return true;
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
            if (count == 0)
            {
                ThrowHelpers.ThrowIfGreaterThan(index, _count, ExceptionArgument.index);
                return;
            }

            ThrowHelpers.ThrowIfGreaterThan(count, _count.WrappingSub(index), ExceptionArgument.count);
            _count = _count.WrappingSub(count);
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(count)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(count)))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>The key at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TKey GetKeyAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index));
        }

        /// <summary>
        ///     Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>A reference to the value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index));
        }

        /// <summary>
        ///     Sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueAt(int index, TValue value)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)) = value;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsKey(TKey key) => IndexOf(key) >= 0;

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(TKey key, out NativePtr<TValue> value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="key">
        ///     When this method returns, contains the key at the specified index if
        ///     the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="key" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetKeyAt(int index, out TKey key)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index));
            return true;
        }

        /// <summary>
        ///     Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value at the specified index if
        ///     the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueAt(int index, out TValue value)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                value = default;
                return false;
            }

            value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index));
            return true;
        }

        /// <summary>
        ///     Gets a reference to the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="value">
        ///     When this method returns, contains a reference to the value at the specified
        ///     index if the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReferenceAt(int index, out NativePtr<TValue> value)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                value = default;
                return false;
            }

            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))));
            return true;
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<TKey, TValue> GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)));
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<TKey, NativePtr<TValue>> GetReferenceAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return new KeyValuePair<TKey, NativePtr<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)))));
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the key/value pair at the specified index
        ///     if the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="keyValuePair" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)));
            return true;
        }

        /// <summary>
        ///     Gets the key/value pair and a reference to its value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the key/value pair and a reference to its value at
        ///     the specified index if the index is valid; otherwise, the default value for the type of
        ///     the <paramref name="keyValuePair" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetReferenceAt(int index, out KeyValuePair<TKey, NativePtr<TValue>> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, NativePtr<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)), NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)))));
            return true;
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the entry was inserted; otherwise,
        ///     <see cref="InsertResult.InsufficientCapacity" /> if the list is full.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult Insert(int index, TKey key, TValue value)
        {
            if (_count == _capacity)
                return InsertResult.InsufficientCapacity;

            if (index < _count)
            {
                SpanHelpers.Move(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index.WrappingAdd(1)))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>())));
                SpanHelpers.Move(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index.WrappingAdd(1)))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index))), Usize.FromI32Unchecked(_count.WrappingSub(index)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>())));
            }

            Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), Isize.FromI32Unchecked(index)) = key;
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), Isize.FromI32Unchecked(index)) = value;
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
        }

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
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
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
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(StackallocSortedList<TKey, TValue>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _current = default;
                _index = 0;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(StackallocSortedList<TKey, TValue>* handle) => new(handle);

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
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                {
                    _current = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), Isize.FromI32Unchecked(_index)), Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), Isize.FromI32Unchecked(_index)));
                    _index = _index.WrappingAdd(1);
                    return true;
                }

                _index = handle->_count.WrappingAdd(1);
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _current = default;
                _index = 0;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private KeyCollection(StackallocSortedList<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(StackallocSortedList<TKey, TValue>* handle) => new(handle);

            /// <summary>
            ///     Returns a read-only span over the entire contents of this.
            /// </summary>
            /// <returns>A read-only span over the entire contents of this.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TKey>(handle->_keys), handle->_count);
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
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

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TKey>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly StackallocSortedList<TKey, TValue>* _handle;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TKey _current;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(StackallocSortedList<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _current = default;
                    _index = 0;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(StackallocSortedList<TKey, TValue>* handle) => new(handle);

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
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    if (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                    {
                        _current = Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), Isize.FromI32Unchecked(_index));
                        _index = _index.WrappingAdd(1);
                        return true;
                    }

                    _index = handle->_count.WrappingAdd(1);
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _current = default;
                    _index = 0;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }

        /// <summary>
        ///     Represents the collection of values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IIsCreated, IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly StackallocSortedList<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueCollection(StackallocSortedList<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(StackallocSortedList<TKey, TValue>* handle) => new(handle);

            /// <summary>
            ///     Returns a span over the entire contents of this.
            /// </summary>
            /// <returns>A span over the entire contents of this.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _handle;
                return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<TValue>(handle->_values), handle->_count);
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
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

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly StackallocSortedList<TKey, TValue>* _handle;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TValue _current;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(StackallocSortedList<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _current = default;
                    _index = 0;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(StackallocSortedList<TKey, TValue>* handle) => new(handle);

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
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    if (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                    {
                        _current = Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), Isize.FromI32Unchecked(_index));
                        _index = _index.WrappingAdd(1);
                        return true;
                    }

                    _index = handle->_count.WrappingAdd(1);
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _current = default;
                    _index = 0;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}