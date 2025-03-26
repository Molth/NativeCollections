using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe fixed size 64 memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSize64MemoryPool : IDisposable
    {
        /// <summary>
        ///     Sentinel
        /// </summary>
        private MemorySlab* _sentinel;

        /// <summary>
        ///     Free list
        /// </summary>
        private MemorySlab* _freeList;

        /// <summary>
        ///     Slabs
        /// </summary>
        private int _slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        private int _freeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        private int _maxFreeSlabs;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Slabs
        /// </summary>
        public int Slabs => _slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        public int FreeSlabs => _freeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        public int MaxFreeSlabs => _maxFreeSlabs;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSize64MemoryPool(int length, int maxFreeSlabs)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            var nodeSize = 1 + length;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + 64 * nodeSize));
            var slab = (MemorySlab*)array;
            slab->Next = slab;
            slab->Previous = slab;
            slab->Bitmap = 0UL;
            array += sizeof(MemorySlab);
            for (byte i = 0; i < 64; ++i)
                *(array + i * nodeSize) = i;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _length = length;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var node = _sentinel;
            while (_slabs > 0)
            {
                _slabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            node = _freeList;
            while (_freeSlabs > 0)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            var slab = _sentinel;
            var nodeSize = 1 + _length;
            if (slab->Bitmap == ulong.MaxValue)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Bitmap == ulong.MaxValue)
                {
                    if (_freeSlabs == 0)
                    {
                        var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + 64 * nodeSize));
                        slab = (MemorySlab*)array;
                        array += sizeof(MemorySlab);
                        for (byte i = 0; i < 64; ++i)
                            *(array + i * nodeSize) = i;
                    }
                    else
                    {
                        slab = _freeList;
                        _freeList = slab->Next;
                        _freeSlabs--;
                    }

                    slab->Next = _sentinel;
                    slab->Previous = _sentinel->Previous;
                    slab->Bitmap = 0UL;
                    _sentinel->Previous->Next = slab;
                    _sentinel->Previous = slab;
                    _sentinel = slab;
                    _slabs++;
                }
            }

            ref var segment = ref slab->Bitmap;
            var id = BitOperationsHelpers.TrailingZeroCount(~segment);
            segment |= 1UL << id;
            return (byte*)slab + sizeof(MemorySlab) + id * nodeSize + 1;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var array = (byte*)ptr;
            var id = *(array - 1);
            array -= sizeof(MemorySlab) + id * (1 + _length) + 1;
            var slab = (MemorySlab*)array;
            ref var segment = ref slab->Bitmap;
            segment &= ~(1UL << id);
            if (segment == 0 && slab != _sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (_freeSlabs == _maxFreeSlabs)
                {
                    NativeMemoryAllocator.Free(slab);
                }
                else
                {
                    slab->Next = _freeList;
                    _freeList = slab;
                    _freeSlabs++;
                }

                _slabs--;
            }
        }

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
            if (capacity > _maxFreeSlabs)
                capacity = _maxFreeSlabs;
            var nodeSize = 1 + _length;
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + 64 * nodeSize));
                var slab = (MemorySlab*)array;
                array += sizeof(MemorySlab);
                for (byte i = 0; i < 64; ++i)
                    *(array + i * nodeSize) = i;
                slab->Next = _freeList;
                _freeList = slab;
            }

            return _freeSlabs;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var node = _freeList;
            while (_freeSlabs > 0)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            _freeList = node;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            var node = _freeList;
            while (_freeSlabs > capacity)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            _freeList = node;
            return _freeSlabs;
        }

        /// <summary>
        ///     Slab
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemorySlab
        {
            /// <summary>
            ///     Next
            /// </summary>
            public MemorySlab* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public MemorySlab* Previous;

            /// <summary>
            ///     Bitmap
            /// </summary>
            public ulong Bitmap;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSize64MemoryPool Empty => new();
    }
}