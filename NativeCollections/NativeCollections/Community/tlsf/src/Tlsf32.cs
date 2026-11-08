using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Two-Level Segregated Fit memory allocator.
    /// </summary>
    /// <remarks>
    ///     https://github.com/mattconte/tlsf
    /// </remarks>
    internal static unsafe class Tlsf32
    {
        /// <summary>
        ///     The base-2 logarithm of the number of second-level indices.
        /// </summary>
        public const int SL_INDEX_COUNT_LOG2 = 5;

        /// <summary>
        ///     The base-2 logarithm of the required block alignment.
        /// </summary>
        public const int ALIGN_SIZE_LOG2 = 2;

        /// <summary>
        ///     The required alignment of allocated blocks, in bytes.
        /// </summary>
        public const int ALIGN_SIZE = 1 << ALIGN_SIZE_LOG2;

        /// <summary>
        ///     The maximum first-level index supported by the allocator.
        /// </summary>
        public const int FL_INDEX_MAX = 30;

        /// <summary>
        ///     The number of second-level indices.
        /// </summary>
        public const int SL_INDEX_COUNT = 1 << SL_INDEX_COUNT_LOG2;

        /// <summary>
        ///     The number of low-order size bits shifted when mapping a large block.
        /// </summary>
        public const int FL_INDEX_SHIFT = SL_INDEX_COUNT_LOG2 + ALIGN_SIZE_LOG2;

        /// <summary>
        ///     The number of first-level indices.
        /// </summary>
        public const int FL_INDEX_COUNT = FL_INDEX_MAX - FL_INDEX_SHIFT + 1;

        /// <summary>
        ///     The size threshold below which the small-block mapping path is used.
        /// </summary>
        public const int SMALL_BLOCK_SIZE = 1 << FL_INDEX_SHIFT;

        /// <summary>
        ///     The bit in a block header that indicates whether the block is free.
        /// </summary>
        public const uint BLOCK_HEADER_FREE_BIT = 1U << 0;

        /// <summary>
        ///     The bit in a block header that indicates whether the previous block is free.
        /// </summary>
        public const uint BLOCK_HEADER_PREV_FREE_BIT = 1U << 1;

        /// <summary>
        ///     The size of block header metadata, in bytes.
        /// </summary>
        public const uint BLOCK_HEADER_OVERHEAD = 4U;

        /// <summary>
        ///     The offset from a block header to its payload, in bytes.
        /// </summary>
        public const uint BLOCK_START_OFFSET = 8U;

        /// <summary>
        ///     The minimum block size supported by the allocator.
        /// </summary>
        public const uint BLOCK_SIZE_MIN = 12U;

        /// <summary>
        ///     The maximum block size supported by the allocator.
        /// </summary>
        public const uint BLOCK_SIZE_MAX = 1U << FL_INDEX_MAX;

        /// <summary>
        ///     Copies a block of memory from the source address to the destination address.
        /// </summary>
        /// <param name="dst">The destination address.</param>
        /// <param name="src">The source address.</param>
        /// <param name="size">The number of bytes to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memcpy(void* dst, void* src, uint size) => SpanHelpers.Copy(ref Unsafe.AsRef<byte>(dst), ref Unsafe.AsRef<byte>(src), Usize.FromU32Unchecked(size));

        /// <summary>
        ///     Returns the index of the most significant set bit in a 32-bit word.
        /// </summary>
        /// <param name="word">The word to inspect.</param>
        /// <returns>The zero-based bit index, or -1 when <paramref name="word" /> is zero.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TlsfFls(uint word)
        {
            var bit = 32.WrappingSub(BitOperationsHelpers.LeadingZeroCount(word));
            return bit.WrappingSub(1);
        }

        /// <summary>
        ///     Returns the index of the least significant set bit in a 32-bit word.
        /// </summary>
        /// <param name="word">The word to inspect.</param>
        /// <returns>The zero-based bit index, or -1 when <paramref name="word" /> is zero.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TlsfFfs(uint word)
        {
            var reverse = word & (~word).WrappingAdd(U32.FromI32Unchecked(1));
            var bit = 32.WrappingSub(BitOperationsHelpers.LeadingZeroCount(reverse));
            return bit.WrappingSub(1);
        }

        /// <summary>
        ///     Returns the index of the most significant set bit in a size value.
        /// </summary>
        /// <param name="size">The size value to inspect.</param>
        /// <returns>The zero-based bit index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TlsfFlsUsize(uint size) => TlsfFls(size);

        /// <summary>
        ///     Returns the smaller of two size values.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>The smaller value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfMin(uint a, uint b) => Math.Min(a, b);

        /// <summary>
        ///     Returns the larger of two size values.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns>The larger value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfMax(uint a, uint b) => Math.Max(a, b);

        /// <summary>
        ///     Returns the size of a block with its state flags removed.
        /// </summary>
        /// <param name="block">The block whose size is read.</param>
        /// <returns>The block size, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint BlockSize(BlockHeader* block) => block->Size & ~(BLOCK_HEADER_FREE_BIT | BLOCK_HEADER_PREV_FREE_BIT);

        /// <summary>
        ///     Sets a block size while preserving the block state flags.
        /// </summary>
        /// <param name="block">The block whose size is set.</param>
        /// <param name="size">The new block size, in bytes.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockSetSize(BlockHeader* block, uint size)
        {
            var oldSize = block->Size;
            block->Size = size | (oldSize & (BLOCK_HEADER_FREE_BIT | BLOCK_HEADER_PREV_FREE_BIT));
        }

        /// <summary>
        ///     Determines whether a block is the terminal block in the pool.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <returns><c>1</c> if the block is terminal; otherwise, <c>0</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BlockIsLast(BlockHeader* block) => BlockSize(block) == U32.FromI32Unchecked(0) ? 1 : 0;

        /// <summary>
        ///     Returns the free-state flag of a block.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <returns>The free-state flag, which is zero when the block is used and nonzero when it is free.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BlockIsFree(BlockHeader* block) => I32.FromU32Unchecked(block->Size & BLOCK_HEADER_FREE_BIT);

        /// <summary>
        ///     Marks a block as free.
        /// </summary>
        /// <param name="block">The block to mark.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockSetFree(BlockHeader* block) => block->Size |= BLOCK_HEADER_FREE_BIT;

        /// <summary>
        ///     Marks a block as used.
        /// </summary>
        /// <param name="block">The block to mark.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockSetUsed(BlockHeader* block) => block->Size &= ~BLOCK_HEADER_FREE_BIT;

        /// <summary>
        ///     Returns the flag indicating whether the previous physical block is free.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <returns>
        ///     The previous-free flag, which is zero when the previous block is used and nonzero when it is free.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BlockIsPrevFree(BlockHeader* block) => I32.FromU32Unchecked(block->Size & BLOCK_HEADER_PREV_FREE_BIT);

        /// <summary>
        ///     Marks the previous physical block as free in the block header.
        /// </summary>
        /// <param name="block">The block whose previous-block flag is set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockSetPrevFree(BlockHeader* block) => block->Size |= BLOCK_HEADER_PREV_FREE_BIT;

        /// <summary>
        ///     Marks the previous physical block as used in the block header.
        /// </summary>
        /// <param name="block">The block whose previous-block flag is cleared.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockSetPrevUsed(BlockHeader* block) => block->Size &= ~BLOCK_HEADER_PREV_FREE_BIT;

        /// <summary>
        ///     Returns the block header associated with a payload pointer.
        /// </summary>
        /// <param name="ptr">The payload pointer.</param>
        /// <returns>The block header associated with <paramref name="ptr" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockFromPtr(void* ptr) => UnsafeHelpers.SubtractByteOffset<BlockHeader>(ptr, Isize.FromU32Unchecked(BLOCK_START_OFFSET));

        /// <summary>
        ///     Returns the payload pointer associated with a block header.
        /// </summary>
        /// <param name="block">The block header.</param>
        /// <returns>The payload pointer for <paramref name="block" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* BlockToPtr(BlockHeader* block) => UnsafeHelpers.AddByteOffset(block, Isize.FromU32Unchecked(BLOCK_START_OFFSET));

        /// <summary>
        ///     Returns the block header at a byte offset from a pointer.
        /// </summary>
        /// <param name="ptr">The base pointer.</param>
        /// <param name="size">The byte offset from <paramref name="ptr" />.</param>
        /// <returns>The block header at the specified offset.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* OffsetToBlock(void* ptr, uint size) => (BlockHeader*)Isize.FromPtrUnchecked(ptr).WrappingAdd(Isize.FromU32Unchecked(size));

        /// <summary>
        ///     Returns the previous physical block.
        /// </summary>
        /// <param name="block">The block whose predecessor is read.</param>
        /// <returns>The previous physical block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockPrev(BlockHeader* block) => block->PrevPhysBlock;

        /// <summary>
        ///     Returns the next physical block.
        /// </summary>
        /// <param name="block">The block whose successor is read.</param>
        /// <returns>The next physical block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockNext(BlockHeader* block)
        {
            var next = OffsetToBlock(BlockToPtr(block), BlockSize(block).WrappingSub(BLOCK_HEADER_OVERHEAD));
            return next;
        }

        /// <summary>
        ///     Links the next physical block back to the specified block.
        /// </summary>
        /// <param name="block">The block to link.</param>
        /// <returns>The next physical block after <paramref name="block" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockLinkNext(BlockHeader* block)
        {
            var next = BlockNext(block);
            next->PrevPhysBlock = block;
            return next;
        }

        /// <summary>
        ///     Marks a block as free and updates the next block metadata.
        /// </summary>
        /// <param name="block">The block to mark as free.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockMarkAsFree(BlockHeader* block)
        {
            var next = BlockLinkNext(block);
            BlockSetPrevFree(next);
            BlockSetFree(block);
        }

        /// <summary>
        ///     Marks a block as used and updates the next block metadata.
        /// </summary>
        /// <param name="block">The block to mark as used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockMarkAsUsed(BlockHeader* block)
        {
            var next = BlockNext(block);
            BlockSetPrevUsed(next);
            BlockSetUsed(block);
        }

        /// <summary>
        ///     Rounds a value up to the specified alignment.
        /// </summary>
        /// <param name="x">The value to align.</param>
        /// <param name="align">The alignment boundary.</param>
        /// <returns>The smallest aligned value greater than or equal to <paramref name="x" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignUp(uint x, uint align) => x.WrappingAdd(align.WrappingSub(U32.FromI32Unchecked(1))) & ~align.WrappingSub(U32.FromI32Unchecked(1));

        /// <summary>
        ///     Rounds a value down to the specified alignment.
        /// </summary>
        /// <param name="x">The value to align.</param>
        /// <param name="align">The alignment boundary.</param>
        /// <returns>The largest aligned value less than or equal to <paramref name="x" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AlignDown(uint x, uint align) => x.WrappingSub(x & align.WrappingSub(U32.FromI32Unchecked(1)));

        /// <summary>
        ///     Rounds a pointer up to the specified alignment.
        /// </summary>
        /// <param name="ptr">The pointer to align.</param>
        /// <param name="align">The alignment boundary.</param>
        /// <returns>The aligned pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignPtr(void* ptr, uint align)
        {
            var aligned = Isize.FromPtrUnchecked(ptr).WrappingAdd(Isize.FromU32Unchecked(align.WrappingSub(U32.FromI32Unchecked(1)))) & ~Isize.FromU32Unchecked(align.WrappingSub(U32.FromI32Unchecked(1)));
            return (void*)aligned;
        }

        /// <summary>
        ///     Adjusts a requested allocation size for the allocator alignment and minimum block size.
        /// </summary>
        /// <param name="size">The requested allocation size, in bytes.</param>
        /// <param name="align">The required alignment, in bytes.</param>
        /// <returns>
        ///     The adjusted allocation size, or zero when the request is zero or exceeds the supported range.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AdjustRequestSize(uint size, uint align)
        {
            var adjust = U32.FromI32Unchecked(0);
            if (size != U32.FromI32Unchecked(0))
            {
                var aligned = AlignUp(size, align);
                if (aligned < BLOCK_SIZE_MAX)
                    adjust = TlsfMax(aligned, BLOCK_SIZE_MIN);
            }

            return adjust;
        }

        /// <summary>
        ///     Maps a block size to the first-level and second-level indices used for insertion.
        /// </summary>
        /// <param name="size">The block size to map.</param>
        /// <param name="fli">A pointer that receives the first-level index.</param>
        /// <param name="sli">A pointer that receives the second-level index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MappingInsert(uint size, int* fli, int* sli)
        {
            int fl, sl;
            if (size < U32.FromI32Unchecked(SMALL_BLOCK_SIZE))
            {
                fl = 0;
                sl = I32.FromU32Unchecked(size) / (SMALL_BLOCK_SIZE / SL_INDEX_COUNT);
            }
            else
            {
                fl = TlsfFlsUsize(size);
                sl = I32.FromU32Unchecked(size >> fl.WrappingSub(SL_INDEX_COUNT_LOG2)) ^ (1 << SL_INDEX_COUNT_LOG2);
                fl = fl.WrappingSub(FL_INDEX_SHIFT.WrappingSub(1));
            }

            Unsafe.AsRef<int>(fli) = fl;
            Unsafe.AsRef<int>(sli) = sl;
        }

        /// <summary>
        ///     Maps a block size to the first-level and second-level indices used for searching.
        /// </summary>
        /// <param name="size">The block size to map.</param>
        /// <param name="fli">A pointer that receives the first-level index.</param>
        /// <param name="sli">A pointer that receives the second-level index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MappingSearch(uint size, int* fli, int* sli)
        {
            if (size >= U32.FromI32Unchecked(SMALL_BLOCK_SIZE))
            {
                var round = U32.FromI32Unchecked((1 << TlsfFlsUsize(size).WrappingSub(SL_INDEX_COUNT_LOG2)).WrappingSub(1));
                size = size.WrappingAdd(round);
            }

            MappingInsert(size, fli, sli);
        }

        /// <summary>
        ///     Finds a suitable free block by using the allocator bitmaps.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="fli">A pointer to the first-level index to search and receive the selected index.</param>
        /// <param name="sli">A pointer to the second-level index to search and receive the selected index.</param>
        /// <returns>A suitable free block, or <see langword="null" /> when no suitable block is available.</returns>
        public static BlockHeader* SearchSuitableBlock(Control* control, int* fli, int* sli)
        {
            var fl = Unsafe.AsRef<int>(fli);
            var sl = Unsafe.AsRef<int>(sli);
            var slMap = Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(fl)) & (~0U << sl);
            if (slMap == U32.FromI32Unchecked(0))
            {
                var flMap = control->FlBitmap & (~0U << fl.WrappingAdd(1));
                if (flMap == U32.FromI32Unchecked(0))
                    return null;

                fl = TlsfFfs(flMap);
                Unsafe.AsRef<int>(fli) = fl;
                slMap = Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(fl));
            }

            sl = TlsfFfs(slMap);
            Unsafe.AsRef<int>(sli) = sl;
            return Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[fl]), Isize.FromI32Unchecked(sl)).Value;
        }

        /// <summary>
        ///     Removes a block from a free list and updates the allocator bitmaps.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to remove.</param>
        /// <param name="fl">The first-level index of the free list.</param>
        /// <param name="sl">The second-level index of the free list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFreeBlock(Control* control, BlockHeader* block, int fl, int sl)
        {
            var prev = block->PrevFree;
            var next = block->NextFree;
            next->PrevFree = prev;
            prev->NextFree = next;
            if (Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[fl]), Isize.FromI32Unchecked(sl)).Value == block)
            {
                Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[fl]), Isize.FromI32Unchecked(sl)) = BlockHeaderPtr.From(next);
                if (next == &control->BlockNull)
                {
                    Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(fl)) &= ~(1U << sl);
                    if (Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(fl)) == U32.FromI32Unchecked(0))
                        control->FlBitmap &= ~(1U << fl);
                }
            }
        }

        /// <summary>
        ///     Inserts a block at the head of a free list and updates the allocator bitmaps.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to insert.</param>
        /// <param name="fl">The first-level index of the free list.</param>
        /// <param name="sl">The second-level index of the free list.</param>
        public static void InsertFreeBlock(Control* control, BlockHeader* block, int fl, int sl)
        {
            var current = Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[fl]), Isize.FromI32Unchecked(sl)).Value;
            block->NextFree = current;
            block->PrevFree = &control->BlockNull;
            current->PrevFree = block;
            Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[fl]), Isize.FromI32Unchecked(sl)) = BlockHeaderPtr.From(block);
            control->FlBitmap |= 1U << fl;
            Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(fl)) |= 1U << sl;
        }

        /// <summary>
        ///     Removes a block from the free list selected by its size.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockRemove(Control* control, BlockHeader* block)
        {
            int fl, sl;
            MappingInsert(BlockSize(block), &fl, &sl);
            RemoveFreeBlock(control, block, fl, sl);
        }

        /// <summary>
        ///     Inserts a block into the free list selected by its size.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to insert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockInsert(Control* control, BlockHeader* block)
        {
            int fl, sl;
            MappingInsert(BlockSize(block), &fl, &sl);
            InsertFreeBlock(control, block, fl, sl);
        }

        /// <summary>
        ///     Determines whether a block contains enough space to be split.
        /// </summary>
        /// <param name="block">The block to inspect.</param>
        /// <param name="size">The size requested for the first part of the split.</param>
        /// <returns><c>1</c> if the block can be split; otherwise, <c>0</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BlockCanSplit(BlockHeader* block, uint size) => BlockSize(block) >= U32.FromI32Unchecked(Unsafe.SizeOf<BlockHeader>()).WrappingAdd(size) ? 1 : 0;

        /// <summary>
        ///     Splits a block and returns the remaining block.
        /// </summary>
        /// <param name="block">The block to split.</param>
        /// <param name="size">The size of the first part of the split.</param>
        /// <returns>The remaining block created by the split.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockSplit(BlockHeader* block, uint size)
        {
            var remaining = OffsetToBlock(BlockToPtr(block), size.WrappingSub(BLOCK_HEADER_OVERHEAD));
            var remainSize = BlockSize(block).WrappingSub(size.WrappingAdd(BLOCK_HEADER_OVERHEAD));
            BlockSetSize(remaining, remainSize);
            BlockSetSize(block, size);
            BlockMarkAsFree(remaining);
            return remaining;
        }

        /// <summary>
        ///     Absorbs a block into the preceding block.
        /// </summary>
        /// <param name="prev">The preceding block that receives the absorbed space.</param>
        /// <param name="block">The block to absorb.</param>
        /// <returns>The enlarged preceding block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockAbsorb(BlockHeader* prev, BlockHeader* block)
        {
            prev->Size = prev->Size.WrappingAdd(BlockSize(block).WrappingAdd(BLOCK_HEADER_OVERHEAD));
            BlockLinkNext(prev);
            return prev;
        }

        /// <summary>
        ///     Merges a block with the previous block when the previous block is free.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The block to merge.</param>
        /// <returns>The resulting block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockMergePrev(Control* control, BlockHeader* block)
        {
            if (BlockIsPrevFree(block) != 0)
            {
                var prev = BlockPrev(block);
                BlockRemove(control, prev);
                block = BlockAbsorb(prev, block);
            }

            return block;
        }

        /// <summary>
        ///     Merges a block with the next block when the next block is free.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The block to merge.</param>
        /// <returns>The resulting block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockMergeNext(Control* control, BlockHeader* block)
        {
            var next = BlockNext(block);
            if (BlockIsFree(next) != 0)
            {
                BlockRemove(control, next);
                block = BlockAbsorb(block, next);
            }

            return block;
        }

        /// <summary>
        ///     Trims a free block and inserts any remaining space into a free list.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to trim.</param>
        /// <param name="size">The size to retain in the block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockTrimFree(Control* control, BlockHeader* block, uint size)
        {
            if (BlockCanSplit(block, size) != 0)
            {
                var remainingBlock = BlockSplit(block, size);
                BlockLinkNext(block);
                BlockSetPrevFree(remainingBlock);
                BlockInsert(control, remainingBlock);
            }
        }

        /// <summary>
        ///     Trims a used block and inserts any remaining space into a free list.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The used block to trim.</param>
        /// <param name="size">The size to retain in the block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BlockTrimUsed(Control* control, BlockHeader* block, uint size)
        {
            if (BlockCanSplit(block, size) != 0)
            {
                var remainingBlock = BlockSplit(block, size);
                BlockSetPrevUsed(remainingBlock);
                remainingBlock = BlockMergeNext(control, remainingBlock);
                BlockInsert(control, remainingBlock);
            }
        }

        /// <summary>
        ///     Trims the leading space from a free block and inserts that space into a free list.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to trim.</param>
        /// <param name="size">The size of the leading space to remove.</param>
        /// <returns>The remaining block after the leading space is removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockTrimFreeLeading(Control* control, BlockHeader* block, uint size)
        {
            var remainingBlock = block;
            if (BlockCanSplit(block, size) != 0)
            {
                remainingBlock = BlockSplit(block, size.WrappingSub(BLOCK_HEADER_OVERHEAD));
                BlockSetPrevFree(remainingBlock);
                BlockLinkNext(block);
                BlockInsert(control, block);
            }

            return remainingBlock;
        }

        /// <summary>
        ///     Locates and removes a suitable free block for the requested size.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="size">The requested block size, in bytes.</param>
        /// <returns>A suitable free block, or <see langword="null" /> when none is available.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BlockHeader* BlockLocateFree(Control* control, uint size)
        {
            int fl = 0, sl = 0;
            BlockHeader* block = null;
            if (size != U32.FromI32Unchecked(0))
            {
                MappingSearch(size, &fl, &sl);
                if (fl < FL_INDEX_COUNT)
                    block = SearchSuitableBlock(control, &fl, &sl);
            }

            if (block != null)
                RemoveFreeBlock(control, block, fl, sl);

            return block;
        }

        /// <summary>
        ///     Prepares a free block for use and returns its payload pointer.
        /// </summary>
        /// <param name="control">The allocator control structure.</param>
        /// <param name="block">The free block to prepare.</param>
        /// <param name="size">The requested block size, in bytes.</param>
        /// <returns>
        ///     The payload pointer for the prepared block, or
        ///     <see langword="null" /> when <paramref name="block" /> is null.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* BlockPrepareUsed(Control* control, BlockHeader* block, uint size)
        {
            void* p = null;
            if (block != null)
            {
                BlockTrimFree(control, block, size);
                BlockMarkAsUsed(block);
                p = BlockToPtr(block);
            }

            return p;
        }

        /// <summary>
        ///     Initializes the allocator control structure and its free-list sentinels.
        /// </summary>
        /// <param name="control">The allocator control structure to initialize.</param>
        public static void ControlConstruct(Control* control)
        {
            int i, j;
            control->BlockNull.NextFree = &control->BlockNull;
            control->BlockNull.PrevFree = &control->BlockNull;
            control->FlBitmap = U32.FromI32Unchecked(0);
            for (i = 0; i < FL_INDEX_COUNT; i = i.WrappingAdd(1))
            {
                Unsafe.Add(ref Unsafe.AsRef<uint>(control->SlBitmap), Isize.FromI32Unchecked(i)) = U32.FromI32Unchecked(0);
                for (j = 0; j < SL_INDEX_COUNT; j = j.WrappingAdd(1))
                    Unsafe.Add(ref Unsafe.AsRef<BlockHeaderPtr>(control->Blocks[i]), Isize.FromI32Unchecked(j)) = BlockHeaderPtr.From(&control->BlockNull);
            }
        }

        /// <summary>
        ///     Returns the size of the allocation associated with a payload pointer.
        /// </summary>
        /// <param name="ptr">The payload pointer.</param>
        /// <returns>The allocation size, in bytes, or zero when <paramref name="ptr" /> is null.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfBlockSize(void* ptr)
        {
            var size = U32.FromI32Unchecked(0);
            if (ptr != null)
            {
                var block = BlockFromPtr(ptr);
                size = BlockSize(block);
            }

            return size;
        }

        /// <summary>
        ///     Returns the size of the allocator control structure.
        /// </summary>
        /// <returns>The size of the control structure, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfSize() => U32.FromI32Unchecked(Unsafe.SizeOf<Control>());

        /// <summary>
        ///     Returns the required allocation alignment.
        /// </summary>
        /// <returns>The required alignment, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfAlignSize() => U32.FromI32Unchecked(ALIGN_SIZE);

        /// <summary>
        ///     Returns the minimum supported block size.
        /// </summary>
        /// <returns>The minimum block size, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfBlockSizeMin() => BLOCK_SIZE_MIN;

        /// <summary>
        ///     Returns the maximum supported block size.
        /// </summary>
        /// <returns>The maximum block size, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfBlockSizeMax() => BLOCK_SIZE_MAX;

        /// <summary>
        ///     Returns the overhead reserved at the boundaries of a memory pool.
        /// </summary>
        /// <returns>The pool boundary overhead, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfPoolOverhead() => U32.FromI32Unchecked(2).WrappingMul(BLOCK_HEADER_OVERHEAD);

        /// <summary>
        ///     Returns the overhead reserved for an allocation block header.
        /// </summary>
        /// <returns>The allocation overhead, in bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TlsfAllocOverhead() => BLOCK_HEADER_OVERHEAD;

        /// <summary>
        ///     Adds a memory pool to an initialized allocator.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="mem">The memory to use for the pool.</param>
        /// <param name="bytes">The size of the memory region, in bytes.</param>
        /// <returns>
        ///     The start of the added pool, or <see langword="null" />
        ///     when the memory is invalid or the pool size is unsupported.
        /// </returns>
        /// <remarks>
        ///     A region whose aligned size equals <see cref="BLOCK_SIZE_MAX" /> (1 GiB) is not
        ///     representable by the free-list index; this method throws
        ///     <see cref="ArgumentOutOfRangeException" /> for that exact size instead of returning
        ///     <see langword="null" /> (the upstream TLSF implementation performs an out-of-bounds write).
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the aligned pool size equals <see cref="BLOCK_SIZE_MAX" />.
        /// </exception>
        public static void* TlsfAddPool(void* tlsf, void* mem, uint bytes)
        {
            BlockHeader* block;
            BlockHeader* next;
            var poolOverhead = TlsfPoolOverhead();
            var poolBytes = AlignDown(bytes.WrappingSub(poolOverhead), U32.FromI32Unchecked(ALIGN_SIZE));
            if (I64.FromIsizeUnchecked(Isize.FromPtrUnchecked(mem)) % I64.FromI32Unchecked(ALIGN_SIZE) != I64.FromI32Unchecked(0))
                return null;

            if (poolBytes < BLOCK_SIZE_MIN || poolBytes > BLOCK_SIZE_MAX)
                return null;

            block = OffsetToBlock(mem, BLOCK_HEADER_OVERHEAD.WrappingNeg());
            BlockSetSize(block, poolBytes);
            BlockSetFree(block);
            BlockSetPrevUsed(block);
            BlockInsert((Control*)tlsf, block);
            next = BlockLinkNext(block);
            BlockSetSize(next, U32.FromI32Unchecked(0));
            BlockSetUsed(next);
            BlockSetPrevFree(next);
            return mem;
        }

        /// <summary>
        ///     Removes a memory pool from an allocator.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="pool">The start of the pool to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TlsfRemovePool(void* tlsf, void* pool)
        {
            var control = (Control*)tlsf;
            var block = OffsetToBlock(pool, BLOCK_HEADER_OVERHEAD.WrappingNeg());
            int fl = 0, sl = 0;
            MappingInsert(BlockSize(block), &fl, &sl);
            RemoveFreeBlock(control, block, fl, sl);
        }

        /// <summary>
        ///     Initializes an allocator control structure at an aligned memory address.
        /// </summary>
        /// <param name="mem">The memory at which to initialize the allocator.</param>
        /// <returns>
        ///     The initialized allocator address, or <see langword="null" />
        ///     when <paramref name="mem" /> is not correctly aligned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* TlsfCreate(void* mem)
        {
            if (Isize.FromPtrUnchecked(mem) % Isize.FromI32Unchecked(ALIGN_SIZE) != Isize.FromI32Unchecked(0))
                return null;

            ControlConstruct((Control*)mem);
            return mem;
        }

        /// <summary>
        ///     Initializes an allocator and adds a memory pool after its control structure.
        /// </summary>
        /// <param name="mem">The memory at which to initialize the allocator.</param>
        /// <param name="bytes">The total memory size, in bytes.</param>
        /// <returns>
        ///     The initialized allocator address, or <see langword="null" />
        ///     when allocator or pool initialization fails.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* TlsfCreateWithPool(void* mem, uint bytes)
        {
            var tlsf = TlsfCreate(mem);
            if (tlsf == null)
                return null;

            return TlsfAddPool(tlsf, UnsafeHelpers.AddByteOffset(mem, Isize.FromU32Unchecked(TlsfSize())), bytes.WrappingSub(TlsfSize())) == null ? null : tlsf;
        }

        /// <summary>
        ///     Returns the address immediately following the allocator control structure.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <returns>The start address available for the allocator pool.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* TlsfGetPool(void* tlsf) => UnsafeHelpers.AddByteOffset(tlsf, Isize.FromU32Unchecked(TlsfSize()));

        /// <summary>
        ///     Allocates a block of memory from the allocator.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="size">The requested allocation size, in bytes.</param>
        /// <returns>
        ///     The allocated payload pointer, or <see langword="null" /> when the request cannot be satisfied.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* TlsfMalloc(void* tlsf, uint size)
        {
            var control = (Control*)tlsf;
            var adjust = AdjustRequestSize(size, U32.FromI32Unchecked(ALIGN_SIZE));
            var block = BlockLocateFree(control, adjust);
            return BlockPrepareUsed(control, block, adjust);
        }

        /// <summary>
        ///     Allocates a block of memory with the specified alignment.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="align">The required alignment, in bytes.</param>
        /// <param name="size">The requested allocation size, in bytes.</param>
        /// <returns>
        ///     The aligned payload pointer, or <see langword="null" /> when the request cannot be satisfied.
        /// </returns>
        public static void* TlsfMemalign(void* tlsf, uint align, uint size)
        {
            var control = (Control*)tlsf;
            var adjust = AdjustRequestSize(size, U32.FromI32Unchecked(ALIGN_SIZE));
            var sizeWithGap = AdjustRequestSize(adjust.WrappingAdd(align).WrappingAdd(U32.FromI32Unchecked(Unsafe.SizeOf<BlockHeader>())), align);
            var alignedSize = adjust != U32.FromI32Unchecked(0) && align > U32.FromI32Unchecked(ALIGN_SIZE) ? sizeWithGap : adjust;
            var block = BlockLocateFree(control, alignedSize);
            if (block != null)
            {
                var ptr = BlockToPtr(block);
                var aligned = AlignPtr(ptr, align);
                var gap = U32.FromIsizeUnchecked(Isize.FromPtrUnchecked(aligned).WrappingSub(Isize.FromPtrUnchecked(ptr)));
                if (gap != U32.FromI32Unchecked(0) && gap < U32.FromI32Unchecked(Unsafe.SizeOf<BlockHeader>()))
                {
                    var gapRemain = U32.FromI32Unchecked(Unsafe.SizeOf<BlockHeader>()).WrappingSub(gap);
                    var offset = TlsfMax(gapRemain, align);
                    var nextAligned = (void*)Isize.FromPtrUnchecked(aligned).WrappingAdd(Isize.FromU32Unchecked(offset));
                    aligned = AlignPtr(nextAligned, align);
                    gap = U32.FromIsizeUnchecked(Isize.FromPtrUnchecked(aligned).WrappingSub(Isize.FromPtrUnchecked(ptr)));
                }

                if (gap != U32.FromI32Unchecked(0))
                    block = BlockTrimFreeLeading(control, block, gap);
            }

            return BlockPrepareUsed(control, block, adjust);
        }

        /// <summary>
        ///     Returns an allocation to the allocator and merges adjacent free blocks.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="ptr">The payload pointer to release.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TlsfFree(void* tlsf, void* ptr)
        {
            if (ptr != null)
            {
                var control = (Control*)tlsf;
                var block = BlockFromPtr(ptr);
                BlockMarkAsFree(block);
                block = BlockMergePrev(control, block);
                block = BlockMergeNext(control, block);
                BlockInsert(control, block);
            }
        }

        /// <summary>
        ///     Resizes an allocation, moving it when the existing block cannot be expanded in place.
        /// </summary>
        /// <param name="tlsf">The allocator control structure.</param>
        /// <param name="ptr">The payload pointer to resize.</param>
        /// <param name="size">The requested allocation size, in bytes.</param>
        /// <returns>
        ///     The resized payload pointer, or <see langword="null" /> when the allocation cannot be created.
        /// </returns>
        public static void* TlsfRealloc(void* tlsf, void* ptr, uint size)
        {
            var control = (Control*)tlsf;
            void* p = null;
            if (ptr != null && size == U32.FromI32Unchecked(0))
            {
                TlsfFree(tlsf, ptr);
            }
            else if (ptr == null)
            {
                p = TlsfMalloc(tlsf, size);
            }
            else
            {
                var block = BlockFromPtr(ptr);
                var next = BlockNext(block);
                var curSize = BlockSize(block);
                var combined = curSize.WrappingAdd(BlockSize(next)).WrappingAdd(BLOCK_HEADER_OVERHEAD);
                var adjust = AdjustRequestSize(size, U32.FromI32Unchecked(ALIGN_SIZE));
                if (adjust > curSize && (BlockIsFree(next) == 0 || adjust > combined))
                {
                    p = TlsfMalloc(tlsf, size);
                    if (p != null)
                    {
                        var minSize = TlsfMin(curSize, size);
                        Memcpy(p, ptr, minSize);
                        TlsfFree(tlsf, ptr);
                    }
                }
                else
                {
                    if (adjust > curSize)
                    {
                        BlockMergeNext(control, block);
                        BlockMarkAsUsed(block);
                    }

                    BlockTrimUsed(control, block, adjust);
                    p = ptr;
                }
            }

            return p;
        }

        /// <summary>
        ///     Stores the metadata for one TLSF block, including its physical and free-list links.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BlockHeader
        {
            /// <summary>
            ///     The previous physical block in the memory pool.
            /// </summary>
            public BlockHeader* PrevPhysBlock;

            /// <summary>
            ///     The block size together with the free-state flags.
            /// </summary>
            public uint Size;

            /// <summary>
            ///     The next block in the free list.
            /// </summary>
            public BlockHeader* NextFree;

            /// <summary>
            ///     The previous block in the free list.
            /// </summary>
            public BlockHeader* PrevFree;
        }

        /// <summary>
        ///     Stores allocator state, including bitmaps, the sentinel block, and free-list heads.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Control
        {
            /// <summary>
            ///     The sentinel block used to terminate free lists.
            /// </summary>
            public BlockHeader BlockNull;

            /// <summary>
            ///     The bitmap of first-level indices that contain free blocks.
            /// </summary>
            public uint FlBitmap;

            /// <summary>
            ///     The bitmaps of second-level indices for each first-level index.
            /// </summary>
            public fixed uint SlBitmap[FL_INDEX_COUNT];

            /// <summary>
            ///     The free-list heads indexed by first-level and second-level indices.
            /// </summary>
            public Blocks Blocks;
        }

        /// <summary>
        ///     Stores the raw storage for the free-list head pointers.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Blocks
        {
            /// <summary>
            ///     The raw storage used for the free-list head pointers.
            /// </summary>
            public fixed uint Headers[FL_INDEX_COUNT * SL_INDEX_COUNT];

            /// <summary>
            ///     Gets a pointer to the free-list head at the specified first-level index.
            /// </summary>
            /// <param name="index">The first-level index.</param>
            /// <value>A pointer to the free-list head for <paramref name="index" />.</value>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="index" /> is negative or
            ///     greater than or equal to <see cref="FL_INDEX_COUNT" />.
            /// </exception>
            public BlockHeaderPtr* this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(FL_INDEX_COUNT), ExceptionArgument.index);
                    return UnsafeHelpers.Add(ref Unsafe.As<Blocks, BlockHeaderPtr>(ref this), Isize.FromI32Unchecked(index.WrappingMul(SL_INDEX_COUNT)));
                }
            }
        }

        /// <summary>
        ///     Wraps a pointer to a TLSF block header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct BlockHeaderPtr
        {
            /// <summary>
            ///     The wrapped block-header pointer.
            /// </summary>
            public readonly BlockHeader* Value;

            /// <summary>
            ///     Initializes a new instance of this class with the specified block-header pointer.
            /// </summary>
            /// <param name="value">The block-header pointer to wrap.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private BlockHeaderPtr(BlockHeader* value) => Value = value;

            /// <summary>
            ///     Creates a wrapper for the specified block-header pointer.
            /// </summary>
            /// <param name="value">The block-header pointer to wrap.</param>
            /// <returns>A wrapper for the specified block-header pointer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static BlockHeaderPtr From(BlockHeader* value) => new(value);
        }
    }
}