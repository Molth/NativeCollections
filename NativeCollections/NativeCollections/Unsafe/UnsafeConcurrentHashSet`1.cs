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
    ///     Unsafe concurrentHashSet
    ///     (Slower than ConcurrentDictionary)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(ConcurrentDictionary<,>))]
    public readonly struct UnsafeConcurrentHashSet<T> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentHashSet<T>>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeObject<ConcurrentDictionary<T, bool>> _handle;

        /// <summary>
        ///     Handle
        /// </summary>
        private ConcurrentDictionary<T, bool> Handle => _handle.Value;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.IsEmpty;
        }

        /// <summary>
        ///     Gets the number of keys contained in this.
        /// </summary>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <value>
        ///     The number of keys contained in this.
        /// </value>
        /// <remarks>
        ///     Count has snapshot semantics and represents the number of items in this
        ///     at the moment when Count was accessed.
        /// </remarks>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.Count;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeConcurrentHashSet(NativeObject<ConcurrentDictionary<T, bool>> handle) => _handle = handle;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(UnsafeConcurrentHashSet<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentHashSet<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("UnsafeConcurrentHashSet<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeConcurrentHashSet<T> left, UnsafeConcurrentHashSet<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeConcurrentHashSet<T> left, UnsafeConcurrentHashSet<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Removes all keys from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Handle.Clear();

        /// <summary>
        ///     Attempts to add the specified key to this.
        /// </summary>
        /// <param name="item">The element to add.</param>
        /// <returns>
        ///     true if the key was added to this successfully;
        ///     otherwise, false.
        /// </returns>
        /// <exception cref="OverflowException">This contains too many elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T item) => Handle.TryAdd(item, default);

        /// <summary>
        ///     Attempts to remove with the specified key from this.
        /// </summary>
        /// <param name="item">The element to remove and return.</param>
        /// <returns>
        ///     true if an object was removed successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(T item) => Handle.TryRemove(item, out _);

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="item">The key to locate in this.</param>
        /// <returns>
        ///     true if this contains an element with the specified key;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(T item) => Handle.ContainsKey(item);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentHashSet<T> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the default concurrency level, has the default initial capacity, and
        ///     uses the default comparer for the key type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentHashSet<T> Create()
        {
            var handle = NativeObject<ConcurrentDictionary<T, bool>>.Create(new ConcurrentDictionary<T, bool>());
            return new UnsafeConcurrentHashSet<T>(handle);
        }

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the specified concurrency level and capacity, and uses the default
        ///     comparer for the key type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update this concurrently, or -1 to indicate a default value.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that this can contain.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel" /> is less than -1.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="capacity" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentHashSet<T> Create(int concurrencyLevel, int capacity)
        {
            var handle = NativeObject<ConcurrentDictionary<T, bool>>.Create(new ConcurrentDictionary<T, bool>(concurrencyLevel, capacity));
            return new UnsafeConcurrentHashSet<T>(handle);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public Enumerator GetEnumerator() => new(AllocEnumerator(Handle));

        /// <summary>
        ///     Alloc enumerator
        /// </summary>
        private static NativeObject<IEnumerator<KeyValuePair<T, bool>>> AllocEnumerator(ConcurrentDictionary<T, bool> handle) => NativeObject<IEnumerator<KeyValuePair<T, bool>>>.Create(BoxEnumerator(handle));

        /// <summary>
        ///     Box enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerator<KeyValuePair<T, bool>> BoxEnumerator(ConcurrentDictionary<T, bool> handle) => handle.GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Enumerator : IIterator<T>, IDisposable
        {
            /// <summary>
            ///     Handle
            /// </summary>
            private readonly NativeObject<IEnumerator<KeyValuePair<T, bool>>> _handle;

            /// <summary>
            ///     Handle
            /// </summary>
            private IEnumerator<KeyValuePair<T, bool>> Handle => _handle.Value;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeObject<IEnumerator<KeyValuePair<T, bool>>> handle) => _handle = handle;

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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => Handle.Reset();

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public T Current => Handle.Current.Key;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Handle.Dispose();
                _handle.Dispose();
            }
        }
    }
}