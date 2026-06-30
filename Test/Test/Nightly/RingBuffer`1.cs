using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;
using static Examples.PaddingHelpers;

namespace Examples
{
    /// <summary>
    ///     Provides a multi-producer, multi-consumer thread-safe bounded segment.
    ///     When the queue is full, enqueues fail and return false.
    ///     When the queue is empty, dequeues fail and return default.
    /// </summary>
    /// <remarks>
    ///     http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue
    /// </remarks>
    public class RingBuffer<T>
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
        private readonly int _slotsLength;

        /// <summary>
        ///     Mask for quickly accessing a position within the queue's array.
        /// </summary>
        private readonly int _slotsMask;

        /// <summary>
        ///     Gets the "freeze offset" for this segment.
        /// </summary>
        private readonly int _segmentFreezeOffset;

        /// <summary>
        ///     The array of items in this queue.
        ///     Each slot contains the item in that slot and its "sequence number".
        /// </summary>
        private readonly Slot<T>[] _slots;

        /// <summary>
        ///     The head and tail positions, with padding to help avoid false sharing contention.
        /// </summary>
        /// <remarks>
        ///     Dequeuing happens from the head, enqueuing happens at the tail.
        ///     Mutable struct: do not make this readonly.
        /// </remarks>
        private PaddedHeadAndTail _headAndTail;

        /// <summary>
        ///     Indicates whether the segment has been marked such that no additional items may be enqueued.
        /// </summary>
        private bool _frozenForEnqueues;

        public RingBuffer(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            capacity = Math.Max(capacity, 2);
            capacity = (int)BitOperations.RoundUpToPowerOf2((uint)capacity);
            _slots = new Slot<T>[capacity];
            _slotsLength = capacity;
            _slotsMask = capacity - 1;
            _segmentFreezeOffset = capacity * 2;
            Initialize();
        }

        public int Capacity => _slotsLength;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>true if this is empty; otherwise, false.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended
        ///     rather than retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another thread will
        ///     modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty => !TryPeek();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the <see cref="IsEmpty" />
        ///     property is recommended rather than retrieving the number of items from the <see cref="Count" />
        ///     property and comparing it to 0.
        /// </remarks>
        public int Count
        {
            get
            {
                var head = Volatile.Read(ref _headAndTail.Head);
                var tail = Volatile.Read(ref _headAndTail.Tail);
                if (head != tail && head != tail - _segmentFreezeOffset)
                {
                    head &= _slotsMask;
                    tail &= _slotsMask;
                    return head < tail ? tail - head : _slotsLength - head + tail;
                }

                return 0;
            }
        }

        /// <summary>
        ///     Creates the segment.
        /// </summary>
        private void Initialize()
        {
            ref var slot = ref MemoryMarshal.GetArrayDataReference(_slots);
            for (var i = 0; i < _slotsLength; ++i)
                Unsafe.Add(ref slot, (nint)i).SequenceNumber = i;
            _headAndTail = new PaddedHeadAndTail();
            _frozenForEnqueues = false;
        }

        /// <summary>
        ///     Ensures that the segment will not accept any subsequent enqueues that aren't already underway, must only be called
        ///     while queue's segment lock is held.
        /// </summary>
        /// <remarks>
        ///     When we mark a segment as being frozen for additional enqueues,
        ///     we set the <see cref="_frozenForEnqueues" /> bool, but that's mostly
        ///     as a small helper to avoid marking it twice.
        ///     The real marking comes by modifying the Tail for the segment, increasing it by this
        ///     <see cref="_segmentFreezeOffset" />.
        ///     This effectively knocks it off the sequence expected by future enqueuers, such that any additional enqueuer will be
        ///     unable to enqueue due to it not lining up with the expected sequence numbers.
        ///     This value is chosen specially so that Tail will grow to a value that maps to the same slot but that won't be
        ///     confused with any other enqueue/dequeue sequence number.
        /// </remarks>
        public void EnsureFrozenForEnqueues()
        {
            if (!_frozenForEnqueues)
            {
                _frozenForEnqueues = true;
                Interlocked.Add(ref _headAndTail.Tail, _segmentFreezeOffset);
            }
        }

        /// <summary>
        ///     Tries to dequeue an element from the queue.
        /// </summary>
        public bool TryDequeue(out T? result)
        {
            ref var slot = ref MemoryMarshal.GetArrayDataReference(_slots);
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var currentHead = Volatile.Read(ref _headAndTail.Head);
                var slotsIndex = currentHead & _slotsMask;
                var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref _headAndTail.Head, currentHead + 1, currentHead) == currentHead)
                    {
                        result = Unsafe.Add(ref slot, (nint)slotsIndex).Item;
                        Volatile.Write(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber, currentHead + _slotsLength);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    var frozen = _frozenForEnqueues;
                    var currentTail = Volatile.Read(ref _headAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - _segmentFreezeOffset - currentHead <= 0))
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
        private bool TryPeek()
        {
            ref var slot = ref MemoryMarshal.GetArrayDataReference(_slots);
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var currentHead = Volatile.Read(ref _headAndTail.Head);
                var slotsIndex = currentHead & _slotsMask;
                var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                    return true;
                if (diff < 0)
                {
                    var frozen = _frozenForEnqueues;
                    var currentTail = Volatile.Read(ref _headAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - _segmentFreezeOffset - currentHead <= 0))
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
        public bool TryEnqueue(T? item)
        {
            ref var slot = ref MemoryMarshal.GetArrayDataReference(_slots);
            while (true)
            {
                var currentTail = Volatile.Read(ref _headAndTail.Tail);
                var slotsIndex = currentTail & _slotsMask;
                var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref slot, (nint)slotsIndex).SequenceNumber);
                var diff = sequenceNumber - currentTail;
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref _headAndTail.Tail, currentTail + 1, currentTail) == currentTail)
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
    ///     Represents a slot in the queue.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Slot<T>
    {
        /// <summary>
        ///     The item.
        /// </summary>
        public T? Item;

        /// <summary>
        ///     The sequence number for this slot, used to synchronize between enqueuers and dequeuers.
        /// </summary>
        public int SequenceNumber;
    }

    /// <summary>
    ///     Padded head and tail indices, to avoid false sharing between producers and consumers.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
    internal struct PaddedHeadAndTail
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
}