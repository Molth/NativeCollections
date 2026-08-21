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
    internal static unsafe class NativeFrozenSet
    {
        /// <summary>
        ///     Sorts the elements in the entire <see cref="Span{T}" /> using the <see cref="IComparable{T}" /> implementation
        ///     of each element of the <see cref="Span{T}" />
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}" /> to sort.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InsertionSort<T>(Span<T> span)
        {
            var comparer = Comparer<T>.Default;
            for (var i = 0; i < span.Length - 1; ++i)
            {
                var key = span[i + 1];
                int j;
                for (j = i; j >= 0 && comparer.Compare(key, span[j]) < 0; --j)
                    span[j + 1] = span[j];
                span[j + 1] = key;
            }
        }

        /// <summary>
        ///     Searches for the specified item and returns its index in the entries array if found.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(void* ptr, in TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).FindItemIndex(item);

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Items();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Dispose();

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSetHandle<TItem> GetNativeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => new(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Searches for the specified item and returns its index in the entries array if found.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(ref UnsafeFrozenSetValue ptr, in TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).FindItemIndex(item);

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Items();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Dispose();

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSetHandle<TItem> GetUnsafeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => new(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        public interface IFrozenSet<T> : IDisposable where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            int Count { get; }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            int FindItemIndex(in T item);

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            NativeArray<T> Items();

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            NativeFrozenSet<T>.Enumerator GetEnumerator();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct NativeFrozenSetHandle<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            public readonly delegate* managed<void*, in T, int> FindItemIndex;

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<T>> Items;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public readonly delegate* managed<void*, int> Count;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public readonly delegate* managed<void*, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            public readonly delegate* managed<void*, void> Dispose;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSetHandle(delegate* managed<void*, in T, int> findItemIndex, delegate* managed<void*, NativeArray<T>> items, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose)
            {
                FindItemIndex = findItemIndex;
                Items = items;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
            }
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public struct UnsafeFrozenSetHandle<T> : IIsCreated where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => FindItemIndex != null;

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, in T, int> FindItemIndex;

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> Items;

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, int> Count;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, void> Dispose;

            /// <summary>
            ///     Gets the value to the underlying object.
            /// </summary>
            public UnsafeFrozenSetValue Value;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public UnsafeFrozenSetHandle(delegate* managed<ref UnsafeFrozenSetValue, in T, int> findItemIndex, delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> items, delegate* managed<ref UnsafeFrozenSetValue, int> count, delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenSetValue, void> dispose)
            {
                FindItemIndex = findItemIndex;
                Items = items;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
                Value = new UnsafeFrozenSetValue();
            }
        }

        /// <summary>
        ///     Gets the value to the underlying object.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = CACHE_LINE_SIZE)]
        public readonly struct UnsafeFrozenSetValue
        {
            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly EmptyFrozenSet<int> _element0;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly SmallFrozenSet<int> _element1;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly SmallComparableFrozenSet<int> _element2;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly Int32FrozenSet _element3;

            /// <summary>
            ///     Union field.
            /// </summary>
            [FieldOffset(0)] private readonly DefaultFrozenSet<int> _element4;
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item) => -1;

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => NativeArray<T>.Empty;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(NativeArray<T>.Empty);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallFrozenSet(ReadOnlySpan<T> source)
            {
                var items = new NativeArray<T>(source.Length);
                source.CopyTo(items);
                _items = items;
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item) => _items.AsReadOnlySpan().IndexOf(item);

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
                get => _items.Length;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _items.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallComparableFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Gets the maximum value in this.
            /// </summary>
            private T Max => _items[^1];

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallComparableFrozenSet(ReadOnlySpan<T> source)
            {
                var items = new NativeArray<T>(source.Length);
                source.CopyTo(items);
                InsertionSort<T>(items);
                _items = items;
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item)
            {
                if (Comparer<T>.Default.Compare(item, Max) <= 0)
                {
                    var items = _items.AsReadOnlySpan();
                    for (var index = 0; index < items.Length; ++index)
                    {
                        var num = Comparer<T>.Default.Compare(item, items[index]);
                        if (num <= 0)
                        {
                            if (num == 0)
                                return index;
                            break;
                        }
                    }
                }

                return -1;
            }

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
                get => _items.Length;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _items.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Int32FrozenSet : IFrozenSet<int>, IReadOnlyCollection<int>
        {
            /// <summary>
            ///     Represents a frozen hash table that stores hash codes and provides fast lookup for matching entries.
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Int32FrozenSet(ReadOnlySpan<int> source)
            {
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                source.CopyTo(hashCodes);
                _hashTable = FrozenHashTable.Create(hashCodes, true);
                ArrayPool<int>.Shared.Return(array);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in int item)
            {
                _hashTable.FindMatchingEntries(item, out var startIndex, out var endIndex);
                var hashCodes = _hashTable.HashCodes;
                for (; startIndex <= endIndex; ++startIndex)
                {
                    if (item == hashCodes[startIndex])
                        return startIndex;
                }

                return -1;
            }

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Items() => _hashTable.HashCodes;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<int>.Enumerator GetEnumerator() => new(_hashTable.HashCodes);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<int> IEnumerable<int>.GetEnumerator()
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
                get => _hashTable.Count;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _hashTable.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct DefaultFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Represents a frozen hash table that stores hash codes and provides fast lookup for matching entries.
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DefaultFrozenSet(ReadOnlySpan<T> source)
            {
                var keysAreHashCodes = KeysAreHashCodes<T>();
                _items = new NativeArray<T>(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var i = 0; i < source.Length; ++i)
                    hashCodes[i] = source[i].GetHashCode();
                _hashTable = FrozenHashTable.Create(hashCodes, keysAreHashCodes);
                for (var index1 = 0; index1 < hashCodes.Length; ++index1)
                {
                    var index2 = hashCodes[index1];
                    _items[index2] = source[index1];
                }

                ArrayPool<int>.Shared.Return(array);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item)
            {
                var hashCode = item.GetHashCode();
                var hashCodes = _hashTable.HashCodes;
                for (_hashTable.FindMatchingEntries(hashCode, out var startIndex, out var endIndex); startIndex <= endIndex; ++startIndex)
                {
                    if (hashCode == hashCodes[startIndex] && item.Equals(_items[startIndex]))
                        return startIndex;
                }

                return -1;
            }

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
            [Obsolete(SR.parameter_obsolete)]
            [EditorBrowsable(EditorBrowsableState.Never)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
                get => _items.Length;
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _hashTable.Dispose();
                _items.Dispose();
            }
        }
    }
}