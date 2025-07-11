﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryPool : IDisposable
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
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Alignment
        /// </summary>
        private int _alignment;

        /// <summary>
        ///     Aligned length
        /// </summary>
        private int _alignedLength;

        /// <summary>
        ///     Aligned node size
        /// </summary>
        private int _alignedNodeSize;

        /// <summary>
        ///     Aligned slab size
        /// </summary>
        private int _alignedSlabSize;

        /// <summary>
        ///     Slabs
        /// </summary>
        public readonly int Slabs => _slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        public readonly int FreeSlabs => _freeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        public readonly int MaxFreeSlabs => _maxFreeSlabs;

        /// <summary>
        ///     Size
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Alignment
        /// </summary>
        public readonly int Alignment => _alignment;

        /// <summary>
        ///     Aligned length
        /// </summary>
        public readonly int AlignedLength => _alignedLength;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryPool(int size, int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, nameof(size));
            ThrowHelpers.ThrowIfNegative(length, nameof(length));
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, nameof(maxFreeSlabs));
            ThrowHelpers.ThrowIfNegative(alignment, nameof(alignment));
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, nameof(alignment));
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (nuint)alignment);
            var alignedNodeSize = (int)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemoryNode), (nuint)alignment);
            var nodeSize = alignedNodeSize + alignedLength;
            var alignedSlabSize = (int)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemorySlab), (nuint)alignment);
            var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + size * nodeSize), (uint)alignment);
            var slab = (MemorySlab*)buffer;
            slab->Next = slab;
            slab->Previous = slab;
            buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabSize);
            MemoryNode* next = null;
            for (var i = size - 1; i >= 0; --i)
            {
                var node = UnsafeHelpers.AddByteOffset<MemoryNode>(buffer, i * nodeSize);
                node->Next = next;
                next = node;
            }

            slab->Sentinel = next;
            slab->Count = size;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _size = size;
            _length = length;
            _alignment = alignment;
            _alignedLength = alignedLength;
            _alignedNodeSize = alignedNodeSize;
            _alignedSlabSize = alignedSlabSize;
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
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            MemoryNode* node;
            var slab = _sentinel;
            if (slab->Count == 0)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Count == 0)
                {
                    var size = _size;
                    if (_freeSlabs == 0)
                    {
                        var nodeSize = _alignedNodeSize + _alignedLength;
                        var alignedSlabSize = _alignedSlabSize;
                        var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + size * nodeSize), (uint)_alignment);
                        slab = (MemorySlab*)buffer;
                        buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabSize);
                        MemoryNode* next = null;
                        for (var i = size - 1; i >= 0; --i)
                        {
                            node = UnsafeHelpers.AddByteOffset<MemoryNode>(buffer, i * nodeSize);
                            node->Next = next;
                            next = node;
                        }

                        slab->Sentinel = next;
                    }
                    else
                    {
                        slab = _freeList;
                        _freeList = slab->Next;
                        _freeSlabs--;
                    }

                    slab->Next = _sentinel;
                    slab->Previous = _sentinel->Previous;
                    slab->Count = size;
                    _sentinel->Previous->Next = slab;
                    _sentinel->Previous = slab;
                    _sentinel = slab;
                    _slabs++;
                }
            }

            node = slab->Sentinel;
            slab->Sentinel = node->Next;
            node->Slab = slab;
            slab->Count--;
            return UnsafeHelpers.AddByteOffset(node, _alignedNodeSize);
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var node = (MemoryNode*)UnsafeHelpers.Subtract<byte>(ptr, _alignedNodeSize);
            var slab = node->Slab;
            slab->Count++;
            if (slab->Count == _size && slab != _sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (_freeSlabs == _maxFreeSlabs)
                {
                    NativeMemoryAllocator.AlignedFree(slab);
                }
                else
                {
                    node->Next = slab->Sentinel;
                    slab->Sentinel = node;
                    slab->Next = _freeList;
                    _freeList = slab;
                    _freeSlabs++;
                }

                _slabs--;
                return;
            }

            node->Next = slab->Sentinel;
            slab->Sentinel = node;
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
            var nodeSize = _alignedNodeSize + _alignedLength;
            var alignedSlabSize = _alignedSlabSize;
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + size * nodeSize), (uint)_alignment);
                var slab = (MemorySlab*)buffer;
                buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabSize);
                MemoryNode* next = null;
                for (var i = size - 1; i >= 0; --i)
                {
                    var node = UnsafeHelpers.AddByteOffset<MemoryNode>(buffer, i * nodeSize);
                    node->Next = next;
                    next = node;
                }

                slab->Sentinel = next;
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
            ///     Sentinel
            /// </summary>
            public MemoryNode* Sentinel;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct MemoryNode
        {
            /// <summary>
            ///     Slab
            /// </summary>
            [FieldOffset(0)] public MemorySlab* Slab;

            /// <summary>
            ///     Next
            /// </summary>
            [FieldOffset(0)] public MemoryNode* Next;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryPool Empty => new();
    }
}