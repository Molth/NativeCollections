﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc fixed size queue memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocFixedSizeQueueMemoryPool<T> where T : unmanaged
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
        ///     Capacity
        /// </summary>
        private readonly int _capacity;

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
        ///     Capacity
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Get buffer size
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Buffer size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize(int capacity) => capacity * (sizeof(T) + sizeof(int));

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocFixedSizeQueueMemoryPool(Span<byte> buffer, int capacity)
        {
            _buffer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _array = (int*)((byte*)_buffer + capacity * sizeof(T));
            _capacity = capacity;
            _head = 0;
            _tail = 0;
            _size = capacity;
            for (var i = 0; i < _capacity; ++i)
                _array[i] = i;
        }

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _size = _capacity;
            for (var i = 0; i < _capacity; ++i)
                _array[i] = i;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            if (_size == 0)
            {
                ptr = null;
                return false;
            }

            var index = _array[_head];
            MoveNext(ref _head);
            _size--;
            ptr = &_buffer[index];
            return true;
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
            if (tmp == _capacity)
                tmp = 0;
            index = tmp;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocFixedSizeQueueMemoryPool<T> Empty => new();
    }
}