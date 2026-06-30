using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;
using static NativeCollections.FrozenHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native frozen dictionary
    /// </summary>
    internal static unsafe class NativeFrozenDictionary
    {
        /// <summary>
        ///     Sorts a pair of spans (one containing the keys and the other containing the corresponding items)
        ///     based on the keys in the first <see cref="Span{TKey}" /> using the <see cref="IComparable{T}" />
        ///     implementation of each key.
        /// </summary>
        /// <typeparam name="TKey">The type of the elements of the key span.</typeparam>
        /// <typeparam name="TValue">The type of the elements of the items span.</typeparam>
        /// <param name="keys">The span that contains the keys to sort.</param>
        /// <param name="items">The span that contains the items that correspond to the keys in <paramref name="keys" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> items)
        {
            var comparer = Comparer<TKey>.Default;
            for (var i = 0; i < keys.Length - 1; ++i)
            {
                var key = keys[i + 1];
                var value = items[i + 1];
                int j;
                for (j = i; j >= 0 && comparer.Compare(key, keys[j]) < 0; --j)
                {
                    keys[j + 1] = keys[j];
                    items[j + 1] = items[j];
                }

                keys[j + 1] = key;
                items[j + 1] = value;
            }
        }

        /// <summary>
        ///     Copies the keys and values from a read-only span of key-value pairs into separate key and value spans.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The read-only span containing the key-value pairs to copy from.</param>
        /// <param name="keys">The destination span for the keys. Must have length at least equal to the source length.</param>
        /// <param name="values">The destination span for the values. Must have length at least equal to the source length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyTo<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> source, Span<TKey> keys, Span<TValue> values)
        {
            for (var i = 0; i < source.Length; ++i)
            {
                ref readonly var kvp = ref source[i];
                keys[i] = kvp.Key;
                values[i] = kvp.Value;
            }
        }

        /// <summary>
        ///     Get value ref or null ref
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly TValue GetValueRefOrNullRef<T, TKey, TValue>(void* ptr, in TKey key) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => ref Unsafe.AsRef<T>(ptr).GetValueRefOrNullRef(key);

        /// <summary>
        ///     Keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TKey> Keys<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Keys();

        /// <summary>
        ///     Values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TValue> Values<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Values();

        /// <summary>
        ///     Count
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Count;

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).GetEnumerator();

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Dispose();

        /// <summary>
        ///     Get handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionaryHandle<TKey, TValue> GetNativeHandle<T, TKey, TValue>() where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => new(&GetValueRefOrNullRef<T, TKey, TValue>, &Keys<T, TKey, TValue>, &Values<T, TKey, TValue>, &Count<T, TKey, TValue>, &GetEnumerator<T, TKey, TValue>, &Dispose<T, TKey, TValue>);

        /// <summary>
        ///     Get value ref or null ref
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly TValue GetValueRefOrNullRef<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr, in TKey key) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => ref Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).GetValueRefOrNullRef(key);

        /// <summary>
        ///     Keys
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TKey> Keys<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Keys();

        /// <summary>
        ///     Values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TValue> Values<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Values();

        /// <summary>
        ///     Count
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Count;

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).GetEnumerator();

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Dispose();

        /// <summary>
        ///     Get handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionaryHandle<TKey, TValue> GetUnsafeHandle<T, TKey, TValue>() where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => new(&GetValueRefOrNullRef<T, TKey, TValue>, &Keys<T, TKey, TValue>, &Values<T, TKey, TValue>, &Count<T, TKey, TValue>, &GetEnumerator<T, TKey, TValue>, &Dispose<T, TKey, TValue>);

        /// <summary>
        ///     Frozen dictionary
        /// </summary>
        public interface IFrozenDictionary<TKey, TValue> : IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Count
            /// </summary>
            int Count { get; }

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            ref readonly TValue GetValueRefOrNullRef(in TKey key);

            /// <summary>
            ///     Keys
            /// </summary>
            NativeArray<TKey> Keys();

            /// <summary>
            ///     Values
            /// </summary>
            NativeArray<TValue> Values();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator();
        }

        /// <summary>
        ///     Native frozen dictionary handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct NativeFrozenDictionaryHandle<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            public readonly delegate* managed<void*, in TKey, ref readonly TValue> GetValueRefOrNullRef;

            /// <summary>
            ///     Keys
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<TKey>> Keys;

            /// <summary>
            ///     Values
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<TValue>> Values;

            /// <summary>
            ///     Count
            /// </summary>
            public readonly delegate* managed<void*, int> Count;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            public readonly delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Dispose
            /// </summary>
            public readonly delegate* managed<void*, void> Dispose;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionaryHandle(delegate* managed<void*, in TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<void*, NativeArray<TKey>> keys, delegate* managed<void*, NativeArray<TValue>> values, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose)
            {
                GetValueRefOrNullRef = getValueRefOrNullRef;
                Keys = keys;
                Values = values;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
            }
        }

        /// <summary>
        ///     Unsafe frozen dictionary handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public struct UnsafeFrozenDictionaryHandle<TKey, TValue> : IIsCreated where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Is created
            /// </summary>
            public readonly bool IsCreated => GetValueRefOrNullRef != null;

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, in TKey, ref readonly TValue> GetValueRefOrNullRef;

            /// <summary>
            ///     Keys
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TKey>> Keys;

            /// <summary>
            ///     Values
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TValue>> Values;

            /// <summary>
            ///     Count
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, int> Count;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeFrozenDictionary<TKey, TValue>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Dispose
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, void> Dispose;

            /// <summary>
            ///     Value
            /// </summary>
            public UnsafeFrozenDictionaryValue Value;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeFrozenDictionaryHandle(delegate* managed<ref UnsafeFrozenDictionaryValue, in TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TKey>> keys, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TValue>> values, delegate* managed<ref UnsafeFrozenDictionaryValue, int> count, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenDictionaryValue, void> dispose)
            {
                GetValueRefOrNullRef = getValueRefOrNullRef;
                Keys = keys;
                Values = values;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
                Value = new UnsafeFrozenDictionaryValue();
            }
        }

        /// <summary>
        ///     Value
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public readonly struct UnsafeFrozenDictionaryValue
        {
            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private readonly EmptyFrozenDictionary<int, int> _element0;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private readonly SmallFrozenDictionary<int, int> _element1;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private readonly SmallComparableFrozenDictionary<int, int> _element2;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private readonly Int32FrozenDictionary<int> _element3;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private readonly DefaultFrozenDictionary<int, int> _element4;
        }

        /// <summary>
        ///     Empty frozen dictionary
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key) => ref Unsafe.NullRef<TValue>();

            /// <summary>
            ///     Keys
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => NativeArray<TKey>.Empty;

            /// <summary>
            ///     Values
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => NativeArray<TValue>.Empty;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(NativeArray<TKey>.Empty, NativeArray<TValue>.Empty);

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

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => 0;
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     Small frozen dictionary
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
                var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(source.Length * Unsafe.SizeOf<TKey>()), alignment);
                var buckets = (TKey*)NativeMemoryAllocator.AlignedAlloc((uint)(bucketsByteCount + source.Length * Unsafe.SizeOf<TValue>()), alignment);
                var entries = UnsafeHelpers.AddByteOffset<TValue>(buckets, (nint)bucketsByteCount);
                var keys = new NativeArray<TKey>(buckets, source.Length);
                var values = new NativeArray<TValue>(entries, source.Length);
                CopyTo(source, keys, values);
                _keys = keys;
                _values = values;
            }

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                var index = _keys.AsReadOnlySpan().IndexOf(key);
                return ref index >= 0 ? ref _values[index] : ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Keys
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Values
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _keys.Length;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

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

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _keys.Dispose();
        }

        /// <summary>
        ///     Small comparable frozen dictionary
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallComparableFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
            ///     Max
            /// </summary>
            private TKey Max => _keys[^1];

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallComparableFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(source.Length * Unsafe.SizeOf<TKey>()), CACHE_LINE_SIZE);
                var keysPtr = (TKey*)NativeMemoryAllocator.AlignedAlloc((uint)(bucketsByteCount + source.Length * Unsafe.SizeOf<TValue>()), CACHE_LINE_SIZE);
                var valuesPtr = UnsafeHelpers.AddByteOffset<TValue>(keysPtr, (nint)bucketsByteCount);
                var keys = new NativeArray<TKey>(keysPtr, source.Length);
                var values = new NativeArray<TValue>(valuesPtr, source.Length);
                CopyTo(source, keys, values);
                InsertionSort<TKey, TValue>(keys, values);
                _keys = keys;
                _values = values;
            }

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                if (Comparer<TKey>.Default.Compare(key, Max) <= 0)
                {
                    var keys = _keys.AsReadOnlySpan();
                    for (var index = 0; index < keys.Length; ++index)
                    {
                        var num = Comparer<TKey>.Default.Compare(key, keys[index]);
                        if (num <= 0)
                        {
                            if (num == 0)
                                return ref _values[index];
                            break;
                        }
                    }
                }

                return ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Keys
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Values
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _keys.Length;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

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

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _keys.Dispose();
        }

        /// <summary>
        ///     Int32 frozen dictionary
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Int32FrozenDictionary<TValue> : IFrozenDictionary<int, TValue>, IReadOnlyCollection<KeyValuePair<int, TValue>> where TValue : unmanaged
        {
            /// <summary>
            ///     Frozen hash table
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Values
            /// </summary>
            private readonly NativeArray<TValue> _values;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Int32FrozenDictionary(ReadOnlySpan<KeyValuePair<int, TValue>> source)
            {
                _values = new NativeArray<TValue>(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; ++index)
                    hashCodes[index] = source[index].Key;
                _hashTable = FrozenHashTable.Create(hashCodes, true);
                for (var index = 0; index < hashCodes.Length; ++index)
                    _values[hashCodes[index]] = source[index].Value;
                ArrayPool<int>.Shared.Return(array);
            }

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in int key)
            {
                _hashTable.FindMatchingEntries(key, out var startIndex, out var endIndex);
                var hashCodes = _hashTable.HashCodes;
                for (; startIndex <= endIndex; ++startIndex)
                {
                    if (key == hashCodes[startIndex])
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Keys
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Keys() => _hashTable.HashCodes;

            /// <summary>
            ///     Values
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashTable.Count;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<int, TValue>.Enumerator GetEnumerator() => new(_hashTable.HashCodes, _values);

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
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

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _hashTable.Dispose();
                _values.Dispose();
            }
        }

        /// <summary>
        ///     Default frozen dictionary
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct DefaultFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Frozen hash table
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Keys
            /// </summary>
            private readonly NativeArray<TKey> _keys;

            /// <summary>
            ///     Values
            /// </summary>
            private readonly NativeArray<TValue> _values;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DefaultFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var keysAreHashCodes = KeysAreHashCodes<TKey>();
                var bucketsByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(source.Length * Unsafe.SizeOf<TKey>()), CACHE_LINE_SIZE);
                var keysPtr = (TKey*)NativeMemoryAllocator.AlignedAlloc((uint)(bucketsByteCount + source.Length * Unsafe.SizeOf<TValue>()), CACHE_LINE_SIZE);
                var valuesPtr = UnsafeHelpers.AddByteOffset<TValue>(keysPtr, (nint)bucketsByteCount);
                _keys = new NativeArray<TKey>(keysPtr, source.Length);
                _values = new NativeArray<TValue>(valuesPtr, source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; ++index)
                    hashCodes[index] = source[index].Key.GetHashCode();
                _hashTable = FrozenHashTable.Create(hashCodes, keysAreHashCodes);
                for (var index1 = 0; index1 < hashCodes.Length; ++index1)
                {
                    var index2 = hashCodes[index1];
                    _keys[index2] = source[index1].Key;
                    _values[index2] = source[index1].Value;
                }

                ArrayPool<int>.Shared.Return(array);
            }

            /// <summary>
            ///     Get value ref or null ref
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                var hashCode = key.GetHashCode();
                var hashCodes = _hashTable.HashCodes;
                for (_hashTable.FindMatchingEntries(hashCode, out var startIndex, out var endIndex); startIndex <= endIndex; ++startIndex)
                {
                    if (hashCode == hashCodes[startIndex] && key.Equals(_keys[startIndex]))
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Keys
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Values
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashTable.Count;
            }

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

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

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _hashTable.Dispose();
                _keys.Dispose();
            }
        }
    }
}