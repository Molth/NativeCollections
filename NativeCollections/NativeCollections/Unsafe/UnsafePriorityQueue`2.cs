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
    ///     Represents a collection of items that have a value and a priority.
    ///     On dequeue, the item with the lowest priority value is removed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafePriorityQueue<TElement, TPriority> : IIsCreated, IDisposable, IEquatable<UnsafePriorityQueue<TElement, TPriority>> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private (TElement Element, TPriority Priority)* _nodes;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

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
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _length;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref readonly (TElement Element, TPriority Priority) this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
        }

        /// <summary>
        ///     Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public UnorderedItemsCollection UnorderedItems => new(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafePriorityQueue(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)capacity);
            _length = capacity;
            _count = 0;
            _version = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafePriorityQueue<TElement, TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafePriorityQueue<TElement, TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafePriorityQueue<{0}, {1}>", SR.GetTypeName(typeof(TElement)), SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafePriorityQueue<TElement, TPriority> left, UnsafePriorityQueue<TElement, TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafePriorityQueue<TElement, TPriority> left, UnsafePriorityQueue<TElement, TPriority> right) => !left.Equals(right);

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
            _count = 0;
            ++_version;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index)
        {
            if ((uint)index >= (uint)_count)
                return false;
            var nodes = _nodes;
            var priority = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)index).Priority;
            var num = --_count;
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
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="element">The actual element that got removed from the queue.</param>
        /// <param name="priority">The priority value associated with the removed element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index, out TElement element, out TPriority priority)
        {
            if ((uint)index >= (uint)_count)
            {
                element = default;
                priority = default;
                return false;
            }

            var nodes = _nodes;
            (element, priority) = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), (nint)index);
            var num = --_count;
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
        ///     Adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TElement element, in TPriority priority)
        {
            var size = _count;
            ++_version;
            if (_length == size)
                Grow(size + 1);
            _count = size + 1;
            MoveUp((element, priority), size);
        }

        /// <summary>
        ///     Attempts to adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if the this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(in TElement element, in TPriority priority)
        {
            var size = _count;
            if (_length != size)
            {
                _count = size + 1;
                MoveUp((element, priority), size);
                ++_version;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds the specified element with associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement EnqueueDequeue(in TElement element, in TPriority priority)
        {
            if (_count != 0)
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
        ///     Attempts to adds the specified element with associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
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
        public bool TryEnqueueDequeue(in TElement element, in TPriority priority, out TElement result)
        {
            if (_count != 0)
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
        ///     Removes and returns the object at the beginning of this.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object that is removed from the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Dequeue()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            var element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
            RemoveRootNode();
            return element;
        }

        /// <summary>
        ///     Removes the minimal element from this,
        ///     and copies it and its associated priority to the <paramref name="element" />.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element)
        {
            if (_count != 0)
            {
                element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
                RemoveRootNode();
                return true;
            }

            element = default;
            return false;
        }

        /// <summary>
        ///     Removes the minimal element from this,
        ///     and copies it and its associated priority to the <paramref name="element" />
        ///     and <paramref name="priority" /> arguments.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <param name="priority">When this method returns, contains the priority associated with the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_count != 0)
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
        ///     Removes the minimal element and then immediately adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement DequeueEnqueue(in TElement element, in TPriority priority)
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes) = (element, priority);
            ++_version;
            return node.Element;
        }

        /// <summary>
        ///     Removes the minimal element and then immediately adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
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
        public bool TryDequeueEnqueue(in TElement element, in TPriority priority, out TElement result)
        {
            if (_count == 0)
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
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object at the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TElement Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            return Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes).Element;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is a minimal element in this,
        ///     and if one is present, copies it and its associated priority to the <paramref name="element" /> and
        ///     <paramref name="priority" /> arguments.
        ///     The element is not removed from this.
        /// </summary>
        /// <param name="element">When this method returns, contains the minimal element in the queue.</param>
        /// <param name="priority">When this method returns, contains the priority associated with the minimal element.</param>
        /// <returns>
        ///     <see langword="true" /> if there is a minimal element;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_count != 0)
            {
                (element, priority) = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes);
                return true;
            }

            element = default;
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
            if (_count >= (int)(_length * 0.9))
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)_count);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_count * Unsafe.SizeOf<(TElement Element, TPriority Priority)>()));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _count;
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
            if (capacity < _count || capacity >= _length)
                return _length;
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)_count);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_count * Unsafe.SizeOf<(TElement Element, TPriority Priority)>()));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = _count;
            ++_version;
            return _length;
        }

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), _count);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)start), _count - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)start), length);

        /// <summary>
        ///     Increases the capacity of this to a new size
        ///     that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > ArrayHelpers.MaxLength)
                newCapacity = ArrayHelpers.MaxLength;
            var expected = _length + 4;
            newCapacity = Math.Max(newCapacity, expected);
            newCapacity = Math.Max(newCapacity, capacity);
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>((uint)newCapacity);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_nodes), (uint)(_count * Unsafe.SizeOf<(TElement Element, TPriority Priority)>()));
            NativeMemoryAllocator.AlignedFree(_nodes);
            _nodes = nodes;
            _length = newCapacity;
        }

        /// <summary>
        ///     Removes the root node (the minimum priority element) from the heap,
        ///     and rebalances the heap by moving the last element down.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveRootNode()
        {
            var index = --_count;
            ++_version;
            if (index > 0)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_nodes), (nint)index);
                MoveDown(node, 0);
            }
        }

        /// <summary>
        ///     Moves a node upward in the heap to restore heap order,
        ///     assuming the node's priority is lower than its parent's.
        /// </summary>
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
        ///     Moves a node downward in the heap to restore heap order,
        ///     assuming the node's priority is greater than its children's.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveDown(in (TElement Element, TPriority Priority) node, int nodeIndex)
        {
            var nodes = _nodes;
            int firstChildIndex;
            int first;
            for (var size = _count; (firstChildIndex = (nodeIndex << 2) + 1) < size; nodeIndex = first)
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
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafePriorityQueue<TElement, TPriority> Empty => default;

        /// <summary>
        ///     Represents the collection of items, without any ordering guarantees.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct UnorderedItemsCollection : IIsCreated, IReadOnlyCollection<(TElement Element, TPriority Priority)>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly UnsafePriorityQueue<TElement, TPriority>* _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal UnorderedItemsCollection(UnsafePriorityQueue<TElement, TPriority>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => _handle->AsReadOnlySpan();

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

            /// <summary>
            ///     Creates a new read-only span over a portion of a regular managed object.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public Enumerator GetEnumerator() => new(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator()
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
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Enumerator : IIterator<(TElement Element, TPriority Priority)>
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly UnsafePriorityQueue<TElement, TPriority>* _handle;

                /// <summary>
                ///     Used to keep enumerator in sync w/ collection.
                /// </summary>
                private readonly int _version;

                /// <summary>
                ///     The current index.
                /// </summary>
                private int _index;

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <returns>The element in the collection at the current position of the enumerator.</returns>
                private (TElement Element, TPriority Priority) _current;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(UnsafePriorityQueue<TElement, TPriority>* handle)
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
                    if ((uint)_index >= (uint)handle->_count)
                    {
                        _index = handle->_count + 1;
                        _current = default;
                        return false;
                    }

                    _current = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(handle->_nodes), (nint)_index);
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
                public readonly (TElement Element, TPriority Priority) Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}