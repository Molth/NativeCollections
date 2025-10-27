// com.unity.collections copyright © 2024 Unity Technologies
// Licensed under the Unity Companion License for Unity-dependent projects (see https://unity3d.com/legal/licenses/unity_companion_license).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS1591

// Resharper disable ALL

namespace NativeCollections
{
    public struct NativeParallelMultiHashMapIterator<TKey> where TKey : unmanaged
    {
        internal TKey key;
        internal int NextEntryIndex;
        internal int EntryIndex;

        /// <summary>
        ///     Returns the entry index.
        /// </summary>
        /// <returns>The entry index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntryIndex() => EntryIndex;
    }

    public unsafe struct UnsafeParallelHashMapBucketData
    {
        internal UnsafeParallelHashMapBucketData(byte* v, byte* k, byte* n, byte* b, int bcm)
        {
            values = v;
            keys = k;
            next = n;
            buckets = b;
            bucketCapacityMask = bcm;
        }

        /// <summary>
        ///     The buffer of values.
        /// </summary>
        /// <value>The buffer of values.</value>
        public readonly byte* values;

        /// <summary>
        ///     The buffer of keys.
        /// </summary>
        /// <value>The buffer of keys.</value>
        public readonly byte* keys;

        /// <summary>
        ///     The next bucket in the chain.
        /// </summary>
        /// <value>The next bucket in the chain.</value>
        public readonly byte* next;

        /// <summary>
        ///     The first bucket in the chain.
        /// </summary>
        /// <value>The first bucket in the chain.</value>
        public readonly byte* buckets;

        /// <summary>
        ///     One less than the bucket capacity.
        /// </summary>
        /// <value>One less than the bucket capacity.</value>
        public readonly int bucketCapacityMask;
    }

    public struct NativeKeyValueArrays<TKey, TValue> where TKey : unmanaged where TValue : unmanaged
    {
        /// <summary>
        ///     The keys.
        /// </summary>
        /// <value>The keys. The key at `Keys[i]` is paired with the value at `Values[i]`.</value>
        public NativeArray<TKey> Keys;

        /// <summary>
        ///     The values.
        /// </summary>
        /// <value>The values. The value at `Values[i]` is paired with the key at `Keys[i]`.</value>
        public NativeArray<TValue> Values;

        /// <summary>
        ///     The number of key-value pairs.
        /// </summary>
        /// <value>The number of key-value pairs.</value>
        public int Length => Keys.Length;

        /// <summary>
        ///     Initializes and returns an instance of NativeKeyValueArrays.
        /// </summary>
        /// <param name="length">The number of keys-value pairs.</param>
        public NativeKeyValueArrays(int length)
        {
            Keys = new(length);
            Values = new(length);
        }

        /// <summary>
        ///     Releases all resources (memory and safety handles).
        /// </summary>
        public void Dispose()
        {
            Keys.Dispose();
            Values.Dispose();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct UnsafeParallelHashMapData
    {
        [FieldOffset(0)] internal byte* values;
        // 4-byte padding on 32-bit architectures here

        [FieldOffset(8)] internal byte* keys;
        // 4-byte padding on 32-bit architectures here

        [FieldOffset(16)] internal byte* next;
        // 4-byte padding on 32-bit architectures here

        [FieldOffset(24)] internal byte* buckets;
        // 4-byte padding on 32-bit architectures here

        [FieldOffset(32)] internal int keyCapacity;

        [FieldOffset(36)] internal int bucketCapacityMask; // = bucket capacity - 1

        [FieldOffset(40)] internal int allocatedIndexLength;

        const int kFirstFreeTLSOffset = JobsUtility.CacheLineSize < 64 ? 64 : JobsUtility.CacheLineSize;
        internal int* firstFreeTLS => (int*)((byte*)Unsafe.AsPointer(ref this) + kFirstFreeTLSOffset);

        // 64 is the cache line size on x86, arm usually has 32 - so it is possible to save some memory there
        internal const int IntsPerCacheLine = JobsUtility.CacheLineSize / sizeof(int);


        internal static int GetBucketSize(int capacity)
        {
            return capacity * 2;
        }

        internal static int GrowCapacity(int capacity)
        {
            if (capacity == 0)
            {
                return 1;
            }

            return capacity * 2;
        }

        internal static void AllocateHashMap<TKey, TValue>(int length, int bucketLength, out UnsafeParallelHashMapData* outBuf) where TKey : unmanaged where TValue : unmanaged
        {
            int maxThreadCount = JobsUtility.ThreadIndexCount;
            int hashMapDataSize = kFirstFreeTLSOffset + (sizeof(int) * IntsPerCacheLine * maxThreadCount);
            UnsafeParallelHashMapData* data = (UnsafeParallelHashMapData*)NativeMemoryAllocator.AlignedAlloc((uint)hashMapDataSize, JobsUtility.CacheLineSize);
            bucketLength = (int)BitOperationsHelpers.RoundUpToPowerOf2((uint)bucketLength);
            data->keyCapacity = length;
            data->bucketCapacityMask = bucketLength - 1;
            int keyOffset, nextOffset, bucketOffset;
            int totalSize = CalculateDataSize<TKey, TValue>(length, bucketLength, out keyOffset, out nextOffset, out bucketOffset);
            data->values = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)totalSize, JobsUtility.CacheLineSize);
            data->keys = data->values + keyOffset;
            data->next = data->values + nextOffset;
            data->buckets = data->values + bucketOffset;

            outBuf = data;
        }

        internal static void ReallocateHashMap<TKey, TValue>(UnsafeParallelHashMapData* data, int newCapacity, int newBucketCapacity) where TKey : unmanaged where TValue : unmanaged
        {
            newBucketCapacity = (int)BitOperationsHelpers.RoundUpToPowerOf2((uint)newBucketCapacity);

            if (data->keyCapacity == newCapacity && (data->bucketCapacityMask + 1) == newBucketCapacity)
            {
                return;
            }

            CheckHashMapReallocateDoesNotShrink(data, newCapacity);

            int keyOffset, nextOffset, bucketOffset;
            int totalSize = CalculateDataSize<TKey, TValue>(newCapacity, newBucketCapacity, out keyOffset, out nextOffset, out bucketOffset);

            byte* newData = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)totalSize, JobsUtility.CacheLineSize);
            byte* newKeys = newData + keyOffset;
            byte* newNext = newData + nextOffset;
            byte* newBuckets = newData + bucketOffset;

            // The items are taken from a free-list and might not be tightly packed, copy all of the old capcity
            Unsafe.CopyBlockUnaligned(newData, data->values, (uint)(data->keyCapacity * Unsafe.SizeOf<TValue>()));
            Unsafe.CopyBlockUnaligned(newKeys, data->keys, (uint)(data->keyCapacity * Unsafe.SizeOf<TKey>()));
            Unsafe.CopyBlockUnaligned(newNext, data->next, (uint)(data->keyCapacity * Unsafe.SizeOf<int>()));

            for (int emptyNext = data->keyCapacity; emptyNext < newCapacity; ++emptyNext)
            {
                ((int*)newNext)[emptyNext] = -1;
            }

            // re-hash the buckets, first clear the new bucket list, then insert all values from the old list
            for (int bucket = 0; bucket < newBucketCapacity; ++bucket)
            {
                ((int*)newBuckets)[bucket] = -1;
            }

            for (int bucket = 0; bucket <= data->bucketCapacityMask; ++bucket)
            {
                int* buckets = (int*)data->buckets;
                int* nextPtrs = (int*)newNext;
                while (buckets[bucket] >= 0)
                {
                    int curEntry = buckets[bucket];
                    buckets[bucket] = nextPtrs[curEntry];
                    int newBucket = Unsafe.Read<TKey>(data->keys + curEntry).GetHashCode() & (newBucketCapacity - 1);
                    nextPtrs[curEntry] = ((int*)newBuckets)[newBucket];
                    ((int*)newBuckets)[newBucket] = curEntry;
                }
            }

            NativeMemoryAllocator.AlignedFree(data->values);
            if (data->allocatedIndexLength > data->keyCapacity)
            {
                data->allocatedIndexLength = data->keyCapacity;
            }

