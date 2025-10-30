using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.FrozenHelpers;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native frozen set
    /// </summary>
    internal static unsafe class NativeFrozenSet
    {
        /// <summary>
        ///     Find item index
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(void* ptr, in TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).FindItemIndex(item);

        /// <summary>
        ///     Items
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Items();

        /// <summary>
        ///     Count
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Count();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).GetEnumerator();

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TItem>(void* ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.AsRef<T>(ptr).Dispose();

        /// <summary>
        ///     Get handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenSetHandle<TItem> GetNativeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => new(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Find item index
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FindItemIndex<T, TItem>(ref UnsafeFrozenSetValue ptr, in TItem item) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).FindItemIndex(item);

        /// <summary>
        ///     Items
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TItem> Items<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Items();

        /// <summary>
        ///     Count
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Count();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenSet<TItem>.Enumerator GetEnumerator<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).GetEnumerator();

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TItem>(ref UnsafeFrozenSetValue ptr) where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => Unsafe.As<UnsafeFrozenSetValue, T>(ref ptr).Dispose();

        /// <summary>
        ///     Get handle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFrozenSetHandle<TItem> GetUnsafeHandle<T, TItem>() where T : unmanaged, IFrozenSet<TItem> where TItem : unmanaged, IEquatable<TItem> => new(&FindItemIndex<T, TItem>, &Items<T, TItem>, &Count<T, TItem>, &GetEnumerator<T, TItem>, &Dispose<T, TItem>);

        /// <summary>
        ///     Frozen set
        /// </summary>
        public interface IFrozenSet<T> : IDisposable where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Find item index
            /// </summary>
            int FindItemIndex(in T item);

            /// <summary>
            ///     Items
            /// </summary>
            NativeArray<T> Items();

            /// <summary>
            ///     Get enumerator
            /// </summary>
            NativeFrozenSet<T>.Enumerator GetEnumerator();

            /// <summary>
            ///     Count
            /// </summary>
            int Count();
        }

        /// <summary>
        ///     Unsafe frozen set handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct UnsafeFrozenSetHandle<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Find item index
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, in T, int> FindItemIndex;

            /// <summary>
            ///     Items
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeArray<T>> Items;

            /// <summary>
            ///     Count
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, int> Count;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Dispose
            /// </summary>
            public readonly delegate* managed<ref UnsafeFrozenSetValue, void> Dispose;

            /// <summary>
            ///     Value
            /// </summary>
            public UnsafeFrozenSetValue Value;

            /// <summary>
            ///     Structure
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
        ///     Value
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 128)]
        public struct UnsafeFrozenSetValue
        {
            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private EmptyFrozenSet<int> _element0;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private SmallFrozenSet<int> _element1;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private SmallComparableFrozenSet<int> _element2;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private Int32FrozenSet _element3;

            /// <summary>
            ///     Element
            /// </summary>
            [FieldOffset(0)] private DefaultFrozenSet<int> _element4;
        }

        /// <summary>
        ///     Native frozen set handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct NativeFrozenSetHandle<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Find item index
            /// </summary>
            public readonly delegate* managed<void*, in T, int> FindItemIndex;

            /// <summary>
            ///     Items
            /// </summary>
            public readonly delegate* managed<void*, NativeArray<T>> Items;

            /// <summary>
            ///     Count
            /// </summary>
            public readonly delegate* managed<void*, int> Count;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            public readonly delegate* managed<void*, NativeFrozenSet<T>.Enumerator> GetEnumerator;

            /// <summary>
            ///     Dispose
            /// </summary>
            public readonly delegate* managed<void*, void> Dispose;

            /// <summary>
            ///     Structure
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
        ///     Empty frozen set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenSet<T> : IFrozenSet<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Find item index
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item) => -1;

            /// <summary>
            ///     Items
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => NativeArray<T>.Empty;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(NativeArray<T>.Empty);

            /// <summary>
            ///     Count
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => 0;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }
        }

        /// <summary>
        ///     Small frozen set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenSet<T> : IFrozenSet<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Items
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallFrozenSet(NativeArray<T> items) => _items = items;

            /// <summary>
            ///     Find item index
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item) => _items.AsReadOnlySpan().IndexOf(item);

            /// <summary>
            ///     Items
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Count
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _items.Length;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _items.Dispose();
        }

        /// <summary>
        ///     Small comparable frozen set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallComparableFrozenSet<T> : IFrozenSet<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Items
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Max
            /// </summary>
            private readonly T _max;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallComparableFrozenSet(NativeArray<T> items)
            {
                _items = items;
                _max = items[^1];
            }

            /// <summary>
            ///     Find item index
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int FindItemIndex(in T item)
            {
                if (Comparer<T>.Default.Compare(item, _max) <= 0)
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
            ///     Items
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Count
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _items.Length;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _items.Dispose();
        }

        /// <summary>
        ///     Int32 frozen set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Int32FrozenSet : IFrozenSet<int>
        {
            /// <summary>
            ///     Frozen hash table
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Structure
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Int32FrozenSet(Span<int> source) => _hashTable = FrozenHashTable.Create(source, true);

            /// <summary>
            ///     Find item index
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
            ///     Items
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Items() => _hashTable.HashCodes;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<int>.Enumerator GetEnumerator() => new(_hashTable.HashCodes);

            /// <summary>
            ///     Count
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _hashTable.Count;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _hashTable.Dispose();
        }

        /// <summary>
        ///     Default frozen set
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct DefaultFrozenSet<T> : IFrozenSet<T> where T : unmanaged, IEquatable<T>
        {
            /// <summary>
            ///     Frozen hash table
            /// </summary>
            private readonly FrozenHashTable _hashTable;

            /// <summary>
            ///     Items
            /// </summary>
            private readonly NativeArray<T> _items;

            /// <summary>
            ///     Structure
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
            ///     Find item index
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
            ///     Items
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Items() => _items;

            /// <summary>
            ///     Get enumerator
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenSet<T>.Enumerator GetEnumerator() => new(_items);

            /// <summary>
            ///     Count
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _items.Length;

            /// <summary>
            ///     Dispose
            /// </summary>
            public void Dispose()
            {
                _hashTable.Dispose();
                _items.Dispose();
            }
        }
    }
}