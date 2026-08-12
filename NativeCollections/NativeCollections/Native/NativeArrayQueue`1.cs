using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using crossbeam;
using static crossbeam.Array_Queue;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     A bounded multi-producer multi-consumer queue.
    ///     <br />
    ///     This queue allocates a fixed-capacity buffer on construction, which is used to store pushed
    ///     elements. The queue cannot hold more elements than the buffer allows. Attempting to push an
    ///     element into a full queue will fail. Alternatively, [`force_push`] makes it possible for
    ///     this queue to be used as a ring-buffer. Having a buffer allocated upfront makes this queue
    ///     a bit faster than [`SegQueue`].`
    /// </summary>
    /// <remarks>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community | FromType.Rust)]
    [BindingType(typeof(ArrayQueue<>))]
    public readonly unsafe struct NativeArrayQueue<T> : IIsCreated, IDisposable, IEquatable<NativeArrayQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly ArrayQueue<T>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArrayQueue(ArrayQueue<T>* handle) => _handle = handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>true if this is empty; otherwise, false.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended
        ///     rather than retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another thread will
        ///     modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty();
        }

        /// <summary>
        ///     Returns `true` if the queue is full.
        /// </summary>
        public bool IsFull => _handle->IsFull();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the <see cref="IsEmpty" />
        ///     property is recommended rather than retrieving the number of items from the <see cref="Count" />
        ///     property and comparing it to 0.
        /// </remarks>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count();
        }

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _handle->Capacity();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeArrayQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArrayQueue<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArrayQueue<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArrayQueue<T> left, NativeArrayQueue<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArrayQueue<T> left, NativeArrayQueue<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Adds an object to the end of this.
        /// </summary>
        /// <param name="item">
        ///     The object to add to the end of this.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T item) => _handle->TryEnqueue(item);

        /// <summary>
        ///     Adds an object to the end of this.
        /// </summary>
        /// <param name="item">
        ///     The object to add to the end of this.
        /// </param>
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
        public InsertResult Enqueue(T item, out T overwritten) => _handle->Enqueue(item, out overwritten);

        /// <summary>
        ///     Attempts to remove and return the object at the beginning of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains the
        ///     object removed. If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     true if an element was removed and returned from the beginning of this successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle->TryDequeue(out result);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeArrayQueue<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is negative or zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayQueue<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            var value = new ArrayQueue<T>((nuint)capacity);
            return new NativeArrayQueue<T>(Box.New(ref value, CACHE_LINE_SIZE));
        }
    }
}