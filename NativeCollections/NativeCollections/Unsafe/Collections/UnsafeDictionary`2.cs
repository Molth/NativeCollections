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
    ///     Represents a collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private HashHelpers.FastModImpl _fastModImpl;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Index of the head of the free list of unused entries.
        /// </summary>
        private int _freeList;

        /// <summary>
        ///     Number of unused entries currently in the free list.
        /// </summary>
        private int _freeCount;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Creates a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>An unsafe dictionary with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeDictionary<TKey, TValue> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            UnsafeDictionary<TKey, TValue> value = default;
            value.Initialize(capacity);
            return value;
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws
        ///     a <see cref="KeyNotFoundException" />, and a set operation creates a new element with the specified key.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                ref var value = ref FindValue(key);
                if (!Unsafe.IsNullRef(ref value))
                    return value;

                ThrowHelpers.ThrowKeyNotFoundException(key);
                return default;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => TryInsertOverwriteExisting(key, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buckets != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count.WrappingSub(_freeCount);

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _entriesLength;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public KeyCollection Keys => KeyCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public ValueCollection Values => ValueCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeDictionary<TKey, TValue> left, UnsafeDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeDictionary<TKey, TValue> left, UnsafeDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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
                SpanHelpers.Set(ref Unsafe.AsRef<byte>(_buckets), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(_bucketsLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));
                SpanHelpers.Set(ref Unsafe.AsRef<byte>(_entries), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>())));
                _count = 0;
                _freeList = -1;
                _freeCount = 0;
            }
        }

        /// <summary>
        ///     Adds the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when an element with the same key already exists in this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value) => TryInsertThrowOnExisting(key, value);

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the key/value pair was successfully
        ///     added to this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value) => TryInsertNone(key, value);

        /// <summary>
        ///     Removes the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully found and
        ///     removed; otherwise, <see langword="false" />. This method returns
        ///     <see langword="false" /> if <paramref name="key" /> is not found in this.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            var collisionCount = U32.FromI32Unchecked(0);
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            ref var bucket = ref GetBucket(hashCode);
            var last = -1;
            var i = bucket.WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    if (last < 0)
                        bucket = entry.Next.WrappingAdd(1);
                    else
                        Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(last)).Next = entry.Next;

                    entry.Next = (-3).WrappingSub(_freeList);
                    _freeList = i;
                    _freeCount = _freeCount.WrappingAdd(1);
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
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
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key, out TValue value)
        {
            var collisionCount = U32.FromI32Unchecked(0);
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            ref var bucket = ref GetBucket(hashCode);
            var last = -1;
            var i = bucket.WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    if (last < 0)
                        bucket = entry.Next.WrappingAdd(1);
                    else
                        Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(last)).Next = entry.Next;

                    value = entry.Value;
                    entry.Next = (-3).WrappingSub(_freeList);
                    _freeList = i;
                    _freeCount = _freeCount.WrappingAdd(1);
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            value = default;
            return false;
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
        public readonly bool ContainsKey(TKey key) => !Unsafe.IsNullRef(ref FindValue(key));

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
            ref var valRef = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref valRef))
            {
                value = valRef;
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
            ref var valRef = ref FindValue(key);
            if (!Unsafe.IsNullRef(ref valRef))
            {
                value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref valRef));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(TKey key) => ref FindValue(key);

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref TValue GetValueRefOrNullRef(TKey key, out bool exists)
        {
            ref var valRef = ref FindValue(key);
            exists = !Unsafe.IsNullRef(ref valRef);
            return ref valRef;
        }

        /// <summary>
        ///     Gets a ref to a <typeparamref name="TValue" /> in this, adding
        ///     a new entry with a default value if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value associated with the specified key.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        /// <remarks>
        ///     Items should not be added to or removed from this
        ///     while the ref <typeparamref name="TValue" /> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(TKey key)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var collisionCount = U32.FromI32Unchecked(0);
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    break;

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    return ref entry.Value;

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
                _freeCount = _freeCount.WrappingSub(1);
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }

                index = count;
                _count = count.WrappingAdd(1);
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket.WrappingSub(1);
            newEntry.Key = key;
            newEntry.Value = default;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return ref newEntry.Value;
        }

        /// <summary>
        ///     Gets a ref to a <typeparamref name="TValue" /> in this, adding
        ///     a new entry with a default value if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <returns>A reference to the value associated with the specified key.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        /// <remarks>
        ///     Items should not be added to or removed from this
        ///     while the ref <typeparamref name="TValue" /> is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(TKey key, out bool exists)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var collisionCount = U32.FromI32Unchecked(0);
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    break;

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    exists = true;
                    return ref entry.Value;
                }

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
                _freeCount = _freeCount.WrappingSub(1);
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }

                index = count;
                _count = count.WrappingAdd(1);
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket.WrappingSub(1);
            newEntry.Key = key;
            newEntry.Value = default;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            exists = false;
            return ref newEntry.Value;
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
            var currentCapacity = _entriesLength;
            if (currentCapacity >= capacity)
                return currentCapacity;

            _version = _version.WrappingAdd(1);
            var newSize = HashHelpers.GetPrime(capacity);
            Resize(newSize);
            return newSize;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => TrimExcess(Count);

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>
        ///     The new capacity of this. If <paramref name="capacity" /> is less
        ///     than <see cref="Count" />, the current capacity is returned unchanged.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < Count)
                return _entriesLength;

            var newSize = HashHelpers.GetPrime(capacity);
            var oldBuckets = _buckets;
            var oldEntries = _entries;
            var currentCapacity = _entriesLength;
            if (newSize >= currentCapacity)
                return currentCapacity;

            var oldCount = _count;
            _version = _version.WrappingAdd(1);
            Initialize(newSize);
            var newEntries = _entries;
            var newCount = 0;
            for (var i = 0; i < oldCount; i = i.WrappingAdd(1))
            {
                var hashCode = Unsafe.Add(ref Unsafe.AsRef<Entry>(oldEntries), Isize.FromI32Unchecked(i)).HashCode;
                if (Unsafe.Add(ref Unsafe.AsRef<Entry>(oldEntries), Isize.FromI32Unchecked(i)).Next >= -1)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(newEntries), Isize.FromI32Unchecked(newCount));
                    entry = Unsafe.Add(ref Unsafe.AsRef<Entry>(oldEntries), Isize.FromI32Unchecked(i));
                    ref var bucket = ref GetBucket(hashCode);
                    entry.Next = bucket.WrappingSub(1);
                    bucket = newCount.WrappingAdd(1);
                    newCount = newCount.WrappingAdd(1);
                }
            }

            NativeMemoryAllocator.AlignedFree(oldBuckets);
            _count = newCount;
            _freeCount = 0;
            return newSize;
        }

        /// <summary>
        ///     Searches for the value associated with the specified key and returns a reference to it.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///     A reference to the value if the key is found; otherwise, a null reference.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the dictionary has been modified concurrently.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref TValue FindValue(TKey key)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var i = GetBucket(hashCode);
            var collisionCount = U32.FromI32Unchecked(0);
            i = i.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    return ref Unsafe.NullRef<TValue>();

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    return ref entry.Value;

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));

                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    break;
            }

            ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            _freeList = -1;
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(size) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            _buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed(bucketsByteCount + Usize.FromI32Unchecked(size) * Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>()), alignment);
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, Isize.FromUsizeUnchecked(bucketsByteCount));
            _bucketsLength = size;
            _entriesLength = size;
            _fastModImpl = HashHelpers.FastModImpl.Create(U32.FromI32Unchecked(size));
        }

        /// <summary>
        ///     Resizes this to the specified new capacity.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize() => Resize(HashHelpers.ExpandPrime(_count));

        /// <summary>
        ///     Resizes this to the specified new capacity.
        /// </summary>
        /// <param name="newSize">The new size of the buckets and entries arrays.</param>
        private void Resize(int newSize)
        {
            var oldBuckets = _buckets;
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(newSize) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            var buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed(bucketsByteCount + Usize.FromI32Unchecked(newSize) * Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>()), alignment);
            var entries = UnsafeHelpers.AddByteOffset<Entry>(buckets, Isize.FromUsizeUnchecked(bucketsByteCount));
            var count = _count;
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(entries), ref Unsafe.AsRef<byte>(_entries), Usize.FromI32Unchecked(count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>())));
            _buckets = buckets;
            _bucketsLength = newSize;
            _fastModImpl = HashHelpers.FastModImpl.Create(U32.FromI32Unchecked(newSize));
            for (var i = 0; i < count; i = i.WrappingAdd(1))
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(i));
                if (entry.Next >= -1)
                {
                    ref var bucket = ref GetBucket(entry.HashCode);
                    entry.Next = bucket.WrappingSub(1);
                    bucket = i.WrappingAdd(1);
                }
            }

            NativeMemoryAllocator.AlignedFree(oldBuckets);
            _entries = entries;
            _entriesLength = newSize;
        }

        /// <summary>
        ///     Inserts the specified key and value into this, overwriting
        ///     the value of an existing entry with the same key.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertOverwriteExisting(TKey key, TValue value)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var collisionCount = U32.FromI32Unchecked(0);
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    break;

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    entry.Value = value;
                    return;
                }

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
                _freeCount = _freeCount.WrappingSub(1);
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }

                index = count;
                _count = count.WrappingAdd(1);
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket.WrappingSub(1);
            newEntry.Key = key;
            newEntry.Value = value;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Inserts the specified key and value into this, and throws if an entry with the same key already exists.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertThrowOnExisting(TKey key, TValue value)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var collisionCount = U32.FromI32Unchecked(0);
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    break;

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    ThrowHelpers.ThrowAddingDuplicateWithKeyException(key);

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
                _freeCount = _freeCount.WrappingSub(1);
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }

                index = count;
                _count = count.WrappingAdd(1);
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket.WrappingSub(1);
            newEntry.Key = key;
            newEntry.Value = value;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>
        ///     <see langword="true" /> if the key/value pair was added;
        ///     otherwise, <see langword="false" /> if the key already exists.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertNone(TKey key, TValue value)
        {
            var hashCode = U32.FromI32Unchecked(key.GetHashCode());
            var collisionCount = U32.FromI32Unchecked(0);
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (true)
            {
                if (U32.FromI32Unchecked(i) >= U32.FromI32Unchecked(_entriesLength))
                    break;

                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    return false;

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(U32.FromI32Unchecked(1));
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
                _freeCount = _freeCount.WrappingSub(1);
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                {
                    Resize();
                    bucket = ref GetBucket(hashCode);
                }

                index = count;
                _count = count.WrappingAdd(1);
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket.WrappingSub(1);
            newEntry.Key = key;
            newEntry.Value = value;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Returns a reference to the bucket corresponding to the specified hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the key.</param>
        /// <returns>A reference to the bucket for the given hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref int GetBucket(uint hashCode) => ref Unsafe.Add(ref Unsafe.AsRef<int>(_buckets), Isize.FromU32Unchecked(_fastModImpl.GetRemainder(hashCode, U32.FromI32Unchecked(_bucketsLength))));

        /// <summary>
        ///     Represents an entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     The hash code of the key.
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     The index of the next entry.
            /// </summary>
            public int Next;

            /// <summary>
            ///     Gets the key of the entry.
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     Gets the value of the entry.
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        public readonly int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, Count));
            var entries = _entries;
            var offset = 0;
            for (var index = 0; index < _count && count != 0; index = index.WrappingAdd(1))
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                if (local.Next >= -1)
                {
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), new KeyValuePair<TKey, TValue>(local.Key, local.Value));
                    count = count.WrappingSub(1);
                }
            }

            return result;
        }

        /// <summary>
        ///     Copies up to the specified number of elements from this. The actual number of copied elements is
        ///     limited by the span's length, the specified count, and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        public readonly void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = Count;
            var entries = _entries;
            var offset = 0;
            for (var index = 0; index < _count && count != 0; index = index.WrappingAdd(1))
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                if (local.Next >= -1)
                {
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), new KeyValuePair<TKey, TValue>(local.Key, local.Value));
                    count = count.WrappingSub(1);
                }
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
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

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
            private readonly UnsafeDictionary<TKey, TValue>* _handle;

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
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(UnsafeDictionary<TKey, TValue>* handle)
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
            internal static Enumerator From(UnsafeDictionary<TKey, TValue>* handle) => new(handle);

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
                while (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), Isize.FromI32Unchecked((_index = _index.WrappingAdd(1)).WrappingSub(1)));
                    if (entry.Next >= -1)
                    {
                        _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                        return true;
                    }
                }

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
            private readonly UnsafeDictionary<TKey, TValue>* _handle;

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
            private KeyCollection(UnsafeDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(UnsafeDictionary<TKey, TValue>* handle) => new(handle);

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
            public int CopyTo(Span<TKey> buffer, int count)
            {
                ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var result = count = Math.Min(buffer.Length, Math.Min(count, _handle->Count));
                var entries = _handle->_entries;
                var offset = 0;
                for (var index = 0; index < _handle->_count && count != 0; index = index.WrappingAdd(1))
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                    if (local.Next >= -1)
                    {
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), local.Key);
                        count = count.WrappingSub(1);
                    }
                }

                return result;
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
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copies all elements from this into a destination span. The span must
            ///     have a length at least equal to the current number of elements in this.
            /// </summary>
            /// <param name="buffer">The destination span to which all elements are copied.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
            /// </exception>
            public void CopyTo(Span<TKey> buffer)
            {
                ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var count = _handle->Count;
                var entries = _handle->_entries;
                var offset = 0;
                for (var index = 0; index < _handle->_count && count != 0; index = index.WrappingAdd(1))
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                    if (local.Next >= -1)
                    {
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), local.Key);
                        count = count.WrappingSub(1);
                    }
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
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

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
                private readonly UnsafeDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
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
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _currentKey = default;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeDictionary<TKey, TValue>* handle) => new(handle);

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
                    while (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                    {
                        ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), Isize.FromI32Unchecked((_index = _index.WrappingAdd(1)).WrappingSub(1)));
                        if (entry.Next >= -1)
                        {
                            _currentKey = entry.Key;
                            return true;
                        }
                    }

                    _index = handle->_count.WrappingAdd(1);
                    _currentKey = default;
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _index = 0;
                    _currentKey = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
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
            private readonly UnsafeDictionary<TKey, TValue>* _handle;

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
            private ValueCollection(UnsafeDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(UnsafeDictionary<TKey, TValue>* handle) => new(handle);

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
                var result = count = Math.Min(buffer.Length, Math.Min(count, _handle->Count));
                var entries = _handle->_entries;
                var offset = 0;
                for (var index = 0; index < _handle->_count && count != 0; index = index.WrappingAdd(1))
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                    if (local.Next >= -1)
                    {
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), local.Value);
                        count = count.WrappingSub(1);
                    }
                }

                return result;
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
                var count = _handle->Count;
                var entries = _handle->_entries;
                var offset = 0;
                for (var index = 0; index < _handle->_count && count != 0; index = index.WrappingAdd(1))
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                    if (local.Next >= -1)
                    {
                        UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked((offset = offset.WrappingAdd(1)).WrappingSub(1))), local.Value);
                        count = count.WrappingSub(1);
                    }
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
                private readonly UnsafeDictionary<TKey, TValue>* _handle;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Used to keep the enumerator synchronized with the collection.
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
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeDictionary<TKey, TValue>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _currentValue = default;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeDictionary<TKey, TValue>* handle) => new(handle);

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
                    while (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(handle->_count))
                    {
                        ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), Isize.FromI32Unchecked((_index = _index.WrappingAdd(1)).WrappingSub(1)));
                        if (entry.Next >= -1)
                        {
                            _currentValue = entry.Value;
                            return true;
                        }
                    }

                    _index = handle->_count.WrappingAdd(1);
                    _currentValue = default;
                    return false;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _index = 0;
                    _currentValue = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _currentValue;
                }
            }
        }
    }
}