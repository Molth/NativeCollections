using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static Examples.PaddingHelpers;

// ReSharper disable ALL

namespace Examples
{
    /// <summary>
    ///     Native concurrentQueue
    /// </summary>
    internal static class SimpleConcurrentQueue
    {
        /// <summary>
        ///     The maximum number of elements the segment can contain.
        /// </summary>
        /// <remarks>
        ///     Must be a power of 2.
        ///     Maximum length of the segments used in the queue.
        ///     This is a somewhat arbitrary limit:
        ///     larger means that as long as we don't exceed the size, we avoid allocating more segments,
        ///     but if we do exceed it, then the segment becomes garbage.
        /// </remarks>
        public const int SLOTS_LENGTH = 4096;

        /// <summary>
        ///     Mask for quickly accessing a position within the queue's array.
        /// </summary>
        public const int SLOTS_MASK = SLOTS_LENGTH - 1;

        /// <summary>
        ///     Gets the "freeze offset" for this segment.
        /// </summary>
        public const int SEGMENT_FREEZE_OFFSET = SLOTS_LENGTH * 2;

        /// <summary>
        ///     Provides a multi-producer, multi-consumer thread-safe bounded segment.
        ///     When the queue is full, enqueues fail and return false.
        ///     When the queue is empty, dequeues fail and return default.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Segment<T> where T : unmanaged
        {
            /// <summary>
            ///     The array of items in this queue.
            ///     Each slot contains the item in that slot and its "sequence number".
            /// </summary>
            public Slots<T> Slots;

            /// <summary>
            ///     The head and tail positions, with padding to help avoid false sharing contention.
            /// </summary>
            /// <remarks>
            ///     Dequeuing happens from the head, enqueuing happens at the tail.
            ///     Mutable struct: do not make this readonly.
            /// </remarks>
            public PaddedHeadAndTail HeadAndTail;

            /// <summary>
            ///     Indicates whether the segment has been marked such that no additional items may be enqueued.
            /// </summary>
            public bool FrozenForEnqueues;

            /// <summary>
            ///     The segment following this one in the queue, or null if this segment is the last in the queue.
            /// </summary>
            public nint NextSegment;

            /// <summary>
            ///     Creates the segment.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize()
            {
                ref var slot = ref Unsafe.As<Slots<T>, Slot<T>>(ref Slots);
                for (var i = 0; i < SLOTS_LENGTH; ++i)
                    Unsafe.Add(ref slot, (nint)i).SequenceNumber = i;
                HeadAndTail = new PaddedHeadAndTail();
                FrozenForEnqueues = false;
                NextSegment = 0;
            }

            /// <summary>
            ///     Ensures that the segment will not accept any subsequent enqueues that aren't already underway, must only be called
            ///     while queue's segment lock is held.
            /// </summary>
            /// <remarks>
            ///     When we mark a segment as being frozen for additional enqueues,
            ///     we set the <see cref="FrozenForEnqueues" /> bool, but that's mostly
            ///     as a small helper to avoid marking it twice.
            ///     The real marking comes by modifying the Tail for the segment, increasing it by this
            ///     <see cref="SEGMENT_FREEZE_OFFSET" />.
            ///     This effectively knocks it off the sequence expected by future enqueuers, such that any additional enqueuer will be
            ///     unable to enqueue due to it not lining up with the expected sequence numbers.
            ///     This value is chosen specially so that Tail will grow to a value that maps to the same slot but that won't be
            ///     confused with any other enqueue/dequeue sequence number.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void EnsureFrozenForEnqueues()
            {
                if (!FrozenForEnqueues)
                {
                    FrozenForEnqueues = true;
                    Interlocked.Add(ref HeadAndTail.Tail, SEGMENT_FREEZE_OFFSET);
                }
            }

            /// <summary>
            ///     Tries to dequeue an element from the queue.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out T result)
            {
                ref var slot = ref Unsafe.As<Slots<T>, Slot<T>>(ref Slots);
                var spinWait = new SpinWait();
                while (true)
                {
                    var currentHead = Volatile.Read(ref HeadAndTail.Head);
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - (currentHead + 1);
                    if (diff == 0)
                    {
                        if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                        {
                            result = Unsafe.Add(ref slot, (nint)slotsIndex).Item;
                            Volatile.Write(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber, currentHead + SLOTS_LENGTH);
                            return true;
                        }
                    }
                    else if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - SEGMENT_FREEZE_OFFSET - currentHead <= 0))
                        {
                            result = default;
                            return false;
                        }

                        spinWait.SpinOnce(-1);
                    }
                }
            }

            /// <summary>
            ///     Tries to peek at an element from the queue, without removing it.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek()
            {
                ref var slot = ref Unsafe.As<Slots<T>, Slot<T>>(ref Slots);
                var spinWait = new SpinWait();
                while (true)
                {
                    var currentHead = Volatile.Read(ref HeadAndTail.Head);
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - (currentHead + 1);
                    if (diff == 0)
                        return true;
                    if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - SEGMENT_FREEZE_OFFSET - currentHead <= 0))
                            return false;
                        spinWait.SpinOnce(-1);
                    }
                }
            }

            /// <summary>
            ///     Attempts to enqueue the item.
            ///     If successful, the item will be stored in the queue and true will be returned; otherwise, the item won't be stored,
            ///     and false will be returned.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(T item)
            {
                ref var slot = ref Unsafe.As<Slots<T>, Slot<T>>(ref Slots);
                while (true)
                {
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    var slotsIndex = currentTail & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - currentTail;
                    if (diff == 0)
                    {
                        if (Interlocked.CompareExchange(ref HeadAndTail.Tail, currentTail + 1, currentTail) == currentTail)
                        {
                            Unsafe.Add(ref slot, (nint)slotsIndex).Item = item;
                            Volatile.Write(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber, currentTail + 1);
                            return true;
                        }
                    }
                    else if (diff < 0)
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        ///     Padded head and tail indices, to avoid false sharing between producers and consumers.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
        public struct PaddedHeadAndTail
        {
            /// <summary>
            ///     Head
            /// </summary>
            [FieldOffset(1 * CACHE_LINE_SIZE)] public int Head;

            /// <summary>
            ///     Tail
            /// </summary>
            [FieldOffset(2 * CACHE_LINE_SIZE)] public int Tail;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [InlineArray(SLOTS_LENGTH)]
        [StructLayout(LayoutKind.Sequential)]
        public struct Slots<T> where T : unmanaged
        {
            private Slot<T> _element0;
        }

        /// <summary>
        ///     Represents a slot in the queue.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Slot<T> where T : unmanaged
        {
            /// <summary>
            ///     The item.
            /// </summary>
            public T Item;

            /// <summary>
            ///     The sequence number for this slot, used to synchronize between enqueuers and dequeuers.
            /// </summary>
            public int SequenceNumber;
        }
    }
}