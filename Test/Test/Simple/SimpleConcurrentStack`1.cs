using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeCollections;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace Examples
{
    /// <summary>
    ///     Unsafe concurrentStack
    ///     (Slower than ConcurrentStack, disable Enumerator, try peek, push/pop range either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct SimpleConcurrentStack<T> : IDisposable where T : unmanaged
    {
        private Padding _padding0;

        /// <summary>
        ///     Head
        /// </summary>
        private UnsafeAtomicPtr<Node> _head;

        private Padding _padding1;

        /// <summary>
        ///     Hazard pointers
        /// </summary>
        private HazardPointers _hp;

        private Padding _padding2;

        private SimpleTidManager2 _tidManager;

        private Padding _padding3;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleConcurrentStack(int maxThreads)
        {
            _head = new UnsafeAtomicPtr<Node>();
            _hp = new HazardPointers(1, maxThreads);
            _tidManager = new SimpleTidManager2(maxThreads);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            NativeMemoryAllocator.AlignedFree(_head.AsRef());
            _hp.Dispose();
            _tidManager.Dispose();
        }

        /// <summary>
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            Node* newNode = NativeMemoryAllocator.AlignedAlloc<Node>(1);
            newNode->Value = item;
            newNode->Next = _head.Load(Ordering.Relaxed);
            if (_head.CompareExchange(newNode, newNode->Next) == newNode->Next)
                return;
            var spinWait = new UnsafeSpinWait();
            do
            {
                spinWait.SpinOnce(-1);
                newNode->Next = _head.Load(Ordering.Relaxed);
            } while (_head.CompareExchange(newNode, newNode->Next) != newNode->Next);
        }

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var head = _head.Load(Ordering.Relaxed);
            if (head == null)
            {
                result = default;
                return false;
            }

            var tid = _tidManager.Rent();
            try
            {
                return TryPop(out result, tid);
            }
            finally
            {
                _tidManager.Return(tid);
            }
        }

        /// <summary>
        ///     Try pop
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryPop(out T result, int tid)
        {
            var spinWait = new UnsafeSpinWait();
            var backoff = 1;
            while (true)
            {
                var head = _hp.protect(0, ref _head, tid);
                if (head == null)
                {
                    _hp.clear(tid);
                    result = default;
                    return false;
                }

                if (_head.CompareExchange(head->Next, head) == head)
                {
                    result = head->Value;
                    _hp.clear(tid);
                    _hp.retire(head, tid);
                    return true;
                }

                _hp.clear(tid);

                for (var i = 0; i < backoff; ++i)
                    spinWait.SpinOnce(-1);

                backoff = spinWait.NextSpinWillYield ? Random.Shared.Next(1, 8) : backoff * 2;
            }
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            /// <summary>
            ///     Value
            /// </summary>
            public T Value;

            /// <summary>
            ///     Next
            /// </summary>
            public Node* Next;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static SimpleConcurrentStack<T> Empty => new();
    }
}