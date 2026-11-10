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
    ///     Represents a thread-safe collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(ConcurrentDictionary<,>))]
    public readonly struct UnsafeConcurrentDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws
        ///     a <see cref="KeyNotFoundException" />, and a set operation creates a new element with the specified key.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Handle[key] = value;
        }

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
        ///     The number of key/value pairs contained in this.
        /// </value>
        /// <exception cref="OverflowException">
        ///     Thrown when the dictionary contains too many elements.
        /// </exception>
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
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        public KeyCollection Keys => KeyCollection.From(_handle);

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        public ValueCollection Values => ValueCollection.From(_handle);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeConcurrentDictionary(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance that is empty, has the default concurrency level, has
        ///     the default initial capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentDictionary<TKey, TValue> Create()
        {
            var handle = NativeObject<ConcurrentDictionary<TKey, TValue>>.Create(new ConcurrentDictionary<TKey, TValue>());
            return new UnsafeConcurrentDictionary<TKey, TValue>(handle);
        }

        /// <summary>
        ///     Creates a new instance that is empty, has the specified concurrency
        ///     level and capacity, and uses the default comparer for the key type.
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
        public static UnsafeConcurrentDictionary<TKey, TValue> Create(int concurrencyLevel, int capacity)
        {
            var handle = NativeObject<ConcurrentDictionary<TKey, TValue>>.Create(new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity));
            return new UnsafeConcurrentDictionary<TKey, TValue>(handle);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeConcurrentDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeConcurrentDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentDictionary<TKey, TValue> left, UnsafeConcurrentDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentDictionary<TKey, TValue> left, UnsafeConcurrentDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Removes all keys and values from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => Handle.Clear();

        /// <summary>
        ///     Attempts to add the specified key and value to this.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">
        ///     The value of the element to add.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the key/value pair was successfully
        ///     added to this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value) => Handle.TryAdd(key, value);

        /// <summary>
        ///     Attempts to remove and return the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object removed from
        ///     this or the default value of <typeparamref name="TValue" /> if the operation failed.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value) => Handle.TryRemove(key, out value);

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Removes a key and value from this.
        /// </summary>
        /// <param name="keyValuePair">
        ///     The <see cref="KeyValuePair{TKey,TValue}" /> representing the key and value to remove.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the key and value represented by <paramref name="keyValuePair" />
        ///     are successfully found and removed; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Both the specified key and value must match the entry in this for it to be removed.
        ///     The key is compared using the default comparer for <typeparamref name="TKey" />.
        ///     The value is compared using the default comparer for <typeparamref name="TValue" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(KeyValuePair<TKey, TValue> keyValuePair) => Handle.TryRemove(keyValuePair);
#endif

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => Handle.ContainsKey(key);

        /// <summary>
        ///     Attempts to get the value associated with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object from this with the
        ///     specified key or the default value of <typeparamref name="TValue" />, if the operation failed.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the key was found in this; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) => Handle.TryGetValue(key, out value);

        /// <summary>
        ///     Updates the value associated with <paramref name="key" /> to <paramref name="newValue" />
        ///     if the existing value is equal to <paramref name="comparisonValue" />.
        /// </summary>
        /// <param name="key">
        ///     The key whose value is compared with <paramref name="comparisonValue" /> and possibly replaced.
        /// </param>
        /// <param name="newValue">
        ///     The value that replaces the value of the element with
        ///     <paramref name="key" /> if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        ///     The value that is compared to the value of the element with <paramref name="key" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the value with <paramref name="key" />
        ///     was equal to <paramref name="comparisonValue" /> and replaced with
        ///     <paramref name="newValue" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => Handle.TryUpdate(key, newValue, comparisonValue);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key if the key is already
        ///     in this, or the new value for the key as returned by valueFactory if the key was not in this.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="valueFactory" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => Handle.GetOrAdd(key, valueFactory);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument passed to the factory methods.</typeparam>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key.</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory" />.</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key if the key is already
        ///     in this, or the new value for the key as returned by valueFactory if the key was not in this.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="valueFactory" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) => Handle.GetOrAdd(key, valueFactory, factoryArgument);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist.</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key
        ///     if the key is already in this, or the new value if the key was not in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value) => Handle.GetOrAdd(key, value);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument passed to the factory methods.</typeparam>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <param name="factoryArgument">
        ///     An argument to pass into <paramref name="addValueFactory" /> and <paramref name="updateValueFactory" />.
        /// </param>
        /// <returns>
        ///     The new value for the key. This will be either be the result of addValueFactory (if
        ///     the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="addValueFactory" /> or
        ///     <paramref name="updateValueFactory" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument) => Handle.AddOrUpdate(key, addValueFactory, updateValueFactory, factoryArgument);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key.</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        ///     The new value for the key. This will be either the result of addValueFactory (if
        ///     the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="addValueFactory" /> or
        ///     <paramref name="updateValueFactory" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) => Handle.AddOrUpdate(key, addValueFactory, updateValueFactory);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist,
        ///     or updates a key/value pair in this if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated.</param>
        /// <param name="addValue">The value to be added for an absent key.</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        ///     The new value for the key. This will be either the value of addValue (if the
        ///     key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="updateValueFactory" /> is <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) => Handle.AddOrUpdate(key, addValue, updateValueFactory);

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public Enumerator GetEnumerator() => Enumerator.From(AllocEnumerator(Handle));

        /// <summary>
        ///     Allocates a native object that holds an enumerator for the dictionary.
        /// </summary>
        /// <param name="handle">The dictionary to enumerate.</param>
        /// <returns>A native object that holds an enumerator for the dictionary.</returns>
        private static NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> AllocEnumerator(ConcurrentDictionary<TKey, TValue> handle) => NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>>.Create(BoxEnumerator(handle));

        /// <summary>
        ///     Returns an enumerator for the dictionary, boxing the value‑type enumerator.
        /// </summary>
        /// <param name="handle">The dictionary to enumerate.</param>
        /// <returns>An enumerator for the dictionary.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerator<KeyValuePair<TKey, TValue>> BoxEnumerator(ConcurrentDictionary<TKey, TValue> handle) => handle.GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
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
        public readonly struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => new(handle);

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
            public KeyValuePair<TKey, TValue> Current => Handle.Current;

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

        /// <summary>
        ///     Represents the collection of keys.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => Handle.Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private KeyCollection(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => new(handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(AllocEnumerator(Handle));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
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
            public readonly struct Enumerator : IIterator<TKey>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => new(handle);

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
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => Handle.Reset();

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public TKey Current => Handle.Current.Key;

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

        /// <summary>
        ///     Represents the collection of values.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IIsCreated, IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count => Handle.Count;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueCollection(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => new(handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(AllocEnumerator(Handle));

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
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
            public readonly struct Enumerator : IIterator<TValue>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => new(handle);

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
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => Handle.Reset();

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public TValue Current => Handle.Current.Value;

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
}