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
    ///     Provides internal structures.
    /// </summary>
    internal static unsafe class NativeFrozenDictionary
    {
        /// <summary>
        ///     Sorts a pair of spans (one containing the keys and the other containing the
        ///     corresponding items) based on the keys in the first <see cref="Span{TKey}" />
        ///     using the <see cref="IComparable{T}" /> implementation of each key.
        /// </summary>
        /// <typeparam name="TKey">The type of the elements of the key span.</typeparam>
        /// <typeparam name="TValue">The type of the elements of the items span.</typeparam>
        /// <param name="keys">The span that contains the keys to sort.</param>
        /// <param name="items">
        ///     The span that contains the items that correspond to the keys in <paramref name="keys" />.
        /// </param>
        private static void InsertionSort<TKey, TValue>(Span<TKey> keys, Span<TValue> items)
        {
            var comparer = Comparer<TKey>.Default;
            for (var i = 0; i < keys.Length.WrappingSub(1); i = i.WrappingAdd(1))
            {
                var key = keys[i.WrappingAdd(1)];
                var value = items[i.WrappingAdd(1)];
                int j;
                for (j = i; j >= 0 && comparer.Compare(key, keys[j]) < 0; j = j.WrappingSub(1))
                {
                    keys[j.WrappingAdd(1)] = keys[j];
                    items[j.WrappingAdd(1)] = items[j];
                }

                keys[j.WrappingAdd(1)] = key;
                items[j.WrappingAdd(1)] = value;
            }
        }

        /// <summary>
        ///     Copies the keys and values from a read-only span of key-value pairs into separate key and value spans.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The read-only span containing the key-value pairs to copy from.</param>
        /// <param name="keys">
        ///     The destination span for the keys. Must have length at least equal to the source length.
        /// </param>
        /// <param name="values">
        ///     The destination span for the values. Must have length at least equal to the source length.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CopyTo<TKey, TValue>(ReadOnlySpan<KeyValuePair<TKey, TValue>> source, Span<TKey> keys, Span<TValue> values)
        {
            for (var i = 0; i < source.Length; i = i.WrappingAdd(1))
            {
                ref readonly var kvp = ref source[i];
                keys[i] = kvp.Key;
                values[i] = kvp.Value;
            }
        }

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly TValue GetValueRefOrNullRef<T, TKey, TValue>(void* ptr, TKey key) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => ref Unsafe.AsRef<T>(ptr).GetValueRefOrNullRef(key);

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        /// <returns>A native array containing the keys in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TKey> Keys<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Keys();

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        /// <returns>A native array containing the values in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TValue> Values<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Values();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        /// <returns>The number of elements in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen dictionary instance.</param>
        private static void Dispose<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Dispose();

        /// <summary>
        ///     Gets a handle that invokes the members of the frozen
        ///     dictionary implementation of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the frozen dictionary implementation.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>
        ///     A handle that invokes the members of the frozen dictionary implementation of <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionaryHandle<TKey, TValue> GetNativeHandle<T, TKey, TValue>() where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => NativeFrozenDictionaryHandle<TKey, TValue>.From(&GetValueRefOrNullRef<T, TKey, TValue>, &Keys<T, TKey, TValue>, &Values<T, TKey, TValue>, &Count<T, TKey, TValue>, &GetEnumerator<T, TKey, TValue>, &Dispose<T, TKey, TValue>);

        /// <summary>
        ///     Gets either a ref to a <typeparamref name="TValue" />
        ///     in this or a ref null if it does not exist in this.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        /// <param name="key">The key used for lookup.</param>
        /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
        /// <remarks>
        ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
        ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly TValue GetValueRefOrNullRef<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr, TKey key) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => ref Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).GetValueRefOrNullRef(key);

        /// <summary>
        ///     Gets a collection containing the keys in the dictionary.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        /// <returns>A native array containing the keys in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TKey> Keys<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Keys();

        /// <summary>
        ///     Gets a collection containing the values in the dictionary.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        /// <returns>A native array containing the values in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TValue> Values<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Values();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        /// <returns>The number of elements in the dictionary.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the frozen dictionary implementation stored in <paramref name="ptr" />.
        /// </typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen dictionary instance.</param>
        private static void Dispose<T, TKey, TValue>(ref UnsafeFrozenDictionaryValue ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.As<UnsafeFrozenDictionaryValue, T>(ref ptr).Dispose();

        /// <summary>
        ///     Gets a handle that invokes the members of the frozen
        ///     dictionary implementation of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the frozen dictionary implementation.</typeparam>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>
        ///     A handle that invokes the members of the frozen dictionary implementation of <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenDictionaryHandle<TKey, TValue> GetUnsafeHandle<T, TKey, TValue>() where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => UnsafeFrozenDictionaryHandle<TKey, TValue>.From(&GetValueRefOrNullRef<T, TKey, TValue>, &Keys<T, TKey, TValue>, &Values<T, TKey, TValue>, &Count<T, TKey, TValue>, &GetEnumerator<T, TKey, TValue>, &Dispose<T, TKey, TValue>);

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        public interface IFrozenDictionary<TKey, TValue> : IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            int Count { get; }

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
            ref readonly TValue GetValueRefOrNullRef(TKey key);

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            NativeArray<TKey> Keys();

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            NativeArray<TValue> Values();

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator();
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct NativeFrozenDictionaryHandle<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </summary>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            public readonly delegate* managed<void*, TKey, ref readonly TValue> GetValueRefOrNullRef;

            /// <summary>
            ///     The function pointer that gets the keys of the dictionary.
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<TKey>> Keys;

            /// <summary>
            ///     The function pointer that gets the values of the dictionary.
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<TValue>> Values;

            /// <summary>
            ///     The function pointer that gets the number of elements in the dictionary.
            /// </summary>
            public readonly delegate* managed<void*, int> Count;

            /// <summary>
            ///     The function pointer that creates an enumerator for the dictionary.
            /// </summary>
            public readonly delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> GetEnumerator;

            /// <summary>
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </summary>
            public readonly delegate* managed<void*, void> Dispose;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="getValueRefOrNullRef">
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </param>
            /// <param name="keys">The function pointer that gets the keys of the dictionary.</param>
            /// <param name="values">The function pointer that gets the values of the dictionary.</param>
            /// <param name="count">The function pointer that gets the number of elements in the dictionary.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the dictionary.</param>
            /// <param name="dispose">
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private NativeFrozenDictionaryHandle(delegate* managed<void*, TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<void*, NativeArray<TKey>> keys, delegate* managed<void*, NativeArray<TValue>> values, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose)
            {
                GetValueRefOrNullRef = getValueRefOrNullRef;
                Keys = keys;
                Values = values;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
            }

            /// <summary>
            ///     Creates a handle from the specified function pointers.
            /// </summary>
            /// <param name="getValueRefOrNullRef">
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </param>
            /// <param name="keys">The function pointer that gets the keys of the dictionary.</param>
            /// <param name="values">The function pointer that gets the values of the dictionary.</param>
            /// <param name="count">The function pointer that gets the number of elements in the dictionary.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the dictionary.</param>
            /// <param name="dispose">
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </param>
            /// <returns>A handle that invokes the specified function pointers.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static NativeFrozenDictionaryHandle<TKey, TValue> From(delegate* managed<void*, TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<void*, NativeArray<TKey>> keys, delegate* managed<void*, NativeArray<TValue>> values, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose) => new(getValueRefOrNullRef, keys, values, count, getEnumerator, dispose);
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public struct UnsafeFrozenDictionaryHandle<TKey, TValue> : IIsCreated where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => GetValueRefOrNullRef != null;

            /// <summary>
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </summary>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, TKey, ref readonly TValue> GetValueRefOrNullRef;

            /// <summary>
            ///     The function pointer that gets the keys of the dictionary.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TKey>> Keys;

            /// <summary>
            ///     The function pointer that gets the values of the dictionary.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TValue>> Values;

            /// <summary>
            ///     The function pointer that gets the number of elements in the dictionary.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, int> Count;

            /// <summary>
            ///     The function pointer that creates an enumerator for the dictionary.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, NativeFrozenDictionary<TKey, TValue>.Enumerator> GetEnumerator;

            /// <summary>
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenDictionaryValue, void> Dispose;

            /// <summary>
            ///     Gets the type-erased storage that the underlying function pointers operate on.
            /// </summary>
            public UnsafeFrozenDictionaryValue Value;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="getValueRefOrNullRef">
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </param>
            /// <param name="keys">The function pointer that gets the keys of the dictionary.</param>
            /// <param name="values">The function pointer that gets the values of the dictionary.</param>
            /// <param name="count">The function pointer that gets the number of elements in the dictionary.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the dictionary.</param>
            /// <param name="dispose">
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private UnsafeFrozenDictionaryHandle(delegate* managed<ref UnsafeFrozenDictionaryValue, TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TKey>> keys, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TValue>> values, delegate* managed<ref UnsafeFrozenDictionaryValue, int> count, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenDictionaryValue, void> dispose)
            {
                GetValueRefOrNullRef = getValueRefOrNullRef;
                Keys = keys;
                Values = values;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
                Value = default;
            }

            /// <summary>
            ///     Creates a handle from the specified function pointers.
            /// </summary>
            /// <param name="getValueRefOrNullRef">
            ///     The function pointer that gets a reference to the value associated with the specified key.
            /// </param>
            /// <param name="keys">The function pointer that gets the keys of the dictionary.</param>
            /// <param name="values">The function pointer that gets the values of the dictionary.</param>
            /// <param name="count">The function pointer that gets the number of elements in the dictionary.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the dictionary.</param>
            /// <param name="dispose">
            ///     The function pointer that releases the unmanaged resources of the dictionary.
            /// </param>
            /// <returns>A handle that invokes the specified function pointers.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static UnsafeFrozenDictionaryHandle<TKey, TValue> From(delegate* managed<ref UnsafeFrozenDictionaryValue, TKey, ref readonly TValue> getValueRefOrNullRef, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TKey>> keys, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeArray<TValue>> values, delegate* managed<ref UnsafeFrozenDictionaryValue, int> count, delegate* managed<ref UnsafeFrozenDictionaryValue, NativeFrozenDictionary<TKey, TValue>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenDictionaryValue, void> dispose) => new(getValueRefOrNullRef, keys, values, count, getEnumerator, dispose);
        }

        /// <summary>
        ///     Represents the type-erased storage of a frozen dictionary implementation.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public readonly struct UnsafeFrozenDictionaryValue
        {
            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly EmptyFrozenDictionary<int, int> _element0;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly SmallFrozenDictionary<int, int> _element1;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly SmallComparableFrozenDictionary<int, int> _element2;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly Int32FrozenDictionary<int> _element3;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly DefaultFrozenDictionary<int, int> _element4;
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Gets either a ref to a <typeparamref name="TValue" />
            ///     in this or a ref null if it does not exist in this.
            /// </summary>
            /// <param name="key">The key to locate in this.</param>
            /// <returns>A null reference, because this dictionary never contains any element.</returns>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(TKey key) => ref Unsafe.NullRef<TValue>();

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => default;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => default;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => NativeFrozenDictionary<TKey, TValue>.Enumerator.From(default, default);

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
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => 0;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="keys">The keys in the dictionary.</param>
            /// <param name="values">The values in the dictionary.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SmallFrozenDictionary(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _keys = keys;
                _values = values;
            }

            /// <summary>
            ///     Creates a small frozen dictionary from key-value pairs.
            /// </summary>
            /// <param name="source">The source key-value pairs.</param>
            /// <returns>A frozen dictionary initialized from <paramref name="source" />.</returns>
            public static SmallFrozenDictionary<TKey, TValue> Create(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<TKey>(), NativeMemoryAllocator.AlignOf<TValue>());
                var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>()), alignment);
                var buckets = (TKey*)NativeMemoryAllocator.AlignedAlloc(bucketsByteCount + Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>()), alignment);
                var entries = UnsafeHelpers.AddByteOffset<TValue>(buckets, Isize.FromUsizeUnchecked(bucketsByteCount));
                var keys = NativeArray<TKey>.From(buckets, source.Length);
                var values = NativeArray<TValue>.From(entries, source.Length);
                CopyTo(source, keys.AsSpan(), values.AsSpan());
                return new SmallFrozenDictionary<TKey, TValue>(keys, values);
            }

            /// <summary>
            ///     Gets either a ref to a <typeparamref name="TValue" />
            ///     in this or a ref null if it does not exist in this.
            /// </summary>
            /// <param name="key">The key to locate in this.</param>
            /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(TKey key)
            {
                var index = _keys.AsReadOnlySpan().IndexOf(key);
                return ref index >= 0 ? ref _values[index] : ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _keys.Length;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => NativeFrozenDictionary<TKey, TValue>.Enumerator.From(_keys, _values);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => _keys.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallComparableFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
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
            ///     Gets the maximum key in this.
            /// </summary>
            private TKey Max => _keys[^1];

            /// <summary>
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="keys">The keys in the dictionary.</param>
            /// <param name="values">The values in the dictionary.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SmallComparableFrozenDictionary(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _keys = keys;
                _values = values;
            }

            /// <summary>
            ///     Creates a small sorted frozen dictionary from key-value pairs.
            /// </summary>
            /// <param name="source">The source key-value pairs.</param>
            /// <returns>A frozen dictionary initialized from <paramref name="source" />.</returns>
            public static SmallComparableFrozenDictionary<TKey, TValue> Create(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>()), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                var keysPtr = (TKey*)NativeMemoryAllocator.AlignedAlloc(bucketsByteCount + Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>()), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                var valuesPtr = UnsafeHelpers.AddByteOffset<TValue>(keysPtr, Isize.FromUsizeUnchecked(bucketsByteCount));
                var keys = NativeArray<TKey>.From(keysPtr, source.Length);
                var values = NativeArray<TValue>.From(valuesPtr, source.Length);
                CopyTo(source, keys.AsSpan(), values.AsSpan());
                InsertionSort(keys.AsSpan(), values.AsSpan());
                return new SmallComparableFrozenDictionary<TKey, TValue>(keys, values);
            }

            /// <summary>
            ///     Gets either a ref to a <typeparamref name="TValue" />
            ///     in this or a ref null if it does not exist in this.
            /// </summary>
            /// <param name="key">The key to locate in this.</param>
            /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(TKey key)
            {
                if (Comparer<TKey>.Default.Compare(key, Max) <= 0)
                {
                    var keys = _keys.AsReadOnlySpan();
                    for (var index = 0; index < keys.Length; index = index.WrappingAdd(1))
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
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _keys.Length;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => NativeFrozenDictionary<TKey, TValue>.Enumerator.From(_keys, _values);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => _keys.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Int32FrozenDictionary<TValue> : IFrozenDictionary<int, TValue>, IReadOnlyCollection<KeyValuePair<int, TValue>> where TValue : unmanaged
        {
            /// <summary>
            ///     Represents a frozen hash table that stores hash codes and provides fast lookup for matching entries.
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            private readonly NativeArray<TValue> _values;

            /// <summary>
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="hashTable">The frozen hash table that stores hash codes.</param>
            /// <param name="values">The values in the dictionary.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Int32FrozenDictionary(FrozenHashTable hashTable, NativeArray<TValue> values)
            {
                _hashTable = hashTable;
                _values = values;
            }

            /// <summary>
            ///     Creates a frozen dictionary for 32-bit integer keys from key-value pairs.
            /// </summary>
            /// <param name="source">The source key-value pairs.</param>
            /// <returns>A frozen dictionary initialized from <paramref name="source" />.</returns>
            public static Int32FrozenDictionary<TValue> Create(ReadOnlySpan<KeyValuePair<int, TValue>> source)
            {
                var values = NativeArray<TValue>.Create(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; index = index.WrappingAdd(1))
                    hashCodes[index] = source[index].Key;

                var hashTable = FrozenHashTable.Create(hashCodes, true);
                for (var index = 0; index < hashCodes.Length; index = index.WrappingAdd(1))
                    values[hashCodes[index]] = source[index].Value;

                ArrayPool<int>.Shared.Return(array);
                return new Int32FrozenDictionary<TValue>(hashTable, values);
            }

            /// <summary>
            ///     Gets either a ref to a <typeparamref name="TValue" />
            ///     in this or a ref null if it does not exist in this.
            /// </summary>
            /// <param name="key">The key to locate in this.</param>
            /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(int key)
            {
                _hashTable.FindMatchingEntries(key, out var startIndex, out var endIndex);
                var hashCodes = _hashTable.HashCodes;
                for (; startIndex <= endIndex; startIndex = startIndex.WrappingAdd(1))
                {
                    if (key == hashCodes[startIndex])
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Keys() => _hashTable.HashCodes;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashTable.Count;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<int, TValue>.Enumerator GetEnumerator() => NativeFrozenDictionary<int, TValue>.Enumerator.From(_hashTable.HashCodes, _values);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _hashTable.Dispose();
                _values.Dispose();
            }
        }

        /// <summary>
        ///     Provides an immutable, read-only dictionary optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct DefaultFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            /// <summary>
            ///     Represents a frozen hash table that stores hash codes and provides fast lookup for matching entries.
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            private readonly NativeArray<TKey> _keys;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            private readonly NativeArray<TValue> _values;

            /// <summary>
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="hashTable">The frozen hash table that stores hash codes.</param>
            /// <param name="keys">The keys in the dictionary.</param>
            /// <param name="values">The values in the dictionary.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private DefaultFrozenDictionary(FrozenHashTable hashTable, NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _hashTable = hashTable;
                _keys = keys;
                _values = values;
            }

            /// <summary>
            ///     Creates a default frozen dictionary from key-value pairs.
            /// </summary>
            /// <param name="source">The source key-value pairs.</param>
            /// <returns>A frozen dictionary initialized from <paramref name="source" />.</returns>
            public static DefaultFrozenDictionary<TKey, TValue> Create(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var keysAreHashCodes = KeysAreHashCodes<TKey>();
                var bucketsByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TKey>()), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                var keysPtr = (TKey*)NativeMemoryAllocator.AlignedAlloc(bucketsByteCount + Usize.FromI32Unchecked(source.Length) * Usize.FromI32Unchecked(Unsafe.SizeOf<TValue>()), Usize.FromI32Unchecked(CACHE_LINE_SIZE));
                var valuesPtr = UnsafeHelpers.AddByteOffset<TValue>(keysPtr, Isize.FromUsizeUnchecked(bucketsByteCount));
                var keys = NativeArray<TKey>.From(keysPtr, source.Length);
                var values = NativeArray<TValue>.From(valuesPtr, source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; index = index.WrappingAdd(1))
                    hashCodes[index] = source[index].Key.GetHashCode();

                var hashTable = FrozenHashTable.Create(hashCodes, keysAreHashCodes);
                for (var index1 = 0; index1 < hashCodes.Length; index1 = index1.WrappingAdd(1))
                {
                    var index2 = hashCodes[index1];
                    keys[index2] = source[index1].Key;
                    values[index2] = source[index1].Value;
                }

                ArrayPool<int>.Shared.Return(array);
                return new DefaultFrozenDictionary<TKey, TValue>(hashTable, keys, values);
            }

            /// <summary>
            ///     Gets either a ref to a <typeparamref name="TValue" />
            ///     in this or a ref null if it does not exist in this.
            /// </summary>
            /// <param name="key">The key to locate in this.</param>
            /// <returns>A reference to the value if the key is found; otherwise, a null reference.</returns>
            /// <remarks>
            ///     Items should not be added or removed from this while the ref <typeparamref name="TValue" />
            ///     is in use. The ref null can be detected using <see cref="Unsafe.IsNullRef" />.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(TKey key)
            {
                var hashCode = key.GetHashCode();
                var hashCodes = _hashTable.HashCodes;
                for (_hashTable.FindMatchingEntries(hashCode, out var startIndex, out var endIndex); startIndex <= endIndex; startIndex = startIndex.WrappingAdd(1))
                {
                    if (hashCode == hashCodes[startIndex] && key.Equals(_keys[startIndex]))
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            /// <summary>
            ///     Gets a collection containing the keys in the dictionary.
            /// </summary>
            /// <returns>A native array containing the keys in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            /// <summary>
            ///     Gets a collection containing the values in the dictionary.
            /// </summary>
            /// <returns>A native array containing the values in the dictionary.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the dictionary and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashTable.Count;
            }

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => NativeFrozenDictionary<TKey, TValue>.Enumerator.From(_keys, _values);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _hashTable.Dispose();
                _keys.Dispose();
            }
        }
    }
}