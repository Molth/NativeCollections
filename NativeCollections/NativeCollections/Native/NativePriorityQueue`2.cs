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
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafePriorityQueue<,>))]
    public readonly unsafe struct NativePriorityQueue<TElement, TPriority> : IIsCreated, IDisposable, IEquatable<NativePriorityQueue<TElement, TPriority>> where TElement : unmanaged where TPriority : unmanaged, IComparable<TPriority>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafePriorityQueue<TElement, TPriority>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativePriorityQueue(int capacity)
        {
            var value = new UnsafePriorityQueue<TElement, TPriority>(capacity);
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _handle->Capacity;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref readonly (TElement Element, TPriority Priority) this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafePriorityQueue<TElement, TPriority>>(_handle)[index];
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref readonly (TElement Element, TPriority Priority) this[uint index]
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
        public bool Equals(NativePriorityQueue<TElement, TPriority> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativePriorityQueue<TElement, TPriority> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativePriorityQueue<{0}, {1}>", SR.GetTypeName(typeof(TElement)), SR.GetTypeName(typeof(TPriority)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativePriorityQueue<TElement, TPriority> left, NativePriorityQueue<TElement, TPriority> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index) => _handle->RemoveAt(index);

        /// <summary>
        ///     Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <param name="element">The actual element that got removed from the queue.</param>
        /// <param name="priority">The priority value associated with the removed element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveAt(int index, out TElement element, out TPriority priority) => _handle->RemoveAt(index, out element, out priority);

        /// <summary>
        ///     Adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in TElement element, in TPriority priority) => _handle->Enqueue(element, priority);

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
        public bool TryEnqueue(in TElement element, in TPriority priority) => _handle->TryEnqueue(element, priority);

        /// <summary>
        ///     Adds the specified element with associated priority to this,
        ///     and immediately removes the minimal element, returning the result.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <returns>The minimal element removed after the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement EnqueueDequeue(in TElement element, in TPriority priority) => _handle->EnqueueDequeue(element, priority);

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
        public bool TryEnqueueDequeue(in TElement element, in TPriority priority, out TElement result) => _handle->TryEnqueueDequeue(element, priority, out result);

        /// <summary>
        ///     Removes and returns the object at the beginning of this.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object that is removed from the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Dequeue() => _handle->Dequeue();

        /// <summary>
        ///     Removes the minimal element from this,
        ///     and copies it and its associated priority to the <paramref name="element" />.
        /// </summary>
        /// <param name="element">When this method returns, contains the removed element.</param>
        /// <returns>
        ///     <see langword="true" /> if the element is successfully removed; <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out TElement element) => _handle->TryDequeue(out element);

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
        public bool TryDequeue(out TElement element, out TPriority priority) => _handle->TryDequeue(out element, out priority);

        /// <summary>
        ///     Removes the minimal element and then immediately adds the specified element with associated priority to this.
        /// </summary>
        /// <param name="element">The element to add to this.</param>
        /// <param name="priority">The priority with which to associate the new element.</param>
        /// <exception cref="T:System.InvalidOperationException">The queue is empty.</exception>
        /// <returns>The minimal element removed before performing the enqueue operation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement DequeueEnqueue(in TElement element, in TPriority priority) => _handle->DequeueEnqueue(element, priority);

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
        public bool TryDequeueEnqueue(in TElement element, in TPriority priority, out TElement result) => _handle->TryDequeueEnqueue(element, priority, out result);

        /// <summary>
        ///     Returns the object at the beginning of this without removing it.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">this is empty.</exception>
        /// <returns>The object at the beginning of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TElement Peek() => _handle->Peek();

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
        public bool TryPeek(out TElement element, out TPriority priority) => _handle->TryPeek(out element, out priority);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

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
        ///     Empty
        /// </summary>
        public static NativePriorityQueue<TElement, TPriority> Empty => default;
    }
}