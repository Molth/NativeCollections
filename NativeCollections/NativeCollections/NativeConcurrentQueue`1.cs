using System;
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
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeConcurrentQueue<T> : IDisposable, IEquatable<NativeConcurrentQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentQueue<T>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueue(int size, int maxFreeSlabs)
        {
            var value = new UnsafeConcurrentQueue<T>(size, maxFreeSlabs);
            var handle = (UnsafeConcurrentQueue<T>*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeConcurrentQueue<T>));
            *handle = value;
            _handle = handle;
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
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count;
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
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

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
            var handle = _handle;
            if (handle == null)
                return;
            _handle->Dispose();
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item) => _handle->Enqueue(item);

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle->TryDequeue(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentQueue<T> Empty => new();
    }

    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueNotArm64<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Cross segment lock
        /// </summary>
        private GCHandle _crossSegmentLock;

        /// <summary>
        ///     Segment pool
        /// </summary>
        private UnsafeMemoryPool _segmentPool;

        /// <summary>
        ///     Tail
        /// </summary>
        private volatile NativeConcurrentQueueSegmentNotArm64<T>* _tail;

        /// <summary>
        ///     Head
        /// </summary>
        private volatile NativeConcurrentQueueSegmentNotArm64<T>* _head;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var segment = _head;
                while (true)
                {
                    var next = Volatile.Read(ref segment->NextSegment);
                    if (segment->TryPeek())
                        return false;
                    if (next != IntPtr.Zero)
                        segment = (NativeConcurrentQueueSegmentNotArm64<T>*)next;
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
                var spinWait = new NativeSpinWait();
                while (true)
                {
                    var head = _head;
                    var tail = _tail;
                    var headHead = Volatile.Read(ref head->HeadAndTail.Head);
                    var headTail = Volatile.Read(ref head->HeadAndTail.Tail);
                    if (head == tail)
                    {
                        if (head == _head && tail == _tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail);
                    }
                    else if ((NativeConcurrentQueueSegmentNotArm64<T>*)head->NextSegment == tail)
                    {
                        var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                        var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                        if (head == _head && tail == _tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                    }
                    else
                    {
                        lock (_crossSegmentLock.Target)
                        {
                            if (head == _head && tail == _tail)
                            {
                                var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                                var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                                if (headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                                {
                                    var count = GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                                    for (var s = (NativeConcurrentQueueSegmentNotArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentNotArm64<T>*)s->NextSegment)
                                        count += s->HeadAndTail.Tail - NativeConcurrentQueue.FREEZE_OFFSET;
                                    return count;
                                }
                            }
                        }
                    }

                    spinWait.SpinOnce();
                }
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueueNotArm64(int size, int maxFreeSlabs)
        {
            var segmentPool = new UnsafeMemoryPool(size, sizeof(NativeConcurrentQueueSegmentNotArm64<T>), maxFreeSlabs);
            _crossSegmentLock = GCHandle.Alloc(new object(), GCHandleType.Normal);
            _segmentPool = segmentPool;
            var segment = (NativeConcurrentQueueSegmentNotArm64<T>*)_segmentPool.Rent();
            segment->Initialize();
            _tail = _head = segment;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _crossSegmentLock.Free();
            _segmentPool.Dispose();
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_crossSegmentLock.Target)
            {
                _tail->EnsureFrozenForEnqueues();
                var node = _head;
                while (node != null)
                {
                    var temp = node;
                    node = (NativeConcurrentQueueSegmentNotArm64<T>*)node->NextSegment;
                    _segmentPool.Return(temp);
                }

                var segment = (NativeConcurrentQueueSegmentNotArm64<T>*)_segmentPool.Rent();
                segment->Initialize();
                _tail = _head = segment;
            }
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            if (!_tail->TryEnqueue(item))
            {
                while (true)
                {
                    var tail = _tail;
                    if (tail->TryEnqueue(item))
                        return;
                    lock (_crossSegmentLock.Target)
                    {
                        if (tail == _tail)
                        {
                            tail->EnsureFrozenForEnqueues();
                            var newTail = (NativeConcurrentQueueSegmentNotArm64<T>*)_segmentPool.Rent();
                            newTail->Initialize();
                            tail->NextSegment = (nint)newTail;
                            _tail = newTail;
                        }
                    }
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
            var head = _head;
            if (head->TryDequeue(out result))
                return true;
            if (head->NextSegment == IntPtr.Zero)
            {
                result = default;
                return false;
            }

            while (true)
            {
                head = _head;
                if (head->TryDequeue(out result))
                    return true;
                if (head->NextSegment == IntPtr.Zero)
                {
                    result = default;
                    return false;
                }

                if (head->TryDequeue(out result))
                    return true;
                lock (_crossSegmentLock.Target)
                {
                    if (head == _head)
                    {
                        _head = (NativeConcurrentQueueSegmentNotArm64<T>*)head->NextSegment;
                        _segmentPool.Return(head);
                    }
                }
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
        private static int GetCount(NativeConcurrentQueueSegmentNotArm64<T>* segment, int head, int tail)
        {
            if (head != tail && head != tail - NativeConcurrentQueue.FREEZE_OFFSET)
            {
                head &= NativeConcurrentQueue.SLOTS_MASK;
                tail &= NativeConcurrentQueue.SLOTS_MASK;
                return head < tail ? tail - head : NativeConcurrentQueue.LENGTH - head + tail;
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
        private static long GetCount(NativeConcurrentQueueSegmentNotArm64<T>* head, int headHead, NativeConcurrentQueueSegmentNotArm64<T>* tail, int tailTail)
        {
            long count = 0;
            var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - NativeConcurrentQueue.FREEZE_OFFSET;
            if (headHead < headTail)
            {
                headHead &= NativeConcurrentQueue.SLOTS_MASK;
                headTail &= NativeConcurrentQueue.SLOTS_MASK;
                count += headHead < headTail ? headTail - headHead : NativeConcurrentQueue.LENGTH - headHead + headTail;
            }

            if (head != tail)
            {
                for (var s = (NativeConcurrentQueueSegmentNotArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentNotArm64<T>*)s->NextSegment)
                    count += s->HeadAndTail.Tail - NativeConcurrentQueue.FREEZE_OFFSET;
                count += tailTail - NativeConcurrentQueue.FREEZE_OFFSET;
            }

            return count;
        }
    }

    /// <summary>
    ///     Native concurrentQueue segment
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueSegmentNotArm64<T> where T : unmanaged
    {
        /// <summary>
        ///     Slots
        /// </summary>
        public NativeConcurrentQueueSegmentSlots<T> Slots;

        /// <summary>
        ///     Head and tail
        /// </summary>
        public NativeConcurrentQueuePaddedHeadAndTailNotArm64 HeadAndTail;

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
            for (var i = 0; i < NativeConcurrentQueue.LENGTH; ++i)
                slots[i].SequenceNumber = i;
            HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTailNotArm64();
            FrozenForEnqueues = false;
            NextSegment = IntPtr.Zero;
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
                Interlocked.Add(ref HeadAndTail.Tail, NativeConcurrentQueue.FREEZE_OFFSET);
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
                var slotsIndex = currentHead & NativeConcurrentQueue.SLOTS_MASK;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                    {
                        result = slots[slotsIndex].Item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentHead + NativeConcurrentQueue.LENGTH);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - NativeConcurrentQueue.FREEZE_OFFSET - currentHead <= 0))
                    {
                        result = default;
                        return false;
                    }

                    spinWait.SpinOnce();
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
                var slotsIndex = currentHead & NativeConcurrentQueue.SLOTS_MASK;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                    return true;
                if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - NativeConcurrentQueue.FREEZE_OFFSET - currentHead <= 0))
                        return false;
                    spinWait.SpinOnce();
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
                var slotsIndex = currentTail & NativeConcurrentQueue.SLOTS_MASK;
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
    }

    /// <summary>
    ///     NativeConcurrentQueue padded head and tail
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
    internal struct NativeConcurrentQueuePaddedHeadAndTailNotArm64
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
        public const int CACHE_LINE_SIZE = 64;
    }

    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueArm64<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Cross segment lock
        /// </summary>
        private GCHandle _crossSegmentLock;

        /// <summary>
        ///     Segment pool
        /// </summary>
        private UnsafeMemoryPool _segmentPool;

        /// <summary>
        ///     Tail
        /// </summary>
        private volatile NativeConcurrentQueueSegmentArm64<T>* _tail;

        /// <summary>
        ///     Head
        /// </summary>
        private volatile NativeConcurrentQueueSegmentArm64<T>* _head;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var segment = _head;
                while (true)
                {
                    var next = Volatile.Read(ref segment->NextSegment);
                    if (segment->TryPeek())
                        return false;
                    if (next != IntPtr.Zero)
                        segment = (NativeConcurrentQueueSegmentArm64<T>*)next;
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
                var spinWait = new NativeSpinWait();
                while (true)
                {
                    var head = _head;
                    var tail = _tail;
                    var headHead = Volatile.Read(ref head->HeadAndTail.Head);
                    var headTail = Volatile.Read(ref head->HeadAndTail.Tail);
                    if (head == tail)
                    {
                        if (head == _head && tail == _tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail);
                    }
                    else if ((NativeConcurrentQueueSegmentArm64<T>*)head->NextSegment == tail)
                    {
                        var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                        var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                        if (head == _head && tail == _tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            return GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                    }
                    else
                    {
                        lock (_crossSegmentLock.Target)
                        {
                            if (head == _head && tail == _tail)
                            {
                                var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                                var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                                if (headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                                {
                                    var count = GetCount(head, headHead, headTail) + GetCount(tail, tailHead, tailTail);
                                    for (var s = (NativeConcurrentQueueSegmentArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentArm64<T>*)s->NextSegment)
                                        count += s->HeadAndTail.Tail - NativeConcurrentQueue.FREEZE_OFFSET;
                                    return count;
                                }
                            }
                        }
                    }

                    spinWait.SpinOnce();
                }
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentQueueArm64(int size, int maxFreeSlabs)
        {
            var segmentPool = new UnsafeMemoryPool(size, sizeof(NativeConcurrentQueueSegmentArm64<T>), maxFreeSlabs);
            _crossSegmentLock = GCHandle.Alloc(new object(), GCHandleType.Normal);
            _segmentPool = segmentPool;
            var segment = (NativeConcurrentQueueSegmentArm64<T>*)_segmentPool.Rent();
            segment->Initialize();
            _tail = _head = segment;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _crossSegmentLock.Free();
            _segmentPool.Dispose();
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            lock (_crossSegmentLock.Target)
            {
                _tail->EnsureFrozenForEnqueues();
                var node = _head;
                while (node != null)
                {
                    var temp = node;
                    node = (NativeConcurrentQueueSegmentArm64<T>*)node->NextSegment;
                    _segmentPool.Return(temp);
                }

                var segment = (NativeConcurrentQueueSegmentArm64<T>*)_segmentPool.Rent();
                segment->Initialize();
                _tail = _head = segment;
            }
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            if (!_tail->TryEnqueue(item))
            {
                while (true)
                {
                    var tail = _tail;
                    if (tail->TryEnqueue(item))
                        return;
                    lock (_crossSegmentLock.Target)
                    {
                        if (tail == _tail)
                        {
                            tail->EnsureFrozenForEnqueues();
                            var newTail = (NativeConcurrentQueueSegmentArm64<T>*)_segmentPool.Rent();
                            newTail->Initialize();
                            tail->NextSegment = (nint)newTail;
                            _tail = newTail;
                        }
                    }
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
            var head = _head;
            if (head->TryDequeue(out result))
                return true;
            if (head->NextSegment == IntPtr.Zero)
            {
                result = default;
                return false;
            }

            while (true)
            {
                head = _head;
                if (head->TryDequeue(out result))
                    return true;
                if (head->NextSegment == IntPtr.Zero)
                {
                    result = default;
                    return false;
                }

                if (head->TryDequeue(out result))
                    return true;
                lock (_crossSegmentLock.Target)
                {
                    if (head == _head)
                    {
                        _head = (NativeConcurrentQueueSegmentArm64<T>*)head->NextSegment;
                        _segmentPool.Return(head);
                    }
                }
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
        private static int GetCount(NativeConcurrentQueueSegmentArm64<T>* segment, int head, int tail)
        {
            if (head != tail && head != tail - NativeConcurrentQueue.FREEZE_OFFSET)
            {
                head &= NativeConcurrentQueue.SLOTS_MASK;
                tail &= NativeConcurrentQueue.SLOTS_MASK;
                return head < tail ? tail - head : NativeConcurrentQueue.LENGTH - head + tail;
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
        private static long GetCount(NativeConcurrentQueueSegmentArm64<T>* head, int headHead, NativeConcurrentQueueSegmentArm64<T>* tail, int tailTail)
        {
            long count = 0;
            var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - NativeConcurrentQueue.FREEZE_OFFSET;
            if (headHead < headTail)
            {
                headHead &= NativeConcurrentQueue.SLOTS_MASK;
                headTail &= NativeConcurrentQueue.SLOTS_MASK;
                count += headHead < headTail ? headTail - headHead : NativeConcurrentQueue.LENGTH - headHead + headTail;
            }

            if (head != tail)
            {
                for (var s = (NativeConcurrentQueueSegmentArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentArm64<T>*)s->NextSegment)
                    count += s->HeadAndTail.Tail - NativeConcurrentQueue.FREEZE_OFFSET;
                count += tailTail - NativeConcurrentQueue.FREEZE_OFFSET;
            }

            return count;
        }
    }

    /// <summary>
    ///     Native concurrentQueue segment
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeConcurrentQueueSegmentArm64<T> where T : unmanaged
    {
        /// <summary>
        ///     Slots
        /// </summary>
        public NativeConcurrentQueueSegmentSlots<T> Slots;

        /// <summary>
        ///     Head and tail
        /// </summary>
        public NativeConcurrentQueuePaddedHeadAndTailArm64 HeadAndTail;

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
            for (var i = 0; i < NativeConcurrentQueue.LENGTH; ++i)
                slots[i].SequenceNumber = i;
            HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTailArm64();
            FrozenForEnqueues = false;
            NextSegment = IntPtr.Zero;
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
                Interlocked.Add(ref HeadAndTail.Tail, NativeConcurrentQueue.FREEZE_OFFSET);
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
                var slotsIndex = currentHead & NativeConcurrentQueue.SLOTS_MASK;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                {
                    if (Interlocked.CompareExchange(ref HeadAndTail.Head, currentHead + 1, currentHead) == currentHead)
                    {
                        result = slots[slotsIndex].Item;
                        Volatile.Write(ref slots[slotsIndex].SequenceNumber, currentHead + NativeConcurrentQueue.LENGTH);
                        return true;
                    }
                }
                else if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - NativeConcurrentQueue.FREEZE_OFFSET - currentHead <= 0))
                    {
                        result = default;
                        return false;
                    }

                    spinWait.SpinOnce();
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
                var slotsIndex = currentHead & NativeConcurrentQueue.SLOTS_MASK;
                var sequenceNumber = Volatile.Read(ref slots[slotsIndex].SequenceNumber);
                var diff = sequenceNumber - (currentHead + 1);
                if (diff == 0)
                    return true;
                if (diff < 0)
                {
                    var frozen = FrozenForEnqueues;
                    var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                    if (currentTail - currentHead <= 0 || (frozen && currentTail - NativeConcurrentQueue.FREEZE_OFFSET - currentHead <= 0))
                        return false;
                    spinWait.SpinOnce();
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
                var slotsIndex = currentTail & NativeConcurrentQueue.SLOTS_MASK;
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
    }

    /// <summary>
    ///     NativeConcurrentQueue padded head and tail
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
    internal struct NativeConcurrentQueuePaddedHeadAndTailArm64
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
        public const int CACHE_LINE_SIZE = 128;
    }

    /// <summary>
    ///     Native concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    internal static class NativeConcurrentQueue
    {
        /// <summary>
        ///     Length
        /// </summary>
        public const int LENGTH = 1024;

        /// <summary>
        ///     Slots mask
        /// </summary>
        public const int SLOTS_MASK = LENGTH - 1;

        /// <summary>
        ///     Freeze offset
        /// </summary>
        public const int FREEZE_OFFSET = LENGTH * 2;
    }

    /// <summary>
    ///     Slots
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeConcurrentQueueSegmentSlots<T> where T : unmanaged
    {
        private NativeConcurrentQueueSegmentSlot<T> _slot0;
        private NativeConcurrentQueueSegmentSlot<T> _slot1;
        private NativeConcurrentQueueSegmentSlot<T> _slot2;
        private NativeConcurrentQueueSegmentSlot<T> _slot3;
        private NativeConcurrentQueueSegmentSlot<T> _slot4;
        private NativeConcurrentQueueSegmentSlot<T> _slot5;
        private NativeConcurrentQueueSegmentSlot<T> _slot6;
        private NativeConcurrentQueueSegmentSlot<T> _slot7;
        private NativeConcurrentQueueSegmentSlot<T> _slot8;
        private NativeConcurrentQueueSegmentSlot<T> _slot9;
        private NativeConcurrentQueueSegmentSlot<T> _slot10;
        private NativeConcurrentQueueSegmentSlot<T> _slot11;
        private NativeConcurrentQueueSegmentSlot<T> _slot12;
        private NativeConcurrentQueueSegmentSlot<T> _slot13;
        private NativeConcurrentQueueSegmentSlot<T> _slot14;
        private NativeConcurrentQueueSegmentSlot<T> _slot15;
        private NativeConcurrentQueueSegmentSlot<T> _slot16;
        private NativeConcurrentQueueSegmentSlot<T> _slot17;
        private NativeConcurrentQueueSegmentSlot<T> _slot18;
        private NativeConcurrentQueueSegmentSlot<T> _slot19;
        private NativeConcurrentQueueSegmentSlot<T> _slot20;
        private NativeConcurrentQueueSegmentSlot<T> _slot21;
        private NativeConcurrentQueueSegmentSlot<T> _slot22;
        private NativeConcurrentQueueSegmentSlot<T> _slot23;
        private NativeConcurrentQueueSegmentSlot<T> _slot24;
        private NativeConcurrentQueueSegmentSlot<T> _slot25;
        private NativeConcurrentQueueSegmentSlot<T> _slot26;
        private NativeConcurrentQueueSegmentSlot<T> _slot27;
        private NativeConcurrentQueueSegmentSlot<T> _slot28;
        private NativeConcurrentQueueSegmentSlot<T> _slot29;
        private NativeConcurrentQueueSegmentSlot<T> _slot30;
        private NativeConcurrentQueueSegmentSlot<T> _slot31;
        private NativeConcurrentQueueSegmentSlot<T> _slot32;
        private NativeConcurrentQueueSegmentSlot<T> _slot33;
        private NativeConcurrentQueueSegmentSlot<T> _slot34;
        private NativeConcurrentQueueSegmentSlot<T> _slot35;
        private NativeConcurrentQueueSegmentSlot<T> _slot36;
        private NativeConcurrentQueueSegmentSlot<T> _slot37;
        private NativeConcurrentQueueSegmentSlot<T> _slot38;
        private NativeConcurrentQueueSegmentSlot<T> _slot39;
        private NativeConcurrentQueueSegmentSlot<T> _slot40;
        private NativeConcurrentQueueSegmentSlot<T> _slot41;
        private NativeConcurrentQueueSegmentSlot<T> _slot42;
        private NativeConcurrentQueueSegmentSlot<T> _slot43;
        private NativeConcurrentQueueSegmentSlot<T> _slot44;
        private NativeConcurrentQueueSegmentSlot<T> _slot45;
        private NativeConcurrentQueueSegmentSlot<T> _slot46;
        private NativeConcurrentQueueSegmentSlot<T> _slot47;
        private NativeConcurrentQueueSegmentSlot<T> _slot48;
        private NativeConcurrentQueueSegmentSlot<T> _slot49;
        private NativeConcurrentQueueSegmentSlot<T> _slot50;
        private NativeConcurrentQueueSegmentSlot<T> _slot51;
        private NativeConcurrentQueueSegmentSlot<T> _slot52;
        private NativeConcurrentQueueSegmentSlot<T> _slot53;
        private NativeConcurrentQueueSegmentSlot<T> _slot54;
        private NativeConcurrentQueueSegmentSlot<T> _slot55;
        private NativeConcurrentQueueSegmentSlot<T> _slot56;
        private NativeConcurrentQueueSegmentSlot<T> _slot57;
        private NativeConcurrentQueueSegmentSlot<T> _slot58;
        private NativeConcurrentQueueSegmentSlot<T> _slot59;
        private NativeConcurrentQueueSegmentSlot<T> _slot60;
        private NativeConcurrentQueueSegmentSlot<T> _slot61;
        private NativeConcurrentQueueSegmentSlot<T> _slot62;
        private NativeConcurrentQueueSegmentSlot<T> _slot63;
        private NativeConcurrentQueueSegmentSlot<T> _slot64;
        private NativeConcurrentQueueSegmentSlot<T> _slot65;
        private NativeConcurrentQueueSegmentSlot<T> _slot66;
        private NativeConcurrentQueueSegmentSlot<T> _slot67;
        private NativeConcurrentQueueSegmentSlot<T> _slot68;
        private NativeConcurrentQueueSegmentSlot<T> _slot69;
        private NativeConcurrentQueueSegmentSlot<T> _slot70;
        private NativeConcurrentQueueSegmentSlot<T> _slot71;
        private NativeConcurrentQueueSegmentSlot<T> _slot72;
        private NativeConcurrentQueueSegmentSlot<T> _slot73;
        private NativeConcurrentQueueSegmentSlot<T> _slot74;
        private NativeConcurrentQueueSegmentSlot<T> _slot75;
        private NativeConcurrentQueueSegmentSlot<T> _slot76;
        private NativeConcurrentQueueSegmentSlot<T> _slot77;
        private NativeConcurrentQueueSegmentSlot<T> _slot78;
        private NativeConcurrentQueueSegmentSlot<T> _slot79;
        private NativeConcurrentQueueSegmentSlot<T> _slot80;
        private NativeConcurrentQueueSegmentSlot<T> _slot81;
        private NativeConcurrentQueueSegmentSlot<T> _slot82;
        private NativeConcurrentQueueSegmentSlot<T> _slot83;
        private NativeConcurrentQueueSegmentSlot<T> _slot84;
        private NativeConcurrentQueueSegmentSlot<T> _slot85;
        private NativeConcurrentQueueSegmentSlot<T> _slot86;
        private NativeConcurrentQueueSegmentSlot<T> _slot87;
        private NativeConcurrentQueueSegmentSlot<T> _slot88;
        private NativeConcurrentQueueSegmentSlot<T> _slot89;
        private NativeConcurrentQueueSegmentSlot<T> _slot90;
        private NativeConcurrentQueueSegmentSlot<T> _slot91;
        private NativeConcurrentQueueSegmentSlot<T> _slot92;
        private NativeConcurrentQueueSegmentSlot<T> _slot93;
        private NativeConcurrentQueueSegmentSlot<T> _slot94;
        private NativeConcurrentQueueSegmentSlot<T> _slot95;
        private NativeConcurrentQueueSegmentSlot<T> _slot96;
        private NativeConcurrentQueueSegmentSlot<T> _slot97;
        private NativeConcurrentQueueSegmentSlot<T> _slot98;
        private NativeConcurrentQueueSegmentSlot<T> _slot99;
        private NativeConcurrentQueueSegmentSlot<T> _slot100;
        private NativeConcurrentQueueSegmentSlot<T> _slot101;
        private NativeConcurrentQueueSegmentSlot<T> _slot102;
        private NativeConcurrentQueueSegmentSlot<T> _slot103;
        private NativeConcurrentQueueSegmentSlot<T> _slot104;
        private NativeConcurrentQueueSegmentSlot<T> _slot105;
        private NativeConcurrentQueueSegmentSlot<T> _slot106;
        private NativeConcurrentQueueSegmentSlot<T> _slot107;
        private NativeConcurrentQueueSegmentSlot<T> _slot108;
        private NativeConcurrentQueueSegmentSlot<T> _slot109;
        private NativeConcurrentQueueSegmentSlot<T> _slot110;
        private NativeConcurrentQueueSegmentSlot<T> _slot111;
        private NativeConcurrentQueueSegmentSlot<T> _slot112;
        private NativeConcurrentQueueSegmentSlot<T> _slot113;
        private NativeConcurrentQueueSegmentSlot<T> _slot114;
        private NativeConcurrentQueueSegmentSlot<T> _slot115;
        private NativeConcurrentQueueSegmentSlot<T> _slot116;
        private NativeConcurrentQueueSegmentSlot<T> _slot117;
        private NativeConcurrentQueueSegmentSlot<T> _slot118;
        private NativeConcurrentQueueSegmentSlot<T> _slot119;
        private NativeConcurrentQueueSegmentSlot<T> _slot120;
        private NativeConcurrentQueueSegmentSlot<T> _slot121;
        private NativeConcurrentQueueSegmentSlot<T> _slot122;
        private NativeConcurrentQueueSegmentSlot<T> _slot123;
        private NativeConcurrentQueueSegmentSlot<T> _slot124;
        private NativeConcurrentQueueSegmentSlot<T> _slot125;
        private NativeConcurrentQueueSegmentSlot<T> _slot126;
        private NativeConcurrentQueueSegmentSlot<T> _slot127;
        private NativeConcurrentQueueSegmentSlot<T> _slot128;
        private NativeConcurrentQueueSegmentSlot<T> _slot129;
        private NativeConcurrentQueueSegmentSlot<T> _slot130;
        private NativeConcurrentQueueSegmentSlot<T> _slot131;
        private NativeConcurrentQueueSegmentSlot<T> _slot132;
        private NativeConcurrentQueueSegmentSlot<T> _slot133;
        private NativeConcurrentQueueSegmentSlot<T> _slot134;
        private NativeConcurrentQueueSegmentSlot<T> _slot135;
        private NativeConcurrentQueueSegmentSlot<T> _slot136;
        private NativeConcurrentQueueSegmentSlot<T> _slot137;
        private NativeConcurrentQueueSegmentSlot<T> _slot138;
        private NativeConcurrentQueueSegmentSlot<T> _slot139;
        private NativeConcurrentQueueSegmentSlot<T> _slot140;
        private NativeConcurrentQueueSegmentSlot<T> _slot141;
        private NativeConcurrentQueueSegmentSlot<T> _slot142;
        private NativeConcurrentQueueSegmentSlot<T> _slot143;
        private NativeConcurrentQueueSegmentSlot<T> _slot144;
        private NativeConcurrentQueueSegmentSlot<T> _slot145;
        private NativeConcurrentQueueSegmentSlot<T> _slot146;
        private NativeConcurrentQueueSegmentSlot<T> _slot147;
        private NativeConcurrentQueueSegmentSlot<T> _slot148;
        private NativeConcurrentQueueSegmentSlot<T> _slot149;
        private NativeConcurrentQueueSegmentSlot<T> _slot150;
        private NativeConcurrentQueueSegmentSlot<T> _slot151;
        private NativeConcurrentQueueSegmentSlot<T> _slot152;
        private NativeConcurrentQueueSegmentSlot<T> _slot153;
        private NativeConcurrentQueueSegmentSlot<T> _slot154;
        private NativeConcurrentQueueSegmentSlot<T> _slot155;
        private NativeConcurrentQueueSegmentSlot<T> _slot156;
        private NativeConcurrentQueueSegmentSlot<T> _slot157;
        private NativeConcurrentQueueSegmentSlot<T> _slot158;
        private NativeConcurrentQueueSegmentSlot<T> _slot159;
        private NativeConcurrentQueueSegmentSlot<T> _slot160;
        private NativeConcurrentQueueSegmentSlot<T> _slot161;
        private NativeConcurrentQueueSegmentSlot<T> _slot162;
        private NativeConcurrentQueueSegmentSlot<T> _slot163;
        private NativeConcurrentQueueSegmentSlot<T> _slot164;
        private NativeConcurrentQueueSegmentSlot<T> _slot165;
        private NativeConcurrentQueueSegmentSlot<T> _slot166;
        private NativeConcurrentQueueSegmentSlot<T> _slot167;
        private NativeConcurrentQueueSegmentSlot<T> _slot168;
        private NativeConcurrentQueueSegmentSlot<T> _slot169;
        private NativeConcurrentQueueSegmentSlot<T> _slot170;
        private NativeConcurrentQueueSegmentSlot<T> _slot171;
        private NativeConcurrentQueueSegmentSlot<T> _slot172;
        private NativeConcurrentQueueSegmentSlot<T> _slot173;
        private NativeConcurrentQueueSegmentSlot<T> _slot174;
        private NativeConcurrentQueueSegmentSlot<T> _slot175;
        private NativeConcurrentQueueSegmentSlot<T> _slot176;
        private NativeConcurrentQueueSegmentSlot<T> _slot177;
        private NativeConcurrentQueueSegmentSlot<T> _slot178;
        private NativeConcurrentQueueSegmentSlot<T> _slot179;
        private NativeConcurrentQueueSegmentSlot<T> _slot180;
        private NativeConcurrentQueueSegmentSlot<T> _slot181;
        private NativeConcurrentQueueSegmentSlot<T> _slot182;
        private NativeConcurrentQueueSegmentSlot<T> _slot183;
        private NativeConcurrentQueueSegmentSlot<T> _slot184;
        private NativeConcurrentQueueSegmentSlot<T> _slot185;
        private NativeConcurrentQueueSegmentSlot<T> _slot186;
        private NativeConcurrentQueueSegmentSlot<T> _slot187;
        private NativeConcurrentQueueSegmentSlot<T> _slot188;
        private NativeConcurrentQueueSegmentSlot<T> _slot189;
        private NativeConcurrentQueueSegmentSlot<T> _slot190;
        private NativeConcurrentQueueSegmentSlot<T> _slot191;
        private NativeConcurrentQueueSegmentSlot<T> _slot192;
        private NativeConcurrentQueueSegmentSlot<T> _slot193;
        private NativeConcurrentQueueSegmentSlot<T> _slot194;
        private NativeConcurrentQueueSegmentSlot<T> _slot195;
        private NativeConcurrentQueueSegmentSlot<T> _slot196;
        private NativeConcurrentQueueSegmentSlot<T> _slot197;
        private NativeConcurrentQueueSegmentSlot<T> _slot198;
        private NativeConcurrentQueueSegmentSlot<T> _slot199;
        private NativeConcurrentQueueSegmentSlot<T> _slot200;
        private NativeConcurrentQueueSegmentSlot<T> _slot201;
        private NativeConcurrentQueueSegmentSlot<T> _slot202;
        private NativeConcurrentQueueSegmentSlot<T> _slot203;
        private NativeConcurrentQueueSegmentSlot<T> _slot204;
        private NativeConcurrentQueueSegmentSlot<T> _slot205;
        private NativeConcurrentQueueSegmentSlot<T> _slot206;
        private NativeConcurrentQueueSegmentSlot<T> _slot207;
        private NativeConcurrentQueueSegmentSlot<T> _slot208;
        private NativeConcurrentQueueSegmentSlot<T> _slot209;
        private NativeConcurrentQueueSegmentSlot<T> _slot210;
        private NativeConcurrentQueueSegmentSlot<T> _slot211;
        private NativeConcurrentQueueSegmentSlot<T> _slot212;
        private NativeConcurrentQueueSegmentSlot<T> _slot213;
        private NativeConcurrentQueueSegmentSlot<T> _slot214;
        private NativeConcurrentQueueSegmentSlot<T> _slot215;
        private NativeConcurrentQueueSegmentSlot<T> _slot216;
        private NativeConcurrentQueueSegmentSlot<T> _slot217;
        private NativeConcurrentQueueSegmentSlot<T> _slot218;
        private NativeConcurrentQueueSegmentSlot<T> _slot219;
        private NativeConcurrentQueueSegmentSlot<T> _slot220;
        private NativeConcurrentQueueSegmentSlot<T> _slot221;
        private NativeConcurrentQueueSegmentSlot<T> _slot222;
        private NativeConcurrentQueueSegmentSlot<T> _slot223;
        private NativeConcurrentQueueSegmentSlot<T> _slot224;
        private NativeConcurrentQueueSegmentSlot<T> _slot225;
        private NativeConcurrentQueueSegmentSlot<T> _slot226;
        private NativeConcurrentQueueSegmentSlot<T> _slot227;
        private NativeConcurrentQueueSegmentSlot<T> _slot228;
        private NativeConcurrentQueueSegmentSlot<T> _slot229;
        private NativeConcurrentQueueSegmentSlot<T> _slot230;
        private NativeConcurrentQueueSegmentSlot<T> _slot231;
        private NativeConcurrentQueueSegmentSlot<T> _slot232;
        private NativeConcurrentQueueSegmentSlot<T> _slot233;
        private NativeConcurrentQueueSegmentSlot<T> _slot234;
        private NativeConcurrentQueueSegmentSlot<T> _slot235;
        private NativeConcurrentQueueSegmentSlot<T> _slot236;
        private NativeConcurrentQueueSegmentSlot<T> _slot237;
        private NativeConcurrentQueueSegmentSlot<T> _slot238;
        private NativeConcurrentQueueSegmentSlot<T> _slot239;
        private NativeConcurrentQueueSegmentSlot<T> _slot240;
        private NativeConcurrentQueueSegmentSlot<T> _slot241;
        private NativeConcurrentQueueSegmentSlot<T> _slot242;
        private NativeConcurrentQueueSegmentSlot<T> _slot243;
        private NativeConcurrentQueueSegmentSlot<T> _slot244;
        private NativeConcurrentQueueSegmentSlot<T> _slot245;
        private NativeConcurrentQueueSegmentSlot<T> _slot246;
        private NativeConcurrentQueueSegmentSlot<T> _slot247;
        private NativeConcurrentQueueSegmentSlot<T> _slot248;
        private NativeConcurrentQueueSegmentSlot<T> _slot249;
        private NativeConcurrentQueueSegmentSlot<T> _slot250;
        private NativeConcurrentQueueSegmentSlot<T> _slot251;
        private NativeConcurrentQueueSegmentSlot<T> _slot252;
        private NativeConcurrentQueueSegmentSlot<T> _slot253;
        private NativeConcurrentQueueSegmentSlot<T> _slot254;
        private NativeConcurrentQueueSegmentSlot<T> _slot255;
        private NativeConcurrentQueueSegmentSlot<T> _slot256;
        private NativeConcurrentQueueSegmentSlot<T> _slot257;
        private NativeConcurrentQueueSegmentSlot<T> _slot258;
        private NativeConcurrentQueueSegmentSlot<T> _slot259;
        private NativeConcurrentQueueSegmentSlot<T> _slot260;
        private NativeConcurrentQueueSegmentSlot<T> _slot261;
        private NativeConcurrentQueueSegmentSlot<T> _slot262;
        private NativeConcurrentQueueSegmentSlot<T> _slot263;
        private NativeConcurrentQueueSegmentSlot<T> _slot264;
        private NativeConcurrentQueueSegmentSlot<T> _slot265;
        private NativeConcurrentQueueSegmentSlot<T> _slot266;
        private NativeConcurrentQueueSegmentSlot<T> _slot267;
        private NativeConcurrentQueueSegmentSlot<T> _slot268;
        private NativeConcurrentQueueSegmentSlot<T> _slot269;
        private NativeConcurrentQueueSegmentSlot<T> _slot270;
        private NativeConcurrentQueueSegmentSlot<T> _slot271;
        private NativeConcurrentQueueSegmentSlot<T> _slot272;
        private NativeConcurrentQueueSegmentSlot<T> _slot273;
        private NativeConcurrentQueueSegmentSlot<T> _slot274;
        private NativeConcurrentQueueSegmentSlot<T> _slot275;
        private NativeConcurrentQueueSegmentSlot<T> _slot276;
        private NativeConcurrentQueueSegmentSlot<T> _slot277;
        private NativeConcurrentQueueSegmentSlot<T> _slot278;
        private NativeConcurrentQueueSegmentSlot<T> _slot279;
        private NativeConcurrentQueueSegmentSlot<T> _slot280;
        private NativeConcurrentQueueSegmentSlot<T> _slot281;
        private NativeConcurrentQueueSegmentSlot<T> _slot282;
        private NativeConcurrentQueueSegmentSlot<T> _slot283;
        private NativeConcurrentQueueSegmentSlot<T> _slot284;
        private NativeConcurrentQueueSegmentSlot<T> _slot285;
        private NativeConcurrentQueueSegmentSlot<T> _slot286;
        private NativeConcurrentQueueSegmentSlot<T> _slot287;
        private NativeConcurrentQueueSegmentSlot<T> _slot288;
        private NativeConcurrentQueueSegmentSlot<T> _slot289;
        private NativeConcurrentQueueSegmentSlot<T> _slot290;
        private NativeConcurrentQueueSegmentSlot<T> _slot291;
        private NativeConcurrentQueueSegmentSlot<T> _slot292;
        private NativeConcurrentQueueSegmentSlot<T> _slot293;
        private NativeConcurrentQueueSegmentSlot<T> _slot294;
        private NativeConcurrentQueueSegmentSlot<T> _slot295;
        private NativeConcurrentQueueSegmentSlot<T> _slot296;
        private NativeConcurrentQueueSegmentSlot<T> _slot297;
        private NativeConcurrentQueueSegmentSlot<T> _slot298;
        private NativeConcurrentQueueSegmentSlot<T> _slot299;
        private NativeConcurrentQueueSegmentSlot<T> _slot300;
        private NativeConcurrentQueueSegmentSlot<T> _slot301;
        private NativeConcurrentQueueSegmentSlot<T> _slot302;
        private NativeConcurrentQueueSegmentSlot<T> _slot303;
        private NativeConcurrentQueueSegmentSlot<T> _slot304;
        private NativeConcurrentQueueSegmentSlot<T> _slot305;
        private NativeConcurrentQueueSegmentSlot<T> _slot306;
        private NativeConcurrentQueueSegmentSlot<T> _slot307;
        private NativeConcurrentQueueSegmentSlot<T> _slot308;
        private NativeConcurrentQueueSegmentSlot<T> _slot309;
        private NativeConcurrentQueueSegmentSlot<T> _slot310;
        private NativeConcurrentQueueSegmentSlot<T> _slot311;
        private NativeConcurrentQueueSegmentSlot<T> _slot312;
        private NativeConcurrentQueueSegmentSlot<T> _slot313;
        private NativeConcurrentQueueSegmentSlot<T> _slot314;
        private NativeConcurrentQueueSegmentSlot<T> _slot315;
        private NativeConcurrentQueueSegmentSlot<T> _slot316;
        private NativeConcurrentQueueSegmentSlot<T> _slot317;
        private NativeConcurrentQueueSegmentSlot<T> _slot318;
        private NativeConcurrentQueueSegmentSlot<T> _slot319;
        private NativeConcurrentQueueSegmentSlot<T> _slot320;
        private NativeConcurrentQueueSegmentSlot<T> _slot321;
        private NativeConcurrentQueueSegmentSlot<T> _slot322;
        private NativeConcurrentQueueSegmentSlot<T> _slot323;
        private NativeConcurrentQueueSegmentSlot<T> _slot324;
        private NativeConcurrentQueueSegmentSlot<T> _slot325;
        private NativeConcurrentQueueSegmentSlot<T> _slot326;
        private NativeConcurrentQueueSegmentSlot<T> _slot327;
        private NativeConcurrentQueueSegmentSlot<T> _slot328;
        private NativeConcurrentQueueSegmentSlot<T> _slot329;
        private NativeConcurrentQueueSegmentSlot<T> _slot330;
        private NativeConcurrentQueueSegmentSlot<T> _slot331;
        private NativeConcurrentQueueSegmentSlot<T> _slot332;
        private NativeConcurrentQueueSegmentSlot<T> _slot333;
        private NativeConcurrentQueueSegmentSlot<T> _slot334;
        private NativeConcurrentQueueSegmentSlot<T> _slot335;
        private NativeConcurrentQueueSegmentSlot<T> _slot336;
        private NativeConcurrentQueueSegmentSlot<T> _slot337;
        private NativeConcurrentQueueSegmentSlot<T> _slot338;
        private NativeConcurrentQueueSegmentSlot<T> _slot339;
        private NativeConcurrentQueueSegmentSlot<T> _slot340;
        private NativeConcurrentQueueSegmentSlot<T> _slot341;
        private NativeConcurrentQueueSegmentSlot<T> _slot342;
        private NativeConcurrentQueueSegmentSlot<T> _slot343;
        private NativeConcurrentQueueSegmentSlot<T> _slot344;
        private NativeConcurrentQueueSegmentSlot<T> _slot345;
        private NativeConcurrentQueueSegmentSlot<T> _slot346;
        private NativeConcurrentQueueSegmentSlot<T> _slot347;
        private NativeConcurrentQueueSegmentSlot<T> _slot348;
        private NativeConcurrentQueueSegmentSlot<T> _slot349;
        private NativeConcurrentQueueSegmentSlot<T> _slot350;
        private NativeConcurrentQueueSegmentSlot<T> _slot351;
        private NativeConcurrentQueueSegmentSlot<T> _slot352;
        private NativeConcurrentQueueSegmentSlot<T> _slot353;
        private NativeConcurrentQueueSegmentSlot<T> _slot354;
        private NativeConcurrentQueueSegmentSlot<T> _slot355;
        private NativeConcurrentQueueSegmentSlot<T> _slot356;
        private NativeConcurrentQueueSegmentSlot<T> _slot357;
        private NativeConcurrentQueueSegmentSlot<T> _slot358;
        private NativeConcurrentQueueSegmentSlot<T> _slot359;
        private NativeConcurrentQueueSegmentSlot<T> _slot360;
        private NativeConcurrentQueueSegmentSlot<T> _slot361;
        private NativeConcurrentQueueSegmentSlot<T> _slot362;
        private NativeConcurrentQueueSegmentSlot<T> _slot363;
        private NativeConcurrentQueueSegmentSlot<T> _slot364;
        private NativeConcurrentQueueSegmentSlot<T> _slot365;
        private NativeConcurrentQueueSegmentSlot<T> _slot366;
        private NativeConcurrentQueueSegmentSlot<T> _slot367;
        private NativeConcurrentQueueSegmentSlot<T> _slot368;
        private NativeConcurrentQueueSegmentSlot<T> _slot369;
        private NativeConcurrentQueueSegmentSlot<T> _slot370;
        private NativeConcurrentQueueSegmentSlot<T> _slot371;
        private NativeConcurrentQueueSegmentSlot<T> _slot372;
        private NativeConcurrentQueueSegmentSlot<T> _slot373;
        private NativeConcurrentQueueSegmentSlot<T> _slot374;
        private NativeConcurrentQueueSegmentSlot<T> _slot375;
        private NativeConcurrentQueueSegmentSlot<T> _slot376;
        private NativeConcurrentQueueSegmentSlot<T> _slot377;
        private NativeConcurrentQueueSegmentSlot<T> _slot378;
        private NativeConcurrentQueueSegmentSlot<T> _slot379;
        private NativeConcurrentQueueSegmentSlot<T> _slot380;
        private NativeConcurrentQueueSegmentSlot<T> _slot381;
        private NativeConcurrentQueueSegmentSlot<T> _slot382;
        private NativeConcurrentQueueSegmentSlot<T> _slot383;
        private NativeConcurrentQueueSegmentSlot<T> _slot384;
        private NativeConcurrentQueueSegmentSlot<T> _slot385;
        private NativeConcurrentQueueSegmentSlot<T> _slot386;
        private NativeConcurrentQueueSegmentSlot<T> _slot387;
        private NativeConcurrentQueueSegmentSlot<T> _slot388;
        private NativeConcurrentQueueSegmentSlot<T> _slot389;
        private NativeConcurrentQueueSegmentSlot<T> _slot390;
        private NativeConcurrentQueueSegmentSlot<T> _slot391;
        private NativeConcurrentQueueSegmentSlot<T> _slot392;
        private NativeConcurrentQueueSegmentSlot<T> _slot393;
        private NativeConcurrentQueueSegmentSlot<T> _slot394;
        private NativeConcurrentQueueSegmentSlot<T> _slot395;
        private NativeConcurrentQueueSegmentSlot<T> _slot396;
        private NativeConcurrentQueueSegmentSlot<T> _slot397;
        private NativeConcurrentQueueSegmentSlot<T> _slot398;
        private NativeConcurrentQueueSegmentSlot<T> _slot399;
        private NativeConcurrentQueueSegmentSlot<T> _slot400;
        private NativeConcurrentQueueSegmentSlot<T> _slot401;
        private NativeConcurrentQueueSegmentSlot<T> _slot402;
        private NativeConcurrentQueueSegmentSlot<T> _slot403;
        private NativeConcurrentQueueSegmentSlot<T> _slot404;
        private NativeConcurrentQueueSegmentSlot<T> _slot405;
        private NativeConcurrentQueueSegmentSlot<T> _slot406;
        private NativeConcurrentQueueSegmentSlot<T> _slot407;
        private NativeConcurrentQueueSegmentSlot<T> _slot408;
        private NativeConcurrentQueueSegmentSlot<T> _slot409;
        private NativeConcurrentQueueSegmentSlot<T> _slot410;
        private NativeConcurrentQueueSegmentSlot<T> _slot411;
        private NativeConcurrentQueueSegmentSlot<T> _slot412;
        private NativeConcurrentQueueSegmentSlot<T> _slot413;
        private NativeConcurrentQueueSegmentSlot<T> _slot414;
        private NativeConcurrentQueueSegmentSlot<T> _slot415;
        private NativeConcurrentQueueSegmentSlot<T> _slot416;
        private NativeConcurrentQueueSegmentSlot<T> _slot417;
        private NativeConcurrentQueueSegmentSlot<T> _slot418;
        private NativeConcurrentQueueSegmentSlot<T> _slot419;
        private NativeConcurrentQueueSegmentSlot<T> _slot420;
        private NativeConcurrentQueueSegmentSlot<T> _slot421;
        private NativeConcurrentQueueSegmentSlot<T> _slot422;
        private NativeConcurrentQueueSegmentSlot<T> _slot423;
        private NativeConcurrentQueueSegmentSlot<T> _slot424;
        private NativeConcurrentQueueSegmentSlot<T> _slot425;
        private NativeConcurrentQueueSegmentSlot<T> _slot426;
        private NativeConcurrentQueueSegmentSlot<T> _slot427;
        private NativeConcurrentQueueSegmentSlot<T> _slot428;
        private NativeConcurrentQueueSegmentSlot<T> _slot429;
        private NativeConcurrentQueueSegmentSlot<T> _slot430;
        private NativeConcurrentQueueSegmentSlot<T> _slot431;
        private NativeConcurrentQueueSegmentSlot<T> _slot432;
        private NativeConcurrentQueueSegmentSlot<T> _slot433;
        private NativeConcurrentQueueSegmentSlot<T> _slot434;
        private NativeConcurrentQueueSegmentSlot<T> _slot435;
        private NativeConcurrentQueueSegmentSlot<T> _slot436;
        private NativeConcurrentQueueSegmentSlot<T> _slot437;
        private NativeConcurrentQueueSegmentSlot<T> _slot438;
        private NativeConcurrentQueueSegmentSlot<T> _slot439;
        private NativeConcurrentQueueSegmentSlot<T> _slot440;
        private NativeConcurrentQueueSegmentSlot<T> _slot441;
        private NativeConcurrentQueueSegmentSlot<T> _slot442;
        private NativeConcurrentQueueSegmentSlot<T> _slot443;
        private NativeConcurrentQueueSegmentSlot<T> _slot444;
        private NativeConcurrentQueueSegmentSlot<T> _slot445;
        private NativeConcurrentQueueSegmentSlot<T> _slot446;
        private NativeConcurrentQueueSegmentSlot<T> _slot447;
        private NativeConcurrentQueueSegmentSlot<T> _slot448;
        private NativeConcurrentQueueSegmentSlot<T> _slot449;
        private NativeConcurrentQueueSegmentSlot<T> _slot450;
        private NativeConcurrentQueueSegmentSlot<T> _slot451;
        private NativeConcurrentQueueSegmentSlot<T> _slot452;
        private NativeConcurrentQueueSegmentSlot<T> _slot453;
        private NativeConcurrentQueueSegmentSlot<T> _slot454;
        private NativeConcurrentQueueSegmentSlot<T> _slot455;
        private NativeConcurrentQueueSegmentSlot<T> _slot456;
        private NativeConcurrentQueueSegmentSlot<T> _slot457;
        private NativeConcurrentQueueSegmentSlot<T> _slot458;
        private NativeConcurrentQueueSegmentSlot<T> _slot459;
        private NativeConcurrentQueueSegmentSlot<T> _slot460;
        private NativeConcurrentQueueSegmentSlot<T> _slot461;
        private NativeConcurrentQueueSegmentSlot<T> _slot462;
        private NativeConcurrentQueueSegmentSlot<T> _slot463;
        private NativeConcurrentQueueSegmentSlot<T> _slot464;
        private NativeConcurrentQueueSegmentSlot<T> _slot465;
        private NativeConcurrentQueueSegmentSlot<T> _slot466;
        private NativeConcurrentQueueSegmentSlot<T> _slot467;
        private NativeConcurrentQueueSegmentSlot<T> _slot468;
        private NativeConcurrentQueueSegmentSlot<T> _slot469;
        private NativeConcurrentQueueSegmentSlot<T> _slot470;
        private NativeConcurrentQueueSegmentSlot<T> _slot471;
        private NativeConcurrentQueueSegmentSlot<T> _slot472;
        private NativeConcurrentQueueSegmentSlot<T> _slot473;
        private NativeConcurrentQueueSegmentSlot<T> _slot474;
        private NativeConcurrentQueueSegmentSlot<T> _slot475;
        private NativeConcurrentQueueSegmentSlot<T> _slot476;
        private NativeConcurrentQueueSegmentSlot<T> _slot477;
        private NativeConcurrentQueueSegmentSlot<T> _slot478;
        private NativeConcurrentQueueSegmentSlot<T> _slot479;
        private NativeConcurrentQueueSegmentSlot<T> _slot480;
        private NativeConcurrentQueueSegmentSlot<T> _slot481;
        private NativeConcurrentQueueSegmentSlot<T> _slot482;
        private NativeConcurrentQueueSegmentSlot<T> _slot483;
        private NativeConcurrentQueueSegmentSlot<T> _slot484;
        private NativeConcurrentQueueSegmentSlot<T> _slot485;
        private NativeConcurrentQueueSegmentSlot<T> _slot486;
        private NativeConcurrentQueueSegmentSlot<T> _slot487;
        private NativeConcurrentQueueSegmentSlot<T> _slot488;
        private NativeConcurrentQueueSegmentSlot<T> _slot489;
        private NativeConcurrentQueueSegmentSlot<T> _slot490;
        private NativeConcurrentQueueSegmentSlot<T> _slot491;
        private NativeConcurrentQueueSegmentSlot<T> _slot492;
        private NativeConcurrentQueueSegmentSlot<T> _slot493;
        private NativeConcurrentQueueSegmentSlot<T> _slot494;
        private NativeConcurrentQueueSegmentSlot<T> _slot495;
        private NativeConcurrentQueueSegmentSlot<T> _slot496;
        private NativeConcurrentQueueSegmentSlot<T> _slot497;
        private NativeConcurrentQueueSegmentSlot<T> _slot498;
        private NativeConcurrentQueueSegmentSlot<T> _slot499;
        private NativeConcurrentQueueSegmentSlot<T> _slot500;
        private NativeConcurrentQueueSegmentSlot<T> _slot501;
        private NativeConcurrentQueueSegmentSlot<T> _slot502;
        private NativeConcurrentQueueSegmentSlot<T> _slot503;
        private NativeConcurrentQueueSegmentSlot<T> _slot504;
        private NativeConcurrentQueueSegmentSlot<T> _slot505;
        private NativeConcurrentQueueSegmentSlot<T> _slot506;
        private NativeConcurrentQueueSegmentSlot<T> _slot507;
        private NativeConcurrentQueueSegmentSlot<T> _slot508;
        private NativeConcurrentQueueSegmentSlot<T> _slot509;
        private NativeConcurrentQueueSegmentSlot<T> _slot510;
        private NativeConcurrentQueueSegmentSlot<T> _slot511;
        private NativeConcurrentQueueSegmentSlot<T> _slot512;
        private NativeConcurrentQueueSegmentSlot<T> _slot513;
        private NativeConcurrentQueueSegmentSlot<T> _slot514;
        private NativeConcurrentQueueSegmentSlot<T> _slot515;
        private NativeConcurrentQueueSegmentSlot<T> _slot516;
        private NativeConcurrentQueueSegmentSlot<T> _slot517;
        private NativeConcurrentQueueSegmentSlot<T> _slot518;
        private NativeConcurrentQueueSegmentSlot<T> _slot519;
        private NativeConcurrentQueueSegmentSlot<T> _slot520;
        private NativeConcurrentQueueSegmentSlot<T> _slot521;
        private NativeConcurrentQueueSegmentSlot<T> _slot522;
        private NativeConcurrentQueueSegmentSlot<T> _slot523;
        private NativeConcurrentQueueSegmentSlot<T> _slot524;
        private NativeConcurrentQueueSegmentSlot<T> _slot525;
        private NativeConcurrentQueueSegmentSlot<T> _slot526;
        private NativeConcurrentQueueSegmentSlot<T> _slot527;
        private NativeConcurrentQueueSegmentSlot<T> _slot528;
        private NativeConcurrentQueueSegmentSlot<T> _slot529;
        private NativeConcurrentQueueSegmentSlot<T> _slot530;
        private NativeConcurrentQueueSegmentSlot<T> _slot531;
        private NativeConcurrentQueueSegmentSlot<T> _slot532;
        private NativeConcurrentQueueSegmentSlot<T> _slot533;
        private NativeConcurrentQueueSegmentSlot<T> _slot534;
        private NativeConcurrentQueueSegmentSlot<T> _slot535;
        private NativeConcurrentQueueSegmentSlot<T> _slot536;
        private NativeConcurrentQueueSegmentSlot<T> _slot537;
        private NativeConcurrentQueueSegmentSlot<T> _slot538;
        private NativeConcurrentQueueSegmentSlot<T> _slot539;
        private NativeConcurrentQueueSegmentSlot<T> _slot540;
        private NativeConcurrentQueueSegmentSlot<T> _slot541;
        private NativeConcurrentQueueSegmentSlot<T> _slot542;
        private NativeConcurrentQueueSegmentSlot<T> _slot543;
        private NativeConcurrentQueueSegmentSlot<T> _slot544;
        private NativeConcurrentQueueSegmentSlot<T> _slot545;
        private NativeConcurrentQueueSegmentSlot<T> _slot546;
        private NativeConcurrentQueueSegmentSlot<T> _slot547;
        private NativeConcurrentQueueSegmentSlot<T> _slot548;
        private NativeConcurrentQueueSegmentSlot<T> _slot549;
        private NativeConcurrentQueueSegmentSlot<T> _slot550;
        private NativeConcurrentQueueSegmentSlot<T> _slot551;
        private NativeConcurrentQueueSegmentSlot<T> _slot552;
        private NativeConcurrentQueueSegmentSlot<T> _slot553;
        private NativeConcurrentQueueSegmentSlot<T> _slot554;
        private NativeConcurrentQueueSegmentSlot<T> _slot555;
        private NativeConcurrentQueueSegmentSlot<T> _slot556;
        private NativeConcurrentQueueSegmentSlot<T> _slot557;
        private NativeConcurrentQueueSegmentSlot<T> _slot558;
        private NativeConcurrentQueueSegmentSlot<T> _slot559;
        private NativeConcurrentQueueSegmentSlot<T> _slot560;
        private NativeConcurrentQueueSegmentSlot<T> _slot561;
        private NativeConcurrentQueueSegmentSlot<T> _slot562;
        private NativeConcurrentQueueSegmentSlot<T> _slot563;
        private NativeConcurrentQueueSegmentSlot<T> _slot564;
        private NativeConcurrentQueueSegmentSlot<T> _slot565;
        private NativeConcurrentQueueSegmentSlot<T> _slot566;
        private NativeConcurrentQueueSegmentSlot<T> _slot567;
        private NativeConcurrentQueueSegmentSlot<T> _slot568;
        private NativeConcurrentQueueSegmentSlot<T> _slot569;
        private NativeConcurrentQueueSegmentSlot<T> _slot570;
        private NativeConcurrentQueueSegmentSlot<T> _slot571;
        private NativeConcurrentQueueSegmentSlot<T> _slot572;
        private NativeConcurrentQueueSegmentSlot<T> _slot573;
        private NativeConcurrentQueueSegmentSlot<T> _slot574;
        private NativeConcurrentQueueSegmentSlot<T> _slot575;
        private NativeConcurrentQueueSegmentSlot<T> _slot576;
        private NativeConcurrentQueueSegmentSlot<T> _slot577;
        private NativeConcurrentQueueSegmentSlot<T> _slot578;
        private NativeConcurrentQueueSegmentSlot<T> _slot579;
        private NativeConcurrentQueueSegmentSlot<T> _slot580;
        private NativeConcurrentQueueSegmentSlot<T> _slot581;
        private NativeConcurrentQueueSegmentSlot<T> _slot582;
        private NativeConcurrentQueueSegmentSlot<T> _slot583;
        private NativeConcurrentQueueSegmentSlot<T> _slot584;
        private NativeConcurrentQueueSegmentSlot<T> _slot585;
        private NativeConcurrentQueueSegmentSlot<T> _slot586;
        private NativeConcurrentQueueSegmentSlot<T> _slot587;
        private NativeConcurrentQueueSegmentSlot<T> _slot588;
        private NativeConcurrentQueueSegmentSlot<T> _slot589;
        private NativeConcurrentQueueSegmentSlot<T> _slot590;
        private NativeConcurrentQueueSegmentSlot<T> _slot591;
        private NativeConcurrentQueueSegmentSlot<T> _slot592;
        private NativeConcurrentQueueSegmentSlot<T> _slot593;
        private NativeConcurrentQueueSegmentSlot<T> _slot594;
        private NativeConcurrentQueueSegmentSlot<T> _slot595;
        private NativeConcurrentQueueSegmentSlot<T> _slot596;
        private NativeConcurrentQueueSegmentSlot<T> _slot597;
        private NativeConcurrentQueueSegmentSlot<T> _slot598;
        private NativeConcurrentQueueSegmentSlot<T> _slot599;
        private NativeConcurrentQueueSegmentSlot<T> _slot600;
        private NativeConcurrentQueueSegmentSlot<T> _slot601;
        private NativeConcurrentQueueSegmentSlot<T> _slot602;
        private NativeConcurrentQueueSegmentSlot<T> _slot603;
        private NativeConcurrentQueueSegmentSlot<T> _slot604;
        private NativeConcurrentQueueSegmentSlot<T> _slot605;
        private NativeConcurrentQueueSegmentSlot<T> _slot606;
        private NativeConcurrentQueueSegmentSlot<T> _slot607;
        private NativeConcurrentQueueSegmentSlot<T> _slot608;
        private NativeConcurrentQueueSegmentSlot<T> _slot609;
        private NativeConcurrentQueueSegmentSlot<T> _slot610;
        private NativeConcurrentQueueSegmentSlot<T> _slot611;
        private NativeConcurrentQueueSegmentSlot<T> _slot612;
        private NativeConcurrentQueueSegmentSlot<T> _slot613;
        private NativeConcurrentQueueSegmentSlot<T> _slot614;
        private NativeConcurrentQueueSegmentSlot<T> _slot615;
        private NativeConcurrentQueueSegmentSlot<T> _slot616;
        private NativeConcurrentQueueSegmentSlot<T> _slot617;
        private NativeConcurrentQueueSegmentSlot<T> _slot618;
        private NativeConcurrentQueueSegmentSlot<T> _slot619;
        private NativeConcurrentQueueSegmentSlot<T> _slot620;
        private NativeConcurrentQueueSegmentSlot<T> _slot621;
        private NativeConcurrentQueueSegmentSlot<T> _slot622;
        private NativeConcurrentQueueSegmentSlot<T> _slot623;
        private NativeConcurrentQueueSegmentSlot<T> _slot624;
        private NativeConcurrentQueueSegmentSlot<T> _slot625;
        private NativeConcurrentQueueSegmentSlot<T> _slot626;
        private NativeConcurrentQueueSegmentSlot<T> _slot627;
        private NativeConcurrentQueueSegmentSlot<T> _slot628;
        private NativeConcurrentQueueSegmentSlot<T> _slot629;
        private NativeConcurrentQueueSegmentSlot<T> _slot630;
        private NativeConcurrentQueueSegmentSlot<T> _slot631;
        private NativeConcurrentQueueSegmentSlot<T> _slot632;
        private NativeConcurrentQueueSegmentSlot<T> _slot633;
        private NativeConcurrentQueueSegmentSlot<T> _slot634;
        private NativeConcurrentQueueSegmentSlot<T> _slot635;
        private NativeConcurrentQueueSegmentSlot<T> _slot636;
        private NativeConcurrentQueueSegmentSlot<T> _slot637;
        private NativeConcurrentQueueSegmentSlot<T> _slot638;
        private NativeConcurrentQueueSegmentSlot<T> _slot639;
        private NativeConcurrentQueueSegmentSlot<T> _slot640;
        private NativeConcurrentQueueSegmentSlot<T> _slot641;
        private NativeConcurrentQueueSegmentSlot<T> _slot642;
        private NativeConcurrentQueueSegmentSlot<T> _slot643;
        private NativeConcurrentQueueSegmentSlot<T> _slot644;
        private NativeConcurrentQueueSegmentSlot<T> _slot645;
        private NativeConcurrentQueueSegmentSlot<T> _slot646;
        private NativeConcurrentQueueSegmentSlot<T> _slot647;
        private NativeConcurrentQueueSegmentSlot<T> _slot648;
        private NativeConcurrentQueueSegmentSlot<T> _slot649;
        private NativeConcurrentQueueSegmentSlot<T> _slot650;
        private NativeConcurrentQueueSegmentSlot<T> _slot651;
        private NativeConcurrentQueueSegmentSlot<T> _slot652;
        private NativeConcurrentQueueSegmentSlot<T> _slot653;
        private NativeConcurrentQueueSegmentSlot<T> _slot654;
        private NativeConcurrentQueueSegmentSlot<T> _slot655;
        private NativeConcurrentQueueSegmentSlot<T> _slot656;
        private NativeConcurrentQueueSegmentSlot<T> _slot657;
        private NativeConcurrentQueueSegmentSlot<T> _slot658;
        private NativeConcurrentQueueSegmentSlot<T> _slot659;
        private NativeConcurrentQueueSegmentSlot<T> _slot660;
        private NativeConcurrentQueueSegmentSlot<T> _slot661;
        private NativeConcurrentQueueSegmentSlot<T> _slot662;
        private NativeConcurrentQueueSegmentSlot<T> _slot663;
        private NativeConcurrentQueueSegmentSlot<T> _slot664;
        private NativeConcurrentQueueSegmentSlot<T> _slot665;
        private NativeConcurrentQueueSegmentSlot<T> _slot666;
        private NativeConcurrentQueueSegmentSlot<T> _slot667;
        private NativeConcurrentQueueSegmentSlot<T> _slot668;
        private NativeConcurrentQueueSegmentSlot<T> _slot669;
        private NativeConcurrentQueueSegmentSlot<T> _slot670;
        private NativeConcurrentQueueSegmentSlot<T> _slot671;
        private NativeConcurrentQueueSegmentSlot<T> _slot672;
        private NativeConcurrentQueueSegmentSlot<T> _slot673;
        private NativeConcurrentQueueSegmentSlot<T> _slot674;
        private NativeConcurrentQueueSegmentSlot<T> _slot675;
        private NativeConcurrentQueueSegmentSlot<T> _slot676;
        private NativeConcurrentQueueSegmentSlot<T> _slot677;
        private NativeConcurrentQueueSegmentSlot<T> _slot678;
        private NativeConcurrentQueueSegmentSlot<T> _slot679;
        private NativeConcurrentQueueSegmentSlot<T> _slot680;
        private NativeConcurrentQueueSegmentSlot<T> _slot681;
        private NativeConcurrentQueueSegmentSlot<T> _slot682;
        private NativeConcurrentQueueSegmentSlot<T> _slot683;
        private NativeConcurrentQueueSegmentSlot<T> _slot684;
        private NativeConcurrentQueueSegmentSlot<T> _slot685;
        private NativeConcurrentQueueSegmentSlot<T> _slot686;
        private NativeConcurrentQueueSegmentSlot<T> _slot687;
        private NativeConcurrentQueueSegmentSlot<T> _slot688;
        private NativeConcurrentQueueSegmentSlot<T> _slot689;
        private NativeConcurrentQueueSegmentSlot<T> _slot690;
        private NativeConcurrentQueueSegmentSlot<T> _slot691;
        private NativeConcurrentQueueSegmentSlot<T> _slot692;
        private NativeConcurrentQueueSegmentSlot<T> _slot693;
        private NativeConcurrentQueueSegmentSlot<T> _slot694;
        private NativeConcurrentQueueSegmentSlot<T> _slot695;
        private NativeConcurrentQueueSegmentSlot<T> _slot696;
        private NativeConcurrentQueueSegmentSlot<T> _slot697;
        private NativeConcurrentQueueSegmentSlot<T> _slot698;
        private NativeConcurrentQueueSegmentSlot<T> _slot699;
        private NativeConcurrentQueueSegmentSlot<T> _slot700;
        private NativeConcurrentQueueSegmentSlot<T> _slot701;
        private NativeConcurrentQueueSegmentSlot<T> _slot702;
        private NativeConcurrentQueueSegmentSlot<T> _slot703;
        private NativeConcurrentQueueSegmentSlot<T> _slot704;
        private NativeConcurrentQueueSegmentSlot<T> _slot705;
        private NativeConcurrentQueueSegmentSlot<T> _slot706;
        private NativeConcurrentQueueSegmentSlot<T> _slot707;
        private NativeConcurrentQueueSegmentSlot<T> _slot708;
        private NativeConcurrentQueueSegmentSlot<T> _slot709;
        private NativeConcurrentQueueSegmentSlot<T> _slot710;
        private NativeConcurrentQueueSegmentSlot<T> _slot711;
        private NativeConcurrentQueueSegmentSlot<T> _slot712;
        private NativeConcurrentQueueSegmentSlot<T> _slot713;
        private NativeConcurrentQueueSegmentSlot<T> _slot714;
        private NativeConcurrentQueueSegmentSlot<T> _slot715;
        private NativeConcurrentQueueSegmentSlot<T> _slot716;
        private NativeConcurrentQueueSegmentSlot<T> _slot717;
        private NativeConcurrentQueueSegmentSlot<T> _slot718;
        private NativeConcurrentQueueSegmentSlot<T> _slot719;
        private NativeConcurrentQueueSegmentSlot<T> _slot720;
        private NativeConcurrentQueueSegmentSlot<T> _slot721;
        private NativeConcurrentQueueSegmentSlot<T> _slot722;
        private NativeConcurrentQueueSegmentSlot<T> _slot723;
        private NativeConcurrentQueueSegmentSlot<T> _slot724;
        private NativeConcurrentQueueSegmentSlot<T> _slot725;
        private NativeConcurrentQueueSegmentSlot<T> _slot726;
        private NativeConcurrentQueueSegmentSlot<T> _slot727;
        private NativeConcurrentQueueSegmentSlot<T> _slot728;
        private NativeConcurrentQueueSegmentSlot<T> _slot729;
        private NativeConcurrentQueueSegmentSlot<T> _slot730;
        private NativeConcurrentQueueSegmentSlot<T> _slot731;
        private NativeConcurrentQueueSegmentSlot<T> _slot732;
        private NativeConcurrentQueueSegmentSlot<T> _slot733;
        private NativeConcurrentQueueSegmentSlot<T> _slot734;
        private NativeConcurrentQueueSegmentSlot<T> _slot735;
        private NativeConcurrentQueueSegmentSlot<T> _slot736;
        private NativeConcurrentQueueSegmentSlot<T> _slot737;
        private NativeConcurrentQueueSegmentSlot<T> _slot738;
        private NativeConcurrentQueueSegmentSlot<T> _slot739;
        private NativeConcurrentQueueSegmentSlot<T> _slot740;
        private NativeConcurrentQueueSegmentSlot<T> _slot741;
        private NativeConcurrentQueueSegmentSlot<T> _slot742;
        private NativeConcurrentQueueSegmentSlot<T> _slot743;
        private NativeConcurrentQueueSegmentSlot<T> _slot744;
        private NativeConcurrentQueueSegmentSlot<T> _slot745;
        private NativeConcurrentQueueSegmentSlot<T> _slot746;
        private NativeConcurrentQueueSegmentSlot<T> _slot747;
        private NativeConcurrentQueueSegmentSlot<T> _slot748;
        private NativeConcurrentQueueSegmentSlot<T> _slot749;
        private NativeConcurrentQueueSegmentSlot<T> _slot750;
        private NativeConcurrentQueueSegmentSlot<T> _slot751;
        private NativeConcurrentQueueSegmentSlot<T> _slot752;
        private NativeConcurrentQueueSegmentSlot<T> _slot753;
        private NativeConcurrentQueueSegmentSlot<T> _slot754;
        private NativeConcurrentQueueSegmentSlot<T> _slot755;
        private NativeConcurrentQueueSegmentSlot<T> _slot756;
        private NativeConcurrentQueueSegmentSlot<T> _slot757;
        private NativeConcurrentQueueSegmentSlot<T> _slot758;
        private NativeConcurrentQueueSegmentSlot<T> _slot759;
        private NativeConcurrentQueueSegmentSlot<T> _slot760;
        private NativeConcurrentQueueSegmentSlot<T> _slot761;
        private NativeConcurrentQueueSegmentSlot<T> _slot762;
        private NativeConcurrentQueueSegmentSlot<T> _slot763;
        private NativeConcurrentQueueSegmentSlot<T> _slot764;
        private NativeConcurrentQueueSegmentSlot<T> _slot765;
        private NativeConcurrentQueueSegmentSlot<T> _slot766;
        private NativeConcurrentQueueSegmentSlot<T> _slot767;
        private NativeConcurrentQueueSegmentSlot<T> _slot768;
        private NativeConcurrentQueueSegmentSlot<T> _slot769;
        private NativeConcurrentQueueSegmentSlot<T> _slot770;
        private NativeConcurrentQueueSegmentSlot<T> _slot771;
        private NativeConcurrentQueueSegmentSlot<T> _slot772;
        private NativeConcurrentQueueSegmentSlot<T> _slot773;
        private NativeConcurrentQueueSegmentSlot<T> _slot774;
        private NativeConcurrentQueueSegmentSlot<T> _slot775;
        private NativeConcurrentQueueSegmentSlot<T> _slot776;
        private NativeConcurrentQueueSegmentSlot<T> _slot777;
        private NativeConcurrentQueueSegmentSlot<T> _slot778;
        private NativeConcurrentQueueSegmentSlot<T> _slot779;
        private NativeConcurrentQueueSegmentSlot<T> _slot780;
        private NativeConcurrentQueueSegmentSlot<T> _slot781;
        private NativeConcurrentQueueSegmentSlot<T> _slot782;
        private NativeConcurrentQueueSegmentSlot<T> _slot783;
        private NativeConcurrentQueueSegmentSlot<T> _slot784;
        private NativeConcurrentQueueSegmentSlot<T> _slot785;
        private NativeConcurrentQueueSegmentSlot<T> _slot786;
        private NativeConcurrentQueueSegmentSlot<T> _slot787;
        private NativeConcurrentQueueSegmentSlot<T> _slot788;
        private NativeConcurrentQueueSegmentSlot<T> _slot789;
        private NativeConcurrentQueueSegmentSlot<T> _slot790;
        private NativeConcurrentQueueSegmentSlot<T> _slot791;
        private NativeConcurrentQueueSegmentSlot<T> _slot792;
        private NativeConcurrentQueueSegmentSlot<T> _slot793;
        private NativeConcurrentQueueSegmentSlot<T> _slot794;
        private NativeConcurrentQueueSegmentSlot<T> _slot795;
        private NativeConcurrentQueueSegmentSlot<T> _slot796;
        private NativeConcurrentQueueSegmentSlot<T> _slot797;
        private NativeConcurrentQueueSegmentSlot<T> _slot798;
        private NativeConcurrentQueueSegmentSlot<T> _slot799;
        private NativeConcurrentQueueSegmentSlot<T> _slot800;
        private NativeConcurrentQueueSegmentSlot<T> _slot801;
        private NativeConcurrentQueueSegmentSlot<T> _slot802;
        private NativeConcurrentQueueSegmentSlot<T> _slot803;
        private NativeConcurrentQueueSegmentSlot<T> _slot804;
        private NativeConcurrentQueueSegmentSlot<T> _slot805;
        private NativeConcurrentQueueSegmentSlot<T> _slot806;
        private NativeConcurrentQueueSegmentSlot<T> _slot807;
        private NativeConcurrentQueueSegmentSlot<T> _slot808;
        private NativeConcurrentQueueSegmentSlot<T> _slot809;
        private NativeConcurrentQueueSegmentSlot<T> _slot810;
        private NativeConcurrentQueueSegmentSlot<T> _slot811;
        private NativeConcurrentQueueSegmentSlot<T> _slot812;
        private NativeConcurrentQueueSegmentSlot<T> _slot813;
        private NativeConcurrentQueueSegmentSlot<T> _slot814;
        private NativeConcurrentQueueSegmentSlot<T> _slot815;
        private NativeConcurrentQueueSegmentSlot<T> _slot816;
        private NativeConcurrentQueueSegmentSlot<T> _slot817;
        private NativeConcurrentQueueSegmentSlot<T> _slot818;
        private NativeConcurrentQueueSegmentSlot<T> _slot819;
        private NativeConcurrentQueueSegmentSlot<T> _slot820;
        private NativeConcurrentQueueSegmentSlot<T> _slot821;
        private NativeConcurrentQueueSegmentSlot<T> _slot822;
        private NativeConcurrentQueueSegmentSlot<T> _slot823;
        private NativeConcurrentQueueSegmentSlot<T> _slot824;
        private NativeConcurrentQueueSegmentSlot<T> _slot825;
        private NativeConcurrentQueueSegmentSlot<T> _slot826;
        private NativeConcurrentQueueSegmentSlot<T> _slot827;
        private NativeConcurrentQueueSegmentSlot<T> _slot828;
        private NativeConcurrentQueueSegmentSlot<T> _slot829;
        private NativeConcurrentQueueSegmentSlot<T> _slot830;
        private NativeConcurrentQueueSegmentSlot<T> _slot831;
        private NativeConcurrentQueueSegmentSlot<T> _slot832;
        private NativeConcurrentQueueSegmentSlot<T> _slot833;
        private NativeConcurrentQueueSegmentSlot<T> _slot834;
        private NativeConcurrentQueueSegmentSlot<T> _slot835;
        private NativeConcurrentQueueSegmentSlot<T> _slot836;
        private NativeConcurrentQueueSegmentSlot<T> _slot837;
        private NativeConcurrentQueueSegmentSlot<T> _slot838;
        private NativeConcurrentQueueSegmentSlot<T> _slot839;
        private NativeConcurrentQueueSegmentSlot<T> _slot840;
        private NativeConcurrentQueueSegmentSlot<T> _slot841;
        private NativeConcurrentQueueSegmentSlot<T> _slot842;
        private NativeConcurrentQueueSegmentSlot<T> _slot843;
        private NativeConcurrentQueueSegmentSlot<T> _slot844;
        private NativeConcurrentQueueSegmentSlot<T> _slot845;
        private NativeConcurrentQueueSegmentSlot<T> _slot846;
        private NativeConcurrentQueueSegmentSlot<T> _slot847;
        private NativeConcurrentQueueSegmentSlot<T> _slot848;
        private NativeConcurrentQueueSegmentSlot<T> _slot849;
        private NativeConcurrentQueueSegmentSlot<T> _slot850;
        private NativeConcurrentQueueSegmentSlot<T> _slot851;
        private NativeConcurrentQueueSegmentSlot<T> _slot852;
        private NativeConcurrentQueueSegmentSlot<T> _slot853;
        private NativeConcurrentQueueSegmentSlot<T> _slot854;
        private NativeConcurrentQueueSegmentSlot<T> _slot855;
        private NativeConcurrentQueueSegmentSlot<T> _slot856;
        private NativeConcurrentQueueSegmentSlot<T> _slot857;
        private NativeConcurrentQueueSegmentSlot<T> _slot858;
        private NativeConcurrentQueueSegmentSlot<T> _slot859;
        private NativeConcurrentQueueSegmentSlot<T> _slot860;
        private NativeConcurrentQueueSegmentSlot<T> _slot861;
        private NativeConcurrentQueueSegmentSlot<T> _slot862;
        private NativeConcurrentQueueSegmentSlot<T> _slot863;
        private NativeConcurrentQueueSegmentSlot<T> _slot864;
        private NativeConcurrentQueueSegmentSlot<T> _slot865;
        private NativeConcurrentQueueSegmentSlot<T> _slot866;
        private NativeConcurrentQueueSegmentSlot<T> _slot867;
        private NativeConcurrentQueueSegmentSlot<T> _slot868;
        private NativeConcurrentQueueSegmentSlot<T> _slot869;
        private NativeConcurrentQueueSegmentSlot<T> _slot870;
        private NativeConcurrentQueueSegmentSlot<T> _slot871;
        private NativeConcurrentQueueSegmentSlot<T> _slot872;
        private NativeConcurrentQueueSegmentSlot<T> _slot873;
        private NativeConcurrentQueueSegmentSlot<T> _slot874;
        private NativeConcurrentQueueSegmentSlot<T> _slot875;
        private NativeConcurrentQueueSegmentSlot<T> _slot876;
        private NativeConcurrentQueueSegmentSlot<T> _slot877;
        private NativeConcurrentQueueSegmentSlot<T> _slot878;
        private NativeConcurrentQueueSegmentSlot<T> _slot879;
        private NativeConcurrentQueueSegmentSlot<T> _slot880;
        private NativeConcurrentQueueSegmentSlot<T> _slot881;
        private NativeConcurrentQueueSegmentSlot<T> _slot882;
        private NativeConcurrentQueueSegmentSlot<T> _slot883;
        private NativeConcurrentQueueSegmentSlot<T> _slot884;
        private NativeConcurrentQueueSegmentSlot<T> _slot885;
        private NativeConcurrentQueueSegmentSlot<T> _slot886;
        private NativeConcurrentQueueSegmentSlot<T> _slot887;
        private NativeConcurrentQueueSegmentSlot<T> _slot888;
        private NativeConcurrentQueueSegmentSlot<T> _slot889;
        private NativeConcurrentQueueSegmentSlot<T> _slot890;
        private NativeConcurrentQueueSegmentSlot<T> _slot891;
        private NativeConcurrentQueueSegmentSlot<T> _slot892;
        private NativeConcurrentQueueSegmentSlot<T> _slot893;
        private NativeConcurrentQueueSegmentSlot<T> _slot894;
        private NativeConcurrentQueueSegmentSlot<T> _slot895;
        private NativeConcurrentQueueSegmentSlot<T> _slot896;
        private NativeConcurrentQueueSegmentSlot<T> _slot897;
        private NativeConcurrentQueueSegmentSlot<T> _slot898;
        private NativeConcurrentQueueSegmentSlot<T> _slot899;
        private NativeConcurrentQueueSegmentSlot<T> _slot900;
        private NativeConcurrentQueueSegmentSlot<T> _slot901;
        private NativeConcurrentQueueSegmentSlot<T> _slot902;
        private NativeConcurrentQueueSegmentSlot<T> _slot903;
        private NativeConcurrentQueueSegmentSlot<T> _slot904;
        private NativeConcurrentQueueSegmentSlot<T> _slot905;
        private NativeConcurrentQueueSegmentSlot<T> _slot906;
        private NativeConcurrentQueueSegmentSlot<T> _slot907;
        private NativeConcurrentQueueSegmentSlot<T> _slot908;
        private NativeConcurrentQueueSegmentSlot<T> _slot909;
        private NativeConcurrentQueueSegmentSlot<T> _slot910;
        private NativeConcurrentQueueSegmentSlot<T> _slot911;
        private NativeConcurrentQueueSegmentSlot<T> _slot912;
        private NativeConcurrentQueueSegmentSlot<T> _slot913;
        private NativeConcurrentQueueSegmentSlot<T> _slot914;
        private NativeConcurrentQueueSegmentSlot<T> _slot915;
        private NativeConcurrentQueueSegmentSlot<T> _slot916;
        private NativeConcurrentQueueSegmentSlot<T> _slot917;
        private NativeConcurrentQueueSegmentSlot<T> _slot918;
        private NativeConcurrentQueueSegmentSlot<T> _slot919;
        private NativeConcurrentQueueSegmentSlot<T> _slot920;
        private NativeConcurrentQueueSegmentSlot<T> _slot921;
        private NativeConcurrentQueueSegmentSlot<T> _slot922;
        private NativeConcurrentQueueSegmentSlot<T> _slot923;
        private NativeConcurrentQueueSegmentSlot<T> _slot924;
        private NativeConcurrentQueueSegmentSlot<T> _slot925;
        private NativeConcurrentQueueSegmentSlot<T> _slot926;
        private NativeConcurrentQueueSegmentSlot<T> _slot927;
        private NativeConcurrentQueueSegmentSlot<T> _slot928;
        private NativeConcurrentQueueSegmentSlot<T> _slot929;
        private NativeConcurrentQueueSegmentSlot<T> _slot930;
        private NativeConcurrentQueueSegmentSlot<T> _slot931;
        private NativeConcurrentQueueSegmentSlot<T> _slot932;
        private NativeConcurrentQueueSegmentSlot<T> _slot933;
        private NativeConcurrentQueueSegmentSlot<T> _slot934;
        private NativeConcurrentQueueSegmentSlot<T> _slot935;
        private NativeConcurrentQueueSegmentSlot<T> _slot936;
        private NativeConcurrentQueueSegmentSlot<T> _slot937;
        private NativeConcurrentQueueSegmentSlot<T> _slot938;
        private NativeConcurrentQueueSegmentSlot<T> _slot939;
        private NativeConcurrentQueueSegmentSlot<T> _slot940;
        private NativeConcurrentQueueSegmentSlot<T> _slot941;
        private NativeConcurrentQueueSegmentSlot<T> _slot942;
        private NativeConcurrentQueueSegmentSlot<T> _slot943;
        private NativeConcurrentQueueSegmentSlot<T> _slot944;
        private NativeConcurrentQueueSegmentSlot<T> _slot945;
        private NativeConcurrentQueueSegmentSlot<T> _slot946;
        private NativeConcurrentQueueSegmentSlot<T> _slot947;
        private NativeConcurrentQueueSegmentSlot<T> _slot948;
        private NativeConcurrentQueueSegmentSlot<T> _slot949;
        private NativeConcurrentQueueSegmentSlot<T> _slot950;
        private NativeConcurrentQueueSegmentSlot<T> _slot951;
        private NativeConcurrentQueueSegmentSlot<T> _slot952;
        private NativeConcurrentQueueSegmentSlot<T> _slot953;
        private NativeConcurrentQueueSegmentSlot<T> _slot954;
        private NativeConcurrentQueueSegmentSlot<T> _slot955;
        private NativeConcurrentQueueSegmentSlot<T> _slot956;
        private NativeConcurrentQueueSegmentSlot<T> _slot957;
        private NativeConcurrentQueueSegmentSlot<T> _slot958;
        private NativeConcurrentQueueSegmentSlot<T> _slot959;
        private NativeConcurrentQueueSegmentSlot<T> _slot960;
        private NativeConcurrentQueueSegmentSlot<T> _slot961;
        private NativeConcurrentQueueSegmentSlot<T> _slot962;
        private NativeConcurrentQueueSegmentSlot<T> _slot963;
        private NativeConcurrentQueueSegmentSlot<T> _slot964;
        private NativeConcurrentQueueSegmentSlot<T> _slot965;
        private NativeConcurrentQueueSegmentSlot<T> _slot966;
        private NativeConcurrentQueueSegmentSlot<T> _slot967;
        private NativeConcurrentQueueSegmentSlot<T> _slot968;
        private NativeConcurrentQueueSegmentSlot<T> _slot969;
        private NativeConcurrentQueueSegmentSlot<T> _slot970;
        private NativeConcurrentQueueSegmentSlot<T> _slot971;
        private NativeConcurrentQueueSegmentSlot<T> _slot972;
        private NativeConcurrentQueueSegmentSlot<T> _slot973;
        private NativeConcurrentQueueSegmentSlot<T> _slot974;
        private NativeConcurrentQueueSegmentSlot<T> _slot975;
        private NativeConcurrentQueueSegmentSlot<T> _slot976;
        private NativeConcurrentQueueSegmentSlot<T> _slot977;
        private NativeConcurrentQueueSegmentSlot<T> _slot978;
        private NativeConcurrentQueueSegmentSlot<T> _slot979;
        private NativeConcurrentQueueSegmentSlot<T> _slot980;
        private NativeConcurrentQueueSegmentSlot<T> _slot981;
        private NativeConcurrentQueueSegmentSlot<T> _slot982;
        private NativeConcurrentQueueSegmentSlot<T> _slot983;
        private NativeConcurrentQueueSegmentSlot<T> _slot984;
        private NativeConcurrentQueueSegmentSlot<T> _slot985;
        private NativeConcurrentQueueSegmentSlot<T> _slot986;
        private NativeConcurrentQueueSegmentSlot<T> _slot987;
        private NativeConcurrentQueueSegmentSlot<T> _slot988;
        private NativeConcurrentQueueSegmentSlot<T> _slot989;
        private NativeConcurrentQueueSegmentSlot<T> _slot990;
        private NativeConcurrentQueueSegmentSlot<T> _slot991;
        private NativeConcurrentQueueSegmentSlot<T> _slot992;
        private NativeConcurrentQueueSegmentSlot<T> _slot993;
        private NativeConcurrentQueueSegmentSlot<T> _slot994;
        private NativeConcurrentQueueSegmentSlot<T> _slot995;
        private NativeConcurrentQueueSegmentSlot<T> _slot996;
        private NativeConcurrentQueueSegmentSlot<T> _slot997;
        private NativeConcurrentQueueSegmentSlot<T> _slot998;
        private NativeConcurrentQueueSegmentSlot<T> _slot999;
        private NativeConcurrentQueueSegmentSlot<T> _slot1000;
        private NativeConcurrentQueueSegmentSlot<T> _slot1001;
        private NativeConcurrentQueueSegmentSlot<T> _slot1002;
        private NativeConcurrentQueueSegmentSlot<T> _slot1003;
        private NativeConcurrentQueueSegmentSlot<T> _slot1004;
        private NativeConcurrentQueueSegmentSlot<T> _slot1005;
        private NativeConcurrentQueueSegmentSlot<T> _slot1006;
        private NativeConcurrentQueueSegmentSlot<T> _slot1007;
        private NativeConcurrentQueueSegmentSlot<T> _slot1008;
        private NativeConcurrentQueueSegmentSlot<T> _slot1009;
        private NativeConcurrentQueueSegmentSlot<T> _slot1010;
        private NativeConcurrentQueueSegmentSlot<T> _slot1011;
        private NativeConcurrentQueueSegmentSlot<T> _slot1012;
        private NativeConcurrentQueueSegmentSlot<T> _slot1013;
        private NativeConcurrentQueueSegmentSlot<T> _slot1014;
        private NativeConcurrentQueueSegmentSlot<T> _slot1015;
        private NativeConcurrentQueueSegmentSlot<T> _slot1016;
        private NativeConcurrentQueueSegmentSlot<T> _slot1017;
        private NativeConcurrentQueueSegmentSlot<T> _slot1018;
        private NativeConcurrentQueueSegmentSlot<T> _slot1019;
        private NativeConcurrentQueueSegmentSlot<T> _slot1020;
        private NativeConcurrentQueueSegmentSlot<T> _slot1021;
        private NativeConcurrentQueueSegmentSlot<T> _slot1022;
        private NativeConcurrentQueueSegmentSlot<T> _slot1023;
    }

    /// <summary>
    ///     Slot
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeConcurrentQueueSegmentSlot<T> where T : unmanaged
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