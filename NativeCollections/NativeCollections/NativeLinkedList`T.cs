using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linked linkedList
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeLinkedList<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeLinkedListHandle
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
        ///     Handle
        /// </summary>
        private readonly NativeLinkedListHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinkedList(int size, int maxFreeSlabs)
        {
            var memoryPool = new NativeMemoryPool(size, sizeof(NativeLinkedListNode<T>), maxFreeSlabs);
            var linkedList = new NativeLinkedList();
            linkedList.Clear();
            var handle = (NativeLinkedListHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeLinkedListHandle));
            handle->MemoryPool = memoryPool;
            handle->LinkedList = linkedList;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Sentinel
        /// </summary>
        public NativeLinkedListNode Sentinel
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->LinkedList.Sentinel;
        }

        /// <summary>
        ///     Head
        /// </summary>
        public NativeLinkedListNode* Head
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->LinkedList.Head;
        }

        /// <summary>
        ///     Tail
        /// </summary>
        public NativeLinkedListNode* Tail
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->LinkedList.Tail;
        }

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
        ///     Add head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddHead(in T item)
        {
            var handle = _handle;
            var node = (NativeLinkedListNode<T>*)handle->MemoryPool.Rent();
            node->Item = item;
            handle->LinkedList.Tail->InsertAfter((NativeLinkedListNode*)node);
        }

        /// <summary>
        ///     Add tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTail(in T item)
        {
            var handle = _handle;
            var node = (NativeLinkedListNode<T>*)handle->MemoryPool.Rent();
            node->Item = item;
            handle->LinkedList.Tail->InsertBefore((NativeLinkedListNode*)node);
        }

        /// <summary>
        ///     Try remove
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveHead(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Head;
            result = ((NativeLinkedListNode<T>*)node)->Item;
            node->Remove();
            handle->MemoryPool.Return(node);
            return true;
        }

        /// <summary>
        ///     Try remove
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveTail(out T result)
        {
            var handle = _handle;
            var linkedList = (NativeLinkedList*)Unsafe.AsPointer(ref handle->LinkedList);
            if (linkedList->IsEmpty)
            {
                result = default;
                return false;
            }

            var node = linkedList->Tail->Previous;
            result = ((NativeLinkedListNode<T>*)node)->Item;
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
            result = ((NativeLinkedListNode<T>*)node)->Item;
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
            result = ((NativeLinkedListNode<T>*)node)->Item;
            return true;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeLinkedList<T> Empty => new();

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
                ref var node = ref _node;
                node = node->Next;
                return node != _nativeLinkedList->Tail;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ((NativeLinkedListNode<T>*)_node)->Item;
            }
        }
    }
}