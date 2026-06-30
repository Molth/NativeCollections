using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe priorityQueue
    /// </summary>
    /// <typeparam name="TPriority">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafePriorityQueue<TPriority> : IIsCreated, IDisposable, IEquatable<UnsafePriorityQueue<TPriority>> where TPriority : unmanaged, IComparable<TPriority>
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
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_nodes);

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
        public readonly ref readonly TPriority this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref readonly TPriority this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Unordered items
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public UnorderedItemsCollection UnorderedItems => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafePriorityQueue(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)capacity);
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafePriorityQueue<TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafePriorityQueue<TPriority> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafePriorityQueue<{0}>", SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafePriorityQueue<TPriority> left, UnsafePriorityQueue<TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafePriorityQueue<TPriority> left, UnsafePriorityQueue<TPriority> right) => !left.Equals(right);

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
        ///     Remove at
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                return false;
            var nodes = _nodes;
            var priority = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)index);
            var num = --_size;
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)num);
                if (node.CompareTo(priority) < 0)
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
        public bool RemoveAt(int index, out TPriority priority)
        {
            if ((uint)index >= (uint)_size)
            {
                priority = default;
                return false;
            }

            var nodes = _nodes;
            priority = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)index);
            var num = --_size;
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)num);
                if (node.CompareTo(priority) < 0)
                    MoveUp(node, index);
                else
                    MoveDown(node, index);
            }

            ++_version;
            return true;
        }

        /// <summary>
        ///     Enqueue
        /// </summary>
        /// <param name="priority">Priority</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TPriority priority)
        {
            var size = _size;
            ++_version;
            if (_length == size)
                Grow(size + 1);
            _size = size + 1;
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
                var node = Unsafe.AsRef<TPriority>(_nodes);
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
                var node = Unsafe.AsRef<TPriority>(_nodes);
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
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var priority = Unsafe.AsRef<TPriority>(_nodes);
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
                priority = Unsafe.AsRef<TPriority>(_nodes);
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
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var node = Unsafe.AsRef<TPriority>(_nodes);
            if (priority.CompareTo(node) > 0)
                MoveDown(priority, 0);
            else
                Unsafe.AsRef<TPriority>(_nodes) = priority;
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

            var node = Unsafe.AsRef<TPriority>(_nodes);
            if (priority.CompareTo(node) > 0)
                MoveDown(priority, 0);
            else
                Unsafe.AsRef<TPriority>(_nodes) = priority;
            ++_version;
            result = node;
            return true;
        }

        /// <summary>
        ///     Peek
        /// </summary>
        /// <returns>Item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TPriority Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            return Unsafe.AsRef<TPriority>(_nodes);
        }

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="priority">Priority</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out TPriority priority)
        {
            if (_size != 0)
            {
                priority = Unsafe.AsRef<TPriority>(_nodes);
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
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
            var nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)_size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _size || capacity >= _length)
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)_size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
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
        public readonly ReadOnlySpan<TPriority> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TPriority>(_nodes), _size);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)start), _size - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<TPriority> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)start), length);

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > ArrayHelpers.MaxLength)
                newCapacity = ArrayHelpers.MaxLength;
            var expected = _length + 4;
            newCapacity = Math.Max(newCapacity, expected);
            newCapacity = Math.Max(newCapacity, capacity);
            var nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)newCapacity);
            Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
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
                var node = Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)index);
                MoveDown(node, 0);
            }
        }

        /// <summary>
        ///     Move up
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="nodeIndex">Node index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveUp(in TPriority node, int nodeIndex)
        {
            var nodes = _nodes;
            int parentIndex;
            for (; nodeIndex > 0; nodeIndex = parentIndex)
            {
                parentIndex = (nodeIndex - 1) >> 2;
                var priority = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)parentIndex);
                if (node.CompareTo(priority) < 0)
                    Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)nodeIndex) = priority;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)nodeIndex) = node;
        }

        /// <summary>
        ///     Move down
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="nodeIndex">Node index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveDown(in TPriority node, int nodeIndex)
        {
            var nodes = _nodes;
            int firstChildIndex;
            int first;
            for (var size = _size; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
            {
                var priority1 = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)firstChildIndex);
                first = firstChildIndex;
                var minSize = firstChildIndex + 4;
                var second = Math.Min(minSize, size);
                while (++firstChildIndex < second)
                {
                    var priority2 = Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)firstChildIndex);
                    if (priority2.CompareTo(priority1) < 0)
                    {
                        priority1 = priority2;
                        first = firstChildIndex;
                    }
                }

                if (node.CompareTo(priority1) > 0)
                    Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)nodeIndex) = priority1;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<TPriority>(nodes), (nint)nodeIndex) = node;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafePriorityQueue<TPriority> Empty => new();

        /// <summary>
        ///     Unordered items collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection : IIsCreated, IReadOnlyCollection<TPriority>
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly UnsafePriorityQueue<TPriority>* _handle;

            /// <summary>
            ///     Is created
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(UnsafePriorityQueue<TPriority>* handle) => _handle = handle;

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan() => _handle->AsReadOnlySpan();

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TPriority> IEnumerable<TPriority>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator IEnumerable.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Enumerator
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<TPriority>
            {
                /// <summary>
                ///     NativePriorityQueue
                /// </summary>
                private readonly UnsafePriorityQueue<TPriority>* _handle;

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
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafePriorityQueue<TPriority>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _current = default;
                }

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    if ((uint)_index >= (uint)handle->_size)
                    {
                        _index = handle->_size + 1;
                        _current = default;
                        return false;
                    }

                    _current = Unsafe.Add(ref Unsafe.AsRef<TPriority>(handle->_nodes), (nint)_index);
                    ++_index;
                    return true;
                }

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = 0;
                    _current = default;
                }

                /// <summary>
                ///     Current
                /// </summary>
                public readonly TPriority Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}