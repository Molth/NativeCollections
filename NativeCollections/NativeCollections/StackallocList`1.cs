using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc list
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocList<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Array
        /// </summary>
        private T* _array;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Version
        /// </summary>
        private int _version;

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[index];
        }

        /// <summary>
        ///     Get or set value
        /// </summary>
        /// <param name="index">Index</param>
        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[index];
        }

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
        public int Capacity => _length;

        /// <summary>
        ///     Get buffer size
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Buffer size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize(int capacity) => capacity * sizeof(T);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocList(Span<byte> buffer,int capacity)
        {
            _array = (T*)MemoryMarshal.GetReference(buffer);
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _version++;
            _size = 0;
        }

        /// <summary>
        ///     Try add
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Added</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(in T item)
        {
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                _version++;
                _size = size + 1;
                _array[size] = item;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Add range
        /// </summary>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryAddRange(StackallocList<T>* collection)
        {
            var other = collection;
            var count = other->_size;
            if (count > 0)
            {
                if (_length - _size < count)
                    return InsertResult.InsufficientCapacity;
                Unsafe.CopyBlockUnaligned(_array + _size, other->_array, (uint)(other->_size * sizeof(T)));
                _size += count;
                _version++;
            }

            return InsertResult.Success;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsert(int index, in T item)
        {
            if ((uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            if (_size == _length)
                return InsertResult.InsufficientCapacity;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_array + (index + 1), _array + index, (uint)((_size - index) * sizeof(T)));
            _array[index] = item;
            _size++;
            _version++;
            return InsertResult.Success;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InsertResult TryInsertRange(int index, StackallocList<T>* collection)
        {
            if ((uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            var other = collection;
            var count = other->_size;
            if (count > 0)
            {
                if (_length - _size < count)
                    return InsertResult.InsufficientCapacity;
                if (index < _size)
                    Unsafe.CopyBlockUnaligned(_array + index + count, _array + index, (uint)((_size - index) * sizeof(T)));
                if (Unsafe.AsPointer(ref this) == collection)
                {
                    Unsafe.CopyBlockUnaligned(_array + index, _array, (uint)(index * sizeof(T)));
                    Unsafe.CopyBlockUnaligned(_array + index * 2, _array + index + count, (uint)((_size - index) * sizeof(T)));
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(_array + index, other->_array, (uint)(other->_size * sizeof(T)));
                }

                _size += count;
                _version++;
            }

            return InsertResult.Success;
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
                RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Swap remove
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SwapRemove(in T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                SwapRemoveAt(index);
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
            if ((uint)index >= (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            _size--;
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_array + index, _array + (index + 1), (uint)((_size - index) * sizeof(T)));
            _version++;
        }

        /// <summary>
        ///     Swap remove at
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapRemoveAt(int index)
        {
            if ((uint)index >= (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            _size--;
            if (index != _size)
                _array[index] = _array[_size];
            _version++;
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
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            var offset = _size - index;
            if (offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "MustBeLess");
            if (count > 0)
            {
                _size -= count;
                if (index < _size)
                    Unsafe.CopyBlockUnaligned(_array + index, _array + (index + count), (uint)((_size - index) * sizeof(T)));
                _version++;
            }
        }

        /// <summary>
        ///     Reverse
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse()
        {
            if (_size > 1)
                MemoryMarshal.CreateSpan(ref *_array, _size).Reverse();
            _version++;
        }

        /// <summary>
        ///     Reverse
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reverse(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            var offset = _size - index;
            if (offset < count)
                throw new ArgumentOutOfRangeException(nameof(count), "MustBeLess");
            if (count > 1)
                MemoryMarshal.CreateSpan(ref *(_array + index), count).Reverse();
            _version++;
        }

        /// <summary>
        ///     Contains
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Contains</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T item) => _size != 0 && IndexOf(item) >= 0;

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item) => _size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *_array, _size).IndexOf(item);

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index)
        {
            if (_size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            return MemoryMarshal.CreateReadOnlySpan(ref *(_array + index), _size - index).IndexOf(item);
        }

        /// <summary>
        ///     Index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOf(in T item, int index, int count)
        {
            if (_size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            if (index > _size - count)
                throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
            return MemoryMarshal.CreateReadOnlySpan(ref *(_array + index), count).IndexOf(item);
        }

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item) => _size == 0 ? -1 : MemoryMarshal.CreateReadOnlySpan(ref *(_array + (_size - 1)), _size).LastIndexOf(item);

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index)
        {
            if (_size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLess");
            return MemoryMarshal.CreateReadOnlySpan(ref *(_array + index), index + 1).LastIndexOf(item);
        }

        /// <summary>
        ///     Last index of
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="count">Count</param>
        /// <returns>Index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LastIndexOf(in T item, int index, int count)
        {
            if (_size == 0)
                return -1;
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "NeedNonNegNum");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "NeedNonNegNum");
            if (index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "BiggerThanCollection");
            if (count > index + 1)
                throw new ArgumentOutOfRangeException(nameof(count), count, "BiggerThanCollection");
            return MemoryMarshal.CreateReadOnlySpan(ref *(_array + index), count).LastIndexOf(item);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref *_array, _length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(_array + start), _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(_array + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_array, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(in StackallocList<T> stackallocList) => stackallocList.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in StackallocList<T> stackallocList) => stackallocList.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocList<T> Empty => new();

        /// <summary>
        ///     Get enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public Enumerator GetEnumerator() => new(Unsafe.AsPointer(ref this));

        /// <summary>
        ///     Enumerator
        /// </summary>
        public struct Enumerator
        {
            /// <summary>
            ///     NativeList
            /// </summary>
            private readonly StackallocList<T>* _nativeList;

            /// <summary>
            ///     Version
            /// </summary>
            private readonly int _version;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Current
            /// </summary>
            private T _current;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="nativeList">NativeList</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(void* nativeList)
            {
                var handle = (StackallocList<T>*)nativeList;
                _nativeList = handle;
                _index = 0;
                _version = handle->_version;
                _current = default;
            }

            /// <summary>
            ///     Move next
            /// </summary>
            /// <returns>Moved</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var handle = _nativeList;
                if (_version == handle->_version && (uint)_index < (uint)handle->_size)
                {
                    _current = handle->_array[_index];
                    _index++;
                    return true;
                }

                if (_version != handle->_version)
                    throw new InvalidOperationException("EnumFailedVersion");
                _index = handle->_size + 1;
                _current = default;
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