using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;
using static NativeCollections.NativeFrozenDictionary;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<NativeFrozenDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeFrozenDictionaryHandle<TKey, TValue>* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a read-only reference to the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <value>
        ///     The value associated with the specified key. If the specified key is not found, the get operation throws
        ///     a <see cref="KeyNotFoundException" />.
        /// </value>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the property is retrieved and <paramref name="key" /> does not exist in the collection.
        /// </exception>
        public ref readonly TValue this[TKey key]
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
                var handle = _handle;
                return handle->Keys(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))).AsReadOnlySpan();
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
                var handle = _handle;
                return handle->Values(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))).AsReadOnlySpan();
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
                var handle = _handle;
                return handle->Count(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeFrozenDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeFrozenDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeFrozenDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        private NativeFrozenDictionary(NativeFrozenDictionaryHandle<TKey, TValue>* handle) => _handle = handle;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The dictionary from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> source)
        {
            using (var keyValuePairs = NativeArray<KeyValuePair<TKey, TValue>>.Create(source.Count))
            {
                var index = 0;
                foreach (var kvp in source)
                    keyValuePairs[(index = index.WrappingAdd(1)).WrappingSub(1)] = kvp;

                return Create(keyValuePairs.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The dictionary from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using (var keyValuePairs = NativeArray<KeyValuePair<TKey, TValue>>.Create(source.Count))
            {
                source.CopyTo(keyValuePairs.AsSpan());
                return Create(keyValuePairs.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The dictionary from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenDictionary<TKey, TValue> Create(UnsafeDictionary<TKey, TValue> source)
        {
            using (var keyValuePairs = NativeArray<KeyValuePair<TKey, TValue>>.Create(source.Count))
            {
                source.CopyTo(keyValuePairs.AsSpan());
                return Create(keyValuePairs.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="source">The dictionary from which to create this.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        public static NativeFrozenDictionary<TKey, TValue> Create(StackallocDictionary<TKey, TValue> source)
        {
            using (var keyValuePairs = NativeArray<KeyValuePair<TKey, TValue>>.Create(source.Count))
            {
                source.CopyTo(keyValuePairs.AsSpan());
                return Create(keyValuePairs.AsReadOnlySpan());
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="source">The span of key/value pairs to initialize from.</param>
        /// <returns>A new instance initialized from <paramref name="source" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create([MustBeDistinct] ReadOnlySpan<KeyValuePair<TKey, TValue>> source) => new(Initialize(source));

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="source">The span of key/value pairs to initialize from.</param>
        /// <returns>A pointer to the newly allocated frozen dictionary handle.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static NativeFrozenDictionaryHandle<TKey, TValue>* Initialize(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
        {
            NativeFrozenDictionaryHandle<TKey, TValue>* handle;
            if (source.IsEmpty)
            {
                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<EmptyFrozenDictionary<TKey, TValue>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<EmptyFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = default;
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<TKey>())
                {
                    handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<SmallComparableFrozenDictionary<TKey, TValue>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallComparableFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = SmallComparableFrozenDictionary<TKey, TValue>.Create(source);
                    return handle;
                }

                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<SmallFrozenDictionary<TKey, TValue>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<SmallFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = SmallFrozenDictionary<TKey, TValue>.Create(source);
                return handle;
            }

            if (typeof(TKey) == typeof(int))
            {
                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<Int32FrozenDictionary<TValue>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                Unsafe.AsRef<NativeFrozenDictionaryHandle<int, TValue>>(handle) = GetNativeHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                Unsafe.AsRef<Int32FrozenDictionary<TValue>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = Int32FrozenDictionary<TValue>.Create(MemoryMarshal.Cast<KeyValuePair<TKey, TValue>, KeyValuePair<int, TValue>>(source));
                return handle;
            }

            handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(CACHE_LINE_SIZE).WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<DefaultFrozenDictionary<TKey, TValue>>())), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
            Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
            Unsafe.AsRef<DefaultFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE))) = DefaultFrozenDictionary<TKey, TValue>.Create(source);
            return handle;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;

            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Determines whether this contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in this.</param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) => !Unsafe.IsNullRef(ref Unsafe.AsRef(in GetValueRefOrNullRef(key)));

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     <paramref name="value" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with
        ///     the specified key; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), key);
            if (!Unsafe.IsNullRef(ref Unsafe.AsRef(in reference)))
            {
                value = reference;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(TKey key)
        {
            var handle = _handle;
            return ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), key);
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <param name="key">The key used for lookup.</param>
        /// <param name="exists">Whether or not the given key already existed in this.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(TKey key, out bool exists)
        {
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)), key);
            exists = !Unsafe.IsNullRef(ref Unsafe.AsRef(in reference));
            return ref reference;
        }

        /// <summary>
        ///     Supports a simple iteration over a generic collection.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IIterator<KeyValuePair<TKey, TValue>>
        {
            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            private readonly NativeArray<TKey> _keys;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            private readonly NativeArray<TValue> _values;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="keys">The native array that contains the keys.</param>
            /// <param name="values">The native array that contains the values.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Enumerator(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _keys = keys;
                _values = values;
                _index = -1;
            }

            /// <summary>
            ///     Creates a new instance of this class.
            /// </summary>
            /// <param name="keys">The native array that contains the keys.</param>
            /// <param name="values">The native array that contains the values.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static Enumerator From(NativeArray<TKey> keys, NativeArray<TValue> values) => new(keys, values);

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
                _index = _index.WrappingAdd(1);
                if (U32.FromI32Unchecked(_index) < U32.FromI32Unchecked(_keys.Length))
                    return true;

                _index = _keys.Length;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The element in the collection at the current position of the enumerator.</value>
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (U32.FromI32Unchecked(_index) >= U32.FromI32Unchecked(_keys.Length))
                        ThrowHelpers.ThrowInvalidOperationException();

                    return new KeyValuePair<TKey, TValue>(_keys[_index], _values[_index]);
                }
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, Isize.FromI32Unchecked(CACHE_LINE_SIZE)));
        }

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
    }
}