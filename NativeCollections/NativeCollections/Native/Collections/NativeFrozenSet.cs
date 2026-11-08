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
        ///     Sorts the elements in the entire <see cref="Span{T}" /> using the
        ///     <see cref="IComparable{T}" /> implementation of each element of the <see cref="Span{T}" />.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the span.</typeparam>
        /// <param name="span">The <see cref="Span{T}" /> to sort.</param>
        private static void InsertionSort<T>(Span<T> span)
        {
            var comparer = Comparer<T>.Default;
            for (var i = 0; i < span.Length.WrappingSub(1); i = i.WrappingAdd(1))
            {
                var key = span[i.WrappingAdd(1)];
                int j;
                for (j = i; j >= 0 && comparer.Compare(key, span[j]) < 0; j = j.WrappingSub(1))
                    span[j.WrappingAdd(1)] = span[j];

                span[j.WrappingAdd(1)] = key;
            }
        }

        /// <summary>
        ///     Searches for the specified item and returns its index in the entries array if found.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen set instance.</param>
        /// <param name="item">The item to locate in this.</param>
        /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(void* ptr, TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).FindItemIndex(item);

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen set instance.</param>
        /// <returns>A native array containing the items in the set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Items();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen set instance.</param>
        /// <returns>The number of elements in the set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen set instance.</param>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The pointer to the unmanaged storage that holds the frozen set instance.</param>
        private static void Dispose<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Dispose();

        /// <summary>
        ///     Gets a handle that invokes the members of the frozen set implementation of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <returns>
        ///     A handle that invokes the members of the frozen set implementation of <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSetHandle<TItem> GetNativeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => NativeFrozenSetHandle<TItem>.From(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Searches for the specified item and returns its index in the entries array if found.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen set instance.</param>
        /// <param name="item">The item to locate in this.</param>
        /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(ref UnsafeFrozenSetValue ptr, TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).FindItemIndex(item);

        /// <summary>
        ///     Gets a collection containing the values in this.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen set instance.</param>
        /// <returns>A native array containing the items in the set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Items();

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen set instance.</param>
        /// <returns>The number of elements in the set.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Count;

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen set instance.</param>
        /// <returns>An enumerator that iterates through the collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).GetEnumerator();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation stored in <paramref name="ptr" />.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <param name="ptr">The reference to the storage that holds the frozen set instance.</param>
        private static void Dispose<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Dispose();

        /// <summary>
        ///     Gets a handle that invokes the members of the frozen set implementation of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the frozen set implementation.</typeparam>
        /// <typeparam name="TItem">The type of the elements in the set.</typeparam>
        /// <returns>
        ///     A handle that invokes the members of the frozen set implementation of <typeparamref name="T" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSetHandle<TItem> GetUnsafeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => UnsafeFrozenSetHandle<TItem>.From(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        public interface IFrozenSet<T> : IDisposable where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            int Count { get; }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
            int FindItemIndex(T item);

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            /// <returns>A native array containing the items in the set.</returns>
            NativeArray<T> Items();

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            NativeFrozenSet<T>.Enumerator GetEnumerator();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct NativeFrozenSetHandle<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     The function pointer that finds the index of the specified item.
            /// </summary>
            public readonly delegate* managed<void*, T, int> FindItemIndex;

            /// <summary>
            ///     The function pointer that gets the items of the set.
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<T>> Items;

            /// <summary>
            ///     The function pointer that gets the number of elements in the set.
            /// </summary>
            public readonly delegate* managed<void*, int> Count;

            /// <summary>
            ///     The function pointer that creates an enumerator for the set.
            /// </summary>
            public readonly delegate* managed<void*, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     The function pointer that releases the unmanaged resources of the set.
            /// </summary>
            public readonly delegate* managed<void*, void> Dispose;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="findItemIndex">The function pointer that finds the index of the specified item.</param>
            /// <param name="items">The function pointer that gets the items of the set.</param>
            /// <param name="count">The function pointer that gets the number of elements in the set.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the set.</param>
            /// <param name="dispose">The function pointer that releases the unmanaged resources of the set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private NativeFrozenSetHandle(delegate* managed<void*, T, int> findItemIndex, delegate* managed<void*, NativeArray<T>> items, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose)
            {
                FindItemIndex = findItemIndex;
                Items = items;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
            }

            /// <summary>
            ///     Creates a handle from the specified function pointers.
            /// </summary>
            /// <param name="findItemIndex">The function pointer that finds the index of the specified item.</param>
            /// <param name="items">The function pointer that gets the items of the set.</param>
            /// <param name="count">The function pointer that gets the number of elements in the set.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the set.</param>
            /// <param name="dispose">The function pointer that releases the unmanaged resources of the set.</param>
            /// <returns>A handle that invokes the specified function pointers.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static NativeFrozenSetHandle<T> From(delegate* managed<void*, T, int> findItemIndex, delegate* managed<void*, NativeArray<T>> items, delegate* managed<void*, int> count, delegate* managed<void*, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<void*, void> dispose) => new(findItemIndex, items, count, getEnumerator, dispose);
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public struct UnsafeFrozenSetHandle<T> : IIsCreated where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => FindItemIndex != null;

            /// <summary>
            ///     The function pointer that finds the index of the specified item.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, T, int> FindItemIndex;

            /// <summary>
            ///     The function pointer that gets the items of the set.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> Items;

            /// <summary>
            ///     The function pointer that gets the number of elements in the set.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, int> Count;

            /// <summary>
            ///     The function pointer that creates an enumerator for the set.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     The function pointer that releases the unmanaged resources of the set.
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, void> Dispose;

            /// <summary>
            ///     Gets the type-erased storage that the underlying function pointers operate on.
            /// </summary>
            public UnsafeFrozenSetValue Value;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="findItemIndex">The function pointer that finds the index of the specified item.</param>
            /// <param name="items">The function pointer that gets the items of the set.</param>
            /// <param name="count">The function pointer that gets the number of elements in the set.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the set.</param>
            /// <param name="dispose">The function pointer that releases the unmanaged resources of the set.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private UnsafeFrozenSetHandle(delegate* managed<ref UnsafeFrozenSetValue, T, int> findItemIndex, delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> items, delegate* managed<ref UnsafeFrozenSetValue, int> count, delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenSetValue, void> dispose)
            {
                FindItemIndex = findItemIndex;
                Items = items;
                Count = count;
                GetEnumerator = getEnumerator;
                Dispose = dispose;
                Value = default;
            }

            /// <summary>
            ///     Creates a handle from the specified function pointers.
            /// </summary>
            /// <param name="findItemIndex">The function pointer that finds the index of the specified item.</param>
            /// <param name="items">The function pointer that gets the items of the set.</param>
            /// <param name="count">The function pointer that gets the number of elements in the set.</param>
            /// <param name="getEnumerator">The function pointer that creates an enumerator for the set.</param>
            /// <param name="dispose">The function pointer that releases the unmanaged resources of the set.</param>
            /// <returns>A handle that invokes the specified function pointers.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static UnsafeFrozenSetHandle<T> From(delegate* managed<ref UnsafeFrozenSetValue, T, int> findItemIndex, delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> items, delegate* managed<ref UnsafeFrozenSetValue, int> count, delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> getEnumerator, delegate* managed<ref UnsafeFrozenSetValue, void> dispose) => new(findItemIndex, items, count, getEnumerator, dispose);
        }

        /// <summary>
        ///     Represents the type-erased storage of a frozen set implementation.
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
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>-1, because this set never contains any element.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(T item) => -1;

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            /// <returns>An empty native array.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => default;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => NativeFrozenSet<T>.Enumerator.From(default);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenSet<T> : IFrozenSet<T>, IReadOnlyCollection<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="items">The underlying elements.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SmallFrozenSet(NativeArray<T> items) => _items = items;

            /// <summary>
            ///     Creates a small frozen set from items.
            /// </summary>
            /// <param name="source">The source items.</param>
            /// <returns>A frozen set initialized from <paramref name="source" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SmallFrozenSet<T> Create(ReadOnlySpan<T> source)
            {
                var items = NativeArray<T>.Create(source.Length);
                source.CopyTo(items.AsSpan());
                return new SmallFrozenSet<T>(items);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(T item) => _items.AsReadOnlySpan().IndexOf(item);

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            /// <returns>A native array containing the items in the set.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the set and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => NativeFrozenSet<T>.Enumerator.From(_items);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => _items.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
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
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="items">The underlying elements.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SmallComparableFrozenSet(NativeArray<T> items) => _items = items;

            /// <summary>
            ///     Creates a small sorted frozen set from items.
            /// </summary>
            /// <param name="source">The source items.</param>
            /// <returns>A frozen set initialized from <paramref name="source" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static SmallComparableFrozenSet<T> Create(ReadOnlySpan<T> source)
            {
                var items = NativeArray<T>.Create(source.Length);
                source.CopyTo(items.AsSpan());
                InsertionSort(items.AsSpan());
                return new SmallComparableFrozenSet<T>(items);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(T item)
            {
                if (Comparer<T>.Default.Compare(item, Max) <= 0)
                {
                    var items = _items.AsReadOnlySpan();
                    for (var index = 0; index < items.Length; index = index.WrappingAdd(1))
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
            /// <returns>A native array containing the items in the set.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the set and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => NativeFrozenSet<T>.Enumerator.From(_items);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
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
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="hashTable">The frozen hash table that stores hash codes.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Int32FrozenSet(FrozenHashTable hashTable) => _hashTable = hashTable;

            /// <summary>
            ///     Creates a frozen set for 32-bit integer items.
            /// </summary>
            /// <param name="source">The source items.</param>
            /// <returns>A frozen set initialized from <paramref name="source" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Int32FrozenSet Create(ReadOnlySpan<int> source)
            {
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                source.CopyTo(hashCodes);
                var hashTable = FrozenHashTable.Create(hashCodes, true);
                ArrayPool<int>.Shared.Return(array);
                return new Int32FrozenSet(hashTable);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(int item)
            {
                _hashTable.FindMatchingEntries(item, out var startIndex, out var endIndex);
                var hashCodes = _hashTable.HashCodes;
                for (; startIndex <= endIndex; startIndex = startIndex.WrappingAdd(1))
                {
                    if (item == hashCodes[startIndex])
                        return startIndex;
                }

                return -1;
            }

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            /// <returns>A native array containing the items in the set.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the set and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Items() => _hashTable.HashCodes;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<int>.Enumerator GetEnumerator() => NativeFrozenSet<int>.Enumerator.From(_hashTable.HashCodes);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => _hashTable.Dispose();
        }

        /// <summary>
        ///     Provides an immutable, read-only set optimized for fast lookup and enumeration.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the set.</typeparam>
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
            ///     Initializes a new instance of this class with the specified state.
            /// </summary>
            /// <param name="hashTable">The frozen hash table that stores hash codes.</param>
            /// <param name="items">The underlying elements.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private DefaultFrozenSet(FrozenHashTable hashTable, NativeArray<T> items)
            {
                _hashTable = hashTable;
                _items = items;
            }

            /// <summary>
            ///     Creates a default frozen set from items.
            /// </summary>
            /// <param name="source">The source items.</param>
            /// <returns>A frozen set initialized from <paramref name="source" />.</returns>
            public static DefaultFrozenSet<T> Create(ReadOnlySpan<T> source)
            {
                var keysAreHashCodes = KeysAreHashCodes<T>();
                var items = NativeArray<T>.Create(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var i = 0; i < source.Length; i = i.WrappingAdd(1))
                    hashCodes[i] = source[i].GetHashCode();

                var hashTable = FrozenHashTable.Create(hashCodes, keysAreHashCodes);
                for (var index1 = 0; index1 < hashCodes.Length; index1 = index1.WrappingAdd(1))
                {
                    var index2 = hashCodes[index1];
                    items[index2] = source[index1];
                }

                ArrayPool<int>.Shared.Return(array);
                return new DefaultFrozenSet<T>(hashTable, items);
            }

            /// <summary>
            ///     Searches for the specified item and returns its index in the entries array if found.
            /// </summary>
            /// <param name="item">The item to locate in this.</param>
            /// <returns>The zero-based index of the specified item if found; otherwise, -1.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(T item)
            {
                var hashCode = item.GetHashCode();
                var hashCodes = _hashTable.HashCodes;
                for (_hashTable.FindMatchingEntries(hashCode, out var startIndex, out var endIndex); startIndex <= endIndex; startIndex = startIndex.WrappingAdd(1))
                {
                    if (hashCode == hashCodes[startIndex] && item.Equals(_items[startIndex]))
                        return startIndex;
                }

                return -1;
            }

            /// <summary>
            ///     Gets the underlying elements.
            /// </summary>
            /// <returns>A native array containing the items in the set.</returns>
            /// <remarks>
            ///     The returned array refers to storage owned by the set and shares its lifetime.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>An enumerator that iterates through the collection.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => NativeFrozenSet<T>.Enumerator.From(_items);

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
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _hashTable.Dispose();
                _items.Dispose();
            }
        }
    }
}