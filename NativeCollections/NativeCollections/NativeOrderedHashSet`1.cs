using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native ordered hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe struct NativeOrderedHashSet<T> : IDisposable, IEquatable<NativeOrderedHashSet<T>> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeOrderedHashSetHandle
        {
            /// <summary>
            ///     Buckets
            /// </summary>
            public int* Buckets;

            /// <summary>
            ///     Entries
            /// </summary>
            public Entry* Entries;

            /// <summary>
            ///     BucketsLength
            /// </summary>
            public int BucketsLength;

            /// <summary>
            ///     EntriesLength
            /// </summary>
            public int EntriesLength;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     FastModMultiplier
            /// </summary>
            public ulong FastModMultiplier;

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                var count = Count;
                if (count > 0)
                {
                    Unsafe.InitBlockUnaligned(Buckets, 0, (uint)(count * sizeof(int)));
                    Unsafe.InitBlockUnaligned(Entries, 0, (uint)(count * sizeof(Entry)));
                    Count = 0;
                    ++Version;
                }
            }

            /// <summary>
            ///     Initialize
            /// </summary>
            /// <param name="capacity">Capacity</param>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize(int capacity)
            {
                var size = HashHelpers.GetPrime(capacity);
                Buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(int)));
                Entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(Entry)));
                BucketsLength = size;
                EntriesLength = size;
                FastModMultiplier = nint.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
            }

            /// <summary>
            ///     Add
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Added</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(in T item) => TryInsertIgnoreInsertion(-1, item);

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
                    var count = Count;
                    RemoveEntryFromBucket(index);
                    var entries = Entries;
                    for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                    {
                        entries[entryIndex - 1] = entries[entryIndex];
                        UpdateBucketIndex(entryIndex, -1);
                    }

                    entries[--Count] = new Entry();
                    ++Version;
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
                    actualValue = Entries[index].Value;
                    var count = Count;
                    RemoveEntryFromBucket(index);
                    var entries = Entries;
                    for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                    {
                        entries[entryIndex - 1] = entries[entryIndex];
                        UpdateBucketIndex(entryIndex, -1);
                    }

                    entries[--Count] = new Entry();
                    ++Version;
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
                var count = Count;
                if ((uint)index >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                RemoveEntryFromBucket(index);
                var entries = Entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    entries[entryIndex - 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, -1);
                }

                entries[--Count] = new Entry();
                ++Version;
            }

            /// <summary>
            ///     Remove at
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveAt(int index, out T item)
            {
                var count = Count;
                if ((uint)index >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                item = Entries[index].Value;
                RemoveEntryFromBucket(index);
                var entries = Entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    entries[entryIndex - 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, -1);
                }

                entries[--Count] = new Entry();
                ++Version;
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
                    actualValue = Entries[index].Value;
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
                    actualValue = new NativeReference<T>(Unsafe.AsPointer(ref Entries[index].Value));
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
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                ref var local = ref Entries[index];
                return local.Value;
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
                var entries = Entries;
                var hashCode = (uint)item.GetHashCode();
                var index = GetBucket(hashCode) - 1;
                while ((uint)index < (uint)EntriesLength)
                {
                    ref var local = ref entries[index];
                    if ((int)local.HashCode != (int)hashCode || !local.Value.Equals(item))
                    {
                        index = local.Next;
                        ++num;
                        if (num > (uint)EntriesLength)
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
                if ((uint)index > (uint)Count)
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
                if ((uint)index >= (uint)Count)
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
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                ref var local = ref Entries[index];
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
                ++Version;
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
                if (EntriesLength < capacity)
                {
                    Resize(HashHelpers.GetPrime(capacity));
                    ++Version;
                }

                return EntriesLength;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess() => TrimExcess(Count);

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <param name="capacity">Capacity</param>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess(int capacity)
            {
                var length = EntriesLength;
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
                var entries = Entries;
                var entry = entries[entryIndex];
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
                            ref var local2 = ref entries[index];
                            if (local2.Next == entryIndex)
                            {
                                local2.Next = entry.Next;
                                return;
                            }

                            index = local2.Next;
                        } while (++num <= EntriesLength);

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
                var entries = Entries;
                ref var local1 = ref GetBucket(entries[entryIndex].HashCode);
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
                            ref var local2 = ref entries[index];
                            if (local2.Next == entryIndex)
                            {
                                local2.Next += shiftAmount;
                                return;
                            }

                            index = local2.Next;
                        } while (++num <= EntriesLength);

                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                    }
                }
            }

            /// <summary>
            ///     Resize
            /// </summary>
            /// <param name="newSize"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Resize(int newSize)
            {
                var buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(int)));
                var entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(Entry)));
                FastModMultiplier = nint.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
                var count = Count;
                Unsafe.CopyBlockUnaligned(entries, Entries, (uint)(count * sizeof(Entry)));
                NativeMemoryAllocator.Free(Buckets);
                Buckets = buckets;
                BucketsLength = newSize;
                for (var entryIndex = 0; entryIndex < count; ++entryIndex)
                    PushEntryIntoBucket(ref entries[entryIndex], entryIndex);
                NativeMemoryAllocator.Free(Entries);
                Entries = entries;
                EntriesLength = newSize;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertIgnoreInsertion(int index, in T item)
            {
                uint outHashCode = 0;
                uint outCollisionCount = 0;
                var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
                if (index1 >= 0)
                    return false;
                if (index < 0)
                    index = Count;
                var entries = Entries;
                if (EntriesLength == Count)
                {
                    Resize(HashHelpers.ExpandPrime(EntriesLength));
                    entries = Entries;
                }

                for (var entryIndex = Count - 1; entryIndex >= index; --entryIndex)
                {
                    entries[entryIndex + 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, 1);
                }

                ref var local = ref entries[index];
                local.HashCode = outHashCode;
                local.Value = item;
                PushEntryIntoBucket(ref local, index);
                ++Count;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertThrowOnExisting(int index, in T item)
            {
                uint outHashCode = 0;
                uint outCollisionCount = 0;
                var index1 = IndexOf(item, ref outHashCode, ref outCollisionCount);
                if (index1 >= 0)
                    throw new ArgumentException($"AddingDuplicateWithItem, {item}");
                if (index < 0)
                    index = Count;
                var entries = Entries;
                if (EntriesLength == Count)
                {
                    Resize(HashHelpers.ExpandPrime(EntriesLength));
                    entries = Entries;
                }

                for (var entryIndex = Count - 1; entryIndex >= index; --entryIndex)
                {
                    entries[entryIndex + 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, 1);
                }

                ref var local = ref entries[index];
                local.HashCode = outHashCode;
                local.Value = item;
                PushEntryIntoBucket(ref local, index);
                ++Count;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Get bucket ref
            /// </summary>
            /// <param name="hashCode">HashCode</param>
            /// <returns>Bucket ref</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ref int GetBucket(uint hashCode)
            {
                var buckets = Buckets;
                return ref nint.Size == 8 ? ref buckets[(int)HashHelpers.FastMod(hashCode, (uint)BucketsLength, FastModMultiplier)] : ref buckets[hashCode % BucketsLength];
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeOrderedHashSetHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeOrderedHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeOrderedHashSetHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeOrderedHashSetHandle));
            handle->Count = 0;
            handle->Version = 0;
            handle->Initialize(capacity);
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Count == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeOrderedHashSet<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeOrderedHashSet<T> nativeOrderedDictionary && nativeOrderedDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeOrderedHashSet<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeOrderedHashSet<T> left, NativeOrderedHashSet<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeOrderedHashSet<T> left, NativeOrderedHashSet<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Buckets);
            NativeMemoryAllocator.Free(handle->Entries);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(in T item) => _handle->Add(item);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item) => _handle->Remove(item);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T equalValue, out T actualValue) => _handle->Remove(equalValue, out actualValue);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out T item) => _handle->RemoveAt(index, out item);

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => _handle->Contains(item);

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in T equalValue, out T actualValue) => _handle->TryGetValue(equalValue, out actualValue);

        /// <summary>
        ///     Try to get the actual value
        /// </summary>
        /// <param name="equalValue">Equal value</param>
        /// <param name="actualValue">Actual value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in T equalValue, out NativeReference<T> actualValue) => _handle->TryGetValueReference(equalValue, out actualValue);

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAt(int index) => _handle->GetAt(index);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item) => _handle->IndexOf(item);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item) => _handle->Insert(index, item);

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index) => _handle->SetAt(index);

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in T item) => _handle->SetAt(index, item);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

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
        ///     Empty
        /// </summary>
        public static NativeOrderedHashSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(_handle);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeOrderedHashSet
            /// </summary>
            private readonly NativeOrderedHashSetHandle* _nativeOrderedDictionary;

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
                var handle = (NativeOrderedHashSetHandle*)nativeOrderedDictionary;
                _nativeOrderedDictionary = handle;
                _version = handle->Version;
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
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                if (_index < handle->Count)
                {
                    ref var local = ref handle->Entries[_index];
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