            data->values = newData;
            data->keys = newKeys;
            data->next = newNext;
            data->buckets = newBuckets;
            data->keyCapacity = newCapacity;
            data->bucketCapacityMask = newBucketCapacity - 1;
        }


        internal static void DeallocateHashMap(UnsafeParallelHashMapData* data)
        {
            NativeMemoryAllocator.AlignedFree(data->values);
            NativeMemoryAllocator.AlignedFree(data);
        }

        internal static int CalculateDataSize<TKey, TValue>(int length, int bucketLength, out int keyOffset, out int nextOffset, out int bucketOffset) where TKey : unmanaged where TValue : unmanaged
        {
            var sizeOfTValue = Unsafe.SizeOf<TValue>();
            var sizeOfTKey = Unsafe.SizeOf<TKey>();
            var sizeOfInt = Unsafe.SizeOf<int>();

            var valuesSize = NativeMemoryAllocator.AlignUp((nuint)(sizeOfTValue * length), JobsUtility.CacheLineSize);
            var keysSize = NativeMemoryAllocator.AlignUp((nuint)(sizeOfTKey * length), JobsUtility.CacheLineSize);
            var nextSize = NativeMemoryAllocator.AlignUp((nuint)(sizeOfInt * length), JobsUtility.CacheLineSize);
            var bucketSize = NativeMemoryAllocator.AlignUp((nuint)(sizeOfInt * bucketLength), JobsUtility.CacheLineSize);
            var totalSize = valuesSize + keysSize + nextSize + bucketSize;

            keyOffset = (int)valuesSize;
            nextOffset = keyOffset + (int)keysSize;
            bucketOffset = nextOffset + (int)nextSize;

            return (int)totalSize;
        }

        internal static bool IsEmpty(UnsafeParallelHashMapData* data)
        {
            if (data->allocatedIndexLength <= 0)
            {
                return true;
            }

            var bucketArray = (int*)data->buckets;
            var bucketNext = (int*)data->next;
            var capacityMask = data->bucketCapacityMask;

            for (int i = 0; i <= capacityMask; ++i)
            {
                int bucket = bucketArray[i];

                if (bucket != -1)
                {
                    return false;
                }
            }

            return true;
        }

        internal static int GetCount(UnsafeParallelHashMapData* data)
        {
            if (data->allocatedIndexLength <= 0)
            {
                return 0;
            }

            var bucketNext = (int*)data->next;
            var freeListSize = 0;

            int maxThreadCount = JobsUtility.ThreadIndexCount;
            for (int tls = 0; tls < maxThreadCount; ++tls)
            {
                for (var freeIdx = data->firstFreeTLS[tls * IntsPerCacheLine];
                     freeIdx >= 0;
                     freeIdx = bucketNext[freeIdx]
                    )
                {
                    ++freeListSize;
                }
            }

            return Math.Min(data->keyCapacity, data->allocatedIndexLength) - freeListSize;
        }

        internal static bool MoveNextSearch(UnsafeParallelHashMapData* data, ref int bucketIndex, ref int nextIndex, out int index)
        {
            var bucketArray = (int*)data->buckets;
            var capacityMask = data->bucketCapacityMask;
            for (int i = bucketIndex; i <= capacityMask; ++i)
            {
                var idx = bucketArray[i];

                if (idx != -1)
                {
                    var bucketNext = (int*)data->next;
                    index = idx;
                    bucketIndex = i + 1;
                    nextIndex = bucketNext[idx];

                    return true;
                }
            }

            index = -1;
            bucketIndex = capacityMask + 1;
            nextIndex = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool MoveNext(UnsafeParallelHashMapData* data, ref int bucketIndex, ref int nextIndex, out int index)
        {
            if (nextIndex != -1)
            {
                var bucketNext = (int*)data->next;
                index = nextIndex;
                nextIndex = bucketNext[nextIndex];
                return true;
            }

            return MoveNextSearch(data, ref bucketIndex, ref nextIndex, out index);
        }

        internal static void GetKeyArray<TKey>(UnsafeParallelHashMapData* data, NativeArray<TKey> result)
            where TKey : unmanaged
        {
            var bucketArray = (int*)data->buckets;
            var bucketNext = (int*)data->next;

            for (int i = 0, count = 0, max = result.Length; i <= data->bucketCapacityMask && count < max; ++i)
            {
                int bucket = bucketArray[i];

                while (bucket != -1)
                {
                    result[count++] = Unsafe.Read<TKey>(data->keys + bucket);
                    bucket = bucketNext[bucket];
                }
            }
        }

        internal static void GetValueArray<TValue>(UnsafeParallelHashMapData* data, NativeArray<TValue> result) where TValue : unmanaged
        {
            var bucketArray = (int*)data->buckets;
            var bucketNext = (int*)data->next;

            for (int i = 0, count = 0, max = result.Length, capacityMask = data->bucketCapacityMask; i <= capacityMask && count < max; ++i)
            {
                int bucket = bucketArray[i];

                while (bucket != -1)
                {
                    result[count++] = Unsafe.Read<TValue>(data->values + bucket);
                    bucket = bucketNext[bucket];
                }
            }
        }

        internal static void GetKeyValueArrays<TKey, TValue>(UnsafeParallelHashMapData* data, NativeKeyValueArrays<TKey, TValue> result)
            where TKey : unmanaged
            where TValue : unmanaged
        {
            var bucketArray = (int*)data->buckets;
            var bucketNext = (int*)data->next;

            for (int i = 0, count = 0, max = result.Length, capacityMask = data->bucketCapacityMask;
                 i <= capacityMask && count < max;
                 ++i
                )
            {
                int bucket = bucketArray[i];

                while (bucket != -1)
                {
                    result.Keys[count] = Unsafe.Read<TKey>(data->keys + bucket);
                    result.Values[count] = Unsafe.Read<TValue>(data->values + bucket);
                    count++;
                    bucket = bucketNext[bucket];
                }
            }
        }

        internal UnsafeParallelHashMapBucketData GetBucketData()
        {
            return new UnsafeParallelHashMapBucketData(values, keys, next, buckets, bucketCapacityMask);
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static void CheckHashMapReallocateDoesNotShrink(UnsafeParallelHashMapData* data, int newCapacity)
        {
            if (data->keyCapacity > newCapacity)
                throw new InvalidOperationException("Shrinking a hash map is not supported");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UnsafeParallelHashMapBase<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        internal static unsafe void Clear(UnsafeParallelHashMapData* data)
        {
            Unsafe.InitBlockUnaligned(data->buckets, 0xff, (uint)((data->bucketCapacityMask + 1) * 4));
            Unsafe.InitBlockUnaligned(data->next, 0xff, (uint)((data->keyCapacity) * 4));

            int maxThreadCount = JobsUtility.ThreadIndexCount;
            for (int tls = 0; tls < maxThreadCount; ++tls)
            {
                data->firstFreeTLS[tls * UnsafeParallelHashMapData.IntsPerCacheLine] = -1;
            }

            data->allocatedIndexLength = 0;
        }

        private const int SentinelRefilling = -2;
        private const int SentinelSwapInProgress = -3;

        internal static unsafe int AllocEntry(UnsafeParallelHashMapData* data, int threadIndex)
        {
            int idx;
            int* nextPtrs = (int*)data->next;

            do
            {
                do
                {
                    idx = Volatile.Read(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine]);
                } while (idx == SentinelSwapInProgress);

                // Check if this thread has a free entry. Negative value means there is nothing free.
                if (idx < 0)
                {
                    // Try to refill local cache. The local cache is a linked list of 16 free entries.

                    // Indicate to other threads that we are refilling the cache.
                    // -2 means refilling cache.
                    // -1 means nothing free on this thread.
                    Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine], SentinelRefilling);

                    // If it failed try to get one from the never-allocated array
                    if (data->allocatedIndexLength < data->keyCapacity)
                    {
                        idx = Interlocked.Add(ref data->allocatedIndexLength, 16) - 16;

                        if (idx < data->keyCapacity - 1)
                        {
                            int count = Math.Min(16, data->keyCapacity - idx);

                            // Set up a linked list of free entries.
                            for (int i = 1; i < count; ++i)
                            {
                                nextPtrs[idx + i] = idx + i + 1;
                            }

                            // Last entry points to null.
                            nextPtrs[idx + count - 1] = -1;

                            // The first entry is going to be allocated to someone so it also points to null.
                            nextPtrs[idx] = -1;

                            // Set the TLS first free to the head of the list, which is the one after the entry we are returning.
                            Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine], idx + 1);

                            return idx;
                        }

                        if (idx == data->keyCapacity - 1)
                        {
                            // We tried to allocate more entries for this thread but we've already hit the key capacity,
                            // so we are in fact out of space. Record that this thread has no more entries.
                            Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine], -1);

                            return idx;
                        }
                    }

                    // If we reach here, then we couldn't allocate more entries for this thread, so it's completely empty.
                    Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine], -1);

                    int maxThreadCount = JobsUtility.ThreadIndexCount;
                    // Failed to get any, try to get one from another free list
                    bool again = true;
                    while (again)
                    {
                        again = false;
                        for (int other = (threadIndex + 1) % maxThreadCount;
                             other != threadIndex;
                             other = (other + 1) % maxThreadCount
                            )
                        {
                            // Attempt to grab a free entry from another thread and switch the other thread's free head
                            // atomically.
                            do
                            {
                                do
                                {
                                    idx = Volatile.Read(ref data->firstFreeTLS[other * UnsafeParallelHashMapData.IntsPerCacheLine]);
                                } while (idx == SentinelSwapInProgress);

                                if (idx < 0)
                                {
                                    break;
                                }
                            } while (Interlocked.CompareExchange(
                                         ref data->firstFreeTLS[other * UnsafeParallelHashMapData.IntsPerCacheLine]
                                         , SentinelSwapInProgress
                                         , idx
                                     ) != idx
                                    );

                            if (idx == -2)
                            {
                                // If the thread was refilling the cache, then try again.
                                again = true;
                            }
                            else if (idx >= 0)
                            {
                                // We succeeded in getting an entry from another thread so remove this entry from the
                                // linked list.
                                Interlocked.Exchange(ref data->firstFreeTLS[other * UnsafeParallelHashMapData.IntsPerCacheLine], nextPtrs[idx]);
                                nextPtrs[idx] = -1;
                                return idx;
                            }
                        }
                    }

                    ThrowFull();
                }

                CheckOutOfCapacity(idx, data->keyCapacity);
            } while (Interlocked.CompareExchange(
                         ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine]
                         , SentinelSwapInProgress
                         , idx
                     ) != idx
                    );

            Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine], nextPtrs[idx]);
            nextPtrs[idx] = -1;
            return idx;
        }

        internal static unsafe void FreeEntry(UnsafeParallelHashMapData* data, int idx, int threadIndex)
        {
            int* nextPtrs = (int*)data->next;
            int next = -1;

            do
            {
                do
                {
                    next = Volatile.Read(ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine]);
                } while (next == SentinelSwapInProgress);

                nextPtrs[idx] = next;
            } while (Interlocked.CompareExchange(
                         ref data->firstFreeTLS[threadIndex * UnsafeParallelHashMapData.IntsPerCacheLine]
                         , idx
                         , next
                     ) != next
                    );
        }

        internal static unsafe bool TryAddAtomic(UnsafeParallelHashMapData* data, TKey key, TValue item, int threadIndex)
        {
            TValue tempItem;
            NativeParallelMultiHashMapIterator<TKey> tempIt;
            if (TryGetFirstValueAtomic(data, key, out tempItem, out tempIt))
            {
                return false;
            }

            // Allocate an entry from the free list
            int idx = AllocEntry(data, threadIndex);

            // Write the new value to the entry
            Unsafe.Write(data->keys + idx, key);
            Unsafe.Write(data->values + idx, item);

            int bucket = key.GetHashCode() & data->bucketCapacityMask;
            // Add the index to the hash-map
            int* buckets = (int*)data->buckets;

            // Make the bucket's head idx. If the exchange returns something other than -1, then the bucket had
            // a non-null head which means we need to do more checks...
            if (Interlocked.CompareExchange(ref buckets[bucket], idx, -1) != -1)
            {
                int* nextPtrs = (int*)data->next;
                int next = -1;

                do
                {
                    // Link up this entry with the rest of the bucket under the assumption that this key
                    // doesn't already exist in the bucket. This assumption could be wrong, which will be
                    // checked later.
                    next = buckets[bucket];
                    nextPtrs[idx] = next;

                    // If the key already exists then we should free the entry we took earlier.
                    if (TryGetFirstValueAtomic(data, key, out tempItem, out tempIt))
                    {
                        // Put back the entry in the free list if someone else added it while trying to add
                        FreeEntry(data, idx, threadIndex);

                        return false;
                    }
                } while (Interlocked.CompareExchange(ref buckets[bucket], idx, next) != next);
            }

            return true;
        }

        internal static unsafe void AddAtomicMulti(UnsafeParallelHashMapData* data, TKey key, TValue item, int threadIndex)
        {
            // Allocate an entry from the free list
            int idx = AllocEntry(data, threadIndex);

            // Write the new value to the entry
            Unsafe.Write(data->keys + idx, key);
            Unsafe.Write(data->values + idx, item);

            int bucket = key.GetHashCode() & data->bucketCapacityMask;
            // Add the index to the hash-map
            int* buckets = (int*)data->buckets;

            int nextPtr;
            int* nextPtrs = (int*)data->next;
            do
            {
                nextPtr = buckets[bucket];
                nextPtrs[idx] = nextPtr;
            } while (Interlocked.CompareExchange(ref buckets[bucket], idx, nextPtr) != nextPtr);
        }

        internal static unsafe bool TryAdd(UnsafeParallelHashMapData* data, TKey key, TValue item, bool isMultiHashMap)
        {
            TValue tempItem;
            NativeParallelMultiHashMapIterator<TKey> tempIt;
            if (isMultiHashMap || !TryGetFirstValueAtomic(data, key, out tempItem, out tempIt))
            {
                // Allocate an entry from the free list
                int idx;
                int* nextPtrs;

                if (data->allocatedIndexLength >= data->keyCapacity && data->firstFreeTLS[0] < 0)
                {
                    int maxThreadCount = JobsUtility.ThreadIndexCount;
                    for (int tls = 1; tls < maxThreadCount; ++tls)
                    {
                        if (data->firstFreeTLS[tls * UnsafeParallelHashMapData.IntsPerCacheLine] >= 0)
                        {
                            idx = data->firstFreeTLS[tls * UnsafeParallelHashMapData.IntsPerCacheLine];
                            nextPtrs = (int*)data->next;
                            data->firstFreeTLS[tls * UnsafeParallelHashMapData.IntsPerCacheLine] = nextPtrs[idx];
                            nextPtrs[idx] = -1;
                            data->firstFreeTLS[0] = idx;
                            break;
                        }
                    }

                    if (data->firstFreeTLS[0] < 0)
                    {
                        int newCap = UnsafeParallelHashMapData.GrowCapacity(data->keyCapacity);
                        UnsafeParallelHashMapData.ReallocateHashMap<TKey, TValue>(data, newCap, UnsafeParallelHashMapData.GetBucketSize(newCap));
                    }
                }

                idx = data->firstFreeTLS[0];

                if (idx >= 0)
                {
                    data->firstFreeTLS[0] = ((int*)data->next)[idx];
                }
                else
                {
                    idx = data->allocatedIndexLength++;
                }

                CheckIndexOutOfBounds(data, idx);

                // Write the new value to the entry
                Unsafe.Write(data->keys + idx, key);
                Unsafe.Write(data->values + idx, item);

                int bucket = key.GetHashCode() & data->bucketCapacityMask;
                // Add the index to the hash-map
                int* buckets = (int*)data->buckets;
                nextPtrs = (int*)data->next;
                nextPtrs[idx] = buckets[bucket];
                buckets[bucket] = idx;

                return true;
            }

            return false;
        }

        internal static unsafe int Remove(UnsafeParallelHashMapData* data, TKey key, bool isMultiHashMap)
        {
            if (data->keyCapacity == 0)
            {
                return 0;
            }

            var removed = 0;

            // First find the slot based on the hash
            var buckets = (int*)data->buckets;
            var nextPtrs = (int*)data->next;
            var bucket = key.GetHashCode() & data->bucketCapacityMask;
            var prevEntry = -1;
            var entryIdx = buckets[bucket];

            while (entryIdx >= 0 && entryIdx < data->keyCapacity)
            {
                if (Unsafe.Read<TKey>(data->keys + entryIdx).Equals(key))
                {
                    ++removed;

                    // Found matching element, remove it
                    if (prevEntry < 0)
                    {
                        buckets[bucket] = nextPtrs[entryIdx];
                    }
                    else
                    {
                        nextPtrs[prevEntry] = nextPtrs[entryIdx];
                    }

                    // And free the index
                    int nextIdx = nextPtrs[entryIdx];
                    nextPtrs[entryIdx] = data->firstFreeTLS[0];
                    data->firstFreeTLS[0] = entryIdx;
                    entryIdx = nextIdx;

                    // Can only be one hit in regular hashmaps, so return
                    if (!isMultiHashMap)
                    {
                        break;
                    }
                }
                else
                {
                    prevEntry = entryIdx;
                    entryIdx = nextPtrs[entryIdx];
                }
            }

            return removed;
        }

        internal static unsafe void Remove(UnsafeParallelHashMapData* data, NativeParallelMultiHashMapIterator<TKey> it)
        {
            // First find the slot based on the hash
            int* buckets = (int*)data->buckets;
            int* nextPtrs = (int*)data->next;
            int bucket = it.key.GetHashCode() & data->bucketCapacityMask;

            int entryIdx = buckets[bucket];

            if (entryIdx == it.EntryIndex)
            {
                buckets[bucket] = nextPtrs[entryIdx];
            }
            else
            {
                while (entryIdx >= 0 && nextPtrs[entryIdx] != it.EntryIndex)
                {
                    entryIdx = nextPtrs[entryIdx];
                }

                if (entryIdx < 0)
                {
                    ThrowInvalidIterator();
                }

                nextPtrs[entryIdx] = nextPtrs[it.EntryIndex];
            }

            // And free the index
            nextPtrs[it.EntryIndex] = data->firstFreeTLS[0];
            data->firstFreeTLS[0] = it.EntryIndex;
        }

        internal static unsafe void RemoveKeyValue<TValueEQ>(UnsafeParallelHashMapData* data, TKey key, TValueEQ value)
            where TValueEQ : unmanaged, IEquatable<TValueEQ>
        {
            if (data->keyCapacity == 0)
            {
                return;
            }

            var buckets = (int*)data->buckets;
            var keyCapacity = (uint)data->keyCapacity;
            var prevNextPtr = buckets + (key.GetHashCode() & data->bucketCapacityMask);
            var entryIdx = *prevNextPtr;

            if ((uint)entryIdx >= keyCapacity)
            {
                return;
            }

            var nextPtrs = (int*)data->next;
            var keys = data->keys;
            var values = data->values;
            var firstFreeTLS = data->firstFreeTLS;

            do
            {
                if (Unsafe.Read<TKey>(keys + entryIdx).Equals(key)
                    && Unsafe.Read<TValueEQ>(values + entryIdx).Equals(value))
                {
                    int nextIdx = nextPtrs[entryIdx];
                    nextPtrs[entryIdx] = firstFreeTLS[0];
                    firstFreeTLS[0] = entryIdx;
                    *prevNextPtr = entryIdx = nextIdx;
                }
                else
                {
                    prevNextPtr = nextPtrs + entryIdx;
                    entryIdx = *prevNextPtr;
                }
            } while ((uint)entryIdx < keyCapacity);
        }

        internal static unsafe bool TryGetFirstValueAtomic(UnsafeParallelHashMapData* data, TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
        {
            it.key = key;

            if (data->allocatedIndexLength <= 0)
            {
                it.EntryIndex = it.NextEntryIndex = -1;
                item = default;
                return false;
            }

            // First find the slot based on the hash
            int* buckets = (int*)data->buckets;
            int bucket = key.GetHashCode() & data->bucketCapacityMask;
            it.EntryIndex = it.NextEntryIndex = buckets[bucket];
            return TryGetNextValueAtomic(data, out item, ref it);
        }

        internal static unsafe bool TryGetNextValueAtomic(UnsafeParallelHashMapData* data, out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
        {
            int entryIdx = it.NextEntryIndex;
            it.NextEntryIndex = -1;
            it.EntryIndex = -1;
            item = default;
            if (entryIdx < 0 || entryIdx >= data->keyCapacity)
            {
                return false;
            }

            int* nextPtrs = (int*)data->next;
            while (!Unsafe.Read<TKey>(data->keys + entryIdx).Equals(it.key))
            {
                entryIdx = nextPtrs[entryIdx];
                if (entryIdx < 0 || entryIdx >= data->keyCapacity)
                {
                    return false;
                }
            }

            it.NextEntryIndex = nextPtrs[entryIdx];
            it.EntryIndex = entryIdx;

            // Read the value
            item = Unsafe.Read<TValue>(data->values + entryIdx);

            return true;
        }

        internal static unsafe bool SetValue(UnsafeParallelHashMapData* data, ref NativeParallelMultiHashMapIterator<TKey> it, ref TValue item)
        {
            int entryIdx = it.EntryIndex;
            if (entryIdx < 0 || entryIdx >= data->keyCapacity)
            {
                return false;
            }

            Unsafe.Write(data->values + entryIdx, item);
            return true;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static void CheckOutOfCapacity(int idx, int keyCapacity)
        {
            if (idx >= keyCapacity)
            {
                throw new InvalidOperationException(string.Format("nextPtr idx {0} beyond capacity {1}", idx, keyCapacity));
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static unsafe void CheckIndexOutOfBounds(UnsafeParallelHashMapData* data, int idx)
        {
            if (idx < 0 || idx >= data->keyCapacity)
                throw new InvalidOperationException("Internal HashMap error");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static void ThrowFull()
        {
            throw new InvalidOperationException("HashMap is full");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
        static void ThrowInvalidIterator()
        {
            throw new InvalidOperationException("Invalid iterator passed to HashMap remove");
        }
    }

    /// <summary>
    ///     A key-value pair.
    /// </summary>
    /// <remarks>Used for enumerators.</remarks>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    public unsafe struct KeyValue<TKey, TValue>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        internal UnsafeParallelHashMapData* m_Buffer;
        internal int m_Index;
        // internal int m_Next;

        /// <summary>
        ///     An invalid KeyValue.
        /// </summary>
        /// <value>
        ///     In a hash map enumerator's initial state, its
        ///     <see cref="UnsafeParallelHashMap{TKey,TValue}.Enumerator.Current" /> value is Null.
        /// </value>
        public static KeyValue<TKey, TValue> Null => new KeyValue<TKey, TValue> { m_Index = -1 };

        /// <summary>
        ///     The key.
        /// </summary>
        /// <value>The key. If this KeyValue is Null, returns the default of TKey.</value>
        public TKey Key
        {
            get
            {
                if (m_Index != -1)
                {
                    return Unsafe.Read<TKey>(m_Buffer->keys + m_Index);
                }

                return default;
            }
        }

        /// <summary>
        ///     Value of key/value pair.
        /// </summary>
        public ref TValue Value
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (m_Index == -1)
                    throw new ArgumentException("must be valid");
#endif

                return ref Unsafe.AsRef<TValue>(m_Buffer->values + Unsafe.SizeOf<TValue>() * m_Index);
            }
        }

        /// <summary>
        ///     Gets the key and the value.
        /// </summary>
        /// <param name="key">Outputs the key. If this KeyValue is Null, outputs the default of TKey.</param>
        /// <param name="value">Outputs the value. If this KeyValue is Null, outputs the default of TValue.</param>
        /// <returns>True if the key-value pair is valid.</returns>
        public bool GetKeyValue(out TKey key, out TValue value)
        {
            if (m_Index != -1)
            {
                key = Unsafe.Read<TKey>(m_Buffer->keys + m_Index);
                value = Unsafe.Read<TValue>(m_Buffer->values + m_Index);
                return true;
            }

            key = default;
            value = default;
            return false;
        }
    }

    internal unsafe struct UnsafeParallelHashMapDataEnumerator
    {
        internal UnsafeParallelHashMapData* m_Buffer;
        internal int m_Index;
        internal int m_BucketIndex;
        internal int m_NextIndex;

        internal unsafe UnsafeParallelHashMapDataEnumerator(UnsafeParallelHashMapData* data)
        {
            m_Buffer = data;
            m_Index = -1;
            m_BucketIndex = 0;
            m_NextIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool MoveNext()
        {
            return UnsafeParallelHashMapData.MoveNext(m_Buffer, ref m_BucketIndex, ref m_NextIndex, out m_Index);
        }

        internal void Reset()
        {
            m_Index = -1;
            m_BucketIndex = 0;
            m_NextIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal KeyValue<TKey, TValue> GetCurrent<TKey, TValue>()
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return new KeyValue<TKey, TValue> { m_Buffer = m_Buffer, m_Index = m_Index };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TKey GetCurrentKey<TKey>()
            where TKey : unmanaged, IEquatable<TKey>
        {
            if (m_Index != -1)
            {
                return Unsafe.Read<TKey>(m_Buffer->keys + m_Index);
            }

            return default;
        }
    }

    /// <summary>
    ///     An unordered, expandable associative array.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeParallelHashMap<TKey, TValue> : IDisposable, IEnumerable<KeyValue<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged // Used by collection initializers.where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        internal UnsafeParallelHashMapData* m_Buffer;

        /// <summary>
        ///     Initializes and returns an instance of UnsafeParallelHashMap.
        /// </summary>
        /// <param name="capacity">The number of key-value pairs that should fit in the initial allocation.</param>
        public UnsafeParallelHashMap(int capacity)
        {
            // Bucket size if bigger to reduce collisions
            UnsafeParallelHashMapData.AllocateHashMap<TKey, TValue>(capacity, capacity * 2, out m_Buffer);

            Clear();
        }

        /// <summary>
        ///     Whether this hash map has been allocated (and not yet deallocated).
        /// </summary>
        /// <value>True if this hash map has been allocated (and not yet deallocated).</value>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Buffer != null;
        }

        /// <summary>
        ///     Whether this hash map is empty.
        /// </summary>
        /// <value>True if this hash map is empty or the hash map has not been constructed.</value>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || UnsafeParallelHashMapData.IsEmpty(m_Buffer);
        }

        /// <summary>
        ///     The current number of key-value pairs in this hash map.
        /// </summary>
        /// <returns>The current number of key-value pairs in this hash map.</returns>
        public readonly int Count() => UnsafeParallelHashMapData.GetCount(m_Buffer);

        /// <summary>
        ///     The number of key-value pairs that fit in the current allocation.
        /// </summary>
        /// <value>The number of key-value pairs that fit in the current allocation.</value>
        /// <param name="value">A new capacity. Must be larger than the current capacity.</param>
        /// <exception cref="InvalidOperationException">Thrown if `value` is less than the current capacity.</exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                UnsafeParallelHashMapData* data = m_Buffer;
                return data->keyCapacity;
            }

            set
            {
                UnsafeParallelHashMapData* data = m_Buffer;
                UnsafeParallelHashMapData.ReallocateHashMap<TKey, TValue>(data, value, UnsafeParallelHashMapData.GetBucketSize(value));
            }
        }

        /// <summary>
        ///     Removes all key-value pairs.
        /// </summary>
        /// <remarks>Does not change the capacity.</remarks>
        public void Clear()
        {
            UnsafeParallelHashMapBase<TKey, TValue>.Clear(m_Buffer);
        }

        /// <summary>
        ///     Adds a new key-value pair.
        /// </summary>
        /// <remarks>If the key is already present, this method returns false without modifying the hash map.</remarks>
        /// <param name="key">The key to add.</param>
        /// <param name="item">The value to add.</param>
        /// <returns>True if the key-value pair was added.</returns>
        public bool TryAdd(TKey key, TValue item)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, false);
        }

        /// <summary>
        ///     Adds a new key-value pair.
        /// </summary>
        /// <remarks>If the key is already present, this method throws without modifying the hash map.</remarks>
        /// <param name="key">The key to add.</param>
        /// <param name="item">The value to add.</param>
        /// <exception cref="ArgumentException">Thrown if the key was already present.</exception>
        public void Add(TKey key, TValue item)
        {
            UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, false);
        }

        /// <summary>
        ///     Removes a key-value pair.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if a key-value pair was removed.</returns>
        public bool Remove(TKey key)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, key, false) != 0;
        }

        /// <summary>
        ///     Returns the value associated with a key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="item">Outputs the value associated with the key. Outputs default if the key was not present.</param>
        /// <returns>True if the key was present.</returns>
        public bool TryGetValue(TKey key, out TValue item)
        {
            NativeParallelMultiHashMapIterator<TKey> tempIt;
            return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out item, out tempIt);
        }

        /// <summary>
        ///     Returns true if a given key is present in this hash map.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>True if the key was present.</returns>
        public bool ContainsKey(TKey key)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out var tempValue, out var tempIt);
        }

        /// <summary>
        ///     Gets and sets values by key.
        /// </summary>
        /// <remarks>Getting a key that is not present will throw. Setting a key that is not already present will add the key.</remarks>
        /// <param name="key">The key to look up.</param>
        /// <value>The value associated with the key.</value>
        /// <exception cref="ArgumentException">For getting, thrown if the key was not present.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue res;
                TryGetValue(key, out res);
                return res;
            }

            set
            {
                if (UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out var item, out var iterator))
                {
                    UnsafeParallelHashMapBase<TKey, TValue>.SetValue(m_Buffer, ref iterator, ref value);
                }
                else
                {
                    UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, value, false);
                }
            }
        }

        /// <summary>
        ///     Releases all resources (memory).
        /// </summary>
        public void Dispose()
        {
            if (!IsCreated)
            {
                return;
            }

            UnsafeParallelHashMapData.DeallocateHashMap(m_Buffer);
            m_Buffer = null;
        }

        /// <summary>
        ///     Returns an array with a copy of all this hash map's keys (in no particular order).
        /// </summary>
        /// <returns>An array with a copy of all this hash map's keys (in no particular order).</returns>
        public NativeArray<TKey> GetKeyArray()
        {
            var result = new NativeArray<TKey>(UnsafeParallelHashMapData.GetCount(m_Buffer));
            UnsafeParallelHashMapData.GetKeyArray(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns an array with a copy of all this hash map's values (in no particular order).
        /// </summary>
        /// <returns>An array with a copy of all this hash map's values (in no particular order).</returns>
        public NativeArray<TValue> GetValueArray()
        {
            var result = new NativeArray<TValue>(UnsafeParallelHashMapData.GetCount(m_Buffer));
            UnsafeParallelHashMapData.GetValueArray(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns a NativeKeyValueArrays with a copy of all this hash map's keys and values.
        /// </summary>
        /// <remarks>
        ///     The key-value pairs are copied in no particular order. For all `i`, `Values[i]` will be the value associated
        ///     with `Keys[i]`.
        /// </remarks>
        /// <returns>A NativeKeyValueArrays with a copy of all this hash map's keys and values.</returns>
        public NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays()
        {
            var result = new NativeKeyValueArrays<TKey, TValue>(UnsafeParallelHashMapData.GetCount(m_Buffer));
            UnsafeParallelHashMapData.GetKeyValueArrays(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns a parallel writer for this hash map.
        /// </summary>
        /// <returns>A parallel writer for this hash map.</returns>
        public ParallelWriter AsParallelWriter()
        {
            ParallelWriter writer;
            writer.m_Buffer = m_Buffer;
            return writer;
        }

        /// <summary>
        ///     Returns a readonly version of this UnsafeParallelHashMap instance.
        /// </summary>
        /// <remarks>ReadOnly containers point to the same underlying data as the UnsafeParallelHashMap it is made from.</remarks>
        /// <returns>ReadOnly instance for this.</returns>
        public ReadOnly AsReadOnly()
        {
            return new ReadOnly(this);
        }

        /// <summary>
        ///     A read-only alias for the value of a UnsafeParallelHashMap. Does not have its own allocated storage.
        /// </summary>
        public struct ReadOnly
            : IEnumerable<KeyValue<TKey, TValue>>
        {
            internal UnsafeParallelHashMap<TKey, TValue> m_HashMapData;

            internal ReadOnly(UnsafeParallelHashMap<TKey, TValue> hashMapData)
            {
                m_HashMapData = hashMapData;
            }

            /// <summary>
            ///     Whether this hash map has been allocated (and not yet deallocated).
            /// </summary>
            /// <value>True if this hash map has been allocated (and not yet deallocated).</value>
            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_HashMapData.IsCreated;
            }

            /// <summary>
            ///     Whether this hash map is empty.
            /// </summary>
            /// <value>True if this hash map is empty or if the map has not been constructed.</value>
            public readonly bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (!IsCreated)
                    {
                        return true;
                    }

                    return m_HashMapData.IsEmpty;
                }
            }

            /// <summary>
            ///     The current number of key-value pairs in this hash map.
            /// </summary>
            /// <returns>The current number of key-value pairs in this hash map.</returns>
            public readonly int Count()
            {
                return m_HashMapData.Count();
            }

            /// <summary>
            ///     The number of key-value pairs that fit in the current allocation.
            /// </summary>
            /// <value>The number of key-value pairs that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return m_HashMapData.Capacity; }
            }

            /// <summary>
            ///     Returns the value associated with a key.
            /// </summary>
            /// <param name="key">The key to look up.</param>
            /// <param name="item">Outputs the value associated with the key. Outputs default if the key was not present.</param>
            /// <returns>True if the key was present.</returns>
            public readonly bool TryGetValue(TKey key, out TValue item)
            {
                return m_HashMapData.TryGetValue(key, out item);
            }

            /// <summary>
            ///     Returns true if a given key is present in this hash map.
            /// </summary>
            /// <param name="key">The key to look up.</param>
            /// <returns>True if the key was present.</returns>
            public readonly bool ContainsKey(TKey key)
            {
                return m_HashMapData.ContainsKey(key);
            }

            /// <summary>
            ///     Gets values by key.
            /// </summary>
            /// <remarks>Getting a key that is not present will throw.</remarks>
            /// <param name="key">The key to look up.</param>
            /// <value>The value associated with the key.</value>
            /// <exception cref="ArgumentException">For getting, thrown if the key was not present.</exception>
            public readonly TValue this[TKey key]
            {
                get
                {
                    TValue res;

                    if (m_HashMapData.TryGetValue(key, out res))
                    {
                        return res;
                    }

                    ThrowKeyNotPresent(key);

                    return default;
                }
            }

            /// <summary>
            ///     Returns an array with a copy of all this hash map's keys (in no particular order).
            /// </summary>
            /// <returns>An array with a copy of all this hash map's keys (in no particular order).</returns>
            public readonly NativeArray<TKey> GetKeyArray()
            {
                return m_HashMapData.GetKeyArray();
            }

            /// <summary>
            ///     Returns an array with a copy of all this hash map's values (in no particular order).
            /// </summary>
            /// <returns>An array with a copy of all this hash map's values (in no particular order).</returns>
            public readonly NativeArray<TValue> GetValueArray()
            {
                return m_HashMapData.GetValueArray();
            }

            /// <summary>
            ///     Returns a NativeKeyValueArrays with a copy of all this hash map's keys and values.
            /// </summary>
            /// <remarks>
            ///     The key-value pairs are copied in no particular order. For all `i`, `Values[i]` will be the value associated
            ///     with `Keys[i]`.
            /// </remarks>
            /// <returns>A NativeKeyValueArrays with a copy of all this hash map's keys and values.</returns>
            public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays()
            {
                return m_HashMapData.GetKeyValueArrays();
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("UNITY_DOTS_DEBUG")]
            readonly void ThrowKeyNotPresent(TKey key)
            {
                throw new ArgumentException($"Key: {key} is not present in the NativeParallelHashMap.");
            }

            /// <summary>
            ///     Returns an enumerator over the key-value pairs of this hash map.
            /// </summary>
            /// <returns>An enumerator over the key-value pairs of this hash map.</returns>
            public readonly Enumerator GetEnumerator()
            {
                return new Enumerator
                {
                    m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_HashMapData.m_Buffer)
                };
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     A parallel writer for a NativeParallelHashMap.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="AsParallelWriter" /> to create a parallel writer for a NativeParallelHashMap.
        /// </remarks>
        public unsafe struct ParallelWriter
        {
            internal UnsafeParallelHashMapData* m_Buffer;

            internal int m_ThreadIndex => Thread.GetCurrentProcessorId();

            /// <summary>
            ///     Returns the index of the current thread.
            /// </summary>
            /// <remarks>
            ///     In a job, each thread gets its own copy of the ParallelWriter struct, and the job system assigns
            ///     each copy the index of its thread.
            /// </remarks>
            /// <value>The index of the current thread.</value>
            public int ThreadIndex => m_ThreadIndex;

            /// <summary>
            ///     The number of key-value pairs that fit in the current allocation.
            /// </summary>
            /// <value>The number of key-value pairs that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    UnsafeParallelHashMapData* data = m_Buffer;
                    return data->keyCapacity;
                }
            }

            /// <summary>
            ///     Adds a new key-value pair.
            /// </summary>
            /// <remarks>If the key is already present, this method returns false without modifying the hash map.</remarks>
            /// <param name="key">The key to add.</param>
            /// <param name="item">The value to add.</param>
            /// <returns>True if the key-value pair was added.</returns>
            public bool TryAdd(TKey key, TValue item)
            {
                return UnsafeParallelHashMapBase<TKey, TValue>.TryAddAtomic(m_Buffer, key, item, m_ThreadIndex);
            }

            /// <summary>
            ///     Adds a new key-value pair.
            /// </summary>
            /// <remarks>If the key is already present, this method returns false without modifying the hash map.</remarks>
            /// <param name="key">The key to add.</param>
            /// <param name="item">The value to add.</param>
            /// <param name="threadIndexOverride"></param>
            /// <returns>True if the key-value pair was added.</returns>
            internal bool TryAdd(TKey key, TValue item, int threadIndexOverride)
            {
                return UnsafeParallelHashMapBase<TKey, TValue>.TryAddAtomic(m_Buffer, key, item, threadIndexOverride);
            }
        }

        /// <summary>
        ///     Returns an enumerator over the key-value pairs of this hash map.
        /// </summary>
        /// <returns>An enumerator over the key-value pairs of this hash map.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator { m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Buffer) };
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     An enumerator over the key-value pairs of a hash map.
        /// </summary>
        /// <remarks>
        ///     In an enumerator's initial state, <see cref="Current" /> is not valid to read.
        ///     From this state, the first <see cref="MoveNext" /> call advances the enumerator to the first key-value pair.
        /// </remarks>
        public struct Enumerator : IEnumerator<KeyValue<TKey, TValue>>
        {
            internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

            /// <summary>
            ///     Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            ///     Advances the enumerator to the next key-value pair.
            /// </summary>
            /// <returns>True if <see cref="Current" /> is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => m_Enumerator.MoveNext();

            /// <summary>
            ///     Resets the enumerator to its initial state.
            /// </summary>
            public void Reset() => m_Enumerator.Reset();

            /// <summary>
            ///     The current key-value pair.
            /// </summary>
            /// <value>The current key-value pair.</value>
            public KeyValue<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Enumerator.GetCurrent<TKey, TValue>();
            }

            object IEnumerator.Current => Current;
        }
    }

    /// <summary>
    ///     An unordered, expandable associative array. Each key can have more than one associated value.
    /// </summary>
    /// <remarks>
    ///     Unlike a regular UnsafeParallelHashMap, an UnsafeParallelMultiHashMap can store multiple key-value pairs with the
    ///     same key.
    ///     The keys are not deduplicated: two key-value pairs with the same key are stored as fully separate key-value pairs.
    /// </remarks>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeParallelMultiHashMap<TKey, TValue> : IDisposable, IEnumerable<KeyValue<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        internal UnsafeParallelHashMapData* m_Buffer;

        /// <summary>
        ///     Initializes and returns an instance of UnsafeParallelMultiHashMap.
        /// </summary>
        /// <param name="capacity">The number of key-value pairs that should fit in the initial allocation.</param>
        public UnsafeParallelMultiHashMap(int capacity)
        {
            // Bucket size if bigger to reduce collisions
            UnsafeParallelHashMapData.AllocateHashMap<TKey, TValue>(capacity, capacity * 2, out m_Buffer);
            Clear();
        }

        /// <summary>
        ///     Whether this hash map is empty.
        /// </summary>
        /// <value>True if this hash map is empty or the hash map has not been constructed.</value>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsCreated || UnsafeParallelHashMapData.IsEmpty(m_Buffer);
        }

        /// <summary>
        ///     Returns the current number of key-value pairs in this hash map.
        /// </summary>
        /// <remarks>Key-value pairs with matching keys are counted as separate, individual pairs.</remarks>
        /// <returns>The current number of key-value pairs in this hash map.</returns>
        public readonly int Count()
        {
            if (m_Buffer->allocatedIndexLength <= 0)
            {
                return 0;
            }

            return UnsafeParallelHashMapData.GetCount(m_Buffer);
        }

        /// <summary>
        ///     Returns the number of key-value pairs that fit in the current allocation.
        /// </summary>
        /// <value>The number of key-value pairs that fit in the current allocation.</value>
        /// <param name="value">A new capacity. Must be larger than the current capacity.</param>
        /// <exception cref="InvalidOperationException">Thrown if `value` is less than the current capacity.</exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                UnsafeParallelHashMapData* data = m_Buffer;
                return data->keyCapacity;
            }

            set
            {
                UnsafeParallelHashMapData* data = m_Buffer;
                UnsafeParallelHashMapData.ReallocateHashMap<TKey, TValue>(data, value, UnsafeParallelHashMapData.GetBucketSize(value));
            }
        }

        /// <summary>
        ///     Removes all key-value pairs.
        /// </summary>
        /// <remarks>Does not change the capacity.</remarks>
        public void Clear()
        {
            UnsafeParallelHashMapBase<TKey, TValue>.Clear(m_Buffer);
        }

        /// <summary>
        ///     Adds a new key-value pair.
        /// </summary>
        /// <remarks>
        ///     If a key-value pair with this key is already present, an additional separate key-value pair is added.
        /// </remarks>
        /// <param name="key">The key to add.</param>
        /// <param name="item">The value to add.</param>
        public void Add(TKey key, TValue item)
        {
            UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, true);
        }

        /// <summary>
        ///     Removes a key and its associated value(s).
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>The number of removed key-value pairs. If the key was not present, returns 0.</returns>
        public int Remove(TKey key)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, key, true);
        }

        /// <summary>
        ///     Removes all key-value pairs with a particular key and a particular value.
        /// </summary>
        /// <remarks>
        ///     Removes all key-value pairs which have a particular key and which *also have* a particular value.
        ///     In other words: (key *AND* value) rather than (key *OR* value).
        /// </remarks>
        /// <typeparam name="TValueEQ">The type of the value.</typeparam>
        /// <param name="key">The key of the key-value pairs to remove.</param>
        /// <param name="value">The value of the key-value pairs to remove.</param>
        public void Remove<TValueEQ>(TKey key, TValueEQ value)
            where TValueEQ : unmanaged, IEquatable<TValueEQ>
        {
            UnsafeParallelHashMapBase<TKey, TValueEQ>.RemoveKeyValue(m_Buffer, key, value);
        }

        /// <summary>
        ///     Removes a single key-value pair.
        /// </summary>
        /// <param name="it">An iterator representing the key-value pair to remove.</param>
        /// <exception cref="InvalidOperationException">Thrown if the iterator is invalid.</exception>
        public void Remove(NativeParallelMultiHashMapIterator<TKey> it)
        {
            UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, it);
        }

        /// <summary>
        ///     Gets an iterator for a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">Outputs the associated value represented by the iterator.</param>
        /// <param name="it">Outputs an iterator.</param>
        /// <returns>True if the key was present.</returns>
        public readonly bool TryGetFirstValue(TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out item, out it);
        }

        /// <summary>
        ///     Advances an iterator to the next value associated with its key.
        /// </summary>
        /// <param name="item">Outputs the next value.</param>
        /// <param name="it">A reference to the iterator to advance.</param>
        /// <returns>True if the key was present and had another value.</returns>
        public readonly bool TryGetNextValue(out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.TryGetNextValueAtomic(m_Buffer, out item, ref it);
        }

        /// <summary>
        ///     Returns true if a given key is present in this hash map.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>True if the key was present in this hash map.</returns>
        public readonly bool ContainsKey(TKey key)
        {
            return TryGetFirstValue(key, out var temp0, out var temp1);
        }

        /// <summary>
        ///     Returns the number of values associated with a given key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The number of values associated with the key. Returns 0 if the key was not present.</returns>
        public readonly int CountValuesForKey(TKey key)
        {
            if (!TryGetFirstValue(key, out var value, out var iterator))
            {
                return 0;
            }

            var count = 1;
            while (TryGetNextValue(out value, ref iterator))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        ///     Sets a new value for an existing key-value pair.
        /// </summary>
        /// <param name="item">The new value.</param>
        /// <param name="it">The iterator representing a key-value pair.</param>
        /// <returns>True if a value was overwritten.</returns>
        public bool SetValue(TValue item, NativeParallelMultiHashMapIterator<TKey> it)
        {
            return UnsafeParallelHashMapBase<TKey, TValue>.SetValue(m_Buffer, ref it, ref item);
        }

        /// <summary>
        ///     Whether this hash map has been allocated (and not yet deallocated).
        /// </summary>
        /// <value>True if this hash map has been allocated (and not yet deallocated).</value>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Buffer != null;
        }

        /// <summary>
        ///     Releases all resources (memory and safety handles).
        /// </summary>
        public void Dispose()
        {
            if (!IsCreated)
            {
                return;
            }

            UnsafeParallelHashMapData.DeallocateHashMap(m_Buffer);
            m_Buffer = null;
        }

        /// <summary>
        ///     Returns an array with a copy of all the keys (in no particular order).
        /// </summary>
        /// <returns>An array with a copy of all the keys (in no particular order).</returns>
        public readonly NativeArray<TKey> GetKeyArray()
        {
            var result = new NativeArray<TKey>(Count());
            UnsafeParallelHashMapData.GetKeyArray(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns an array with a copy of all the values (in no particular order).
        /// </summary>
        /// <returns>An array with a copy of all the values (in no particular order).</returns>
        public readonly NativeArray<TValue> GetValueArray()
        {
            var result = new NativeArray<TValue>(Count());
            UnsafeParallelHashMapData.GetValueArray(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns a NativeKeyValueArrays with a copy of all the keys and values (in no particular order).
        /// </summary>
        /// <remarks>
        ///     A key with *N* values is included *N* times in the array.
        /// </remarks>
        /// <returns>A NativeKeyValueArrays with a copy of all the keys and values (in no particular order).</returns>
        public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays()
        {
            var result = new NativeKeyValueArrays<TKey, TValue>(Count());
            UnsafeParallelHashMapData.GetKeyValueArrays(m_Buffer, result);
            return result;
        }

        /// <summary>
        ///     Returns an enumerator over the values of an individual key.
        /// </summary>
        /// <param name="key">The key to get an enumerator for.</param>
        /// <returns>An enumerator over the values of a key.</returns>
        public Enumerator GetValuesForKey(TKey key)
        {
            return new Enumerator { hashmap = this, key = key, isFirst = true };
        }

        /// <summary>
        ///     An enumerator over the values of an individual key in a multi hash map.
        /// </summary>
        /// <remarks>
        ///     In an enumerator's initial state, <see cref="Current" /> is not valid to read.
        ///     The first <see cref="MoveNext" /> call advances the enumerator to the first value of the key.
        /// </remarks>
        public struct Enumerator : IEnumerator<TValue>
        {
            internal UnsafeParallelMultiHashMap<TKey, TValue> hashmap;
            internal TKey key;
            internal bool isFirst;

            TValue value;
            NativeParallelMultiHashMapIterator<TKey> iterator;

            /// <summary>
            ///     Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            ///     Advances the enumerator to the next value of the key.
            /// </summary>
            /// <returns>True if <see cref="Current" /> is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                //Avoids going beyond the end of the collection.
                if (isFirst)
                {
                    isFirst = false;
                    return hashmap.TryGetFirstValue(key, out value, out iterator);
                }

                return hashmap.TryGetNextValue(out value, ref iterator);
            }

            /// <summary>
            ///     Resets the enumerator to its initial state.
            /// </summary>
            public void Reset() => isFirst = true;

            /// <summary>
            ///     The current value.
            /// </summary>
            /// <value>The current value.</value>
            public TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => value;
            }

            object IEnumerator.Current => Current;

            /// <summary>
            ///     Returns this enumerator.
            /// </summary>
            /// <returns>This enumerator.</returns>
            public Enumerator GetEnumerator()
            {
                return this;
            }
        }

        /// <summary>
        ///     Returns a parallel writer for this hash map.
        /// </summary>
        /// <returns>A parallel writer for this hash map.</returns>
        public ParallelWriter AsParallelWriter()
        {
            ParallelWriter writer;

            writer.m_Buffer = m_Buffer;

            return writer;
        }

        /// <summary>
        ///     A parallel writer for an UnsafeParallelMultiHashMap.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="AsParallelWriter" /> to create a parallel writer for a NativeParallelMultiHashMap.
        /// </remarks>
        public unsafe struct ParallelWriter
        {
            internal UnsafeParallelHashMapData* m_Buffer;

            internal int m_ThreadIndex => Thread.GetCurrentProcessorId();

            /// <summary>
            ///     Returns the number of key-value pairs that fit in the current allocation.
            /// </summary>
            /// <value>The number of key-value pairs that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Buffer->keyCapacity;
            }

            /// <summary>
            ///     Adds a new key-value pair.
            /// </summary>
            /// <remarks>
            ///     If a key-value pair with this key is already present, an additional separate key-value pair is added.
            /// </remarks>
            /// <param name="key">The key to add.</param>
            /// <param name="item">The value to add.</param>
            public void Add(TKey key, TValue item)
            {
                UnsafeParallelHashMapBase<TKey, TValue>.AddAtomicMulti(m_Buffer, key, item, m_ThreadIndex);
            }
        }

        /// <summary>
        ///     Returns an enumerator over the key-value pairs of this hash map.
        /// </summary>
        /// <remarks>A key with *N* values is visited by the enumerator *N* times.</remarks>
        /// <returns>An enumerator over the key-value pairs of this hash map.</returns>
        public KeyValueEnumerator GetEnumerator()
        {
            return new KeyValueEnumerator { m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Buffer) };
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     An enumerator over the key-value pairs of a multi hash map.
        /// </summary>
        /// <remarks>
        ///     A key with *N* values is visited by the enumerator *N* times.
        ///     In an enumerator's initial state, <see cref="Current" /> is not valid to read.
        ///     The first <see cref="MoveNext" /> call advances the enumerator to the first key-value pair.
        /// </remarks>
        public struct KeyValueEnumerator : IEnumerator<KeyValue<TKey, TValue>>
        {
            internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

            /// <summary>
            ///     Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            ///     Advances the enumerator to the next key-value pair.
            /// </summary>
            /// <returns>True if <see cref="Current" /> is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => m_Enumerator.MoveNext();

            /// <summary>
            ///     Resets the enumerator to its initial state.
            /// </summary>
            public void Reset() => m_Enumerator.Reset();

            /// <summary>
            ///     The current key-value pair.
            /// </summary>
            /// <value>The current key-value pair.</value>
            public KeyValue<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Enumerator.GetCurrent<TKey, TValue>();
            }

            object IEnumerator.Current => Current;
        }

        /// <summary>
        ///     Returns a readonly version of this NativeParallelHashMap instance.
        /// </summary>
        /// <remarks>ReadOnly containers point to the same underlying data as the NativeParallelHashMap it is made from.</remarks>
        /// <returns>ReadOnly instance for this.</returns>
        public ReadOnly AsReadOnly()
        {
            return new ReadOnly(this);
        }

        /// <summary>
        ///     A read-only alias for the value of a UnsafeParallelHashMap. Does not have its own allocated storage.
        /// </summary>
        public struct ReadOnly : IEnumerable<KeyValue<TKey, TValue>>
        {
            internal UnsafeParallelMultiHashMap<TKey, TValue> m_MultiHashMapData;

            internal ReadOnly(UnsafeParallelMultiHashMap<TKey, TValue> container)
            {
                m_MultiHashMapData = container;
            }

            /// <summary>
            ///     Whether this hash map has been allocated (and not yet deallocated).
            /// </summary>
            /// <value>True if this hash map has been allocated (and not yet deallocated).</value>
            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_MultiHashMapData.IsCreated;
            }

            /// <summary>
            ///     Whether this hash map is empty.
            /// </summary>
            /// <value>True if this hash map is empty or if the map has not been constructed.</value>
            public readonly bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (!IsCreated)
                    {
                        return true;
                    }

                    return m_MultiHashMapData.IsEmpty;
                }
            }

            /// <summary>
            ///     The current number of key-value pairs in this hash map.
            /// </summary>
            /// <returns>The current number of key-value pairs in this hash map.</returns>
            public readonly int Count()
            {
                return m_MultiHashMapData.Count();
            }

            /// <summary>
            ///     The number of key-value pairs that fit in the current allocation.
            /// </summary>
            /// <value>The number of key-value pairs that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get { return m_MultiHashMapData.Capacity; }
            }

            /// <summary>
            ///     Gets an iterator for a key.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="item">Outputs the associated value represented by the iterator.</param>
            /// <param name="it">Outputs an iterator.</param>
            /// <returns>True if the key was present.</returns>
            public readonly bool TryGetFirstValue(TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
            {
                return m_MultiHashMapData.TryGetFirstValue(key, out item, out it);
            }

            /// <summary>
            ///     Advances an iterator to the next value associated with its key.
            /// </summary>
            /// <param name="item">Outputs the next value.</param>
            /// <param name="it">A reference to the iterator to advance.</param>
            /// <returns>True if the key was present and had another value.</returns>
            public readonly bool TryGetNextValue(out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
            {
                return m_MultiHashMapData.TryGetNextValue(out item, ref it);
            }

            /// <summary>
            ///     Returns true if a given key is present in this hash map.
            /// </summary>
            /// <param name="key">The key to look up.</param>
            /// <returns>True if the key was present.</returns>
            public readonly bool ContainsKey(TKey key)
            {
                return m_MultiHashMapData.ContainsKey(key);
            }

            /// <summary>
            ///     Returns an array with a copy of all this hash map's keys (in no particular order).
            /// </summary>
            /// <returns>An array with a copy of all this hash map's keys (in no particular order).</returns>
            public readonly NativeArray<TKey> GetKeyArray()
            {
                return m_MultiHashMapData.GetKeyArray();
            }

            /// <summary>
            ///     Returns an array with a copy of all this hash map's values (in no particular order).
            /// </summary>
            /// <returns>An array with a copy of all this hash map's values (in no particular order).</returns>
            public readonly NativeArray<TValue> GetValueArray()
            {
                return m_MultiHashMapData.GetValueArray();
            }

            /// <summary>
            ///     Returns a NativeKeyValueArrays with a copy of all this hash map's keys and values.
            /// </summary>
            /// <remarks>
            ///     The key-value pairs are copied in no particular order. For all `i`, `Values[i]` will be the value associated
            ///     with `Keys[i]`.
            /// </remarks>
            /// <returns>A NativeKeyValueArrays with a copy of all this hash map's keys and values.</returns>
            public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays()
            {
                return m_MultiHashMapData.GetKeyValueArrays();
            }

            /// <summary>
            ///     Returns an enumerator over the key-value pairs of this hash map.
            /// </summary>
            /// <remarks>A key with *N* values is visited by the enumerator *N* times.</remarks>
            /// <returns>An enumerator over the key-value pairs of this hash map.</returns>
            public KeyValueEnumerator GetEnumerator()
            {
                return new KeyValueEnumerator
                {
                    m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_MultiHashMapData.m_Buffer)
                };
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }

    public static class JobsUtility
    {
        public const int CacheLineSize = 128;

        public static int ThreadIndexCount => Environment.ProcessorCount;
    }

    /// <summary>
    ///     An unordered, expandable set of unique values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UnsafeParallelHashSet<T> : IDisposable, IEnumerable<T> where T : unmanaged, IEquatable<T>
    {
        internal UnsafeParallelHashMap<T, bool> m_Data;

        /// <summary>
        ///     Initializes and returns an instance of UnsafeParallelHashSet.
        /// </summary>
        /// <param name="capacity">The number of values that should fit in the initial allocation.</param>
        public UnsafeParallelHashSet(int capacity)
        {
            m_Data = new UnsafeParallelHashMap<T, bool>(capacity);
        }

        /// <summary>
        ///     Whether this set is empty.
        /// </summary>
        /// <value>True if this set is empty.</value>
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Data.IsEmpty;
        }

        /// <summary>
        ///     Returns the current number of values in this set.
        /// </summary>
        /// <returns>The current number of values in this set.</returns>
        public int Count() => m_Data.Count();

        /// <summary>
        ///     The number of values that fit in the current allocation.
        /// </summary>
        /// <value>The number of values that fit in the current allocation.</value>
        /// <param name="value">A new capacity. Must be larger than current capacity.</param>
        /// <exception cref="InvalidOperationException">Thrown if `value` is less than the current capacity.</exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => m_Data.Capacity;
            set => m_Data.Capacity = value;
        }

        /// <summary>
        ///     Whether this set has been allocated (and not yet deallocated).
        /// </summary>
        /// <value>True if this set has been allocated (and not yet deallocated).</value>
        public readonly bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Data.IsCreated;
        }

        /// <summary>
        ///     Releases all resources (memory).
        /// </summary>
        public void Dispose() => m_Data.Dispose();

        /// <summary>
        ///     Removes all values.
        /// </summary>
        /// <remarks>Does not change the capacity.</remarks>
        public void Clear() => m_Data.Clear();

        /// <summary>
        ///     Adds a new value (unless it is already present).
        /// </summary>
        /// <param name="item">The value to add.</param>
        /// <returns>True if the value was not already present.</returns>
        public bool Add(T item) => m_Data.TryAdd(item, false);

        /// <summary>
        ///     Removes a particular value.
        /// </summary>
        /// <param name="item">The value to remove.</param>
        /// <returns>True if the value was present.</returns>
        public bool Remove(T item) => m_Data.Remove(item);

        /// <summary>
        ///     Returns true if a particular value is present.
        /// </summary>
        /// <param name="item">The value to check for.</param>
        /// <returns>True if the value was present.</returns>
        public bool Contains(T item) => m_Data.ContainsKey(item);

        /// <summary>
        ///     Returns an array with a copy of this set's values (in no particular order).
        /// </summary>
        /// <returns>An array with a copy of the set's values.</returns>
        public NativeArray<T> ToNativeArray() => m_Data.GetKeyArray();

        /// <summary>
        ///     Returns a parallel writer.
        /// </summary>
        /// <returns>A parallel writer.</returns>
        public ParallelWriter AsParallelWriter()
        {
            return new ParallelWriter { m_Data = m_Data.AsParallelWriter() };
        }

        /// <summary>
        ///     A parallel writer for an UnsafeParallelHashSet.
        /// </summary>
        /// <remarks>
        ///     Use <see cref="AsParallelWriter" /> to create a parallel writer for a set.
        /// </remarks>
        public struct ParallelWriter
        {
            internal UnsafeParallelHashMap<T, bool>.ParallelWriter m_Data;

            /// <summary>
            ///     The number of values that fit in the current allocation.
            /// </summary>
            /// <value>The number of values that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Data.Capacity;
            }

            /// <summary>
            ///     Adds a new value (unless it is already present).
            /// </summary>
            /// <param name="item">The value to add.</param>
            /// <returns>True if the value is not already present.</returns>
            public bool Add(T item) => m_Data.TryAdd(item, false);

            /// <summary>
            ///     Adds a new value (unless it is already present).
            /// </summary>
            /// <param name="item">The value to add.</param>
            /// <param name="threadIndexOverride"></param>
            /// <returns>True if the value is not already present.</returns>
            internal bool Add(T item, int threadIndexOverride) => m_Data.TryAdd(item, false, threadIndexOverride);
        }

        /// <summary>
        ///     Returns an enumerator over the values of this set.
        /// </summary>
        /// <returns>An enumerator over the values of this set.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator { m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Data.m_Buffer) };
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
        /// </summary>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Method is not implemented.</exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     An enumerator over the values of a set.
        /// </summary>
        /// <remarks>
        ///     In an enumerator's initial state, <see cref="Current" /> is invalid.
        ///     The first <see cref="MoveNext" /> call advances the enumerator to the first value.
        /// </remarks>
        public struct Enumerator : IEnumerator<T>
        {
            internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

            /// <summary>
            ///     Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            /// <summary>
            ///     Advances the enumerator to the next value.
            /// </summary>
            /// <returns>True if `Current` is valid to read after the call.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => m_Enumerator.MoveNext();

            /// <summary>
            ///     Resets the enumerator to its initial state.
            /// </summary>
            public void Reset() => m_Enumerator.Reset();

            /// <summary>
            ///     The current value.
            /// </summary>
            /// <value>The current value.</value>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Enumerator.GetCurrentKey<T>();
            }

            object IEnumerator.Current => Current;
        }

        /// <summary>
        ///     Returns a readonly version of this UnsafeParallelHashSet instance.
        /// </summary>
        /// <remarks>ReadOnly containers point to the same underlying data as the UnsafeParallelHashSet it is made from.</remarks>
        /// <returns>ReadOnly instance for this.</returns>
        public ReadOnly AsReadOnly()
        {
            return new ReadOnly(ref this);
        }

        /// <summary>
        ///     A read-only alias for the value of a UnsafeParallelHashSet. Does not have its own allocated storage.
        /// </summary>
        public struct ReadOnly : IEnumerable<T>
        {
            internal UnsafeParallelHashMap<T, bool> m_Data;

            internal ReadOnly(ref UnsafeParallelHashSet<T> data)
            {
                m_Data = data.m_Data;
            }

            /// <summary>
            ///     Whether this hash set has been allocated (and not yet deallocated).
            /// </summary>
            /// <value>True if this hash set has been allocated (and not yet deallocated).</value>
            public readonly bool IsCreated
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Data.IsCreated;
            }

            /// <summary>
            ///     Whether this hash set is empty.
            /// </summary>
            /// <value>True if this hash set is empty or if the map has not been constructed.</value>
            public readonly bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => !m_Data.IsCreated || m_Data.IsEmpty;
            }

            /// <summary>
            ///     The current number of items in this hash set.
            /// </summary>
            /// <returns>The current number of items in this hash set.</returns>
            public readonly int Count() => m_Data.Count();

            /// <summary>
            ///     The number of items that fit in the current allocation.
            /// </summary>
            /// <value>The number of items that fit in the current allocation.</value>
            public readonly int Capacity
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_Data.Capacity;
            }

            /// <summary>
            ///     Returns true if a given item is present in this hash set.
            /// </summary>
            /// <param name="item">The item to look up.</param>
            /// <returns>True if the item was present.</returns>
            public readonly bool Contains(T item)
            {
                return m_Data.ContainsKey(item);
            }

            /// <summary>
            ///     Returns an array with a copy of all this hash set's items (in no particular order).
            /// </summary>
            /// <returns>An array with a copy of all this hash set's items (in no particular order).</returns>
            public readonly NativeArray<T> ToNativeArray()
            {
                return m_Data.GetKeyArray();
            }

            /// <summary>
            ///     Returns an enumerator over the items of this hash set.
            /// </summary>
            /// <returns>An enumerator over the items of this hash set.</returns>
            public readonly Enumerator GetEnumerator()
            {
                return new Enumerator
                {
                    m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Data.m_Buffer)
                };
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            ///     This method is not implemented. Use <see cref="GetEnumerator" /> instead.
            /// </summary>
            /// <returns>Throws NotImplementedException.</returns>
            /// <exception cref="NotImplementedException">Method is not implemented.</exception>
            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}