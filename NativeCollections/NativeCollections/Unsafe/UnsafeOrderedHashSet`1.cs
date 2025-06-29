using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe ordered hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeOrderedHashSet<T> : IDisposable, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Buckets
        /// </summary>
        private int* _buckets;

        /// <summary>
        ///     Entries
        /// </summary>
        private Entry* _entries;

        /// <summary>
        ///     BucketsLength
        /// </summary>
        private int _bucketsLength;

        /// <summary>
        ///     EntriesLength
        /// </summary>
        private int _entriesLength;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     FastModMultiplier
        /// </summary>
        private ulong _fastModMultiplier;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _entriesLength;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeOrderedHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            this = new UnsafeOrderedHashSet<T>();
            Initialize(capacity);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_buckets);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var count = _count;
            if (count > 0)
            {
                Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_buckets), 0, (uint)(count * sizeof(int)));
                Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_entries), 0, (uint)(count * sizeof(Entry)));
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
        public bool Add(in T item) => TryInsertIgnoreInsertion(item);

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
            if ((uint)index >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(index));
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
            if ((uint)index >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(index));
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
        public bool Contains(in T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in T equalValue, out T actualValue)
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
        public bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue)
        {
            var index = IndexOf(equalValue);
            if (index >= 0)
            {
                actualValue = new NativeReference<T>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value));
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
        public T GetAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return local.Value;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAt(int index, out T item)
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
        public int IndexOf(in T item)
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
        private int IndexOf(in T item, ref uint outHashCode, ref uint outCollisionCount)
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
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
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
        public void Insert(int index, in T item)
        {
            if ((uint)index > (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            TryInsertThrowOnExisting(index, item);
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T item)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            if (item.Equals(local.Value))
                return;
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            if (IndexOf(item, ref outHashCode, ref outCollisionCount) >= 0)
                throw new ArgumentException($"AddingDuplicateWithItem, {item}");
            RemoveEntryFromBucket(index);
            local.HashCode = outHashCode;
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            ++_version;
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (_entriesLength < capacity)
            {
                Resize(HashHelpers.GetPrime(capacity));
                ++_version;
            }

            return _entriesLength;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => TrimExcess(_count);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
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
        ///     Push entry into bucket
        /// </summary>
        /// <param name="entry">Entry</param>
        /// <param name="entryIndex">Entry index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushEntryIntoBucket(ref Entry entry, int entryIndex)
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
        private void RemoveEntryFromBucket(int entryIndex)
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

                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
            }
        }

        /// <summary>
        ///     Update bucket index
        /// </summary>
        /// <param name="entryIndex">Entry index</param>
        /// <param name="shiftAmount">Shift amount</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBucketIndex(int entryIndex, int shiftAmount)
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

                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
            }
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize(int capacity)
        {
            var size = HashHelpers.GetPrime(capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(size * sizeof(int)), alignment);
            _buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(bucketsByteCount + size * sizeof(Entry)), alignment);
            _entries = UnsafeHelpers.AddByteOffset<Entry>(_buckets, (nint)bucketsByteCount);
            _bucketsLength = size;
            _entriesLength = size;
            _fastModMultiplier = sizeof(nint) == 8 ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
        }

        /// <summary>
        ///     Resize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newSize)
        {
            var oldBuckets = _buckets;
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<int>(), NativeMemoryAllocator.AlignOf<Entry>());
            var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(newSize * sizeof(int)), alignment);
            var buckets = (int*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(bucketsByteCount + newSize * sizeof(Entry)), alignment);
            var entries = UnsafeHelpers.AddByteOffset<Entry>(buckets, (nint)bucketsByteCount);
            var count = _count;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(entries), ref Unsafe.AsRef<byte>(_entries), (uint)(count * sizeof(Entry)));
            _buckets = buckets;
            _bucketsLength = newSize;
            _fastModMultiplier = sizeof(nint) == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
            for (var entryIndex = 0; entryIndex < count; ++entryIndex)
                PushEntryIntoBucket(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)entryIndex), entryIndex);
            NativeMemoryAllocator.AlignedFree(oldBuckets);
            _entries = entries;
            _entriesLength = newSize;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertIgnoreInsertion(in T item)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
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
            return true;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertThrowOnExisting(int index, in T item)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                throw new ArgumentException($"AddingDuplicateWithItem, {item}");
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
            local.Value = item;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
        }

        /// <summary>
        ///     Get bucket ref
        /// </summary>
        /// <param name="hashCode">HashCode</param>
        /// <returns>Bucket ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref int GetBucket(uint hashCode)
        {
            var buckets = _buckets;
            return ref sizeof(nint) == 8 ? ref Unsafe.Add(ref Unsafe.AsRef<int>(buckets), (nint)HashHelpers.FastMod(hashCode, (uint)_bucketsLength, _fastModMultiplier)) : ref Unsafe.Add(ref Unsafe.AsRef<int>(buckets), (nint)(hashCode % _bucketsLength));
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
        public int CopyTo(Span<T> buffer, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var entries = _entries;
            for (var index = 0; index < count; ++index)
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
            return count;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> buffer)
        {
            if (buffer.Length < Count)
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {Count}, but buffer length is {buffer.Length}.");
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var entries = _entries;
            for (var index = 0; index < _count; ++index)
                Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref reference, (nint)index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, T>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeOrderedHashSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeOrderedHashSet
            /// </summary>
            private readonly UnsafeOrderedHashSet<T>* _nativeOrderedDictionary;

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
            /// <param name="nativeOrderedDictionary">NativeOrderedHashSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeOrderedDictionary)
            {
                var handle = (UnsafeOrderedHashSet<T>*)nativeOrderedDictionary;
                _nativeOrderedDictionary = handle;
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
                var handle = _nativeOrderedDictionary;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
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
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}