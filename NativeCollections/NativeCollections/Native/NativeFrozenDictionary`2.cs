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
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<NativeFrozenDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFrozenDictionaryHandle<TKey, TValue>* _handle;

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
                return handle->Keys(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
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
                return handle->Values(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
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
                return handle->Count(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeFrozenDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeFrozenDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeFrozenDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
                keyValuePairs[index++] = kvp;
            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(UnsafeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(StackallocDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenDictionary([MustBeDistinct] ReadOnlySpan<KeyValuePair<TKey, TValue>> source) => _handle = Initialize(source);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionaryHandle<TKey, TValue>* Initialize(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
        {
            NativeFrozenDictionaryHandle<TKey, TValue>* handle;
            if (source.IsEmpty)
            {
                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<EmptyFrozenDictionary<TKey, TValue>>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<EmptyFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new EmptyFrozenDictionary<TKey, TValue>();
                return handle;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<TKey>())
                {
                    handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<SmallComparableFrozenDictionary<TKey, TValue>>()), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallComparableFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallComparableFrozenDictionary<TKey, TValue>(source);
                    return handle;
                }

                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<SmallFrozenDictionary<TKey, TValue>>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<SmallFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallFrozenDictionary<TKey, TValue>(source);
                return handle;
            }

            if (typeof(TKey) == typeof(int))
            {
                handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<Int32FrozenDictionary<TValue>>()), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenDictionaryHandle<int, TValue>>(handle) = GetNativeHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                Unsafe.AsRef<Int32FrozenDictionary<TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new Int32FrozenDictionary<TValue>(MemoryMarshal.Cast<KeyValuePair<TKey, TValue>, KeyValuePair<int, TValue>>(source));
                return handle;
            }

            handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + Unsafe.SizeOf<DefaultFrozenDictionary<TKey, TValue>>()), CACHE_LINE_SIZE);
            Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
            Unsafe.AsRef<DefaultFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new DefaultFrozenDictionary<TKey, TValue>(source);
            return handle;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
            NativeMemoryAllocator.AlignedFree(handle);
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
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), key);
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
            var handle = _handle;
            return ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), key);
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
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), key);
            exists = Unsafe.IsNullRef(ref Unsafe.AsRef(in reference));
            return ref reference;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeFrozenDictionary<TKey, TValue> Empty => default;

        /// <summary>
        ///     Enumerator
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
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _keys = keys;
                _values = values;
                _index = -1;
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     <code data-dev-comment-type="langword">true</code> if the enumerator was successfully advanced to the next element;
            ///     <code data-dev-comment-type="langword">false</code> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ++_index;
                if ((uint)_index < (uint)_keys.Length)
                    return true;
                _index = _keys.Length;
                return false;
            }

            /// <summary>
            ///     Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _index = -1;

            /// <summary>
            ///     Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if ((uint)_index >= (uint)_keys.Length)
                        ThrowHelpers.ThrowInvalidOperationException();
                    return new KeyValuePair<TKey, TValue>(_keys[_index], _values[_index]);
                }
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
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