using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe collection of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <remarks>
    ///     Writer operations invoke caller-supplied factory delegates while the write lock of the
    ///     target shard is held. Such delegates must not call back into the same dictionary
    ///     instance, because the shard lock is not reentrant and doing so deadlocks; they should
    ///     also be fast, since they block other writers of that shard.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    public readonly struct UnsafeShardedDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeShardedDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Array of shards, each containing a separate hash map and its own reader‑writer lock.
        /// </summary>
        /// <remarks>
        ///     Segment design is inspired by the algorithm outlined at: https://github.com/xacrimon/dashmap
        /// </remarks>
        private readonly NativeArray<Shard> _shards;

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
            get
            {
                if (!TryGetValue(key, out var value))
                    ThrowHelpers.ThrowKeyNotFoundException(key);

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => TryInsertOverwriteExisting(key, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _shards.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => CheckIsEmpty(_shards);

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <value>
        ///     The number of key/value pairs contained in this.
        /// </value>
        /// <exception cref="OverflowException">Thrown when the dictionary contains too many elements.</exception>
        /// <remarks>
        ///     Count has snapshot semantics and represents the number
        ///     of items in this at the moment when Count was accessed.
        /// </remarks>
        public int Count => GetCount(_shards);

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        public KeyCollection Keys => KeyCollection.From(_shards);

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        public ValueCollection Values => ValueCollection.From(_shards);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="shards">The shards that back the collection.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeShardedDictionary(NativeArray<Shard> shards) => _shards = shards;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(UnsafeShardedDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is UnsafeShardedDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("UnsafeShardedDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeShardedDictionary<TKey, TValue> left, UnsafeShardedDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeShardedDictionary<TKey, TValue> left, UnsafeShardedDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (ref var shard in _shards)
                shard.HashMap.Dispose();

            _shards.Dispose();
        }

        /// <summary>
        ///     Removes all keys and values from this.
        /// </summary>
        public void Clear()
        {
            foreach (ref var shard in _shards)
            {
                using (shard.RwLock.EnterWriteScope())
                {
                    shard.HashMap.Clear();
                }
            }
        }

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
        public bool TryAdd(TKey key, TValue value)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashMap.TryAdd(key, value);
            }
        }

        /// <summary>
        ///     Attempts to remove and return the value with the specified key from this.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        ///     When this method returns, <paramref name="value" /> contains the object removed from
        ///     this or the default value of <typeparamref name="TValue" /> if the operation failed.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if an object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                return shard.HashMap.Remove(key, out value);
            }
        }

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
        public bool TryRemove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(keyValuePair.Key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                if (shard.HashMap.TryGetValue(keyValuePair.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value))
                {
                    shard.HashMap.Remove(keyValuePair.Key);
                    return true;
                }
            }

            return false;
        }
