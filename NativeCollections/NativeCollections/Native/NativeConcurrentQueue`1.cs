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
    [BindingType(typeof(UnsafeConcurrentQueue<>))]
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
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeConcurrentQueue<T>>(1);
            Unsafe.AsRef<UnsafeConcurrentQueue<T>>(handle) = value;
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
            handle->Dispose();
            NativeMemoryAllocator.AlignedFree(handle);
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
    /// </summary>
    internal static partial class NativeConcurrentQueue
    {
        /// <summary>
        ///     Native concurrentQueue
        ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NativeConcurrentQueueNotArm64<T> : IDisposable where T : unmanaged
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
            public readonly bool IsEmpty
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
                        if (next != UnsafeHelpers.ToIntPtr(0))
                            segment = (NativeConcurrentQueueSegmentNotArm64<T>*)next;
                        else if (Volatile.Read(ref segment->NextSegment) == UnsafeHelpers.ToIntPtr(0))
                            break;
                    }

                    return true;
                }
            }

            /// <summary>
            ///     Count
            /// </summary>
            public readonly int Count
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
                                            count += s->HeadAndTail.Tail - FREEZE_OFFSET;
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
                var segmentPool = new UnsafeMemoryPool(size, sizeof(NativeConcurrentQueueSegmentNotArm64<T>), maxFreeSlabs, (int)Math.Max(NativeMemoryAllocator.AlignOf<T>(), ArchitectureHelpers.CACHE_LINE_SIZE_NOT_ARM64));
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
                if (head->NextSegment == 0)
                {
                    result = default;
                    return false;
                }

                while (true)
                {
                    head = _head;
                    if (head->TryDequeue(out result))
                        return true;
                    if (head->NextSegment == 0)
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
                if (head != tail && head != tail - FREEZE_OFFSET)
                {
                    head &= SLOTS_MASK;
                    tail &= SLOTS_MASK;
                    return head < tail ? tail - head : LENGTH - head + tail;
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
                var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - FREEZE_OFFSET;
                if (headHead < headTail)
                {
                    headHead &= SLOTS_MASK;
                    headTail &= SLOTS_MASK;
                    count += headHead < headTail ? headTail - headHead : LENGTH - headHead + headTail;
                }

                if (head != tail)
                {
                    for (var s = (NativeConcurrentQueueSegmentNotArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentNotArm64<T>*)s->NextSegment)
                        count += s->HeadAndTail.Tail - FREEZE_OFFSET;
                    count += tailTail - FREEZE_OFFSET;
                }

                return count;
            }
        }

        /// <summary>
        ///     Native concurrentQueue segment
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NativeConcurrentQueueSegmentNotArm64<T> where T : unmanaged
        {
            /// <summary>
            ///     Slots
            /// </summary>
            public NativeConcurrentQueueSegmentSlots1024<T> Slots;

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
                for (var i = 0; i < LENGTH; ++i)
                    Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)i).SequenceNumber = i;
                HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTailNotArm64();
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
                    Interlocked.Add(ref HeadAndTail.Tail, FREEZE_OFFSET);
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
                            Volatile.Write(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber, currentHead + LENGTH);
                            return true;
                        }
                    }
                    else if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - FREEZE_OFFSET - currentHead <= 0))
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
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - (currentHead + 1);
                    if (diff == 0)
                        return true;
                    if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - FREEZE_OFFSET - currentHead <= 0))
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
        ///     NativeConcurrentQueue padded head and tail
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
        public struct NativeConcurrentQueuePaddedHeadAndTailNotArm64
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
            private const int CACHE_LINE_SIZE = (int)ArchitectureHelpers.CACHE_LINE_SIZE_NOT_ARM64;
        }
    }

    /// <summary>
    ///     Native concurrentQueue
    /// </summary>
    internal static partial class NativeConcurrentQueue
    {
        /// <summary>
        ///     Native concurrentQueue
        ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NativeConcurrentQueueArm64<T> : IDisposable where T : unmanaged
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
            public readonly bool IsEmpty
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
                        if (next != UnsafeHelpers.ToIntPtr(0))
                            segment = (NativeConcurrentQueueSegmentArm64<T>*)next;
                        else if (Volatile.Read(ref segment->NextSegment) == UnsafeHelpers.ToIntPtr(0))
                            break;
                    }

                    return true;
                }
            }

            /// <summary>
            ///     Count
            /// </summary>
            public readonly int Count
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
                                            count += s->HeadAndTail.Tail - FREEZE_OFFSET;
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
                var segmentPool = new UnsafeMemoryPool(size, sizeof(NativeConcurrentQueueSegmentArm64<T>), maxFreeSlabs, (int)Math.Max(NativeMemoryAllocator.AlignOf<T>(), ArchitectureHelpers.CACHE_LINE_SIZE_ARM64));
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
                if (head->NextSegment == 0)
                {
                    result = default;
                    return false;
                }

                while (true)
                {
                    head = _head;
                    if (head->TryDequeue(out result))
                        return true;
                    if (head->NextSegment == 0)
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
                if (head != tail && head != tail - FREEZE_OFFSET)
                {
                    head &= SLOTS_MASK;
                    tail &= SLOTS_MASK;
                    return head < tail ? tail - head : LENGTH - head + tail;
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
                var headTail = (head == tail ? tailTail : Volatile.Read(ref head->HeadAndTail.Tail)) - FREEZE_OFFSET;
                if (headHead < headTail)
                {
                    headHead &= SLOTS_MASK;
                    headTail &= SLOTS_MASK;
                    count += headHead < headTail ? headTail - headHead : LENGTH - headHead + headTail;
                }

                if (head != tail)
                {
                    for (var s = (NativeConcurrentQueueSegmentArm64<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueueSegmentArm64<T>*)s->NextSegment)
                        count += s->HeadAndTail.Tail - FREEZE_OFFSET;
                    count += tailTail - FREEZE_OFFSET;
                }

                return count;
            }
        }

        /// <summary>
        ///     Native concurrentQueue segment
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct NativeConcurrentQueueSegmentArm64<T> where T : unmanaged
        {
            /// <summary>
            ///     Slots
            /// </summary>
            public NativeConcurrentQueueSegmentSlots1024<T> Slots;

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
                for (var i = 0; i < LENGTH; ++i)
                    Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)i).SequenceNumber = i;
                HeadAndTail = new NativeConcurrentQueuePaddedHeadAndTailArm64();
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
                    Interlocked.Add(ref HeadAndTail.Tail, FREEZE_OFFSET);
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
                            Volatile.Write(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber, currentHead + LENGTH);
                            return true;
                        }
                    }
                    else if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - FREEZE_OFFSET - currentHead <= 0))
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
                    var slotsIndex = currentHead & SLOTS_MASK;
                    var sequenceNumber = Volatile.Read(ref Unsafe.Add(ref Unsafe.AsRef<NativeConcurrentQueueSegmentSlot<T>>(slots), (nint)slotsIndex).SequenceNumber);
                    var diff = sequenceNumber - (currentHead + 1);
                    if (diff == 0)
                        return true;
                    if (diff < 0)
                    {
                        var frozen = FrozenForEnqueues;
                        var currentTail = Volatile.Read(ref HeadAndTail.Tail);
                        if (currentTail - currentHead <= 0 || (frozen && currentTail - FREEZE_OFFSET - currentHead <= 0))
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
        ///     NativeConcurrentQueue padded head and tail
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 3 * CACHE_LINE_SIZE)]
        public struct NativeConcurrentQueuePaddedHeadAndTailArm64
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
            private const int CACHE_LINE_SIZE = (int)ArchitectureHelpers.CACHE_LINE_SIZE_ARM64;
        }
    }

    /// <summary>
    ///     Native concurrentQueue
    /// </summary>
    internal static partial class NativeConcurrentQueue
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
    ///     Native concurrentQueue
    /// </summary>
    internal static partial class NativeConcurrentQueue
    {
#if NET8_0_OR_GREATER
        /// <summary>
        ///     Slots
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        [InlineArray(LENGTH)]
        public struct NativeConcurrentQueueSegmentSlots1024<T> where T : unmanaged
        {
            private NativeConcurrentQueueSegmentSlot<T> _element;
        }
#else
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
#endif

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