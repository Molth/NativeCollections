using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native priorityQueue
    /// </summary>
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe struct NativePriorityQueue<TPriority> : IDisposable, IEquatable<NativePriorityQueue<TPriority>> where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativePriorityQueueHandle
        {
            /// <summary>
            ///     Nodes
            /// </summary>
            public TPriority* Nodes;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Version
            /// </summary>
            public int Version;

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public TPriority this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Nodes[index];
            }

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                Size = 0;
                ++Version;
            }

            /// <summary>
            ///     Enqueue
            /// </summary>
            /// <param name="priority">Priority</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue(in TPriority priority)
            {
                var size = Size;
                ++Version;
                if (Length == size)
                    Grow(size + 1);
                Size = size + 1;
                MoveUp(priority, size);
            }

            /// <summary>
            ///     Try enqueue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(in TPriority priority)
            {
                var size = Size;
                ++Version;
                if (Length != size)
                {
                    Size = size + 1;
                    MoveUp(priority, size);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Enqueue dequeue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <returns>Priority</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TPriority EnqueueDequeue(in TPriority priority)
            {
                if (Size != 0)
                {
                    var node = Nodes[0];
                    if (priority.CompareTo(node) > 0)
                    {
                        MoveDown(priority, 0);
                        ++Version;
                        return node;
                    }
                }

                return priority;
            }

            /// <summary>
            ///     Try enqueue dequeue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <param name="result">Priority</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueueDequeue(in TPriority priority, out TPriority result)
            {
                if (Size != 0)
                {
                    var node = Nodes[0];
                    if (priority.CompareTo(node) > 0)
                    {
                        MoveDown(priority, 0);
                        ++Version;
                        result = node;
                        return true;
                    }
                }

                result = priority;
                return false;
            }

            /// <summary>
            ///     Dequeue
            /// </summary>
            /// <returns>Priority</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TPriority Dequeue()
            {
                if (Size == 0)
                    throw new InvalidOperationException("EmptyQueue");
                var priority = Nodes[0];
                RemoveRootNode();
                return priority;
            }

            /// <summary>
            ///     Try dequeue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <returns>Dequeued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out TPriority priority)
            {
                if (Size != 0)
                {
                    priority = Nodes[0];
                    RemoveRootNode();
                    return true;
                }

                priority = default;
                return false;
            }

            /// <summary>
            ///     Dequeue enqueue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <returns>Priority</returns>
            public TPriority DequeueEnqueue(in TPriority priority)
            {
                if (Size == 0)
                    throw new InvalidOperationException("EmptyQueue");
                var node = Nodes[0];
                if (priority.CompareTo(node) > 0)
                    MoveDown(priority, 0);
                else
                    Nodes[0] = priority;
                ++Version;
                return node;
            }

            /// <summary>
            ///     Try dequeue enqueue
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <param name="result">Priority</param>
            /// <returns>Dequeued</returns>
            public bool TryDequeueEnqueue(in TPriority priority, out TPriority result)
            {
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                var node = Nodes[0];
                if (priority.CompareTo(node) > 0)
                    MoveDown(priority, 0);
                else
                    Nodes[0] = priority;
                ++Version;
                result = node;
                return true;
            }

            /// <summary>
            ///     Peek
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TPriority Peek() => Size == 0 ? throw new InvalidOperationException("EmptyQueue") : Nodes[0];

            /// <summary>
            ///     Try peek
            /// </summary>
            /// <param name="priority">Priority</param>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek(out TPriority priority)
            {
                if (Size != 0)
                {
                    priority = Nodes[0];
                    return true;
                }

                priority = default;
                return false;
            }

            /// <summary>
            ///     Ensure capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int EnsureCapacity(int capacity)
            {
                if (capacity < 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");

                if (Length < capacity)
                {
                    Grow(capacity);
                    ++Version;
                }

                return Length;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            /// <returns>New capacity</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int TrimExcess()
            {
                if (Size >= (int)(Length * 0.9))
                    return Length;
                var nodes = (TPriority*)NativeMemoryAllocator.Alloc((uint)(Size * sizeof(TPriority)));
                Unsafe.CopyBlockUnaligned(nodes, Nodes, (uint)Size);
                NativeMemoryAllocator.Free(Nodes);
                Nodes = nodes;
                Length = Size;
                ++Version;
                return Length;
            }

            /// <summary>
            ///     Grow
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Grow(int capacity)
            {
                var newCapacity = 2 * Length;
                if ((uint)newCapacity > 2147483591)
                    newCapacity = 2147483591;
                var expected = Length + 4;
                newCapacity = newCapacity > expected ? newCapacity : expected;
                if (newCapacity < capacity)
                    newCapacity = capacity;
                var nodes = (TPriority*)NativeMemoryAllocator.Alloc((uint)(newCapacity * sizeof(TPriority)));
                Unsafe.CopyBlockUnaligned(nodes, Nodes, (uint)Size);
                NativeMemoryAllocator.Free(Nodes);
                Nodes = nodes;
                Length = newCapacity;
            }

            /// <summary>
            ///     Remove root node
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void RemoveRootNode()
            {
                var index = --Size;
                ++Version;
                if (index > 0)
                {
                    var node = Nodes[index];
                    MoveDown(node, 0);
                }
            }

            /// <summary>
            ///     Move up
            /// </summary>
            /// <param name="node">Node</param>
            /// <param name="nodeIndex">Node index</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void MoveUp(in TPriority node, int nodeIndex)
            {
                var nodes = Nodes;
                int parentIndex;
                for (; nodeIndex > 0; nodeIndex = parentIndex)
                {
                    parentIndex = (nodeIndex - 1) >> 2;
                    var priority = nodes[parentIndex];
                    if (node.CompareTo(priority) < 0)
                        nodes[nodeIndex] = priority;
                    else
                        break;
                }

                nodes[nodeIndex] = node;
            }

            /// <summary>
            ///     Move down
            /// </summary>
            /// <param name="node">Node</param>
            /// <param name="nodeIndex">Node index</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void MoveDown(in TPriority node, int nodeIndex)
            {
                var nodes = Nodes;
                int firstChildIndex;
                int first;
                for (var size = Size; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
                {
                    var priority1 = nodes[firstChildIndex];
                    first = firstChildIndex;
                    var minSize = firstChildIndex + 4;
                    var second = minSize <= size ? minSize : size;
                    while (++firstChildIndex < second)
                    {
                        var priority2 = nodes[firstChildIndex];
                        if (priority2.CompareTo(priority1) < 0)
                        {
                            priority1 = priority2;
                            first = firstChildIndex;
                        }
                    }

                    if (node.CompareTo(priority1) > 0)
                        nodes[nodeIndex] = priority1;
                    else
                        break;
                }

                nodes[nodeIndex] = node;
            }
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativePriorityQueueHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePriorityQueue(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativePriorityQueueHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativePriorityQueueHandle));
            handle->Nodes = (TPriority*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(TPriority)));
            handle->Length = capacity;
            handle->Size = 0;
            handle->Version = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Size == 0;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public TPriority this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[index];
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Size;

        /// <summary>
        ///     Unordered items
        /// </summary>
        public UnorderedItemsCollection UnorderedItems => new(_handle);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativePriorityQueue<TPriority> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativePriorityQueue<TPriority> nativeQueue && nativeQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativePriorityQueue<{typeof(TPriority).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativePriorityQueue<TPriority> left, NativePriorityQueue<TPriority> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativePriorityQueue<TPriority> left, NativePriorityQueue<TPriority> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Nodes);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _handle->Clear();
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TPriority priority)
        {
            _handle->Enqueue(priority);
        }

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in TPriority priority) => _handle->TryEnqueue(priority);

        /// <summary>
        ///     Enqueue dequeue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Priority</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority EnqueueDequeue(in TPriority priority) => _handle->EnqueueDequeue(priority);

        /// <summary>
        ///     Try enqueue dequeue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <param name="result">Priority</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueDequeue(in TPriority priority, out TPriority result) => _handle->TryEnqueueDequeue(priority, out result);

        /// <summary>
        ///     Dequeue
        /// </summary>
        /// <returns>Priority</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TPriority priority) => _handle->TryDequeue(out priority);

        /// <summary>
        ///     Dequeue enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Priority</returns>
        public TPriority DequeueEnqueue(in TPriority priority) => _handle->DequeueEnqueue(priority);

        /// <summary>
        ///     Try dequeue enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <param name="result">Priority</param>
        /// <returns>Dequeued</returns>
        public bool TryDequeueEnqueue(in TPriority priority, out TPriority result) => _handle->TryDequeueEnqueue(priority, out result);

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Peek() => _handle->Peek();

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TPriority priority) => _handle->TryPeek(out priority);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativePriorityQueue<TPriority> Empty => new();

        /// <summary>
        ///     Unordered items collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly NativePriorityQueueHandle* _nativePriorityQueue;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativePriorityQueue">Native priorityQueue</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(void* nativePriorityQueue) => _nativePriorityQueue = (NativePriorityQueueHandle*)nativePriorityQueue;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativePriorityQueue);

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativePriorityQueue
                /// </summary>
                private readonly NativePriorityQueueHandle* _nativePriorityQueue;

                /// <summary>
                ///     Version
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Current
                /// </summary>
                private TPriority _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativePriorityQueue">Native priorityQueue</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativePriorityQueue)
                {
                    var handle = (NativePriorityQueueHandle*)nativePriorityQueue;
                    _nativePriorityQueue = handle;
                    _index = 0;
                    _version = handle->Version;
                    _current = default;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _nativePriorityQueue;
                    if (_version != handle->Version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index >= (uint)handle->Size)
                    {
                        _index = handle->Size + 1;
                        _current = default;
                        return false;
                    }

                    _current = handle->Nodes[_index];
                    ++_index;
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public TPriority Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}