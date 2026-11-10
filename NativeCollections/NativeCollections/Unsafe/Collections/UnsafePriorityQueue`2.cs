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
    /// <typeparam name="TElement">The type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">The type of the priority values.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafePriorityQueue<TElement, TPriority> : IIsCreated, IDisposable, IEquatable<UnsafePriorityQueue<TElement, TPriority>> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private (TElement Element, TPriority Priority)* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Used to keep the enumerator synchronized with the collection.
        /// </summary>
        private int _version;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="buffer">The buffer containing the elements of the queue.</param>
        /// <param name="capacity">The total number of elements the internal data structure can hold.</param>
        /// <param name="count">The number of elements contained in this.</param>
        /// <param name="version">Used to keep the enumerator synchronized with the collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafePriorityQueue((TElement Element, TPriority Priority)* buffer, int capacity, int count, int version)
        {
            _buffer = buffer;
            _capacity = capacity;
            _count = count;
            _version = version;
        }

        /// <summary>
        ///     Creates a priority queue with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated priority queue.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafePriorityQueue<TElement, TPriority> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var buffer = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>(Usize.FromI32Unchecked(capacity));
            return new UnsafePriorityQueue<TElement, TPriority>(buffer, capacity, 0, 0);
        }

        /// <summary>
        ///     Gets a read-only reference to the element and priority of the node at the specified index in the heap.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the node in the heap, where index <c>0</c>
        ///     refers to the node that would be returned by the next dequeue operation.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        public readonly ref readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_count), ExceptionArgument.index);
                return ref Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer), Isize.FromI32Unchecked(index));
            }
        }

        /// <summary>
        ///     Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        [MustBePinned(SR.parameter_this)]
        public UnorderedItemsCollection UnorderedItems => UnorderedItemsCollection.From(UnsafeHelpers.AsPointer(ref this));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafePriorityQueue<TElement, TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafePriorityQueue<TElement, TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafePriorityQueue<{0}, {1}>", SR.GetTypeName(typeof(TElement)), SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafePriorityQueue<TElement, TPriority> left, UnsafePriorityQueue<TElement, TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafePriorityQueue<TElement, TPriority> left, UnsafePriorityQueue<TElement, TPriority> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
            _version = _version.WrappingAdd(1);
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index)
        {
            if (Usize.FromU32Unchecked(U32.FromI32Unchecked(index)) >= Usize.FromI32Unchecked(_count))
                return false;

            var nodes = _buffer;
            var priority = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(index)).Priority;
            var num = _count = _count.WrappingSub(1);
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(num));
                if (node.Priority.CompareTo(priority) < 0)
                    MoveUp(node, index);
                else
                    MoveDown(node, index);
            }

            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="element">The actual element that got removed from the queue.</param>
        /// <param name="priority">The priority value associated with the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index, out TElement element, out TPriority priority)
        {
            if (Usize.FromU32Unchecked(U32.FromI32Unchecked(index)) >= Usize.FromI32Unchecked(_count))
            {
                element = default;
                priority = default;
                return false;
            }

            var nodes = _buffer;
            (element, priority) = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(index));
            var num = _count = _count.WrappingSub(1);
            if (index < num)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(num));
                if (node.Priority.CompareTo(priority) < 0)
                    MoveUp(node, index);
                else
                    MoveDown(node, index);
            }

            _version = _version.WrappingAdd(1);
            return true;
        }

        /// <summary>
        ///     Adds the specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(TElement element, TPriority priority)
        {
            var size = _count;
            _version = _version.WrappingAdd(1);
            if (_capacity == size)
                Grow(size.WrappingAdd(1));

            _count = size.WrappingAdd(1);
            MoveUp((element, priority), size);
        }

        /// <summary>
        ///     Attempts to add the specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(TElement element, TPriority priority)
        {
            var size = _count;
            if (_capacity != size)
            {
                _count = size.WrappingAdd(1);
                MoveUp((element, priority), size);
                _version = _version.WrappingAdd(1);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds the specified element with the associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement EnqueueDequeue(TElement element, TPriority priority)
        {
            if (_count != 0)
            {
                var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
                if (priority.CompareTo(node.Priority) > 0)
                {
                    MoveDown((element, priority), 0);
                    _version = _version.WrappingAdd(1);
                    return node.Element;
                }
            }

            return element;
        }

        /// <summary>
        ///     Attempts to add the specified element with the associated priority to
        ///     this, and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <param name="result">
        ///     When this method returns, the minimal element removed by the operation if the method
        ///     returned <see langword="true" />; otherwise, the specified element.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified element was added to this and the minimal element was removed;
        ///     <see langword="false" /> if this was empty or the specified priority is not greater than the
        ///     current minimal priority, in which case <paramref name="result" /> receives the specified element
        ///     and no element is removed.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueDequeue(TElement element, TPriority priority, out TElement result)
        {
            if (_count != 0)
            {
                var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
                if (priority.CompareTo(node.Priority) > 0)
                {
                    MoveDown((element, priority), 0);
                    _version = _version.WrappingAdd(1);
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
        /// <returns>The object that is removed from the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Dequeue()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            var element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer).Element;
            RemoveRootNode();
            return element;
        }

        /// <summary>
        ///     Removes the minimal element from this and copies it to the <paramref name="element" />.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element)
        {
            if (_count != 0)
            {
                element = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer).Element;
                RemoveRootNode();
                return true;
            }

            element = default;
            return false;
        }

        /// <summary>
        ///     Removes the minimal element from this, and copies it and its associated priority
        ///     to the <paramref name="element" /> and <paramref name="priority" /> arguments.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <param name="priority">
        ///     When this method returns, contains the priority associated with the removed element.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element, out TPriority priority)
        {
            if (_count != 0)
            {
                (element, priority) = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
                RemoveRootNode();
                return true;
            }

            element = default;
            priority = default;
            return false;
        }

        /// <summary>
        ///     Removes the minimal element and then immediately adds the
        ///     specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement DequeueEnqueue(TElement element, TPriority priority)
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer) = (element, priority);

            _version = _version.WrappingAdd(1);
            return node.Element;
        }

        /// <summary>
        ///     Removes the minimal element and then immediately adds the
        ///     specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <param name="result">
        ///     When this method returns, the minimal element removed before the specified
        ///     element is added; otherwise, the default value for the type of the
        ///     <paramref name="result" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the minimal element was removed and the specified element was added
        ///     to this; <see langword="false" /> if this is empty and the specified element was not added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueEnqueue(TElement element, TPriority priority, out TElement result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            var node = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
            if (priority.CompareTo(node.Priority) > 0)
                MoveDown((element, priority), 0);
            else
                Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer) = (element, priority);

            _version = _version.WrappingAdd(1);
            result = node.Element;
            return true;
        }

        /// <summary>
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <returns>The object at the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TElement Peek()
        {
            ThrowHelpers.ThrowIfEmptyQueue(_count);
            return Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer).Element;
        }

        /// <summary>
        ///     Returns a value that indicates whether there is a minimal element in this, and if one
        ///     is present, copies it and its associated priority to the <paramref name="element" />
        ///     and <paramref name="priority" /> arguments. The element is not removed from this.
        /// </summary>
        /// <param name="element">When this method returns, contains the minimal element in the queue.</param>
        /// <param name="priority">
        ///     When this method returns, contains the priority associated with the minimal element.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is a minimal element; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryPeek(out TElement element, out TPriority priority)
        {
            if (_count != 0)
            {
                (element, priority) = Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer);
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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (_capacity < capacity)
            {
                Grow(capacity);
                _version = _version.WrappingAdd(1);
            }

            return _capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        public int TrimExcess()
        {
            if (_count >= I32.FromF64Unchecked(F64.FromI32Unchecked(_capacity) * 0.9))
                return _capacity;

            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>(Usize.FromI32Unchecked(_count));
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<(TElement Element, TPriority Priority)>())));
            NativeMemoryAllocator.AlignedFree(_buffer);
            _buffer = nodes;
            _capacity = _count;
            _version = _version.WrappingAdd(1);
            return _capacity;
        }

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            if (capacity < _count || capacity >= _capacity)
                return _capacity;

            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>(Usize.FromI32Unchecked(capacity));
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<(TElement Element, TPriority Priority)>())));
            NativeMemoryAllocator.AlignedFree(_buffer);
            _buffer = nodes;
            _capacity = capacity;
            _version = _version.WrappingAdd(1);
            return _capacity;
        }

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer), _count);

        /// <summary>
        ///     Increases the capacity of this to a new size that is at least the specified minimum capacity.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = CollectionHelpers.GrowCapacity(_capacity, capacity);
            var nodes = NativeMemoryAllocator.AlignedAlloc<(TElement Element, TPriority Priority)>(Usize.FromI32Unchecked(newCapacity));
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(nodes), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_count).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<(TElement Element, TPriority Priority)>())));
            NativeMemoryAllocator.AlignedFree(_buffer);
            _buffer = nodes;
            _capacity = newCapacity;
        }

        /// <summary>
        ///     Removes the root node (the minimum priority element) from the
        ///     heap, and rebalances the heap by moving the last element down.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveRootNode()
        {
            var index = _count = _count.WrappingSub(1);
            _version = _version.WrappingAdd(1);
            if (index > 0)
            {
                var node = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(_buffer), Isize.FromI32Unchecked(index));
                MoveDown(node, 0);
            }
        }

        /// <summary>
        ///     Moves a node upward in the heap to restore heap order,
        ///     assuming the node's priority is lower than its parent's.
        /// </summary>
        /// <param name="node">The element and priority of the node to move through the heap.</param>
        /// <param name="nodeIndex">The zero-based index in the heap from which the node starts moving.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveUp((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            var nodes = _buffer;
            int parentIndex;
            for (; nodeIndex > 0; nodeIndex = parentIndex)
            {
                parentIndex = nodeIndex.WrappingSub(1) >> 2;
                var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(parentIndex));
                if (node.Priority.CompareTo(tuple.Priority) < 0)
                    Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(nodeIndex)) = tuple;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(nodeIndex)) = node;
        }

        /// <summary>
        ///     Moves a node downward in the heap to restore heap order,
        ///     assuming the node's priority is greater than its children's.
        /// </summary>
        /// <param name="node">The element and priority of the node to move through the heap.</param>
        /// <param name="nodeIndex">The zero-based index in the heap from which the node starts moving.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void MoveDown((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            var nodes = _buffer;
            int firstChildIndex;
            int first;
            for (var size = _count; (firstChildIndex = (nodeIndex << 2).WrappingAdd(1)) < size; nodeIndex = first)
            {
                var valueTuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(firstChildIndex));
                first = firstChildIndex;
                var minSize = firstChildIndex.WrappingAdd(4);
                var second = Math.Min(minSize, size);
                while ((firstChildIndex = firstChildIndex.WrappingAdd(1)) < second)
                {
                    var tuple = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(firstChildIndex));
                    if (tuple.Priority.CompareTo(valueTuple.Priority) < 0)
                    {
                        valueTuple = tuple;
                        first = firstChildIndex;
                    }
                }

                if (node.Priority.CompareTo(valueTuple.Priority) > 0)
                    Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(nodeIndex)) = valueTuple;
                else
                    break;
            }

            Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(nodes), Isize.FromI32Unchecked(nodeIndex)) = node;
        }

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
            public bool IsCreated => _handle != null;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => _handle->Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private UnorderedItemsCollection(UnsafePriorityQueue<TElement, TPriority>* handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static UnorderedItemsCollection From(UnsafePriorityQueue<TElement, TPriority>* handle) => new(handle);

            /// <summary>
            ///     Returns a read-only span over the entire contents of this.
            /// </summary>
            /// <returns>A read-only span over the entire contents of this.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => _handle->AsReadOnlySpan();

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(_handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
                ///     Used to keep the enumerator synchronized with the collection.
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
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafePriorityQueue<TElement, TPriority>* handle)
                {
                    _handle = handle;
                    _version = handle->_version;
                    _index = 0;
                    _current = default;
                }

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafePriorityQueue<TElement, TPriority>* handle) => new(handle);

                /// <summary>
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
                /// <exception cref="InvalidOperationException">
                ///     Thrown when the collection was modified after the enumerator was created.
                /// </exception>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    var handle = _handle;
                    ThrowHelpers.ThrowIfEnumFailedVersion(_version, handle->_version);
                    if (U32.FromI32Unchecked(_index) >= U32.FromI32Unchecked(handle->_count))
                    {
                        _index = handle->_count.WrappingAdd(1);
                        _current = default;
                        return false;
                    }

                    _current = Unsafe.Add(ref Unsafe.AsRef<(TElement Element, TPriority Priority)>(handle->_buffer), Isize.FromI32Unchecked(_index));
                    _index = _index.WrappingAdd(1);
                    return true;
                }

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset()
                {
                    _index = 0;
                    _current = default;
                }

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly (TElement Element, TPriority Priority) Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _current;
                }
            }
        }
    }
}