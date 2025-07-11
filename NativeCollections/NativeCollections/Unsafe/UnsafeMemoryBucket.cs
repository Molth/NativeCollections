using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory bucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryBucket : IDisposable
    {
        /// <summary>
        ///     Capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Buffer
        /// </summary>
        private nint* _buffer;

        /// <summary>
        ///     Index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Memory allocator
        /// </summary>
        private CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length) : this(capacity, length, CustomMemoryAllocator.Default)
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        /// <param name="allocator">Memory allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length, CustomMemoryAllocator allocator)
        {
            _capacity = capacity;
            _length = length;
            _buffer = NativeMemoryAllocator.AlignedAllocZeroed<nint>((uint)capacity);
            _index = 0;
            _allocator = allocator;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            for (var i = _capacity - 1; i >= 0; --i)
            {
                var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)i);
                if (buffer == null)
                    break;
                _allocator.AlignedFree(buffer);
            }

            NativeMemoryAllocator.AlignedFree(_buffer);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* buffer = null;
            if (_index < _capacity)
            {
                buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index);
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index++) = 0;
            }

            if (buffer == null)
                buffer = _allocator.AlignedAlloc<byte>((uint)_length);
            return buffer;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (_index != 0)
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)(--_index)) = (nint)ptr;
            else
                _allocator.AlignedFree(ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryBucket Empty => new();
    }
}