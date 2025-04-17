using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe hashSet
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeHashSet<T> : IDisposable where T : unmanaged, IEquatable<T>
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
        ///     FastModMultiplier
        /// </summary>
        private ulong _fastModMultiplier;

        /// <summary>
        ///     Count
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
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _count - _freeCount == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _count - _freeCount;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _entriesLength;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeHashSet(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            this = new UnsafeHashSet<T>();
            Initialize(capacity);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_buckets);

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var count = _count;
            if (count > 0)
            {
                Unsafe.InitBlockUnaligned(_buckets, 0, (uint)(_bucketsLength * sizeof(int) + count * sizeof(Entry)));
                _count = 0;
                _freeList = -1;
                _freeCount = 0;
            }
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
            var hashCode = (uint)item.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref _entries[i];
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return false;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
            }

            int index;
            if (_freeCount > 0)
            {
                index = _freeList;
                _freeCount--;
                _freeList = -3 - _entries[_freeList].Next;
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
                _count = count + 1;
            }

            ref var newEntry = ref _entries[index];
            newEntry.HashCode = hashCode;
            newEntry.Next = bucket - 1;
            newEntry.Value = item;
            bucket = index + 1;
            _version++;
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
            var hashCode = (uint)item.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref _entries[i];
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                {
                    if (last < 0)
                        bucket = entry.Next + 1;
                    else
                        _entries[last].Next = entry.Next;
                    entry.Next = -3 - _freeList;
                    _freeList = i;
                    _freeCount++;
                    return true;
                }

                last = i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
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
            var hashCode = (uint)equalValue.GetHashCode();
            ref var bucket = ref GetBucket(hashCode);
            var i = bucket - 1;
            while (i >= 0)
            {
                ref var entry = ref _entries[i];
                if (entry.HashCode == hashCode && entry.Value.Equals(equalValue))
                {
                    if (last < 0)
                        bucket = entry.Next + 1;
                    else
                        _entries[last].Next = entry.Next;
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
                actualValue = _entries[index].Value;
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
                actualValue = new NativeReference<T>(Unsafe.AsPointer(ref _entries[index].Value));
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
            var currentCapacity = _entriesLength;
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
        public int TrimExcess() => TrimExcess(_count);

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
            var oldBuckets = _buckets;
            var oldEntries = _entries;
            var currentCapacity = _entriesLength;
            if (newSize >= currentCapacity)
                return currentCapacity;
            var oldCount = _count;
            _version++;
            Initialize(newSize);
            var newEntries = _entries;
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

            NativeMemoryAllocator.Free(oldBuckets);
            _count = newCount;
            _freeCount = 0;
            return newSize;
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
            _freeList = -1;
            _buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(size * (sizeof(int) + sizeof(Entry))));
            _entries = (Entry*)((byte*)_buckets + size * sizeof(int));
            _bucketsLength = size;
            _entriesLength = size;
            _fastModMultiplier = sizeof(nint) == 8 ? HashHelpers.GetFastModMultiplier((uint)size) : 0;
        }

        /// <summary>
        ///     Resize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize() => Resize(HashHelpers.ExpandPrime(_count));

        /// <summary>
        ///     Resize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newSize)
        {
            var oldBuckets = _buckets;
            var buckets = (int*)NativeMemoryAllocator.AllocZeroed((uint)(newSize * (sizeof(int) + sizeof(Entry))));
            var entries = (Entry*)((byte*)buckets + newSize * sizeof(int));
            var count = _count;
            Unsafe.CopyBlockUnaligned(entries, _entries, (uint)(_entriesLength * sizeof(Entry)));
            _buckets = buckets;
            _bucketsLength = newSize;
            _fastModMultiplier = sizeof(nint) == 8 ? HashHelpers.GetFastModMultiplier((uint)newSize) : 0;
            for (var i = 0; i < count; ++i)
            {
                ref var entry = ref entries[i];
                if (entry.Next >= -1)
                {
                    ref var bucket = ref GetBucket(entry.HashCode);
                    entry.Next = bucket - 1;
                    bucket = i + 1;
                }
            }

            NativeMemoryAllocator.Free(oldBuckets);
            _entries = entries;
            _entriesLength = newSize;
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
            var hashCode = (uint)item.GetHashCode();
            var i = GetBucket(hashCode) - 1;
            while (i >= 0)
            {
                ref var entry = ref _entries[i];
                if (entry.HashCode == hashCode && entry.Value.Equals(item))
                    return i;
                i = entry.Next;
                collisionCount++;
                if (collisionCount > (uint)_entriesLength)
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
        private ref int GetBucket(uint hashCode) => ref sizeof(nint) == 8 ? ref _buckets[HashHelpers.FastMod(hashCode, (uint)_bucketsLength, _fastModMultiplier)] : ref _buckets[hashCode % (uint)_bucketsLength];

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
        ///     Get byte count
        /// </summary>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetByteCount() => (_count - _freeCount) * sizeof(T);

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> buffer) => CopyTo(MemoryMarshal.Cast<T, byte>(buffer));

        /// <summary>
        ///     Copy to
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer)
        {
            ref var reference = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(buffer));
            var count = _count - _freeCount;
            var entries = _entries;
            var offset = 0;
            for (var index = 0; index < _count && count != 0; ++index)
            {
                ref var local = ref entries[index];
                if (local.Next >= -1)
                {
                    Unsafe.Add(ref reference, offset++) = local.Value;
                    --count;
                }
            }
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeHashSet<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeHashSet
            /// </summary>
            private readonly UnsafeHashSet<T>* _nativeHashSet;

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
                var handle = (UnsafeHashSet<T>*)nativeHashSet;
                _nativeHashSet = handle;
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
                var handle = _nativeHashSet;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                while ((uint)_index < (uint)handle->_count)
                {
                    ref var entry = ref handle->_entries[_index++];
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