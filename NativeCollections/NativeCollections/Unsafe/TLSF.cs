using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS1591
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Two-Level Segregated Fit memory allocator 32
    ///     https://github.com/mattconte/tlsf
    /// </summary>
    internal static class TLSF
    {
        public static unsafe class TLSF32
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
            public const uint block_size_min = 16 - sizeof(uint);
            public const uint block_size_max = (uint)1 << FL_INDEX_MAX;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void memcpy(void* dst, void* src, uint size) => Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(dst), ref Unsafe.AsRef<byte>(src), size);

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
            public static uint tlsf_min(uint a, uint b) => Math.Min(a, b);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint tlsf_max(uint a, uint b) => Math.Max(a, b);

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
            public static block_header_t* block_from_ptr(void* ptr) => UnsafeHelpers.SubtractByteOffset<block_header_t>(ptr, (nint)block_start_offset);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_to_ptr(block_header_t* block) => UnsafeHelpers.AddByteOffset(block, (nint)block_start_offset);

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

                Unsafe.AsRef<int>(fli) = fl;
                Unsafe.AsRef<int>(sli) = sl;
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
                var fl = Unsafe.AsRef<int>(fli);
                var sl = Unsafe.AsRef<int>(sli);
                var sl_map = Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) & (~0U << sl);
                if (!(sl_map != 0))
                {
                    var fl_map = control->fl_bitmap & (~0U << (fl + 1));
                    if (!(fl_map != 0))
                        return null;
                    fl = tlsf_ffs(fl_map);
                    Unsafe.AsRef<int>(fli) = fl;
                    sl_map = Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl);
                }

                sl = tlsf_ffs(sl_map);
                Unsafe.AsRef<int>(sli) = sl;
                return Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void remove_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var prev = block->prev_free;
                var next = block->next_free;
                next->prev_free = prev;
                prev->next_free = next;
                if (Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) == block)
                {
                    Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) = next;
                    if (next == &control->block_null)
                    {
                        Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) &= ~(1U << sl);
                        if (!(Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) != 0))
                            control->fl_bitmap &= ~(1U << fl);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void insert_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var current = (block_header_t*)Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl);
                block->next_free = current;
                block->prev_free = &control->block_null;
                current->prev_free = block;
                Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) = block;
                control->fl_bitmap |= 1U << fl;
                Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) |= 1U << sl;
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
                    Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)i) = 0;
                    for (j = 0; j < SL_INDEX_COUNT; ++j)
                        Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[i]), (nint)j) = &control->block_null;
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
                const uint size = 4294967292U;
                block = offset_to_block(mem, size);
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
                tlsf_add_pool(tlsf, UnsafeHelpers.AddByteOffset(mem, (nint)tlsf_size()), bytes - tlsf_size());
                return tlsf;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_get_pool(void* tlsf) => UnsafeHelpers.AddByteOffset(tlsf, (nint)tlsf_size());

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
                public blocks_t blocks;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct blocks_t
            {
                public fixed uint headers[FL_INDEX_COUNT * SL_INDEX_COUNT];

                public block_header_t_ptr* this[int i]
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => UnsafeHelpers.Add(ref Unsafe.As<blocks_t, block_header_t_ptr>(ref this), i * SL_INDEX_COUNT);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct block_header_t_ptr
            {
                public block_header_t* value;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private block_header_t_ptr(block_header_t* value) => this.value = value;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static implicit operator block_header_t_ptr(block_header_t* ptr) => new(ptr);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static implicit operator block_header_t*(block_header_t_ptr ptr) => ptr.value;
            }
        }

        public static unsafe class TLSF64
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
            public const ulong block_size_min = 32 - sizeof(ulong);
            public const ulong block_size_max = (ulong)1 << FL_INDEX_MAX;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void memcpy(void* dst, void* src, ulong size) => Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(dst), ref Unsafe.AsRef<byte>(src), (uint)size);

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
            public static ulong tlsf_min(ulong a, ulong b) => Math.Min(a, b);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong tlsf_max(ulong a, ulong b) => Math.Max(a, b);

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
            public static block_header_t* block_from_ptr(void* ptr) => UnsafeHelpers.SubtractByteOffset<block_header_t>(ptr, (nint)block_start_offset);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* block_to_ptr(block_header_t* block) => UnsafeHelpers.AddByteOffset(block, (nint)block_start_offset);

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

                Unsafe.AsRef<int>(fli) = fl;
                Unsafe.AsRef<int>(sli) = sl;
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
                var fl = Unsafe.AsRef<int>(fli);
                var sl = Unsafe.AsRef<int>(sli);
                var sl_map = Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) & (~0U << sl);
                if (!(sl_map != 0))
                {
                    var fl_map = control->fl_bitmap & (~0U << (fl + 1));
                    if (!(fl_map != 0))
                        return null;
                    fl = tlsf_ffs(fl_map);
                    Unsafe.AsRef<int>(fli) = fl;
                    sl_map = Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl);
                }

                sl = tlsf_ffs(sl_map);
                Unsafe.AsRef<int>(sli) = sl;
                return Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void remove_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var prev = block->prev_free;
                var next = block->next_free;
                next->prev_free = prev;
                prev->next_free = next;
                if (Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) == block)
                {
                    Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) = next;
                    if (next == &control->block_null)
                    {
                        Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) &= ~(1U << sl);
                        if (!(Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) != 0))
                            control->fl_bitmap &= ~(1U << fl);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void insert_free_block(control_t* control, block_header_t* block, int fl, int sl)
            {
                var current = (block_header_t*)Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl);
                block->next_free = current;
                block->prev_free = &control->block_null;
                current->prev_free = block;
                Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[fl]), (nint)sl) = block;
                control->fl_bitmap |= 1U << fl;
                Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)fl) |= 1U << sl;
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
                    Unsafe.Add(ref Unsafe.AsRef<uint>(control->sl_bitmap), (nint)i) = 0;
                    for (j = 0; j < SL_INDEX_COUNT; ++j)
                        Unsafe.Add(ref Unsafe.AsRef<block_header_t_ptr>(control->blocks[i]), (nint)j) = &control->block_null;
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
                const ulong size = 18446744073709551608UL;
                block = offset_to_block(mem, size);
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
                tlsf_add_pool(tlsf, UnsafeHelpers.AddByteOffset(mem, (nint)tlsf_size()), bytes - tlsf_size());
                return tlsf;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void* tlsf_get_pool(void* tlsf) => UnsafeHelpers.AddByteOffset(tlsf, (nint)tlsf_size());

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
                public blocks_t blocks;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct blocks_t
            {
                public fixed ulong headers[FL_INDEX_COUNT * SL_INDEX_COUNT];

                public block_header_t_ptr* this[int i]
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => UnsafeHelpers.Add(ref Unsafe.As<blocks_t, block_header_t_ptr>(ref this), i * SL_INDEX_COUNT);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct block_header_t_ptr
            {
                public block_header_t* value;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private block_header_t_ptr(block_header_t* value) => this.value = value;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static implicit operator block_header_t_ptr(block_header_t* ptr) => new(ptr);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static implicit operator block_header_t*(block_header_t_ptr ptr) => ptr.value;
            }
        }
    }
}