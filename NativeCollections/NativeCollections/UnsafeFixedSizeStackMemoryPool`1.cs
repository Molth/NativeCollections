using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe fixed size stack memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeStackMemoryPool<T> : IDisposable where T : unmanaged
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
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSizeStackMemoryPool(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * (sizeof(T) + sizeof(int))));
            _array = (int*)((byte*)_buffer + capacity * sizeof(T));
            _length = capacity;
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
            var size = _size - 1;
            if ((uint)size >= (uint)_length)
                return null;
            _size = size;
            var item = _array[size];
            return &_buffer[item];
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var size = _size;
            _array[size] = (int)(ptr - _buffer);
            _size = size + 1;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSizeStackMemoryPool<T> Empty => new();
    }
}