using System;
using System.Collections;
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
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community)]
    [BindingType(typeof(UnsafeShardedDictionary<,>))]
    public readonly unsafe struct NativeShardedDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<NativeShardedDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeShardedDictionary<TKey, TValue>* _handle;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        public UnsafeShardedDictionary<TKey, TValue>.KeyCollection Keys => _handle->Keys;

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        public UnsafeShardedDictionary<TKey, TValue>.ValueCollection Values => _handle->Values;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeShardedDictionary(UnsafeShardedDictionary<TKey, TValue>* handle) => _handle = handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
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
            get => Unsafe.AsRef<UnsafeShardedDictionary<TKey, TValue>>(_handle)[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.AsRef<UnsafeShardedDictionary<TKey, TValue>>(_handle)[key] = value;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeShardedDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeShardedDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeShardedDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeShardedDictionary<TKey, TValue> left, NativeShardedDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeShardedDictionary<TKey, TValue> left, NativeShardedDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Removes all keys and values from this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

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
        public bool TryAdd(TKey key, TValue value) => _handle->TryAdd(key, value);

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
        public bool TryRemove(TKey key, out TValue value) => _handle->TryRemove(key, out value);

#if NET5_0_OR_GREATER
        /// <summary>
        ///     Removes a key and value from this.
        /// </summary>
        /// <param name="keyValuePair">The <see cref="KeyValuePair{TKey,TValue}" /> representing the key and value to remove.</param>
        /// <returns>
        ///     true if the key and value represented by <paramref name="keyValuePair" /> are successfully found and removed;
        ///     otherwise, false.
        /// </returns>
        /// <remarks>
        ///     Both the specified key and value must match the entry in this for it to be removed.
        ///     The key is compared using the default comparer for <typeparamref name="TKey" />.
        ///     The value is compared using the default comparer for <typeparamref name="TValue" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(KeyValuePair<TKey, TValue> keyValuePair) => _handle->TryRemove(keyValuePair);
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
        public bool ContainsKey(TKey key) => _handle->ContainsKey(key);

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
        public bool TryGetValue(TKey key, out TValue value) => _handle->TryGetValue(key, out value);

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
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue) => _handle->TryUpdate(key, newValue, comparisonValue);

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
        ///     key is already in this, or the new value for the key as returned by valueFactory
        ///     if the key was not in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) => _handle->GetOrAdd(key, valueFactory);

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
        ///     key is already in this, or the new value for the key as returned by valueFactory
        ///     if the key was not in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument) => _handle->GetOrAdd(key, valueFactory, factoryArgument);

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
        ///     key is already in this, or the new value if the key was not in this.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value) => _handle->GetOrAdd(key, value);

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
        public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument) => _handle->AddOrUpdate(key, addValueFactory, updateValueFactory, factoryArgument);

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
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory) => _handle->AddOrUpdate(key, addValueFactory, updateValueFactory);

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
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) => _handle->AddOrUpdate(key, addValue, updateValueFactory);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeShardedDictionary<TKey, TValue> Empty => default;

        /// <summary>
        ///     Initializes a new instance of this
        ///     class that is empty, has the default concurrency level, has the default initial capacity, and
        ///     uses the default comparer for the key type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeShardedDictionary<TKey, TValue> Create()
        {
            var value = UnsafeShardedDictionary<TKey, TValue>.Create();
            return new NativeShardedDictionary<TKey, TValue>(Box.New(ref value));
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
        public static NativeShardedDictionary<TKey, TValue> Create(int concurrencyLevel, int capacity)
        {
            var value = UnsafeShardedDictionary<TKey, TValue>.Create(concurrencyLevel, capacity);
            return new NativeShardedDictionary<TKey, TValue>(Box.New(ref value));
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public UnsafeShardedDictionary<TKey, TValue>.Enumerator GetEnumerator() => _handle->GetEnumerator();

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}