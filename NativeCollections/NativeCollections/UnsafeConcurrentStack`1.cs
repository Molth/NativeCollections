using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2208
#pragma warning disable CS8632

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
        public volatile nint Head;

        /// <summary>
        ///     Node pool
        /// </summary>
        public UnsafeMemoryPool NodePool;

        /// <summary>
        ///     Node lock
        /// </summary>
        public NativeConcurrentSpinLock NodeLock;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var count = 0;
                for (var node = (Node*)Head; node != null; node = node->Next)
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
            var nodePool = new UnsafeMemoryPool(size, sizeof(Node), maxFreeSlabs);
            Head = IntPtr.Zero;
            NodePool = nodePool;
            NodeLock.Reset();
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NodePool.Dispose();

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            NodeLock.Enter();
            try
            {
                var node = (Node*)Head;
                while (node != null)
                {
                    var temp = node;
                    node = node->Next;
                    NodePool.Return(temp);
                }
            }
            finally
            {
                NodeLock.Exit();
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
            NodeLock.Enter();
            try
            {
                newNode = (Node*)NodePool.Rent();
            }
            finally
            {
                NodeLock.Exit();
            }

            newNode->Value = item;
            newNode->Next = (Node*)Head;
            if (Interlocked.CompareExchange(ref Head, (nint)newNode, (nint)newNode->Next) == (nint)newNode->Next)
                return;
            var spinWait = new NativeSpinWait();
            do
            {
                spinWait.SpinOnce();
                newNode->Next = (Node*)Head;
            } while (Interlocked.CompareExchange(ref Head, (nint)newNode, (nint)newNode->Next) != (nint)newNode->Next);
        }

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var head = (Node*)Head;
            if (head == null)
            {
                result = default;
                return false;
            }

            if (Interlocked.CompareExchange(ref Head, (nint)head->Next, (nint)head) == (nint)head)
            {
                result = head->Value;
                NodeLock.Enter();
                try
                {
                    NodePool.Return(head);
                }
                finally
                {
                    NodeLock.Exit();
                }

                return true;
            }

            var spinWait = new NativeSpinWait();
            var backoff = 1;
#if !NET6_0_OR_GREATER
            var random = new NativeXoshiro256();
            random.Initialize();
#endif
            while (true)
            {
                head = (Node*)Head;
                if (head == null)
                {
                    result = default;
                    return false;
                }

                if (Interlocked.CompareExchange(ref Head, (nint)head->Next, (nint)head) == (nint)head)
                {
                    result = head->Value;
                    NodeLock.Enter();
                    try
                    {
                        NodePool.Return(head);
                    }
                    finally
                    {
                        NodeLock.Exit();
                    }

                    return true;
                }

                for (var i = 0; i < backoff; ++i)
                    spinWait.SpinOnce();
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