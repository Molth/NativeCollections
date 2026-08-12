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
    ///     Represents a collection of key/value pairs that are sorted by the keys
    ///     and are accessible by key and by index.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeSortedList<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeSortedList<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IComparable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        private TKey* _keys;

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        private TValue* _values;

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
        private int _capacity;

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
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                var index = IndexOf(key);
                if (index >= 0)
                    return Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
                ThrowHelpers.ThrowKeyNotFoundException(key);
                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index) = value;
                    ++_version;
                }
                else
                {
                    Insert(~index, key, value);
                }
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_keys);

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
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeSortedList(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
            var keysByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<TKey>()), alignment);
            _keys = (TKey*)NativeMemoryAllocator.AlignedAlloc(keysByteCount + (uint)capacity * (uint)Unsafe.SizeOf<TValue>(), alignment);
            _values = UnsafeHelpers.AddByteOffset<TValue>(_keys, (nint)keysByteCount);
            _count = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeSortedList<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeSortedList<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeSortedList<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeSortedList<TKey, TValue> left, UnsafeSortedList<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeSortedList<TKey, TValue> left, UnsafeSortedList<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_keys);

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
        /// <returns>The index of <paramref name="key" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in TKey key)
        {
            var num = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TKey>(_keys), _count).BinarySearch(key);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Adds the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in this.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value)
        {
            var num = IndexOf(key);
            if (num >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(key);
            Insert(~num, key, value);
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
        public bool TryAdd(in TKey key, in TValue value)
        {
            var num = IndexOf(key);
            if (num >= 0)
                return false;
            Insert(~num, key, value);
            return true;
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
        public bool Remove(in TKey key)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                --_count;
                if (index < _count)
                {
                    SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                    SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
                }

                ++_version;
                return true;
            }

            return false;
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
        public bool Remove(in TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
                --_count;
                if (index < _count)
                {
                    SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                    SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
                }

                ++_version;
                return true;
            }

            value = default;
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
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">The removed element.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            --_count;
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and removed;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_count)
                return false;
            --_count;
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

            ++_version;
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="keyValuePair">The removed element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            --_count;
            if (index < _count)
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

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
            {
                SpanHelpers.Copy(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + count))), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Copy(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + count))), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

            ++_version;
        }

        /// <summary>
        ///     Gets the key at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <returns>The element at the specified index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TKey GetKeyAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index);
        }

        /// <summary>
        ///     Gets the value at the specified index.
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
            return ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
        }

        /// <summary>
        ///     Sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueAt(int index, in TValue value)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index) = value;
            ++_version;
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
        public readonly bool ContainsKey(in TKey key) => IndexOf(key) >= 0;

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
        public readonly bool TryGetValue(in TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
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
        public readonly bool TryGetValueReference(in TKey key, out NativePtr<TValue> value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets the key associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="key">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="key" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetKeyAt(int index, out TKey key)
        {
            if ((uint)index >= (uint)_count)
            {
                key = default;
                return false;
            }

            key = Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index);
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
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

            value = Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index);
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
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

            value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)));
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
        public readonly KeyValuePair<TKey, TValue> GetAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
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
        public readonly KeyValuePair<TKey, NativePtr<TValue>> GetReferenceAt(int index)
        {
            ThrowHelpers.ThrowIfNegative(index, ExceptionArgument.index);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _count, ExceptionArgument.index);
            return new KeyValuePair<TKey, NativePtr<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index))));
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
        public readonly bool TryGetAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index));
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="index">The key of the value to get.</param>
        /// <param name="keyValuePair">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="keyValuePair" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetReferenceAt(int index, out KeyValuePair<TKey, NativePtr<TValue>> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            keyValuePair = new KeyValuePair<TKey, NativePtr<TValue>>(Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index), new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index))));
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
                var alignment = Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
                var keysByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<TKey>()), alignment);
                var keys = (TKey*)NativeMemoryAllocator.AlignedAlloc((uint)(capacity * Unsafe.SizeOf<TValue>()), alignment);
                var values = UnsafeHelpers.AddByteOffset<TValue>(keys, (nint)keysByteCount);
                if (_count > 0)
                {
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(keys), ref Unsafe.AsRef<byte>(_keys), (uint)(_count * Unsafe.SizeOf<TKey>()));
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(values), ref Unsafe.AsRef<byte>(_values), (uint)(_count * Unsafe.SizeOf<TValue>()));
                }

                NativeMemoryAllocator.AlignedFree(_keys);
                _keys = keys;
                _values = values;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, in TKey key, in TValue value)
        {
            if (_count == _capacity)
                EnsureCapacity(_count + 1);
            if (index < _count)
            {
                SpanHelpers.Move(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)(index + 1))), ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<TKey>()));
                SpanHelpers.Move(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)(index + 1))), ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index)), (uint)((_count - index) * Unsafe.SizeOf<TValue>()));
            }

            Unsafe.Add(ref Unsafe.AsRef<TKey>(_keys), (nint)index) = key;
            Unsafe.Add(ref Unsafe.AsRef<TValue>(_values), (nint)index) = value;
            ++_count;
            ++_version;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeSortedList<TKey, TValue> Empty => default;

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
        readonly IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
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
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafeSortedList<TKey, TValue>* _handle;

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
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeSortedList<TKey, TValue>* handle)
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
                    _current = new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)_index), Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)_index));
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
            private readonly UnsafeSortedList<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(UnsafeSortedList<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan()
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TKey>(handle->_keys), handle->_count);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan(int start)
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)start), handle->_count - start);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TKey> AsReadOnlySpan(int start, int length)
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)start), length);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<TKey>(KeyCollection value) => value.AsReadOnlySpan();

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
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
                private readonly UnsafeSortedList<TKey, TValue>* _handle;

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
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafeSortedList<TKey, TValue>* handle)
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
                        _current = Unsafe.Add(ref Unsafe.AsRef<TKey>(handle->_keys), (nint)_index);
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
            private readonly UnsafeSortedList<TKey, TValue>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(UnsafeSortedList<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan()
            {
                var handle = _handle;
                return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<TValue>(handle->_values), handle->_count);
            }

            /// <summary>
            ///     Creates a new span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan(int start)
            {
                var handle = _handle;
                return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), handle->_count - start);
            }

            /// <summary>
            ///     Creates a new span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<TValue> AsSpan(int start, int length)
            {
                var handle = _handle;
                return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), length);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TValue> AsReadOnlySpan()
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TValue>(handle->_values), handle->_count);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TValue> AsReadOnlySpan(int start)
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), handle->_count - start);
            }

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TValue> AsReadOnlySpan(int start, int length)
            {
                var handle = _handle;
                return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)start), length);
            }

            /// <summary>
            ///     Creates a new span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator Span<TValue>(ValueCollection value) => value.AsSpan();

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnlySpan<TValue>(ValueCollection value) => value.AsReadOnlySpan();

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
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TValue>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly UnsafeSortedList<TKey, TValue>* _handle;

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
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafeSortedList<TKey, TValue>* handle)
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
                        _current = Unsafe.Add(ref Unsafe.AsRef<TValue>(handle->_values), (nint)_index);
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
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}