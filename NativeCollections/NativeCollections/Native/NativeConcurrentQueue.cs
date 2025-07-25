using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2208
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentQueue
    /// </summary>
    internal static class NativeConcurrentQueue
    {
        /// <summary>
        ///     Segment
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NativeConcurrentQueueSegment<T> where T : unmanaged
        {
            /// <summary>
            ///     Slots
            /// </summary>
            public NativeConcurrentQueueSegmentSlots1024<T> Slots;

            /// <summary>
            ///     Head and tail
            /// </summary>
            public NativeConcurrentQueuePaddedHeadAndTail HeadAndTail;

            /// <summary>
            ///     Frozen for enqueues
            /// </summary>
            public bool FrozenForEnqueues;

            /// <summary>
            ///     Next segment
            /// </summary>
            public nint NextSegment;

            /// <summary>
            ///     Initialize
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize()
            {
                var slots = (NativeConcurrentQueueSegmentSlot<T>*)Unsafe.AsPointer(ref Slots);
                for (var i = 0; i < SLOTS_LENGTH; ++i)
                    Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)i).SequenceNumber = i;
                HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTail();
                FrozenForEnqueues = false;
                NextSegment = 0;
            }

            /// <summary>
            ///     Ensure frozen for enqueues
            /// </summary>
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
            ///     Try dequeue
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Dequeued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out T result)
            {
                var slots = (NativeConcurrentQueueSegmentSlot<T>*)Unsafe.AsPointer(ref Slots);
                var spinWait = new NativeSpinWait();
                while (true)
                {
                    var currentHead = Volatile.Read(ref HeadAndTail.Head);
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - (currentHead + 1);
                    if (diff == 0)
                    {
                        if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                        {
                            result = Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).Item;
                            Volatile.Write(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber, currentHead + SLOTS_LENGTH);
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
            ///     Try peek
            /// </summary>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek()
            {
                var slots = (NativeConcurrentQueueSegmentSlot<T>*)Unsafe.AsPointer(ref Slots);
                var spinWait = new NativeSpinWait();
                while (true)
                {
                    var currentHead = Volatile.Read(ref HeadAndTail.Head);
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber);
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
            ///     Try enqueue
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(in T item)
            {
                var slots = (NativeConcurrentQueueSegmentSlot<T>*)Unsafe.AsPointer(ref Slots);
                while (true)
                {
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    var slotsIndex = currentTail & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - currentTail;
                    if (diff == 0)
                    {
                        if (Interlocked.CompareExchange(ref HeadAndTail.Tail, currentTail + 1, currentTail) == currentTail)
                        {
                            Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).Item = item;
                            Volatile.Write(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber, currentTail + 1);
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
        ///     Padded head and tail
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
        public struct NativeConcurrentQueuePaddedHeadAndTail
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
        ///     Slots length
        /// </summary>
        public const int SLOTS_LENGTH = 1024;

        /// <summary>
        ///     Slots mask
        /// </summary>
        public const int SLOTS_MASK = SLOTS_LENGTH - 1;

        /// <summary>
        ///     Segment freeze offset
        /// </summary>
        public const int SEGMENT_FREEZE_OFFSET = SLOTS_LENGTH * 2;

        /// <summary>
        ///     Catch line size
        /// </summary>
        public const int CACHE_LINE_SIZE = 128;

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots1024<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots512<T> _element0;
            private NativeConcurrentQueueSegmentSlots512<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots512<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots256<T> _element0;
            private NativeConcurrentQueueSegmentSlots256<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots256<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots128<T> _element0;
            private NativeConcurrentQueueSegmentSlots128<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots128<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots64<T> _element0;
            private NativeConcurrentQueueSegmentSlots64<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots64<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots32<T> _element0;
            private NativeConcurrentQueueSegmentSlots32<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots32<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots16<T> _element0;
            private NativeConcurrentQueueSegmentSlots16<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots16<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots8<T> _element0;
            private NativeConcurrentQueueSegmentSlots8<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots8<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots4<T> _element0;
            private NativeConcurrentQueueSegmentSlots4<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots4<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlots2<T> _element0;
            private NativeConcurrentQueueSegmentSlots2<T> _element1;
        }

        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlots2<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlot<T> _element0;
            private NativeConcurrentQueueSegmentSlot<T> _element1;
        }

        /// <summary>
        ///     Slot
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConcurrentQueueSegmentSlot<T> where T : unmanaged
        {
            /// <summary>
            ///     Item
            /// </summary>
            public T Item;

            /// <summary>
            ///     Sequence number
            /// </summary>
            public int SequenceNumber;
        }
    }
}