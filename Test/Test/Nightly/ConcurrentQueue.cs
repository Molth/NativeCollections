using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace Examples
{
    /// <summary>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </summary>
    public static unsafe class ConcurrentQueue
    {
        public const int CACHE_LINE_SIZE = 128;

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

        public struct Slot<T> where T : unmanaged
        {
            /// The value.
            public T value;

            /// The state of the slot.
            public UnsafeAtomicUIntPtr state;

            /// Waits until a value is written into the slot.
            public void wait_write()
            {
                var backoff = new UnsafeSpinWait();
                while ((this.state.Read() & WRITE) == 0)
                {
                    backoff.SpinOnce();
                }
            }
        }

        [InlineArray((int)BLOCK_CAP)]
        public struct Slots<T> where T : unmanaged
        {
            public Slot<T> slot;

            public Slot<T>* get_unchecked(nuint index) => (Slot<T>*)Unsafe.AsPointer(ref Unsafe.Add(ref slot, index));
        }

        /// A block in a linked list.
        /// 
        /// Each block in the list can hold up to `BLOCK_CAP` values.
        public struct Block<T> where T : unmanaged
        {
            /// The next block in the linked list.
            public UnsafeAtomicReference<Block<T>> next;

            /// Slots for values.
            public Slots<T> slots;

            /// Waits until the next pointer is set.
            public Block<T>* wait_next()
            {
                var backoff = new UnsafeSpinWait();
                while (true)
                {
                    var next = this.next.Read();
                    if (next != null)
                    {
                        return next;
                    }

                    backoff.SpinOnce();
                }
            }

            public static void destroy(Block<T>* block)
            {
                // No thread is using the block, now it is safe to destroy it.
                NativeMemoryAllocator.AlignedFree(block);
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
                    if ((slot->state.Read() & READ) == 0
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

        [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
        public struct CachePaddedPosition
        {
            /// The index in the queue.
            [FieldOffset(1 * CACHE_LINE_SIZE)] public nuint index;

            /// The block in the linked list.
            [FieldOffset(2 * CACHE_LINE_SIZE)] public nuint block;
        }

        /// A position in a queue.
        public struct CachePaddedPosition<T> where T : unmanaged
        {
            public CachePaddedPosition data;

            public ref UnsafeAtomicUIntPtr index => ref Unsafe.As<nuint, UnsafeAtomicUIntPtr>(ref data.index);

            public ref UnsafeAtomicReference<Block<T>> block => ref Unsafe.As<nuint, UnsafeAtomicReference<Block<T>>>(ref data.block);
        }

        public struct SegQueue<T> where T : unmanaged
        {
            /// The head of the queue.
            public CachePaddedPosition<T> head;

            /// The tail of the queue.
            public CachePaddedPosition<T> tail;

            public void push(T value)
            {
                var backoff = new UnsafeSpinWait();
                var tail = this.tail.index.Read();
                var block = this.tail.block.Read();
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
                            tail = this.tail.index.Read();
                            block = this.tail.block.Read();
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
                                this.head.block.Exchange(@new);
                                block = @new;
                            }
                            else
                            {
                                next_block = @new;
                                tail = this.tail.index.Read();
                                block = this.tail.block.Read();
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
                                // Debug.Assert(next_block != null);
                                var next_index = unchecked(new_tail + (1 << (int)SHIFT));

                                this.tail.block.Exchange(next_block);
                                this.tail.index.Exchange(next_index);
                                block->next.Exchange(next_block);

                                next_block = null;
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
                            block = this.tail.block.Read();
                            backoff.SpinOnce(-1);
                        }
                    }
                }
                finally
                {
                    if (next_block != null)
                        Block<T>.destroy(next_block);
                }
            }

            public bool pop(out T result)
            {
                var backoff = new UnsafeSpinWait();
                var head = this.head.index.Read();
                var block = this.head.block.Read();

                while (true)
                {
                    // Calculate the offset of the index into the block.
                    var offset = (head >> (int)SHIFT) % LAP;

                    // If we reached the end of the block, wait until the next one is installed.
                    if (offset == BLOCK_CAP)
                    {
                        backoff.SpinOnce();
                        head = this.head.index.Read();
                        block = this.head.block.Read();
                        continue;
                    }

                    var new_head = head + (1 << (int)SHIFT);

                    if ((new_head & HAS_NEXT) == 0)
                    {
                        Interlocked.MemoryBarrier();
                        var tail = this.tail.index.Read();

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
                        head = this.head.index.Read();
                        block = this.head.block.Read();
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
                            if (next->next.Read() != null)
                            {
                                next_index |= HAS_NEXT;
                            }

                            this.head.block.Exchange(next);
                            this.head.index.Exchange(next_index);
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
                        block = this.head.block.Read();
                        backoff.SpinOnce(-1);
                    }
                }
            }

            /// Returns `true` if the queue is empty.
            public bool is_empty()
            {
                var head = this.head.index.Read();
                var tail = this.tail.index.Read();
                return head >> (int)SHIFT == tail >> (int)SHIFT;
            }

            /// Returns the number of elements in the queue.
            public nuint len()
            {
                while (true)
                {
                    // Load the tail index, then load the head index.
                    var tail = this.tail.index.Read();
                    var head = this.head.index.Read();

                    // If the tail index didn't change, we've got consistent indices to work with.
                    if (this.tail.index.Read() == tail)
                    {
                        // Erase the lower bits.
                        tail &= unchecked((nuint)~((1 << (int)SHIFT) - 1));
                        head &= unchecked((nuint)~((1 << (int)SHIFT) - 1));

                        // Fix up indices if they fall onto block ends.
                        if (((tail >> (int)SHIFT) & (LAP - 1)) == LAP - 1)
                        {
                            tail = unchecked(tail + (1 << (int)SHIFT));
                        }

                        if (((head >> (int)SHIFT) & (LAP - 1)) == LAP - 1)
                        {
                            head = unchecked(head + (1 << (int)SHIFT));
                        }

                        // Rotate indices so that head falls into the first block.
                        var lap = (head >> (int)SHIFT) / LAP;
                        tail = unchecked(tail + ((lap * LAP) << (int)SHIFT));
                        head = unchecked(head + ((lap * LAP) << (int)SHIFT));

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
                            // var slot = *((Block<T>*)block)->slots.get_unchecked(offset);
                        }
                        else
                        {
                            // Deallocate the block and move to the next one.
                            var next = ((Block<T>*)block)->next.AsRef();
                            Block<T>.destroy((Block<T>*)block);
                            block = next;
                        }

                        head = unchecked(head + (1 << (int)SHIFT));
                    }

                    // Deallocate the last remaining block.
                    if (block != 0)
                    {
                        Block<T>.destroy((Block<T>*)block);
                    }
                }
            }
        }
    }
}