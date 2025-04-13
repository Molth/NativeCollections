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
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Buffer
        /// </summary>
        private void** _buffer;

        /// <summary>
        ///     Index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Memory allocator
        /// </summary>
        private CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _size;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="allocator">Memory allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int size, int length, CustomMemoryAllocator allocator)
        {
            _size = size;
            _length = length;
            _buffer = (void**)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(void*)));
            _index = 0;
            _allocator = allocator;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            for (var i = _size - 1; i >= 0; --i)
            {
                var buffer = _buffer[i];
                if (buffer == null)
                    break;
                _allocator.Free(buffer);
            }

            NativeMemoryAllocator.Free(_buffer);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* buffer = null;
            if (_index < _size)
            {
                buffer = _buffer[_index];
                _buffer[_index++] = null;
            }

            if (buffer == null)
                buffer = _allocator.Alloc((uint)_length);
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
                _buffer[--_index] = ptr;
            else
                _allocator.Free(ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryBucket Empty => new();
    }
}