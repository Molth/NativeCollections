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
    [UnsafeCollection(FromType.None)]
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
            ThrowHelpers.ThrowIfNegativeOrZero(maxLength, nameof(maxLength));
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, nameof(maxFreeSlabs));
            var size = sizeof(MemoryNode) + maxLength;
            var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemorySlab) + size), (uint)NativeMemoryAllocator.AlignOf<nint>());
            var slab = (MemorySlab*)buffer;
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
                NativeMemoryAllocator.AlignedFree(temp);
            }

            node = _freeList;
            while (_freeSlabs > 0)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfNegative(alignment, nameof(alignment));
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, nameof(alignment));
            alignment = Math.Max(alignment, (int)NativeMemoryAllocator.AlignOf<MemoryNode>());
            var nodeSize = sizeof(MemoryNode);
            var maxPadding = alignment - 1;
            var byteCount = nodeSize + maxPadding + length;
            if (byteCount > _size)
            {
                ThrowHelpers.ThrowIfGreaterThan(nodeSize + maxPadding, _size, nameof(alignment));
                ThrowHelpers.ThrowMustBeLessOrEqualException(length, nameof(length));
            }

            var slab = _sentinel;
            if (slab->Length + byteCount > _size)
            {
                if (_freeSlabs == 0)
                {
                    var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemorySlab) + _size), (uint)NativeMemoryAllocator.AlignOf<nint>());
                    slab = (MemorySlab*)buffer;
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

            var node = UnsafeHelpers.AddByteOffset<MemoryNode>(slab, sizeof(MemorySlab) + slab->Length);
            node->Slab = slab;
            slab->Nodes++;
            var ptr = UnsafeHelpers.AddByteOffset(node, nodeSize);
            var result = (byte*)(nint)NativeMemoryAllocator.AlignUp((nuint)(nint)ptr, (nuint)alignment);
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = UnsafeHelpers.ByteOffset(node, result);
            slab->Length += nodeSize + (int)UnsafeHelpers.ByteOffset(ptr, result) + length;
            return result;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var byteOffset = (int)Unsafe.Subtract(ref Unsafe.AsRef<nint>(ptr), 1);
            var node = UnsafeHelpers.SubtractByteOffset<MemoryNode>(ptr, byteOffset);
            var slab = node->Slab;
            slab->Nodes--;
            if (slab->Nodes == 0 && slab != _sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (_freeSlabs == _maxFreeSlabs)
                {
                    NativeMemoryAllocator.AlignedFree(slab);
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
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity > _maxFreeSlabs)
                capacity = _maxFreeSlabs;
            var size = _size;
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemorySlab) + size), (uint)NativeMemoryAllocator.AlignOf<nint>());
                var slab = (MemorySlab*)buffer;
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
                NativeMemoryAllocator.AlignedFree(temp);
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
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            var node = _freeList;
            while (_freeSlabs > capacity)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
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

            /// <summary>
            ///     Dummy
            /// </summary>
            private nint _dummy;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeLinearMemoryPool Empty => new();
    }
}