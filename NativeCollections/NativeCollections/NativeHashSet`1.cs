using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeHashSet<T> : IDisposable, IEquatable<NativeHashSet<T>> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeHashSetHandle
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
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                var count = Count;
                if (count > 0)
                {
                    Unsafe.InitBlockUnaligned(Buckets, 0, (uint)(BucketsLength * sizeof(int)));
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
            public int Initialize(int capacity)
            {
                var size = HashHelpers.GetPrime(capacity);
                FreeList = -1;
                Buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(int)));
                Entries = (Entry*)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(Entry)));
                BucketsLength = size;
                EntriesLength = size;
                FastModMultiplier = nint.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
                return size;
            }

            /// <summary>
            ///     Add
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Added</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Add(in T item)
            {
                uint collisionCount = 0;
                var hashCode = item.GetHashCode();
                ref var bucket = ref GetBucketRef(hashCode);
                var i = bucket - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Value.Equals(item))
                        return false;
                    i = entry.Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                int index;
                if (FreeCount > 0)
                {
                    index = FreeList;
                    FreeCount--;
                    FreeList = -3 - Entries[FreeList].Next;
                }
                else
                {
                    var count = Count;
                    if (count == EntriesLength)
                    {
                        Resize();
                        bucket = ref GetBucketRef(hashCode);
                    }

                    index = count;
                    Count = count + 1;
                }

                ref var newEntry = ref Entries[index];
                newEntry.HashCode = hashCode;
                newEntry.Next = bucket - 1;
                newEntry.Value = item;
                bucket = index + 1;
                Version++;
                return true;
            }

            /// <summary>
            ///     Remove
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in T item)
            {
                uint collisionCount = 0;
                var last = -1;
                var hashCode = item.GetHashCode();
                ref var bucket = ref GetBucketRef(hashCode);
                var i = bucket - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Value.Equals(item))
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
            /// <param name="equalValue">Equal value</param>
            /// <param name="actualValue">Actual value</param>
            /// <returns>Removed</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Remove(in T equalValue, out T actualValue)
            {
                uint collisionCount = 0;
                var last = -1;
                var hashCode = equalValue.GetHashCode();
                ref var bucket = ref GetBucketRef(hashCode);
                var i = bucket - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
                    {
                        if (last < 0)
                            bucket = entry.Next + 1;
                        else
                            Entries[last].Next = entry.Next;
                        entry.Next = -3 - FreeList;
                        FreeList = i;
                        FreeCount++;
                        actualValue = entry.Value;
                        return true;
                    }

                    last = i;
                    i = entry.Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                actualValue = default;
                return false;
            }

            /// <summary>
            ///     Contains
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Contains</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(in T item) => FindItemIndex(item) >= 0;

            /// <summary>
            ///     Try to get the actual value
            /// </summary>
            /// <param name="equalValue">Equal value</param>
            /// <param name="actualValue">Actual value</param>
            /// <returns>Got</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(in T equalValue, out T actualValue)
            {
                var index = FindItemIndex(equalValue);
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
                var index = FindItemIndex(equalValue);
                if (index >= 0)
                {
                    actualValue = new NativeReference<T>(Unsafe.AsPointer(ref Entries[index].Value));
                    return true;
                }

                actualValue = default;
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
                var newSize = HashHelpers.GetPrime(capacity);
                Resize(newSize);
                return newSize;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess()
            {
                var capacity = Count - FreeCount;
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
                var count = 0;
                for (var i = 0; i < oldCount; ++i)
                {
                    var hashCode = oldEntries[i].HashCode;
                    if (oldEntries[i].Next >= -1)
                    {
                        ref var entry = ref newEntries[count];
                        entry = oldEntries[i];
                        ref var bucket = ref GetBucketRef(hashCode);
                        entry.Next = bucket - 1;
                        bucket = count + 1;
                        count++;
                    }
                }

                NativeMemoryAllocator.Free(oldEntries);
                Count = capacity;
                FreeCount = 0;
                return newSize;
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
                Unsafe.CopyBlockUnaligned(entries, Entries, (uint)(EntriesLength * sizeof(Entry)));
                var buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * sizeof(int)));
                NativeMemoryAllocator.Free(Buckets);
                Buckets = buckets;
                BucketsLength = newSize;
                FastModMultiplier = nint.Size == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
                for (var i = 0; i < count; ++i)
                {
                    ref var entry = ref entries[i];
                    if (entry.Next >= -1)
                    {
                        ref var bucket = ref GetBucketRef(entry.HashCode);
                        entry.Next = bucket - 1;
                        bucket = i + 1;
                    }
                }

                NativeMemoryAllocator.Free(Entries);
                Entries = entries;
                EntriesLength = newSize;
            }

            /// <summary>
            ///     Find item index
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Index</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int FindItemIndex(in T item)
            {
                uint collisionCount = 0;
                var hashCode = item.GetHashCode();
                var i = GetBucketRef(hashCode) - 1;
                while (i >= 0)
                {
                    ref var entry = ref Entries[i];
                    if (entry.HashCode == hashCode && entry.Value.Equals(item))
                        return i;
                    i = entry.Next;
                    collisionCount++;
                    if (collisionCount > (uint)EntriesLength)
                        throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }

                return -1;
            }

            /// <summary>
            ///     Get bucket ref
            /// </summary>
            /// <param name="hashCode">HashCode</param>
            /// <returns>Bucket ref</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ref int GetBucketRef(int hashCode) => ref nint.Size == 8 ? ref Buckets[HashHelpers.FastMod((uint)hashCode, (uint)BucketsLength, FastModMultiplier)] : ref Buckets[(uint)hashCode % (uint)BucketsLength];
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeHashSetHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeHashSetHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeHashSetHandle));
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
        public bool Equals(NativeHashSet<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeHashSet<T> nativeHashSet && nativeHashSet == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeHashSet<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeHashSet<T> left, NativeHashSet<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeHashSet<T> left, NativeHashSet<T> right) => left._handle != right._handle;

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
        ///     Entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>
            ///     HashCode
            /// </summary>
            public int HashCode;

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
        ///     Empty
        /// </summary>
        public static NativeHashSet<T> Empty => new();

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
            ///     NativeHashSet
            /// </summary>
            private readonly NativeHashSetHandle* _nativeHashSet;

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
            /// <param name="nativeHashSet">NativeHashSet</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeHashSet)
            {
                var handle = (NativeHashSetHandle*)nativeHashSet;
                _nativeHashSet = handle;
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
                var handle = _nativeHashSet;
                if (_version != handle->Version)
                    throw new InvalidOperationException("EnumFailedVersion");
                while ((uint)_index < (uint)handle->Count)
                {
                    ref var entry = ref handle->Entries[_index++];
                    if (entry.Next >= -1)
                    {
                        _current = entry.Value;
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
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}