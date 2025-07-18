﻿using System;
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
    ///     Unsafe priorityQueue
    /// </summary>
    /// <typeparam name="TElement">Type</typeparam>
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafePriorityQueue<TElement, TPriority> : IDisposable where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
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
        public readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly (TElement Element, TPriority Priority) this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Unordered items
        /// </summary>
        public UnorderedItemsCollection UnorderedItems => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafePriorityQueue(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity < 4)
                capacity = 4;
            _nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)capacity);
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_nodes);

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
        ///     Enqueue
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="priority">Priority</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TElement element, in TPriority priority)
        {
            var size = _size;
            ++_version;
            if (_length == size)
                Grow(size + 1);
            _size = size + 1;
            MoveUp((element, priority), size);
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
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
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
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
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
            var element = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0).Element;
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
                var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
                element = tuple.Element;
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
                var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
                element = tuple.Element;
                priority = tuple.Priority;
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
            var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0) = (element, priority);
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

            var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0) = (element, priority);
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
            return Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0).Element;
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
                var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)0);
                element = tuple.Element;
                priority = tuple.Priority;
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
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (_length < capacity)
            {
                Grow(capacity);
                ++_version;
            }

            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            if (_size >= (int)(_length * 0.9))
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)_size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * sizeof((TElement Element, TPriority Priority))));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _size;
            ++_version;
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity < _size || capacity >= _length)
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)_size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * sizeof((TElement Element, TPriority Priority))));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _size;
            ++_version;
            return _length;
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
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = _length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)newCapacity);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * sizeof((TElement Element, TPriority Priority))));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = newCapacity;
        }

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
                var second = minSize <= size ? minSize : size;
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
        public static UnsafePriorityQueue<TElement, TPriority> Empty => new();

        /// <summary>
        ///     Unordered items collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly UnsafePriorityQueue<TElement, TPriority>* _nativePriorityQueue;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _nativePriorityQueue->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativePriorityQueue">Native priorityQueue</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(void* nativePriorityQueue) => _nativePriorityQueue = (UnsafePriorityQueue<TElement, TPriority>*)nativePriorityQueue;

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
                private readonly UnsafePriorityQueue<TElement, TPriority>* _nativePriorityQueue;

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
                    var handle = (UnsafePriorityQueue<TElement, TPriority>*)nativePriorityQueue;
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