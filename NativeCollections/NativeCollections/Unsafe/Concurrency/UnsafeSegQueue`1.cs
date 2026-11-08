using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.SegQueue;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An unbounded multi-producer multi-consumer queue.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the queue.</typeparam>
    /// <remarks>
    ///     <para>
    ///         The queue stores elements in a linked list of blocks, with multiple element slots in each block.
    ///     </para>
    ///     <para>
    ///         The queue has no element-count limit. Blocks are allocated as
    ///         elements are added and are released when they are no longer needed.
    ///     </para>
    ///     <para>
    ///         Allocating blocks as the queue grows makes it somewhat slower than <see cref="UnsafeArrayQueue{T}" />.
    ///     </para>
    ///     <para>Source:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see href="https://github.com/crossbeam-rs/crossbeam/blob/main/crossbeam-queue/src/seg_queue.rs">
    ///                     Crossbeam Rust implementation
    ///                 </see>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community | FromType.Rust)]
    [BindingType(typeof(RawSegQueue<>))]
    public struct UnsafeSegQueue<T> : IIsCreated, IDisposable, IEquatable<UnsafeSegQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private RawSegQueue<T> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Always <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The default value of this type is a valid empty queue: the first block is
        ///                 allocated by the first enqueue, so there is no separate initialization step.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public readonly bool IsCreated => true;

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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeSegQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeSegQueue<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeSegQueue<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeSegQueue<T> left, UnsafeSegQueue<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeSegQueue<T> left, UnsafeSegQueue<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Adds an element to the end of the queue.
        /// </summary>
        /// <param name="item">
        ///     The element to add to the queue.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item) => _handle.Enqueue(item);

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
    }
}