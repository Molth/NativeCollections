using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe ulong bitmap memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeUInt64MemoryPool : IDisposable
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
        ///     Aligned node byte count
        /// </summary>
        private int _alignedNodeByteCount;

        /// <summary>
        ///     Aligned slab byte count
        /// </summary>
        private int _alignedSlabByteCount;

        /// <summary>
        ///     Free slabs
        /// </summary>
        private int _freeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        private int _maxFreeSlabs;

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
        ///     Alignment
        /// </summary>
        public int Alignment => _alignment;

        /// <summary>
        ///     Aligned length
        /// </summary>
        public int AlignedLength => _alignedLength;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUInt64MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            if (alignment < 0)
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "MustBeNonNegative");
            if (!BitOperationsHelpers.IsPow2((uint)alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (nuint)alignment);
            var alignedNodeByteCount = (int)NativeMemoryAllocator.AlignUp((nuint)sizeof(nint), (nuint)alignment);
            var nodeByteCount = alignedNodeByteCount + alignedLength;
            var alignedSlabByteCount = (int)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemorySlab), (nuint)alignment);
            var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabByteCount + 64 * nodeByteCount), (uint)alignment);
            var slab = (MemorySlab*)buffer;
            slab->Next = slab;
            slab->Previous = slab;
            slab->Bitmap = 0UL;
            buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabByteCount);
            for (var i = 0; i < 64; ++i)
                Unsafe.As<byte, nint>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(buffer), UnsafeHelpers.ToIntPtr(i * nodeByteCount))) = i;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _length = length;
            _alignment = alignment;
            _alignedLength = alignedLength;
            _alignedNodeByteCount = alignedNodeByteCount;
            _alignedSlabByteCount = alignedSlabByteCount;
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
            var slab = _sentinel;
            var alignedNodeByteCount = _alignedNodeByteCount;
            var nodeByteCount = alignedNodeByteCount + _alignedLength;
            var alignedSlabByteCount = _alignedSlabByteCount;
            if (slab->Bitmap == ulong.MaxValue)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Bitmap == ulong.MaxValue)
                {
                    if (_freeSlabs == 0)
                    {
                        var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabByteCount + 64 * nodeByteCount), (uint)_alignment);
                        slab = (MemorySlab*)buffer;
                        buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabByteCount);
                        for (var i = 0; i < 64; ++i)
                            Unsafe.As<byte, nint>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(buffer), UnsafeHelpers.ToIntPtr(i * nodeByteCount))) = i;
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
            return UnsafeHelpers.AddByteOffset(slab, alignedSlabByteCount + id * nodeByteCount + alignedNodeByteCount);
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var alignedNodeByteCount = _alignedNodeByteCount;
            var nodeByteCount = alignedNodeByteCount + _alignedLength;
            var alignedSlabByteCount = _alignedSlabByteCount;
            var buffer = (byte*)ptr;
            var id = (int)Unsafe.AsRef<nint>(UnsafeHelpers.SubtractByteOffset<byte>(buffer, alignedNodeByteCount));
            buffer = UnsafeHelpers.SubtractByteOffset<byte>(buffer, alignedSlabByteCount + id * nodeByteCount + alignedNodeByteCount);
            var slab = (MemorySlab*)buffer;
            ref var segment = ref slab->Bitmap;
            segment &= ~(1UL << id);
            if (segment == 0 && slab != _sentinel)
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
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity > _maxFreeSlabs)
                capacity = _maxFreeSlabs;
            var nodeByteCount = sizeof(nint) + _alignedLength;
            var alignedSlabByteCount = _alignedSlabByteCount;
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var buffer = (byte*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabByteCount + 64 * nodeByteCount), (uint)_alignment);
                var slab = (MemorySlab*)buffer;
                buffer = UnsafeHelpers.AddByteOffset<byte>(buffer, alignedSlabByteCount);
                for (var i = 0; i < 64; ++i)
                    Unsafe.As<byte, nint>(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(buffer), UnsafeHelpers.ToIntPtr(i * nodeByteCount))) = i;
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
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
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
            ///     Bitmap
            /// </summary>
            public ulong Bitmap;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeUInt64MemoryPool Empty => new();
    }
}