using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeFrozenDictionary;
#if !NET5_0_OR_GREATER
using System.Buffers;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632
#pragma warning disable CS9082
#pragma warning disable CS9092

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenDictionary<TKey, TValue> : IDisposable, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeFrozenDictionaryHandle<TKey, TValue> _handle;

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
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Keys(ref handle.Value);
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
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Values(ref handle.Value);
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
                ref var handle = ref Unsafe.AsRef(in _handle);
                return handle.Count(ref handle.Value);
            }
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create<TReadOnlyCollection>(in TReadOnlyCollection source) where TReadOnlyCollection : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(in UnsafeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(in StackallocDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
            {
                keyValuePairs[index] = kvp;
                ++index;
            }

            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
        {
            if (source.Length == 0)
            {
                var handle = GetUnsafeHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.As<UnsafeFrozenDictionaryValue, EmptyFrozenDictionary<TKey, TValue>>(ref handle.Value) = new EmptyFrozenDictionary<TKey, TValue>();
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
                    var handle = GetUnsafeHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.As<UnsafeFrozenDictionaryValue, SmallComparableFrozenDictionary<TKey, TValue>>(ref handle.Value) = new SmallComparableFrozenDictionary<TKey, TValue>(keyValuePairs.AsSpan(0, source.Length));
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

                    var handle = GetUnsafeHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.As<UnsafeFrozenDictionaryValue, SmallFrozenDictionary<TKey, TValue>>(ref handle.Value) = new SmallFrozenDictionary<TKey, TValue>(keys, values);
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
                    var handle = GetUnsafeHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                    Unsafe.As<UnsafeFrozenDictionaryValue, Int32FrozenDictionary<TValue>>(ref handle.Value) = new Int32FrozenDictionary<TValue>(buffer.Cast<KeyValuePair<int, TValue>>());
                    _handle = Unsafe.As<UnsafeFrozenDictionaryHandle<int, TValue>, UnsafeFrozenDictionaryHandle<TKey, TValue>>(ref handle);
                }
                else
                {
                    var handle = GetUnsafeHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.As<UnsafeFrozenDictionaryValue, DefaultFrozenDictionary<TKey, TValue>>(ref handle.Value) = new DefaultFrozenDictionary<TKey, TValue>(buffer);
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
            ref var handle = ref Unsafe.AsRef(in _handle);
            handle.Dispose(ref handle.Value);
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
        ///     Get value ref
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value ref</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key)
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return ref handle.GetValueRefOrNullRef(ref handle.Value, key);
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
            ref var handle = ref Unsafe.AsRef(in _handle);
            ref readonly var reference = ref handle.GetValueRefOrNullRef(ref handle.Value, key);
            exists = Unsafe.IsNullRef(ref Unsafe.AsRef(in reference));
            return ref reference;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFrozenDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            ref var handle = ref Unsafe.AsRef(in _handle);
            return handle.GetEnumerator(ref handle.Value);
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