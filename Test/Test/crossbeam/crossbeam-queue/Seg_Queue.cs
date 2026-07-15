using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;
using static crossbeam.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace crossbeam
{
    /// <summary>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </summary>
    public static unsafe class Seg_Queue
    {
        // Bits indicating the state of a slot:
        // * If a value has been written into the slot, `WRITE` is set.
        // * If a value has been read from the slot, `READ` is set.
        // * If the block is being destroyed, `DESTROY` is set.

        public const nuint WRITE = 1;
        public const nuint READ = 2;
        public const nuint DESTROY = 4;

        // Each block covers one "lap" of indices.
        public const nuint LAP = 32;

        // The maximum number of values a block can hold.
        public const nuint BLOCK_CAP = LAP - 1;

        // How many lower bits are reserved for metadata.
        public const nuint SHIFT = 1;

        // Indicates that the block is not the last one.
        public const nuint HAS_NEXT = 1;

        /// A slot in a block.
        public struct Slot<T> where T : unmanaged
        {
            /// The value.
            public T value;

            /// The state of the slot.
            public UnsafeAtomicUsize state;

            /// Waits until a value is written into the slot.
            public void wait_write()
            {
                var backoff = new UnsafeSpinWait();
                while ((this.state.load(Ordering.Acquire) & WRITE) == 0)
                {
                    backoff.SpinOnce();
                }
            }
        }

        [InlineArray((int)BLOCK_CAP)]
        public struct Slots<T> where T : unmanaged
        {
            public Slot<T> slot;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Slot<T>* get_unchecked(nuint index) => (Slot<T>*)Unsafe.AsPointer(ref Unsafe.Add(ref slot, index));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Slot<T>* get_unchecked_mut(nuint index) => get_unchecked(index);
        }

        /// A block in a linked list.
        /// <br />
        /// Each block in the list can hold up to `BLOCK_CAP` values.
        public struct Block<T> where T : unmanaged
        {
            /// The next block in the linked list.
            public UnsafeAtomicPtr<Block<T>> next;

            /// Slots for values.
            public Slots<T> slots;

            /// Waits until the next pointer is set.
            public Block<T>* wait_next()
            {
                var backoff = new UnsafeSpinWait();
                while (true)
                {
                    var next = this.next.load(Ordering.Acquire);
                    if (next != null)
                    {
                        return next;
                    }

                    backoff.SpinOnce();
                }
            }

            /// Sets the `DESTROY` bit in slots starting from `start` and destroys the block.
            public static void destroy(Block<T>* block, nuint start)
            {
                // It is not necessary to set the `DESTROY` bit in the last slot because that slot has
                // begun destruction of the block.
                for (nuint i = start; i < BLOCK_CAP - 1; ++i)
                {
                    var slot = block->slots.get_unchecked(i);

                    // Mark the `DESTROY` bit if a thread is still using the slot.
                    if ((slot->state.load(Ordering.Acquire) & READ) == 0
                        && (slot->state.Or(DESTROY) & READ) == 0)
                    {
                        // If a thread is still using the slot, it will continue destruction of the block.
                        return;
                    }
                }

                // No thread is using the block, now it is safe to destroy it.
                NativeMemoryAllocator.AlignedFree(block);
            }
        }

        /// A position in a queue.
        [StructLayout(LayoutKind.Sequential, Size = 2 * CACHE_LINE_SIZE)]
        public struct CachePaddedPosition<T> where T : unmanaged
        {
            private CachePadding padding;

            /// The index in the queue.
            public UnsafeAtomicUsize index;

            /// The block in the linked list.
            public UnsafeAtomicPtr<Block<T>> block;
        }

        /// An unbounded multi-producer multi-consumer queue.
        /// <br />
        /// This queue is implemented as a linked list of segments, where each segment is a small buffer
        /// that can hold a handful of elements. There is no limit to how many elements can be in the queue
        /// at a time. However, since segments need to be dynamically allocated as elements get pushed,
        public struct SegQueue<T> where T : unmanaged
        {
            /// The head of the queue.
            public CachePaddedPosition<T> head;

            /// The tail of the queue.
            public CachePaddedPosition<T> tail;

            /// Pushes back an element to the tail.
            public void push(T value)
            {
                var backoff = new UnsafeSpinWait();
                var tail = this.tail.index.load(Ordering.Acquire);
                var block = this.tail.block.load(Ordering.Acquire);
                Block<T>* next_block = null;

                try
                {
                    while (true)
                    {
                        // Calculate the offset of the index into the block.
                        var offset = (tail >> (int)SHIFT) % LAP;

                        // If we reached the end of the block, wait until the next one is installed.
                        if (offset == BLOCK_CAP)
                        {
                            backoff.SpinOnce();
                            tail = this.tail.index.load(Ordering.Acquire);
                            block = this.tail.block.load(Ordering.Acquire);
                            continue;
                        }

                        // If we're going to have to install the next block, allocate it in advance in order to
                        // make the wait for other threads as short as possible.
                        if (offset + 1 == BLOCK_CAP && next_block == null)
                        {
                            next_block = NativeMemoryAllocator.AlignedAllocZeroed<Block<T>>(1);
                        }

                        // If this is the first push operation, we need to allocate the first block.
                        if (block == null)
                        {
                            var @new = NativeMemoryAllocator.AlignedAllocZeroed<Block<T>>(1);

                            if (this
                                    .tail
                                    .block
                                    .CompareExchange(@new, block)
                                == block)
                            {
                                this.head.block.store(@new, Ordering.Release);
                                block = @new;
                            }
                            else
                            {
                                next_block = @new;
                                tail = this.tail.index.load(Ordering.Acquire);
                                block = this.tail.block.load(Ordering.Acquire);
                                continue;
                            }
                        }

                        var new_tail = tail + (1 << (int)SHIFT);

                        // Try advancing the tail forward.
                        var t = this.tail.index.CompareExchange(
                            new_tail,
                            tail
                        );

                        if (t == tail)
                        {
                            // If we've reached the end of the block, install the next one.
                            if (offset + 1 == BLOCK_CAP)
                            {
                                var next_block_unwarp = next_block;
                                next_block = null;

                                var next_index = new_tail.wrapping_add(1 << (int)SHIFT);

                                this.tail.block.store(next_block_unwarp, Ordering.Release);
                                this.tail.index.store(next_index, Ordering.Release);
                                block->next.store(next_block_unwarp, Ordering.Release);
                            }

                            // Write the value into the slot.
                            var slot = block->slots.get_unchecked(offset);
                            slot->value = value;
                            slot->state.Or(WRITE);

                            return;
                        }
                        else
                        {
                            tail = t;
                            block = this.tail.block.load(Ordering.Acquire);
                            backoff.SpinOnce(-1);
                        }
                    }
                }
                finally
                {
                    if (next_block != null)
                        NativeMemoryAllocator.AlignedFree(next_block);
                }
            }

            /// Pushes an element to the queue with exclusive mutable access.
            /// <br />
            /// Avoids atomic operations and synchronization, assuming
            /// no other threads access the queue concurrently.
            public void push_mut(T value)
            {
                var tail = this.tail.index.AsRef();
                var block = this.tail.block.AsRef();

                // Calculate the offset of the index into the block.
                var offset = (tail >> (int)SHIFT) % LAP;

                // If this is the first push operation, we need to allocate the first block.
                if (block == null)
                {
                    var @new = NativeMemoryAllocator.AlignedAllocZeroed<Block<T>>(1);
                    this.head.block.AsRef() = @new;
                    this.tail.block.AsRef() = @new;

                    block = @new;
                }

                var new_tail = tail + (1 << (int)SHIFT);

                this.tail.index.AsRef() = new_tail;

                unsafe
                {
                    // If we've reached the end of the block, install the next one.
                    if (offset + 1 == BLOCK_CAP)
                    {
                        var next_block = NativeMemoryAllocator.AlignedAllocZeroed<Block<T>>(1);
                        var next_index = new_tail.wrapping_add(1 << (int)SHIFT);

                        this.tail.block.AsRef() = next_block;
                        this.tail.index.AsRef() = next_index;
                        block->next.AsRef() = next_block;
                    }

                    // Write the value into the slot.
                    var slot = block->slots.get_unchecked(offset);
                    slot->value = value;
                    block->slots.get_unchecked_mut(offset)->state.AsRef() |= WRITE;
                }
            }

            /// Pops the head element from the queue.
            public bool pop(out T result)
            {
                var backoff = new UnsafeSpinWait();
                var head = this.head.index.load(Ordering.Acquire);
                var block = this.head.block.load(Ordering.Acquire);

                while (true)
                {
                    // Calculate the offset of the index into the block.
                    var offset = (head >> (int)SHIFT) % LAP;

                    // If we reached the end of the block, wait until the next one is installed.
                    if (offset == BLOCK_CAP)
                    {
                        backoff.SpinOnce();
                        head = this.head.index.load(Ordering.Acquire);
                        block = this.head.block.load(Ordering.Acquire);
                        continue;
                    }

                    var new_head = head + (1 << (int)SHIFT);

                    if ((new_head & HAS_NEXT) == 0)
                    {
                        Interlocked.MemoryBarrier();
                        var tail = this.tail.index.load(Ordering.Relaxed);

                        // If the tail equals the head, that means the queue is empty.
                        if ((head >> (int)SHIFT) == (tail >> (int)SHIFT))
                        {
                            result = default;
                            return false;
                        }

                        // If head and tail are not in the same block, set `HAS_NEXT` in head.
                        if ((head >> (int)SHIFT) / LAP != (tail >> (int)SHIFT) / LAP)
                        {
                            new_head |= HAS_NEXT;
                        }
                    }

                    // The block can be null here only if the first push operation is in progress. In that
                    // case, just wait until it gets initialized.
                    if (block == null)
                    {
                        backoff.SpinOnce();
                        head = this.head.index.load(Ordering.Acquire);
                        block = this.head.block.load(Ordering.Acquire);
                        continue;
                    }

                    // Try moving the head index forward.
                    var h = this.head.index.CompareExchange(
                        new_head,
                        head
                    );

                    if (h == head)
                    {
                        // If we've reached the end of the block, move to the next one.
                        if (offset + 1 == BLOCK_CAP)
                        {
                            var next = block->wait_next();
                            var next_index = (new_head & ~HAS_NEXT) + (1 << (int)SHIFT);
                            if (next->next.load(Ordering.Relaxed) != null)
                            {
                                next_index |= HAS_NEXT;
                            }

                            this.head.block.store(next, Ordering.Release);
                            this.head.index.store(next_index, Ordering.Release);
                        }

                        // Read the value.
                        var slot = block->slots.get_unchecked(offset);
                        slot->wait_write();
                        var value = slot->value;

                        // Destroy the block if we've reached the end, or if another thread wanted to
                        // destroy but couldn't because we were busy reading from the slot.
                        if (offset + 1 == BLOCK_CAP)
                        {
                            Block<T>.destroy(block, 0);
                        }
                        else if ((slot->state.Or(READ) & DESTROY) != 0)
                        {
                            Block<T>.destroy(block, offset + 1);
                        }

                        result = value;
                        return true;
                    }
                    else
                    {
                        head = h;
                        block = this.head.block.load(Ordering.Acquire);
                        backoff.SpinOnce(-1);
                    }
                }
            }

            /// Pops the head element from the queue using an exclusive reference.
            /// <br />
            /// Avoids atomic operations and synchronization, assuming
            /// no other threads access the queue concurrently.
            public bool pop_mut(out T result)
            {
                var head = this.head.index.AsRef();
                var block = this.head.block.AsRef();

                // Calculate the offset of the index into the block.
                var offset = (head >> (int)SHIFT) % LAP;

                var new_head = head + (1 << (int)SHIFT);

                if ((new_head & HAS_NEXT) == 0)
                {
                    var tail = this.tail.index.AsRef();

                    // If the tail equals the head, that means the queue is empty.
                    if (head >> (int)SHIFT == tail >> (int)SHIFT)
                    {
                        result = default;
                        return false;
                    }

                    // If head and tail are not in the same block, set `HAS_NEXT` in head.
                    if ((head >> (int)SHIFT) / LAP != (tail >> (int)SHIFT) / LAP)
                    {
                        new_head |= HAS_NEXT;
                    }
                }

                this.head.index.AsRef() = new_head;

                unsafe
                {
                    // If we've reached the end of the block, move to the next one.
                    if (offset + 1 == BLOCK_CAP)
                    {
                        var next = block->next.AsRef();
                        var next_index = (new_head & ~HAS_NEXT).wrapping_add(1 << (int)SHIFT);
                        if (next->next.AsRef() != null)
                        {
                            next_index |= HAS_NEXT;
                        }

                        this.head.block.AsRef() = next;
                        this.head.index.AsRef() = next_index;
                    }

                    // Read the value.
                    var slot = block->slots.get_unchecked(offset);
                    var value = slot->value;

                    // Destroy the block if we've reached the end
                    if (offset + 1 == BLOCK_CAP)
                    {
                        NativeMemoryAllocator.AlignedFree(block);
                    }
                    else
                    {
                        var state = block->slots.get_unchecked_mut(offset)->state.AsRef();
                        block->slots.get_unchecked_mut(offset)->state.AsRef() = state | READ;
                        if ((state & DESTROY) != 0)
                        {
                            Block<T>.destroy(block, offset + 1);
                        }
                    }

                    result = value;
                    return true;
                }
            }

            /// Returns `true` if the queue is empty.
            public bool is_empty()
            {
                var head = this.head.index.load(Ordering.SeqCst);
                var tail = this.tail.index.load(Ordering.SeqCst);
                return head >> (int)SHIFT == tail >> (int)SHIFT;
            }

            /// Returns the number of elements in the queue.
            public nuint len()
            {
                while (true)
                {
                    // Load the tail index, then load the head index.
                    var tail = this.tail.index.load(Ordering.SeqCst);
                    var head = this.head.index.load(Ordering.SeqCst);

                    // If the tail index didn't change, we've got consistent indices to work with.
                    if (this.tail.index.load(Ordering.SeqCst) == tail)
                    {
                        // Erase the lower bits.
                        tail &= unchecked((nuint)~((1 << (int)SHIFT) - 1));
                        head &= unchecked((nuint)~((1 << (int)SHIFT) - 1));

                        // Fix up indices if they fall onto block ends.
                        if (((tail >> (int)SHIFT) & (LAP - 1)) == LAP - 1)
                        {
                            tail = tail.wrapping_add(1 << (int)SHIFT);
                        }

                        if (((head >> (int)SHIFT) & (LAP - 1)) == LAP - 1)
                        {
                            head = head.wrapping_add(1 << (int)SHIFT);
                        }

                        // Rotate indices so that head falls into the first block.
                        var lap = (head >> (int)SHIFT) / LAP;
                        tail = tail.wrapping_sub((lap * LAP) << (int)SHIFT);
                        head = head.wrapping_sub((lap * LAP) << (int)SHIFT);

                        // Remove the lower bits.
                        tail >>= (int)SHIFT;
                        head >>= (int)SHIFT;

                        // Return the difference minus the number of blocks between tail and head.
                        return tail - head - tail / LAP;
                    }
                }
            }

            public void drop()
            {
                ref var head = ref this.head.index.AsRef();
                ref var tail = ref this.tail.index.AsRef();
                ref var block = ref this.head.block.AsRef();

                // Erase the lower bits.
                head &= unchecked((nuint)~((1 << (int)SHIFT) - 1));
                tail &= unchecked((nuint)~((1 << (int)SHIFT) - 1));

                unsafe
                {
                    // Drop all values between `head` and `tail` and deallocate the heap-allocated blocks.
                    while (head != tail)
                    {
                        var offset = (head >> (int)SHIFT) % LAP;

                        if (offset < BLOCK_CAP)
                        {
                            // Drop the value in the slot.
                            // var slot = *block->slots.get_unchecked(offset);
                        }
                        else
                        {
                            // Deallocate the block and move to the next one.
                            var next = block->next.AsRef();
                            NativeMemoryAllocator.AlignedFree(block);
                            block = next;
                        }

                        head = head.wrapping_add(1 << (int)SHIFT);
                    }

                    // Deallocate the last remaining block.
                    if (block != null)
                    {
                        NativeMemoryAllocator.AlignedFree(block);
                    }
                }
            }
        }
    }
}