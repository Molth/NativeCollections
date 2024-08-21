using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Threading;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeConcurrentQueue<T> : IDisposable, IEquatable<NativeConcurrentQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentQueueHandle
        {
            /// <summary>
            ///     Cross segment lock
            /// </summary>
            public NativeMonitorLock CrossSegmentLock;

            /// <summary>
            ///     Segment pool
            /// </summary>
            public NativeMemoryPool SegmentPool;

            /// <summary>
            ///     Slots pool
            /// </summary>
            public NativeArrayPool<NativeConcurrentQueueSegment<T>.Slot> SlotsPool;

            /// <summary>
            ///     Tail
            /// </summary>
            public volatile NativeConcurrentQueueSegment<T>* Tail;

            /// <summary>
            ///     Head
            /// </summary>
            public volatile NativeConcurrentQueueSegment<T>* Head;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeConcurrentQueueHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="maxLength">Max length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueue(int size, int maxFreeSlabs, int maxLength)
        {
            var segmentPool = new NativeMemoryPool(size, sizeof(NativeConcurrentQueueSegment<T>), maxFreeSlabs);
            if (maxLength < 32)
                maxLength = 32;
            else if (maxLength > 1048576)
                maxLength = 1048576;
            var slotsPool = new NativeArrayPool<NativeConcurrentQueueSegment<T>.Slot>(1, maxLength);
            _handle = (NativeConcurrentQueueHandle*)NativeMemoryAllocator.Alloc(sizeof(NativeConcurrentQueueHandle));
            _handle->CrossSegmentLock = new NativeMonitorLock(new object());
            _handle->SegmentPool = segmentPool;
            _handle->SlotsPool = slotsPool;
            var segment = (NativeConcurrentQueueSegment<T>*)_handle->SegmentPool.Rent();
            var slots = _handle->SlotsPool.Rent(32);
            segment->Initialize(slots.Array, 32);
            _handle->Tail = _handle->Head = segment;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var segment = _handle->Head;
                while (true)
                {
                    var next = Volatile.Read(ref segment->NextSegment);
                    if (segment->TryPeek())
                        return false;
                    if (next != IntPtr.Zero)
                        segment = (NativeConcurrentQueueSegment<T>*)next;
                    else if (Volatile.Read(ref segment->NextSegment) == IntPtr.Zero)
                        break;
                }

                return true;
            }
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var spinCount = 0;
                while (true)
                {
                    var head = _handle->Head;
                    var tail = _handle->Tail;
                    var headHead = Volatile.Read(ref head->HeadAndTail.Head);
                    var headTail = Volatile.Read(ref head->HeadAndTail.Tail);
                    if (head == tail)
                    {
                        if (head == _handle->Head && tail == _handle->Tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail);
                    }
                    else if ((NativeConcurrentQueueSegment<T>*)head->NextSegment == tail)
                    {
                        var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                        var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                        if (head == _handle->Head && tail == _handle->Tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                    }
                    else
                    {
                        _handle->CrossSegmentLock.Enter();
                        if (head == _handle->Head && tail == _handle->Tail)
                        {
                            var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                            var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                            if (headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            {
                                var count = GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                                for (var s = (NativeConcurrentQueueSegment<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegment<T>*)s->NextSegment)
                                    count += s->HeadAndTail.Tail - s->FreezeOffset;
                                return count;
                            }
                        }

                        _handle->CrossSegmentLock.Exit();
                    }

                    if ((spinCount >= 10 && (spinCount - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = spinCount >= 10 ? (spinCount - 10) / 2 : spinCount;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (spinCount <= 30 && 1 << spinCount < iterations)
                            iterations = 1 << spinCount;
                        Thread.SpinWait(iterations);
                    }

                    spinCount = spinCount == int.MaxValue ? 10 : spinCount + 1;
                }
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentQueue<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentQueue<T> nativeConcurrentQueue && nativeConcurrentQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentQueue<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            _handle->CrossSegmentLock.Dispose();
            _handle->SegmentPool.Dispose();
            _handle->SlotsPool.Dispose();
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _handle->CrossSegmentLock.Enter();
            _handle->Tail->EnsureFrozenForEnqueues();
            var node = _handle->Head;
            while (node != null)
            {
                var temp = node;
                node = (NativeConcurrentQueueSegment<T>*)node->NextSegment;
                temp->Dispose(_handle->SlotsPool);
                _handle->SegmentPool.Return(temp);
            }

            var segment = (NativeConcurrentQueueSegment<T>*)NativeMemoryAllocator.Alloc(sizeof(NativeConcurrentQueueSegment<T>));
            var slots = _handle->SlotsPool.Rent(32);
            segment->Initialize(slots.Array, 32);
            _handle->Tail = _handle->Head = segment;
            _handle->CrossSegmentLock.Exit();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (!_handle->Tail->TryEnqueue(item))
            {
                while (true)
                {
                    var tail = _handle->Tail;
                    if (tail->TryEnqueue(item))
                        return;
                    _handle->CrossSegmentLock.Enter();
                    if (tail == _handle->Tail)
                    {
                        tail->EnsureFrozenForEnqueues();
                        var newSize = tail->Length * 2;
                        var nextSize = newSize <= 1048576 ? newSize : 1048576;
                        var newTail = (NativeConcurrentQueueSegment<T>*)_handle->SegmentPool.Rent();
                        if (_handle->SlotsPool.TryRent(nextSize, out var array))
                            newTail->Initialize(array.Array, nextSize);
                        else
                            newTail->Initialize(nextSize);
                        tail->NextSegment = (nint)newTail;
                        _handle->Tail = newTail;
                    }

                    _handle->CrossSegmentLock.Exit();
                }
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
            var head = _handle->Head;
            if (head->TryDequeue(out result))
                return true;
            if (head->NextSegment == IntPtr.Zero)
            {
                result = default;
                return false;
            }

            while (true)
            {
                head = _handle->Head;
                if (head->TryDequeue(out result))
                    return true;
                if (head->NextSegment == IntPtr.Zero)
                {
                    result = default;
                    return false;
                }

                if (head->TryDequeue(out result))
                    return true;
                _handle->CrossSegmentLock.Enter();
                if (head == _handle->Head)
                    _handle->Head = (NativeConcurrentQueueSegment<T>*)head->NextSegment;
                _handle->CrossSegmentLock.Exit();
            }
        }

        /// <summary>
        ///     Get count
        /// </summary>
        /// <param name="segment">Segment</param>
        /// <param name="head">Head</param>
        /// <param name="tail">Tail</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount(NativeConcurrentQueueSegment<T>* segment, int head, int tail)
        {
            if (head != tail && head != tail - segment->FreezeOffset)
            {
                head &= segment->SlotsMask;
                tail &= segment->SlotsMask;
                return head < tail ? tail - head : segment->Length - head + tail;
            }

            return 0;
        }

        /// <summary>
        ///     Get count
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="headHead">Head head</param>
        /// <param name="tail">Tail</param>
        /// <param name="tailTail">Tail tail</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetCount(NativeConcurrentQueueSegment<T>* head, int headHead, NativeConcurrentQueueSegment<T>* tail, int tailTail)
        {
            long count = 0;
            var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - head->FreezeOffset;
            if (headHead < headTail)
            {
                headHead &= head->SlotsMask;
                headTail &= head->SlotsMask;
                count += headHead < headTail ? headTail - headHead : head->Length - headHead + headTail;
            }

            if (head != tail)
            {
                for (var s = (NativeConcurrentQueueSegment<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegment<T>*)s->NextSegment)
                    count += s->HeadAndTail.Tail - s->FreezeOffset;
                count += tailTail - tail->FreezeOffset;
            }

            return count;
        }
    }

    /// <summary>
    ///     Native concurrentQueue segment
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueSegment<T> where T : unmanaged
    {
        /// <summary>
        ///     Slots
        /// </summary>
        public Slot* Slots;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length;

        /// <summary>
        ///     Slots mask
        /// </summary>
        public int SlotsMask;

        /// <summary>
        ///     Head and tail
        /// </summary>
        public PaddedHeadAndTail HeadAndTail;

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
        /// <param name="boundedLength">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(int boundedLength)
        {
            Slots = (Slot*)NativeMemoryAllocator.Alloc(boundedLength * sizeof(Slot));
            for (var i = 0; i < boundedLength; ++i)
                Slots[i].SequenceNumber = i;
            Length = boundedLength;
            SlotsMask = boundedLength - 1;
            HeadAndTail = new PaddedHeadAndTail();
            FrozenForEnqueues = false;
            NextSegment = IntPtr.Zero;
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="slots">Slots</param>
        /// <param name="boundedLength">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize(Slot* slots, int boundedLength)
        {
            Slots = slots;
            for (var i = 0; i < boundedLength; ++i)
                Slots[i].SequenceNumber = i;
            Length = boundedLength;
            SlotsMask = boundedLength - 1;
            HeadAndTail = new PaddedHeadAndTail();
            FrozenForEnqueues = false;
            NextSegment = IntPtr.Zero;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        /// <param name="arrayPool">Slots pool</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(in NativeArrayPool<Slot> arrayPool)
        {
            if (arrayPool.TryReturn(Length, Slots))
                return;
            NativeMemoryAllocator.Free(Slots);
        }

        /// <summary>
        ///     Freeze offset
        /// </summary>
        public int FreezeOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length * 2;
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
                Interlocked.Add(ref HeadAndTail.Tail, FreezeOffset);
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
            var slots = Slots;
            var count = 0;
            while (true)
            {
                var currentHead = Volatile.Read(ref HeadAndTail.Head);
                var slotsIndex = currentHead & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                    {
                        result = slots[slotsIndex].Item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentHead + Length);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - FreezeOffset - currentHead <= 0))
                    {
                        result = default;
                        return false;
                    }

                    if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
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
            var slots = Slots;
            var count = 0;
            while (true)
            {
                var currentHead = Volatile.Read(ref HeadAndTail.Head);
                var slotsIndex = currentHead & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                    return true;
                if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - FreezeOffset - currentHead <= 0))
                        return false;
                    if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
                }
            }
        }

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T item)
        {
            var slots = Slots;
            while (true)
            {
                var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                var slotsIndex = currentTail & SlotsMask;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - currentTail;
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Tail, currentTail + 1, currentTail) == currentTail)
                    {
                        slots[slotsIndex].Item = item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentTail + 1);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Slot
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Slot
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

    /// <summary>
    ///     Padded head and tail
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

        /// <summary>
        ///     Catch line size
        /// </summary>
#if TARGET_ARM64
        public const int CACHE_LINE_SIZE = 128;
#else
        public const int CACHE_LINE_SIZE = 64;
#endif
    }
}