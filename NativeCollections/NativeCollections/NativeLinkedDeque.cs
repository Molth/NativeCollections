using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked deque
    ///     (Slower than Deque)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeLinkedDeque<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeLinkedDequeHandle
        {
            /// <summary>
            ///     Memory pool
            /// </summary>
            public NativeMemoryPool MemoryPool;

            /// <summary>
            ///     Linked list
            /// </summary>
            public NativeLinkedList LinkedList;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeLinkedDequeNode
        {
            /// <summary>
            ///     Node list
            /// </summary>
            public NativeLinkedListNode NodeList;

            /// <summary>
            ///     Item
            /// </summary>
            public T Item;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeLinkedDequeHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedDeque(int size, int maxFreeSlabs)
        {
            var memoryPool = new NativeMemoryPool(size, sizeof(NativeLinkedDequeNode), maxFreeSlabs);
            var linkedList = new NativeLinkedList();
            linkedList.Clear();
            var handle = (NativeLinkedDequeHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeLinkedDequeHandle));
            handle->MemoryPool = memoryPool;
            handle->LinkedList = linkedList;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Slabs
        /// </summary>
        public int Slabs => _handle->MemoryPool.Slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        public int FreeSlabs => _handle->MemoryPool.FreeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        public int MaxFreeSlabs => _handle->MemoryPool.MaxFreeSlabs;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _handle->MemoryPool.Size;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->MemoryPool.Dispose();
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var handle = _handle;
            var tail = handle->LinkedList.Tail;
            for (var node = handle->LinkedList.Head; node != tail; node = node->Next)
                handle->MemoryPool.Return(node);
            handle->LinkedList.Clear();
        }

        /// <summary>
        ///     Count
        /// </summary>
        /// <returns>Count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count() => _handle->LinkedList.Count();

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->MemoryPool.EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => _handle->MemoryPool.TrimExcess();

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->MemoryPool.TrimExcess(capacity);

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueHead(in T item)
        {
            var handle = _handle;
            var node = (NativeLinkedDequeNode*)handle->MemoryPool.Rent();
            node->Item = item;
            handle->LinkedList.Tail->InsertAfter((NativeLinkedListNode*)node);
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item)
        {
            var handle = _handle;
            var node = (NativeLinkedDequeNode*)handle->MemoryPool.Rent();
            node->Item = item;
            handle->LinkedList.Tail->InsertBefore((NativeLinkedListNode*)node);
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Head;
            result = ((NativeLinkedDequeNode*)node)->Item;
            node->Remove();
            handle->MemoryPool.Return(node);
            return true;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Tail->Previous;
            result = ((NativeLinkedDequeNode*)node)->Item;
            node->Remove();
            handle->MemoryPool.Return(node);
            return true;
        }

        /// <summary>
        ///     Try peek head
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekHead(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Head;
            result = ((NativeLinkedDequeNode*)node)->Item;
            return true;
        }

        /// <summary>
        ///     Try peek tail
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekTail(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Tail->Previous;
            result = ((NativeLinkedDequeNode*)node)->Item;
            return true;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeLinkedDeque<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new((NativeLinkedList*)Unsafe.AsPointer(ref _handle->LinkedList));

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     Native linked list
            /// </summary>
            private readonly NativeLinkedList* _nativeLinkedList;

            /// <summary>
            ///     Native linked list node
            /// </summary>
            private NativeLinkedListNode* _node;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeLinkedList">NativeLinkedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeLinkedList* nativeLinkedList)
            {
                _nativeLinkedList = nativeLinkedList;
                _node = nativeLinkedList->Tail;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                _node = _node->Next;
                return _node != _nativeLinkedList->Tail;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((NativeLinkedDequeNode*)_node)->Item;
            }
        }
    }
}