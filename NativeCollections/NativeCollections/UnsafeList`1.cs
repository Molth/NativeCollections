﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe list
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeList<T> : IDisposable where T : unmanaged, IEquatable<T>
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
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _array = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
            _length = capacity;
            _size = 0;
            _version = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_array);

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
        ///     Add
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(in T item)
        {
            _version++;
            var size = _size;
            if ((uint)size < (uint)_length)
            {
                _size = size + 1;
                _array[size] = item;
            }
            else
            {
                Grow(size + 1);
                _size = size + 1;
                _array[size] = item;
            }
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
        public void AddRange(UnsafeList<T>* collection)
        {
            var other = collection;
            var count = other->_size;
            if (count > 0)
            {
                if (_length - _size < count)
                    Grow(checked(_size + count));
                Unsafe.CopyBlockUnaligned(_array + _size, other->_array, (uint)(other->_size * sizeof(T)));
                _size += count;
                _version++;
            }
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, in T item)
        {
            if ((uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            if (_size == _length)
                Grow(_size + 1);
            if (index < _size)
                Unsafe.CopyBlockUnaligned(_array + (index + 1), _array + index, (uint)((_size - index) * sizeof(T)));
            _array[index] = item;
            _size++;
            _version++;
        }

        /// <summary>
        ///     Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="collection">Collection</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertRange(int index, UnsafeList<T>* collection)
        {
            if ((uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException(nameof(index), index, "IndexMustBeLessOrEqual");
            var other = collection;
            var count = other->_size;
            if (count > 0)
            {
                if (_length - _size < count)
                    Grow(checked(_size + count));
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
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (_length < capacity)
                Grow(capacity);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            var threshold = (int)(_length * 0.9);
            if (_size < threshold)
                SetCapacity(_size);
            return _length;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < _size || capacity >= _length)
                return _length;
            SetCapacity(_size);
            return _length;
        }

        /// <summary>
        ///     Grow
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Grow(int capacity)
        {
            var newCapacity = 2 * _length;
            if ((uint)newCapacity > 2147483591)
                newCapacity = 2147483591;
            var expected = _length + 4;
            newCapacity = newCapacity > expected ? newCapacity : expected;
            if (newCapacity < capacity)
                newCapacity = capacity;
            SetCapacity(newCapacity);
        }

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
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            if (capacity < _size)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "SmallCapacity");
            if (capacity != _length)
            {
                if (capacity > 0)
                {
                    var newItems = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * sizeof(T)));
                    if (_size > 0)
                        Unsafe.CopyBlockUnaligned(newItems, _array, (uint)(_size * sizeof(T)));
                    NativeMemoryAllocator.Free(_array);
                    _array = newItems;
                    _length = capacity;
                }
                else
                {
                    NativeMemoryAllocator.Free(_array);
                    _array = (T*)NativeMemoryAllocator.Alloc(0);
                    _length = 0;
                }
            }
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
        public static implicit operator Span<T>(in UnsafeList<T> unsafeList) => unsafeList.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(in UnsafeList<T> unsafeList) => unsafeList.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeList<T> Empty => new();

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
            private readonly UnsafeList<T>* _nativeList;

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
                var handle = (UnsafeList<T>*)nativeList;
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