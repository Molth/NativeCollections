using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe collection of items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the set.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(ConcurrentDictionary<,>))]
    public readonly struct UnsafeConcurrentHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeObject<ConcurrentDictionary<T, bool>> _handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private ConcurrentDictionary<T, bool> Handle => _handle.Value;

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
        /// <value>
        ///     The number of elements contained in this.
        /// </value>
        /// <exception cref="OverflowException">Thrown when the hash set contains too many elements.</exception>
        /// <remarks>
        ///     Count has snapshot semantics and represents the number
        ///     of items in this at the moment when Count was accessed.
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
        private UnsafeConcurrentHashSet(NativeObject<ConcurrentDictionary<T, bool>> handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance that is empty, has the default concurrency level, has the
        ///     default initial capacity, and uses the default comparer for the element type.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentHashSet<T> Create()
        {
            var handle = NativeObject<ConcurrentDictionary<T, bool>>.Create(new ConcurrentDictionary<T, bool>());
            return new UnsafeConcurrentHashSet<T>(handle);
        }

        /// <summary>
        ///     Creates a new instance that is empty, has the specified concurrency
        ///     level and capacity, and uses the default comparer for the element type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update this concurrently; in .NET 8 and later,
        ///     -1 indicates the default concurrency level.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that this can contain.
        /// </param>
        /// <returns>A new instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="concurrencyLevel" /> is less than 1 (except -1 on .NET 8 and later)
        ///     or <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentHashSet<T> Create(int concurrencyLevel, int capacity)
        {
            var handle = NativeObject<ConcurrentDictionary<T, bool>>.Create(new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity));
            return new UnsafeConcurrentHashSet<T>(handle);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeConcurrentHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeConcurrentHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentHashSet<T> left, UnsafeConcurrentHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentHashSet<T> left, UnsafeConcurrentHashSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Removes all elements from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Handle.Clear();

        /// <summary>
        ///     Attempts to add the specified element to this.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns>
        ///     <see langword="true" /> if the element was successfully
        ///     added to this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) => Handle.TryAdd(item, default);

        /// <summary>
        ///     Removes the specified element from this.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>
        ///     <see langword="true" /> if an object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => Handle.TryRemove(item, out _);

        /// <summary>
        ///     Determines whether this contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains the specified element; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => Handle.ContainsKey(item);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public Enumerator GetEnumerator() => Enumerator.From(AllocEnumerator(Handle));

        /// <summary>
        ///     Allocates a native object that holds an enumerator for the hash set.
        /// </summary>
        /// <param name="handle">The hash set to enumerate.</param>
        /// <returns>A native object that holds an enumerator for the hash set.</returns>
        private static NativeObject<IEnumerator<KeyValuePair<T, bool>>> AllocEnumerator(ConcurrentDictionary<T, bool> handle) => NativeObject<IEnumerator<KeyValuePair<T, bool>>>.Create(BoxEnumerator(handle));

        /// <summary>
        ///     Returns an enumerator for the hash set, boxing the value‑type enumerator.
        /// </summary>
        /// <param name="handle">The hash set to enumerate.</param>
        /// <returns>An enumerator for the hash set.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerator<KeyValuePair<T, bool>> BoxEnumerator(ConcurrentDictionary<T, bool> handle) => handle.GetEnumerator();

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

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Enumerator : IIterator<T>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeObject<IEnumerator<KeyValuePair<T, bool>>> _handle;

            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private IEnumerator<KeyValuePair<T, bool>> Handle => _handle.Value;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeObject<IEnumerator<KeyValuePair<T, bool>>> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeObject<IEnumerator<KeyValuePair<T, bool>>> handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => Handle.MoveNext();

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => Handle.Reset();

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public T Current => Handle.Current.Key;

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Handle.Dispose();
                _handle.Dispose();
            }
        }
    }
}