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
    ///     Native dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeDictionary<TKey, TValue> : IDisposable, IEquatable<NativeDictionary<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDictionaryHandle
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
            ///     FastModMultiplier
            /// </summary>
            public ulong FastModMultiplier;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count;

            /// <summary>
            ///     FreeList
            /// </summary>
            public int FreeList;

            /// <summary>
            ///     FreeCount
            /// </summary>
            public int FreeCount;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     Get or set value
            /// </summary>
            /// <param name="key">Key</param>
            public TValue this[in TKey key]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ref var value = ref FindValue(key);
                    if (Unsafe.AsPointer(ref Unsafe.AsRef(in value)) != null)
                        return value;
                    throw new KeyNotFoundException(key.ToString());
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => TryInsertOverwriteExisting(key, value);
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
                    Count = 0;
                    FreeList = -1;
                    FreeCount = 0;
                    Unsafe.InitBlockUnaligned(Entries, 0, (uint)(count * sizeof(Entry)));
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
                FreeList = -1;
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
            public void Add(in TKey key, in TValue value) => TryInsertThrowOnExisting(key, value);

            /// <summary>
            ///     Try add
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Added</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAdd(in TKey key, in TValue value) => TryInsertNone(key, value);

            /// <summary>
            ///     Remove
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in TKey key)
            {
                uint collisionCount = 0;
                var hashCode = (uint)key.GetHashCode();
                ref var bucket = ref GetBucket(hashCode);
                var last = -1;
                var i = bucket - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    {
                        if (last < 0)
                            bucket = entry.Next + 1;
                        else
                            Entries[last].Next = entry.Next;
                        entry.Next = -3 - FreeList;
                        FreeList = i;
                        FreeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                return false;
            }

            /// <summary>
            ///     Remove
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in TKey key, out TValue value)
            {
                uint collisionCount = 0;
                var hashCode = (uint)key.GetHashCode();
                ref var bucket = ref GetBucket(hashCode);
                var last = -1;
                var i = bucket - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Key.Equals(key))
                    {
                        if (last < 0)
                            bucket = entry.Next + 1;
                        else
                            Entries[last].Next = entry.Next;
                        value = entry.Value;
                        entry.Next = -3 - FreeList;
                        FreeList = i;
                        FreeCount++;
                        return true;
                    }

                    last = i;
                    i = entry.Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                value = default;
                return false;
            }

            /// <summary>
            ///     Contains key
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Contains key</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsKey(in TKey key) => Unsafe.AsPointer(ref Unsafe.AsRef(in FindValue(key))) != null;

            /// <summary>
            ///     Try to get the value
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in TKey key, out TValue value)
            {
                ref var valRef = ref FindValue(key);
                if (Unsafe.AsPointer(ref Unsafe.AsRef(in valRef)) != null)
                {
                    value = valRef;
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
                ref var valRef = ref FindValue(key);
                if (Unsafe.AsPointer(ref Unsafe.AsRef(in valRef)) != null)
                {
                    value = new NativeReference<TValue>(Unsafe.AsPointer(ref valRef));
                    return true;
                }

                value = default;
                return false;
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
                var currentCapacity = EntriesLength;
                if (currentCapacity >= capacity)
                    return currentCapacity;
                Version++;
                var newSize = HashHelpers.GetPrime(capacity);
                Resize(newSize);
                return newSize;
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
                if (capacity < 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
                var newSize = HashHelpers.GetPrime(capacity);
                var oldEntries = Entries;
                var currentCapacity = EntriesLength;
                if (newSize >= currentCapacity)
                    return currentCapacity;
                var oldCount = Count;
                Version++;
                NativeMemoryAllocator.Free(Buckets);
                Initialize(newSize);
                var newEntries = Entries;
                var newCount = 0;
                for (var i = 0; i < oldCount; ++i)
                {
                    var hashCode = oldEntries[i].HashCode;
                    if (oldEntries[i].Next >= -1)
                    {
                        ref var entry = ref newEntries[newCount];
                        entry = oldEntries[i];
                        ref var bucket = ref GetBucket(hashCode);
                        entry.Next = bucket - 1;
                        bucket = newCount + 1;
                        newCount++;
                    }
                }

                NativeMemoryAllocator.Free(oldEntries);
                Count = newCount;
                FreeCount = 0;
                return newSize;
            }

            /// <summary>
            ///     Find value
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>Value</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ref TValue FindValue(in TKey key)
            {
                var hashCode = (uint)key.GetHashCode();
                var i = GetBucket(hashCode);
                uint collisionCount = 0;
                i--;
                do
                {
                    if ((uint)i >= (uint)EntriesLength)
                        return ref Unsafe.AsRef<TValue>(null);
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Key.Equals(key))
                        return ref entry.Value;
                    i = entry.Next;
                    collisionCount++;
                } while (collisionCount <= (uint)EntriesLength);

                throw new InvalidOperationException("ConcurrentOperationsNotSupported");
            }

            /// <summary>
            ///     Resize
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Resize() => Resize(HashHelpers.ExpandPrime(Count));

            /// <summary>
            ///     Resize
            /// </summary>
            /// <param name="newSize"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Resize(int newSize)
            {
                var entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(Entry)));
                var count = Count;
                Unsafe.CopyBlockUnaligned(entries, Entries, (uint)(count * sizeof(Entry)));
                var buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(int)));
                NativeMemoryAllocator.Free(Buckets);
                Buckets = buckets;
                BucketsLength = newSize;
                FastModMultiplier = nint.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
                for (var i = 0; i < count; ++i)
                {
                    if (entries[i].Next >= -1)
                    {
                        ref var bucket = ref GetBucket(entries[i].HashCode);
                        entries[i].Next = bucket - 1;
                        bucket = i + 1;
                    }
                }

                NativeMemoryAllocator.Free(Entries);
                Entries = entries;
                EntriesLength = newSize;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void TryInsertOverwriteExisting(in TKey key, in TValue value)
            {
                var hashCode = (uint)key.GetHashCode();
                uint collisionCount = 0;
                ref var bucket = ref GetBucket(hashCode);
                var i = bucket - 1;
                while (true)
                {
                    if ((uint)i >= (uint)EntriesLength)
                        break;
                    if (Entries[i].HashCode == hashCode && Entries[i].Key.Equals(key))
                    {
                        Entries[i].Value = value;
                        return;
                    }

                    i = Entries[i].Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                int index;
                if (FreeCount > 0)
                {
                    index = FreeList;
                    FreeList = -3 - Entries[FreeList].Next;
                    FreeCount--;
                }
                else
                {
                    var count = Count;
                    if (count == EntriesLength)
                    {
                        Resize();
                        bucket = ref GetBucket(hashCode);
                    }

                    index = count;
                    Count = count + 1;
                }

                ref var entry = ref Entries[index];
                entry.HashCode = hashCode;
                entry.Next = bucket - 1;
                entry.Key = key;
                entry.Value = value;
                bucket = index + 1;
                Version++;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertThrowOnExisting(in TKey key, in TValue value)
            {
                var hashCode = (uint)key.GetHashCode();
                uint collisionCount = 0;
                ref var bucket = ref GetBucket(hashCode);
                var i = bucket - 1;
                while (true)
                {
                    if ((uint)i >= (uint)EntriesLength)
                        break;
                    if (Entries[i].HashCode == hashCode && Entries[i].Key.Equals(key))
                        throw new ArgumentException($"AddingDuplicateWithKey, {key}");
                    i = Entries[i].Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                int index;
                if (FreeCount > 0)
                {
                    index = FreeList;
                    FreeList = -3 - Entries[FreeList].Next;
                    FreeCount--;
                }
                else
                {
                    var count = Count;
                    if (count == EntriesLength)
                    {
                        Resize();
                        bucket = ref GetBucket(hashCode);
                    }

                    index = count;
                    Count = count + 1;
                }

                ref var entry = ref Entries[index];
                entry.HashCode = hashCode;
                entry.Next = bucket - 1;
                entry.Key = key;
                entry.Value = value;
                bucket = index + 1;
                Version++;
                return true;
            }

            /// <summary>
            ///     Insert
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="value">Value</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool TryInsertNone(in TKey key, in TValue value)
            {
                var hashCode = (uint)key.GetHashCode();
                uint collisionCount = 0;
                ref var bucket = ref GetBucket(hashCode);
                var i = bucket - 1;
                while (true)
                {
                    if ((uint)i >= (uint)EntriesLength)
                        break;
                    if (Entries[i].HashCode == hashCode && Entries[i].Key.Equals(key))
                        return false;
                    i = Entries[i].Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                int index;
                if (FreeCount > 0)
                {
                    index = FreeList;
                    FreeList = -3 - Entries[FreeList].Next;
                    FreeCount--;
                }
                else
                {
                    var count = Count;
                    if (count == EntriesLength)
                    {
                        Resize();
                        bucket = ref GetBucket(hashCode);
                    }

                    index = count;
                    Count = count + 1;
                }

                ref var entry = ref Entries[index];
                entry.HashCode = hashCode;
                entry.Next = bucket - 1;
                entry.Key = key;
                entry.Value = value;
                bucket = index + 1;
                Version++;
                return true;
            }

            /// <summary>
            ///     Get bucket ref
            /// </summary>
            /// <param name="hashCode">HashCode</param>
            /// <returns>Bucket ref</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ref int GetBucket(uint hashCode) => ref nint.Size == 8 ? ref Buckets[HashHelpers.FastMod(hashCode, (uint)BucketsLength, FastModMultiplier)] : ref Buckets[hashCode % BucketsLength];
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeDictionaryHandle* _handle;

        /// <summary>
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(_handle);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(_handle);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDictionary(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeDictionaryHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeDictionaryHandle));
            handle->Count = 0;
            handle->FreeCount = 0;
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
        public bool IsEmpty
        {
            get
            {
                var handle = _handle;
                return handle->Count - handle->FreeCount == 0;
            }
        }

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
        public int Count
        {
            get
            {
                var handle = _handle;
                return handle->Count - handle->FreeCount;
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeDictionary<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeDictionary<TKey, TValue> nativeDictionary && nativeDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeDictionary<TKey, TValue> left, NativeDictionary<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeDictionary<TKey, TValue> left, NativeDictionary<TKey, TValue> right) => left._handle != right._handle;

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
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key) => _handle->Remove(key);

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in TKey key, out TValue value) => _handle->Remove(key, out value);

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
            ///     HashCode
            /// </summary>
            public uint HashCode;

            /// <summary>
            ///     Next
            /// </summary>
            public int Next;

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
        public static NativeDictionary<TKey, TValue> Empty => new();

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
            ///     NativeDictionary
            /// </summary>
            private readonly NativeDictionary<TKey, TValue>.NativeDictionaryHandle* _nativeDictionary;

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
            /// <param name="nativeDictionary">NativeDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeDictionary)
            {
                var handle = (NativeDictionaryHandle*)nativeDictionary;
                _nativeDictionary = handle;
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
                var handle = _nativeDictionary;
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                while ((uint)_index < (uint)handle->Count)
                {
                    ref var entry = ref handle->Entries[_index++];
                    if (entry.Next >= -1)
                    {
                        _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                        return true;
                    }
                }

                _index = handle->Count + 1;
                _current = default;
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
            ///     NativeDictionary
            /// </summary>
            private readonly NativeDictionary<TKey, TValue>.NativeDictionaryHandle* _nativeDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeDictionary">NativeDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(void* nativeDictionary) => _nativeDictionary = (NativeDictionaryHandle*)nativeDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeDictionary
                /// </summary>
                private readonly NativeDictionary<TKey, TValue>.NativeDictionaryHandle* _nativeDictionary;

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
                /// <param name="nativeDictionary">NativeDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeDictionary)
                {
                    var handle = (NativeDictionaryHandle*)nativeDictionary;
                    _nativeDictionary = handle;
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
                    var handle = _nativeDictionary;
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    while ((uint)_index < (uint)handle->Count)
                    {
                        ref var entry = ref handle->Entries[_index++];
                        if (entry.Next >= -1)
                        {
                            _currentKey = entry.Key;
                            return true;
                        }
                    }

                    _index = handle->Count + 1;
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
            ///     NativeDictionary
            /// </summary>
            private readonly NativeDictionary<TKey, TValue>.NativeDictionaryHandle* _nativeDictionary;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeDictionary">NativeDictionary</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(void* nativeDictionary) => _nativeDictionary = (NativeDictionaryHandle*)nativeDictionary;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativeDictionary);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativeDictionary
                /// </summary>
                private readonly NativeDictionary<TKey, TValue>.NativeDictionaryHandle* _nativeDictionary;

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
                /// <param name="nativeDictionary">NativeDictionary</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativeDictionary)
                {
                    var handle = (NativeDictionaryHandle*)nativeDictionary;
                    _nativeDictionary = handle;
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
                    var handle = _nativeDictionary;
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    while ((uint)_index < (uint)handle->Count)
                    {
                        ref var entry = ref handle->Entries[_index++];
                        if (entry.Next >= -1)
                        {
                            _currentValue = entry.Value;
                            return true;
                        }
                    }

                    _index = handle->Count + 1;
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