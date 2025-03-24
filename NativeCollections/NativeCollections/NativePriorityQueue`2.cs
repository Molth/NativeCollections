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
    /// <typeparam name="TElement">Type</typeparam>
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativePriorityQueue<TElement, TPriority> : IDisposable, IEquatable<NativePriorityQueue<TElement, TPriority>> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
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
            public ValueTuple<TElement, TPriority>* Nodes;

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
            public (TElement Element, TPriority Priority) this[int index]
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
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Enqueue(in TElement element, in TPriority priority)
            {
                var size = Size;
                ++Version;
                if (Length == size)
                    Grow(size + 1);
                Size = size + 1;
                MoveUp(new ValueTuple<TElement, TPriority>(element, priority), size);
            }

            /// <summary>
            ///     Try enqueue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueue(in TElement element, in TPriority priority)
            {
                var size = Size;
                ++Version;
                if (Length != size)
                {
                    Size = size + 1;
                    MoveUp(new ValueTuple<TElement, TPriority>(element, priority), size);
                    return true;
                }

                return false;
            }

            /// <summary>
            ///     Enqueue dequeue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <returns>Element</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TElement EnqueueDequeue(in TElement element, in TPriority priority)
            {
                if (Size != 0)
                {
                    var node = Nodes[0];
                    if (priority.CompareTo(node.Item2) > 0)
                    {
                        MoveDown(new ValueTuple<TElement, TPriority>(element, priority), 0);
                        ++Version;
                        return node.Item1;
                    }
                }

                return element;
            }

            /// <summary>
            ///     Try enqueue dequeue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <param name="result">Element</param>
            /// <returns>Enqueued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryEnqueueDequeue(in TElement element, in TPriority priority, out TElement result)
            {
                if (Size != 0)
                {
                    var node = Nodes[0];
                    if (priority.CompareTo(node.Item2) > 0)
                    {
                        MoveDown(new ValueTuple<TElement, TPriority>(element, priority), 0);
                        ++Version;
                        result = node.Item1;
                        return true;
                    }
                }

                result = element;
                return false;
            }

            /// <summary>
            ///     Dequeue
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TElement Dequeue()
            {
                if (Size == 0)
                    throw new InvalidOperationException("EmptyQueue");
                var element = Nodes[0].Item1;
                RemoveRootNode();
                return element;
            }

            /// <summary>
            ///     Try dequeue
            /// </summary>
            /// <param name="element">Element</param>
            /// <returns>Dequeued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out TElement element)
            {
                if (Size != 0)
                {
                    var tuple = Nodes[0];
                    element = tuple.Item1;
                    RemoveRootNode();
                    return true;
                }

                element = default;
                return false;
            }

            /// <summary>
            ///     Try dequeue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <returns>Dequeued</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryDequeue(out TElement element, out TPriority priority)
            {
                if (Size != 0)
                {
                    var tuple = Nodes[0];
                    element = tuple.Item1;
                    priority = tuple.Item2;
                    RemoveRootNode();
                    return true;
                }

                element = default;
                priority = default;
                return false;
            }

            /// <summary>
            ///     Dequeue enqueue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <returns>Element</returns>
            public TElement DequeueEnqueue(in TElement element, in TPriority priority)
            {
                if (Size == 0)
                    throw new InvalidOperationException("EmptyQueue");
                var node = Nodes[0];
                if (priority.CompareTo(node.Item2) > 0)
                    MoveDown(new ValueTuple<TElement, TPriority>(element, priority), 0);
                else
                    Nodes[0] = new ValueTuple<TElement, TPriority>(element, priority);
                ++Version;
                return node.Item1;
            }

            /// <summary>
            ///     Try dequeue enqueue
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <param name="result">Element</param>
            /// <returns>Dequeued</returns>
            public bool TryDequeueEnqueue(in TElement element, in TPriority priority, out TElement result)
            {
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                var node = Nodes[0];
                if (priority.CompareTo(node.Item2) > 0)
                    MoveDown(new ValueTuple<TElement, TPriority>(element, priority), 0);
                else
                    Nodes[0] = new ValueTuple<TElement, TPriority>(element, priority);
                ++Version;
                result = node.Item1;
                return true;
            }

            /// <summary>
            ///     Peek
            /// </summary>
            /// <returns>Item</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TElement Peek() => Size == 0 ? throw new InvalidOperationException("EmptyQueue") : Nodes[0].Item1;

            /// <summary>
            ///     Try peek
            /// </summary>
            /// <param name="element">Element</param>
            /// <param name="priority">Priority</param>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek(out TElement element, out TPriority priority)
            {
                if (Size != 0)
                {
                    var tuple = Nodes[0];
                    element = tuple.Item1;
                    priority = tuple.Item2;
                    return true;
                }

                element = default;
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
                var nodes = (ValueTuple<TElement, TPriority>*)NativeMemoryAllocator.Alloc((uint)(Size * sizeof(ValueTuple<TElement, TPriority>)));
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
                var nodes = (ValueTuple<TElement, TPriority>*)NativeMemoryAllocator.Alloc((uint)(newCapacity * sizeof(ValueTuple<TElement, TPriority>)));
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
            private void MoveUp(in ValueTuple<TElement, TPriority> node, int nodeIndex)
            {
                var nodes = Nodes;
                int parentIndex;
                for (; nodeIndex > 0; nodeIndex = parentIndex)
                {
                    parentIndex = (nodeIndex - 1) >> 2;
                    var tuple = nodes[parentIndex];
                    if (node.Item2.CompareTo(tuple.Item2) < 0)
                        nodes[nodeIndex] = tuple;
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
            private void MoveDown(in ValueTuple<TElement, TPriority> node, int nodeIndex)
            {
                var nodes = Nodes;
                int firstChildIndex;
                int first;
                for (var size = Size; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
                {
                    var valueTuple = nodes[firstChildIndex];
                    first = firstChildIndex;
                    var minSize = firstChildIndex + 4;
                    var second = minSize <= size ? minSize : size;
                    while (++firstChildIndex < second)
                    {
                        var tuple = nodes[firstChildIndex];
                        if (tuple.Item2.CompareTo(valueTuple.Item2) < 0)
                        {
                            valueTuple = tuple;
                            first = firstChildIndex;
                        }
                    }

                    if (node.Item2.CompareTo(valueTuple.Item2) > 0)
                        nodes[nodeIndex] = valueTuple;
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
            handle->Nodes = (ValueTuple<TElement, TPriority>*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(ValueTuple<TElement, TPriority>)));
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
        public (TElement Element, TPriority Priority) this[int index]
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
        public bool Equals(NativePriorityQueue<TElement, TPriority> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativePriorityQueue<TElement, TPriority> nativeQueue && nativeQueue == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativePriorityQueue<{typeof(TElement).Name}, {typeof(TPriority).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => left._handle != right._handle;

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
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TElement element, in TPriority priority)
        {
            _handle->Enqueue(element, priority);
        }

        /// <summary>
        ///     Try enqueue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in TElement element, in TPriority priority) => _handle->TryEnqueue(element, priority);

        /// <summary>
        ///     Enqueue dequeue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement EnqueueDequeue(in TElement element, in TPriority priority) => _handle->EnqueueDequeue(element, priority);

        /// <summary>
        ///     Try enqueue dequeue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <param name="result">Element</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueDequeue(in TElement element, in TPriority priority, out TElement result) => _handle->TryEnqueueDequeue(element, priority, out result);

        /// <summary>
        ///     Dequeue
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element) => _handle->TryDequeue(out element);

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element, out TPriority priority) => _handle->TryDequeue(out element, out priority);

        /// <summary>
        ///     Dequeue enqueue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Element</returns>
        public TElement DequeueEnqueue(in TElement element, in TPriority priority) => _handle->DequeueEnqueue(element, priority);

        /// <summary>
        ///     Try dequeue enqueue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <param name="result">Element</param>
        /// <returns>Dequeued</returns>
        public bool TryDequeueEnqueue(in TElement element, in TPriority priority, out TElement result) => _handle->TryDequeueEnqueue(element, priority, out result);

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Peek() => _handle->Peek();

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TElement element, out TPriority priority) => _handle->TryPeek(out element, out priority);

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
        public static NativePriorityQueue<TElement, TPriority> Empty => new();

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
                private ValueTuple<TElement, TPriority> _current;

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
                public (TElement Element, TPriority Priority) Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}