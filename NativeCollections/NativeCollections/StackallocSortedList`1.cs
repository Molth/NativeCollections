using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc sortedList
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocSortedList<T> : IReadOnlyCollection<T> where T : unmanaged, IComparable<T>
    {
        /// <summary>
        ///     Items
        /// </summary>
        private T* _items;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity) => capacity * sizeof(T);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocSortedList(Span<byte> buffer, int capacity)
        {
            _items = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _size = 0;
            _version = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            ++_version;
            _size = 0;
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item)
        {
            var num = BinarySearchHelpers.IndexOf(_items, _size, item);
            return num >= 0 ? num : -1;
        }

        /// <summary>
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAdd(in T item)
        {
            var num = IndexOf(item);
            return num >= 0 ? InsertResult.AlreadyExists : Insert(~num, item);
        }

        /// <summary>
        ///     Remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                --_size;
                if (index < _size)
                    Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
                ++_version;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            --_size;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, out T item)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            item = _items[index];
            --_size;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
            ++_version;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                return false;
            --_size;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemoveAt(int index, out T item)
        {
            if ((uint)index >= (uint)_size)
            {
                item = default;
                return false;
            }

            item = _items[index];
            --_size;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((_size - index) * sizeof(T)));
            ++_version;
            return true;
        }

        /// <summary>
        ///     Remove range
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeNonNegative");
            if (count == 0)
                return;
            if (index + count > _size)
                throw new ArgumentOutOfRangeException(nameof(count), count, "MustBeLess");
            _size -= count;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index, _items + index + count, (uint)((_size - index) * sizeof(T)));
            ++_version;
        }

        /// <summary>
        ///     Get item at index
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "MustBeNonNegative");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return _items[index];
        }

        /// <summary>
        ///     Contains item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains item</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => IndexOf(item) >= 0;

        /// <summary>
        ///     Get at
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        /// <returns>Got</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetAt(int index, out T item)
        {
            if ((uint)index >= (uint)_size)
            {
                item = default;
                return false;
            }

            item = _items[index];
            return true;
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_items, _size);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_items + start), _size - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_items + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in StackallocSortedList<T> stackallocSortedList) => stackallocSortedList.AsReadOnlySpan();

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InsertResult Insert(int index, in T item)
        {
            if (_size == _capacity)
                return InsertResult.InsufficientCapacity;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_items + index + 1, _items + index, (uint)((_size - index) * sizeof(T)));
            _items[index] = item;
            ++_size;
            ++_version;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocSortedList<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Get enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("CannotCallGetEnumerator");

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeSortedList
            /// </summary>
            private readonly StackallocSortedList<T>* _nativeSortedList;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeSortedList">NativeSortedList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeSortedList)
            {
                var handle = (StackallocSortedList<T>*)nativeSortedList;
                _nativeSortedList = handle;
                _current = default;
                _index = 0;
                _version = handle->_version;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeSortedList;
                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                if ((uint)_index < (uint)handle->_size)
                {
                    _current = handle->_items[_index];
                    ++_index;
                    return true;
                }

                _index = handle->_size + 1;
                return false;
            }

            /// <summary>
            ///     Current
            /// </summary>
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }
        }
    }
}