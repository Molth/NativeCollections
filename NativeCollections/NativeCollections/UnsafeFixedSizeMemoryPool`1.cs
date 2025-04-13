using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe fixed size memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeMemoryPool<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly int* _index;

        /// <summary>
        ///     Bit buffer
        /// </summary>
        private readonly int* _bitArray;

        /// <summary>
        ///     Capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Bit buffer length
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
        public int Capacity => _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSizeMemoryPool(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var extremeLength = UnsafeBitArray.GetInt32ArrayLengthFromBitLength(capacity);
            _buffer = (T*)NativeMemoryAllocator.Alloc((uint)(capacity * (sizeof(T) + sizeof(int)) + extremeLength * sizeof(int)));
            _index = (int*)((byte*)_buffer + capacity * sizeof(T));
            _bitArray = (int*)((byte*)_index + capacity * sizeof(int));
            Unsafe.InitBlockUnaligned(_bitArray, 0, (uint)(extremeLength * sizeof(int)));
            _capacity = capacity;
            _bitArrayLength = extremeLength;
            _size = capacity;
            for (var i = 0; i < capacity; ++i)
                _index[i] = i;
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
            _size = _capacity;
            for (var i = 0; i < _capacity; ++i)
                _index[i] = i;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_capacity)
            {
                ptr = null;
                return false;
            }

            _size = size;
            var index = _index[size];
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
            if ((ulong)index >= (ulong)_capacity)
                throw new InvalidOperationException("Mismatch");
            ref var segment = ref _bitArray[index >> 5];
            var bitMask = 1 << (int)index;
            if ((segment & bitMask) == 0)
                throw new InvalidOperationException("Duplicate");
            segment &= ~bitMask;
            _index[_size++] = (int)index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSizeMemoryPool<T> Empty => new();
    }
}