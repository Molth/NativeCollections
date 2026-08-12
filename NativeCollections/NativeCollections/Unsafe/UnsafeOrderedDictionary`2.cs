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
    ///     Represents a collection of key/value pairs that are accessible by the key or index.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeOrderedDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeOrderedDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Array of bucket.
        /// </summary>
        private int* _buckets;

        /// <summary>
        ///     Array of entry.
        /// </summary>
        private Entry* _entries;

        /// <summary>
        ///     Length of the bucket array.
        /// </summary>
        private int _bucketsLength;

        /// <summary>
        ///     Length of the entry array.
        /// </summary>
        private int _entriesLength;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private HashHelpers.FastModImpl _fastModImpl;

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
        public TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                if (!TryGetValue(key, out var obj))
                    ThrowHelpers.ThrowKeyNotFoundException(key);
                return obj;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => TryInsertOverwriteExisting(-1, key, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buckets);

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
        public readonly int Capacity => _entriesLength;

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
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeOrderedDictionary(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            this = new UnsafeOrderedDictionary<TKey, TValue>();
            Initialize(capacity);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeOrderedDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeOrderedDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeOrderedDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeOrderedDictionary<TKey, TValue> left, UnsafeOrderedDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeOrderedDictionary<TKey, TValue> left, UnsafeOrderedDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buckets);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var count = _count;
            if (count > 0)
            {
                SpanHelpers.Set(ref Unsafe.AsRef<byte>(_buckets), 0, (uint)(count * Unsafe.SizeOf<int>()));
                SpanHelpers.Set(ref Unsafe.AsRef<byte>(_entries), 0, (uint)(count * Unsafe.SizeOf<Entry>()));
                _count = 0;
                ++_version;
            }
        }

        /// <summary>
        ///     Adds the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in this.</exception>
        /// iveInlining)]
        public void Add(in TKey key, in TValue value) => TryInsertThrowOnExisting(-1, key, value);

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
        public bool TryAdd(in TKey key, in TValue value) => TryInsertIgnoreInsertion(key, value);

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
                var count = _count;
                RemoveEntryFromBucket(index);
                var entries = _entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                    UpdateBucketIndex(entryIndex, -1);
                }

                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
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
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
                var count = _count;
                RemoveEntryFromBucket(index);
                var entries = _entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                    UpdateBucketIndex(entryIndex, -1);
                }

                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
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
            var count = _count;
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)count, ExceptionArgument.index);
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
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
            var count = _count;
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            var count = _count;
            if ((uint)index >= (uint)count)
                return false;
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
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
            var count = _count;
            if ((uint)index >= (uint)count)
            {
                keyValuePair = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex - 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(--_count)) = new Entry();
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
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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
                value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(in TKey key)
        {
            var index = IndexOf(key);
            return ref index >= 0 ? ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value : ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                exists = true;
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
            }

            exists = false;
            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Gets a ref to a <typeparamref name="TValue" /> in the this, adding a new entry with a default value
        ///     if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <remarks>Items should not be added to or removed from the this while the ref <typeparamref name="TValue" /> is in use.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value;
            var index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = default;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return ref local.Value;
        }

        /// <summary>
        ///     Gets a ref to a <typeparamref name="TValue" /> in the this, adding a new entry with a default value
        ///     if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <remarks>Items should not be added to or removed from the this while the ref <typeparamref name="TValue" /> is in use.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key, out bool exists)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
            {
                exists = true;
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value;
            }

            var index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = default;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            exists = false;
            return ref local.Value;
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return local.Key;
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return ref local.Value;
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

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            key = local.Key;
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

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            value = local.Value;
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

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            value = new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref local.Value));
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return new KeyValuePair<TKey, TValue>(local.Key, local.Value);
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
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return new KeyValuePair<TKey, NativePtr<TValue>>(local.Key, new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref local.Value)));
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

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
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

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, NativePtr<TValue>>(local.Key, new NativePtr<TValue>(UnsafeHelpers.AsPointer(ref local.Value)));
            return true;
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        /// <returns>The index of <paramref name="key" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in TKey key)
        {
            uint num = 0;
            return IndexOf(key, ref num, ref num);
        }

        /// <summary>
        ///     Determines the index of a specific key in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int IndexOf(in TKey key, ref uint outHashCode, ref uint outCollisionCount)
        {
            uint num = 0;
            var entries = _entries;
            var hashCode = (uint)key.GetHashCode();
            var index = GetBucket(hashCode) - 1;
            while ((uint)index < (uint)_entriesLength)
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                if ((int)local.HashCode != (int)hashCode || !local.Key.Equals(key))
                {
                    index = local.Next;
                    ++num;
                    if (num > (uint)_entriesLength)
                        ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
                }
                else
                {
                    outHashCode = hashCode;
                    return index;
                }
            }

            outCollisionCount = num;
            outHashCode = hashCode;
            return -1;
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in this.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in TKey key, in TValue value)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            TryInsertThrowOnExisting(index, key, value);
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
        public readonly void SetAt(int index, in TValue value)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value = value;
        }

        /// <summary>
        ///     Sets the key/value pair at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="key">The key to store at the specified index.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in TKey key, in TValue value)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            if (key.Equals(local.Key))
            {
                local.Value = value;
                return;
            }

            uint outHashCode = 0;
            uint outCollisionCount = 0;
            if (IndexOf(key, ref outHashCode, ref outCollisionCount) >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(key);
            RemoveEntryFromBucket(index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_version;
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
            if (_entriesLength < capacity)
            {
                Resize(HashHelpers.GetPrime(capacity));
                ++_version;
            }

            return _entriesLength;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => TrimExcess(_count);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            var length = _entriesLength;
            if (capacity <= length)
                return length;
            capacity = HashHelpers.GetPrime(capacity);
            if (capacity >= length)
                return length;
            Resize(capacity);
            return capacity;
        }

        /// <summary>
        ///     Inserts an entry into the hash bucket chain at the head position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void PushEntryIntoBucket(ref Entry entry, int entryIndex)
        {
            ref var local = ref GetBucket(entry.HashCode);
            entry.Next = local - 1;
            local = entryIndex + 1;
        }

        /// <summary>
        ///     Removes an entry from its hash bucket chain.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void RemoveEntryFromBucket(int entryIndex)
        {
            var entries = _entries;
            var entry = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
            ref var local1 = ref GetBucket(entry.HashCode);
            if (local1 == entryIndex + 1)
            {
                local1 = entry.Next + 1;
            }
            else
            {
                var index = local1 - 1;
                var num = 0;
                while (true)
                {
                    do
                    {
                        ref var local2 = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                        if (local2.Next == entryIndex)
                        {
                            local2.Next = entry.Next;
                            return;
                        }

                        index = local2.Next;
                    } while (++num <= _entriesLength);

                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
                }
            }
        }

        /// <summary>
        ///     Updates the bucket chain pointers when an entry's index shifts by a specified amount.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void UpdateBucketIndex(int entryIndex, int shiftAmount)
        {
            var entries = _entries;
            ref var local1 = ref GetBucket(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex).HashCode);
            if (local1 == entryIndex + 1)
            {
                local1 += shiftAmount;
            }
            else
            {
                var index = local1 - 1;
                var num = 0;
                while (true)
                {
                    do
                    {
                        ref var local2 = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                        if (local2.Next == entryIndex)
                        {
                            local2.Next += shiftAmount;
                            return;
                        }

                        index = local2.Next;
                    } while (++num <= _entriesLength);

                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
                }
            }
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(size * Unsafe.SizeOf<int>()), alignment);
            _buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed(bucketsByteCount + (uint)size * (uint)Unsafe.SizeOf<Entry>(), alignment);
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, (nint)bucketsByteCount);
            _bucketsLength = size;
            _entriesLength = size;
            _fastModImpl = HashHelpers.GetFastModImpl((uint)size);
        }

        /// <summary>
        ///     Resizes this to the specified new capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newSize)
        {
            var oldBuckets = _buckets;
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(newSize * Unsafe.SizeOf<int>()), alignment);
            var buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed(bucketsByteCount + (uint)newSize * (uint)Unsafe.SizeOf<Entry>(), alignment);
            var entries = UnsafeHelpers.AddByteOffset<Entry>(buckets, (nint)bucketsByteCount);
            var count = _count;
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(entries), ref Unsafe.AsRef<byte>(_entries), (uint)(count * Unsafe.SizeOf<Entry>()));
            _buckets = buckets;
            _bucketsLength = newSize;
            _fastModImpl = HashHelpers.GetFastModImpl((uint)newSize);
            for (var entryIndex = 0; entryIndex < count; ++entryIndex)
                PushEntryIntoBucket(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex), entryIndex);
            NativeMemoryAllocator.AlignedFree(oldBuckets);
            _entries = entries;
            _entriesLength = newSize;
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertIgnoreInsertion(in TKey key, in TValue value)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return false;
            var index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertOverwriteExisting(int index, in TKey key, in TValue value)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value = value;
                return true;
            }

            if (index < 0)
                index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Inserts an item into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in this.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertThrowOnExisting(int index, in TKey key, in TValue value)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(key);
            if (index < 0)
                index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
        }

        /// <summary>
        ///     Returns a reference to the bucket corresponding to the specified hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the key.</param>
        /// <returns>A reference to the bucket for the given hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref int GetBucket(uint hashCode) => ref Unsafe.Add(ref Unsafe.AsRef<int>(_buckets), (nint)_fastModImpl.GetRemainder(hashCode, (uint)_bucketsLength));

        /// <summary>
        ///     Represents an entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     The index of the next entry.
            /// </summary>
            public int Next;

            /// <summary>
            ///     The hash code of the key.
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     Gets the key to the underlying object.
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     Gets the value to the underlying object.
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var entries = _entries;
            for (var index = 0; index < count; ++index)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key, Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value));
            return count;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var entries = _entries;
            for (var index = 0; index < _count; ++index)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key, Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value));
        }

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeOrderedDictionary<TKey, TValue> Empty => default;

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
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _handle;

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
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(UnsafeOrderedDictionary<TKey, TValue>* handle)
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
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if (_index < handle->_count)
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
                    _current = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
                    ++_index;
                    return true;
                }

                _current = new KeyValuePair<TKey, TValue>();
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
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _handle;

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
            internal KeyCollection(UnsafeOrderedDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Copies up to the specified number of elements from this.
            ///     The actual number of copied elements is limited by the span's length, the specified count,
            ///     and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from the this.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TKey> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var entries = _handle->_entries;
                for (var index = 0; index < count; ++index)
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key);
                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this.
            ///     The actual number of copied elements is limited by the span's length, the specified count,
            ///     and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from the this.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TKey> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var entries = _handle->_entries;
                for (var index = 0; index < _handle->_count; ++index)
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key);
            }

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

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
                private readonly UnsafeOrderedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TKey _currentKey;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafeOrderedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _currentKey = default;
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
                    if (_index < handle->_count)
                    {
                        ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
                        _currentKey = local.Key;
                        ++_index;
                        return true;
                    }

                    _currentKey = default;
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = 0;
                    _currentKey = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                public readonly TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _currentKey;
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
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _handle;

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
            internal ValueCollection(UnsafeOrderedDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Copies up to the specified number of elements from this.
            ///     The actual number of copied elements is limited by the span's length, the specified count,
            ///     and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from the this.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TValue> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _handle->_count));
                var entries = _handle->_entries;
                for (var index = 0; index < count; ++index)
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
                return count;
            }

            /// <summary>
            ///     Copies up to the specified number of elements from this.
            ///     The actual number of copied elements is limited by the span's length, the specified count,
            ///     and the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which elements are copied.</param>
            /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
            /// <returns>The actual number of elements copied from the this.</returns>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TValue> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var entries = _handle->_entries;
                for (var index = 0; index < _handle->_count; ++index)
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
            }

            /// <summary>
            ///     Copies all elements from this into a destination span.
            ///     The span must have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

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
                private readonly UnsafeOrderedDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private TValue _currentValue;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafeOrderedDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _currentValue = default;
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
                    if (_index < handle->_count)
                    {
                        ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
                        _currentValue = local.Value;
                        ++_index;
                        return true;
                    }

                    _currentValue = default;
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = 0;
                    _currentValue = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _currentValue;
                }
            }
        }
    }
}