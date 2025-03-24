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
    ///     Native concurrentStack
    ///     (Slower than ConcurrentStack, disable Enumerator, try peek, push/pop range either)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeConcurrentStack<T> : IDisposable, IEquatable<NativeConcurrentStack<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentStackHandle
        {
            /// <summary>
            ///     Head
            /// </summary>
            public volatile nint Head;

            /// <summary>
            ///     Node pool
            /// </summary>
            public NativeMemoryPool NodePool;

            /// <summary>
            ///     Node lock
            /// </summary>
            public ulong NodeLock;

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
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                NativeConcurrentSpinLock nodeLock = Unsafe.AsPointer(ref NodeLock);
                nodeLock.Enter();
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
                    nodeLock.Exit();
                }
            }

            /// <summary>
            ///     Push
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(in T item)
            {
                NativeConcurrentSpinLock nodeLock = Unsafe.AsPointer(ref NodeLock);
                Node* newNode;
                nodeLock.Enter();
                try
                {
                    newNode = (Node*)NodePool.Rent();
                }
                finally
                {
                    nodeLock.Exit();
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

                NativeConcurrentSpinLock nodeLock = Unsafe.AsPointer(ref NodeLock);
                if (Interlocked.CompareExchange(ref Head, (nint)head->Next, (nint)head) == (nint)head)
                {
                    result = head->Value;
                    nodeLock.Enter();
                    try
                    {
                        NodePool.Return(head);
                    }
                    finally
                    {
                        nodeLock.Exit();
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
                        nodeLock.Enter();
                        try
                        {
                            NodePool.Return(head);
                        }
                        finally
                        {
                            nodeLock.Exit();
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
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeConcurrentStackHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentStack(int size, int maxFreeSlabs)
        {
            var nodePool = new NativeMemoryPool(size, sizeof(Node), maxFreeSlabs);
            var handle = (NativeConcurrentStackHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeConcurrentStackHandle));
            handle->Head = nint.Zero;
            handle->NodePool = nodePool;
            NativeConcurrentSpinLock nodeLock = &handle->NodeLock;
            nodeLock.Reset();
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     IsEmpty
        /// </summary>
        public bool IsEmpty => _handle->Head == nint.Zero;

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
        public bool Equals(NativeConcurrentStack<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentStack<T> nativeConcurrentStack && nativeConcurrentStack == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentStack<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentStack<T> left, NativeConcurrentStack<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentStack<T> left, NativeConcurrentStack<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->NodePool.Dispose();
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item) => _handle->Push(item);

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result) => _handle->TryPop(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentStack<T> Empty => new();

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
    }
}