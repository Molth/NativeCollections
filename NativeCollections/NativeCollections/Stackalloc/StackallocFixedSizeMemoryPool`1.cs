using System;
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
    public unsafe struct StackallocFixedSizeMemoryPool<T> where T : unmanaged
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
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Get byte count
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Byte count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            var extremeLength = UnsafeBitArray.GetInt32ArrayLengthFromBitLength(capacity);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * sizeof(T)), alignment);
            return (int)(bufferByteCount + (capacity + extremeLength) * sizeof(int) + alignment - 1);
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
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * sizeof(T)), alignment);
            _buffer = (T*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _index = UnsafeHelpers.AddByteOffset<int>(_buffer, (nint)bufferByteCount);
            _bitArray = UnsafeHelpers.AddByteOffset<int>(_index, capacity * sizeof(int));
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_bitArray), 0, (uint)(extremeLength * sizeof(int)));
            _capacity = capacity;
            _bitArrayLength = extremeLength;
            _size = capacity;
            for (var i = 0; i < capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(_bitArray), 0, (uint)(_bitArrayLength * sizeof(int)));
            _size = _capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
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
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)size);
            ref var segment = ref Unsafe.Add(ref Unsafe.AsRef<int>(_bitArray), (nint)(index >> 5));
            var bitMask = 1 << index;
            segment |= bitMask;
            ptr = (T*)Unsafe.AsPointer(ref Unsafe.Add(ref Unsafe.AsRef<T>(_buffer), (nint)index));
            return true;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var (index, remainder) = MathHelpers.DivRem(byteOffset, sizeof(T));
            if ((ulong)index >= (ulong)_capacity || remainder != 0)
                ThrowHelpers.ThrowMismatchException();
            ref var segment = ref Unsafe.Add(ref Unsafe.AsRef<int>(_bitArray), index >> 5);
            var bitMask = 1 << (int)index;
            if ((segment & bitMask) == 0)
                ThrowHelpers.ThrowDuplicateException();
            segment &= ~bitMask;
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)_size++) = (int)index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocFixedSizeMemoryPool<T> Empty => new();
    }
}