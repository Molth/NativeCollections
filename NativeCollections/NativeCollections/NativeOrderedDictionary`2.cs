using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native ordered dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeOrderedDictionary<TKey, TValue> : IDisposable, IEquatable<NativeOrderedDictionary<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeOrderedDictionaryHandle
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
            ///     Get or set value
            /// </summary>
            /// <param name="key">Key</param>
            public TValue this[in TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (!TryGetValue(key, out var obj))
                        throw new KeyNotFoundException(key.ToString());
                    return obj;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => TryInsertOverwriteExisting(-1, key, value);
            }

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
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(in TKey key, in TValue value) => TryInsertThrowOnExisting(-1, key, value);

            /// <summary>
            ///     Try add
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Added</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAdd(in TKey key, in TValue value) => TryInsertIgnoreInsertion(-1, key, value);

            /// <summary>
            ///     Remove
            /// </summary>
            /// <param name="key">Key</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in TKey key)
            {
                var index = IndexOf(key);
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
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in TKey key, out TValue value)
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    value = Entries[index].Value;
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

                value = default;
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
            /// <param name="keyValuePair">Key value pair</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
            {
                var count = Count;
                if ((uint)index >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                ref var local = ref Entries[index];
                keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
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
            ///     Contains key
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Contains key</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(in TKey key) => IndexOf(key) >= 0;

            /// <summary>
            ///     Try to get the value
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in TKey key, out TValue value)
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    value = Entries[index].Value;
                    return true;
                }

                value = default;
                return false;
            }

            /// <summary>
            ///     Try to get the value
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value)
            {
                var index = IndexOf(key);
                if (index >= 0)
                {
                    value = new NativeReference<TValue>(Unsafe.AsPointer(ref Entries[index].Value));
                    return true;
                }

                value = default;
                return false;
            }

            /// <summary>
            ///     Get at
            /// </summary>
            /// <param name="index">Index</param>
            /// <returns>KeyValuePair</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KeyValuePair<TKey, TValue> GetAt(int index)
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                ref var local = ref Entries[index];
                return new KeyValuePair<TKey, TValue>(local.Key, local.Value);
            }

            /// <summary>
            ///     Index of
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int IndexOf(in TKey key)
            {
                uint num = 0;
                return IndexOf(key, ref num, ref num);
            }

            /// <summary>
            ///     Index of
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="outHashCode">Out hashCode</param>
            /// <param name="outCollisionCount">Out collision count</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int IndexOf(in TKey key, ref uint outHashCode, ref uint outCollisionCount)
            {
                uint num = 0;
                var entries = Entries;
                var hashCode = (uint)key.GetHashCode();
                var index = GetBucket(hashCode) - 1;
                while ((uint)index < (uint)EntriesLength)
                {
                    ref var local = ref entries[index];
                    if ((int)local.HashCode != (int)hashCode || !local.Key.Equals(key))
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
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Insert(int index, in TKey key, in TValue value)
            {
                if ((uint)index > (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                TryInsertThrowOnExisting(index, key, value);
            }

            /// <summary>
            ///     Set at
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetAt(int index, in TValue value)
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                Entries[index].Value = value;
            }

            /// <summary>
            ///     Set at
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetAt(int index, in TKey key, in TValue value)
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                ref var local = ref Entries[index];
                if (key.Equals(local.Key))
                {
                    local.Value = value;
                    return;
                }

                uint outHashCode = 0;
                uint outCollisionCount = 0;
                if (IndexOf(key, ref outHashCode, ref outCollisionCount) >= 0)
                    throw new ArgumentException($"AddingDuplicateWithKey, {key}");
                RemoveEntryFromBucket(index);
                local.HashCode = outHashCode;
                local.Key = key;
                local.Value = value;
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
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertIgnoreInsertion(int index, in TKey key, in TValue value)
            {
                uint outHashCode = 0;
                uint outCollisionCount = 0;
                var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
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
                local.Key = key;
                local.Value = value;
                PushEntryIntoBucket(ref local, index);
                ++Count;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertOverwriteExisting(int index, in TKey key, in TValue value)
            {
                uint outHashCode = 0;
                uint outCollisionCount = 0;
                var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
                if (index1 >= 0)
                {
                    Entries[index1].Value = value;
                    return true;
                }

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
                local.Key = key;
                local.Value = value;
                PushEntryIntoBucket(ref local, index);
                ++Count;
                ++Version;
                return true;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertThrowOnExisting(int index, in TKey key, in TValue value)
            {
                uint outHashCode = 0;
                uint outCollisionCount = 0;
                var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
                if (index1 >= 0)
                    throw new ArgumentException($"AddingDuplicateWithKey, {key}");
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
                local.Key = key;
                local.Value = value;
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
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(_handle);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(_handle);

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeOrderedDictionaryHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeOrderedDictionary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeOrderedDictionaryHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeOrderedDictionaryHandle));
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
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[key] = value;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeOrderedDictionary<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary && nativeOrderedDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeOrderedDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeOrderedDictionary<TKey, TValue> left, NativeOrderedDictionary<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeOrderedDictionary<TKey, TValue> left, NativeOrderedDictionary<TKey, TValue> right) => left._handle != right._handle;

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
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in TKey key, in TValue value) => _handle->Add(key, value);

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in TKey key, in TValue value) => _handle->TryAdd(key, value);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key) => _handle->Remove(key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value) => _handle->Remove(key, out value);

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
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair) => _handle->RemoveAt(index, out keyValuePair);

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => _handle->ContainsKey(key);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value) => _handle->TryGetValue(key, out value);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value) => _handle->TryGetValueReference(key, out value);

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue> GetAt(int index) => _handle->GetAt(index);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in TKey key) => _handle->IndexOf(key);

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in TKey key, in TValue value) => _handle->Insert(index, key, value);

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in TValue value) => _handle->SetAt(index, value);

        /// <summary>
        ///     Set at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAt(int index, in TKey key, in TValue value) => _handle->SetAt(index, key, value);

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
            ///     Key
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeOrderedDictionary<TKey, TValue> Empty => new();

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
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly NativeOrderedDictionaryHandle* _nativeOrderedDictionary;

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
            private KeyValuePair<TKey, TValue> _current;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeOrderedDictionary)
            {
                var handle = (NativeOrderedDictionaryHandle*)nativeOrderedDictionary;
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
                    _current = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
                    ++_index;
                    return true;
                }

                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection
        {
            /// <summary>
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly NativeOrderedDictionaryHandle* _nativeOrderedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeOrderedDictionary) => _nativeOrderedDictionary = (NativeOrderedDictionaryHandle*)nativeOrderedDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeOrderedDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeOrderedDictionary
                /// </summary>
                private readonly NativeOrderedDictionaryHandle* _nativeOrderedDictionary;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Current
                /// </summary>
                private TKey _currentKey;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeOrderedDictionary)
                {
                    var handle = (NativeOrderedDictionaryHandle*)nativeOrderedDictionary;
                    _nativeOrderedDictionary = handle;
                    _version = handle->Version;
                    _index = 0;
                    _currentKey = default;
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
                        _currentKey = local.Key;
                        ++_index;
                        return true;
                    }

                    _currentKey = default;
                    return false;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TKey Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _currentKey;
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection
        {
            /// <summary>
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly NativeOrderedDictionaryHandle* _nativeOrderedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeOrderedDictionary) => _nativeOrderedDictionary = (NativeOrderedDictionaryHandle*)nativeOrderedDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeOrderedDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeOrderedDictionary
                /// </summary>
                private readonly NativeOrderedDictionaryHandle* _nativeOrderedDictionary;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Current
                /// </summary>
                private TValue _currentValue;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeOrderedDictionary)
                {
                    var handle = (NativeOrderedDictionaryHandle*)nativeOrderedDictionary;
                    _nativeOrderedDictionary = handle;
                    _version = handle->Version;
                    _index = 0;
                    _currentValue = default;
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
                        _currentValue = local.Value;
                        ++_index;
                        return true;
                    }

                    _currentValue = default;
                    return false;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _currentValue;
                }
            }
        }
    }
}