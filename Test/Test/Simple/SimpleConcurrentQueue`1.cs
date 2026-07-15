using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;
using static Examples.SimpleConcurrentQueue;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace Examples
{
    /// <summary>
    ///     Unsafe concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct SimpleConcurrentQueue<T> : IDisposable where T : unmanaged
    {
        private Padding _padding0;

        /// <summary>
        ///     Cross segment lock
        /// </summary>
        private GCHandle _crossSegmentLock;

        private Padding _padding1;

        /// <summary>
        ///     Tail
        /// </summary>
        private UnsafeAtomicPtr<Segment<T>> _tail;

        private Padding _padding2;

        /// <summary>
        ///     Head
        /// </summary>
        private UnsafeAtomicPtr<Segment<T>> _head;

        private Padding _padding3;

        /// <summary>
        ///     Hazard pointers
        /// </summary>
        private HazardPointers _hp;

        private Padding _padding4;

        private SimpleTidManager2 _tidManager;

        private Padding _padding5;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleConcurrentQueue(int maxThreads)
        {
            _crossSegmentLock = GCHandle.Alloc(new object(), GCHandleType.Normal);
            var segment = NativeMemoryAllocator.AlignedAlloc<Segment<T>>(1);
            segment->Initialize();
            _tail = _head = new UnsafeAtomicPtr<Segment<T>>(segment);
            _hp = new HazardPointers(1, maxThreads);
            _tidManager = new SimpleTidManager2(maxThreads);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var node = _head.AsRef();
            while (node != null)
            {
                var temp = node;
                node = (Segment<T>*)node->NextSegment;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _crossSegmentLock.Free();
            _hp.Dispose();
            _tidManager.Dispose();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            var tid = _tidManager.Rent();
            try
            {
                Enqueue(item, tid);
            }
            finally
            {
                _tidManager.Return(tid);
            }
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Enqueue(T item, int tid)
        {
            var tail = _hp.protect(0, ref _tail, tid);
            if (tail->TryEnqueue(item))
            {
                _hp.clear(tid);
                return;
            }

            _hp.clear(tid);
            while (true)
            {
                tail = _hp.protect(0, ref _tail, tid);
                if (tail->TryEnqueue(item))
                {
                    _hp.clear(tid);
                    return;
                }

                lock (_crossSegmentLock.Target!)
                {
                    if (tail == _tail.Read())
                    {
                        tail->EnsureFrozenForEnqueues();
                        var newTail = NativeMemoryAllocator.AlignedAlloc<Segment<T>>(1);
                        newTail->Initialize();
                        tail->NextSegment = (nint)newTail;
                        _tail.Exchange(newTail);
                    }
                }

                _hp.clear(tid);
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
            var tid = _tidManager.Rent();
            try
            {
                return TryDequeue(out result, tid);
            }
            finally
            {
                _tidManager.Return(tid);
            }
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryDequeue(out T result, int tid)
        {
            var head = _hp.protect(0, ref _head, tid);
            if (head->TryDequeue(out result))
            {
                _hp.clear(tid);
                return true;
            }

            if (head->NextSegment == 0)
            {
                _hp.clear(tid);
                result = default;
                return false;
            }

            _hp.clear(tid);
            while (true)
            {
                head = _hp.protect(0, ref _head, tid);
                if (head->TryDequeue(out result))
                {
                    _hp.clear(tid);
                    return true;
                }

                if (head->NextSegment == 0)
                {
                    _hp.clear(tid);
                    result = default;
                    return false;
                }

                if (head->TryDequeue(out result))
                {
                    _hp.clear(tid);
                    return true;
                }

                lock (_crossSegmentLock.Target!)
                {
                    if (head == _head.Read())
                    {
                        _head.Exchange((Segment<T>*)head->NextSegment);
                        _hp.clear(tid);
                        _hp.retire(head, tid);
                    }
                    else
                    {
                        _hp.clear(tid);
                    }
                }
            }
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static SimpleConcurrentQueue<T> Empty => new();
    }
}