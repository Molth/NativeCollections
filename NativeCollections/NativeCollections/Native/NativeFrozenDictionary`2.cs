using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.ArchitectureHelpers;
using static NativeCollections.NativeFrozenDictionary;
#if !NET5_0_OR_GREATER
using System.Buffers;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public readonly unsafe struct NativeFrozenDictionary<TKey, TValue> : IDisposable, IEquatable<NativeFrozenDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFrozenDictionaryHandle<TKey, TValue>* _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Get value
        /// </summary>
        /// <param name="key">Key</param>
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
        ///     Is empty
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Keys
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
        ///     Values
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
        ///     Count
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
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFrozenDictionary<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFrozenDictionary<TKey, TValue> nativeDictionary && nativeDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeFrozenDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFrozenDictionary<TKey, TValue> left, NativeFrozenDictionary<TKey, TValue> right) => left._handle != right._handle;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create<TReadOnlyCollection>(in TReadOnlyCollection source) where TReadOnlyCollection : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(in UnsafeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionary<TKey, TValue> Create(in StackallocDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new NativeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
        {
            if (source.Length == 0)
            {
                var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(EmptyFrozenDictionary<TKey, TValue>)), CACHE_LINE_SIZE);
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<EmptyFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new EmptyFrozenDictionary<TKey, TValue>();
                _handle = handle;
                return;
            }

            if (source.Length <= 10)
            {
                if (FrozenHelpers.IsKnownComparable<TKey>())
                {
#if NET5_0_OR_GREATER
                    using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Length);
#else
                    var keyValuePairs = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(source.Length);
#endif
                    var index = 0;
                    foreach (var kvp in source)
                    {
                        keyValuePairs[index] = kvp;
                        ++index;
                    }

#if NET5_0_OR_GREATER
                    keyValuePairs.AsSpan().Sort(static (x, y) => Comparer<TKey>.Default.Compare(x.Key, y.Key));
#else
                    Array.Sort(keyValuePairs, 0, source.Length, FrozenHelpers.KeyValuePairComparer<TKey, TValue>.Default);
#endif
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(SmallComparableFrozenDictionary<TKey, TValue>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallComparableFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallComparableFrozenDictionary<TKey, TValue>(keyValuePairs.AsSpan(0, source.Length));
#if !NET5_0_OR_GREATER
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(keyValuePairs);
#endif
                    _handle = handle;
                }
                else
                {
                    var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
                    var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(source.Length * sizeof(TKey)), alignment);
                    var buckets = (TKey*)NativeMemoryAllocator.AlignedAlloc((uint)(bucketsByteCount + source.Length * sizeof(TValue)), alignment);
                    var entries = UnsafeHelpers.AddByteOffset<TValue>(buckets, (nint)bucketsByteCount);
                    var keys = new NativeArray<TKey>(buckets, source.Length);
                    var values = new NativeArray<TValue>(entries, source.Length);
                    var index = 0;
                    foreach (var kvp in source)
                    {
                        keys[index] = kvp.Key;
                        values[index] = kvp.Value;
                        ++index;
                    }

                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(SmallFrozenDictionary<TKey, TValue>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new SmallFrozenDictionary<TKey, TValue>(keys, values);
                    _handle = handle;
                }
            }
            else
            {
                using var buffer = new NativeArray<KeyValuePair<TKey, TValue>>(source.Length);
                var index = 0;
                foreach (var kvp in source)
                {
                    buffer[index] = kvp;
                    ++index;
                }

                if (typeof(TKey) == typeof(int))
                {
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(Int32FrozenDictionary<TValue>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<int, TValue>>(handle) = GetNativeHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                    Unsafe.AsRef<Int32FrozenDictionary<TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new Int32FrozenDictionary<TValue>(buffer.Cast<KeyValuePair<int, TValue>>());
                    _handle = handle;
                }
                else
                {
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(CACHE_LINE_SIZE + sizeof(DefaultFrozenDictionary<TKey, TValue>)), CACHE_LINE_SIZE);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetNativeHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<DefaultFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE)) = new DefaultFrozenDictionary<TKey, TValue>(buffer);
                    _handle = handle;
                }
            }
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => !Unsafe.IsNullRef(ref Unsafe.AsRef(in GetValueRefOrNullRef(key)));

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
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
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key)
        {
            var handle = _handle;
            return ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE), key);
        }

        /// <summary>
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="exists">Exists</param>
        /// <returns>Value ref</returns>
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
        public static NativeFrozenDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Enumerator
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            /// <summary>
            ///     Keys
            /// </summary>
            private readonly NativeArray<TKey> _keys;

            /// <summary>
            ///     Values
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

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
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

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
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
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, CACHE_LINE_SIZE));
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowHelpers.ThrowCannotCallGetEnumeratorException();
            return default;
        }
    }
}