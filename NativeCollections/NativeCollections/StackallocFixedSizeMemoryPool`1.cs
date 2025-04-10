﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc fixed size memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocFixedSizeMemoryPool<T> : IDisposable where T : unmanaged
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
        ///     Bit array
        /// </summary>
        private readonly int* _bitArray;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Bit array length
        /// </summary>
        private readonly int _bitArrayLength;

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
        public int Capacity => _length;

        /// <summary>
        ///     Get buffer size
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Buffer size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize(int capacity)
        {
            var extremeLength = UnsafeBitArray.GetInt32ArrayLengthFromBitLength(capacity);
            return capacity * (sizeof(T) + sizeof(int)) + extremeLength * sizeof(int);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocFixedSizeMemoryPool(Span<byte> buffer, int capacity)
        {
            var extremeLength = UnsafeBitArray.GetInt32ArrayLengthFromBitLength(capacity);
            _buffer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _array = (int*)((byte*)_buffer + capacity * sizeof(T));
            _bitArray = (int*)((byte*)_array + capacity * sizeof(int));
            Unsafe.InitBlockUnaligned(_bitArray, 0, (uint)(extremeLength * sizeof(int)));
            _length = capacity;
            _bitArrayLength = extremeLength;
            _size = capacity;
            for (var i = 0; i < capacity; ++i)
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
            Unsafe.InitBlockUnaligned(_bitArray, 0, (uint)(_bitArrayLength * sizeof(int)));
            _size = _length;
            for (var i = 0; i < _length; ++i)
                _array[i] = i;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
            {
                ptr = null;
                return false;
            }

            _size = size;
            var index = _array[size];
            ref var segment = ref _bitArray[index >> 5];
            var bitMask = 1 << index;
            segment |= bitMask;
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
            var index = ptr - _buffer;
            if ((ulong)index >= (ulong)_length)
                throw new InvalidOperationException("Mismatch");
            ref var segment = ref _bitArray[index >> 5];
            var bitMask = 1 << (int)index;
            if ((segment & bitMask) == 0)
                throw new InvalidOperationException("Duplicate");
            segment &= ~bitMask;
            _array[_size++] = (int)index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocFixedSizeMemoryPool<T> Empty => new();
    }
}