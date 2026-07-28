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
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _size;

        /// <summary>
        ///     Used to keep enumerator in sync w/ collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_nodes);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref readonly TPriority this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref readonly TPriority this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Gets a collection that enumerates the elements of the queue in an unordered manner.
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafePriorityQueue<TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafePriorityQueue<TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafePriorityQueue<{0}>", SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafePriorityQueue<TPriority> left, UnsafePriorityQueue<TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafePriorityQueue<TPriority> left, UnsafePriorityQueue<TPriority> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_nodes);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _size = 0;
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
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
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="priority">The priority value associated with the removed element.</param>
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
        ///     Adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
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
        ///     Attempts to adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if the this is already full and the item could not be added.
        /// </returns>
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
        ///     Adds the specified element with associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
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
        ///     Attempts to adds the specified element with associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <param name="result">
        ///     When this method returns, the minimal element removed after the enqueue operation;
        ///     otherwise, the default value for the type of the <paramref name="result" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if the this is already full and the item could not be added.
        /// </returns>
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
        ///     Removes and returns the object at the beginning of this.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object that is removed from the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Dequeue()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            var priority = Unsafe.AsRef<TPriority>(_nodes);
            RemoveRootNode();
            return priority;
        }

        /// <summary>
        ///     Removes the minimal element from this,
        ///     and copies it and its associated priority to the <paramref name="priority" />.
        /// </summary>
        /// <param name="priority">When this method returns, contains the priority associated with the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully removed; <see langword="false" /> if this is empty.
        /// </returns>
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
        ///     Removes the minimal element and then immediately adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        ///     Removes the minimal element and then immediately adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <param name="result">
        ///     When this method returns, the minimal element removed after the enqueue operation;
        ///     otherwise, the default value for the type of the <paramref name="result" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully removed to this;
        ///     <see langword="false" /> if the this is already empty and the item could not be removed.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object at the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TPriority Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_size);
            return Unsafe.AsRef<TPriority>(_nodes);
        }

        /// <summary>
        ///     Returns a value that indicates whether there is a minimal element in this,
        ///     and if one is present, copies it and its associated priority to the <paramref name="priority" />.
        ///     The element is not removed from this.
        /// </summary>
        /// <param name="priority">When this method returns, contains the priority associated with the minimal element.</param>
        /// <returns>
        ///     <see langword="true" /> if there is a minimal element;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
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
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
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
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            if (_size >= (int)(_length * 0.9))
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)_size);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _size;
            ++_version;
            return _length;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _size || capacity >= _length)
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<TPriority>((uint)_size);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _size;
            ++_version;
            return _length;
        }

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<TPriority> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<TPriority>(_nodes), _size);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<TPriority>(_nodes), (nint)start), _size - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
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
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_size * Unsafe.SizeOf<TPriority>()));
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
        public static UnsafePriorityQueue<TPriority> Empty => default;

        /// <summary>
        ///     Represents the collection of items, without any ordering guarantees.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection : IIsCreated, IReadOnlyCollection<TPriority>
        {
            /// <summary>
            ///     NativePriorityQueue
            /// </summary>
            private readonly UnsafePriorityQueue<TPriority>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(UnsafePriorityQueue<TPriority>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan() => _handle->AsReadOnlySpan();

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<TPriority> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TPriority> IEnumerable<TPriority>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
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
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     Index
                /// </summary>
                private int _index;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
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
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
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
                ///     Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset()
                {
                    _index = 0;
                    _current = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                public readonly TPriority Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}