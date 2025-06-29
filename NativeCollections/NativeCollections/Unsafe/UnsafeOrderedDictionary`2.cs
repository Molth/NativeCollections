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
    ///     Unsafe ordered dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeOrderedDictionary<TKey, TValue> : IDisposable, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeOrderedDictionary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            this = new UnsafeOrderedDictionary<TKey, TValue>();
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
        public bool TryAdd(in TKey key, in TValue value) => TryInsertIgnoreInsertion(key, value);

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
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            var count = _count;
            if ((uint)index >= (uint)count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
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
        /// <param name="keyValuePair">Key value pair</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            var count = _count;
            if ((uint)index >= (uint)count)
            {
                keyValuePair = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
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
                value = Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
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
                value = new NativeReference<TValue>(Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value));
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(in TKey key)
        {
            var index = IndexOf(key);
            return ref index >= 0 ? ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value : ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="exists">Exists</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            var index = IndexOf(key);
            if (index >= 0)
            {
                exists = true;
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value;
            }

            exists = false;
            return ref Unsafe.NullRef<TValue>();
        }

        /// <summary>
        ///     Get value ref or add default
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value;
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
            local.Key = key;
            local.Value = default;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return ref local.Value;
        }

        /// <summary>
        ///     Get value ref or add default
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="exists">Exists</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueRefOrAddDefault(in TKey key, out bool exists)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
            {
                exists = true;
                return ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value;
            }

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
            local.Key = key;
            local.Value = default;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            exists = false;
            return ref local.Value;
        }

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TKey GetKeyAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return local.Key;
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return ref local.Value;
        }

        /// <summary>
        ///     Get key at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <returns>Key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetKeyAt(int index, out TKey key)
        {
            if ((uint)index >= (uint)_count)
            {
                key = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            key = local.Key;
            return true;
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueAt(int index, out TValue value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            value = local.Value;
            return true;
        }

        /// <summary>
        ///     Get value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReferenceAt(int index, out NativeReference<TValue> value)
        {
            if ((uint)index >= (uint)_count)
            {
                value = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            value = new NativeReference<TValue>(Unsafe.AsPointer(ref local.Value));
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return new KeyValuePair<TKey, TValue>(local.Key, local.Value);
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>KeyValuePair</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public KeyValuePair<TKey, NativeReference<TValue>> GetReferenceAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            return new KeyValuePair<TKey, NativeReference<TValue>>(local.Key, new NativeReference<TValue>(Unsafe.AsPointer(ref local.Value)));
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAt(int index, out KeyValuePair<TKey, TValue> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, TValue>(local.Key, local.Value);
            return true;
        }

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetReferenceAt(int index, out KeyValuePair<TKey, NativeReference<TValue>> keyValuePair)
        {
            if ((uint)index >= (uint)_count)
            {
                keyValuePair = default;
                return false;
            }

            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
            keyValuePair = new KeyValuePair<TKey, NativeReference<TValue>>(local.Key, new NativeReference<TValue>(Unsafe.AsPointer(ref local.Value)));
            return true;
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
            var entries = _entries;
            var hashCode = (uint)key.GetHashCode();
            var index = GetBucket(hashCode) - 1;
            while ((uint)index < (uint)_entriesLength)
            {
                ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index);
                if ((int)local.HashCode != (int)hashCode || !local.Key.Equals(key))
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
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in TKey key, in TValue value)
        {
            if ((uint)index > (uint)_count)
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
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index).Value = value;
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
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index);
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
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryInsertIgnoreInsertion(in TKey key, in TValue value)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
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
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
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
                Unsafe.Add(ref Unsafe.AsRef<Entry>(_entries), (nint)index1).Value = value;
                return true;
            }

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
            local.Key = key;
            local.Value = value;
            PushEntryIntoBucket(ref local, index);
            ++_count;
            ++_version;
            return true;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryInsertThrowOnExisting(int index, in TKey key, in TValue value)
        {
            uint outHashCode = 0;
            uint outCollisionCount = 0;
            var index1 = IndexOf(key, ref outHashCode, ref outCollisionCount);
            if (index1 >= 0)
                throw new ArgumentException($"AddingDuplicateWithKey, {key}");
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
            local.Key = key;
            local.Value = value;
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
            ///     Key
            /// </summary>
            public TKey Key;

            /// <summary>
            ///     Value
            /// </summary>
            public TValue Value;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<KeyValuePair<TKey, TValue>> buffer, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            count = Math.Min(buffer.Length, Math.Min(count, _count));
            var entries = _entries;
            for (var index = 0; index < count; ++index)
                Unsafe.WriteUnaligned(ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref Unsafe.Add(ref reference, index)), new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key, Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value));
            return count;
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer), count);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<KeyValuePair<TKey, TValue>> buffer)
        {
            if (buffer.Length < Count)
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {Count}, but buffer length is {buffer.Length}.");
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var entries = _entries;
            for (var index = 0; index < _count; ++index)
                Unsafe.WriteUnaligned(ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref Unsafe.Add(ref reference, (nint)index)), new KeyValuePair<TKey, TValue>(Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key, Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value));
        }

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, KeyValuePair<TKey, TValue>>(buffer));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeOrderedDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

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
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _nativeOrderedDictionary;

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
                var handle = (UnsafeOrderedDictionary<TKey, TValue>*)nativeOrderedDictionary;
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
        public readonly struct KeyCollection : IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _nativeOrderedDictionary;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeOrderedDictionary->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeOrderedDictionary) => _nativeOrderedDictionary = (UnsafeOrderedDictionary<TKey, TValue>*)nativeOrderedDictionary;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TKey> buffer, int count)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _nativeOrderedDictionary->_count));
                var entries = _nativeOrderedDictionary->_entries;
                for (var index = 0; index < count; ++index)
                    Unsafe.WriteUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref reference, index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key);
                return count;
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TKey> buffer)
            {
                if (buffer.Length < Count)
                    throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {Count}, but buffer length is {buffer.Length}.");
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var entries = _nativeOrderedDictionary->_entries;
                for (var index = 0; index < _nativeOrderedDictionary->_count; ++index)
                    Unsafe.WriteUnaligned(ref Unsafe.As<TKey, byte>(ref Unsafe.Add(ref reference, (nint)index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Key);
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TKey>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeOrderedDictionary);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

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
                ///     NativeOrderedDictionary
                /// </summary>
                private readonly UnsafeOrderedDictionary<TKey, TValue>* _nativeOrderedDictionary;

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
                    var handle = (UnsafeOrderedDictionary<TKey, TValue>*)nativeOrderedDictionary;
                    _nativeOrderedDictionary = handle;
                    _version = handle->_version;
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
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if (_index < handle->_count)
                    {
                        ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
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
        public readonly struct ValueCollection : IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeOrderedDictionary
            /// </summary>
            private readonly UnsafeOrderedDictionary<TKey, TValue>* _nativeOrderedDictionary;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativeOrderedDictionary->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeOrderedDictionary">NativeOrderedDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeOrderedDictionary) => _nativeOrderedDictionary = (UnsafeOrderedDictionary<TKey, TValue>*)nativeOrderedDictionary;

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<TValue> buffer, int count)
            {
                if (count < 0)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                count = Math.Min(buffer.Length, Math.Min(count, _nativeOrderedDictionary->_count));
                var entries = _nativeOrderedDictionary->_entries;
                for (var index = 0; index < count; ++index)
                    Unsafe.WriteUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref reference, index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
                return count;
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int CopyTo(Span<byte> buffer, int count) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer), count);

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<TValue> buffer)
            {
                if (buffer.Length < Count)
                    throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"Requires size is {Count}, but buffer length is {buffer.Length}.");
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var entries = _nativeOrderedDictionary->_entries;
                for (var index = 0; index < _nativeOrderedDictionary->_count; ++index)
                    Unsafe.WriteUnaligned(ref Unsafe.As<TValue, byte>(ref Unsafe.Add(ref reference, (nint)index)), Unsafe.Add(ref Unsafe.AsRef<Entry>(entries), (nint)index).Value);
            }

            /// <summary>
            ///     Copy to
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CopyTo(Span<byte> buffer) => CopyTo(MemoryMarshal.Cast<byte, TValue>(buffer));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeOrderedDictionary);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

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
                ///     NativeOrderedDictionary
                /// </summary>
                private readonly UnsafeOrderedDictionary<TKey, TValue>* _nativeOrderedDictionary;

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
                    var handle = (UnsafeOrderedDictionary<TKey, TValue>*)nativeOrderedDictionary;
                    _nativeOrderedDictionary = handle;
                    _version = handle->_version;
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
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if (_index < handle->_count)
                    {
                        ref var local = ref Unsafe.Add(ref Unsafe.AsRef<Entry>(handle->_entries), (nint)_index);
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