using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An unbounded multi-producer multi-consumer queue.
    /// </summary>
    /// <remarks>
    ///     The queue is implemented as a linked list of segments, where each segment is a small buffer that
    ///     can hold a handful of elements. There is no limit to how many elements can be in the queue at a
    ///     time. Segments are allocated as elements are added and are released when they are no longer needed.
    ///     <para>
    ///         The implementation is based on <see href="https://github.com/crossbeam-rs/crossbeam">Crossbeam</see>.
    ///     </para>
    /// </remarks>
    internal static unsafe class SegQueue
    {
        /// <summary>
        ///     Indicates that a value has been written into a slot.
        /// </summary>
        private const nuint WRITE = 1;

        /// <summary>
        ///     Indicates that a value has been read from a slot.
        /// </summary>
        private const nuint READ = 2;

        /// <summary>
        ///     Indicates that a block is being destroyed.
        /// </summary>
        private const nuint DESTROY = 4;

        /// <summary>
        ///     The number of indices covered by one block.
        /// </summary>
        private const nuint LAP = 32;

        /// <summary>
        ///     The maximum number of values a block can hold.
        /// </summary>
        private const nuint BLOCK_CAP = LAP - 1;

        /// <summary>
        ///     The number of lower index bits reserved for metadata.
        /// </summary>
        private const nuint SHIFT = 1;

        /// <summary>
        ///     Indicates that the block is not the last block in the queue.
        /// </summary>
        private const nuint HAS_NEXT = 1;

        /// <summary>
        ///     A slot in a block.
        /// </summary>
        /// <typeparam name="T">The type of the value stored in the slot.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        private struct Slot<T> where T : unmanaged
        {
            /// <summary>
            ///     The value.
            /// </summary>
            public T Value;

            /// <summary>
            ///     The state of the slot.
            /// </summary>
            public UnsafeAtomicUsize State;

            /// <summary>
            ///     Waits until a value is written into the slot.
            /// </summary>
            public void WaitWrite()
            {
                Backoff backoff = default;
                while ((State.Load(Ordering.Acquire) & WRITE) == Usize.FromI32Unchecked(0))
                    backoff.Snooze();
            }
        }

        /// <summary>
        ///     Stores the fixed number <see cref="BLOCK_CAP" /> of slots contained in a block.
        /// </summary>
        /// <typeparam name="T">The type of values stored in the slots.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        private struct Slots<T> where T : unmanaged
        {
            /// <summary>
            ///     The numeric suffix of each field is its zero-based slot index.
            /// </summary>
            private Slot<T>
                _slot00,
                _slot01,
                _slot02,
                _slot03,
                _slot04,
                _slot05,
                _slot06,
                _slot07,
                _slot08,
                _slot09,
                _slot10,
                _slot11,
                _slot12,
                _slot13,
                _slot14,
                _slot15,
                _slot16,
                _slot17,
                _slot18,
                _slot19,
                _slot20,
                _slot21,
                _slot22,
                _slot23,
                _slot24,
                _slot25,
                _slot26,
                _slot27,
                _slot28,
                _slot29,
                _slot30;

            /// <summary>
            ///     Returns a pointer to the slot at the specified index without performing bounds checks.
            /// </summary>
            /// <param name="index">The zero-based slot index.</param>
            /// <returns>A pointer to the slot at <paramref name="index" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Slot<T>* GetUnchecked(nuint index) => UnsafeHelpers.Add(ref _slot00, Isize.FromUsizeUnchecked(index));
        }

        /// <summary>
        ///     A block in a linked list.
        /// </summary>
        /// <typeparam name="T">The type of values stored in the block.</typeparam>
        /// <remarks>
        ///     Each block in the list can hold up to <see cref="BLOCK_CAP" /> values.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct Block<T> where T : unmanaged
        {
            /// <summary>
            ///     Creates an empty block.
            /// </summary>
            /// <returns>A pointer to the allocated block.</returns>
            public static Block<T>* AllocateZeroed() => NativeMemoryAllocator.AlignedAllocZeroed<Block<T>>(Usize.FromI32Unchecked(1));

            /// <summary>
            ///     Releases an allocated block.
            /// </summary>
            /// <param name="block">The block to release.</param>
            public static void Drop(Block<T>* block) => NativeMemoryAllocator.AlignedFree(block);

            /// <summary>
            ///     The next block in the linked list.
            /// </summary>
            /// <remarks>
            ///     This field must stay a plain atomic word and must only be reached through the <see cref="Next" />
            ///     property. Declaring it as <c>UnsafeAtomicPtr&lt;Block&lt;T&gt;&gt;</c> would make the layout of the
            ///     generic type depend on its own instantiation, and <see cref="Block{T}" /> then fails to load with
            ///     a <see cref="TypeLoadException" /> on some target frameworks (net5.0 / net6.0), which takes the
            ///     whole <see cref="SegQueue" /> down. Storing one non-generic <see cref="UnsafeAtomicIsize" /> here
            ///     keeps a single, identical memory layout on all target frameworks. Do not write it any other way.
            /// </remarks>
            private UnsafeAtomicIsize _next;

            /// <summary>
            ///     The next block in the linked list.
            /// </summary>
            /// <remarks>
            ///     Must be written exactly like this: reinterpret the storage by reference instead of declaring a field
            ///     of type <c>UnsafeAtomicPtr&lt;Block&lt;T&gt;&gt;</c>. A generic type that contains a field typed
            ///     with its own instantiation cannot be loaded on net5.0 / net6.0 (<see cref="TypeLoadException" />
            ///     when the type is loaded), so every access goes through this property and the queue stays
            ///     usable there. The reinterpretation is layout compatible: <see cref="UnsafeAtomicPtr{T}" />
            ///     wraps exactly one <see cref="UnsafeAtomicIsize" /> (a single <see cref="IntPtr" />),
            ///     so size and alignment are identical. Do not "simplify" this back into a direct field.
            /// </remarks>
            public ref UnsafeAtomicPtr<Block<T>> Next => ref Unsafe.As<UnsafeAtomicIsize, UnsafeAtomicPtr<Block<T>>>(ref _next);

            /// <summary>
            ///     Slots for values.
            /// </summary>
            public Slots<T> Slots;

            /// <summary>
            ///     Waits until the next pointer is set.
            /// </summary>
            /// <returns>The next block in the linked list.</returns>
            public Block<T>* WaitNext()
            {
                Backoff backoff = default;
                while (true)
                {
                    var next = Next.Load(Ordering.Acquire);
                    if (next != null)
                        return next;

                    backoff.Snooze();
                }
            }

            /// <summary>
            ///     Sets the <see cref="DESTROY" /> bit in slots starting from
            ///     <paramref name="start" /> and destroys the block when all slots have been read.
            /// </summary>
            /// <param name="block">The block to inspect and destroy.</param>
            /// <param name="start">The first slot to inspect.</param>
            public static void Destroy(Block<T>* block, nuint start)
            {
                for (var i = start; i < BLOCK_CAP.WrappingSub(Usize.FromI32Unchecked(1)); i = i.WrappingAdd(Usize.FromI32Unchecked(1)))
                {
                    var slot = block->Slots.GetUnchecked(i);
                    if ((slot->State.Load(Ordering.Acquire) & READ) == Usize.FromI32Unchecked(0) && (slot->State.Or(DESTROY) & READ) == Usize.FromI32Unchecked(0))
                        return;
                }

                Drop(block);
            }
        }

        /// <summary>
        ///     A position in a queue.
        /// </summary>
        /// <typeparam name="T">The type of values stored in the queue.</typeparam>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        private struct CachePaddedPosition<T> where T : unmanaged
        {
            /// <summary>
            ///     The index in the queue.
            /// </summary>
            public UnsafeAtomicUsize Index;

            /// <summary>
            ///     The block in the linked list.
            /// </summary>
            public UnsafeAtomicPtr<Block<T>> Block;
        }

        /// <summary>
        ///     An unbounded multi-producer multi-consumer queue.
        /// </summary>
        /// <typeparam name="T">The type of elements stored in the queue.</typeparam>
        /// <remarks>
        ///     This queue is implemented as a linked list of segments, where each segment is a small buffer that
        ///     can hold a handful of elements. There is no limit to how many elements can be in the queue at a
        ///     time. Segments are allocated as elements are added and are released when they are no longer needed.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct RawSegQueue<T> : IDisposable where T : unmanaged
        {
            /// <summary>
            ///     The head of the queue.
            /// </summary>
            private CachePaddedPosition<T> _head;

            /// <summary>
            ///     The tail of the queue.
            /// </summary>
            private CachePaddedPosition<T> _tail;

            /// <summary>
            ///     Adds an element to the end of the queue.
            /// </summary>
            /// <param name="value">The element to add to the queue.</param>
            public void Enqueue(T value)
            {
                Backoff backoff = default;
                var tail = _tail.Index.Load(Ordering.Acquire);
                var block = _tail.Block.Load(Ordering.Acquire);
                Block<T>* nextBlock = null;
                try
                {
                    while (true)
                    {
                        var offset = (tail >> I32.FromUsizeUnchecked(SHIFT)) % LAP;
                        if (offset == BLOCK_CAP)
                        {
                            backoff.Snooze();

                            tail = _tail.Index.Load(Ordering.Acquire);
                            block = _tail.Block.Load(Ordering.Acquire);
                            continue;
                        }

                        if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP && nextBlock == null)
                            nextBlock = Block<T>.AllocateZeroed();

                        if (block == null)
                        {
                            var @new = Block<T>.AllocateZeroed();
                            if (_tail.Block.CompareExchange(@new, block) == block)
                            {
                                _head.Block.Store(@new, Ordering.Release);
                                block = @new;
                            }
                            else
                            {
                                nextBlock = @new;
                                tail = _tail.Index.Load(Ordering.Acquire);
                                block = _tail.Block.Load(Ordering.Acquire);
                                continue;
                            }
                        }

                        var newTail = tail.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));

                        var t = _tail.Index.CompareExchange(newTail, tail);
                        if (t == tail)
                        {
                            if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                            {
                                var nextBlockUnwrap = nextBlock;
                                nextBlock = null;
                                var nextIndex = newTail.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));

                                _tail.Block.Store(nextBlockUnwrap, Ordering.Release);
                                _tail.Index.Store(nextIndex, Ordering.Release);

                                block->Next.Store(nextBlockUnwrap, Ordering.Release);
                            }

                            var slot = block->Slots.GetUnchecked(offset);
                            slot->Value = value;
                            slot->State.Or(WRITE);
                            return;
                        }
                        else
                        {
                            tail = t;
                            block = _tail.Block.Load(Ordering.Acquire);

                            backoff.Spin();
                        }
                    }
                }
                finally
                {
                    if (nextBlock != null)
                        Block<T>.Drop(nextBlock);
                }
            }

            /// <summary>
            ///     Adds an element to the queue using exclusive mutable access.
            /// </summary>
            /// <param name="value">The element to add to the queue.</param>
            /// <remarks>
            ///     Avoids atomic operations and synchronization. The caller must
            ///     ensure that no other thread accesses the queue concurrently.
            /// </remarks>
            public void EnqueueMut(T value)
            {
                var tail = _tail.Index.AsRef();
                var block = _tail.Block.AsRef();
                var offset = (tail >> I32.FromUsizeUnchecked(SHIFT)) % LAP;
                if (block == null)
                {
                    var @new = Block<T>.AllocateZeroed();
                    _head.Block.AsRef() = @new;
                    _tail.Block.AsRef() = @new;
                    block = @new;
                }

                var newTail = tail.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                _tail.Index.AsRef() = newTail;
                if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                {
                    var nextBlock = Block<T>.AllocateZeroed();
                    var nextIndex = newTail.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                    _tail.Block.AsRef() = nextBlock;
                    _tail.Index.AsRef() = nextIndex;
                    block->Next.AsRef() = nextBlock;
                }

                var slot = block->Slots.GetUnchecked(offset);
                slot->Value = value;
                block->Slots.GetUnchecked(offset)->State.AsRef() |= WRITE;
            }

            /// <summary>
            ///     Attempts to remove an element from the beginning of the queue.
            /// </summary>
            /// <param name="result">
            ///     When this method returns <see langword="true" />, contains the removed
            ///     element; otherwise, contains the default value of <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if an element was removed;
            ///     otherwise, <see langword="false" /> if the queue is empty.
            /// </returns>
            public bool TryDequeue(out T result)
            {
                Backoff backoff = default;
                var head = _head.Index.Load(Ordering.Acquire);
                var block = _head.Block.Load(Ordering.Acquire);
                while (true)
                {
                    var offset = (head >> I32.FromUsizeUnchecked(SHIFT)) % LAP;
                    if (offset == BLOCK_CAP)
                    {
                        backoff.Snooze();
                        head = _head.Index.Load(Ordering.Acquire);
                        block = _head.Block.Load(Ordering.Acquire);
                        continue;
                    }

                    var newHead = head.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                    if ((newHead & HAS_NEXT) == Usize.FromI32Unchecked(0))
                    {
                        Interlocked.MemoryBarrier();

                        var tail = _tail.Index.Load(Ordering.Relaxed);
                        if (head >> I32.FromUsizeUnchecked(SHIFT) == tail >> I32.FromUsizeUnchecked(SHIFT))
                        {
                            result = default;
                            return false;
                        }

                        if ((head >> I32.FromUsizeUnchecked(SHIFT)) / LAP != (tail >> I32.FromUsizeUnchecked(SHIFT)) / LAP)
                            newHead |= HAS_NEXT;
                    }

                    if (block == null)
                    {
                        backoff.Snooze();
                        head = _head.Index.Load(Ordering.Acquire);
                        block = _head.Block.Load(Ordering.Acquire);
                        continue;
                    }

                    var h = _head.Index.CompareExchange(newHead, head);
                    if (h == head)
                    {
                        if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                        {
                            var next = block->WaitNext();
                            var nextIndex = (newHead & ~HAS_NEXT).WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                            if (next->Next.Load(Ordering.Relaxed) != null)
                                nextIndex |= HAS_NEXT;

                            _head.Block.Store(next, Ordering.Release);
                            _head.Index.Store(nextIndex, Ordering.Release);
                        }

                        var slot = block->Slots.GetUnchecked(offset);
                        slot->WaitWrite();

                        var value = slot->Value;

                        if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                            Block<T>.Destroy(block, Usize.FromI32Unchecked(0));
                        else if ((slot->State.Or(READ) & DESTROY) != Usize.FromI32Unchecked(0))
                            Block<T>.Destroy(block, offset.WrappingAdd(Usize.FromI32Unchecked(1)));

                        result = value;
                        return true;
                    }

                    head = h;
                    block = _head.Block.Load(Ordering.Acquire);
                    backoff.Spin();
                }
            }

            /// <summary>
            ///     Attempts to remove an element from the beginning of the queue using exclusive mutable access.
            /// </summary>
            /// <param name="result">
            ///     When this method returns <see langword="true" />, contains the removed
            ///     element; otherwise, contains the default value of <typeparamref name="T" />.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if an element was removed;
            ///     otherwise, <see langword="false" /> if the queue is empty.
            /// </returns>
            /// <remarks>
            ///     Avoids atomic operations and synchronization. The caller must
            ///     ensure that no other thread accesses the queue concurrently.
            /// </remarks>
            public bool TryDequeueMut(out T result)
            {
                var head = _head.Index.AsRef();
                var block = _head.Block.AsRef();
                var offset = (head >> I32.FromUsizeUnchecked(SHIFT)) % LAP;

                var newHead = head.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                if ((newHead & HAS_NEXT) == Usize.FromI32Unchecked(0))
                {
                    var tail = _tail.Index.AsRef();
                    if (head >> I32.FromUsizeUnchecked(SHIFT) == tail >> I32.FromUsizeUnchecked(SHIFT))
                    {
                        result = default;
                        return false;
                    }

                    if ((head >> I32.FromUsizeUnchecked(SHIFT)) / LAP != (tail >> I32.FromUsizeUnchecked(SHIFT)) / LAP)
                        newHead |= HAS_NEXT;
                }

                _head.Index.AsRef() = newHead;
                if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                {
                    var next = block->Next.AsRef();
                    var nextIndex = (newHead & ~HAS_NEXT).WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                    if (next->Next.AsRef() != null)
                        nextIndex |= HAS_NEXT;

                    _head.Block.AsRef() = next;
                    _head.Index.AsRef() = nextIndex;
                }

                var slot = block->Slots.GetUnchecked(offset);
                var value = slot->Value;
                if (offset.WrappingAdd(Usize.FromI32Unchecked(1)) == BLOCK_CAP)
                {
                    Block<T>.Drop(block);
                }
                else
                {
                    var state = block->Slots.GetUnchecked(offset)->State.AsRef();
                    block->Slots.GetUnchecked(offset)->State.AsRef() = state | READ;

                    if ((state & DESTROY) != Usize.FromI32Unchecked(0))
                        Block<T>.Destroy(block, offset.WrappingAdd(Usize.FromI32Unchecked(1)));
                }

                result = value;
                return true;
            }

            /// <summary>
            ///     Determines whether the queue is empty.
            /// </summary>
            /// <returns><see langword="true" /> if the queue is empty; otherwise, <see langword="false" />.</returns>
            /// <remarks>
            ///     The result may change as other threads modify the queue. Use this
            ///     method when only the presence or absence of elements is needed.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool IsEmpty()
            {
                var head = _head.Index.Load(Ordering.SeqCst);
                var tail = _tail.Index.Load(Ordering.SeqCst);
                return head >> I32.FromUsizeUnchecked(SHIFT) == tail >> I32.FromUsizeUnchecked(SHIFT);
            }

            /// <summary>
            ///     Returns the number of elements contained in the queue.
            /// </summary>
            /// <returns>The number of elements contained in the queue.</returns>
            /// <remarks>
            ///     The result may change as other threads modify the queue. Use
            ///     <see cref="IsEmpty" /> when only the presence or absence of elements is needed.
            /// </remarks>
            public nuint Count()
            {
                while (true)
                {
                    var tail = _tail.Index.Load(Ordering.SeqCst);
                    var head = _head.Index.Load(Ordering.SeqCst);
                    if (_tail.Index.Load(Ordering.SeqCst) == tail)
                    {
                        tail &= Usize.FromI32Unchecked(~(1 << I32.FromUsizeUnchecked(SHIFT)).WrappingSub(1));
                        head &= Usize.FromI32Unchecked(~(1 << I32.FromUsizeUnchecked(SHIFT)).WrappingSub(1));

                        if (((tail >> I32.FromUsizeUnchecked(SHIFT)) & LAP.WrappingSub(Usize.FromI32Unchecked(1))) == LAP.WrappingSub(Usize.FromI32Unchecked(1)))
                            tail = tail.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));

                        if (((head >> I32.FromUsizeUnchecked(SHIFT)) & LAP.WrappingSub(Usize.FromI32Unchecked(1))) == LAP.WrappingSub(Usize.FromI32Unchecked(1)))
                            head = head.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));

                        var lap = (head >> I32.FromUsizeUnchecked(SHIFT)) / LAP;

                        tail = tail.WrappingSub(lap.WrappingMul(LAP) << I32.FromUsizeUnchecked(SHIFT));
                        head = head.WrappingSub(lap.WrappingMul(LAP) << I32.FromUsizeUnchecked(SHIFT));

                        tail >>= I32.FromUsizeUnchecked(SHIFT);
                        head >>= I32.FromUsizeUnchecked(SHIFT);

                        return tail.WrappingSub(head).WrappingSub(tail / LAP);
                    }
                }
            }

            /// <summary>
            ///     Releases the resources used by the queue.
            /// </summary>
            public void Dispose()
            {
                ref var head = ref _head.Index.AsRef();
                ref var tail = ref _tail.Index.AsRef();
                ref var block = ref _head.Block.AsRef();

                head &= Usize.FromI32Unchecked(~(1 << I32.FromUsizeUnchecked(SHIFT)).WrappingSub(1));
                tail &= Usize.FromI32Unchecked(~(1 << I32.FromUsizeUnchecked(SHIFT)).WrappingSub(1));

                while (head != tail)
                {
                    var offset = (head >> I32.FromUsizeUnchecked(SHIFT)) % LAP;
                    if (offset >= BLOCK_CAP)
                    {
                        var next = block->Next.AsRef();
                        Block<T>.Drop(block);
                        block = next;
                    }

                    head = head.WrappingAdd(Usize.FromI32Unchecked(1 << I32.FromUsizeUnchecked(SHIFT)));
                }

                if (block != null)
                    Block<T>.Drop(block);
            }
        }
    }
}