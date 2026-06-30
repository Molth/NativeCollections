using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe uint bitmap memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeUInt32MemoryPool : IIsCreated, IDisposable, IEquatable<UnsafeUInt32MemoryPool>
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
        private readonly int _maxFreeSlabs;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Alignment
        /// </summary>
        private readonly int _alignment;

        /// <summary>
        ///     Aligned slab size
        /// </summary>
        private readonly int _alignedSlabSize;

        /// <summary>
        ///     Aligned node size
        /// </summary>
        private readonly int _alignedNodeSize;

        /// <summary>
        ///     Aligned length
        /// </summary>
        private readonly int _alignedLength;

        /// <summary>
        ///     Full node size
        /// </summary>
        private readonly int _fullNodeSize;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_sentinel);

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
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUInt32MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, (int)NativeMemoryAllocator.AlignOf<MemorySlab>());
            var alignedSlabSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemorySlab>(), (nuint)alignment);
            var alignedNodeSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<nint>(), (nuint)alignment);
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (nuint)alignment);
            var fullNodeSize = alignedNodeSize + alignedLength;
            var slab = Create(alignedSlabSize, fullNodeSize, alignment);
            slab->Next = slab;
            slab->Previous = slab;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _length = length;
            _alignment = alignment;
            _alignedSlabSize = alignedSlabSize;
            _alignedNodeSize = alignedNodeSize;
            _alignedLength = alignedLength;
            _fullNodeSize = fullNodeSize;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeUInt32MemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeUInt32MemoryPool other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeUInt32MemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeUInt32MemoryPool left, UnsafeUInt32MemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeUInt32MemoryPool left, UnsafeUInt32MemoryPool right) => !left.Equals(right);

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
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => ClearInternal(0);

        /// <summary>
        ///     Clear
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeSlabs);
            ClearInternal(capacity);
            return _freeSlabs;
        }

        /// <summary>
        ///     Clear
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearInternal(int capacity)
        {
            TrimExcessInternal(capacity);
            var node = _sentinel;
            while (_slabs > 1)
            {
                _slabs--;
                var temp = node;
                node = node->Next;
                if (_freeSlabs == capacity)
                {
                    NativeMemoryAllocator.AlignedFree(temp);
                }
                else
                {
                    temp->Next = _freeList;
                    _freeList = temp;
                    _freeSlabs++;
                }
            }

            node->Next = node;
            node->Previous = node;
            _sentinel = node;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            var slab = _sentinel;
            if (slab->Bitmap == uint.MaxValue)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Bitmap == uint.MaxValue)
                {
                    if (_freeSlabs == 0)
                    {
                        slab = Create(_alignedSlabSize, _fullNodeSize, _alignment);
                    }
                    else
                    {
                        slab = _freeList;
                        _freeList = slab->Next;
                        _freeSlabs--;
                    }

                    slab->Next = _sentinel;
                    slab->Previous = _sentinel->Previous;
                    _sentinel->Previous->Next = slab;
                    _sentinel->Previous = slab;
                    _sentinel = slab;
                    _slabs++;
                }
            }

            ref var segment = ref slab->Bitmap;
            var bitMask = BitOperationsHelpers.TrailingZeroCount(~segment);
            segment |= 1U << bitMask;
            return UnsafeHelpers.AddByteOffset(slab, _alignedSlabSize + bitMask * _fullNodeSize + _alignedNodeSize);
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var bitMask = (int)Unsafe.AsRef<nint>(UnsafeHelpers.SubtractByteOffset(ptr, _alignedNodeSize));
            var slab = (MemorySlab*)UnsafeHelpers.SubtractByteOffset(ptr, _alignedSlabSize + bitMask * _fullNodeSize + _alignedNodeSize);
            ref var segment = ref slab->Bitmap;
            segment &= ~(1U << bitMask);
            if (slab != _sentinel)
            {
                if (segment == 0)
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
                    return;
                }

                if ((segment | (1U << bitMask)) == uint.MaxValue)
                {
                    slab->Previous->Next = slab->Next;
                    slab->Next->Previous = slab->Previous;
                    slab->Next = _sentinel->Next;
                    slab->Previous = _sentinel;
                    _sentinel->Next->Previous = slab;
                    _sentinel->Next = slab;
                }
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeSlabs);
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var slab = Create(_alignedSlabSize, _fullNodeSize, _alignment);
                slab->Next = _freeList;
                _freeList = slab;
            }

            return _freeSlabs;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => TrimExcessInternal(0);

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            TrimExcessInternal(capacity);
            return _freeSlabs;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TrimExcessInternal(int capacity)
        {
            var node = _freeList;
            while (_freeSlabs > capacity)
            {
                _freeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
        }

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemorySlab* Create(int alignedSlabSize, int fullNodeSize, int alignment)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + 32 * fullNodeSize), (uint)alignment);
            Initialize(slab, alignedSlabSize, fullNodeSize);
            return slab;
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab, int alignedSlabSize, int fullNodeSize)
        {
            var buffer = UnsafeHelpers.AddByteOffset(slab, alignedSlabSize);
            for (var i = 0; i < 32; ++i)
                Unsafe.AsRef<nint>(UnsafeHelpers.AddByteOffset(buffer, i * fullNodeSize)) = i;
            slab->Bitmap = 0U;
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
            public uint Bitmap;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeUInt32MemoryPool Empty => new();
    }
}