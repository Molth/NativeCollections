using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.ArrayQueue;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     A bounded multi-producer multi-consumer queue.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the queue.</typeparam>
    /// <remarks>
    ///     <para>
    ///         This type is not intended as a drop-in replacement for
    ///         <see cref="ConcurrentQueue{T}" />. It does not provide the following members:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ConcurrentQueue{T}.TryPeek" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ConcurrentQueue{T}.GetEnumerator" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ConcurrentQueue{T}.Clear" />
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The queue allocates a fixed-capacity buffer when constructed.
    ///         <br />
    ///         The buffer determines the maximum number of elements the queue can hold.
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="TryEnqueue" /> returns <see langword="false" /> when the queue is full.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="Enqueue" /> replaces the oldest element when the queue is full and returns
    ///                 it in an <see cref="InsertResult" />, allowing the queue to act as a ring buffer.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         The preallocated buffer makes this queue somewhat faster than <see cref="NativeSegQueue{T}" />.
    ///     </para>
    ///     <para>
    ///         The implementation is based on Dmitry Vyukov's bounded MPMC queue.
    ///     </para>
    ///     <para>Sources:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Crossbeam Rust implementation:<br />
    ///                 <see href="https://github.com/crossbeam-rs/crossbeam/blob/main/crossbeam-queue/src/array_queue.rs">https://github.com/crossbeam-rs/crossbeam/blob/main/crossbeam-queue/src/array_queue.rs</see>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Dmitry Vyukov's bounded MPMC queue:<br />
    ///                 <see href="http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue">http://www.1024cores.net/home/lock-free-algorithms/queues/bounded-mpmc-queue</see>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community | FromType.Rust)]
    [BindingType(typeof(RawArrayQueue<>))]
    public struct UnsafeArrayQueue<T> : IIsCreated, IDisposable, IEquatable<UnsafeArrayQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private RawArrayQueue<T> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value><see langword="true" /> if this is empty; otherwise, <see langword="false" />.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended rather
        ///     than retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another
        ///     thread will modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty => _handle.IsEmpty();

        /// <summary>
        ///     Gets a value that indicates whether the queue is full.
        /// </summary>
        /// <value><see langword="true" /> if the queue is full; otherwise, <see langword="false" />.</value>
        /// <remarks>
        ///     The value may change as other threads modify the queue.
        /// </remarks>
        public bool IsFull => _handle.IsFull();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the
        ///     <see cref="IsEmpty" /> property is recommended rather than retrieving the
        ///     number of items from the <see cref="Count" /> property and comparing it to 0.
        /// </remarks>
        public int Count => I32.FromUsizeUnchecked(_handle.Count());

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => I32.FromUsizeUnchecked(_handle.Capacity());

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="padding">The padding used to avoid false sharing with adjacent data.</param>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeArrayQueue(CachePadding padding, RawArrayQueue<T> handle)
        {
            _padding = padding;
            _handle = handle;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeArrayQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeArrayQueue<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeArrayQueue<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeArrayQueue<T> left, UnsafeArrayQueue<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeArrayQueue<T> left, UnsafeArrayQueue<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Attempts to add an element to the end of the queue.
        /// </summary>
        /// <param name="item">
        ///     The element to add to the queue.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the item was successfully added to the queue;
        ///     <see langword="false" /> if the queue is already full and the item could not be enqueued.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnqueue(T item) => _handle.TryEnqueue(item);

        /// <summary>
        ///     Adds an element to the end of the queue, replacing the oldest element if necessary.
        /// </summary>
        /// <param name="item">
        ///     The element to add to the queue.
        /// </param>
        /// <param name="overwritten">
        ///     When this method returns, contains the element that was overwritten if the
        ///     buffer was full; otherwise, the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see cref="InsertResult.Success" /> if no element was replaced; otherwise,
        ///     <see cref="InsertResult.Overwritten" /> if the oldest element was replaced.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult Enqueue(T item, out T overwritten) => _handle.Enqueue(item, out overwritten);

        /// <summary>
        ///     Attempts to remove an element from the beginning of the queue.
        /// </summary>
        /// <param name="result">
        ///     When this method returns <see langword="true" />, contains the removed
        ///     element; otherwise, contains the default value of <typeparamref name="T" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an element was removed;
        ///     otherwise, <see langword="false" /> if the queue is empty.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle.TryDequeue(out result);

        /// <summary>
        ///     Creates a new bounded queue with the given capacity.
        /// </summary>
        /// <param name="capacity">The requested queue capacity.</param>
        /// <returns>A new bounded queue with a capacity of <paramref name="capacity" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or equal to zero.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeArrayQueue<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            return new UnsafeArrayQueue<T>(default, RawArrayQueue<T>.Create(Usize.FromI32Unchecked(capacity)));
        }
    }
}