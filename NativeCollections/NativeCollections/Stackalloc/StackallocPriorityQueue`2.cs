using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc priorityQueue
    /// </summary>
    /// <typeparam name="TElement">Type</typeparam>
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocPriorityQueue<TElement, TPriority> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Nodes
        /// </summary>
        private (TElement Element, TPriority Priority)* _nodes;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref readonly (TElement Element, TPriority Priority) this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Unordered items
        /// </summary>
        public UnorderedItemsCollection UnorderedItems => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => capacity * sizeof((TElement Element, TPriority Priority)) + (int)NativeMemoryAllocator.AlignOf<(TElement Element, TPriority Priority)>() - 1;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocPriorityQueue(Span<byte> buffer, int capacity)
        {
            _nodes = NativeArray<(TElement Element, TPriority Priority)>.Create(buffer).Buffer;
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _size = 0;
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                return false;
            var nodes = _nodes;
            var priority = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)index).Priority;
            var num = --_size;
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)num);
                if (node.Priority.CompareTo(priority) < 0)
                    MoveUp(node, index);
                else
                    MoveDown(node, index);
            }

            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index, out TElement element, out TPriority priority)
        {
            if ((uint)index >= (uint)_size)
            {
                element = default;
                priority = default;
                return false;
            }

            var nodes = _nodes;
            (element, priority) = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)index);
            var num = --_size;
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)num);
                if (node.Priority.CompareTo(priority) < 0)
                    MoveUp(node, index);
                else
                    MoveDown(node, index);
            }

            ++_version;
            return true;
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
            var size = _size;
            if (_length != size)
            {
                _size = size + 1;
                MoveUp((element, priority), size);
                ++_version;
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
            if (_size != 0)
            {
                var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
                if (priority.CompareTo(node.Priority) > 0)
                {
                    MoveDown((element, priority), 0);
                    ++_version;
                    return node.Element;
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
            if (_size != 0)
            {
                var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
                if (priority.CompareTo(node.Priority) > 0)
                {
                    MoveDown((element, priority), 0);
                    ++_version;
                    result = node.Element;
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
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
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
            if (_size != 0)
            {
                element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
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
            if (_size != 0)
            {
                (element, priority) = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
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
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes) = (element, priority);
            ++_version;
            return node.Element;
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
            if (_size == 0)
            {
                result = default;
                return false;
            }

            var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes) = (element, priority);
            ++_version;
            result = node.Element;
            return true;
        }

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TElement Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            return Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
        }

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_size != 0)
            {
                (element, priority) = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
                return true;
            }

            element = default;
            priority = default;
            return false;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), _size);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)start), _size - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)start), length);

        /// <summary>
        ///     Remove root node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveRootNode()
        {
            var index = --_size;
            ++_version;
            if (index > 0)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
                MoveDown(node, 0);
            }
        }

        /// <summary>
        ///     Move up
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="nodeIndex">Node index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveUp(in (TElement Element, TPriority Priority) node, int nodeIndex)
        {
            var nodes = _nodes;
            int parentIndex;
            for (; nodeIndex > 0; nodeIndex = parentIndex)
            {
                parentIndex = (nodeIndex - 1) >> 2;
                var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)parentIndex);
                if (node.Priority.CompareTo(tuple.Priority) < 0)
                    Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)nodeIndex) = tuple;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)nodeIndex) = node;
        }

        /// <summary>
        ///     Move down
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="nodeIndex">Node index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveDown(in (TElement Element, TPriority Priority) node, int nodeIndex)
        {
            var nodes = _nodes;
            int firstChildIndex;
            int first;
            for (var size = _size; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
            {
                var valueTuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)firstChildIndex);
                first = firstChildIndex;
                var minSize = firstChildIndex + 4;
                var second = Math.Min(minSize, size);
                while (++firstChildIndex < second)
                {
                    var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)firstChildIndex);
                    if (tuple.Priority.CompareTo(valueTuple.Priority) < 0)
                    {
                        valueTuple = tuple;
                        first = firstChildIndex;
                    }
                }

                if (node.Priority.CompareTo(valueTuple.Priority) > 0)
                    Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)nodeIndex) = valueTuple;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)nodeIndex) = node;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocPriorityQueue<TElement, TPriority> Empty => new();

        /// <summary>
        ///     Unordered items collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly StackallocPriorityQueue<TElement, TPriority>* _nativePriorityQueue;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativePriorityQueue->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativePriorityQueue">Native priorityQueue</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(void* nativePriorityQueue) => _nativePriorityQueue = (StackallocPriorityQueue<TElement, TPriority>*)nativePriorityQueue;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => _nativePriorityQueue->AsReadOnlySpan();

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start) => _nativePriorityQueue->AsReadOnlySpan(start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start, int length) => _nativePriorityQueue->AsReadOnlySpan(start, length);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_nativePriorityQueue);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            public struct Enumerator
            {
                /// <summary>
                ///     NativePriorityQueue
                /// </summary>
                private readonly StackallocPriorityQueue<TElement, TPriority>* _nativePriorityQueue;

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
                private (TElement Element, TPriority Priority) _current;

                /// <summary>
                ///     Structure
                /// </summary>
                /// <param name="nativePriorityQueue">Native priorityQueue</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(void* nativePriorityQueue)
                {
                    var handle = (StackallocPriorityQueue<TElement, TPriority>*)nativePriorityQueue;
                    _nativePriorityQueue = handle;
                    _index = 0;
                    _version = handle->_version;
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
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    if ((uint)_index >= (uint)handle->_size)
                    {
                        _index = handle->_size + 1;
                        _current = default;
                        return false;
                    }

                    _current = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(handle->_nodes), (nint)_index);
                    ++_index;
                    return true;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public readonly (TElement Element, TPriority Priority) Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}