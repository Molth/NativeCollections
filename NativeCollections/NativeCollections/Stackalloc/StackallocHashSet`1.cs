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
    ///     Stackalloc hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocHashSet<T> : IIsCreated, IEquatable<StackallocHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Buckets
        /// </summary>
        private readonly int* _buckets;

        /// <summary>
        ///     Entries
        /// </summary>
        private readonly Entry* _entries;

        /// <summary>
        ///     BucketsLength
        /// </summary>
        private readonly int _bucketsLength;

        /// <summary>
        ///     EntriesLength
        /// </summary>
        private readonly int _entriesLength;

        /// <summary>
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private readonly ulong _fastModMultiplier;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        private int _count;

        /// <summary>
        ///     FreeList
        /// </summary>
        private int _freeList;

        /// <summary>
        ///     FreeCount
        /// </summary>
        private int _freeCount;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buckets);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public readonly bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count - _freeCount;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _entriesLength;

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
            var size = HashHelpers.GetPrime(capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(size * Unsafe.SizeOf<int>()), alignment);
            return (int)(bucketsByteCount + size * Unsafe.SizeOf<Entry>() + alignment - 1);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MustBeZeroed(nameof(buffer))]
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocHashSet([MustBeZeroed] [MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var size = HashHelpers.GetPrime(capacity);
            _freeList = -1;
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(size * Unsafe.SizeOf<int>()), alignment);
            _buckets = (int*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, (nint)bucketsByteCount);
            _bucketsLength = size;
            _entriesLength = size;
            _fastModMultiplier = Environment.Is64BitProcess ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
            _count = 0;
            _freeCount = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocHashSet<T> left, StackallocHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocHashSet<T> left, StackallocHashSet<T> right) => !left.Equals(right);

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
                _freeList = -1;
                _freeCount = 0;
            }
        }

        /// <summary>
        ///     Adds the specified element to this.
        /// </summary>
        /// <param name="item">The element to add to the set.</param>
        /// <returns>
        ///     true if the element is added to the instance;
        ///     false if the element is already present.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in T item)
        {
            uint collisionCount = 0;
            var hashCode = (uint)item.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)i);
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return InsertResult.AlreadyExists;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeCount--;
                _freeList = -3 - Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)_freeList).Next;
            }
            else
            {
                var count = _count;
                if (count == _entriesLength)
                    return InsertResult.InsufficientCapacity;
                index = count;
                _count = count + 1;
            }

            ref var newEntry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket - 1;
            newEntry.Value = item;
            bucket = index + 1;
            _version++;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from this.
        /// </summary>
        /// <param name="item">The object to remove from this.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="item" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="item" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            uint collisionCount = 0;
            var last = -1;
            var hashCode = (uint)item.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)i);
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                {
                    if (last < 0)
                        bucket = entry.Next + 1;
                    else
                        Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)last).Next = entry.Next;
                    entry.Next = -3 - _freeList;
                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            return false;
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object and returns the equal value from this.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="equalValue" /> is successfully removed;
        ///     otherwise, <see langword="false" />.
        ///     This method also returns <see langword="false" /> if <paramref name="equalValue" /> was not found in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T equalValue, out T actualValue)
        {
            uint collisionCount = 0;
            var last = -1;
            var hashCode = (uint)equalValue.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)i);
                if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
                {
                    if (last < 0)
                        bucket = entry.Next + 1;
                    else
                        Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)last).Next = entry.Next;
                    entry.Next = -3 - _freeList;
                    _freeList = i;
                    _freeCount++;
                    actualValue = entry.Value;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>true if this contains the specified element; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => FindItemIndex(item) >= 0;

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(in T equalValue, out T actualValue)
        {
            var index = FindItemIndex(equalValue);
            if (index >= 0)
            {
                actualValue = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Searches the set for a given value and returns the equal value it finds, if any.
        /// </summary>
        /// <param name="equalValue">The value to search for.</param>
        /// <param name="actualValue">
        ///     The value from the set that the search found, or the default value of
        ///     <typeparamref name="T" /> when the search yielded no match.
        /// </param>
        /// <returns>A value indicating whether the search was successful.</returns>
        /// <remarks>
        ///     This can be useful when you want to reuse a previously stored reference instead of
        ///     a newly constructed one (so that more sharing of references can occur) or to look up
        ///     a value that has more complete data than the value you currently have, although their
        ///     comparer functions indicate they are equal.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(in T equalValue, out NativePtr<T> actualValue)
        {
            var index = FindItemIndex(equalValue);
            if (index >= 0)
            {
                actualValue = new NativePtr<T>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Find item index
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int FindItemIndex(in T item)
        {
            uint collisionCount = 0;
            var hashCode = (uint)item.GetHashCode();
            var i = GetBucket(hashCode) - 1;
            while (i >= 0)
            {
                ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)i);
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
                    ThrowHelpers.ThrowConcurrentOperationsNotSupportedException();
            }

            return -1;
        }

        /// <summary>
        ///     Get bucket ref
        /// </summary>
        /// <param name="hashCode">HashCode</param>
        /// <returns>Bucket ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref int GetBucket(uint hashCode) => ref Environment.Is64BitProcess ? ref Unsafe.Add(ref Unsafe.AsRef<int>(_buckets), (nint)HashHelpers.FastMod(hashCode, (uint)_bucketsLength, _fastModMultiplier)) : ref Unsafe.Add(ref Unsafe.AsRef<int>(_buckets), (nint)(hashCode % (uint)_bucketsLength));

        /// <summary>
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     HashCode
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     Next
            /// </summary>
            public int Next;

            /// <summary>
            ///     Value
            /// </summary>
            public T Value;
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
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var result = count = Math.Min(buffer.Length, Math.Min(count, Count));
            var entries = _entries;
            var offset = 0;
            for (var index = 0; index < _count && count != 0; ++index)
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                if (local.Next >= -1)
                {
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)offset++), local.Value);
                    --count;
                }
            }

            return result;
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
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var count = Count;
            var entries = _entries;
            var offset = 0;
            for (var index = 0; index < _count && count != 0; ++index)
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                if (local.Next >= -1)
                {
                    UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)offset++), local.Value);
                    --count;
                }
            }
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
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocHashSet<T> Empty => default;

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
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
        public struct Enumerator : IIterator<T>
        {
            /// <summary>
            ///     NativeHashSet
            /// </summary>
            private readonly StackallocHashSet<T>* _handle;

            /// <summary>
            ///     Used to keep enumerator in sync w/ collection.
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocHashSet<T>* handle)
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
                while ((uint)_index < (uint)handle->_count)
                {
                    ref var entry = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index++);
                    if (entry.Next >= -1)
                    {
                        _current = entry.Value;
                        return true;
                    }
                }

                _index = handle->_count + 1;
                _current = default;
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
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}