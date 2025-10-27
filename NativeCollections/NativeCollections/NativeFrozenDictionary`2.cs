using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.NativeFrozenDictionary;
#if !NET5_0_OR_GREATER
using System.Buffers;
#endif

#pragma warning disable CS1591
#pragma warning disable CS9082

// Resharper disable ALL

namespace NativeCollections
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public unsafe readonly struct NativeFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
    {
        private readonly NativeFrozenDictionaryHandle<TKey, TValue>* _handle;

        public static NativeFrozenDictionary<TKey, TValue> Empty => new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in TKey key) => !Unsafe.IsNullRef(ref Unsafe.AsRef(in GetValueRefOrNullRef(key)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in TKey key, out TValue value)
        {
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE), key);
            if (!Unsafe.IsNullRef(ref Unsafe.AsRef(in reference)))
            {
                value = reference;
                return true;
            }

            value = default;
            return false;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key)
        {
            var handle = _handle;
            return ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE), key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly TValue GetValueRefOrNullRef(in TKey key, out bool exists)
        {
            var handle = _handle;
            ref readonly var reference = ref handle->GetValueRefOrNullRef(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE), key);
            exists = Unsafe.IsNullRef(ref Unsafe.AsRef(in reference));
            return ref reference;
        }

        public ReadOnlySpan<TKey> Keys
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                return handle->Keys(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE));
            }
        }

        public ReadOnlySpan<TValue> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                return handle->Values(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE));
            }
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var handle = _handle;
                return handle->Count(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            handle->Dispose(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE));
            NativeMemoryAllocator.AlignedFree(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFrozenDictionary(ICollection<KeyValuePair<TKey, TValue>> source)
        {
            if (source.Count == 0)
            {
                var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeFrozenDictionaryHandle<TKey, TValue>>(), NativeMemoryAllocator.AlignOf<EmptyFrozenDictionary<TKey, TValue>>());
                var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(NativeFrozenDictionaryHandle<TKey, TValue>) + ArchitectureHelpers.CACHE_LINE_SIZE + sizeof(EmptyFrozenDictionary<TKey, TValue>)), (uint)alignment);
                Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetHandle<EmptyFrozenDictionary<TKey, TValue>, TKey, TValue>();
                Unsafe.AsRef<EmptyFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE)) = new EmptyFrozenDictionary<TKey, TValue>();
                _handle = handle;
                return;
            }

            if (source.Count <= 10)
            {
                if (Constants.IsKnownComparable<TKey>())
                {
#if NET5_0_OR_GREATER
                    using var kvps = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
#else
                    var kvps = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(source.Count);
#endif
                    var index = 0;
                    foreach (var kvp in source)
                    {
                        kvps[index] = kvp;
                        ++index;
                    }

#if NET5_0_OR_GREATER
                    kvps.AsSpan().Sort(static (x, y) => Comparer<TKey>.Default.Compare(x.Key, y.Key));
#else
                    Array.Sort(kvps, 0, source.Count, KeyValuePairComparer<TKey, TValue>.Default);
#endif
                    var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeFrozenDictionaryHandle<TKey, TValue>>(), NativeMemoryAllocator.AlignOf<SmallComparableFrozenDictionary<TKey, TValue>>());
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(NativeFrozenDictionaryHandle<TKey, TValue>) + ArchitectureHelpers.CACHE_LINE_SIZE + sizeof(SmallComparableFrozenDictionary<TKey, TValue>)), (uint)alignment);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetHandle<SmallComparableFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallComparableFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE)) = new SmallComparableFrozenDictionary<TKey, TValue>(kvps);
#if !NET5_0_OR_GREATER
                    ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(kvps);
#endif
                    _handle = handle;
                }
                else
                {
                    var keys = new NativeArray<TKey>(source.Count);
                    var values = new NativeArray<TValue>(source.Count);
                    var index = 0;
                    foreach (var kvp in source)
                    {
                        keys[index] = kvp.Key;
                        values[index] = kvp.Value;
                        ++index;
                    }

                    var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeFrozenDictionaryHandle<TKey, TValue>>(), NativeMemoryAllocator.AlignOf<SmallFrozenDictionary<TKey, TValue>>());
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(NativeFrozenDictionaryHandle<TKey, TValue>) + ArchitectureHelpers.CACHE_LINE_SIZE + sizeof(SmallFrozenDictionary<TKey, TValue>)), (uint)alignment);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetHandle<SmallFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<SmallFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE)) = new SmallFrozenDictionary<TKey, TValue>(keys, values);
                    _handle = handle;
                }
            }
            else
            {
                var kvps = new NativeArray<KeyValuePair<TKey, TValue>>(source.Count);
                var index = 0;
                foreach (var kvp in source)
                {
                    kvps[index] = kvp;
                    ++index;
                }

                if (typeof(TKey) == typeof(int))
                {
                    var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeFrozenDictionaryHandle<TKey, TValue>>(), NativeMemoryAllocator.AlignOf<Int32FrozenDictionary<TValue>>());
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(NativeFrozenDictionaryHandle<TKey, TValue>) + ArchitectureHelpers.CACHE_LINE_SIZE + sizeof(Int32FrozenDictionary<TValue>)), (uint)alignment);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<int, TValue>>(handle) = GetHandle<Int32FrozenDictionary<TValue>, int, TValue>();
                    Unsafe.AsRef<Int32FrozenDictionary<TValue>>(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE)) = new Int32FrozenDictionary<TValue>(kvps.Cast<KeyValuePair<int, TValue>>());
                    _handle = handle;
                }
                else
                {
                    var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeFrozenDictionaryHandle<TKey, TValue>>(), NativeMemoryAllocator.AlignOf<DefaultFrozenDictionary<TKey, TValue>>());
                    var handle = (NativeFrozenDictionaryHandle<TKey, TValue>*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(NativeFrozenDictionaryHandle<TKey, TValue>) + ArchitectureHelpers.CACHE_LINE_SIZE + sizeof(DefaultFrozenDictionary<TKey, TValue>)), (uint)alignment);
                    Unsafe.AsRef<NativeFrozenDictionaryHandle<TKey, TValue>>(handle) = GetHandle<DefaultFrozenDictionary<TKey, TValue>, TKey, TValue>();
                    Unsafe.AsRef<DefaultFrozenDictionary<TKey, TValue>>(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE)) = new DefaultFrozenDictionary<TKey, TValue>(kvps);
                    _handle = handle;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            var handle = _handle;
            return handle->GetEnumerator(UnsafeHelpers.AddByteOffset(handle, ArchitectureHelpers.CACHE_LINE_SIZE));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator
        {
            private readonly NativeArray<TKey> _keys;
            private readonly NativeArray<TValue> _values;
            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                this._keys = keys;
                this._values = values;
                this._index = -1;
            }

            /// <summary>Advances the enumerator to the next element of the collection.</summary>
            /// <returns>
            ///     <code data-dev-comment-type="langword">true</code> if the enumerator was successfully advanced to the next element;
            ///     <code data-dev-comment-type="langword">false</code> if the enumerator has passed the end of the collection.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                ++this._index;
                if ((uint)this._index < (uint)this._keys.Length)
                    return true;
                this._index = this._keys.Length;
                return false;
            }

            /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
            /// <returns>The element in the collection at the current position of the enumerator.</returns>
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if ((uint)this._index >= (uint)this._keys.Length)
                        ThrowHelpers.ThrowInvalidOperationException();
                    return new KeyValuePair<TKey, TValue>(this._keys[this._index], this._values[this._index]);
                }
            }
        }
    }
}