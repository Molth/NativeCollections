using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using crossbeam;
using static crossbeam.Seg_Queue;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An unbounded multi-producer multi-consumer queue.
    ///     <br />
    ///     This queue is implemented as a linked list of segments, where each segment is a small buffer
    ///     that can hold a handful of elements. There is no limit to how many elements can be in the queue
    ///     at a time. However, since segments need to be dynamically allocated as elements get pushed,
    /// </summary>
    /// <remarks>
    ///     https://github.com/crossbeam-rs/crossbeam
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community | FromType.Rust)]
    [BindingType(typeof(SegQueue<>))]
    public struct UnsafeSegQueue<T> : IIsCreated, IDisposable, IEquatable<UnsafeSegQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     Handle
        /// </summary>
        private SegQueue<T> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => true;

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
            get => _handle.IsEmpty();
        }

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
            get => _handle.Count();
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeSegQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeSegQueue<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeSegQueue<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeSegQueue<T> left, UnsafeSegQueue<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeSegQueue<T> left, UnsafeSegQueue<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.drop();

        /// <summary>
        ///     Adds an object to the end of this.
        /// </summary>
        /// <param name="item">
        ///     The object to add to the end of this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item) => _handle.Enqueue(item);

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
        public bool TryDequeue(out T result) => _handle.TryDequeue(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSegQueue<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeSegQueue<T> Create() => new();
    }
}