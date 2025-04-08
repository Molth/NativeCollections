using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe fixed size queue memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeQueueMemoryPool<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Array
        /// </summary>
        private readonly int* _array;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Head
        /// </summary>
        private int _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private int _tail;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSizeQueueMemoryPool(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * (sizeof(T) + sizeof(int))));
            _array = (int*)((byte*)_buffer + capacity * sizeof(T));
            _length = capacity;
            _head = 0;
            _tail = 0;
            _size = capacity;
            for (var i = 0; i < _length; ++i)
                _array[i] = i;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => NativeMemoryAllocator.Free(_buffer);

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _size = _length;
            for (var i = 0; i < _length; ++i)
                _array[i] = i;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent()
        {
            if (_size == 0)
                return null;
            var removed = _array[_head];
            MoveNext(ref _head);
            _size--;
            return &_buffer[removed];
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            _array[_tail] = (int)(ptr - _buffer);
            MoveNext(ref _tail);
            _size++;
        }

        /// <summary>
        ///     Move next
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveNext(ref int index)
        {
            var tmp = index + 1;
            if (tmp == _length)
                tmp = 0;
            index = tmp;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSizeQueueMemoryPool<T> Empty => new();
    }
}