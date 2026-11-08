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
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocDictionary<TKey, TValue> : IIsCreated, IEquatable<StackallocDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Array of bucket.
        /// </summary>
        private readonly int* _buckets;

        /// <summary>
        ///     Array of entry.
        /// </summary>
        private readonly Entry* _entries;

        /// <summary>
        ///     Length of the bucket array.
        /// </summary>
        private readonly int _bucketsLength;

        /// <summary>
        ///     Length of the entry array.
        /// </summary>
        private readonly int _entriesLength;

        /// <summary>
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private readonly HashHelpers.FastModImpl _fastModImpl;

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
            var size = HashHelpers.GetPrime(capacity);
            var nativeSize = Usize.FromI32Unchecked(size);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = NativeMemoryAllocator.AlignUp(nativeSize * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            var byteCount = bucketsByteCount + nativeSize * Usize.FromI32Unchecked(Unsafe.SizeOf<Entry>()) + alignment.WrappingSub(Usize.FromI32Unchecked(1));
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
        ///     The maximum number of elements the dictionary can hold. Must be non-negative.
        /// </param>
        /// <returns>
        ///     A dictionary that uses <paramref name="buffer" /> as its
        ///     storage and holds up to <paramref name="capacity" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static StackallocDictionary<TKey, TValue> FromBytes([MustBeZeroed] [MustBePinned] Span<byte> buffer, int capacity) => new(buffer, capacity);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the dictionary can hold.
        /// </param>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        private StackallocDictionary([MustBeZeroed] [MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var size = HashHelpers.GetPrime(capacity);
            _freeList = -1;
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(size) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            _buckets = (int*)NativeArray<byte>.FromBytes(buffer, alignment).Buffer;
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, Isize.FromUsizeUnchecked(bucketsByteCount));
            _bucketsLength = size;
            _entriesLength = size;
            _fastModImpl = HashHelpers.FastModImpl.Create(U32.FromI32Unchecked(size));
            _count = 0;
            _freeCount = 0;
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
        public readonly bool Equals(StackallocDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("StackallocDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocDictionary<TKey, TValue> left, StackallocDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(StackallocDictionary<TKey, TValue> left, StackallocDictionary<TKey, TValue> right) => !left.Equals(right);

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
        public InsertResult TryAdd(TKey key, TValue value) => TryInsertNone(key, value);

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
        public InsertResult TryAddOrUpdate(TKey key, TValue value) => TryInsertOverwriteExisting(key, value);

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
        ///     Gets a pointer to the value associated with the specified key, adding
        ///     a new entry with a default value if the key does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains a pointer to the value associated with the
        ///     specified key if the operation succeeds; otherwise, the default value for the type
        ///     of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the value was found or a new
        ///     entry was added; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        /// <remarks>
        ///     Items should not be added to or removed from this while the returned pointer is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(TKey key, out NativePtr<TValue> value)
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
                    value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref entry.Value));
                    return true;
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
                    value = default;
                    return false;
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
            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref newEntry.Value));
            return true;
        }

        /// <summary>
        ///     Gets a pointer to the value associated with the specified key, adding
        ///     a new entry with a default value if the key does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains a pointer to the value associated with the
        ///     specified key if the operation succeeds; otherwise, the default value for the type
        ///     of the <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <param name="exists">
        ///     When this method returns, indicates whether the specified key was
        ///     already present in this. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the value was found or a new
        ///     entry was added; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        /// <remarks>
        ///     Items should not be added to or removed from this while the returned pointer is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueRefOrAddDefault(TKey key, out NativePtr<TValue> value, out bool exists)
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
                    value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref entry.Value));
                    exists = true;
                    return true;
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
                    value = default;
                    exists = false;
                    return false;
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
            value = NativePtr<TValue>.From(UnsafeHelpers.AsPointer(ref newEntry.Value));
            exists = false;
            return true;
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
        ///     Adds the specified key and value to the collection,
        ///     overwriting the value of an existing entry with the same key.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the entry was added; <see cref="InsertResult.Overwritten" />
        ///     if the value of an existing entry with the same key was replaced; otherwise,
        ///     <see cref="InsertResult.InsufficientCapacity" /> if the collection has no free capacity.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult TryInsertOverwriteExisting(TKey key, TValue value)
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
                    return InsertResult.Overwritten;
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
                    return InsertResult.InsufficientCapacity;

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
            return InsertResult.Success;
        }

        /// <summary>
        ///     Adds the specified key and value to the collection without overwriting an existing entry.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if the entry was added;
        ///     <see cref="InsertResult.AlreadyExists" /> if an entry with the same key already exists; otherwise,
        ///     <see cref="InsertResult.InsufficientCapacity" /> if the collection has no free capacity.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult TryInsertNone(TKey key, TValue value)
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
                    return InsertResult.AlreadyExists;

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
                    return InsertResult.InsufficientCapacity;

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
            return InsertResult.Success;
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
            private readonly StackallocDictionary<TKey, TValue>* _handle;

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
            private Enumerator(StackallocDictionary<TKey, TValue>* handle)
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
            internal static Enumerator From(StackallocDictionary<TKey, TValue>* handle) => new(handle);

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
            private readonly StackallocDictionary<TKey, TValue>* _handle;

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
            private KeyCollection(StackallocDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(StackallocDictionary<TKey, TValue>* handle) => new(handle);

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
                private readonly StackallocDictionary<TKey, TValue>* _handle;

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
                private Enumerator(StackallocDictionary<TKey, TValue>* handle)
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
                internal static Enumerator From(StackallocDictionary<TKey, TValue>* handle) => new(handle);

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
            private readonly StackallocDictionary<TKey, TValue>* _handle;

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
            private ValueCollection(StackallocDictionary<TKey, TValue>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(StackallocDictionary<TKey, TValue>* handle) => new(handle);

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
                private readonly StackallocDictionary<TKey, TValue>* _handle;

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
                private Enumerator(StackallocDictionary<TKey, TValue>* handle)
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
                internal static Enumerator From(StackallocDictionary<TKey, TValue>* handle) => new(handle);

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