using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe last-in, first-out collection of objects.
    /// </summary>
    /// <remarks>disable Enumerator, try peek, push/pop range either</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [BindingType(typeof(ConcurrentStack<>))]
    public readonly struct UnsafeConcurrentStack<T> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentStack<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeObject<ConcurrentStack<T>> _handle;

        /// <summary>
        ///     Handle
        /// </summary>
        private ConcurrentStack<T> Handle => _handle.Value;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>true if this is empty; otherwise, false.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of this property is recommended rather than
        ///     retrieving the number of items from the <see cref="Count" /> property and comparing it to 0.
        ///     However, as this collection is intended to be accessed concurrently, it may be the case that another thread will
        ///     modify the collection after <see cref="IsEmpty" /> returns, thus invalidating the result.
        /// </remarks>
        public bool IsEmpty => Handle.IsEmpty;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>The number of elements contained in this.</value>
        /// <remarks>
        ///     For determining whether the collection contains any items, use of the <see cref="IsEmpty" />
        ///     property is recommended rather than retrieving the number of items from the <see cref="Count" />
        ///     property and comparing it to 0.
        /// </remarks>
        public int Count => Handle.Count;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeConcurrentStack(NativeObject<ConcurrentStack<T>> handle) => _handle = handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(UnsafeConcurrentStack<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentStack<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("UnsafeConcurrentStack<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeConcurrentStack<T> left, UnsafeConcurrentStack<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeConcurrentStack<T> left, UnsafeConcurrentStack<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Removes all objects from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Handle.Clear();

        /// <summary>
        ///     Inserts an object at the top of this.
        /// </summary>
        /// <param name="item">
        ///     The object to push onto this.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item) => Handle.Push(item);

        /// <summary>
        ///     Attempts to pop and return the object at the top of this.
        /// </summary>
        /// <param name="result">
        ///     When this method returns, if the operation was successful, <paramref name="result" /> contains the object removed.
        ///     If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        ///     true if an element was removed and returned from the top of this successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result) => Handle.TryPop(out result);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentStack<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentStack<T> Create() => new(NativeObject<ConcurrentStack<T>>.Create(new ConcurrentStack<T>()));
    }
}