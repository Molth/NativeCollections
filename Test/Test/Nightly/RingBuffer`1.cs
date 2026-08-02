using System;
using System.Runtime.CompilerServices;
using crossbeam;
using NativeCollections;

// ReSharper disable All

namespace Examples
{
    /// A bounded multi-producer multi-consumer queue.
    /// <br />
    /// This queue allocates a fixed-capacity buffer on construction, which is used to store pushed
    /// elements. The queue cannot hold more elements than the buffer allows. Attempting to push an
    /// element into a full queue will fail. Alternatively, [`force_push`] makes it possible for
    /// this queue to be used as a ring-buffer.
    /// <remarks>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </remarks>
    public sealed class RingBuffer<T>
    {
        private Array_Queue.ArrayQueue<T> _inner;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is negative or zero.</exception>
        public RingBuffer(int capacity)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            _inner = new Array_Queue.ArrayQueue<T>((nuint)capacity);
        }

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
            get => _inner.is_empty();
        }

        /// <summary>
        ///     Returns `true` if the queue is full.
        /// </summary>
        public bool IsFull => _inner.is_full();

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
            get => (int)_inner.len();
        }

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => (int)_inner.capacity();

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
        public bool TryEnqueue(T? item) => _inner.push(item).is_ok();

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
        public InsertResult Enqueue(T? item, out T? overwritten)
        {
            var option = _inner.force_push(item);
            if (option.is_some())
            {
                overwritten = option.unwrap_unchecked();
                return InsertResult.Overwritten;
            }

            overwritten = default;
            return InsertResult.Success;
        }

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
        public bool TryDequeue(out T? result)
        {
            var option = _inner.pop();
            if (option.is_some())
            {
                result = option.unwrap_unchecked();
                return true;
            }

            result = default;
            return false;
        }
    }
}