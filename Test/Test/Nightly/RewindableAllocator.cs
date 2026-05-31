// com.unity.collections copyright © 2024 Unity Technologies
// Licensed under the Unity Companion License for Unity-dependent projects (see https://unity3d.com/legal/licenses/unity_companion_license).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An allocator that is fast like a linear allocator, is threadsafe, and automatically invalidates
    ///     all allocations made from it, when "rewound" by the user.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RewindableAllocator : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Spinner
        {
            private int m_Lock;

            /// <summary>
            ///     Continually spin until the lock can be acquired.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Acquire()
            {
                for (;;)
                {
                    // Optimistically assume the lock is free on the first try.
                    if (Interlocked.CompareExchange(ref m_Lock, 1, 0) == 0)
                    {
                        return;
                    }

                    // Wait for lock to be released without generate cache misses.
                    while (Volatile.Read(ref m_Lock) == 1)
                    {
                    }

                    // Future improvement: the 'continue` instruction above could be swapped for a 'pause' intrinsic
                    // instruction when the CPU supports it, to further reduce contention by reducing load-store unit
                    // utilization. However, this would need to be optional because if you don't use hyper-threading
                    // and you don't care about power efficiency, using the 'pause' instruction will slow down lock
                    // acquisition in the contended scenario.
                }
            }

            /// <summary>
            ///     Try to acquire the lock and immediately return without spinning.
            /// </summary>
            /// <returns><see langword="true" /> if the lock was acquired, <see langword="false" /> otherwise.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAcquire()
            {
                // First do a memory load (read) to check if lock is free in order to prevent uncessary cache missed.
                return Volatile.Read(ref m_Lock) == 0 && Interlocked.CompareExchange(ref m_Lock, 1, 0) == 0;
            }

            /// <summary>
            ///     Try to acquire the lock, and spin only if <paramref name="spin" /> is <see langword="true" />.
            /// </summary>
            /// <param name="spin">Set to true to spin the lock.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryAcquire(bool spin)
            {
                if (spin)
                {
                    Acquire();
                    return true;
                }

                return TryAcquire();
            }

            /// <summary>
            ///     Release the lock
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Release() => Volatile.Write(ref m_Lock, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Union
        {
            public long m_long;

            // Number of bits used to store current position in a block to give out memory.
            // This limits the maximum block size to 1TB (2^40).
            private const int currentBits = 40;

            // Offset of current position in m_long
            private const int currentOffset = 0;

            // Number of bits used to store the allocation count in a block
            private const long currentMask = (1L << currentBits) - 1;

            // Number of bits used to store allocation count in a block.
            // This limits the maximum number of allocations per block to 16 millions (2^24)
            private const int allocCountBits = 24;

            // Offset of allocation count in m_long
            private const int allocCountOffset = currentOffset + currentBits;
            private const long allocCountMask = (1L << allocCountBits) - 1;

            // Current position in a block to give out memory
            public long m_current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (m_long >> currentOffset) & currentMask;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    m_long &= ~(currentMask << currentOffset);
                    m_long |= (value & currentMask) << currentOffset;
                }
            }

            // The number of allocations in a block
            public long m_allocCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (m_long >> allocCountOffset) & allocCountMask;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    m_long &= ~(allocCountMask << allocCountOffset);
                    m_long |= (value & allocCountMask) << allocCountOffset;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryBlock : IDisposable
        {
            // can't align any coarser than this many bytes
            public const int kMaximumAlignment = 16384;

            // pointer to contiguous memory
            public byte* m_pointer;

            // how many bytes of contiguous memory it points to
            public long m_bytes;

            // Union of current position to give out memory and allocation counts
            public Union m_union;

            public MemoryBlock(long bytes)
            {
                m_pointer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)bytes, kMaximumAlignment);
                m_bytes = bytes;
                m_union = default;
            }

            public void Rewind() => m_union = default;

            public void Dispose()
            {
                NativeMemoryAllocator.AlignedFree(m_pointer);
                m_pointer = null;
                m_bytes = 0;
                m_union = default;
            }

            public bool Contains(IntPtr ptr)
            {
                var pointer = (void*)ptr;
                return pointer >= m_pointer && pointer < m_pointer + m_union.m_current;
            }
        }

        /// <summary>
        ///     A range of allocated memory.
        /// </summary>
        /// <remarks>
        ///     The name is perhaps misleading: only in combination with a <see cref="Block" /> does
        ///     a `Range` have sufficient information to represent the number of bytes in an allocation. The reason `Range` is its
        ///     own type that's separate from `Block`
        ///     stems from some efficiency concerns in the implementation details. In most cases, a `Range` is only used in
        ///     conjunction with an associated `Block`.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct Range
        {
            /// <summary>
            ///     Pointer to the start of this range.
            /// </summary>
            /// <value>Pointer to the start of this range.</value>
            public IntPtr Pointer; //  0

            /// <summary>
            ///     Number of items allocated in this range.
            /// </summary>
            /// <remarks>The actual allocation may be larger. See <see cref="Block.AllocatedItems" />.</remarks>
            /// <value>Number of items allocated in this range. </value>
            public int Items; //  8
        }

        /// <summary>
        ///     Represents an individual allocation within an allocator.
        /// </summary>
        /// <remarks>
        ///     A block consists of a <see cref="Range" /> plus metadata about the type of elements for which the block was
        ///     allocated.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct Block
        {
            /// <summary>
            ///     The range of memory encompassed by this block.
            /// </summary>
            /// <value>The range of memory encompassed by this block.</value>
            public Range Range;

            /// <summary>
            ///     Number of bytes per item.
            /// </summary>
            /// <value>Number of bytes per item.</value>
            public int BytesPerItem;

            /// <summary>
            ///     Number of items allocated for.
            /// </summary>
            /// <value>Number of items allocated for.</value>
            public int AllocatedItems;

            /// <summary>
            ///     Log2 of the byte alignment.
            /// </summary>
            /// <remarks>The alignment must always be power of 2. Storing the alignment as its log2 helps enforces this.</remarks>
            /// <value>Log2 of the byte alignment.</value>
            public byte Log2Alignment;

            /// <summary>
            ///     This field only exists to pad the `Block` struct. Ignore it.
            /// </summary>
            /// <value>This field only exists to pad the `Block` struct. Ignore it.</value>
            public byte Padding0;

            /// <summary>
            ///     This field only exists to pad the `Block` struct. Ignore it.
            /// </summary>
            /// <value>This field only exists to pad the `Block` struct. Ignore it.</value>
            public ushort Padding1;

            /// <summary>
            ///     This field only exists to pad the `Block` struct. Ignore it.
            /// </summary>
            /// <value>This field only exists to pad the `Block` struct. Ignore it.</value>
            public uint Padding2;

            /// <summary>
            ///     Number of bytes requested for this block.
            /// </summary>
            /// <remarks>The actual allocation size may be larger due to alignment.</remarks>
            /// <value>Number of bytes requested for this block.</value>
            public long Bytes => (long)BytesPerItem * Range.Items;

            /// <summary>
            ///     Number of bytes allocated for this block.
            /// </summary>
            /// <remarks>The requested allocation size may be smaller. Any excess is due to alignment</remarks>
            /// <value>Number of bytes allocated for this block.</value>
            public long AllocatedBytes => (long)BytesPerItem * AllocatedItems;

            /// <summary>
            ///     The alignment.
            /// </summary>
            /// <remarks>
            ///     Must be power of 2 that's greater than or equal to 0.
            ///     Set alignment *before* the allocation is made. Setting it after has no effect on the allocation.
            /// </remarks>
            /// <param name="value">A new alignment. If not a power of 2, it will be rounded up to the next largest power of 2.</param>
            /// <value>The alignment.</value>
            public int Alignment
            {
                get => 1 << Log2Alignment;
                set => Log2Alignment = (byte)(32 - BitOperations.LeadingZeroCount((uint)(Math.Max(1, value) - 1)));
            }
        }

        // Log2 of Maximum memory block size.  Cannot exceed MemoryBlock.Union.currentBits.
        private const int kLog2MaxMemoryBlockSize = 26;

        // Maximum memory block size.  Can exceed maximum memory block size if user requested more.
        private const long kMaxMemoryBlockSize = 1L << kLog2MaxMemoryBlockSize; // 64MB

        /// Minimum memory block size, 128KB.
        private const long kMinMemoryBlockSize = 128 * 1024;

        /// Maximum number of memory blocks.
        private const int kMaxNumBlocks = 64;

        // Bit mask (bit 31) of the memory block busy flag indicating whether the block is busy rewinding.
        private const int kBlockBusyRewindMask = 0x1 << 31;

        // Bit mask of the memory block busy flag indicating whether the block is busy allocating.
        private const int kBlockBusyAllocateMask = ~kBlockBusyRewindMask;

        private const int kCacheLineSize = 128;

        private AtomicSafetyHandle m_handle;
        private Spinner m_spinner;
        private NativeArray<MemoryBlock> m_block;
        private int m_last; // highest-index block that has memory to allocate from
        private int m_used; // highest-index block that we actually allocated from, since last rewind
        private byte m_enableBlockFree; // flag indicating if allocator enables individual block free
        private byte m_reachMaxBlockSize; // flag indicating if reach maximum block size

        public AtomicSafetyHandle Handle => m_handle.Clone();

        /// <summary>
        ///     Initializes the allocator. Must be called before first use.
        /// </summary>
        /// <param name="initialSizeInBytes">The initial capacity of the allocator, in bytes</param>
        /// <param name="enableBlockFree">A flag indicating if allocator enables individual block free</param>
        public void Initialize(int initialSizeInBytes, bool enableBlockFree = false)
        {
            m_handle = AtomicSafetyHandle.Create();
            m_spinner = default;
            m_block = new NativeArray<MemoryBlock>(kMaxNumBlocks);
            // Initial block size should be larger than min block size
            var blockSize = initialSizeInBytes > kMinMemoryBlockSize ? initialSizeInBytes : kMinMemoryBlockSize;
            m_block[0] = new MemoryBlock(blockSize);
            m_last = m_used = 0;
            m_enableBlockFree = enableBlockFree ? (byte)1 : (byte)0;
            m_reachMaxBlockSize = initialSizeInBytes >= kMaxMemoryBlockSize ? (byte)1 : (byte)0;
        }

        /// <summary>
        ///     Property to get and set enable block free flag, a flag indicating whether the allocator should enable individual
        ///     block to be freed.
        /// </summary>
        public bool EnableBlockFree
        {
            get => m_enableBlockFree != 0;
            set => m_enableBlockFree = value ? (byte)1 : (byte)0;
        }

        /// <summary>
        ///     Retrieves the number of memory blocks that the allocator has requested from the system.
        /// </summary>
        public int BlocksAllocated => m_last + 1;

        /// <summary>
        ///     Retrieves the size of the initial memory block, as requested in the Initialize function.
        /// </summary>
        public int InitialSizeInBytes => (int)m_block[0].m_bytes;

        /// <summary>
        ///     Retrieves the maximum memory block size.
        /// </summary>
        public long MaxMemoryBlockSize => kMaxMemoryBlockSize;

        /// <summary>
        ///     Retrieves the total bytes of the memory blocks allocated by this allocator.
        /// </summary>
        public long BytesAllocated
        {
            get
            {
                long totalBytes = 0;
                for (var i = 0; i <= m_last; i++)
                    totalBytes += m_block[i].m_bytes;
                return totalBytes;
            }
        }

        /// <summary>
        ///     Rewind the allocator; invalidate all allocations made from it, and potentially also free memory blocks
        ///     it has allocated from the system.
        /// </summary>
        public void Rewind()
        {
            m_handle.Bump();
            while (m_last > m_used) // *delete* all blocks we didn't even allocate from this time around.
                m_block[m_last--].Dispose();
            while (m_used > 0) // simply *rewind* all blocks we used in this update, to avoid allocating again, every update.
                m_block[m_used--].Rewind();
            m_block[0].Rewind();
        }

        /// <summary>
        ///     Dispose the allocator. This must be called to free the memory blocks that were allocated from the system.
        /// </summary>
        public void Dispose()
        {
            m_handle.Dispose();
            m_used = 0; // so that we delete all blocks in Rewind() on the next line
            Rewind();
            m_block[0].Dispose();
            m_block.Dispose();
            m_last = m_used = 0;
        }

        private int TryAllocate(ref Block block, int startIndex, int lastIndex, long alignedSize, long alignmentMask)
        {
            for (var best = startIndex; best <= lastIndex; best++)
            {
                Union oldUnion;
                Union readUnion = default;
                long begin = 0;
                var skip = false;
                readUnion.m_long = Interlocked.Read(ref m_block[best].m_union.m_long);
                do
                {
                    begin = (readUnion.m_current + alignmentMask) & ~alignmentMask;
                    if (begin + block.Bytes > m_block[best].m_bytes)
                    {
                        skip = true;
                        break;
                    }

                    oldUnion = readUnion;
                    Union newUnion = default;
                    newUnion.m_current = begin + alignedSize > m_block[best].m_bytes ? m_block[best].m_bytes : begin + alignedSize;
                    newUnion.m_allocCount = readUnion.m_allocCount + 1;
                    readUnion.m_long = Interlocked.CompareExchange(ref m_block[best].m_union.m_long, newUnion.m_long, oldUnion.m_long);
                } while (readUnion.m_long != oldUnion.m_long);

                if (skip)
                    continue;

                block.Range.Pointer = (IntPtr)(m_block[best].m_pointer + begin);
                block.AllocatedItems = block.Range.Items;
                Interlocked.MemoryBarrier();
                int oldUsed;
                int readUsed;
                int newUsed;
                readUsed = m_used;
                do
                {
                    oldUsed = readUsed;
                    newUsed = best > oldUsed ? best : oldUsed;
                    readUsed = Interlocked.CompareExchange(ref m_used, newUsed, oldUsed);
                } while (newUsed != oldUsed);

                return kErrorNone;
            }

            return kErrorBufferOverflow;
        }

        /// <summary>
        ///     Memory allocation Success status
        /// </summary>
        public const int kErrorNone = 0;

        /// <summary>
        ///     Memory allocation Buffer Overflow status
        /// </summary>
        public const int kErrorBufferOverflow = -1;

        /// <summary>
        ///     Try to allocate, free, or reallocate a block of memory. This is an public function, and
        ///     is not generally called by the user.
        /// </summary>
        /// <param name="block">The memory block to allocate, free, or reallocate</param>
        /// <returns>0 if successful. Otherwise, returns the error code from the allocator function.</returns>
        private int Try(ref Block block)
        {
            if (block.Range.Pointer == IntPtr.Zero)
            {
                // Make the alignment multiple of cacheline size
                var alignment = Math.Max(kCacheLineSize, block.Alignment);
                var extra = alignment != kCacheLineSize ? 1 : 0;
                const int cachelineMask = kCacheLineSize - 1;
                if (extra == 1)
                    alignment = (alignment + cachelineMask) & ~cachelineMask;

                // Adjust the size to be multiple of alignment, add extra alignment
                // to size if alignment is more than cacheline size
                var mask = alignment - 1L;
                var size = (block.Bytes + extra * alignment + mask) & ~mask;

                // Check all the blocks to see if any of them have enough memory
                var last = m_last;
                var error = TryAllocate(ref block, 0, m_last, size, mask);
                if (error == kErrorNone)
                    return error;

                // If that fails, allocate another block that's guaranteed big enough, and allocate from it.
                // Allocate twice as much as last time until it reaches MaxMemoryBlockSize, after that, increase
                // the block size by MaxMemoryBlockSize.
                m_spinner.Acquire();

                // After getting the lock, we must try to allocate again, because if many threads waited at
                // the lock, the first one allocates and when it unlocks, it's likely that there's space for the
                // other threads' allocations in the first thread's block.
                error = TryAllocate(ref block, last, m_last, size, mask);
                if (error == kErrorNone)
                {
                    m_spinner.Release();
                    return error;
                }

                var bytes = m_reachMaxBlockSize == 0 ? m_block[m_last].m_bytes << 1 : m_block[m_last].m_bytes + kMaxMemoryBlockSize;

                // if user asks more, skip smaller sizes
                bytes = Math.Max(bytes, size);
                m_reachMaxBlockSize = bytes >= kMaxMemoryBlockSize ? (byte)1 : (byte)0;
                m_block[m_last + 1] = new MemoryBlock(bytes);
                Interlocked.Increment(ref m_last);
                error = TryAllocate(ref block, m_last, m_last, size, mask);
                m_spinner.Release();
                return error;
            }

            // To free memory, no-op unless allocator enables individual block to be freed
            if (block.Range.Items == 0)
            {
                if (m_enableBlockFree != 0)
                {
                    for (var blockIndex = 0; blockIndex <= m_last; ++blockIndex)
                    {
                        if (m_block[blockIndex].Contains(block.Range.Pointer))
                        {
                            Union oldUnion;
                            Union readUnion = default;
                            readUnion.m_long = Interlocked.Read(ref m_block[blockIndex].m_union.m_long);
                            do
                            {
                                oldUnion = readUnion;
                                var newUnion = readUnion;
                                newUnion.m_allocCount--;
                                if (newUnion.m_allocCount == 0)
                                    newUnion.m_current = 0;
                                readUnion.m_long = Interlocked.CompareExchange(ref m_block[blockIndex].m_union.m_long, newUnion.m_long, oldUnion.m_long);
                            } while (readUnion.m_long != oldUnion.m_long);
                        }
                    }
                }

                return 0; // we could check to see if the pointer belongs to us, if we want to be strict about it.
            }

            return -1;
        }

        /// <summary>
        ///     Allocate a NativeArray of type T from memory that is guaranteed to remain valid until the end of the
        ///     next Update of this World. There is no need to Dispose the NativeArray so allocated. It is not possible
        ///     to free the memory by Disposing it - it is automatically freed after the end of the next Update for this
        ///     World.
        /// </summary>
        /// <typeparam name="T">The element type of the NativeArray to allocate.</typeparam>
        /// <param name="length">The length of the NativeArray to allocate, measured in elements.</param>
        /// <returns>The NativeArray allocated by this function.</returns>
        public NativeArray<T> Allocate<T>(int length) where T : unmanaged
        {
            Block block = default;
            block.Range.Pointer = IntPtr.Zero;
            block.Range.Items = length;
            block.BytesPerItem = Unsafe.SizeOf<T>();
            // Make the alignment multiple of cacheline size
            block.Alignment = (int)Math.Max(kCacheLineSize, NativeMemoryAllocator.AlignOf<T>());
            var error = Try(ref block);
            Debug.Assert(error == kErrorNone);
            return new NativeArray<T>((T*)block.Range.Pointer, length);
        }

        public void Free<T>(NativeArray<T> container) where T : unmanaged
        {
            if (!container.IsCreated)
                return;
            Block block = default;
            block.Range.Pointer = (IntPtr)container.Buffer;
            block.Range.Items = 0;
            var error = Try(ref block);
            Debug.Assert(error == kErrorNone);
        }
    }
}