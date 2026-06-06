using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrentStack
    ///     (Slower than ConcurrentStack, disable Enumerator, try peek, push/pop range either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeConcurrentStack<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Head
        /// </summary>
        private volatile nint _head;

        /// <summary>
        ///     Node pool
        /// </summary>
        private UnsafeMemoryPool _nodePool;

        /// <summary>
        ///     Node lock
        /// </summary>
        private UnsafeConcurrentSpinLock _nodeLock;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public readonly bool IsEmpty => _head == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var count = 0;
                for (var node = (Node*)_head; node != null; node = node->Next)
                    count++;
                return count;
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeConcurrentStack(int size, int maxFreeSlabs)
        {
            var nodePool = new UnsafeMemoryPool(size, Unsafe.SizeOf<Node>(), maxFreeSlabs, (int)NativeMemoryAllocator.AlignOf<Node>());
            _head = 0;
            _nodePool = nodePool;
            _nodeLock = new UnsafeConcurrentSpinLock();
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _nodePool.Dispose();

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _nodeLock.Enter();
            try
            {
                var node = (Node*)_head;
                while (node != null)
                {
                    var temp = node;
                    node = node->Next;
                    _nodePool.Return(temp);
                }
            }
            finally
            {
                _nodeLock.Exit();
            }
        }

        /// <summary>
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item)
        {
            Node* newNode;
            _nodeLock.Enter();
            try
            {
                newNode = (Node*)_nodePool.Rent();
            }
            finally
            {
                _nodeLock.Exit();
            }

            newNode->Value = item;
            newNode->Next = (Node*)_head;
            if (Interlocked.CompareExchange(ref _head, (nint)newNode, (nint)newNode->Next) == (nint)newNode->Next)
                return;
            var spinWait = new NativeSpinWait();
            do
            {
                spinWait.SpinOnce(-1);
                newNode->Next = (Node*)_head;
            } while (Interlocked.CompareExchange(ref _head, (nint)newNode, (nint)newNode->Next) != (nint)newNode->Next);
        }

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var head = (Node*)_head;
            if (head == null)
            {
                result = default;
                return false;
            }

            if (Interlocked.CompareExchange(ref _head, (nint)head->Next, (nint)head) == (nint)head)
            {
                result = head->Value;
                _nodeLock.Enter();
                try
                {
                    _nodePool.Return(head);
                }
                finally
                {
                    _nodeLock.Exit();
                }

                return true;
            }

            var spinWait = new NativeSpinWait();
            var backoff = 1;
#if !NET6_0_OR_GREATER
            var random = NativeXoshiro256.Create();
#endif
            while (true)
            {
                head = (Node*)_head;
                if (head == null)
                {
                    result = default;
                    return false;
                }

                if (Interlocked.CompareExchange(ref _head, (nint)head->Next, (nint)head) == (nint)head)
                {
                    result = head->Value;
                    _nodeLock.Enter();
                    try
                    {
                        _nodePool.Return(head);
                    }
                    finally
                    {
                        _nodeLock.Exit();
                    }

                    return true;
                }

                for (var i = 0; i < backoff; ++i)
                    spinWait.SpinOnce(-1);
                if (spinWait.NextSpinWillYield)
                {
#if NET6_0_OR_GREATER
                    backoff = Random.Shared.Next(1, 8);
#else
                    backoff = random.NextInt32(1, 8);
#endif
                }
                else
                {
                    backoff *= 2;
                }
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
        public static UnsafeConcurrentStack<T> Empty => new();
    }
}