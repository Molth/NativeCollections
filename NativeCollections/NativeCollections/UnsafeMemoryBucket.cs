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
    [UnsafeCollection(NativeCollectionType.None)]
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
        ///     Array
        /// </summary>
        private void** _array;

        /// <summary>
        ///     Index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Memory pool
        /// </summary>
        private NativeMemoryPool _memoryPool;

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
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int size, int length, int maxFreeSlabs)
        {
            var memoryPool = new NativeMemoryPool(size, length, maxFreeSlabs);
            _size = size;
            _length = length;
            _array = (void**)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(void*)));
            _index = 0;
            _memoryPool = memoryPool;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            NativeMemoryAllocator.Free(_array);
            _memoryPool.Dispose();
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
                buffer = _array[_index];
                _array[_index++] = null;
            }

            if (buffer == null)
                buffer = _memoryPool.Rent();
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
                _array[--_index] = ptr;
            else
                _memoryPool.Return(ptr);
        }
    }
}