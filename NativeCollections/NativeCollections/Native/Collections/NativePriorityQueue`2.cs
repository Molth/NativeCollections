using System;
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
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafePriorityQueue<,>))]
    public readonly unsafe struct NativePriorityQueue<TElement, TPriority> : IIsCreated, IDisposable, IEquatable<NativePriorityQueue<TElement, TPriority>> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafePriorityQueue<TElement, TPriority>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativePriorityQueue(UnsafePriorityQueue<TElement, TPriority>* handle) => _handle = handle;

        /// <summary>
        ///     Creates a priority queue with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the instance can hold. Must be non-negative.</param>
        /// <returns>A newly allocated priority queue.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativePriorityQueue<TElement, TPriority> Create(int capacity)
        {
            var value = UnsafePriorityQueue<TElement, TPriority>.Create(capacity);
            return new NativePriorityQueue<TElement, TPriority>(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _handle->Capacity;

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
        public ref readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafePriorityQueue<TElement, TPriority>>(_handle)[index];
        }

        /// <summary>
        ///     Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        public UnsafePriorityQueue<TElement, TPriority>.UnorderedItemsCollection UnorderedItems => _handle->UnorderedItems;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativePriorityQueue<TElement, TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativePriorityQueue<TElement, TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativePriorityQueue<{0}, {1}>", SR.GetTypeName(typeof(TElement)), SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index) => _handle->RemoveAt(index);

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
        public bool RemoveAt(int index, out TElement element, out TPriority priority) => _handle->RemoveAt(index, out element, out priority);

        /// <summary>
        ///     Adds the specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(TElement element, TPriority priority) => _handle->Enqueue(element, priority);

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
        public bool TryEnqueue(TElement element, TPriority priority) => _handle->TryEnqueue(element, priority);

        /// <summary>
        ///     Adds the specified element with the associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement EnqueueDequeue(TElement element, TPriority priority) => _handle->EnqueueDequeue(element, priority);

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
        public bool TryEnqueueDequeue(TElement element, TPriority priority, out TElement result) => _handle->TryEnqueueDequeue(element, priority, out result);

        /// <summary>
        ///     Removes and returns the object at the beginning of this.
        /// </summary>
        /// <returns>The object that is removed from the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Removes the minimal element from this and copies it to the <paramref name="element" />.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element) => _handle->TryDequeue(out element);

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
        public bool TryDequeue(out TElement element, out TPriority priority) => _handle->TryDequeue(out element, out priority);

        /// <summary>
        ///     Removes the minimal element and then immediately adds the
        ///     specified element with the associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement DequeueEnqueue(TElement element, TPriority priority) => _handle->DequeueEnqueue(element, priority);

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
        public bool TryDequeueEnqueue(TElement element, TPriority priority, out TElement result) => _handle->TryDequeueEnqueue(element, priority, out result);

        /// <summary>
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <returns>The object at the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Peek() => _handle->Peek();

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
        public bool TryPeek(out TElement element, out TPriority priority) => _handle->TryPeek(out element, out priority);

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
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the excess capacity of this based on the current count.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the excess capacity of this to the specified number of elements.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<(TElement Element, TPriority Priority)> AsReadOnlySpan() => _handle->AsReadOnlySpan();
    }
}