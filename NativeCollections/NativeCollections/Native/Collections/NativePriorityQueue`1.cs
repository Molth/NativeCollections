using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a collection of items that have a priority. On
    ///     dequeue, the item with the lowest priority value is removed.
    /// </summary>
    /// <typeparam name="TPriority">The type of the priority values.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafePriorityQueue<>))]
    public readonly unsafe struct NativePriorityQueue<TPriority> : IIsCreated, IDisposable, IEquatable<NativePriorityQueue<TPriority>> where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafePriorityQueue<TPriority>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativePriorityQueue(UnsafePriorityQueue<TPriority>* handle) => _handle = handle;

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
        public static NativePriorityQueue<TPriority> Create(int capacity)
        {
            var value = UnsafePriorityQueue<TPriority>.Create(capacity);
            return new NativePriorityQueue<TPriority>(Box.New(ref value));
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
        ///     Gets a read-only reference to the priority of the node at the specified index in the heap.
        /// </summary>
        /// <param name="index">
        ///     The zero-based index of the node in the heap, where index <c>0</c>
        ///     refers to the node that would be returned by the next dequeue operation.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        public ref readonly TPriority this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafePriorityQueue<TPriority>>(_handle)[index];
        }

        /// <summary>
        ///     Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        public UnsafePriorityQueue<TPriority>.UnorderedItemsCollection UnorderedItems => _handle->UnorderedItems;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativePriorityQueue<TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativePriorityQueue<TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativePriorityQueue<{0}>", SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativePriorityQueue<TPriority> left, NativePriorityQueue<TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativePriorityQueue<TPriority> left, NativePriorityQueue<TPriority> right) => !left.Equals(right);

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
        /// <param name="priority">The element that was removed from the queue.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully removed; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index, out TPriority priority) => _handle->RemoveAt(index, out priority);

        /// <summary>
        ///     Adds the specified element to this.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(TPriority priority) => _handle->Enqueue(priority);

        /// <summary>
        ///     Attempts to add the specified element to this.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to this;
        ///     <see langword="false" /> if this is already full and the item could not be added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(TPriority priority) => _handle->TryEnqueue(priority);

        /// <summary>
        ///     Adds the specified element to this, and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority EnqueueDequeue(TPriority priority) => _handle->EnqueueDequeue(priority);

        /// <summary>
        ///     Attempts to add the specified element to this, and
        ///     immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        /// <param name="result">
        ///     When this method returns, the minimal priority removed by the operation if the method
        ///     returned <see langword="true" />; otherwise, the specified priority.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the specified priority was added to this and the minimal priority was removed;
        ///     <see langword="false" /> if this was empty or the specified priority is not greater than the
        ///     current minimal priority, in which case <paramref name="result" /> receives the specified priority
        ///     and no element is removed.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueDequeue(TPriority priority, out TPriority result) => _handle->TryEnqueueDequeue(priority, out result);

        /// <summary>
        ///     Removes and returns the object at the beginning of this.
        /// </summary>
        /// <returns>The object that is removed from the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Removes the minimal element from this and copies it to the <paramref name="priority" />.
        /// </summary>
        /// <param name="priority">When this method returns, contains the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully
        ///     removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TPriority priority) => _handle->TryDequeue(out priority);

        /// <summary>
        ///     Removes the minimal element and then immediately adds the specified element to this.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority DequeueEnqueue(TPriority priority) => _handle->DequeueEnqueue(priority);

        /// <summary>
        ///     Removes the minimal element and then immediately adds the specified element to this.
        /// </summary>
        /// <param name="priority">The element to add to this.</param>
        /// <param name="result">
        ///     When this method returns, the minimal element removed before the specified
        ///     element is added; otherwise, the default value for the type of the
        ///     <paramref name="result" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the minimal priority was removed and the specified priority was added
        ///     to this; <see langword="false" /> if this is empty and the specified priority was not added.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueEnqueue(TPriority priority, out TPriority result) => _handle->TryDequeueEnqueue(priority, out result);

        /// <summary>
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <returns>The object at the beginning of this.</returns>
        /// <exception cref="InvalidOperationException">Thrown when this is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TPriority Peek() => _handle->Peek();

        /// <summary>
        ///     Returns a value that indicates whether there is a minimal element in this, and if one is
        ///     present, copies it to the <paramref name="priority" />. The element is not removed from this.
        /// </summary>
        /// <param name="priority">When this method returns, contains the minimal element in the queue.</param>
        /// <returns>
        ///     <see langword="true" /> if there is a minimal element; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out TPriority priority) => _handle->TryPeek(out priority);

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
        public ReadOnlySpan<TPriority> AsReadOnlySpan() => _handle->AsReadOnlySpan();
    }
}