#endif

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterReadScope())
            {
                return shard.HashMap.ContainsKey(key);
            }
        }

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
        public bool TryGetValue(TKey key, out TValue value)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterReadScope())
            {
                return shard.HashMap.TryGetValue(key, out value);
            }
        }

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
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                ref var valRef = ref shard.HashMap.GetValueRefOrNullRef(key, out var exists);
                if (exists && EqualityComparer<TValue>.Default.Equals(valRef, comparisonValue))
                {
                    valRef = newValue;
                    return true;
                }
            }

            return false;
        }

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
        /// <remarks>
        ///     <paramref name="valueFactory" /> runs while the write lock of the target shard is held;
        ///     it must not call back into this dictionary instance.
        /// </remarks>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            ThrowHelpers.ThrowIfNull(valueFactory, ExceptionArgument.valueFactory);
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                if (shard.HashMap.TryGetValue(key, out var existingValue))
                    return existingValue;

                var newValue = valueFactory(key);
                shard.HashMap[key] = newValue;
                return newValue;
            }
        }

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
        /// <remarks>
        ///     <paramref name="valueFactory" /> runs while the write lock of the target shard is held;
        ///     it must not call back into this dictionary instance.
        /// </remarks>
        public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            ThrowHelpers.ThrowIfNull(valueFactory, ExceptionArgument.valueFactory);
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                if (shard.HashMap.TryGetValue(key, out var existingValue))
                    return existingValue;

                var newValue = valueFactory(key, factoryArgument);
                shard.HashMap[key] = newValue;
                return newValue;
            }
        }

        /// <summary>
        ///     Adds a key/value pair to this if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist.</param>
        /// <returns>
        ///     The value for the key. This will be either the existing value for the key
        ///     if the key is already in this, or the new value if the key was not in this.
        /// </returns>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                ref var valRef = ref shard.HashMap.GetValueRefOrAddDefault(key, out var exists);
                if (exists)
                    return valRef;

                valRef = value;
                return value;
            }
        }

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
        /// <remarks>
        ///     Both factories run while the write lock of the target shard is held; they must not
        ///     call back into this dictionary instance.
        /// </remarks>
        public TValue AddOrUpdate<TArg>(TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
        {
            ThrowHelpers.ThrowIfNull(addValueFactory, ExceptionArgument.valueFactory);
            ThrowHelpers.ThrowIfNull(updateValueFactory, ExceptionArgument.updateValueFactory);
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                ref var valRef = ref shard.HashMap.GetValueRefOrNullRef(key, out var exists);
                if (exists)
                {
                    var newValue = updateValueFactory(key, valRef, factoryArgument);
                    valRef = newValue;
                    return newValue;
                }
                else
                {
                    var newValue = addValueFactory(key, factoryArgument);
                    shard.HashMap[key] = newValue;
                    return newValue;
                }
            }
        }

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
        /// <remarks>
        ///     Both factories run while the write lock of the target shard is held; they must not
        ///     call back into this dictionary instance.
        /// </remarks>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            ThrowHelpers.ThrowIfNull(addValueFactory, ExceptionArgument.valueFactory);
            ThrowHelpers.ThrowIfNull(updateValueFactory, ExceptionArgument.updateValueFactory);
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                ref var valRef = ref shard.HashMap.GetValueRefOrNullRef(key, out var exists);
                if (exists)
                {
                    var newValue = updateValueFactory(key, valRef);
                    valRef = newValue;
                    return newValue;
                }
                else
                {
                    var newValue = addValueFactory(key);
                    shard.HashMap[key] = newValue;
                    return newValue;
                }
            }
        }

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
        /// <remarks>
        ///     <paramref name="updateValueFactory" /> runs while the write lock of the target shard
        ///     is held; it must not call back into this dictionary instance.
        /// </remarks>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            ThrowHelpers.ThrowIfNull(updateValueFactory, ExceptionArgument.updateValueFactory);
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                ref var valRef = ref shard.HashMap.GetValueRefOrNullRef(key, out var exists);
                if (exists)
                {
                    var newValue = updateValueFactory(key, valRef);
                    valRef = newValue;
                    return newValue;
                }

                shard.HashMap[key] = addValue;
                return addValue;
            }
        }

        /// <summary>
        ///     Gets the shard selected by the specified hash code.
        /// </summary>
        /// <param name="hashCode">The hash code used to select the shard.</param>
        /// <returns>A reference to the shard selected by <paramref name="hashCode" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Shard GetShard(uint hashCode) => ref _shards[I32.FromU32Unchecked(hashCode & U32.FromI32Unchecked(_shards.Length).WrappingSub(U32.FromI32Unchecked(1)))];

        /// <summary>
        ///     Sets the value for the specified key in the shard that owns the key.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        private void TryInsertOverwriteExisting(TKey key, TValue value)
        {
            ref var shard = ref GetShard(U32.FromI32Unchecked(key.GetHashCode()));
            using (shard.RwLock.EnterWriteScope())
            {
                shard.HashMap[key] = value;
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <param name="shards">The shards to inspect.</param>
        /// <returns><see langword="true" /> if every shard is empty; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CheckIsEmpty(NativeArray<Shard> shards)
        {
            foreach (ref var shard in shards)
            {
                using (shard.RwLock.EnterReadScope())
                {
                    if (!shard.HashMap.IsEmpty)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <param name="shards">The shards to sum the counts from.</param>
        /// <returns>
        ///     The total number of key/value pairs contained in all of <paramref name="shards" />.
        /// </returns>
        /// <exception cref="OverflowException">Thrown when the dictionary contains too many elements.</exception>
        /// <remarks>
        ///     Each shard is read while its read lock is held, so the counts
        ///     are accumulated shard by shard rather than captured atomically.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCount(NativeArray<Shard> shards)
        {
            var count = 0;
            foreach (ref var shard in shards)
            {
                using (shard.RwLock.EnterReadScope())
                {
                    checked
                    {
                        count += shard.HashMap.Count;
                    }
                }
            }

            return count;
        }

        /// <summary>
        ///     A single shard consisting of a hash set and its associated lock for concurrent access.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        internal struct Shard
        {
            /// <summary>
            ///     Reader‑writer lock protecting the hash set in this shard.
            /// </summary>
            public UnsafeReaderWriterLock RwLock;

            /// <summary>
            ///     The underlying hash map holding the key‑value pairs for this shard.
            /// </summary>
            public UnsafeDictionary<TKey, TValue> HashMap;
        }

        /// <summary>
        ///     Creates a new instance that is empty, has the default concurrency level, has
        ///     the default initial capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeShardedDictionary<TKey, TValue> Create() => Create(-1, 31);

        /// <summary>
        ///     Creates a new instance that is empty, has the specified concurrency
        ///     level and capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update this concurrently, or -1 to indicate a default value.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that this can contain.
        /// </param>
        /// <returns>A new instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="concurrencyLevel" /> is
        ///     less than 1 or <paramref name="capacity" /> is negative.
        /// </exception>
        public static UnsafeShardedDictionary<TKey, TValue> Create(int concurrencyLevel, int capacity)
        {
            if (concurrencyLevel < 1)
            {
                ThrowHelpers.ThrowIfNotEqual(concurrencyLevel, -1, ExceptionArgument.concurrencyLevel);
                concurrencyLevel = EnvironmentHelpers.DefaultConcurrencyLevel;
            }

            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            concurrencyLevel = I32.FromU32Unchecked(BitOperationsHelpers.RoundUpToPowerOf2(U32.FromI32Unchecked(concurrencyLevel)));
            if (concurrencyLevel < 0)
                concurrencyLevel = 1 << 30;

            var shards = NativeArray<Shard>.Create(concurrencyLevel, CACHE_LINE_SIZE, true);
            foreach (ref var shard in shards)
                shard.HashMap = UnsafeDictionary<TKey, TValue>.Create(capacity);

            return new UnsafeShardedDictionary<TKey, TValue>(shards);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        /// <remarks>
        ///     The enumerator acquires the read lock of each shard in turn. Do not mutate this
        ///     dictionary from the enumerating thread while enumeration is in progress.
        /// </remarks>
        public Enumerator GetEnumerator() => Enumerator.From(_shards);

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
        /// <remarks>
        ///     At most one shard read lock is held at a time. Mutating the dictionary from the
        ///     enumerating thread while enumeration is in progress may deadlock; take a snapshot
        ///     instead.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>, IDisposable
        {
            /// <summary>
            ///     Gets the handle to the underlying object.
            /// </summary>
            private readonly NativeArray<Shard> _handle;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Supports a simple iteration over a generic collection.
            /// </summary>
            private UnsafeDictionary<TKey, TValue>.Enumerator _enumerator;

            /// <summary>
            ///     Indicates whether the enumerator currently holds a read lock on a shard.
            /// </summary>
            private bool _locked;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeArray<Shard> handle)
            {
                _handle = handle;
                _index = -1;
                _enumerator = default;
                _locked = false;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeArray<Shard> handle) => new(handle);

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
            ///     <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (_locked)
                {
                    if (_enumerator.MoveNext())
                        return true;

                    _handle[_index].RwLock.ExitRead();
                    _locked = false;
                }

                while ((_index = _index.WrappingAdd(1)) < _handle.Length)
                {
                    ref var shard = ref _handle[_index];
                    shard.RwLock.EnterRead();
                    _locked = true;
                    _enumerator = shard.HashMap.GetEnumerator();

                    if (_enumerator.MoveNext())
                        return true;

                    shard.RwLock.ExitRead();
                    _locked = false;
                }

                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                if (_locked)
                    _handle[_index].RwLock.ExitRead();

                _index = -1;
                _enumerator = default;
                _locked = false;
            }

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly KeyValuePair<TKey, TValue> Current => _enumerator.Current;

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => Reset();
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
            private readonly NativeArray<Shard> _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            /// <value>
            ///     The number of keys contained in this.
            /// </value>
            /// <exception cref="OverflowException">
            ///     Thrown when the dictionary contains too many elements.
            /// </exception>
            /// <remarks>
            ///     Count has snapshot semantics and represents the number
            ///     of items in this at the moment when Count was accessed.
            /// </remarks>
            public int Count => GetCount(_handle);

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private KeyCollection(NativeArray<Shard> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static KeyCollection From(NativeArray<Shard> handle) => new(handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(UnsafeShardedDictionary<TKey, TValue>.Enumerator.From(_handle));

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
            public struct Enumerator : IIterator<TKey>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private UnsafeShardedDictionary<TKey, TValue>.Enumerator _handle;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeShardedDictionary<TKey, TValue>.Enumerator handle) => _handle = handle;

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeShardedDictionary<TKey, TValue>.Enumerator handle) => new(handle);

                /// <summary>
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => _handle.MoveNext();

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => _handle.Reset();

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TKey Current => _handle.Current.Key;

                /// <summary>
                ///     Performs application-defined tasks associated with
                ///     freeing, releasing, or resetting unmanaged resources.
                /// </summary>
                public void Dispose() => _handle.Dispose();
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
            private readonly NativeArray<Shard> _handle;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public bool IsCreated => _handle.IsCreated;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            /// <value>
            ///     The number of values contained in this.
            /// </value>
            /// <exception cref="OverflowException">
            ///     Thrown when the dictionary contains too many elements.
            /// </exception>
            /// <remarks>
            ///     Count has snapshot semantics and represents the number
            ///     of items in this at the moment when Count was accessed.
            /// </remarks>
            public int Count => GetCount(_handle);

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueCollection(NativeArray<Shard> handle) => _handle = handle;

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="handle">The handle to the underlying object.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ValueCollection From(NativeArray<Shard> handle) => new(handle);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            public Enumerator GetEnumerator() => Enumerator.From(UnsafeShardedDictionary<TKey, TValue>.Enumerator.From(_handle));

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
            public struct Enumerator : IIterator<TValue>, IDisposable
            {
                /// <summary>
                ///     Gets the handle to the underlying object.
                /// </summary>
                private UnsafeShardedDictionary<TKey, TValue>.Enumerator _handle;

                /// <summary>
                ///     Initializes a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                private Enumerator(UnsafeShardedDictionary<TKey, TValue>.Enumerator handle) => _handle = handle;

                /// <summary>
                ///     Creates a new instance of this class.
                /// </summary>
                /// <param name="handle">The handle to the underlying object.</param>
                /// <returns>A new instance of this class.</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static Enumerator From(UnsafeShardedDictionary<TKey, TValue>.Enumerator handle) => new(handle);

                /// <summary>
                ///     Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                ///     <see langword="true" /> if the enumerator was successfully advanced to the next element;
                ///     <see langword="false" /> if the enumerator has passed the end of the collection.
                /// </returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext() => _handle.MoveNext();

                /// <summary>
                ///     Sets the enumerator to its initial position, which
                ///     is before the first element in the collection.
                /// </summary>
                public void Reset() => _handle.Reset();

                /// <summary>
                ///     Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                /// <value>The element in the collection at the current position of the enumerator.</value>
                public readonly TValue Current => _handle.Current.Value;

                /// <summary>
                ///     Performs application-defined tasks associated with
                ///     freeing, releasing, or resetting unmanaged resources.
                /// </summary>
                public void Dispose() => _handle.Dispose();
            }
        }
    }
}