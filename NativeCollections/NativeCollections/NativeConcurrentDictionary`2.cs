using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentDictionary
    ///     (Slower than ConcurrentDictionary)
    /// </summary>
    /// <typeparam name="TKey">Type</typeparam>
    /// <typeparam name="TValue">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeConcurrentDictionary<TKey, TValue> : IDisposable, IEquatable<NativeConcurrentDictionary<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentDictionary<TKey, TValue>* _handle;

        /// <summary>
        ///     Keys
        /// </summary>
        public  UnsafeConcurrentDictionary<TKey, TValue>.KeyCollection Keys => _handle->Keys;

        /// <summary>
        ///     Values
        /// </summary>
        public  UnsafeConcurrentDictionary<TKey, TValue>.ValueCollection Values => _handle->Values;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="concurrencyLevel">Concurrency level</param>
        /// <param name="capacity">Capacity</param>
        /// <param name="growLockArray">Grow lock array</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentDictionary(int size, int maxFreeSlabs, int concurrencyLevel, int capacity, bool growLockArray)
        {
            var value = new UnsafeConcurrentDictionary<TKey, TValue>(size, maxFreeSlabs, concurrencyLevel, capacity, growLockArray);   
            var handle = (UnsafeConcurrentDictionary<TKey, TValue>*)NativeMemoryAllocator.Alloc((uint)sizeof(UnsafeConcurrentDictionary<TKey, TValue>));
            *handle = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="key">Key</param>
        public TValue this[in TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (*_handle)[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => (*_handle)[key] = value;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->IsEmpty;
        }

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Count;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentDictionary<TKey, TValue> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentDictionary<TKey, TValue> nativeConcurrentDictionary && nativeConcurrentDictionary == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentDictionary<TKey, TValue> left, NativeConcurrentDictionary<TKey, TValue> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentDictionary<TKey, TValue> left, NativeConcurrentDictionary<TKey, TValue> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Dispose();
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in TKey key, in TValue value) => _handle->TryAdd(key, value);

        /// <summary>
        ///     Try remove
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(in TKey key, out TValue value) => _handle->TryRemove(key, out value);

        /// <summary>
        ///     Try remove
        /// </summary>
        /// <param name="keyValuePair">Key value pair</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(in KeyValuePair<TKey, TValue> keyValuePair) => _handle->TryRemove(keyValuePair);

        /// <summary>
        ///     Contains key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Contains key</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => _handle->ContainsKey(key);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value) => _handle->TryGetValue(key, out value);

        /// <summary>
        ///     Try to get the value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValueReference(in TKey key, out NativeReference<TValue> value) => _handle->TryGetValueReference(key, out value);

        /// <summary>
        ///     Try update
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="newValue">New value</param>
        /// <param name="comparisonValue">Comparison value</param>
        /// <returns>Updated</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(in TKey key, in TValue newValue, in TValue comparisonValue) => _handle->TryUpdate(key, newValue, comparisonValue);

        /// <summary>
        ///     Get or add value
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public TValue GetOrAdd(in TKey key, in TValue value) => _handle->GetOrAdd(key, value);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentDictionary<TKey, TValue> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public UnsafeConcurrentDictionary<TKey, TValue>.Enumerator GetEnumerator() => _handle->GetEnumerator();
    }
}