﻿using System;
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
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocPriorityQueue<TPriority> where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Nodes
        /// </summary>
        private TPriority* _nodes;

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
        public bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public TPriority this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _nodes[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public TPriority this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _nodes[index];
        }

        /// <summary>
        ///     Unordered items
        /// </summary>
        public UnorderedItemsCollection UnorderedItems => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get buffer size
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Buffer size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize(int capacity) => capacity * sizeof(TPriority);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocPriorityQueue(Span<byte> buffer, int capacity)
        {
            _nodes = (TPriority*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
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
        ///     Try enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Enqueued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in TPriority priority)
        {
            var size = _size;
            if (_length != size)
            {
                _size = size + 1;
                MoveUp(priority, size);
                ++_version;
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
            if (_size != 0)
            {
                var node = _nodes[0];
                if (priority.CompareTo(node) > 0)
                {
                    MoveDown(priority, 0);
                    ++_version;
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
            if (_size != 0)
            {
                var node = _nodes[0];
                if (priority.CompareTo(node) > 0)
                {
                    MoveDown(priority, 0);
                    ++_version;
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
            if (_size == 0)
                throw new InvalidOperationException("EmptyQueue");
            var priority = _nodes[0];
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
            if (_size != 0)
            {
                priority = _nodes[0];
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
            if (_size == 0)
                throw new InvalidOperationException("EmptyQueue");
            var node = _nodes[0];
            if (priority.CompareTo(node) > 0)
                MoveDown(priority, 0);
            else
                _nodes[0] = priority;
            ++_version;
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
            if (_size == 0)
            {
                result = default;
                return false;
            }

            var node = _nodes[0];
            if (priority.CompareTo(node) > 0)
                MoveDown(priority, 0);
            else
                _nodes[0] = priority;
            ++_version;
            result = node;
            return true;
        }

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Peek() => _size == 0 ? throw new InvalidOperationException("EmptyQueue") : _nodes[0];

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TPriority priority)
        {
            if (_size != 0)
            {
                priority = _nodes[0];
                return true;
            }

            priority = default;
            return false;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TPriority> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_nodes, _size);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_nodes + start), _size - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TPriority> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_nodes + start), length);

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
                var node = _nodes[index];
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
            var nodes = _nodes;
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
            var nodes = _nodes;
            int firstChildIndex;
            int first;
            for (var size = _size; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
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

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocPriorityQueue<TPriority> Empty => new();

        /// <summary>
        ///     Unordered items collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly StackallocPriorityQueue<TPriority>* _nativePriorityQueue;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativePriorityQueue">Native priorityQueue</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(void* nativePriorityQueue) => _nativePriorityQueue = (StackallocPriorityQueue<TPriority>*)nativePriorityQueue;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan() => _nativePriorityQueue->AsReadOnlySpan();

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => _nativePriorityQueue->AsReadOnlySpan(start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start, int length) => _nativePriorityQueue->AsReadOnlySpan(start, length);

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
                private readonly StackallocPriorityQueue<TPriority>* _nativePriorityQueue;

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
                    var handle = (StackallocPriorityQueue<TPriority>*)nativePriorityQueue;
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
                    if (_version != handle->_version)
                        throw new InvalidOperationException("EnumFailedVersion");
                    if ((uint)_index >= (uint)handle->_size)
                    {
                        _index = handle->_size + 1;
                        _current = default;
                        return false;
                    }

                    _current = handle->_nodes[_index];
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