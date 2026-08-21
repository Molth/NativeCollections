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
    ///     Represents a fixed-size circular buffer that supports insertion and removal from both ends,
    ///     overwriting the oldest element when full.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeRingBuffer<>))]
    public readonly unsafe struct NativeRingBuffer<T> : IIsCreated, IDisposable, IEquatable<NativeRingBuffer<T>>, IReadOnlyCollection<T> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeRingBuffer<T>* _handle;

        /// <summary>
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeRingBuffer(int capacity)
        {
            var value = new UnsafeRingBuffer<T>(capacity);
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
        ///     Returns `true` if the queue is full.
        /// </summary>
        public bool IsFull => _handle->IsFull;

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
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeRingBuffer<T>>(_handle)[index];
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeRingBuffer<T>>(_handle)[index];
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeRingBuffer<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeRingBuffer<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeRingBuffer<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeRingBuffer<T> left, NativeRingBuffer<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeRingBuffer<T> left, NativeRingBuffer<T> right) => !left.Equals(right);

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
        ///     Adds an item to the head of the ring buffer.
        ///     If the buffer is full, the oldest element at the tail is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(in T item) => _handle->EnqueueHead(item);

        /// <summary>
        ///     Adds an item to the head of the ring buffer.
        ///     If the buffer is full, the oldest element at the tail is overwritten and returned via
        ///     <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the buffer was full;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueHead(in T item, out T overwritten) => _handle->EnqueueHead(item, out overwritten);

        /// <summary>
        ///     Attempts to add an item to the head of the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueHead(in T item) => _handle->TryEnqueueHead(item);

        /// <summary>
        ///     Adds an item to the tail of the ring buffer.
        ///     If the buffer is full, the oldest element at the head is overwritten.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(in T item) => _handle->EnqueueTail(item);

        /// <summary>
        ///     Adds an item to the tail of the ring buffer.
        ///     If the buffer is full, the oldest element at the head is overwritten and returned via
        ///     <paramref name="overwritten" />.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the buffer was full;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     An <see cref="InsertResult" /> value indicating whether the item was added successfully
        ///     <see cref="InsertResult.Success" /> or if an existing element was overwritten
        ///     <see cref="InsertResult.Overwritten" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult EnqueueTail(in T item, out T overwritten) => _handle->EnqueueTail(item, out overwritten);

        /// <summary>
        ///     Attempts to add an item to the tail of the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueueTail(in T item) => _handle->TryEnqueueTail(item);

        /// <summary>
        ///     Removes the object at the beginning of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully removed;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result) => _handle->TryDequeueHead(out result);

        /// <summary>
        ///     Removes the object at the ending of this, and copies it to the <paramref name="result" /> parameter.
        /// </summary>
        /// <param name="result">The removed object.</param>
        /// <returns>
        ///     <see langword="true" /> if the object is successfully removed;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result) => _handle->TryDequeueTail(out result);

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the beginning of this,
        ///     and if one is present, copies it to the <paramref name="result" /> parameter.
        ///     The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the beginning of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the beginning of this;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekHead(out T result) => _handle->TryPeekHead(out result);

        /// <summary>
        ///     Returns a value that indicates whether there is an object at the ending of this,
        ///     and if one is present, copies it to the <paramref name="result" /> parameter.
        ///     The object is not removed from this.
        /// </summary>
        /// <param name="result">
        ///     If present, the object at the ending of this;
        ///     otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if there is an object at the ending of this;
        ///     <see langword="false" /> if this is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekTail(out T result) => _handle->TryPeekTail(out result);

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<T> buffer, int count) => _handle->CopyTo(buffer, count);

        /// <summary>
        ///     Copies up to the specified number of elements from this.
        ///     The actual number of copied elements is limited by the span's length, the specified count,
        ///     and the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which elements are copied.</param>
        /// <param name="count">The maximum number of elements to copy. Must be non-negative.</param>
        /// <returns>The actual number of elements copied from the this.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CopyTo(Span<byte> buffer, int count) => _handle->CopyTo(buffer, count);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<T> buffer) => _handle->CopyTo(buffer);

        /// <summary>
        ///     Copies all elements from this into a destination span.
        ///     The span must have a length at least equal to the current number of elements in this.
        /// </summary>
        /// <param name="buffer">The destination span to which all elements are copied.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="buffer" /> has insufficient length to hold all of this's elements.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<byte> buffer) => _handle->CopyTo(buffer);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeRingBuffer<T> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public UnsafeRingBuffer<T>.Enumerator GetEnumerator() => _handle->GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
    }
}