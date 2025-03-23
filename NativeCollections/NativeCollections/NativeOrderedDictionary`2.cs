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
    [NativeCollection]
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
        }

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(this);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(this);

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
            _handle = handle;
            Initialize(capacity);
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
        public void Clear()
        {
            var handle = _handle;
            var count = handle->Count;
            if (count > 0)
            {
                Unsafe.InitBlockUnaligned(handle->Buckets, 0, (uint)(count * sizeof(int)));
                Unsafe.InitBlockUnaligned(handle->Entries, 0, (uint)(count * sizeof(Entry)));
                handle->Count = 0;
                ++handle->Version;
            }
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
            var handle = _handle;
            var index = IndexOf(key);
            if (index >= 0)
            {
                var count = handle->Count;
                RemoveEntryFromBucket(index);
                var entries = handle->Entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    entries[entryIndex - 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, -1);
                }

                entries[--handle->Count] = new Entry();
                ++handle->Version;
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
            var handle = _handle;
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = handle->Entries[index].Value;
                var count = handle->Count;
                RemoveEntryFromBucket(index);
                var entries = handle->Entries;
                for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
                {
                    entries[entryIndex - 1] = entries[entryIndex];
                    UpdateBucketIndex(entryIndex, -1);
                }

                entries[--handle->Count] = new Entry();
                ++handle->Version;
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
            var handle = _handle;
            var count = handle->Count;
            if ((uint)index >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(index));
            RemoveEntryFromBucket(index);
            var entries = handle->Entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                entries[entryIndex - 1] = entries[entryIndex];
                UpdateBucketIndex(entryIndex, -1);
            }

            entries[--handle->Count] = new Entry();
            ++handle->Version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            var handle = _handle;
            var count = handle->Count;
            if ((uint)index >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref handle->Entries[index];
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
            RemoveEntryFromBucket(index);
            var entries = handle->Entries;
            for (var entryIndex = index + 1; entryIndex < count; ++entryIndex)
            {
                entries[entryIndex - 1] = entries[entryIndex];
                UpdateBucketIndex(entryIndex, -1);
            }

            entries[--handle->Count] = new Entry();
            ++handle->Version;
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
                value = _handle->Entries[index].Value;
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
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref _handle->Entries[index].Value));
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
            var handle = _handle;
            if ((uint)index >= (uint)handle->Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref handle->Entries[index];
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
            var handle = _handle;
            uint num = 0;
            var entries = handle->Entries;
            var hashCode = (uint)key.GetHashCode();
            var index = GetBucket(hashCode) - 1;
            while ((uint)index < (uint)handle->EntriesLength)
            {
                ref var local = ref entries[index];
                if ((int)local.HashCode != (int)hashCode || !local.Key.Equals(key))
                {
                    index = local.Next;
                    ++num;
                    if (num > (uint)handle->EntriesLength)
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
            if ((uint)index > (uint)_handle->Count)
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
            var handle = _handle;
            if ((uint)index >= (uint)handle->Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            handle->Entries[index].Value = value;
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
            var handle = _handle;
            if ((uint)index >= (uint)handle->Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref handle->Entries[index];
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
            ++handle->Version;
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
            var handle = _handle;
            if (handle->EntriesLength < capacity)
            {
                Resize(HashHelpers.GetPrime(capacity));
                ++handle->Version;
            }

            return handle->EntriesLength;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => TrimExcess(_handle->Count);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            var handle = _handle;
            var length = handle->EntriesLength;
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
            var handle = _handle;
            var entries = handle->Entries;
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
                    } while (++num <= handle->EntriesLength);

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
            var handle = _handle;
            var entries = handle->Entries;
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
                    } while (++num <= handle->EntriesLength);

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
            var handle = _handle;
            var size = HashHelpers.GetPrime(capacity);
            handle->Buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(int)));
            handle->Entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(Entry)));
            handle->BucketsLength = size;
            handle->EntriesLength = size;
            handle->FastModMultiplier = IntPtr.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
        }

        /// <summary>
        ///     Resize
        /// </summary>
        /// <param name="newSize"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newSize)
        {
            var handle = _handle;
            var buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(int)));
            var entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(Entry)));
            handle->FastModMultiplier = IntPtr.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
            var count = handle->Count;
            Unsafe.CopyBlockUnaligned(entries, handle->Entries, (uint)(count * sizeof(Entry)));
            NativeMemoryAllocator.Free(handle->Buckets);
            handle->Buckets = buckets;
            handle->BucketsLength = newSize;
            for (var entryIndex = 0; entryIndex < count; ++entryIndex)
                PushEntryIntoBucket(ref entries[entryIndex], entryIndex);
            NativeMemoryAllocator.Free(handle->Entries);
            handle->Entries = entries;
            handle->EntriesLength = newSize;
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
            var handle = _handle;
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return false;
            if (index < 0)
                index = handle->Count;
            var entries = handle->Entries;
            if (handle->EntriesLength == handle->Count)
            {
                Resize(HashHelpers.ExpandPrime(handle->EntriesLength));
                entries = handle->Entries;
            }

            for (var entryIndex = handle->Count - 1; entryIndex >= index; --entryIndex)
            {
                entries[entryIndex + 1] = entries[entryIndex];
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref entries[index];
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++handle->Count;
            ++handle->Version;
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
            var handle = _handle;
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
            {
                handle->Entries[index1].Value = value;
                return true;
            }

            if (index < 0)
                index = handle->Count;
            var entries = handle->Entries;
            if (handle->EntriesLength == handle->Count)
            {
                Resize(HashHelpers.ExpandPrime(handle->EntriesLength));
                entries = handle->Entries;
            }

            for (var entryIndex = handle->Count - 1; entryIndex >= index; --entryIndex)
            {
                entries[entryIndex + 1] = entries[entryIndex];
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref entries[index];
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++handle->Count;
            ++handle->Version;
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
            var handle = _handle;
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                throw new ArgumentException($"AddingDuplicateWithKey, {key}");
            if (index < 0)
                index = handle->Count;
            var entries = handle->Entries;
            if (handle->EntriesLength == handle->Count)
            {
                Resize(HashHelpers.ExpandPrime(handle->EntriesLength));
                entries = handle->Entries;
            }

            for (var entryIndex = handle->Count - 1; entryIndex >= index; --entryIndex)
            {
                entries[entryIndex + 1] = entries[entryIndex];
                UpdateBucketIndex(entryIndex, 1);
            }

            ref var local = ref entries[index];
            local.HashCode = outHashCode;
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++handle->Count;
            ++handle->Version;
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
            var handle = _handle;
            var buckets = handle->Buckets;
            return ref IntPtr.Size == 8 ? ref buckets[(int)HashHelpers.FastMod(hashCode, (uint)handle->BucketsLength, handle->FastModMultiplier)] : ref buckets[hashCode % handle->BucketsLength];
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
        public Enumerator GetEnumerator() => new(this);

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly NativeOrderedDictionary<TKey, TValue> _nativeOrderedDictionary;

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
            internal Enumerator(NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary)
            {
                _nativeOrderedDictionary = nativeOrderedDictionary;
                _version = nativeOrderedDictionary._handle->Version;
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
                var handle = _nativeOrderedDictionary._handle;
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
            private readonly NativeOrderedDictionary<TKey, TValue> _nativeOrderedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary) => _nativeOrderedDictionary = nativeOrderedDictionary;

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
                private readonly NativeOrderedDictionary<TKey, TValue> _nativeOrderedDictionary;

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
                internal Enumerator(NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary)
                {
                    _nativeOrderedDictionary = nativeOrderedDictionary;
                    _version = nativeOrderedDictionary._handle->Version;
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
                    var handle = _nativeOrderedDictionary._handle;
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
            private readonly NativeOrderedDictionary<TKey, TValue> _nativeOrderedDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary) => _nativeOrderedDictionary = nativeOrderedDictionary;

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
                private readonly NativeOrderedDictionary<TKey, TValue> _nativeOrderedDictionary;

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
                internal Enumerator(NativeOrderedDictionary<TKey, TValue> nativeOrderedDictionary)
                {
                    _nativeOrderedDictionary = nativeOrderedDictionary;
                    _version = nativeOrderedDictionary._handle->Version;
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
                    var handle = _nativeOrderedDictionary._handle;
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