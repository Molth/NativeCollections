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
    ///     Stackalloc ordered hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocOrderedHashSet<T> : IIsCreated, IEquatable<StackallocOrderedHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
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
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
        /// </summary>
        private readonly ulong _fastModMultiplier;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buckets);

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _entriesLength;

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var size = HashHelpers.GetPrime(capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
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
        public StackallocOrderedHashSet([MustBeZeroed] [MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var size = HashHelpers.GetPrime(capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(size * Unsafe.SizeOf<int>()), alignment);
            _buckets = (int*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, (nint)bucketsByteCount);
            _bucketsLength = size;
            _entriesLength = size;
            _fastModMultiplier = Environment.Is64BitProcess ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(StackallocOrderedHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is StackallocOrderedHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("StackallocOrderedHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(StackallocOrderedHashSet<T> left, StackallocOrderedHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(StackallocOrderedHashSet<T> left, StackallocOrderedHashSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var count = _count;
            if (count > 0)
            {
                Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_buckets), 0, (uint)(count * Unsafe.SizeOf<int>()));
                Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_entries), 0, (uint)(count * Unsafe.SizeOf<Entry>()));
                _count = 0;
                ++_version;
            }
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in T item) => TryInsertIgnoreInsertion(-1, item);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            var index = IndexOf(item);
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
        ///     Remove
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T equalValue, out T actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
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
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out T item)
        {
            var count = _count;
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)count, ExceptionArgument.index);
            item = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
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
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out T item)
        {
            var count = _count;
            if ((uint)index >= (uint)count)
            {
                item = default;
                return false;
            }

            item = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(in T equalValue, out T actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = new NativeReference<T>(UnsafeHelpers.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value));
                return true;
            }

            actualValue = default;
            return false;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetAt(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return ref local.Value;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetAt(int index, out T item)
        {
            if ((uint)index >= (uint)_count)
            {
                item = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            item = local.Value;
            return true;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int IndexOf(in T item)
        {
            uint num = 0;
            return IndexOf(item, ref num, ref num);
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="outHashCode">Out hashCode</param>
        /// <param name="outCollisionCount">Out collision count</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int IndexOf(in T item, ref uint outHashCode, ref uint outCollisionCount)
        {
            uint num = 0;
            var entries = _entries;
            var hashCode = (uint)item.GetHashCode();
            var index = GetBucket(hashCode) - 1;
            while ((uint)index < (uint)_entriesLength)
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                if ((int)local.HashCode != (int)hashCode || !local.Value.Equals(item))
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
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsert(int index, in T item)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)index, (uint)_count, ExceptionArgument.index);
            return TryInsertIgnoreInsertion(index, item);
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T item)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count, ExceptionArgument.index);
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            if (item.Equals(local.Value))
                return;
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            if (IndexOf(item, ref outHashCode, ref outCollisionCount) >= 0)
                ThrowHelpers.ThrowAddingDuplicateWithKeyException(item);
            RemoveEntryFromBucket(index);
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            ++_version;
        }

        /// <summary>
        ///     Push entry into bucket
        /// </summary>
        /// <param name="entry">Entry</param>
        /// <param name="entryIndex">Entry index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void PushEntryIntoBucket(ref Entry entry, int entryIndex)
        {
            ref var local = ref GetBucket(entry.HashCode);
            entry.Next = local - 1;
            local = entryIndex + 1;
        }

        /// <summary>
        ///     Remove entry from bucket
        /// </summary>
        /// <param name="entryIndex">Entry index</param>
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
        ///     Update bucket index
        /// </summary>
        /// <param name="entryIndex">Entry index</param>
        /// <param name="shiftAmount">Shift amount</param>
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
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult TryInsertIgnoreInsertion(int index, in T item)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return InsertResult.AlreadyExists;
            if (index < 0)
                index = _count;
            var entries = _entries;
            if (_entriesLength == _count)
                return InsertResult.InsufficientCapacity;
            for (var entryIndex = _count - 1; entryIndex >= index; --entryIndex)
            {
                Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)(entryIndex + 1)) = Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex);
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Get bucket ref
        /// </summary>
        /// <param name="hashCode">HashCode</param>
        /// <returns>Bucket ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly ref int GetBucket(uint hashCode)
        {
            var buckets = _buckets;
            return ref Environment.Is64BitProcess ? ref Unsafe.Add(ref Unsafe.AsRef<int>(buckets), (nint)HashHelpers.FastMod(hashCode, (uint)_bucketsLength, _fastModMultiplier)) : ref Unsafe.Add(ref Unsafe.AsRef<int>(buckets), (nint)(hashCode % _bucketsLength));
        }

        /// <summary>
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     Next
            /// </summary>
            public int Next;

            /// <summary>
            ///     HashCode
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     Value
            /// </summary>
            public T Value;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<T> buffer, int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var entries = _entries;
            for (var index = 0; index < count; ++index)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
            return count;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Count, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var entries = _entries;
            for (var index = 0; index < _count; ++index)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref reference, (nint)index), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocOrderedHashSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MustBePinned(SR.parameter_this)]
        public Enumerator GetEnumerator() => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
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
            ///     NativeOrderedHashSet
            /// </summary>
            private readonly StackallocOrderedHashSet<T>* _handle;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(StackallocOrderedHashSet<T>* handle)
            {
                _handle = handle;
                _version = handle->_version;
                _index = 0;
                _current = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _handle;
                ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                if (_index < handle->_count)
                {
                    ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
                    _current = local.Value;
                    ++_index;
                    return true;
                }

                _current = default;
                return false;
            }

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}