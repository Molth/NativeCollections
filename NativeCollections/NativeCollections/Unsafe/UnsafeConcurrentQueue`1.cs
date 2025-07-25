using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2208
#pragma warning disable CS0169
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeConcurrentQueue<T> : IDisposable where T : unmanaged
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
        private volatile NativeConcurrentQueue.NativeConcurrentQueueSegment<T>* _tail;

        /// <summary>
        ///     Head
        /// </summary>
        private volatile NativeConcurrentQueue.NativeConcurrentQueueSegment<T>* _head;

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
                    if (next != new IntPtr(0))
                        segment = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)next;
                    else if (Volatile.Read(ref segment->NextSegment) == new IntPtr(0))
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
                            return GetCount(headHead, headTail);
                    }
                    else if ((NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)head->NextSegment == tail)
                    {
                        var tailHead = Volatile.Read(ref tail->HeadAndTail.Head);
                        var tailTail = Volatile.Read(ref tail->HeadAndTail.Tail);
                        if (head == _head && tail == _tail && headHead == Volatile.Read(ref head->HeadAndTail.Head) && headTail == Volatile.Read(ref head->HeadAndTail.Tail) && tailHead == Volatile.Read(ref tail->HeadAndTail.Head) && tailTail == Volatile.Read(ref tail->HeadAndTail.Tail))
                            return GetCount(headHead, headTail) + GetCount(tailHead, tailTail);
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
                                    var count = GetCount(headHead, headTail) + GetCount(tailHead, tailTail);
                                    for (var s = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)head->NextSegment; s != tail; s = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)s->NextSegment)
                                        count += s->HeadAndTail.Tail - NativeConcurrentQueue.SEGMENT_FREEZE_OFFSET;
                                    return count;
                                }
                            }
                        }
                    }

                    spinWait.SpinOnce(-1);
                }
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeConcurrentQueue(int size, int maxFreeSlabs)
        {
            var segmentPool = new UnsafeMemoryPool(size, sizeof(NativeConcurrentQueue.NativeConcurrentQueueSegment<T>), maxFreeSlabs, (int)Math.Max(NativeMemoryAllocator.AlignOf<T>(), 128));
            _crossSegmentLock = GCHandle.Alloc(new object(), GCHandleType.Normal);
            _segmentPool = segmentPool;
            var segment = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)_segmentPool.Rent();
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
                    node = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)node->NextSegment;
                    _segmentPool.Return(temp);
                }

                var segment = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)_segmentPool.Rent();
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
                            var newTail = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)_segmentPool.Rent();
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
                        _head = (NativeConcurrentQueue.NativeConcurrentQueueSegment<T>*)head->NextSegment;
                        _segmentPool.Return(head);
                    }
                }
            }
        }

        /// <summary>
        ///     Get count
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="tail">Tail</param>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetCount(int head, int tail)
        {
            if (head != tail && head != tail - NativeConcurrentQueue.SEGMENT_FREEZE_OFFSET)
            {
                head &= NativeConcurrentQueue.SLOTS_MASK;
                tail &= NativeConcurrentQueue.SLOTS_MASK;
                return head < tail ? tail - head : NativeConcurrentQueue.SLOTS_LENGTH - head + tail;
            }

            return 0;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentQueue<T> Empty => new();
    }
}