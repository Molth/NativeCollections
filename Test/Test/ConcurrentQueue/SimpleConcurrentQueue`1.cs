using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;
using static Examples.SimpleConcurrentQueue;
using static Examples.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace Examples
{
    [StructLayout(LayoutKind.Explicit, Size = 2 * CACHE_LINE_SIZE)]
    internal struct CachePaddedAtomicReference
    {
        [FieldOffset(1 * CACHE_LINE_SIZE)] public nuint AtomicReference;
    }

    internal unsafe struct CachePaddedAtomicReference<T> where T : unmanaged
    {
        private CachePaddedAtomicReference _data;

        public ref UnsafeAtomicReference<Segment<T>> Segment => ref Unsafe.As<nuint, UnsafeAtomicReference<Segment<T>>>(ref _data.AtomicReference);
    }

    /// <summary>
    ///     Unsafe concurrentQueue
    ///     (Slower than ConcurrentQueue, disable Enumerator, try peek either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct SimpleConcurrentQueue<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Cross segment lock
        /// </summary>
        private GCHandle _crossSegmentLock;

        /// <summary>
        ///     Tail
        /// </summary>
        private CachePaddedAtomicReference<T> _tail;

        /// <summary>
        ///     Head
        /// </summary>
        private CachePaddedAtomicReference<T> _head;

        private HazardPointers _hp;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleConcurrentQueue(int maxThreads)
        {
            _crossSegmentLock = GCHandle.Alloc(new object(), GCHandleType.Normal);
            var segment = NativeMemoryAllocator.AlignedAlloc<Segment<T>>(1);
            segment->Initialize();
            _tail = _head = new CachePaddedAtomicReference<T>() { Segment = new UnsafeAtomicReference<Segment<T>>(segment) };
            _hp = new HazardPointers(1, maxThreads);
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var node = _head.Segment.AsRef();
            while (node != null)
            {
                var temp = node;
                node = (Segment<T>*)node->NextSegment;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _crossSegmentLock.Free();
            _hp.Dispose();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            using var tid = SimpleTidManager.Guard();

            var tail = _hp.protect(0, ref _tail.Segment, tid);
            if (tail->TryEnqueue(item))
            {
                _hp.clear(tid);
                return;
            }

            _hp.clear(tid);
            while (true)
            {
                tail = _hp.protect(0, ref _tail.Segment, tid);
                if (tail->TryEnqueue(item))
                {
                    _hp.clear(tid);
                    return;
                }

                lock (_crossSegmentLock.Target!)
                {
                    if (tail == _tail.Segment.Read())
                    {
                        tail->EnsureFrozenForEnqueues();
                        var newTail = NativeMemoryAllocator.AlignedAlloc<Segment<T>>(1);
                        newTail->Initialize();
                        tail->NextSegment = (nint)newTail;
                        _tail.Segment.Exchange(newTail);
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
            using var tid = SimpleTidManager.Guard();

            var head = _hp.protect(0, ref _head.Segment, tid);
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
                head = _hp.protect(0, ref _head.Segment, tid);
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
                    if (head == _head.Segment.Read())
                    {
                        _head.Segment.Exchange((Segment<T>*)head->NextSegment);
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