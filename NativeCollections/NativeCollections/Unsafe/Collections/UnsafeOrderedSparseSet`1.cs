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
    ///     Represents a sparse set mapping integer keys to values that are
    ///     accessible by the key, with O(1) insertion, removal, and lookup.
    ///     The <see cref="OrderedKeys" />, <see cref="OrderedValues" /> and
    ///     <see cref="OrderedKeyValuePairs" /> collections enumerate elements in insertion order,
    ///     whereas <see cref="Keys" /> and <see cref="Values" /> follow the internal dense array order.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeOrderedSparseSet<TValue> : IIsCreated, IDisposable, IEquatable<UnsafeOrderedSparseSet<TValue>>, IReadOnlyCollection<KeyValuePair<int, TValue>> where TValue : unmanaged
    {
        /// <summary>
        ///     The dense array of entries (key-value pairs).
        /// </summary>
        private Entry* _dense;

        /// <summary>
        ///     The sparse array mapping keys to dense indices.
        /// </summary>
        private int* _sparse;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     The index of the head.
        /// </summary>
        private int _head;

        /// <summary>
        ///     The index of the tail.
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a collection containing the keys in the sparse set.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => KeyCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the sparse set.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => ValueCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the key/value pairs in the sparse set, in insertion order.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public OrderedKeyValuePairCollection OrderedKeyValuePairs => OrderedKeyValuePairCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the keys in the sparse set, in insertion order.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public OrderedKeyCollection OrderedKeys => OrderedKeyCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the sparse set, in insertion order.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public OrderedValueCollection OrderedValues => OrderedValueCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws
        ///     a <see cref="KeyNotFoundException" />, and a set operation adds a new element with the specified key
        ///     or overwrites the value of an existing element.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the property is set and <paramref name="key" /> is negative
        ///     or greater than or equal to <see cref="Capacity" />.
        /// </exception>
        public TValue this[int key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                if (TryGetValue(key, out var obj))
                    return obj;

                ThrowHelpers.ThrowKeyNotFoundException(key);
                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Insert(key, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _dense != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the entry with the smallest key in this.
        /// </summary>
        /// <value>The entry with the smallest key in this, or <see langword="null" /> if this is empty.</value>
        public readonly KeyValuePair<int, TValue>? Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var min = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; i = i.WrappingAdd(1))
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(i)).Key;
                        if (key < min)
                        {
                            min = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the entry with the largest key in this.
        /// </summary>
        /// <value>The entry with the largest key in this, or <see langword="null" /> if this is empty.</value>
        public readonly KeyValuePair<int, TValue>? Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var max = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; i = i.WrappingAdd(1))
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(i)).Key;
                        if (key > max)
                        {
                            max = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="dense">The dense array of entries.</param>
        /// <param name="sparse">The sparse array mapping keys to dense indices.</param>
        /// <param name="capacity">The total number of elements the internal data structure can hold.</param>
        /// <param name="head">The index of the head.</param>
        /// <param name="tail">The index of the tail.</param>
        /// <param name="count">The number of elements contained in this.</param>
        /// <param name="version">Used to keep the enumerator synchronized with the collection.</param>
        private UnsafeOrderedSparseSet(Entry* dense, int* sparse, int capacity, int head, int tail, int count, int version)
        {
            _dense = dense;
            _sparse = sparse;
            _capacity = capacity;
            _head = head;
            _tail = tail;
            _count = count;
            _version = version;
        }

        /// <summary>
        ///     Creates an ordered sparse set with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated ordered sparse set.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeOrderedSparseSet<TValue> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
            var denseByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>()), alignment);
            var dense = (Entry*)NativeMemoryAllocator.AlignedAlloc(denseByteCount + Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            var sparse = UnsafeHelpers.AddByteOffset<int>(dense, Isize.FromUsizeUnchecked(denseByteCount));
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(sparse), capacity).Fill(-1);
            return new UnsafeOrderedSparseSet<TValue>(dense, sparse, capacity, -1, -1, 0, 0);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeOrderedSparseSet<TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeOrderedSparseSet<TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeOrderedSparseSet<{0}>", SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeOrderedSparseSet<TValue> left, UnsafeOrderedSparseSet<TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeOrderedSparseSet<TValue> left, UnsafeOrderedSparseSet<TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_dense);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(_sparse), _capacity).Fill(-1);
            _head = -1;
            _tail = -1;
            _count = 0;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Sets the capacity of this, which is the length of its internal arrays.
        ///     Valid keys for this range from 0 to <paramref name="capacity" /> - 1.
        /// </summary>
        /// <param name="capacity">
        ///     The new capacity. It must be at least the current largest key plus one.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current largest key plus one.
        /// </exception>
        public void SetCapacity(int capacity)
        {
            var max = Max;
            var maxKey = max.HasValue ? max.GetValueOrDefault().Key : 0;
            var sparseLength = !max.HasValue ? 0 : maxKey.WrappingAdd(1);
            ThrowHelpers.ThrowIfLessThan(capacity, sparseLength, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
                var denseByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>()), alignment);
                var dense = (Entry*)NativeMemoryAllocator.AlignedAlloc(denseByteCount + Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
                var sparse = UnsafeHelpers.AddByteOffset<int>(dense, Isize.FromUsizeUnchecked(denseByteCount));
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(dense), ref Unsafe.AsRef<byte>(_dense), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>())));
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(sparse), ref Unsafe.AsRef<byte>(_sparse), Usize.FromI32Unchecked(sparseLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));
                if (capacity > sparseLength)
                    MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<int>(sparse), Isize.FromI32Unchecked(sparseLength)), capacity.WrappingSub(sparseLength)).Fill(-1);

                NativeMemoryAllocator.AlignedFree(_dense);
                _dense = dense;
                _sparse = sparse;
                _capacity = capacity;
                _version = _version.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the key/value pair was successfully
        ///     added to this; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="key" /> is negative or greater than or equal to <see cref="Capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _capacity, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index != -1)
                return false;

            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(count));
            entry.Key = key;
            entry.Value = value;
            entry.Next = -1;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(tail)).Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
            count = count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the key/value pair was added to this;
        ///     <see cref="InsertResult.Overwritten" /> if an existing value was replaced.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="key" /> is negative or greater than or equal to <see cref="Capacity" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Insert(int key, TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _capacity, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value = value;
                _version = _version.WrappingAdd(1);
                return InsertResult.Overwritten;
            }

            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(count));
            entry.Key = key;
            entry.Value = value;
            entry.Next = -1;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = count;
            ref var tail = ref _tail;
            if (tail != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(tail)).Next = count;
                entry.Previous = tail;
            }
            else
            {
                _head = count;
                entry.Previous = -1;
            }

            tail = count;
            count = count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return InsertResult.Success;
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
        public bool Remove(int key)
        {
            if (U32.FromI32Unchecked(key) >= U32.FromI32Unchecked(_capacity))
                return false;

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index == -1)
                return false;

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
            return true;
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
        public bool Remove(int key, out TValue value)
        {
            if (U32.FromI32Unchecked(key) >= U32.FromI32Unchecked(_capacity))
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index == -1)
            {
                value = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            value = entry.Value;
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
            return true;
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
        public readonly bool ContainsKey(int key) => key >= 0 && key < _capacity && Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) != -1;

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
        public readonly bool TryGetValue(int key, out TValue value)
        {
            if (U32.FromI32Unchecked(key) >= U32.FromI32Unchecked(_capacity))
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index != -1)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets a reference to the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains a reference to the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(int key, out NativePtr<TValue> value)
        {
            if (U32.FromI32Unchecked(key) >= U32.FromI32Unchecked(_capacity))
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));
            if (index != -1)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref entry.Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The index of <paramref name="key" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(int key) => U32.FromI32Unchecked(key) >= U32.FromI32Unchecked(_capacity) ? -1 : Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key));

        /// <summary>
        ///     Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index in the dense array.</param>
        /// <returns>The key at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetKeyAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Key;
        }

        /// <summary>
        ///     Gets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index in the dense array.</param>
        /// <returns>A reference to the value at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value;
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
        public readonly bool TryGetKeyAt(int index, out int key)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Key;
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

            value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value;
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

            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value));
            return true;
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index in the dense array.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, TValue> GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)));
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index in the dense array.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, NativePtr<TValue>> GetReferenceAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            return new KeyValuePair<int, NativePtr<TValue>>(entry.Key, NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref entry.Value)));
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
        public readonly bool TryGetAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)));
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
        public readonly bool TryGetReferenceAt(int index, out KeyValuePair<int, NativePtr<TValue>> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            keyValuePair = new KeyValuePair<int, NativePtr<TValue>>(entry.Key, NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref entry.Value)));
            return true;
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
        public void SetAt(int index, TValue value)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index)).Value = value;
            _version = _version.WrappingAdd(1);
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
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            var key = entry.Key;
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">The removed key/value pair at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the item is successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
                return false;

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            var key = entry.Key;
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the removed key/value pair at the specified index
        ///     if the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="keyValuePair" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the item is successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(index));
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            if (entry.Next != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = entry.Previous;
            else
                _tail = entry.Previous;

            if (entry.Previous != -1)
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = entry.Next;
            else
                _head = entry.Next;

            _count = _count.WrappingSub(1);
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(_count));
                entry = lastEntry;
                if (entry.Next != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Next)).Previous = index;
                else
                    _tail = index;

                if (entry.Previous != -1)
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), Isize.FromI32Unchecked(entry.Previous)).Next = index;
                else
                    _head = index;

                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(lastEntry.Key)) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), Isize.FromI32Unchecked(key)) = -1;
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Represents an entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     Gets the key of the entry.
            /// </summary>
            public int Key;

            /// <summary>
            ///     Gets the value of the entry.
            /// </summary>
            public TValue Value;

            /// <summary>
            ///     The index of the next entry.
            /// </summary>
            public int Next;

            /// <summary>
            ///     The index of the previous entry.
            /// </summary>
            public int Previous;
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
        readonly IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
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
        public struct Enumerator : IIterator<KeyValuePair<int, TValue>>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Used to keep the enumerator synchronized with the collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -1;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                var num = _index.WrappingAdd(1);
                if (num >= handle->_count)
                    return false;

                _index = num;
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly KeyValuePair<int, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_handle->_dense), Isize.FromI32Unchecked(_index)));
            }
        }

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<int>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private KeyCollection(UnsafeOrderedSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Gets the key at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the key to get.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
            /// </exception>
            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), Isize.FromI32Unchecked(index)).Key;
                }
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
            IEnumerator<int> IEnumerable<int>.GetEnumerator()
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
            public struct Enumerator : IIterator<int>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly UnsafeOrderedSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                    var num = _index.WrappingAdd(1);
                    if (num >= handle->_count)
                        return false;

                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => _index = -1;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_handle->_dense), Isize.FromI32Unchecked(_index)).Key;
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
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueCollection(UnsafeOrderedSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Gets a reference to the value at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the value in the collection.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
            /// </exception>
            public ref TValue this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), Isize.FromI32Unchecked(index)).Value;
                }
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
                private readonly UnsafeOrderedSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                    var num = _index.WrappingAdd(1);
                    if (num >= handle->_count)
                        return false;

                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => _index = -1;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_handle->_dense), Isize.FromI32Unchecked(_index)).Value;
                }
            }
        }

        /// <summary>
        ///     Represents the collection of key/value pairs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyValuePairCollection : IIsCreated, IReadOnlyCollection<KeyValuePair<int, TValue>>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OrderedKeyValuePairCollection(UnsafeOrderedSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static OrderedKeyValuePairCollection From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            public int CopyTo(Span<KeyValuePair<int, TValue>> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), new KeyValuePair<int, TValue>(entry.Key, entry.Value));
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<int, TValue>>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            public void CopyTo(Span<KeyValuePair<int, TValue>> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < _handle->_count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), new KeyValuePair<int, TValue>(entry.Key, entry.Value));
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<int, TValue>>(buffer));

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
            IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
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
            public struct Enumerator : IIterator<KeyValuePair<int, TValue>>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly UnsafeOrderedSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     The entry at the current position of the enumerator.
                /// </summary>
                private Entry* _current;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                    var num = _index.WrappingAdd(1);
                    if (num >= handle->_count)
                        return false;

                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(_current->Next));

                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    var handle = _handle;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly KeyValuePair<int, TValue> Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.AsRef<KeyValuePair<int, TValue>>(_current);
                }
            }
        }

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedKeyCollection : IIsCreated, IReadOnlyCollection<int>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OrderedKeyCollection(UnsafeOrderedSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static OrderedKeyCollection From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            public int CopyTo(Span<int> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), entry.Key);
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, int>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            public void CopyTo(Span<int> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < _handle->_count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), entry.Key);
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, int>(buffer));

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
            IEnumerator<int> IEnumerable<int>.GetEnumerator()
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
            public struct Enumerator : IIterator<int>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly UnsafeOrderedSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     The entry at the current position of the enumerator.
                /// </summary>
                private Entry* _current;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                    var num = _index.WrappingAdd(1);
                    if (num >= handle->_count)
                        return false;

                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(_current->Next));

                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    var handle = _handle;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Key;
                }
            }
        }

        /// <summary>
        ///     Represents the collection of values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct OrderedValueCollection : IIsCreated, IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeOrderedSparseSet<TValue>* _handle;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private OrderedValueCollection(UnsafeOrderedSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static OrderedValueCollection From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), entry.Value);
                    current = entry.Next;
                }

                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this. The actual number of copied elements is
            ///     limited by the span's length, the specified count, and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from this.</returns>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="count" /> is negative.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var dense = _handle->_dense;
                var current = _handle->_head;
                for (var index = 0; index < _handle->_count; index = index.WrappingAdd(1))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(dense), Isize.FromI32Unchecked(current));
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), entry.Value);
                    current = entry.Next;
                }
            }

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

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
                private readonly UnsafeOrderedSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     The entry at the current position of the enumerator.
                /// </summary>
                private Entry* _current;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeOrderedSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeOrderedSparseSet<TValue>* handle) => new(handle);

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
                    var num = _index.WrappingAdd(1);
                    if (num >= handle->_count)
                        return false;

                    _index = num;
                    if (num != 0)
                        _current = UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(_current->Next));

                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    var handle = _handle;
                    _index = -1;
                    _current = handle->_head != -1 ? UnsafeHelpers.Add<Entry>(handle->_dense, Isize.FromI32Unchecked(handle->_head)) : null;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current->Value;
                }
            }
        }
    }
}