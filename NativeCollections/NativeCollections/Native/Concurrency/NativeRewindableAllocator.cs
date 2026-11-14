using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Allocates unmanaged memory concurrently and reclaims it in bulk.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Allocations belong to the allocator. Keep pointers and views only until the next
    ///         <see cref="Rewind" /> or <see cref="Dispose" />. Use those operations to reclaim storage.
    ///     </para>
    ///     <para>
    ///         Copies share the same allocator. Dispose once across all copies. Use an instance returned
    ///         by <see cref="Create" /> for allocation and rewind, and stop using it and its copies after
    ///         disposal. <see cref="IsCreated" /> reports whether the value holds a pointer.
    ///     </para>
    ///     <para>
    ///         Standard storage allocation and adaptive thread chunks by OpenAI Codex.
    ///     </para>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None | FromType.Community | FromType.C)]
    public readonly unsafe struct NativeRewindableAllocator : IIsCreated, IDisposable, IEquatable<NativeRewindableAllocator>
    {
        /// <summary>
        ///     The base-two exponent of the standard page size.
        /// </summary>
        private const int PAGE_SHIFT = 16;

        /// <summary>
        ///     The standard page size and alignment in bytes.
        /// </summary>
        private const nuint PAGE_SIZE = 1U << PAGE_SHIFT;

        /// <summary>
        ///     The largest request served by a thread slot.
        /// </summary>
        private const nuint HALF_PAGE_SIZE = PAGE_SIZE / 2;

        /// <summary>
        ///     The largest alignment mask accepted by the small-allocation fast path.
        /// </summary>
        private const nuint HALF_PAGE_MASK = HALF_PAGE_SIZE - 1;

        /// <summary>
        ///     The base-two exponent of the smallest thread chunk.
        /// </summary>
        private const int CHUNK_SHIFT = 12;

        /// <summary>
        ///     The initial size of a thread's chunk in bytes.
        /// </summary>
        private const nuint MIN_CHUNK_SIZE = 1U << CHUNK_SHIFT;

        /// <summary>
        ///     The number of split levels below a standard page.
        /// </summary>
        private const int CHUNK_ORDERS = PAGE_SHIFT - CHUNK_SHIFT;

        /// <summary>
        ///     The number of size orders added to a thread's preferred grant after refill.
        /// </summary>
        private const int CHUNK_GROWTH_ORDERS = 2;

        /// <summary>
        ///     A cursor for an empty slot, large enough to subtract any small request without wrapping.
        /// </summary>
        private const nuint EMPTY_CURSOR = HALF_PAGE_SIZE;

        /// <summary>
        ///     A lower bound above every result from the empty cursor.
        /// </summary>
        private const nuint EMPTY_FLOOR = PAGE_SIZE;

        /// <summary>
        ///     The low address bits within a standard page.
        /// </summary>
        private const nuint PAGE_MASK = PAGE_SIZE - 1;

        /// <summary>
        ///     The size order of a half page.
        /// </summary>
        private const int HALF_CHUNK_ORDER = CHUNK_ORDERS - 1;

        /// <summary>
        ///     The availability bit for an unassigned half page.
        /// </summary>
        private const uint HALF_CHUNK_BIT = 1U << HALF_CHUNK_ORDER;

        /// <summary>
        ///     The combined limit for standard pages and retained dedicated segments.
        /// </summary>
        private const nuint PAGE_LIMIT = 8192;

        /// <summary>
        ///     The maximum number of retained dedicated segments.
        /// </summary>
        private const nuint DEDICATED_LIMIT = 255;

        /// <summary>
        ///     The minimum initial standard capacity.
        /// </summary>
        private const nuint MIN_STANDARD_BYTES = 4 * PAGE_SIZE;

        /// <summary>
        ///     The largest additional standard segment.
        /// </summary>
        private const nuint MAX_STANDARD_GROWTH = 64 * 1024 * 1024;

        /// <summary>
        ///     The descriptor capacity for geometric growth through the full page budget.
        /// </summary>
        private const nuint STANDARD_SEGMENT_SLOTS = 24;

        /// <summary>
        ///     The number of cache-line-separated slots in each bank.
        /// </summary>
        private const uint SHARD_COUNT = 64U;

        /// <summary>
        ///     The mask selecting a slot index within either bank from a managed thread ID.
        /// </summary>
        private const uint SHARD_MASK = SHARD_COUNT - 1U;

        /// <summary>
        ///     The number of thread slots across the two banks.
        /// </summary>
        private const uint TOTAL_SHARDS = SHARD_COUNT << 1;

        /// <summary>
        ///     The number of 64-bit storage units in each bitmap, including on 32-bit targets.
        /// </summary>
        private const int BITMAP_STORAGE_WORDS = 4;

        /// <summary>
        ///     The base-two logarithm of the bit width used for bitmap scans.
        /// </summary>
        private static int BitmapWordShift
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Environment.Is64BitProcess ? 6 : 5;
        }

        /// <summary>
        ///     The bit index mask within a bitmap scan word.
        /// </summary>
        private static int BitmapWordMask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Environment.Is64BitProcess ? 63 : 31;
        }

        /// <summary>
        ///     The number of native-width scan words in each bitmap.
        /// </summary>
        private static nuint BitmapWordCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Environment.Is64BitProcess ? Usize.FromI32Unchecked(4) : Usize.FromI32Unchecked(8);
        }

        /// <summary>
        ///     The number of alignment thresholds from one page through the largest 64-bit alignment.
        /// </summary>
        private const int ALIGNMENT_ROWS = sizeof(ulong) * 8 - PAGE_SHIFT;

        /// <summary>
        ///     The exclusive page count limit for direct lower-bound lookup.
        /// </summary>
        private const int CACHED_PAGE_COUNTS = 64;

        /// <summary>
        ///     The offset of the aligned thread slots in the control allocation.
        /// </summary>
        private static nuint ShardsOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Usize.FromI32Unchecked(Unsafe.SizeOf<State>().WrappingAdd(CACHE_LINE_SIZE - 1) & -CACHE_LINE_SIZE);
        }

        /// <summary>
        ///     The control block at the end of the initial native allocation.
        /// </summary>
        private readonly State* _state;

        /// <summary>
        ///     Wraps the initialized native control block.
        /// </summary>
        /// <param name="state">The initialized native control block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeRewindableAllocator(State* state) => _state = state;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        /// <remarks>
        ///     The default value returns <see langword="false" />. A created value continues to return
        ///     <see langword="true" /> after disposal, so callers must track ownership and lifetime separately.
        /// </remarks>
        public bool IsCreated => _state != null;

        /// <summary>
        ///     Creates an allocator that owns its native storage.
        /// </summary>
        /// <param name="initialCapacity">
        ///     The initial standard segment capacity in bytes, rounded up to a 64 KiB multiple with a
        ///     minimum of 256 KiB. Zero selects the minimum.
        /// </param>
        /// <returns>The allocator to dispose when its storage is no longer needed.</returns>
        /// <exception cref="OutOfMemoryException">
        ///     <paramref name="initialCapacity" /> exceeds 512 MiB,
        ///     or a native allocation fails.
        /// </exception>
        /// <remarks>
        ///     The control block and descriptor tables follow the initial standard segment in one native allocation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeRewindableAllocator Create(nuint initialCapacity = 0)
        {
            if (initialCapacity > PAGE_LIMIT.WrappingMul(PAGE_SIZE))
                ThrowHelpers.ThrowOutOfMemoryException();

            var capacity = MathHelpers.Max(NativeMemoryAllocator.AlignUp(initialCapacity, PAGE_SIZE), MIN_STANDARD_BYTES);
            var shardOffset = ShardsOffset;
            var shardBytes = Usize.FromU32Unchecked(TOTAL_SHARDS).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<Shard>()));
            var standardOffset = shardOffset.WrappingAdd(shardBytes);
            var entriesOffset = standardOffset.WrappingAdd(STANDARD_SEGMENT_SLOTS.WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<Segment>())));
            var lowerBoundsOffset = entriesOffset.WrappingAdd(DEDICATED_LIMIT.WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>())));
            var lowerBoundsBytes = Usize.FromI32Unchecked(CACHED_PAGE_COUNTS).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<nuint>()));
            var chunksOffset = lowerBoundsOffset.WrappingAdd(lowerBoundsBytes);
            var chunksBytes = Usize.FromI32Unchecked(CHUNK_ORDERS).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<nuint>()));
            var controlBytes = chunksOffset.WrappingAdd(chunksBytes);
            var start = (byte*)NativeMemoryAllocator.AlignedAlloc(capacity.WrappingAdd(controlBytes), PAGE_SIZE);
            var state = UnsafeHelpers.AddByteOffset<State>(start, Isize.FromUsizeUnchecked(capacity));
            var shards = UnsafeHelpers.AddByteOffset<Shard>(state, Isize.FromUsizeUnchecked(shardOffset));
            var standard = UnsafeHelpers.AddByteOffset<Segment>(state, Isize.FromUsizeUnchecked(standardOffset));
            var entries = UnsafeHelpers.AddByteOffset<DedicatedEntry>(state, Isize.FromUsizeUnchecked(entriesOffset));
            var lowerBounds = UnsafeHelpers.AddByteOffset<nuint>(state, Isize.FromUsizeUnchecked(lowerBoundsOffset));
            var chunks = UnsafeHelpers.AddByteOffset<nuint>(state, Isize.FromUsizeUnchecked(chunksOffset));
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(shards), U8.FromI32Unchecked(0), shardBytes);
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(lowerBounds), U8.FromI32Unchecked(0), lowerBoundsBytes);
            Unsafe.AsRef<Segment>(standard) = Segment.From(start, capacity);
            Unsafe.AsRef<State>(state) = State.From(shards, standard, entries, lowerBounds, chunks);
            return new NativeRewindableAllocator(state);
        }

        /// <summary>
        ///     Allocates aligned storage for unmanaged elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="elementCount">The number of elements. A zero count still reserves storage.</param>
        /// <returns>A non-null pointer aligned to <see cref="NativeMemoryAllocator.AlignOf{T}()" />.</returns>
        /// <exception cref="OverflowException">
        ///     Computing the element byte count, rounding the segment size, or computing the
        ///     native allocation size overflowed.
        /// </exception>
        /// <exception cref="OutOfMemoryException">The page or dedicated segment limit was reached, or native allocation failed.</exception>
        /// <remarks>
        ///     Concurrent calls are supported on a live allocator.
        ///     The byte count and natural alignment of the elements determine the allocation path.
        ///     The returned memory may contain data from an earlier allocation.
        ///     See <see cref="AlignedAlloc(nuint, nuint)" /> for allocation limits.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAlloc<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAlloc(byteCount, NativeMemoryAllocator.AlignOf<T>());
        }

        /// <summary>
        ///     Allocates and clears aligned storage for unmanaged elements.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type.</typeparam>
        /// <param name="elementCount">The number of elements. A zero count still reserves storage.</param>
        /// <returns>A non-null pointer aligned to <see cref="NativeMemoryAllocator.AlignOf{T}()" />.</returns>
        /// <exception cref="OverflowException">
        ///     Computing the element byte count, rounding the segment size, or computing the
        ///     native allocation size overflowed.
        /// </exception>
        /// <exception cref="OutOfMemoryException">The page or dedicated segment limit was reached, or native allocation failed.</exception>
        /// <remarks>
        ///     Concurrent calls are supported on a live allocator.
        ///     Only the requested element bytes are cleared. A zero-element request reserves space and performs no writes.
        ///     See <see cref="AlignedAlloc(nuint, nuint)" /> for allocation limits.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAllocZeroed<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAllocZeroed(byteCount, NativeMemoryAllocator.AlignOf<T>());
        }

        /// <summary>
        ///     Allocates aligned unmanaged memory.
        /// </summary>
        /// <param name="byteCount">The requested byte count. A zero count still reserves storage.</param>
        /// <param name="alignment">The required nonzero power-of-two alignment, in bytes.</param>
        /// <returns>A non-null pointer aligned to <paramref name="alignment" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="alignment" /> is zero or is not a power of two.</exception>
        /// <exception cref="OverflowException">
        ///     Rounding the dedicated segment size or computing the underlying native allocation
        ///     size overflowed.
        /// </exception>
        /// <exception cref="OutOfMemoryException">The page or dedicated segment limit was reached, or native allocation failed.</exception>
        /// <remarks>
        ///     Concurrent calls are supported on a live allocator.
        ///     The larger of <paramref name="byteCount" /> and <paramref name="alignment" /> selects the allocation path.
        ///     Up to 32 KiB uses aligned space within a 64 KiB page. Thread slots start with a
        ///     chunk sized for the request, from 4 KiB upward, and can grow to 64 KiB on refill.
        ///     Threads can use unassigned chunks in the same page. Repeated half-page requests
        ///     may reserve a second half for the next call on the same thread.
        ///     Above 32 KiB through 64 KiB uses a whole standard page. Larger requests use a dedicated segment.
        ///     Dedicated segment capacity is the requested byte count rounded to a page, with a minimum
        ///     of one page. The requested alignment determines which segments can be reused.
        ///     The returned memory may contain data from an earlier allocation.
        ///     The allocator can retain up to 255 dedicated segments and claim up to 8,192 standard pages
        ///     minus the retained dedicated segment count.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAlloc(nuint byteCount, nuint alignment)
        {
            var mask = alignment.WrappingSub(Usize.FromI32Unchecked(1));
            if (byteCount > HALF_PAGE_SIZE || mask > HALF_PAGE_MASK)
            {
                ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
                return byteCount > PAGE_SIZE || alignment > PAGE_SIZE ? AllocateDedicated(byteCount, alignment) : AllocatePage();
            }

            if ((alignment & mask) != Usize.FromI32Unchecked(0))
                ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);

            var threadId = U32.FromI32Unchecked(Environment.CurrentManagedThreadId);
            var state = _state;
            var slot = Usize.FromU32Unchecked(threadId & SHARD_MASK).WrappingMul(Usize.FromI32Unchecked(CACHE_LINE_SIZE));
            var shard = UnsafeHelpers.AddByteOffset<Shard>(state, Isize.FromUsizeUnchecked(slot.WrappingAdd(ShardsOffset)));
            if (byteCount == Usize.FromI32Unchecked(0))
                byteCount = Usize.FromI32Unchecked(1);

            if (shard->Owner.Load(Ordering.Acquire) == threadId)
            {
                if ((byteCount | mask) >= HALF_PAGE_MASK && (byteCount == HALF_PAGE_SIZE || mask == HALF_PAGE_MASK))
                {
                    var half = shard->SpareHalf;
                    if (half != Usize.FromI32Unchecked(0))
                    {
                        shard->SpareHalf = Usize.FromI32Unchecked(0);
                        return (void*)half;
                    }

                    return RefillShard(state, shard, threadId, byteCount, mask);
                }

                var next = shard->Cursor.WrappingSub(byteCount) & ~mask;
                if (next >= shard->Floor)
                {
                    shard->Cursor = next;
                    return (void*)next;
                }
            }

            return RefillShard(state, shard, threadId, byteCount, mask);
        }

        /// <summary>
        ///     Allocates and clears aligned unmanaged memory.
        /// </summary>
        /// <param name="byteCount">The requested byte count. A zero count still reserves storage.</param>
        /// <param name="alignment">The required nonzero power-of-two alignment, in bytes.</param>
        /// <returns>A non-null pointer aligned to <paramref name="alignment" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="alignment" /> is zero or is not a power of two.</exception>
        /// <exception cref="OverflowException">
        ///     Rounding the dedicated segment size or computing the underlying native allocation
        ///     size overflowed.
        /// </exception>
        /// <exception cref="OutOfMemoryException">The page or dedicated segment limit was reached, or native allocation failed.</exception>
        /// <remarks>
        ///     Concurrent calls are supported on a live allocator.
        ///     Only the requested bytes are cleared. A zero-byte request reserves space and performs no writes.
        ///     See <see cref="AlignedAlloc(nuint, nuint)" /> for allocation limits.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAllocZeroed(nuint byteCount, nuint alignment)
        {
            var result = AlignedAlloc(byteCount, alignment);
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(result), U8.FromI32Unchecked(0), byteCount);
            return result;
        }

        /// <summary>
        ///     Invalidates every outstanding allocation and retains the backing storage for reuse.
        /// </summary>
        /// <remarks>
        ///     Finish all allocation calls and all access through allocated pointers or views before rewinding.
        ///     Discard those pointers and views. Later allocations may reuse the same addresses.
        ///     Standard pages are available to every thread again. Retained dedicated segments are available
        ///     to requests that fit their capacity and alignment. Rewind leaves the stored bytes intact.
        /// </remarks>
        public void Rewind()
        {
            var state = _state;
            if (state->ClaimedPages != Usize.FromI32Unchecked(0))
            {
                state->ClaimedPages = Usize.FromI32Unchecked(0);
                state->ActiveSegment = Usize.FromI32Unchecked(0);
                state->NextPage = state->Standard->Start;
                state->SegmentEnd = UnsafeHelpers.AddByteOffset<byte>(state->NextPage, Isize.FromUsizeUnchecked(state->Standard->Capacity));
                state->SharedCursor = EMPTY_CURSOR;
                state->SharedFloor = EMPTY_FLOOR;
                state->ChunkMask = 0U;
                var occupied = state->OccupiedPrimaryShards;
                var shards = state->Shards;
                while (occupied != 0UL)
                {
                    var index = BitOperationsHelpers.TrailingZeroCount(occupied);
                    UnsafeHelpers.Add<Shard>(shards, Isize.FromI32Unchecked(index))->Owner.Store(0U, Ordering.Relaxed);
                    occupied &= occupied.WrappingSub(1UL);
                }

                state->OccupiedPrimaryShards = 0UL;
                if (state->OccupiedSecondaryShards != 0UL)
                    ClearSecondaryShards(state);
            }

            DedicatedPool.Rewind(&state->Dedicated);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        ///     Finish all allocation calls and all access through allocated pointers or views before disposal.
        ///     Disposing through one copy invalidates all copies. Dispose once for each successful creation.
        ///     Call this method on a value returned by <see cref="Create" />.
        /// </remarks>
        public void Dispose()
        {
            var state = _state;
            if (state == null)
                return;

            for (var index = Usize.FromI32Unchecked(1); index < state->StandardCount; index = index.WrappingAdd(Usize.FromI32Unchecked(1)))
                NativeMemoryAllocator.AlignedFree(UnsafeHelpers.Add<Segment>(state->Standard, Isize.FromUsizeUnchecked(index))->Start);

            var pool = &state->Dedicated;
            for (var index = Usize.FromI32Unchecked(0); index < pool->RetainedCount; index = index.WrappingAdd(Usize.FromI32Unchecked(1)))
                NativeMemoryAllocator.AlignedFree(UnsafeHelpers.Add<DedicatedEntry>(pool->Entries, Isize.FromUsizeUnchecked(index))->Start);

            NativeMemoryAllocator.AlignedFree(state->Standard->Start);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeRewindableAllocator other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeRewindableAllocator other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeRewindableAllocator";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeRewindableAllocator left, NativeRewindableAllocator right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeRewindableAllocator left, NativeRewindableAllocator right) => !left.Equals(right);

        /// <summary>
        ///     Clears the occupied secondary slots at rewind.
        /// </summary>
        /// <param name="state">The allocator state under exclusive use.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ClearSecondaryShards(State* state)
        {
            var occupied = state->OccupiedSecondaryShards;
            var shards = UnsafeHelpers.Add<Shard>(state->Shards, Isize.FromU32Unchecked(SHARD_COUNT));
            while (occupied != 0UL)
            {
                var index = BitOperationsHelpers.TrailingZeroCount(occupied);
                UnsafeHelpers.Add<Shard>(shards, Isize.FromI32Unchecked(index))->Owner.Store(0U, Ordering.Relaxed);
                occupied &= occupied.WrappingSub(1UL);
            }

            state->OccupiedSecondaryShards = 0UL;
        }

        /// <summary>
        ///     Tries the secondary slot after a primary collision, or refills from standard storage.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        /// <param name="shard">The primary slot selected by the current thread.</param>
        /// <param name="threadId">The current thread's managed ID.</param>
        /// <param name="byteCount">The positive byte count.</param>
        /// <param name="mask">The requested alignment minus one.</param>
        /// <returns>The aligned allocation.</returns>
        /// <remarks>
        ///     The idea of assigning pages to a thread-local allocation path comes from
        ///     <see href="https://github.com/microsoft/mimalloc/blob/v3.6.1/src/alloc/alloc.c">mimalloc v3.6.1</see>.
        ///     An owner remains exclusive until rewind. On a primary collision the thread tries
        ///     the corresponding secondary slot before taking the refill lock. The lock also
        ///     protects the ownership check when another thread claims an empty primary slot.
        ///     Threads that own neither slot share a chunk under the refill lock.
        ///     A failed claim leaves slot ownership unchanged.
        ///     The smallest fitting order comes from the last requested byte with the alignment bits set.
        ///     Shifting out the minimum chunk size and adding a low set bit maps that interval to
        ///     a base-two logarithm, including the zero order.
        ///     The release store publishes the initialized slot. Half-page requests can reserve a pair
        ///     so the next half-page request from the owner can skip the refill lock.
        ///     At the page limit, allocations can use remaining space in the shared chunk, and
        ///     half-page requests can also use an aligned range in the owner's current chunk.
        ///     When only half-page storage has been allocated, the cursor and floor form an empty
        ///     interval. A full 4 KiB request consumes the entire smallest grant, so the next
        ///     refill can grow directly to a page.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte* RefillShard(State* state, Shard* shard, uint threadId, nuint byteCount, nuint mask)
        {
            var index = threadId & SHARD_MASK;
            var currentOwner = shard->Owner.Load(Ordering.Relaxed);
            var isSecondary = false;
            if (currentOwner != 0U && currentOwner != threadId)
            {
                isSecondary = true;
                shard = UnsafeHelpers.Add<Shard>(shard, Isize.FromU32Unchecked(SHARD_COUNT));
                if (shard->Owner.Load(Ordering.Acquire) == threadId)
                {
                    if (byteCount == HALF_PAGE_SIZE || mask == HALF_PAGE_MASK)
                    {
                        var half = shard->SpareHalf;
                        if (half != Usize.FromI32Unchecked(0))
                        {
                            shard->SpareHalf = Usize.FromI32Unchecked(0);
                            return (byte*)half;
                        }
                    }
                    else
                    {
                        var next = shard->Cursor.WrappingSub(byteCount) & ~mask;
                        if (next >= shard->Floor)
                        {
                            shard->Cursor = next;
                            return (byte*)next;
                        }
                    }
                }
            }

            EnterRefillLock(state);
            try
            {
                var owner = shard->Owner.Load(Ordering.Relaxed);
                if (!isSecondary && owner != 0U && owner != threadId)
                {
                    isSecondary = true;
                    shard = UnsafeHelpers.Add<Shard>(shard, Isize.FromU32Unchecked(SHARD_COUNT));
                    owner = shard->Owner.Load(Ordering.Relaxed);
                }

                if (byteCount == HALF_PAGE_SIZE || mask == HALF_PAGE_MASK)
                {
                    if ((state->ChunkMask & HALF_CHUNK_BIT) == 0U)
                    {
                        var claimed = state->ClaimedPages;
                        var limit = state->StandardPageLimit;
                        if (claimed == limit)
                            return TakeHalfAtPageLimit(state, shard, owner == threadId, byteCount, mask);

                        if (owner == threadId)
                        {
                            var page = AdvanceStandardPage(state, claimed, limit);
                            shard->SpareHalf = Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(page)).WrappingAdd(HALF_PAGE_SIZE);
                            return page;
                        }
                    }

                    var half = TakeChunk(state, HALF_CHUNK_ORDER, HALF_CHUNK_ORDER, out _);
                    if (owner == 0U)
                    {
                        shard->Floor = EMPTY_FLOOR;
                        shard->Cursor = EMPTY_CURSOR;
                        shard->NextOrder = 0;
                        shard->SpareHalf = Usize.FromI32Unchecked(0);
                        var bit = 1UL << I32.FromU32Unchecked(index);
                        if (!isSecondary)
                            state->OccupiedPrimaryShards |= bit;
                        else
                            state->OccupiedSecondaryShards |= bit;

                        shard->Owner.Store(threadId, Ordering.Release);
                    }

                    return (byte*)half;
                }

                if (owner == 0U || owner == threadId)
                {
                    nuint start;
                    int order;
                    if (owner == threadId && shard->NextOrder == CHUNK_ORDERS && state->ClaimedPages < state->StandardPageLimit)
                    {
                        start = Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(TakeStandardPage(state)));
                        order = CHUNK_ORDERS;
                    }
                    else
                    {
                        var lastByte = U32.FromUsizeUnchecked(byteCount.WrappingSub(Usize.FromI32Unchecked(1)) | mask);
                        var minimumOrder = BitOperationsHelpers.Log2(((lastByte >> CHUNK_SHIFT) << 1) | 1U);
                        var preferredOrder = owner == 0U ? minimumOrder : Math.Max(minimumOrder, shard->NextOrder);
                        var atPageLimit = state->ClaimedPages == state->StandardPageLimit;
                        if (atPageLimit && owner == threadId && shard->SpareHalf != Usize.FromI32Unchecked(0))
                        {
                            start = shard->SpareHalf;
                            shard->SpareHalf = Usize.FromI32Unchecked(0);
                            order = HALF_CHUNK_ORDER;
                        }
                        else
                        {
                            if (atPageLimit && (state->ChunkMask & (uint.MaxValue << minimumOrder)) == 0U)
                            {
                                var sharedNext = state->SharedCursor.WrappingSub(byteCount) & ~mask;
                                if (sharedNext >= state->SharedFloor)
                                {
                                    state->SharedCursor = sharedNext;
                                    return (byte*)sharedNext;
                                }
                            }

                            start = TakeChunk(state, preferredOrder, minimumOrder, out order);
                        }
                    }

                    shard->Floor = start;
                    shard->NextOrder = order == 0 && byteCount == MIN_CHUNK_SIZE ? CHUNK_ORDERS : Math.Min(order.WrappingAdd(CHUNK_GROWTH_ORDERS), CHUNK_ORDERS);
                    shard->Cursor = start.WrappingAdd(MIN_CHUNK_SIZE << order).WrappingSub(byteCount) & ~mask;
                    if (owner == 0U)
                    {
                        shard->SpareHalf = Usize.FromI32Unchecked(0);
                        var bit = 1UL << I32.FromU32Unchecked(index);
                        if (!isSecondary)
                            state->OccupiedPrimaryShards |= bit;
                        else
                            state->OccupiedSecondaryShards |= bit;

                        shard->Owner.Store(threadId, Ordering.Release);
                    }

                    return (byte*)shard->Cursor;
                }

                var next = state->SharedCursor.WrappingSub(byteCount) & ~mask;
                if (next >= state->SharedFloor)
                {
                    state->SharedCursor = next;
                    return (byte*)next;
                }

                var sharedLastByte = U32.FromUsizeUnchecked(byteCount.WrappingSub(Usize.FromI32Unchecked(1)) | mask);
                var sharedMinimum = BitOperationsHelpers.Log2(((sharedLastByte >> CHUNK_SHIFT) << 1) | 1U);
                var shared = TakeChunk(state, CHUNK_ORDERS, sharedMinimum, out var sharedOrder);
                state->SharedFloor = shared;
                state->SharedCursor = shared.WrappingAdd(MIN_CHUNK_SIZE << sharedOrder).WrappingSub(byteCount) & ~mask;
                return (byte*)state->SharedCursor;
            }
            finally
            {
                ExitRefillLock(state);
            }
        }

        /// <summary>
        ///     Takes a half-page request from the current thread's chunk or the shared chunk under the refill lock.
        /// </summary>
        /// <param name="state">The allocator state at its page limit with no unassigned half page.</param>
        /// <param name="shard">The slot examined by the calling thread.</param>
        /// <param name="ownsShard">Whether the calling thread owns the slot.</param>
        /// <param name="byteCount">The positive byte count.</param>
        /// <param name="mask">The requested alignment minus one.</param>
        /// <returns>The aligned allocation.</returns>
        /// <exception cref="OutOfMemoryException">Neither chunk can fit the request.</exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte* TakeHalfAtPageLimit(State* state, Shard* shard, bool ownsShard, nuint byteCount, nuint mask)
        {
            if (ownsShard)
            {
                var next = shard->Cursor.WrappingSub(byteCount) & ~mask;
                if (next >= shard->Floor)
                {
                    shard->Cursor = next;
                    return (byte*)next;
                }
            }

            var sharedNext = state->SharedCursor.WrappingSub(byteCount) & ~mask;
            if (sharedNext < state->SharedFloor)
                ThrowHelpers.ThrowOutOfMemoryException();

            state->SharedCursor = sharedNext;
            return (byte*)sharedNext;
        }

        /// <summary>
        ///     Claims an aligned power-of-two chunk under the refill lock.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        /// <param name="preferredOrder">The preferred chunk size order.</param>
        /// <param name="minimumOrder">The smallest size order that fits the request.</param>
        /// <param name="order">The order of the chunk returned.</param>
        /// <returns>The lower address of the chunk.</returns>
        /// <remarks>
        ///     At each size there is at most one unassigned chunk. Always split the smallest
        ///     available chunk that fits, so the lower levels filled by the split are empty.
        ///     When the page budget is exhausted, a thread can claim a smaller leftover
        ///     chunk that fits the current request.
        ///     Growing the amount granted to active threads follows the adaptive cache sizing
        ///     principle in <see href="https://github.com/google/tcmalloc/blob/master/docs/design.md">TCMalloc</see>.
        /// </remarks>
        private static nuint TakeChunk(State* state, int preferredOrder, int minimumOrder, out int order)
        {
            order = preferredOrder;
            var mask = state->ChunkMask;
            var available = mask & (uint.MaxValue << order);
            if (available == 0U && minimumOrder < order && state->ClaimedPages == state->StandardPageLimit)
            {
                available = mask & (uint.MaxValue << minimumOrder);
                if (available != 0U)
                    order = BitOperationsHelpers.TrailingZeroCount(available);
            }

            nuint start;
            int found;
            if (available == 0U)
            {
                start = Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(TakeStandardPage(state)));
                found = CHUNK_ORDERS;
            }
            else
            {
                found = BitOperationsHelpers.TrailingZeroCount(available);
                start = Unsafe.Add(ref Unsafe.AsRef<nuint>(state->Chunks), Isize.FromI32Unchecked(found));
                mask &= ~(1U << found);
            }

            for (var level = found; level > order;)
            {
                level = level.WrappingSub(1);
                Unsafe.Add(ref Unsafe.AsRef<nuint>(state->Chunks), Isize.FromI32Unchecked(level)) = start.WrappingAdd(MIN_CHUNK_SIZE << level);
                mask |= 1U << level;
            }

            state->ChunkMask = mask;
            return start;
        }

        /// <summary>
        ///     Takes a whole standard page under the refill lock.
        /// </summary>
        /// <returns>The page-aligned address.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private byte* AllocatePage()
        {
            var state = _state;
            EnterRefillLock(state);
            var page = state->NextPage;
            var claimed = state->ClaimedPages;
            if (page == state->SegmentEnd || claimed == state->StandardPageLimit)
                return AllocatePageSlow(state);

            state->NextPage = UnsafeHelpers.AddByteOffset<byte>(page, Isize.FromUsizeUnchecked(PAGE_SIZE));
            state->ClaimedPages = claimed.WrappingAdd(Usize.FromI32Unchecked(1));
            ExitRefillLock(state);
            return page;
        }

        /// <summary>
        ///     Completes a whole-page request at a segment boundary or the page limit.
        /// </summary>
        /// <param name="state">The allocator state with its refill lock held.</param>
        /// <returns>The page-aligned address.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte* AllocatePageSlow(State* state)
        {
            try
            {
                return TakeStandardPage(state);
            }
            finally
            {
                ExitRefillLock(state);
            }
        }

        /// <summary>
        ///     Claims the next page in a standard segment under the refill lock.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        /// <returns>The page-aligned address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* TakeStandardPage(State* state)
        {
            var claimed = state->ClaimedPages;
            var limit = state->StandardPageLimit;
            if (claimed >= limit)
                ThrowHelpers.ThrowOutOfMemoryException();

            return AdvanceStandardPage(state, claimed, limit);
        }

        /// <summary>
        ///     Advances the standard cursor under the refill lock after confirming that the page budget has room.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        /// <param name="claimed">The number of pages already claimed.</param>
        /// <param name="limit">The current standard page budget.</param>
        /// <returns>The newly claimed page.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* AdvanceStandardPage(State* state, nuint claimed, nuint limit)
        {
            var next = state->NextPage;
            if (next == state->SegmentEnd)
                next = SwitchStandardSegment(state, limit.WrappingSub(claimed));

            state->NextPage = UnsafeHelpers.AddByteOffset<byte>(next, Isize.FromUsizeUnchecked(PAGE_SIZE));
            state->ClaimedPages = claimed.WrappingAdd(Usize.FromI32Unchecked(1));
            return next;
        }

        /// <summary>
        ///     Advances to the next retained standard segment or grows the pool under the refill lock.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        /// <param name="remainingPages">The remaining page budget.</param>
        /// <returns>The start of the new active segment.</returns>
        /// <remarks>
        ///     Increasing capacity by half, rounded down to whole pages, fits the 8,192-page budget in at most
        ///     20 descriptors from the four-page minimum, including the final partial segment.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte* SwitchStandardSegment(State* state, nuint remainingPages)
        {
            var index = state->ActiveSegment.WrappingAdd(Usize.FromI32Unchecked(1));
            var next = UnsafeHelpers.Add<Segment>(state->Standard, Isize.FromUsizeUnchecked(index));
            if (index == state->StandardCount)
            {
                var previous = UnsafeHelpers.Add<Segment>(state->Standard, Isize.FromUsizeUnchecked(state->ActiveSegment));
                var half = (previous->Capacity >> 1) & ~PAGE_MASK;
                var growth = MathHelpers.Min(previous->Capacity.WrappingAdd(half), MAX_STANDARD_GROWTH);
                var capacity = MathHelpers.Min(growth, remainingPages << PAGE_SHIFT);
                var start = (byte*)NativeMemoryAllocator.AlignedAlloc(capacity, PAGE_SIZE);
                Unsafe.AsRef<Segment>(next) = Segment.From(start, capacity);
                state->StandardCount = index.WrappingAdd(Usize.FromI32Unchecked(1));
            }

            state->ActiveSegment = index;
            state->SegmentEnd = UnsafeHelpers.AddByteOffset<byte>(next->Start, Isize.FromUsizeUnchecked(next->Capacity));
            return next->Start;
        }

        /// <summary>
        ///     Allocates or reuses a dedicated segment.
        /// </summary>
        /// <param name="byteCount">The requested byte count.</param>
        /// <param name="alignment">The requested power-of-two alignment.</param>
        /// <returns>A pointer to the start of the segment.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the page-aligned capacity of <paramref name="byteCount" /> overflows a
        ///     native-sized integer.
        /// </exception>
        /// <exception cref="OutOfMemoryException">
        ///     Thrown when the page or dedicated segment limit is reached, or when the native allocation fails.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private byte* AllocateDedicated(nuint byteCount, nuint alignment)
        {
            var capacity = byteCount <= PAGE_SIZE ? PAGE_SIZE : NativeMemoryAllocator.AlignUp(byteCount, PAGE_SIZE);
            var nativeAlignment = MathHelpers.Max(alignment, PAGE_SIZE);
            var row = Usize.FromI32Unchecked(BitOperationsHelpers.TrailingZeroCount(nativeAlignment).WrappingSub(PAGE_SHIFT));
            var state = _state;
            EnterRefillLock(state);
            try
            {
                var pool = &state->Dedicated;
                var first = DedicatedPool.FindLowerBound(pool, capacity);
                var reused = DedicatedPool.TryTake(pool, first, row);
                if (reused != null)
                    return reused;

                if (pool->RetainedCount == DEDICATED_LIMIT || state->ClaimedPages >= state->StandardPageLimit)
                    ThrowHelpers.ThrowOutOfMemoryException();

                var start = (byte*)NativeMemoryAllocator.AlignedAlloc(capacity, nativeAlignment);
                DedicatedPool.Register(pool, first, row, start, capacity);
                state->StandardPageLimit = state->StandardPageLimit.WrappingSub(Usize.FromI32Unchecked(1));
                return start;
            }
            finally
            {
                ExitRefillLock(state);
            }
        }

        /// <summary>
        ///     Acquires the lock protecting standard segment growth and dedicated reuse.
        /// </summary>
        /// <param name="state">The allocator state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnterRefillLock(State* state)
        {
            if (state->RefillLock.CompareExchange(1U, 0U) != 0U)
                WaitForRefillLock(ref state->RefillLock);
        }

        /// <summary>
        ///     Releases the refill lock and publishes updates made while holding it.
        /// </summary>
        /// <param name="state">The allocator state with its refill lock held.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExitRefillLock(State* state) => state->RefillLock.Store(0U, Ordering.Release);

        /// <summary>
        ///     Waits for the refill lock and acquires it when it becomes available.
        /// </summary>
        /// <param name="gate">The held lock.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WaitForRefillLock(ref UnsafeAtomicU32 gate)
        {
            Backoff wait = default;
            while (true)
            {
                wait.Snooze();

                if (gate.Load(Ordering.Relaxed) == 0U && gate.CompareExchange(1U, 0U) == 0U)
                    break;
            }
        }

        /// <summary>
        ///     Holds the active chunk of a single managed thread in its own cache-line region.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        private struct Shard
        {
            /// <summary>
            ///     The owning managed thread ID, or zero until a thread reserves a chunk.
            /// </summary>
            public UnsafeAtomicU32 Owner;

            /// <summary>
            ///     The exclusive upper address of the unused prefix of the current chunk.
            /// </summary>
            public nuint Cursor;

            /// <summary>
            ///     The lower address of the current chunk.
            /// </summary>
            public nuint Floor;

            /// <summary>
            ///     The preferred size order for the next refill of the current chunk.
            /// </summary>
            public int NextOrder;

            /// <summary>
            ///     The second half of a page reserved for half-page requests, or zero.
            /// </summary>
            public nuint SpareHalf;
        }

        /// <summary>
        ///     Describes a retained standard segment.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Segment
        {
            /// <summary>
            ///     The aligned start of the segment.
            /// </summary>
            public readonly byte* Start;

            /// <summary>
            ///     The segment capacity in bytes.
            /// </summary>
            public readonly nuint Capacity;

            /// <summary>
            ///     Initializes a descriptor for existing storage.
            /// </summary>
            /// <param name="start">The segment address.</param>
            /// <param name="capacity">The segment capacity.</param>
            private Segment(byte* start, nuint capacity)
            {
                Start = start;
                Capacity = capacity;
            }

            /// <summary>
            ///     Describes an existing standard segment.
            /// </summary>
            /// <param name="start">The segment address.</param>
            /// <param name="capacity">The segment capacity.</param>
            /// <returns>The descriptor for the segment.</returns>
            public static Segment From(byte* start, nuint capacity) => new(start, capacity);
        }

        /// <summary>
        ///     Associates a dedicated segment with its ordered lookup key.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct DedicatedEntry
        {
            /// <summary>
            ///     The page-aligned capacity with the alignment row in the low bits.
            /// </summary>
            /// <remarks>The capacity has <see cref="PAGE_SHIFT" /> zero low bits, which leave room for every alignment row.</remarks>
            public readonly nuint Key;

            /// <summary>
            ///     The address returned when the segment is claimed from the index.
            /// </summary>
            public readonly byte* Start;

            /// <summary>
            ///     Gets the segment capacity in pages.
            /// </summary>
            public nuint Pages
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Key >> PAGE_SHIFT;
            }

            /// <summary>
            ///     Gets the row for the segment's guaranteed alignment.
            /// </summary>
            public nuint AlignmentRow
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Key & PAGE_MASK;
            }

            /// <summary>
            ///     Initializes the ordered key and address for an existing segment.
            /// </summary>
            /// <param name="start">The segment address.</param>
            /// <param name="capacity">The segment capacity rounded to a page, in bytes.</param>
            /// <param name="row">The segment's guaranteed alignment row.</param>
            private DedicatedEntry(byte* start, nuint capacity, nuint row)
            {
                Key = capacity | row;
                Start = start;
            }

            /// <summary>
            ///     Describes an existing dedicated segment.
            /// </summary>
            /// <param name="start">The segment address.</param>
            /// <param name="capacity">The segment capacity rounded to a page, in bytes.</param>
            /// <param name="row">The segment's guaranteed alignment row.</param>
            /// <returns>An entry ordered by capacity and alignment.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static DedicatedEntry From(byte* start, nuint capacity, nuint row) => new(start, capacity, row);
        }

        /// <summary>
        ///     Stores one bit per possible retained segment in index order.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Bitmap
        {
            /// <summary>
            ///     The words of the bitmap, with the lowest index in the least significant bit.
            /// </summary>
            public fixed ulong Words[BITMAP_STORAGE_WORDS];
        }

        /// <summary>
        ///     Tracks an ordered prefix of reusable segments followed by segments first allocated in the current cycle.
        /// </summary>
        /// <remarks>The refill lock protects this state during allocations. Rewind runs after callers finish.</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct DedicatedPool
        {
            /// <summary>
            ///     The largest suffix shifted entry by entry before using a bulk move.
            /// </summary>
            private const nuint INLINE_MOVE_LIMIT = 8;

            /// <summary>
            ///     The smallest pending suffix sorted with the merge path.
            /// </summary>
            private const int MERGE_SORT_THRESHOLD = 24;

            /// <summary>
            ///     The backing array of sorted entries.
            /// </summary>
            public readonly DedicatedEntry* Entries;

            /// <summary>
            ///     The first index for each cached request size expressed in pages.
            /// </summary>
            public readonly nuint* LowerBounds;

            /// <summary>
            ///     The number of ordered entries that are visible to the lookup in this cycle.
            /// </summary>
            public nuint Count;

            /// <summary>
            ///     The total number of retained entries, including newly allocated segments.
            /// </summary>
            public nuint RetainedCount;

            /// <summary>
            ///     The highest alignment row used by any retained segment.
            /// </summary>
            public nuint HighestRow;

            /// <summary>
            ///     Marks retained entries that can still be reused in the current cycle.
            /// </summary>
            public Bitmap Available;

            /// <summary>
            ///     Row n marks entries aligned to at least 2^(16+n) bytes. Bits beyond the initialized entries are zero.
            /// </summary>
            public fixed ulong AlignmentMasks[ALIGNMENT_ROWS * BITMAP_STORAGE_WORDS];

            /// <summary>
            ///     Initializes an empty pool over preallocated index storage.
            /// </summary>
            /// <param name="entries">The native storage for the sorted index.</param>
            /// <param name="lowerBounds">The zeroed table of cached capacity lower bounds.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private DedicatedPool(DedicatedEntry* entries, nuint* lowerBounds) : this()
            {
                Entries = entries;
                LowerBounds = lowerBounds;
            }

            /// <summary>
            ///     Uses the given entry storage and zeroed lower-bound table.
            /// </summary>
            /// <param name="entries">The native storage for the sorted index.</param>
            /// <param name="lowerBounds">The zeroed table of cached capacity lower bounds.</param>
            /// <returns>An empty dedicated pool.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static DedicatedPool From(DedicatedEntry* entries, nuint* lowerBounds) => new(entries, lowerBounds);

            /// <summary>
            ///     Returns the first entry whose capacity is at least the requested size, or the entry count.
            /// </summary>
            /// <param name="pool">The dedicated pool protected by the refill lock.</param>
            /// <param name="capacity">The requested capacity rounded to a page, in bytes.</param>
            /// <returns>The lower-bound index, or the number of initialized entries.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static nuint FindLowerBound(DedicatedPool* pool, nuint capacity)
            {
                var pages = capacity >> PAGE_SHIFT;
                if (pages < Usize.FromI32Unchecked(CACHED_PAGE_COUNTS))
                    return Unsafe.Add(ref Unsafe.AsRef<nuint>(pool->LowerBounds), Isize.FromUsizeUnchecked(pages));

                var entries = pool->Entries;
                var end = pool->Count;
                if (end == Usize.FromI32Unchecked(0) || entries->Key >= capacity)
                    return Usize.FromI32Unchecked(0);

                return SearchLowerBound(entries, end, capacity);
            }

            /// <summary>
            ///     Binary searches the ordered entries after the first entry has been excluded.
            /// </summary>
            /// <param name="entries">The sorted table of dedicated entries.</param>
            /// <param name="end">The exclusive upper bound of the initialized entries.</param>
            /// <param name="minimum">The ordering key for the requested capacity with the minimum alignment row.</param>
            /// <returns>The first index with sufficient capacity, or the original upper bound.</returns>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static nuint SearchLowerBound(DedicatedEntry* entries, nuint end, nuint minimum)
            {
                var first = Usize.FromI32Unchecked(1);
                while (first < end)
                {
                    var middle = first.WrappingAdd(end) >> 1;
                    if (UnsafeHelpers.Add<DedicatedEntry>(entries, Isize.FromUsizeUnchecked(middle))->Key < minimum)
                        first = middle.WrappingAdd(Usize.FromI32Unchecked(1));
                    else
                        end = middle;
                }

                return first;
            }

            /// <summary>
            ///     Claims the first eligible free entry at or after the capacity lower bound. The caller holds the refill lock.
            /// </summary>
            /// <param name="pool">The dedicated pool protected by the refill lock.</param>
            /// <param name="first">The lower-bound index for the requested capacity.</param>
            /// <param name="row">The minimum alignment row for the request.</param>
            /// <returns>The selected segment address, or null if no available segment fits.</returns>
            /// <remarks>
            ///     Eligibility masks set bits only for initialized entries. The scan stops before the first word beyond the entry
            ///     count.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static byte* TryTake(DedicatedPool* pool, nuint first, nuint row)
            {
                if (first >= pool->Count)
                    return null;

                ref var free = ref Unsafe.AsRef<nuint>(pool->Available.Words);
                var rowOffset = row.WrappingMul(BitmapWordCount);
                ref var eligible = ref Unsafe.Add(ref Unsafe.AsRef<nuint>(pool->AlignmentMasks), Isize.FromUsizeUnchecked(rowOffset));
                var word = first >> BitmapWordShift;
                var offset = Isize.FromUsizeUnchecked(word);
                var bits = Unsafe.Add(ref eligible, offset) & Unsafe.Add(ref free, offset) & (~Usize.FromI32Unchecked(0) << (I32.FromUsizeUnchecked(first) & BitmapWordMask));
                while (bits == Usize.FromI32Unchecked(0))
                {
                    word = word.WrappingAdd(Usize.FromI32Unchecked(1));
                    if (word << BitmapWordShift >= pool->Count)
                        return null;

                    offset = Isize.FromUsizeUnchecked(word);
                    bits = Unsafe.Add(ref eligible, offset) & Unsafe.Add(ref free, offset);
                }

                var bit = BitOperationsHelpers.TrailingZeroCount(bits);
                Unsafe.Add(ref free, offset) &= ~(Usize.FromI32Unchecked(1) << bit);
                var index = (word << BitmapWordShift) | Usize.FromI32Unchecked(bit);
                return UnsafeHelpers.Add<DedicatedEntry>(pool->Entries, Isize.FromUsizeUnchecked(index))->Start;
            }

            /// <summary>
            ///     Registers a segment already claimed for the current cycle.
            /// </summary>
            /// <param name="pool">The dedicated pool protected by the refill lock.</param>
            /// <param name="first">The cached lower bound for the segment's capacity.</param>
            /// <param name="row">The segment's guaranteed alignment row.</param>
            /// <param name="start">The new segment address.</param>
            /// <param name="capacity">The new segment capacity rounded to a page, in bytes.</param>
            /// <remarks>
            ///     An empty index or an out-of-order key defers indexing until rewind. A strictly larger
            ///     capacity can extend an existing sorted index in place when no entries are pending.
            ///     Newly registered segments remain unavailable until rewind.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Register(DedicatedPool* pool, nuint first, nuint row, byte* start, nuint capacity)
            {
                var index = pool->RetainedCount;
                Unsafe.Add(ref Unsafe.AsRef<DedicatedEntry>(pool->Entries), Isize.FromUsizeUnchecked(index)) = DedicatedEntry.From(start, capacity, row);
                pool->RetainedCount = index.WrappingAdd(Usize.FromI32Unchecked(1));
                pool->HighestRow = MathHelpers.Max(pool->HighestRow, row);

                if (index != pool->Count || index == Usize.FromI32Unchecked(0) || first != index)
                    return;

                ref var masks = ref Unsafe.AsRef<nuint>(pool->AlignmentMasks);
                var word = index >> BitmapWordShift;
                var bit = Usize.FromI32Unchecked(1) << (I32.FromUsizeUnchecked(index) & BitmapWordMask);
                var rowWidth = BitmapWordCount;
                var end = word.WrappingAdd(row.WrappingMul(rowWidth));
                for (var offset = word; offset <= end; offset = offset.WrappingAdd(rowWidth))
                    Unsafe.Add(ref masks, Isize.FromUsizeUnchecked(offset)) |= bit;

                ref var bounds = ref Unsafe.AsRef<nuint>(pool->LowerBounds);
                for (var affected = (capacity >> PAGE_SHIFT).WrappingAdd(Usize.FromI32Unchecked(1)); affected < Usize.FromI32Unchecked(CACHED_PAGE_COUNTS); affected = affected.WrappingAdd(Usize.FromI32Unchecked(1)))
                    Unsafe.Add(ref bounds, Isize.FromUsizeUnchecked(affected)) = pool->RetainedCount;

                pool->Count = pool->RetainedCount;
            }

            /// <summary>
            ///     Makes all retained segments available and indexes the segments added since the previous rewind.
            /// </summary>
            /// <param name="pool">The dedicated pool after all allocation callers have finished.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Rewind(DedicatedPool* pool)
            {
                if (pool->RetainedCount != pool->Count)
                    IndexPending(pool);

                if (pool->Count != Usize.FromI32Unchecked(0))
                    pool->Available = Unsafe.AsRef<Bitmap>(pool->AlignmentMasks);
            }

            /// <summary>
            ///     Orders the pending suffix and rebuilds the capacity and alignment indexes.
            /// </summary>
            /// <param name="pool">The dedicated pool after all allocation callers have finished.</param>
            /// <remarks>
            ///     Small suffixes use binary insertion and larger batches use merge sort.
            ///     Both paths preserve newest-first order for equal keys.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void IndexPending(DedicatedPool* pool)
            {
                var total = pool->RetainedCount;
                if (total.WrappingSub(pool->Count) >= Usize.FromI32Unchecked(MERGE_SORT_THRESHOLD))
                    MergePending(pool->Entries, I32.FromUsizeUnchecked(pool->Count), I32.FromUsizeUnchecked(total));
                else
                    InsertPending(pool->Entries, pool->Count, total);

                RebuildIndex(pool, total);
                pool->Count = total;
            }

            /// <summary>
            ///     Inserts a short pending suffix into the ordered prefix in place.
            /// </summary>
            /// <param name="table">The table containing the indexed prefix and pending suffix.</param>
            /// <param name="first">The first pending index.</param>
            /// <param name="total">The number of retained segments.</param>
            private static void InsertPending(DedicatedEntry* table, nuint first, nuint total)
            {
                ref var entries = ref Unsafe.AsRef<DedicatedEntry>(table);
                for (var index = first; index < total; index = index.WrappingAdd(Usize.FromI32Unchecked(1)))
                {
                    if (index == Usize.FromI32Unchecked(0))
                        continue;

                    ref var pending = ref Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(index));
                    var previous = index.WrappingSub(Usize.FromI32Unchecked(1));
                    ref var tail = ref Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(previous));
                    if (tail.Key < pending.Key)
                        continue;

                    var entry = pending;
                    var lower = Usize.FromI32Unchecked(0);
                    var upper = index;
                    while (lower < upper)
                    {
                        var middle = lower.WrappingAdd(upper) >> 1;
                        ref var item = ref Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(middle));
                        if (item.Key < entry.Key)
                            lower = middle.WrappingAdd(Usize.FromI32Unchecked(1));
                        else
                            upper = middle;
                    }

                    var suffix = index.WrappingSub(lower);
                    if (suffix <= INLINE_MOVE_LIMIT)
                    {
                        for (var target = index; target > lower; target = target.WrappingSub(Usize.FromI32Unchecked(1)))
                        {
                            var source = target.WrappingSub(Usize.FromI32Unchecked(1));
                            Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(target)) = Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(source));
                        }
                    }
                    else
                    {
                        var source = UnsafeHelpers.Add<DedicatedEntry>(table, Isize.FromUsizeUnchecked(lower));
                        var destination = UnsafeHelpers.Add<DedicatedEntry>(source, Isize.FromI32Unchecked(1));
                        SpanHelpers.Move(ref Unsafe.AsRef<byte>(destination), ref Unsafe.AsRef<byte>(source), suffix.WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>())));
                    }

                    Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(lower)) = entry;
                }
            }

            /// <summary>
            ///     Sorts newly retained segments and merges them with the indexed prefix.
            /// </summary>
            /// <param name="entries">The table containing the indexed prefix and pending suffix.</param>
            /// <param name="first">The first pending index.</param>
            /// <param name="total">The number of retained segments.</param>
            /// <remarks>
            ///     The scratch table holds at most 255 entries. Sorting adjacent pairs directly
            ///     avoids merging one-entry runs. Choosing the newer entry or run for equal keys
            ///     preserves the insertion path's newest-first order.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            [SkipLocalsInit]
            private static void MergePending(DedicatedEntry* entries, int first, int total)
            {
                var next = first.WrappingAdd(1);
                for (; next < total; next = next.WrappingAdd(1))
                {
                    var previous = UnsafeHelpers.Add<DedicatedEntry>(entries, Isize.FromI32Unchecked(next.WrappingSub(1)));
                    if (previous->Key >= UnsafeHelpers.Add<DedicatedEntry>(previous, Isize.FromI32Unchecked(1))->Key)
                        break;
                }

                var sorted = next == total;
                if (sorted)
                {
                    if (first == 0)
                        return;

                    var suffix = UnsafeHelpers.Add<DedicatedEntry>(entries, Isize.FromI32Unchecked(first));
                    if (UnsafeHelpers.Add<DedicatedEntry>(suffix, Isize.FromI32Unchecked(-1))->Key < suffix->Key)
                        return;
                }

                var scratchBytes = DEDICATED_LIMIT.WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>()));
                var buffer = stackalloc byte[I32.FromUsizeUnchecked(scratchBytes)];
                var scratch = (DedicatedEntry*)buffer;
                var source = entries;
                var destination = scratch;
                var pending = total.WrappingSub(first);
                var width = pending;
                if (!sorted)
                {
                    var index = first;
                    var last = total.WrappingSub(1);
                    for (; index < last; index = index.WrappingAdd(2))
                    {
                        var left = UnsafeHelpers.Add<DedicatedEntry>(entries, Isize.FromI32Unchecked(index));
                        var right = UnsafeHelpers.Add<DedicatedEntry>(left, Isize.FromI32Unchecked(1));
                        var target = UnsafeHelpers.Add<DedicatedEntry>(scratch, Isize.FromI32Unchecked(index));
                        var second = UnsafeHelpers.Add<DedicatedEntry>(target, Isize.FromI32Unchecked(1));
                        if (left->Key < right->Key)
                        {
                            Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(left);
                            Unsafe.AsRef<DedicatedEntry>(second) = Unsafe.AsRef<DedicatedEntry>(right);
                        }
                        else
                        {
                            Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(right);
                            Unsafe.AsRef<DedicatedEntry>(second) = Unsafe.AsRef<DedicatedEntry>(left);
                        }
                    }

                    if (index == last)
                        Unsafe.Add(ref Unsafe.AsRef<DedicatedEntry>(scratch), Isize.FromI32Unchecked(index)) = Unsafe.Add(ref Unsafe.AsRef<DedicatedEntry>(entries), Isize.FromI32Unchecked(index));

                    source = scratch;
                    destination = entries;
                    width = 2;
                }

                for (; width < pending; width <<= 1)
                {
                    var stride = width << 1;
                    for (var start = first; start < total; start = start.WrappingAdd(stride))
                    {
                        var middle = Math.Min(start.WrappingAdd(width), total);
                        var end = Math.Min(start.WrappingAdd(stride), total);
                        MergeRuns(source, destination, start, middle, end);
                    }

                    var previous = source;
                    source = destination;
                    destination = previous;
                }

                if (first == 0)
                {
                    if (source != entries)
                        SpanHelpers.Move(ref Unsafe.AsRef<byte>(entries), ref Unsafe.AsRef<byte>(source), Usize.FromI32Unchecked(total).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>())));

                    return;
                }

                if (source == scratch)
                {
                    var bytes = Usize.FromI32Unchecked(first).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>()));
                    SpanHelpers.Move(ref Unsafe.AsRef<byte>(scratch), ref Unsafe.AsRef<byte>(entries), bytes);
                    MergeRuns(scratch, entries, 0, first, total);
                }
                else
                {
                    MergeRuns(entries, scratch, 0, first, total);
                    SpanHelpers.Move(ref Unsafe.AsRef<byte>(entries), ref Unsafe.AsRef<byte>(scratch), Usize.FromI32Unchecked(total).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<DedicatedEntry>())));
                }
            }

            /// <summary>
            ///     Merges two adjacent sorted runs into a separate table, choosing the newer right run for equal keys.
            /// </summary>
            /// <param name="source">The table containing both runs.</param>
            /// <param name="destination">The separate destination table.</param>
            /// <param name="start">The first index of the left run.</param>
            /// <param name="middle">The first index of the right run.</param>
            /// <param name="end">The exclusive end of the right run.</param>
            private static void MergeRuns(DedicatedEntry* source, DedicatedEntry* destination, int start, int middle, int end)
            {
                var left = UnsafeHelpers.Add<DedicatedEntry>(source, Isize.FromI32Unchecked(start));
                var right = UnsafeHelpers.Add<DedicatedEntry>(source, Isize.FromI32Unchecked(middle));
                var leftEnd = right;
                var rightEnd = UnsafeHelpers.Add<DedicatedEntry>(source, Isize.FromI32Unchecked(end));
                var target = UnsafeHelpers.Add<DedicatedEntry>(destination, Isize.FromI32Unchecked(start));
                while (left != leftEnd && right != rightEnd)
                {
                    if (left->Key < right->Key)
                    {
                        Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(left);
                        left = UnsafeHelpers.Add<DedicatedEntry>(left, Isize.FromI32Unchecked(1));
                    }
                    else
                    {
                        Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(right);
                        right = UnsafeHelpers.Add<DedicatedEntry>(right, Isize.FromI32Unchecked(1));
                    }

                    target = UnsafeHelpers.Add<DedicatedEntry>(target, Isize.FromI32Unchecked(1));
                }

                while (left != leftEnd)
                {
                    Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(left);
                    target = UnsafeHelpers.Add<DedicatedEntry>(target, Isize.FromI32Unchecked(1));
                    left = UnsafeHelpers.Add<DedicatedEntry>(left, Isize.FromI32Unchecked(1));
                }

                while (right != rightEnd)
                {
                    Unsafe.AsRef<DedicatedEntry>(target) = Unsafe.AsRef<DedicatedEntry>(right);
                    target = UnsafeHelpers.Add<DedicatedEntry>(target, Isize.FromI32Unchecked(1));
                    right = UnsafeHelpers.Add<DedicatedEntry>(right, Isize.FromI32Unchecked(1));
                }
            }

            /// <summary>
            ///     Builds the cached lower bounds and inclusive alignment masks for a sorted table.
            /// </summary>
            /// <param name="pool">The dedicated pool whose entries have been ordered.</param>
            /// <param name="total">The number of initialized entries.</param>
            /// <remarks>
            ///     For each cached page count, the first sufficiently large entry sets the lower bound.
            ///     A descending pass unions each alignment row into the row below it.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void RebuildIndex(DedicatedPool* pool, nuint total)
            {
                ref var entries = ref Unsafe.AsRef<DedicatedEntry>(pool->Entries);
                var rowWidth = BitmapWordCount;
                var maskWords = pool->HighestRow.WrappingAdd(Usize.FromI32Unchecked(1)).WrappingMul(rowWidth);
                SpanHelpers.Set(ref Unsafe.AsRef<byte>(pool->AlignmentMasks), U8.FromI32Unchecked(0), maskWords.WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<nuint>())));
                ref var masks = ref Unsafe.AsRef<nuint>(pool->AlignmentMasks);
                ref var bounds = ref Unsafe.AsRef<nuint>(pool->LowerBounds);
                var nextPage = Usize.FromI32Unchecked(0);
                for (var index = Usize.FromI32Unchecked(0); index < total; index = index.WrappingAdd(Usize.FromI32Unchecked(1)))
                {
                    var entry = Unsafe.Add(ref entries, Isize.FromUsizeUnchecked(index));
                    var word = entry.AlignmentRow.WrappingMul(rowWidth).WrappingAdd(index >> BitmapWordShift);
                    Unsafe.Add(ref masks, Isize.FromUsizeUnchecked(word)) |= Usize.FromI32Unchecked(1) << (I32.FromUsizeUnchecked(index) & BitmapWordMask);
                    if (nextPage < Usize.FromI32Unchecked(CACHED_PAGE_COUNTS) && nextPage <= entry.Pages)
                    {
                        var end = MathHelpers.Min(entry.Pages.WrappingAdd(Usize.FromI32Unchecked(1)), Usize.FromI32Unchecked(CACHED_PAGE_COUNTS));
                        for (; nextPage < end; nextPage = nextPage.WrappingAdd(Usize.FromI32Unchecked(1)))
                            Unsafe.Add(ref bounds, Isize.FromUsizeUnchecked(nextPage)) = index;
                    }
                }

                for (; nextPage < Usize.FromI32Unchecked(CACHED_PAGE_COUNTS); nextPage = nextPage.WrappingAdd(Usize.FromI32Unchecked(1)))
                    Unsafe.Add(ref bounds, Isize.FromUsizeUnchecked(nextPage)) = total;

                ref var rows = ref Unsafe.AsRef<Bitmap>(pool->AlignmentMasks);
                for (var row = pool->HighestRow; row != Usize.FromI32Unchecked(0); row = row.WrappingSub(Usize.FromI32Unchecked(1)))
                {
                    ref var high = ref Unsafe.As<Bitmap, ulong>(ref Unsafe.Add(ref rows, Isize.FromUsizeUnchecked(row)));
                    ref var low = ref Unsafe.As<Bitmap, ulong>(ref Unsafe.Add(ref rows, Isize.FromUsizeUnchecked(row.WrappingSub(Usize.FromI32Unchecked(1)))));
                    low |= high;
                    Unsafe.Add(ref low, Isize.FromI32Unchecked(1)) |= Unsafe.Add(ref high, Isize.FromI32Unchecked(1));
                    Unsafe.Add(ref low, Isize.FromI32Unchecked(2)) |= Unsafe.Add(ref high, Isize.FromI32Unchecked(2));
                    Unsafe.Add(ref low, Isize.FromI32Unchecked(3)) |= Unsafe.Add(ref high, Isize.FromI32Unchecked(3));
                }
            }
        }

        /// <summary>
        ///     Holds the allocator's native tables, page cursors and refill state.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct State
        {
            /// <summary>
            ///     The cache-line-aligned slots used by allocation threads.
            /// </summary>
            public readonly Shard* Shards;

            /// <summary>
            ///     The retained standard segments, starting with the embedded segment.
            /// </summary>
            public readonly Segment* Standard;

            /// <summary>
            ///     Serializes shared allocation, refill and segment growth.
            /// </summary>
            public UnsafeAtomicU32 RefillLock;

            /// <summary>
            ///     The number of initialized standard segment descriptors.
            /// </summary>
            public nuint StandardCount;

            /// <summary>
            ///     The index of the standard segment supplying pages.
            /// </summary>
            public nuint ActiveSegment;

            /// <summary>
            ///     The number of standard pages claimed since rewind.
            /// </summary>
            public nuint ClaimedPages;

            /// <summary>
            ///     The page budget after subtracting retained dedicated segments.
            /// </summary>
            public nuint StandardPageLimit;

            /// <summary>
            ///     The next unclaimed page in the active segment.
            /// </summary>
            public byte* NextPage;

            /// <summary>
            ///     The exclusive end of the active segment.
            /// </summary>
            public byte* SegmentEnd;

            /// <summary>
            ///     The lower address of the chunk used by threads whose slots collide.
            /// </summary>
            public nuint SharedFloor;

            /// <summary>
            ///     The exclusive upper address of the unused prefix of the shared chunk.
            /// </summary>
            public nuint SharedCursor;

            /// <summary>
            ///     The addresses of unassigned chunks indexed by their size order.
            /// </summary>
            public readonly nuint* Chunks;

            /// <summary>
            ///     The size orders with an unassigned chunk.
            /// </summary>
            public uint ChunkMask;

            /// <summary>
            ///     The primary slots whose ownership must be cleared on rewind.
            /// </summary>
            public ulong OccupiedPrimaryShards;

            /// <summary>
            ///     The secondary slots whose ownership must be cleared on rewind.
            /// </summary>
            public ulong OccupiedSecondaryShards;

            /// <summary>
            ///     The ordered index and reuse state of dedicated segments.
            /// </summary>
            public DedicatedPool Dedicated;

            /// <summary>
            ///     Initializes the control block over its native tables and first standard segment.
            /// </summary>
            /// <param name="shards">The zeroed thread slots.</param>
            /// <param name="standard">The standard descriptor table with its first entry initialized.</param>
            /// <param name="entries">The storage for dedicated entries.</param>
            /// <param name="lowerBounds">The zeroed dedicated capacity index.</param>
            /// <param name="chunks">The storage for unassigned chunk addresses.</param>
            private State(Shard* shards, Segment* standard, DedicatedEntry* entries, nuint* lowerBounds, nuint* chunks) : this()
            {
                Shards = shards;
                Standard = standard;
                StandardCount = Usize.FromI32Unchecked(1);
                StandardPageLimit = PAGE_LIMIT;
                NextPage = standard->Start;
                SegmentEnd = UnsafeHelpers.AddByteOffset<byte>(NextPage, Isize.FromUsizeUnchecked(standard->Capacity));
                Chunks = chunks;
                SharedCursor = EMPTY_CURSOR;
                SharedFloor = EMPTY_FLOOR;
                Dedicated = DedicatedPool.From(entries, lowerBounds);
            }

            /// <summary>
            ///     Creates a control block over initialized native storage.
            /// </summary>
            /// <param name="shards">The zeroed thread slots.</param>
            /// <param name="standard">The standard descriptor table with its first entry initialized.</param>
            /// <param name="entries">The storage for dedicated entries.</param>
            /// <param name="lowerBounds">The zeroed dedicated capacity index.</param>
            /// <param name="chunks">The storage for unassigned chunk addresses.</param>
            /// <returns>The initialized allocator state.</returns>
            public static State From(Shard* shards, Segment* standard, DedicatedEntry* entries, nuint* lowerBounds, nuint* chunks) => new(shards, standard, entries, lowerBounds, chunks);
        }
    }
}