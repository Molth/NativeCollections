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
    ///     Stackalloc sparseSet
    /// </summary>
    /// <remarks>
    ///     https://github.com/bombela/sparseset
    /// </remarks>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Community | FromType.Rust)]
    public unsafe struct StackallocSparseSet<TValue> : IIsCreated, IEquatable<StackallocSparseSet<TValue>>, IReadOnlyCollection<KeyValuePair<int, TValue>> where TValue : unmanaged
    {
        /// <summary>
        ///     Dense
        /// </summary>
        private readonly Entry* _dense;

        /// <summary>
        ///     Sparse
        /// </summary>
        private readonly int* _sparse;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws a
        ///     <see cref="KeyNotFoundException" />, and a set operation creates a new element with the specified key.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     The property is retrieved and <paramref name="key" /> does not exist in the collection.
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
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_dense);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the minimum value in this.
        /// </summary>
        /// <returns>The minimum value in the set.</returns>
        public readonly KeyValuePair<int, TValue>? Min
        {
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var min = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; ++i)
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)i).Key;
                        if (key < min)
                        {
                            min = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

        /// <summary>
        ///     Gets the maximum value in this.
        /// </summary>
        /// <returns>The maximum value in the set.</returns>
        public readonly KeyValuePair<int, TValue>? Max
        {
            get
            {
                if (_count > 0)
                {
                    var index = 0;
                    var max = Unsafe.AsRef<Entry>(_dense).Key;
                    for (var i = 1; i < _count; ++i)
                    {
                        var key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)i).Key;
                        if (key > max)
                        {
                            max = key;
                            index = i;
                        }
                    }

                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                    return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
                }

                return null;
            }
        }

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
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
            var denseByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<Entry>()), alignment);
            return (int)(denseByteCount + capacity * Unsafe.SizeOf<int>() + alignment - 1);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSparseSet([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<Entry>(), NativeMemoryAllocator.AlignOf<int>());
            var denseByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<Entry>()), alignment);
            _dense = (Entry*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _sparse = UnsafeHelpers.AddByteOffset<int>(_dense, (nint)denseByteCount);
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(_sparse), capacity).Fill(-1);
            _length = capacity;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocSparseSet<TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocSparseSet<TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocSparseSet<{0}>", SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocSparseSet<TValue> left, StackallocSparseSet<TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocSparseSet<TValue> left, StackallocSparseSet<TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            MemoryMarshal.CreateSpan(ref Unsafe.AsRef<int>(_sparse), _length).Fill(-1);
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the key/value pair was added to this successfully;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int key, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _length, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
                return false;
            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)count);
            entry.Key = key;
            entry.Value = value;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = count;
            ++count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already
        ///     exist, or updates a key/value pair in this if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Insert(int key, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(key, ExceptionArgument.key);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(key, _length, ExceptionArgument.key);
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value = value;
                ++_version;
                return InsertResult.Overwritten;
            }

            ref var count = ref _count;
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)count);
            entry.Key = key;
            entry.Value = value;
            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = count;
            ++count;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Removes the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is
        ///     not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key)
        {
            if ((uint)key >= (uint)_length)
                return false;
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index == -1)
                return false;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index) = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the value with the specified key from this,
        ///     and copies the element to the <paramref name="value" /> parameter.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out TValue value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index == -1)
            {
                value = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            value = entry.Value;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool ContainsKey(int key) => key >= 0 && key < _length && Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) != -1;

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(int key, out TValue value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value;
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
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(int key, out NativePtr<TValue> value)
        {
            if ((uint)key >= (uint)_length)
            {
                value = default;
                return false;
            }

            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);
            if (index != -1)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref entry.Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="key" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(int key) => (uint)key >= (uint)_length ? -1 : Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key);

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetKeyAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Key;
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            return ref entry.Value;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="index">The key of the value to get.</param>
        /// <param name="key">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="key" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetKeyAt(int index, out int key)
        {
            if ((uint)index >= (uint)_count)
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Key;
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="index">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueAt(int index, out TValue value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            value = entry.Value;
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="index">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReferenceAt(int index, out NativePtr<TValue> value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref entry.Value));
            return true;
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, TValue> GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index));
        }

        /// <summary>
        ///     Gets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly KeyValuePair<int, NativePtr<TValue>> GetReferenceAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            return new KeyValuePair<int, NativePtr<TValue>>(entry.Key, new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref entry.Value)));
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="keyValuePair" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index));
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="keyValuePair" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetReferenceAt(int index, out KeyValuePair<int, NativePtr<TValue>> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            keyValuePair = new KeyValuePair<int, NativePtr<TValue>>(entry.Key, new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref entry.Value)));
            return true;
        }

        /// <summary>
        ///     Sets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index).Value = value;
            ++_version;
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
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
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
            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<int, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)index);
            var key = entry.Key;
            keyValuePair = Unsafe.As<Entry, KeyValuePair<int, TValue>>(ref entry);
            --_count;
            if (index != _count)
            {
                ref var lastEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_dense), (nint)_count);
                entry = lastEntry;
                Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)lastEntry.Key) = index;
            }

            Unsafe.Add(ref Unsafe.AsRef<int>(_sparse), (nint)key) = -1;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), _count);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), (nint)start), _count - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<KeyValuePair<int, TValue>> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_dense), (nint)start), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<KeyValuePair<int, TValue>>(StackallocSparseSet<TValue> value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     Key
            /// </summary>
            public int Key;

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocSparseSet<TValue> Empty => default;

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
        readonly IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
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
        public struct Enumerator : IIterator<KeyValuePair<int, TValue>>
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocSparseSet<TValue>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocSparseSet<TValue>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = -1;
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
                var num = _index + 1;
                if (num >= handle->_count)
                    return false;
                _index = num;
                return true;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly KeyValuePair<int, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.Add(ref Unsafe.AsRef<KeyValuePair<int, TValue>>(_handle->_dense), (nint)_index);
            }
        }

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<int>
        {
            /// <summary>
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocSparseSet<TValue>* _handle;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(StackallocSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Get key
            /// </summary>
            /// <param name="index">The zero-based starting index.</param>
            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), (nint)index).Key;
                }
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
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
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<int>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(StackallocSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
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
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                public readonly int Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_handle->_dense), (nint)_index).Key;
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
            ///     NativeSparseSet
            /// </summary>
            private readonly StackallocSparseSet<TValue>* _handle;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(StackallocSparseSet<TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->_count;

            /// <summary>
            ///     Reinterprets the given location as a reference to a value.
            /// </summary>
            public ref TValue this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(index, handle->_count, ExceptionArgument.index);
                    return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_dense), (nint)index).Value;
                }
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
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
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>
            {
                /// <summary>
                ///     NativeSparseSet
                /// </summary>
                private readonly StackallocSparseSet<TValue>* _handle;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(StackallocSparseSet<TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = -1;
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
                    var num = _index + 1;
                    if (num >= handle->_count)
                        return false;
                    _index = num;
                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => _index = -1;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => Unsafe.Add(ref Unsafe.AsRef<Entry>(_handle->_dense), (nint)_index).Value;
                }
            }
        }
    }
}