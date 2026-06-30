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
    ///     Unsafe concurrentDictionary
    ///     (Slower than ConcurrentDictionary)
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.NotImplemented)]
    [BindingType(typeof(ConcurrentDictionary<,>))]
    public readonly struct UnsafeConcurrentDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

        /// <summary>
        ///     Handle
        /// </summary>
        private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, a get operation throws a
        ///     <see cref="KeyNotFoundException" />, and a set operation creates a new element with the specified key.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     The property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Handle[key] = value;
        }

        /// <summary>
        ///     Is created
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
        ///     Gets the number of key/value pairs contained in this.
        /// </summary>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <value>
        ///     The number of key/value pairs contained in this.
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
        ///     Keys
        /// </summary>
        public KeyCollection Keys => new(_handle);

        /// <summary>
        ///     Values
        /// </summary>
        public ValueCollection Values => new(_handle);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeConcurrentDictionary(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(UnsafeConcurrentDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is UnsafeConcurrentDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("UnsafeConcurrentDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeConcurrentDictionary<TKey, TValue> left, UnsafeConcurrentDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentDictionary<TKey, TValue> left, UnsafeConcurrentDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        ///     true if the key/value pair was added to this successfully;
        ///     otherwise, false.
        /// </returns>
        /// <exception cref="OverflowException">This contains too many elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value) => Handle.TryAdd(key, value);

        /// <summary>
        ///     Attempts to remove and return the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object removed from this or the default value of
        ///     <typeparamref name="TValue" /> if the operation failed.
        /// </param>
        /// <returns>
        ///     true if an object was removed successfully;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value) => Handle.TryRemove(key, out value);

#if NET5_0_OR_GREATER
        /// <summary>Removes a key and value from the dictionary.</summary>
        /// <param name="keyValuePair">The <see cref="KeyValuePair{TKey,TValue}" /> representing the key and value to remove.</param>
        /// <returns>
        ///     true if the key and value represented by <paramref name="keyValuePair" /> are successfully found and removed;
        ///     otherwise, false.
        /// </returns>
        /// <remarks>
        ///     Both the specified key and value must match the entry in the dictionary for it to be removed.
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
        ///     true if this contains an element with the specified key;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => Handle.ContainsKey(key);

        /// <summary>
        ///     Attempts to get the value associated with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object from this with the specified key or the
        ///     default value of <typeparamref name="TValue" />, if the operation failed.
        /// </param>
        /// <returns>
        ///     true if the key was found in this;
        ///     otherwise, false.
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
        ///     The value that is compared to the value of the element with
        ///     <paramref name="key" />.
        /// </param>
        /// <returns>
        ///     true if the value with <paramref name="key" /> was equal to <paramref name="comparisonValue" /> and
        ///     replaced with <paramref name="newValue" />;
        ///     otherwise, false.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => Handle.TryUpdate(key, newValue, comparisonValue);

        /// <summary>
        ///     Adds a key/value pair to this
        ///     if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="valueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The value for the key.
        ///     This will be either the existing value for the key if the
        ///     key is already in the dictionary, or the new value for the key as returned by valueFactory
        ///     if the key was not in the dictionary.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => Handle.GetOrAdd(key, valueFactory);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory" />.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="valueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The value for the key.
        ///     This will be either the existing value for the key if the
        ///     key is already in the dictionary, or the new value for the key as returned by valueFactory
        ///     if the key was not in the dictionary.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) => Handle.GetOrAdd(key, valueFactory, factoryArgument);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The value for the key.
        ///     This will be either the existing value for the key if the
        ///     key is already in the dictionary, or the new value if the key was not in the dictionary.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value) => Handle.GetOrAdd(key, value);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already
        ///     exist, or updates a key/value pair in this if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key
        ///     based on the key's existing value
        /// </param>
        /// <param name="factoryArgument">
        ///     An argument to pass into <paramref name="addValueFactory" /> and
        ///     <paramref name="updateValueFactory" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="addValueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="updateValueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The new value for the key.
        ///     This will be either be the result of addValueFactory (if the key was
        ///     absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument) => Handle.AddOrUpdate(key, addValueFactory, updateValueFactory, factoryArgument);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already
        ///     exist, or updates a key/value pair in this if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key
        ///     based on the key's existing value
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="addValueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="updateValueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The new value for the key.
        ///     This will be either the result of addValueFactory (if the key was
        ///     absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) => Handle.AddOrUpdate(key, addValueFactory, updateValueFactory);

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already
        ///     exist, or updates a key/value pair in this if the key
        ///     already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">
        ///     The function used to generate a new value for an existing key based on
        ///     the key's existing value
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="updateValueFactory" /> is a null reference.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     The dictionary contains too many elements.
        /// </exception>
        /// <returns>
        ///     The new value for the key.
        ///     This will be either the value of addValue (if the key was
        ///     absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) => Handle.AddOrUpdate(key, addValue, updateValueFactory);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the default concurrency level, has the default initial capacity, and
        ///     uses the default comparer for the key type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentDictionary<TKey, TValue> Create()
        {
            var handle = NativeObject<ConcurrentDictionary<TKey, TValue>>.Alloc(new ConcurrentDictionary<TKey, TValue>());
            return new UnsafeConcurrentDictionary<TKey, TValue>(handle);
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="concurrencyLevel" /> is less than 1.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="capacity" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeConcurrentDictionary<TKey, TValue> Create(int concurrencyLevel, int capacity)
        {
            var handle = NativeObject<ConcurrentDictionary<TKey, TValue>>.Alloc(new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity));
            return new UnsafeConcurrentDictionary<TKey, TValue>(handle);
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(AllocEnumerator(Handle));

        /// <summary>
        ///     Alloc enumerator
        /// </summary>
        private static NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> AllocEnumerator(ConcurrentDictionary<TKey, TValue> handle) => NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>>.Alloc(BoxEnumerator(handle));

        /// <summary>
        ///     Box enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerator<KeyValuePair<TKey, TValue>> BoxEnumerator(ConcurrentDictionary<TKey, TValue> handle) => handle.GetEnumerator();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
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
        public readonly struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            /// <summary>
            ///     Handle
            /// </summary>
            private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

            /// <summary>
            ///     Handle
            /// </summary>
            private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => Handle.MoveNext();

            /// <summary>
            ///     Reset
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => Handle.Reset();

            /// <summary>
            ///     Current
            /// </summary>
            public KeyValuePair<TKey, TValue> Current => Handle.Current;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                Handle.Dispose();
                _handle.Dispose();
            }
        }

        /// <summary>
        ///     Key collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct KeyCollection : IIsCreated, IReadOnlyCollection<TKey>
        {
            /// <summary>
            ///     NativeConcurrentDictionary
            /// </summary>
            private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

            /// <summary>
            ///     Handle
            /// </summary>
            private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

            /// <summary>
            ///     Is created
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => Handle.Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyCollection(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(AllocEnumerator(Handle));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
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
            public readonly struct Enumerator : IIterator<TKey>, IDisposable
            {
                /// <summary>
                ///     Handle
                /// </summary>
                private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

                /// <summary>
                ///     Handle
                /// </summary>
                private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => Handle.MoveNext();

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => Handle.Reset();

                /// <summary>
                ///     Current
                /// </summary>
                public TKey Current => Handle.Current.Key;

                /// <summary>
                ///     Dispose
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose()
                {
                    Handle.Dispose();
                    _handle.Dispose();
                }
            }
        }

        /// <summary>
        ///     Value collection
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct ValueCollection : IIsCreated, IReadOnlyCollection<TValue>
        {
            /// <summary>
            ///     NativeConcurrentDictionary
            /// </summary>
            private readonly NativeObject<ConcurrentDictionary<TKey, TValue>> _handle;

            /// <summary>
            ///     Handle
            /// </summary>
            private ConcurrentDictionary<TKey, TValue> Handle => _handle.Value;

            /// <summary>
            ///     Is created
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count => Handle.Count;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueCollection(NativeObject<ConcurrentDictionary<TKey, TValue>> handle) => _handle = handle;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            /// <returns>Enumerator</returns>
            public Enumerator GetEnumerator() => new(AllocEnumerator(Handle));

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                ThrowHelpers.ThrowCannotCallGetEnumeratorException();
                return default;
            }

            /// <summary>
            ///     Get enumerator
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
            public readonly struct Enumerator : IIterator<TValue>, IDisposable
            {
                /// <summary>
                ///     Handle
                /// </summary>
                private readonly NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> _handle;

                /// <summary>
                ///     Handle
                /// </summary>
                private IEnumerator<KeyValuePair<TKey, TValue>> Handle => _handle.Value;

                /// <summary>
                ///     Structure
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal Enumerator(NativeObject<IEnumerator<KeyValuePair<TKey, TValue>>> handle) => _handle = handle;

                /// <summary>
                ///     Move next
                /// </summary>
                /// <returns>Moved</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => Handle.MoveNext();

                /// <summary>
                ///     Reset
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => Handle.Reset();

                /// <summary>
                ///     Current
                /// </summary>
                public TValue Current => Handle.Current.Value;

                /// <summary>
                ///     Dispose
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
}