using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeFrozenDictionary;

#if !NET7_0_OR_GREATER
#pragma warning disable CS9082 // Local is returned by reference but was initialized to a value that cannot be returned by reference
#pragma warning disable CS9083 // A member is returned by reference but was initialized to a value that cannot be returned by reference 
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeFrozenDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeFrozenDictionaryHandle<TKey, TValue> _handle;

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
        public ref readonly TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref readonly var local = ref GetValueRefOrNullRef(key);
                if (!Unsafe.IsNullRef(ref Unsafe.AsRef(in local)))
                    return ref local;
                ThrowHelpers.ThrowKeyNotFoundException(key);
                return ref local;
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        public ReadOnlySpan<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Keys(ref handle.Value);
            }
        }

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        public ReadOnlySpan<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Values(ref handle.Value);
            }
        }

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Count(ref handle.Value);
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
                keyValuePairs[index++] = kvp;
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(UnsafeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(StackallocDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenDictionary([MustBeDistinct] ReadOnlySpan<KeyValuePair<TKey, TValue>> source) => _handle = Initialize(source);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(UnsafeFrozenDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is UnsafeFrozenDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("UnsafeFrozenDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeFrozenDictionary<TKey, TValue> left, UnsafeFrozenDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeFrozenDictionary<TKey, TValue> left, UnsafeFrozenDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnsafeFrozenDictionaryHandle<TKey, TValue> Initialize(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
        {
            UnsafeFrozenDictionaryHandle<TKey, TValue> handle;
            if (source.IsEmpty)
            {
                handle = GetUnsafeHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.As<UnsafeFrozenDictionaryValue, EmptyFrozenDictionary<TKey, TValue>>(ref handle.Value) = new EmptyFrozenDictionary<TKey, TValue>();
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<TKey>())
                {
                    handle = GetUnsafeHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.As<UnsafeFrozenDictionaryValue, SmallComparableFrozenDictionary<TKey, TValue>>(ref handle.Value) = new SmallComparableFrozenDictionary<TKey, TValue>(source);
                    return handle;
                }

                handle = GetUnsafeHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.As<UnsafeFrozenDictionaryValue, SmallFrozenDictionary<TKey, TValue>>(ref handle.Value) = new SmallFrozenDictionary<TKey, TValue>(source);
                return handle;
            }

            if (typeof(TKey) == typeof(int))
            {
                var int32Handle = GetUnsafeHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                handle = Unsafe.As<UnsafeFrozenDictionaryHandle<int, TValue>, UnsafeFrozenDictionaryHandle<TKey, TValue>>(ref int32Handle);
                Unsafe.As<UnsafeFrozenDictionaryValue, Int32FrozenDictionary<TValue>>(ref handle.Value) = new Int32FrozenDictionary<TValue>(MemoryMarshal.Cast<KeyValuePair<TKey, TValue>, KeyValuePair<int, TValue>>(source));
                return handle;
            }

            handle = GetUnsafeHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
            Unsafe.As<UnsafeFrozenDictionaryValue, DefaultFrozenDictionary<TKey, TValue>>(ref handle.Value) = new DefaultFrozenDictionary<TKey, TValue>(source);
            return handle;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => !Unsafe.IsNullRef(ref Unsafe.AsRef(in GetValueRefOrNullRef(key)));

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="value" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            ref readonly var reference = ref handle.GetValueRefOrNullRef(ref handle.Value, key);
            if (!Unsafe.IsNullRef(ref Unsafe.AsRef(in reference)))
            {
                value = reference;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return ref handle.GetValueRefOrNullRef(ref handle.Value, key);
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" /> in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not a new entry for the given key was added to this.</param>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" /> is in use.
        ///     The ref null can be detected using System.Runtime.CompilerServices.Unsafe.IsNullRef
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            ref readonly var reference = ref handle.GetValueRefOrNullRef(ref handle.Value, key);
            exists = Unsafe.IsNullRef(ref Unsafe.AsRef(in reference));
            return ref reference;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeFrozenDictionary<TKey, TValue> Empty => default;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.GetEnumerator(ref handle.Value);
        }

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