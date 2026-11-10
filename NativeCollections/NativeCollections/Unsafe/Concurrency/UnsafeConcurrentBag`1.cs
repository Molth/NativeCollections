using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe, unordered collection of objects.
    /// </summary>
    /// <typeparam name="T">The type of elements in the bag.</typeparam>
    /// <remarks>
    ///     This type does not provide enumeration or peek operations.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(ConcurrentBag<>))]
    public readonly struct UnsafeConcurrentBag<T> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentBag<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeObject<ConcurrentBag<T>> _handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private ConcurrentBag<T> Handle => _handle.Value;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.IsEmpty;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     The count returned represents a moment-in-time snapshot of the contents of the bag. It does not
        ///     reflect any updates to the collection after <see cref="ConcurrentBag{T}.GetEnumerator" /> was called.
        /// </remarks>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.Count;
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeConcurrentBag(NativeObject<ConcurrentBag<T>> handle) => _handle = handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentBag<T> Create() => new(NativeObject<ConcurrentBag<T>>.Create(new ConcurrentBag<T>()));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeConcurrentBag<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentBag<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeConcurrentBag<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentBag<T> left, UnsafeConcurrentBag<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentBag<T> left, UnsafeConcurrentBag<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Removes all values from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Handle.Clear();

        /// <summary>
        ///     Adds an object to this.
        /// </summary>
        /// <param name="item">
        ///     The object to be added to this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item) => Handle.Add(item);

        /// <summary>
        ///     Attempts to remove and return an object from this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, <paramref name="result" /> contains the object removed
        ///     from this or the default value of <typeparamref name="T" /> if the operation failed.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryTake(out T result) => Handle.TryTake(out result);
    }
}