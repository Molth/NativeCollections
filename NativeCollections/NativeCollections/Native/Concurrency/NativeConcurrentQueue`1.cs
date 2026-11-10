using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe first-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the queue.</typeparam>
    /// <remarks>
    ///     This type does not provide enumeration or peek operations.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(UnsafeConcurrentQueue<>))]
    public readonly unsafe struct NativeConcurrentQueue<T> : IIsCreated, IDisposable, IEquatable<NativeConcurrentQueue<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeConcurrentQueue<T>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentQueue(UnsafeConcurrentQueue<T>* handle) => _handle = handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentQueue<T> Create()
        {
            var value = UnsafeConcurrentQueue<T>.Create();
            return new NativeConcurrentQueue<T>(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

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
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the
        ///     <see cref="IsEmpty" /> property is recommended rather than retrieving the
        ///     number of items from the <see cref="Count" /> property and comparing it to 0.
        /// </remarks>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeConcurrentQueue<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentQueue<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeConcurrentQueue<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeConcurrentQueue<T> left, NativeConcurrentQueue<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Removes all objects from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Adds an object to the end of this.
        /// </summary>
        /// <param name="item">
        ///     The object to add to the end of this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item) => _handle->Enqueue(item);

        /// <summary>
        ///     Attempts to remove and return the object at the beginning of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains
        ///     the object removed. If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an element was successfully removed and returned
        ///     from the beginning of this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result) => _handle->TryDequeue(out result);
    }
}