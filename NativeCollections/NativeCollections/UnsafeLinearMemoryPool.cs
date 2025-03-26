using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe linear memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(NativeCollectionType.None)]
    public unsafe struct UnsafeLinearMemoryPool : IDisposable
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
        ///     Size
        /// </summary>
        private int _size;

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
        ///     Max length
        /// </summary>
        public int MaxLength => _size - sizeof(MemoryNode);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="maxLength">Max length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeLinearMemoryPool(int maxLength, int maxFreeSlabs)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "MustBePositive");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            var size = sizeof(MemoryNode) + maxLength;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + size));
            var slab = (MemorySlab*)array;
            slab->Next = slab;
            slab->Previous = slab;
            slab->Nodes = 0;
            slab->Length = 0;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _size = size;
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
        /// <param name="length">Length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var size = _size;
            if (length + sizeof(MemoryNode) > size)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeLessOrEqual");
            var slab = _sentinel;
            if (slab->Length + sizeof(MemoryNode) + length > size)
            {
                if (_freeSlabs == 0)
                {
                    var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + size));
                    slab = (MemorySlab*)array;
                }
                else
                {
                    slab = _freeList;
                    _freeList = slab->Next;
                    _freeSlabs--;
                }

                slab->Next = _sentinel;
                slab->Previous = _sentinel->Previous;
                slab->Nodes = 0;
                slab->Length = 0;
                _sentinel->Previous->Next = slab;
                _sentinel->Previous = slab;
                _sentinel = slab;
                _slabs++;
            }

            var node = (MemoryNode*)((byte*)slab + sizeof(MemorySlab) + slab->Length);
            node->Slab = slab;
            slab->Nodes++;
            slab->Length += sizeof(MemoryNode) + length;
            return (byte*)node + sizeof(MemoryNode);
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var node = (MemoryNode*)((byte*)ptr - sizeof(MemoryNode));
            var slab = node->Slab;
            slab->Nodes--;
            if (slab->Nodes == 0 && slab != _sentinel)
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
            var size = _size;
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(MemorySlab) + size));
                var slab = (MemorySlab*)array;
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
            ///     Nodes
            /// </summary>
            public int Nodes;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryNode
        {
            /// <summary>
            ///     Slab
            /// </summary>
            public MemorySlab* Slab;
        }
        
        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeLinearMemoryPool Empty => new();
    }
}