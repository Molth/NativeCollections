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
    ///     Represents a collection of items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
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
        /// <returns>An unsafe hash set with the specified initial capacity.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeHashSet<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            UnsafeHashSet<T> value = default;
            value.Initialize(capacity);
            return value;
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeHashSet<T> left, UnsafeHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeHashSet<T> left, UnsafeHashSet<T> right) => !left.Equals(right);

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
        public bool Add(T item)
        {
            var collisionCount = 0U;
            var hashCode = U32.FromI32Unchecked(item.GetHashCode());
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return false;

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(1U);
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeCount = _freeCount.WrappingSub(1);
                _freeList = (-3).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(_freeList)).Next);
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
            newEntry.Value = item;
            bucket = index.WrappingAdd(1);
            _version = _version.WrappingAdd(1);
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
        /// <exception cref="InvalidOperationException">
        ///     Thrown when a concurrent update corrupted the state of this collection.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item)
        {
            var collisionCount = 0U;
            var last = -1;
            var hashCode = U32.FromI32Unchecked(item.GetHashCode());
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
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
                collisionCount = collisionCount.WrappingAdd(1U);
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
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
            var collisionCount = 0U;
            var last = -1;
            var hashCode = U32.FromI32Unchecked(equalValue.GetHashCode());
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket.WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
                {
                    if (last < 0)
                        bucket = entry.Next.WrappingAdd(1);
                    else
                        Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(last)).Next = entry.Next;

                    entry.Next = (-3).WrappingSub(_freeList);
                    _freeList = i;
                    _freeCount = _freeCount.WrappingAdd(1);
                    actualValue = entry.Value;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(1U);
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(T item) => FindItemIndex(item) >= 0;

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
            var index = FindItemIndex(equalValue);
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
            var index = FindItemIndex(equalValue);
            if (index >= 0)
            {
                actualValue = NativePtr<T>.From(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(index)).Value));
                return true;
            }

            actualValue = default;
            return false;
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
        ///     Searches for the specified item and returns its index in the entries array if found.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The index of <paramref name="item" /> in the entries array if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int FindItemIndex(T item)
        {
            var collisionCount = 0U;
            var hashCode = U32.FromI32Unchecked(item.GetHashCode());
            var i = GetBucket(hashCode).WrappingSub(1);
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), Isize.FromI32Unchecked(i));
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return i;

                i = entry.Next;
                collisionCount = collisionCount.WrappingAdd(1U);
                if (collisionCount > U32.FromI32Unchecked(_entriesLength))
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            return -1;
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
            ///     The hash code of the element.
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     The index of the next entry.
            /// </summary>
            public int Next;

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
        public readonly int CopyTo(Span<T> buffer, int count)
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
        public readonly void CopyTo(Span<T> buffer)
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
            private readonly UnsafeHashSet<T>* _handle;

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
            private Enumerator(UnsafeHashSet<T>* handle)
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
            internal static Enumerator From(UnsafeHashSet<T>* handle) => new(handle);

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
                        _current = entry.Value;
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
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}