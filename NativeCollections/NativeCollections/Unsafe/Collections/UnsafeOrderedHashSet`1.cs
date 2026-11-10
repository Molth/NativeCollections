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
    ///     Represents a collection of unique elements that preserves insertion order.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeOrderedHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeOrderedHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
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
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private HashHelpers.FastModImpl _fastModImpl;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buckets != null;

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
        public readonly int Capacity => _entriesLength;

        /// <summary>
        ///     Creates an ordered hash set with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated ordered hash set.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeOrderedHashSet<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            UnsafeOrderedHashSet<T> value = default;
            value.Initialize(capacity);
            return value;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeOrderedHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeOrderedHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeOrderedHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeOrderedHashSet<T> left, UnsafeOrderedHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeOrderedHashSet<T> left, UnsafeOrderedHashSet<T> right) => !left.Equals(right);

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
                _version = _version.WrappingAdd(1);
            }
        }

        /// <summary>
        ///     Adds the specified element to this.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is added to the instance;
        ///     <see langword="false" /> if the element is already present.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) => TryInsertIgnoreInsertion(item);

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                var count = _count;
                RemoveEntryFromBucket(index);
                var entries = _entries;
                for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
                {
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                    UpdateBucketIndex(entryIndex, -1);
                }

                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
                _version = _version.WrappingAdd(1);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object and returns the equal value from this.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="equalValue" /> is successfully
        ///     removed; otherwise, <see langword="false" />. This method also returns
        ///     <see langword="false" /> if <paramref name="equalValue" /> was not found in this.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T equalValue, out T actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value;
                var count = _count;
                RemoveEntryFromBucket(index);
                var entries = _entries;
                for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
                {
                    Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                    UpdateBucketIndex(entryIndex, -1);
                }

                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
                _version = _version.WrappingAdd(1);
                return true;
            }

            actualValue = default;
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
            var count = _count;
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(count), ExceptionArgument.index);
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="item">The removed element.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out T item)
        {
            var count = _count;
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(count), ExceptionArgument.index);
            item = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value;
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
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
            var count = _count;
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(count))
                return false;

            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="item">The removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item is successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out T item)
        {
            var count = _count;
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(count))
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value;
            RemoveEntryFromBucket(index);
            var entries = _entries;
            for (var entryIndex = index.WrappingAdd(1); entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingSub(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, -1);
            }

            Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(_count = _count.WrappingSub(1))) = default;
            _version = _version.WrappingAdd(1);
            return true;
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
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(T equalValue, out T actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value;
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Searches the set for a given value and returns a reference to the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     A reference to the value from the set that the search found, or the default value
        ///     of <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of a newly constructed
        ///     one (so that more sharing of references can occur) or to look up a value that has more complete
        ///     data than the value you currently have, although their comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(T equalValue, out NativePtr<T> actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = NativePtr<T>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetAt(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            return ref local.Value;
        }

        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="item">
        ///     When this method returns, contains the element at the specified index
        ///     if the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="item" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified index is valid and
        ///     the value was retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out T item)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_count))
            {
                item = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            item = local.Value;
            return true;
        }

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(T item)
        {
            var num = 0U;
            return IndexOf(item, ref num, ref num);
        }

        /// <summary>
        ///     Determines the index of a specific element in this.
        /// </summary>
        /// <param name="item">The item to locate.</param>
        /// <param name="outHashCode">
        ///     When this method returns, contains the hash code of <paramref name="item" />.
        /// </param>
        /// <param name="outCollisionCount">
        ///     When this method returns and the item was not found, contains the number
        ///     of collisions encountered while probing; otherwise, its value is unchanged.
        /// </param>
        /// <returns>The index of <paramref name="item" /> if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int IndexOf(T item, ref uint outHashCode, ref uint outCollisionCount)
        {
            var num = 0U;
            var entries = _entries;
            var hashCode = U32.FromI32Unchecked(item.GetHashCode());
            var index = GetBucket(hashCode).WrappingSub(1);
            while (U32.FromI32Unchecked(index) < U32.FromI32Unchecked(_entriesLength))
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                if (local.HashCode != hashCode || !local.Value.Equals(item))
                {
                    index = local.Next;
                    num = num.WrappingAdd(1U);
                    if (num > U32.FromI32Unchecked(_entriesLength))
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
        ///     Inserts an element into this at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than <see cref="Count" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="item" /> already exists in this.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            TryInsertThrowOnExisting(index, item);
        }

        /// <summary>
        ///     Sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <param name="item">The value to store at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="item" /> already exists in this.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, T item)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index));
            if (item.Equals(local.Value))
                return;

            var outHashCode = 0U;
            var outCollisionCount = 0U;
            if (IndexOf(item, ref outHashCode, ref outCollisionCount) >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(item);

            RemoveEntryFromBucket(index);
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
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
            if (_entriesLength < capacity)
            {
                Resize(HashHelpers.GetPrime(capacity));
                _version = _version.WrappingAdd(1);
            }

            return _entriesLength;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => TrimExcess(_count);

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>
        ///     The new capacity of this. If <paramref name="capacity" /> is less
        ///     than <see cref="Count" />, the current capacity is returned unchanged.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var length = _entriesLength;
            if (capacity < _count)
                return length;

            if (capacity >= length)
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
        /// <param name="entry">A reference to the entry to insert into its hash bucket chain.</param>
        /// <param name="entryIndex">The zero-based index of the entry in the entries array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void PushEntryIntoBucket(ref Entry entry, int entryIndex)
        {
            ref var local = ref GetBucket(entry.HashCode);
            entry.Next = local.WrappingSub(1);
            local = entryIndex.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes an entry from its hash bucket chain.
        /// </summary>
        /// <param name="entryIndex">The zero-based index of the entry to remove from its hash bucket chain.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void RemoveEntryFromBucket(int entryIndex)
        {
            var entries = _entries;
            var entry = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
            ref var local1 = ref GetBucket(entry.HashCode);
            if (local1 == entryIndex.WrappingAdd(1))
            {
                local1 = entry.Next.WrappingAdd(1);
            }
            else
            {
                var index = local1.WrappingSub(1);
                var num = 0;
                while (true)
                {
                    while (true)
                    {
                        ref var local2 = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                        if (local2.Next == entryIndex)
                        {
                            local2.Next = entry.Next;
                            return;
                        }

                        index = local2.Next;

                        if ((num = num.WrappingAdd(1)) > _entriesLength)
                            break;
                    }

                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
                }
            }
        }

        /// <summary>
        ///     Updates the bucket chain pointers when an entry's index shifts by a specified amount.
        /// </summary>
        /// <param name="entryIndex">The zero-based index of the entry whose bucket chain pointers are updated.</param>
        /// <param name="shiftAmount">
        ///     The number of positions by which the entry index shifts,
        ///     which is added to the bucket value that points at the entry.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void UpdateBucketIndex(int entryIndex, int shiftAmount)
        {
            var entries = _entries;
            ref var local1 = ref GetBucket(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex)).HashCode);
            if (local1 == entryIndex.WrappingAdd(1))
            {
                local1 = local1.WrappingAdd(shiftAmount);
            }
            else
            {
                var index = local1.WrappingSub(1);
                var num = 0;
                while (true)
                {
                    while (true)
                    {
                        ref var local2 = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
                        if (local2.Next == entryIndex)
                        {
                            local2.Next = local2.Next.WrappingAdd(shiftAmount);
                            return;
                        }

                        index = local2.Next;

                        if ((num = num.WrappingAdd(1)) > _entriesLength)
                            break;
                    }

                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
                }
            }
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
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
            for (var entryIndex = 0; entryIndex < count; entryIndex = entryIndex.WrappingAdd(1))
                PushEntryIntoBucket(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex)), entryIndex);

            NativeMemoryAllocator.AlignedFree(oldBuckets);
            _entries = entries;
            _entriesLength = newSize;
        }

        /// <summary>
        ///     Attempts to insert the specified item without overwriting an existing item.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was inserted at the end;
        ///     otherwise, <see langword="false" /> if an equal item already exists.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertIgnoreInsertion(T item)
        {
            var outHashCode = 0U;
            var outCollisionCount = 0U;
            var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return false;

            var index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count.WrappingSub(1); entryIndex >= index; entryIndex = entryIndex.WrappingSub(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingAdd(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Inserts the specified item at the specified index and throws if an equal item already exists.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index at which to insert the item, or a negative value to append it.
        /// </param>
        /// <param name="item">The item to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertThrowOnExisting(int index, T item)
        {
            var outHashCode = 0U;
            var outCollisionCount = 0U;
            var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(item);

            if (index < 0)
                index = _count;

            var entries = _entries;
            if (_entriesLength == _count)
            {
                Resize(HashHelpers.ExpandPrime(_entriesLength));
                entries = _entries;
            }

            for (var entryIndex = _count.WrappingSub(1); entryIndex >= index; entryIndex = entryIndex.WrappingSub(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex.WrappingAdd(1))) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(entryIndex));
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index));
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            _count = _count.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Returns a reference to the bucket corresponding to the specified hash code.
        /// </summary>
        /// <param name="hashCode">The hash code of the element.</param>
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
            ///     The index of the next entry.
            /// </summary>
            public int Next;

            /// <summary>
            ///     The hash code of the element.
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     Gets the value of the entry.
            /// </summary>
            public T Value;
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
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var entries = _entries;
            for (var index = 0; index < count; index = index.WrappingAdd(1))
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index)).Value);

            return count;
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
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span. The span must
        ///     have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all the elements of this.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var entries = _entries;
            for (var index = 0; index < _count; index = index.WrappingAdd(1))
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), Isize.FromI32Unchecked(index)).Value);
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
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

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
            private readonly UnsafeOrderedHashSet<T>* _handle;

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
            private Enumerator(UnsafeOrderedHashSet<T>* handle)
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
            internal static Enumerator From(UnsafeOrderedHashSet<T>* handle) => new(handle);

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
                if (_index < handle->_count)
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), Isize.FromI32Unchecked(_index));
                    _current = local.Value;
                    _index = _index.WrappingAdd(1);
                    return true;
                }

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