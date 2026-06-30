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
    ///     Unsafe dictionary
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public readonly unsafe struct UnsafeFrozenDictionary<TKey, TValue> : IIsCreated, IDisposable, IEquatable<UnsafeFrozenDictionary<TKey, TValue>>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

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
        public static UnsafeFrozenDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            var index = 0;
            foreach (var kvp in source)
                keyValuePairs[index++] = kvp;
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(NativeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(in UnsafeDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionary<TKey, TValue> Create(in StackallocDictionary<TKey, TValue> source)
        {
            using var keyValuePairs = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
            source.CopyTo(keyValuePairs);
            return new UnsafeFrozenDictionary<TKey, TValue>(keyValuePairs);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        [MustBeDistinct(nameof(source))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFrozenDictionary([MustBeDistinct] ReadOnlySpan<KeyValuePair<TKey, TValue>> source) => _handle = Initialize(source);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(UnsafeFrozenDictionary<TKey, TValue> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is UnsafeFrozenDictionary<TKey, TValue> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("UnsafeFrozenDictionary<{0}, {1}>", SR.GetTypeName(typeof(TKey)), SR.GetTypeName(typeof(TValue)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeFrozenDictionary<TKey, TValue> left, UnsafeFrozenDictionary<TKey, TValue> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeFrozenDictionary<TKey, TValue> left, UnsafeFrozenDictionary<TKey, TValue> right) => !left.Equals(right);

        /// <summary>
        ///     Structure
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
    }
}