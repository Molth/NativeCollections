using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native dynamic heap (Two-Level Segregated Fit) memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeDynamicMemoryPool : IDisposable
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly void* _tlsf;

        /// <summary>
        ///     Size
        /// </summary>
        private readonly ulong _size;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDynamicMemoryPool(ulong size)
        {
            ulong actualSize;
            void* array;
            void* handle;
            if (IntPtr.Size == 8)
            {
                actualSize = TLSF64.align_up(TLSF64.tlsf_size() + TLSF64.tlsf_pool_overhead() + 7 * TLSF64.tlsf_alloc_overhead() + size, 8);
#if NET6_0_OR_GREATER
                if (actualSize > uint.MaxValue)
#else
                if (actualSize > int.MaxValue)
#endif
                    throw new ArgumentOutOfRangeException(nameof(size), size, "MustBeLess");
                array = NativeMemoryAllocator.Alloc((uint)actualSize);
                handle = TLSF64.tlsf_create_with_pool(array, actualSize);
                if (handle == null)
                {
                    NativeMemoryAllocator.Free(array);
                    throw new ArgumentOutOfRangeException(nameof(size), "Memory must be aligned to 8 bytes.");
                }
            }
            else
            {
                actualSize = TLSF32.align_up((uint)(TLSF32.tlsf_size() + TLSF32.tlsf_pool_overhead() + 7 * TLSF32.tlsf_alloc_overhead() + size), 4);
                if (actualSize > TLSF32.block_size_max)
                    throw new ArgumentOutOfRangeException(nameof(size), size, "MustBeLess");
                array = NativeMemoryAllocator.Alloc((uint)actualSize);
                handle = TLSF32.tlsf_create_with_pool(array, (uint)actualSize);
                if (handle == null)
                {
                    NativeMemoryAllocator.Free(array);
                    throw new ArgumentOutOfRangeException(nameof(size), "Memory must be aligned to 4 bytes.");
                }
            }

            _tlsf = handle;
            _size = size;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _tlsf != null;

        /// <summary>
        ///     Size
        /// </summary>
        public ulong Size => _size;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeDynamicMemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeDynamicMemoryPool nativeDynamicMemoryPool && nativeDynamicMemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_tlsf;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeDynamicMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left._tlsf == right._tlsf;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left._tlsf != right._tlsf;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_tlsf == null)
                return;
            NativeMemoryAllocator.Free(_tlsf);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="size">Size</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(ulong size) => IntPtr.Size == 8 ? TLSF64.tlsf_malloc(_tlsf, size) : TLSF32.tlsf_malloc(_tlsf, (uint)size);

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="actualSize">Actual size</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(ulong size, out ulong actualSize)
        {
            if (IntPtr.Size == 8)
            {
                var ptr = TLSF64.tlsf_malloc(_tlsf, size);
                actualSize = ptr != null ? TLSF64.tlsf_block_size(ptr) : 0;
                return ptr;
            }
            else
            {
                var ptr = TLSF32.tlsf_malloc(_tlsf, (uint)size);
                actualSize = ptr != null ? TLSF32.tlsf_block_size(ptr) : 0;
                return ptr;
            }
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (IntPtr.Size == 8)
                TLSF64.tlsf_free(_tlsf, ptr);
            else
                TLSF32.tlsf_free(_tlsf, ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeDynamicMemoryPool Empty => new();

        /// <summary>
        ///     Two-Level Segregated Fit memory allocator 64
        /// </summary>
        private static class TLSF64
        {
            public const int SL_INDEX_COUNT_LOG2 = 5;
            public const int ALIGN_SIZE_LOG2 = 3;
            public const int ALIGN_SIZE = 1 << ALIGN_SIZE_LOG2;
            public const int FL_INDEX_MAX = 32;
            public const int SL_INDEX_COUNT = 1 << SL_INDEX_COUNT_LOG2;
            public const int FL_INDEX_SHIFT = SL_INDEX_COUNT_LOG2 + ALIGN_SIZE_LOG2;
            public const int FL_INDEX_COUNT = FL_INDEX_MAX - FL_INDEX_SHIFT + 1;
            public const int SMALL_BLOCK_SIZE = 1 << FL_INDEX_SHIFT;
            public const ulong block_header_free_bit = 1 << 0;
            public const ulong block_header_prev_free_bit = 1 << 1;
            public const ulong block_header_overhead = sizeof(ulong);
            public const ulong block_start_offset = sizeof(ulong) + sizeof(ulong);
            public static readonly ulong block_size_min = (ulong)(sizeof(block_header_t) - sizeof(ulong));
            public const ulong block_size_max = (ulong)1 << FL_INDEX_MAX;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void memcpy(void* dst, void* src, ulong size) => Unsafe.CopyBlock(dst, src, (uint)size);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_fls(uint word)
            {
                var bit = 32 - BitOperationsHelpers.LeadingZeroCount(word);
                return bit - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_ffs(uint word)
            {
                var reverse = word & (~word + 1);
                var bit = 32 - BitOperationsHelpers.LeadingZeroCount(reverse);
                return bit - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_fls_sizet(ulong size)
            {
                var bit = BitOperationsHelpers.LeadingZeroCount(size);
                return bit == 64 ? -1 : 63 - bit;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_min(ulong a, ulong b) => a < b ? a : b;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_max(ulong a, ulong b) => a > b ? a : b;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong block_size(block_header_t* block) => block->size & ~(block_header_free_bit | block_header_prev_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_size(block_header_t* block, ulong size)
            {
                var oldsize = block->size;
                block->size = size | (oldsize & (block_header_free_bit | block_header_prev_free_bit));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_last(block_header_t* block) => block_size(block) == 0 ? 1 : 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_free(block_header_t* block) => (int)(block->size & block_header_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_free(block_header_t* block) => block->size |= block_header_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_used(block_header_t* block) => block->size &= ~block_header_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_prev_free(block_header_t* block) => (int)(block->size & block_header_prev_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_prev_free(block_header_t* block) => block->size |= block_header_prev_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_prev_used(block_header_t* block) => block->size &= ~block_header_prev_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_from_ptr(void* ptr) => (block_header_t*)((byte*)ptr - block_start_offset);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_to_ptr(block_header_t* block) => (byte*)block + block_start_offset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* offset_to_block(void* ptr, ulong size) => (block_header_t*)((nint)ptr + (nint)size);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_prev(block_header_t* block) => block->prev_phys_block;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_next(block_header_t* block)
            {
                var next = offset_to_block(block_to_ptr(block), block_size(block) - block_header_overhead);
                return next;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_link_next(block_header_t* block)
            {
                var next = block_next(block);
                next->prev_phys_block = block;
                return next;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_mark_as_free(block_header_t* block)
            {
                var next = block_link_next(block);
                block_set_prev_free(next);
                block_set_free(block);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_mark_as_used(block_header_t* block)
            {
                var next = block_next(block);
                block_set_prev_used(next);
                block_set_used(block);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong align_up(ulong x, ulong align) => (x + (align - 1)) & ~(align - 1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong align_down(ulong x, ulong align) => x - (x & (align - 1));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* align_ptr(void* ptr, ulong align)
            {
                var aligned = ((nint)ptr + (nint)(align - 1)) & ~ (nint)(align - 1);
                return (void*)aligned;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong adjust_request_size(ulong size, ulong align)
            {
                ulong adjust = 0;
                if (size != 0)
                {
                    var aligned = align_up(size, align);
                    if (aligned < block_size_max)
                        adjust = tlsf_max(aligned, block_size_min);
                }

                return adjust;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void mapping_insert(ulong size, int* fli, int* sli)
            {
                int fl, sl;
                if (size < SMALL_BLOCK_SIZE)
                {
                    fl = 0;
                    sl = (int)size / (SMALL_BLOCK_SIZE / SL_INDEX_COUNT);
                }
                else
                {
                    fl = tlsf_fls_sizet(size);
                    sl = (int)(size >> (fl - SL_INDEX_COUNT_LOG2)) ^ (1 << SL_INDEX_COUNT_LOG2);
                    fl -= FL_INDEX_SHIFT - 1;
                }

                *fli = fl;
                *sli = sl;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void mapping_search(ulong size, int* fli, int* sli)
            {
                if (size >= SMALL_BLOCK_SIZE)
                {
                    var round = (ulong)((1 << (tlsf_fls_sizet(size) - SL_INDEX_COUNT_LOG2)) - 1);
                    size += round;
                }

                mapping_insert(size, fli, sli);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* search_suitable_block(control_t* control, int* fli, int* sli)
            {
                var fl = *fli;
                var sl = *sli;
                var sl_map = control->sl_bitmap[fl] & (~0U << sl);
                if (!(sl_map != 0))
                {
                    var fl_map = control->fl_bitmap & (~0U << (fl + 1));
                    if (!(fl_map != 0))
                        return null;
                    fl = tlsf_ffs(fl_map);
                    *fli = fl;
                    sl_map = control->sl_bitmap[fl];
                }

                sl = tlsf_ffs(sl_map);
                *sli = sl;
                return get_blocks(control, fl)[sl];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void remove_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var prev = block->prev_free;
                var next = block->next_free;
                next->prev_free = prev;
                prev->next_free = next;
                if (get_blocks(control, fl)[sl] == block)
                {
                    get_blocks(control, fl)[sl] = next;
                    if (next == &control->block_null)
                    {
                        control->sl_bitmap[fl] &= ~(1U << sl);
                        if (!(control->sl_bitmap[fl] != 0))
                            control->fl_bitmap &= ~(1U << fl);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void insert_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var current = get_blocks(control, fl)[sl];
                block->next_free = current;
                block->prev_free = &control->block_null;
                current->prev_free = block;
                get_blocks(control, fl)[sl] = block;
                control->fl_bitmap |= 1U << fl;
                control->sl_bitmap[fl] |= 1U << sl;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_remove(control_t* control, block_header_t* block)
            {
                int fl, sl;
                mapping_insert(block_size(block), &fl, &sl);
                remove_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_insert(control_t* control, block_header_t* block)
            {
                int fl, sl;
                mapping_insert(block_size(block), &fl, &sl);
                insert_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_can_split(block_header_t* block, ulong size) => block_size(block) >= (ulong)sizeof(block_header_t) + size ? 1 : 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_split(block_header_t* block, ulong size)
            {
                var remaining = offset_to_block(block_to_ptr(block), size - block_header_overhead);
                var remain_size = block_size(block) - (size + block_header_overhead);
                block_set_size(remaining, remain_size);
                block_set_size(block, size);
                block_mark_as_free(remaining);
                return remaining;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_absorb(block_header_t* prev, block_header_t* block)
            {
                prev->size += block_size(block) + block_header_overhead;
                block_link_next(prev);
                return prev;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_merge_prev(control_t* control, block_header_t* block)
            {
                if (block_is_prev_free(block) != 0)
                {
                    var prev = block_prev(block);
                    block_remove(control, prev);
                    block = block_absorb(prev, block);
                }

                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_merge_next(control_t* control, block_header_t* block)
            {
                var next = block_next(block);
                if (block_is_free(next) != 0)
                {
                    block_remove(control, next);
                    block = block_absorb(block, next);
                }

                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_trim_free(control_t* control, block_header_t* block, ulong size)
            {
                if (block_can_split(block, size) != 0)
                {
                    var remaining_block = block_split(block, size);
                    block_link_next(block);
                    block_set_prev_free(remaining_block);
                    block_insert(control, remaining_block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_trim_used(control_t* control, block_header_t* block, ulong size)
            {
                if (block_can_split(block, size) != 0)
                {
                    var remaining_block = block_split(block, size);
                    block_set_prev_used(remaining_block);
                    remaining_block = block_merge_next(control, remaining_block);
                    block_insert(control, remaining_block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_trim_free_leading(control_t* control, block_header_t* block, ulong size)
            {
                var remaining_block = block;
                if (block_can_split(block, size) != 0)
                {
                    remaining_block = block_split(block, size - block_header_overhead);
                    block_set_prev_free(remaining_block);
                    block_link_next(block);
                    block_insert(control, block);
                }

                return remaining_block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_locate_free(control_t* control, ulong size)
            {
                int fl = 0, sl = 0;
                block_header_t* block = null;
                if (size != 0)
                {
                    mapping_search(size, &fl, &sl);
                    if (fl < FL_INDEX_COUNT)
                        block = search_suitable_block(control, &fl, &sl);
                }

                if (block != null)
                    remove_free_block(control, block, fl, sl);
                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_prepare_used(control_t* control, block_header_t* block, ulong size)
            {
                void* p = null;
                if (block != null)
                {
                    block_trim_free(control, block, size);
                    block_mark_as_used(block);
                    p = block_to_ptr(block);
                }

                return p;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void control_construct(control_t* control)
            {
                int i, j;
                control->block_null.next_free = &control->block_null;
                control->block_null.prev_free = &control->block_null;
                control->fl_bitmap = 0;
                for (i = 0; i < FL_INDEX_COUNT; ++i)
                {
                    control->sl_bitmap[i] = 0;
                    for (j = 0; j < SL_INDEX_COUNT; ++j)
                        get_blocks(control, i)[j] = &control->block_null;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_block_size(void* ptr)
            {
                ulong size = 0;
                if (ptr != null)
                {
                    var block = block_from_ptr(ptr);
                    size = block_size(block);
                }

                return size;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_size() => (ulong)sizeof(control_t);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_align_size() => ALIGN_SIZE;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_block_size_min() => block_size_min;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_block_size_max() => block_size_max;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_pool_overhead() => 2 * block_header_overhead;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_alloc_overhead() => block_header_overhead;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_add_pool(void* tlsf, void* mem, ulong bytes)
            {
                block_header_t* block;
                block_header_t* next;
                var pool_overhead = tlsf_pool_overhead();
                var pool_bytes = align_down(bytes - pool_overhead, ALIGN_SIZE);
                if ((long)mem % ALIGN_SIZE != 0)
                    return null;
                if (pool_bytes < block_size_min || pool_bytes > block_size_max)
                    return null;
                block = offset_to_block(mem, unchecked((ulong)-(nint)block_header_overhead));
                block_set_size(block, pool_bytes);
                block_set_free(block);
                block_set_prev_used(block);
                block_insert((control_t*)tlsf, block);
                next = block_link_next(block);
                block_set_size(next, 0);
                block_set_used(next);
                block_set_prev_free(next);
                return mem;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void tlsf_remove_pool(void* tlsf, void* pool)
            {
                var control = (control_t*)tlsf;
                var block = offset_to_block(pool, unchecked((ulong)-(int)block_header_overhead));
                int fl = 0, sl = 0;
                mapping_insert(block_size(block), &fl, &sl);
                remove_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_create(void* mem)
            {
                if ((nint)mem % ALIGN_SIZE != 0)
                    return null;
                control_construct((control_t*)mem);
                return mem;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_create_with_pool(void* mem, ulong bytes)
            {
                var tlsf = tlsf_create(mem);
                tlsf_add_pool(tlsf, (byte*)mem + tlsf_size(), bytes - tlsf_size());
                return tlsf;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_get_pool(void* tlsf) => (byte*)tlsf + tlsf_size();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_malloc(void* tlsf, ulong size)
            {
                var control = (control_t*)tlsf;
                var adjust = adjust_request_size(size, ALIGN_SIZE);
                var block = block_locate_free(control, adjust);
                return block_prepare_used(control, block, adjust);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_memalign(void* tlsf, ulong align, ulong size)
            {
                var control = (control_t*)tlsf;
                var adjust = adjust_request_size(size, ALIGN_SIZE);
                var gap_minimum = (ulong)sizeof(block_header_t);
                var size_with_gap = adjust_request_size(adjust + align + gap_minimum, align);
                var aligned_size = adjust != 0 && align > ALIGN_SIZE ? size_with_gap : adjust;
                var block = block_locate_free(control, aligned_size);
                if (block != null)
                {
                    var ptr = block_to_ptr(block);
                    var aligned = align_ptr(ptr, align);
                    var gap = (ulong)((nint)aligned - (nint)ptr);
                    if (gap != 0 && gap < gap_minimum)
                    {
                        var gap_remain = gap_minimum - gap;
                        var offset = tlsf_max(gap_remain, align);
                        var next_aligned = (void*)((nint)aligned + (nint)offset);
                        aligned = align_ptr(next_aligned, align);
                        gap = (ulong)((nint)aligned - (nint)ptr);
                    }

                    if (gap != 0)
                        block = block_trim_free_leading(control, block, gap);
                }

                return block_prepare_used(control, block, adjust);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void tlsf_free(void* tlsf, void* ptr)
            {
                if (ptr != null)
                {
                    var control = (control_t*)tlsf;
                    var block = block_from_ptr(ptr);
                    block_mark_as_free(block);
                    block = block_merge_prev(control, block);
                    block = block_merge_next(control, block);
                    block_insert(control, block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_realloc(void* tlsf, void* ptr, ulong size)
            {
                var control = (control_t*)tlsf;
                void* p = null;
                if (ptr != null && size == 0)
                    tlsf_free(tlsf, ptr);
                else if (!(ptr != null))
                    p = tlsf_malloc(tlsf, size);
                else
                {
                    var block = block_from_ptr(ptr);
                    var next = block_next(block);
                    var cursize = block_size(block);
                    var combined = cursize + block_size(next) + block_header_overhead;
                    var adjust = adjust_request_size(size, ALIGN_SIZE);
                    if (adjust > cursize && (!(block_is_free(next) != 0) || adjust > combined))
                    {
                        p = tlsf_malloc(tlsf, size);
                        if (p != null)
                        {
                            var minsize = tlsf_min(cursize, size);
                            memcpy(p, ptr, minsize);
                            tlsf_free(tlsf, ptr);
                        }
                    }
                    else
                    {
                        if (adjust > cursize)
                        {
                            block_merge_next(control, block);
                            block_mark_as_used(block);
                        }

                        block_trim_used(control, block, adjust);
                        p = ptr;
                    }
                }

                return p;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t** get_blocks(control_t* control, int i) => (block_header_t**)control->blocks + i * SL_INDEX_COUNT;

            [StructLayout(LayoutKind.Sequential)]
            public struct block_header_t
            {
                public block_header_t* prev_phys_block;
                public ulong size;
                public block_header_t* next_free;
                public block_header_t* prev_free;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct control_t
            {
                public block_header_t block_null;
                public uint fl_bitmap;
                public fixed uint sl_bitmap[FL_INDEX_COUNT];
                public fixed ulong blocks[FL_INDEX_COUNT * SL_INDEX_COUNT];
            }
        }

        /// <summary>
        ///     Two-Level Segregated Fit memory allocator 32
        /// </summary>
        private static class TLSF32
        {
            public const int SL_INDEX_COUNT_LOG2 = 5;
            public const int ALIGN_SIZE_LOG2 = 2;
            public const int ALIGN_SIZE = 1 << ALIGN_SIZE_LOG2;
            public const int FL_INDEX_MAX = 30;
            public const int SL_INDEX_COUNT = 1 << SL_INDEX_COUNT_LOG2;
            public const int FL_INDEX_SHIFT = SL_INDEX_COUNT_LOG2 + ALIGN_SIZE_LOG2;
            public const int FL_INDEX_COUNT = FL_INDEX_MAX - FL_INDEX_SHIFT + 1;
            public const int SMALL_BLOCK_SIZE = 1 << FL_INDEX_SHIFT;
            public const uint block_header_free_bit = 1 << 0;
            public const uint block_header_prev_free_bit = 1 << 1;
            public const uint block_header_overhead = sizeof(uint);
            public const uint block_start_offset = sizeof(uint) + sizeof(uint);
            public static readonly uint block_size_min = (uint)(sizeof(block_header_t) - sizeof(uint));
            public const uint block_size_max = (uint)1 << FL_INDEX_MAX;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void memcpy(void* dst, void* src, uint size) => Unsafe.CopyBlock(dst, src, size);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_fls(uint word)
            {
                var bit = 32 - BitOperationsHelpers.LeadingZeroCount(word);
                return bit - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_ffs(uint word)
            {
                var reverse = word & (~word + 1);
                var bit = 32 - BitOperationsHelpers.LeadingZeroCount(reverse);
                return bit - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int tlsf_fls_sizet(uint size) => tlsf_fls(size);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_min(uint a, uint b) => a < b ? a : b;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_max(uint a, uint b) => a > b ? a : b;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint block_size(block_header_t* block) => block->size & ~(block_header_free_bit | block_header_prev_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_size(block_header_t* block, uint size)
            {
                var oldsize = block->size;
                block->size = size | (oldsize & (block_header_free_bit | block_header_prev_free_bit));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_last(block_header_t* block) => block_size(block) == 0 ? 1 : 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_free(block_header_t* block) => (int)(block->size & block_header_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_free(block_header_t* block) => block->size |= block_header_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_used(block_header_t* block) => block->size &= ~block_header_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_is_prev_free(block_header_t* block) => (int)(block->size & block_header_prev_free_bit);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_prev_free(block_header_t* block) => block->size |= block_header_prev_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_set_prev_used(block_header_t* block) => block->size &= ~block_header_prev_free_bit;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_from_ptr(void* ptr) => (block_header_t*)((byte*)ptr - block_start_offset);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_to_ptr(block_header_t* block) => (byte*)block + block_start_offset;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* offset_to_block(void* ptr, uint size) => (block_header_t*)((nint)ptr + (nint)size);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_prev(block_header_t* block) => block->prev_phys_block;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_next(block_header_t* block)
            {
                var next = offset_to_block(block_to_ptr(block), block_size(block) - block_header_overhead);
                return next;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_link_next(block_header_t* block)
            {
                var next = block_next(block);
                next->prev_phys_block = block;
                return next;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_mark_as_free(block_header_t* block)
            {
                var next = block_link_next(block);
                block_set_prev_free(next);
                block_set_free(block);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_mark_as_used(block_header_t* block)
            {
                var next = block_next(block);
                block_set_prev_used(next);
                block_set_used(block);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint align_up(uint x, uint align) => (x + (align - 1)) & ~(align - 1);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint align_down(uint x, uint align) => x - (x & (align - 1));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* align_ptr(void* ptr, uint align)
            {
                var aligned = ((nint)ptr + (nint)(align - 1)) & ~ (nint)(align - 1);
                return (void*)aligned;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint adjust_request_size(uint size, uint align)
            {
                uint adjust = 0;
                if (size != 0)
                {
                    var aligned = align_up(size, align);
                    if (aligned < block_size_max)
                        adjust = tlsf_max(aligned, block_size_min);
                }

                return adjust;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void mapping_insert(uint size, int* fli, int* sli)
            {
                int fl, sl;
                if (size < SMALL_BLOCK_SIZE)
                {
                    fl = 0;
                    sl = (int)size / (SMALL_BLOCK_SIZE / SL_INDEX_COUNT);
                }
                else
                {
                    fl = tlsf_fls_sizet(size);
                    sl = (int)(size >> (fl - SL_INDEX_COUNT_LOG2)) ^ (1 << SL_INDEX_COUNT_LOG2);
                    fl -= FL_INDEX_SHIFT - 1;
                }

                *fli = fl;
                *sli = sl;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void mapping_search(uint size, int* fli, int* sli)
            {
                if (size >= SMALL_BLOCK_SIZE)
                {
                    var round = (uint)((1 << (tlsf_fls_sizet(size) - SL_INDEX_COUNT_LOG2)) - 1);
                    size += round;
                }

                mapping_insert(size, fli, sli);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* search_suitable_block(control_t* control, int* fli, int* sli)
            {
                var fl = *fli;
                var sl = *sli;
                var sl_map = control->sl_bitmap[fl] & (~0U << sl);
                if (!(sl_map != 0))
                {
                    var fl_map = control->fl_bitmap & (~0U << (fl + 1));
                    if (!(fl_map != 0))
                        return null;
                    fl = tlsf_ffs(fl_map);
                    *fli = fl;
                    sl_map = control->sl_bitmap[fl];
                }

                sl = tlsf_ffs(sl_map);
                *sli = sl;
                return get_blocks(control, fl)[sl];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void remove_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var prev = block->prev_free;
                var next = block->next_free;
                next->prev_free = prev;
                prev->next_free = next;
                if (get_blocks(control, fl)[sl] == block)
                {
                    get_blocks(control, fl)[sl] = next;
                    if (next == &control->block_null)
                    {
                        control->sl_bitmap[fl] &= ~(1U << sl);
                        if (!(control->sl_bitmap[fl] != 0))
                            control->fl_bitmap &= ~(1U << fl);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void insert_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var current = get_blocks(control, fl)[sl];
                block->next_free = current;
                block->prev_free = &control->block_null;
                current->prev_free = block;
                get_blocks(control, fl)[sl] = block;
                control->fl_bitmap |= 1U << fl;
                control->sl_bitmap[fl] |= 1U << sl;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_remove(control_t* control, block_header_t* block)
            {
                int fl, sl;
                mapping_insert(block_size(block), &fl, &sl);
                remove_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_insert(control_t* control, block_header_t* block)
            {
                int fl, sl;
                mapping_insert(block_size(block), &fl, &sl);
                insert_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int block_can_split(block_header_t* block, uint size) => block_size(block) >= (uint)sizeof(block_header_t) + size ? 1 : 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_split(block_header_t* block, uint size)
            {
                var remaining = offset_to_block(block_to_ptr(block), size - block_header_overhead);
                var remain_size = block_size(block) - (size + block_header_overhead);
                block_set_size(remaining, remain_size);
                block_set_size(block, size);
                block_mark_as_free(remaining);
                return remaining;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_absorb(block_header_t* prev, block_header_t* block)
            {
                prev->size += block_size(block) + block_header_overhead;
                block_link_next(prev);
                return prev;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_merge_prev(control_t* control, block_header_t* block)
            {
                if (block_is_prev_free(block) != 0)
                {
                    var prev = block_prev(block);
                    block_remove(control, prev);
                    block = block_absorb(prev, block);
                }

                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_merge_next(control_t* control, block_header_t* block)
            {
                var next = block_next(block);
                if (block_is_free(next) != 0)
                {
                    block_remove(control, next);
                    block = block_absorb(block, next);
                }

                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_trim_free(control_t* control, block_header_t* block, uint size)
            {
                if (block_can_split(block, size) != 0)
                {
                    var remaining_block = block_split(block, size);
                    block_link_next(block);
                    block_set_prev_free(remaining_block);
                    block_insert(control, remaining_block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void block_trim_used(control_t* control, block_header_t* block, uint size)
            {
                if (block_can_split(block, size) != 0)
                {
                    var remaining_block = block_split(block, size);
                    block_set_prev_used(remaining_block);
                    remaining_block = block_merge_next(control, remaining_block);
                    block_insert(control, remaining_block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_trim_free_leading(control_t* control, block_header_t* block, uint size)
            {
                var remaining_block = block;
                if (block_can_split(block, size) != 0)
                {
                    remaining_block = block_split(block, size - block_header_overhead);
                    block_set_prev_free(remaining_block);
                    block_link_next(block);
                    block_insert(control, block);
                }

                return remaining_block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t* block_locate_free(control_t* control, uint size)
            {
                int fl = 0, sl = 0;
                block_header_t* block = null;
                if (size != 0)
                {
                    mapping_search(size, &fl, &sl);
                    if (fl < FL_INDEX_COUNT)
                        block = search_suitable_block(control, &fl, &sl);
                }

                if (block != null)
                    remove_free_block(control, block, fl, sl);
                return block;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_prepare_used(control_t* control, block_header_t* block, uint size)
            {
                void* p = null;
                if (block != null)
                {
                    block_trim_free(control, block, size);
                    block_mark_as_used(block);
                    p = block_to_ptr(block);
                }

                return p;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void control_construct(control_t* control)
            {
                int i, j;
                control->block_null.next_free = &control->block_null;
                control->block_null.prev_free = &control->block_null;
                control->fl_bitmap = 0;
                for (i = 0; i < FL_INDEX_COUNT; ++i)
                {
                    control->sl_bitmap[i] = 0;
                    for (j = 0; j < SL_INDEX_COUNT; ++j)
                        get_blocks(control, i)[j] = &control->block_null;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_block_size(void* ptr)
            {
                uint size = 0;
                if (ptr != null)
                {
                    var block = block_from_ptr(ptr);
                    size = block_size(block);
                }

                return size;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_size() => (uint)sizeof(control_t);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_align_size() => ALIGN_SIZE;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_block_size_min() => block_size_min;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_block_size_max() => block_size_max;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_pool_overhead() => 2 * block_header_overhead;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_alloc_overhead() => block_header_overhead;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_add_pool(void* tlsf, void* mem, uint bytes)
            {
                block_header_t* block;
                block_header_t* next;
                var pool_overhead = tlsf_pool_overhead();
                var pool_bytes = align_down(bytes - pool_overhead, ALIGN_SIZE);
                if ((long)mem % ALIGN_SIZE != 0)
                    return null;
                if (pool_bytes < block_size_min || pool_bytes > block_size_max)
                    return null;
                block = offset_to_block(mem, unchecked((uint)-(nint)block_header_overhead));
                block_set_size(block, pool_bytes);
                block_set_free(block);
                block_set_prev_used(block);
                block_insert((control_t*)tlsf, block);
                next = block_link_next(block);
                block_set_size(next, 0);
                block_set_used(next);
                block_set_prev_free(next);
                return mem;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void tlsf_remove_pool(void* tlsf, void* pool)
            {
                var control = (control_t*)tlsf;
                var block = offset_to_block(pool, unchecked((uint)-(int)block_header_overhead));
                int fl = 0, sl = 0;
                mapping_insert(block_size(block), &fl, &sl);
                remove_free_block(control, block, fl, sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_create(void* mem)
            {
                if ((nint)mem % ALIGN_SIZE != 0)
                    return null;
                control_construct((control_t*)mem);
                return mem;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_create_with_pool(void* mem, uint bytes)
            {
                var tlsf = tlsf_create(mem);
                tlsf_add_pool(tlsf, (byte*)mem + tlsf_size(), bytes - tlsf_size());
                return tlsf;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_get_pool(void* tlsf) => (byte*)tlsf + tlsf_size();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_malloc(void* tlsf, uint size)
            {
                var control = (control_t*)tlsf;
                var adjust = adjust_request_size(size, ALIGN_SIZE);
                var block = block_locate_free(control, adjust);
                return block_prepare_used(control, block, adjust);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_memalign(void* tlsf, uint align, uint size)
            {
                var control = (control_t*)tlsf;
                var adjust = adjust_request_size(size, ALIGN_SIZE);
                var gap_minimum = (uint)sizeof(block_header_t);
                var size_with_gap = adjust_request_size(adjust + align + gap_minimum, align);
                var aligned_size = adjust != 0 && align > ALIGN_SIZE ? size_with_gap : adjust;
                var block = block_locate_free(control, aligned_size);
                if (block != null)
                {
                    var ptr = block_to_ptr(block);
                    var aligned = align_ptr(ptr, align);
                    var gap = (uint)((nint)aligned - (nint)ptr);
                    if (gap != 0 && gap < gap_minimum)
                    {
                        var gap_remain = gap_minimum - gap;
                        var offset = tlsf_max(gap_remain, align);
                        var next_aligned = (void*)((nint)aligned + (nint)offset);
                        aligned = align_ptr(next_aligned, align);
                        gap = (uint)((nint)aligned - (nint)ptr);
                    }

                    if (gap != 0)
                        block = block_trim_free_leading(control, block, gap);
                }

                return block_prepare_used(control, block, adjust);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void tlsf_free(void* tlsf, void* ptr)
            {
                if (ptr != null)
                {
                    var control = (control_t*)tlsf;
                    var block = block_from_ptr(ptr);
                    block_mark_as_free(block);
                    block = block_merge_prev(control, block);
                    block = block_merge_next(control, block);
                    block_insert(control, block);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_realloc(void* tlsf, void* ptr, uint size)
            {
                var control = (control_t*)tlsf;
                void* p = null;
                if (ptr != null && size == 0)
                    tlsf_free(tlsf, ptr);
                else if (!(ptr != null))
                    p = tlsf_malloc(tlsf, size);
                else
                {
                    var block = block_from_ptr(ptr);
                    var next = block_next(block);
                    var cursize = block_size(block);
                    var combined = cursize + block_size(next) + block_header_overhead;
                    var adjust = adjust_request_size(size, ALIGN_SIZE);
                    if (adjust > cursize && (!(block_is_free(next) != 0) || adjust > combined))
                    {
                        p = tlsf_malloc(tlsf, size);
                        if (p != null)
                        {
                            var minsize = tlsf_min(cursize, size);
                            memcpy(p, ptr, minsize);
                            tlsf_free(tlsf, ptr);
                        }
                    }
                    else
                    {
                        if (adjust > cursize)
                        {
                            block_merge_next(control, block);
                            block_mark_as_used(block);
                        }

                        block_trim_used(control, block, adjust);
                        p = ptr;
                    }
                }

                return p;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static block_header_t** get_blocks(control_t* control, int i) => (block_header_t**)control->blocks + i * SL_INDEX_COUNT;

            [StructLayout(LayoutKind.Sequential)]
            public struct block_header_t
            {
                public block_header_t* prev_phys_block;
                public uint size;
                public block_header_t* next_free;
                public block_header_t* prev_free;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct control_t
            {
                public block_header_t block_null;
                public uint fl_bitmap;
                public fixed uint sl_bitmap[FL_INDEX_COUNT];
                public fixed uint blocks[FL_INDEX_COUNT * SL_INDEX_COUNT];
            }
        }
    }
}