using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory pool that provides reusable fixed-size memory blocks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeU64MemoryPool : IIsCreated, IDisposable, IEquatable<UnsafeU64MemoryPool>
    {
        /// <summary>
        ///     Sentinel node of the linked list of active slabs.
        /// </summary>
        private MemorySlab* _sentinel;

        /// <summary>
        ///     Head of the free list of slabs that are available for reuse.
        /// </summary>
        private MemorySlab* _freeList;

        /// <summary>
        ///     Gets the total number of slabs currently allocated in the pool.
        /// </summary>
        private int _slabs;

        /// <summary>
        ///     Gets the number of slabs that are currently free and available for reuse.
        /// </summary>
        private int _freeSlabs;

        /// <summary>
        ///     Gets the maximum number of free slabs that can be retained before excess slabs are freed.
        /// </summary>
        private readonly int _maxFreeSlabs;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        private readonly int _alignment;

        /// <summary>
        ///     The aligned size (in bytes) of the per-slab overhead, including alignment padding.
        /// </summary>
        private readonly int _alignedSlabSize;

        /// <summary>
        ///     The aligned size (in bytes) of the per-node header, including alignment padding.
        /// </summary>
        private readonly int _alignedNodeSize;

        /// <summary>
        ///     Gets the aligned length (in bytes) of the data portion of each node after alignment.
        /// </summary>
        private readonly int _alignedLength;

        /// <summary>
        ///     The total size (in bytes) of a single node, which includes the node header and the data region,
        ///     with alignment taken into account. This value is used for offset calculations within a slab.
        /// </summary>
        private readonly int _fullNodeSize;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_sentinel);

        /// <summary>
        ///     Gets the total number of slabs currently allocated in the pool.
        /// </summary>
        public readonly int Slabs => _slabs;

        /// <summary>
        ///     Gets the number of slabs that are currently free and available for reuse.
        /// </summary>
        public readonly int FreeSlabs => _freeSlabs;

        /// <summary>
        ///     Gets the maximum number of free slabs that can be retained before excess slabs are freed.
        /// </summary>
        public readonly int MaxFreeSlabs => _maxFreeSlabs;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public readonly int Alignment => _alignment;

        /// <summary>
        ///     Gets the aligned length (in bytes) of the data portion of each node after alignment.
        /// </summary>
        public readonly int AlignedLength => _alignedLength;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified node length, maximum free slabs, and alignment.
        /// </summary>
        /// <remarks>
        ///     Each slab in this pool contains exactly 64 nodes,
        ///     as the allocation bitmap is stored as a <see cref="ulong" />.
        /// </remarks>
        /// <param name="length">
        ///     The length (in bytes) of the data region of each node.
        ///     Must be non-negative.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain in the free list.
        ///     Must be non-negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment for allocations, in bytes.
        ///     Must be a power of two and at least the alignment of the internal slab structure.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" />, <paramref name="maxFreeSlabs" />,
        ///     or <paramref name="alignment" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeU64MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, (int)NativeMemoryAllocator.AlignOf<MemorySlab>());
            var alignedSlabSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemorySlab>(), (uint)alignment);
            var alignedNodeSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<nint>(), (uint)alignment);
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (uint)alignment);
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeU64MemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeU64MemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeU64MemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeU64MemoryPool left, UnsafeU64MemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeU64MemoryPool left, UnsafeU64MemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
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
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => ClearInternal(0);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeSlabs);
            ClearInternal(capacity);
            return _freeSlabs;
        }

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
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
        ///     Retrieves a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            var slab = _sentinel;
            if (slab->Bitmap == ulong.MaxValue)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Bitmap == ulong.MaxValue)
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
            segment |= 1UL << bitMask;
            return UnsafeHelpers.AddByteOffset(slab, _alignedSlabSize + bitMask * _fullNodeSize + _alignedNodeSize);
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var bitMask = (int)Unsafe.AsRef<nint>(UnsafeHelpers.SubtractByteOffset(ptr, _alignedNodeSize));
            var slab = (MemorySlab*)UnsafeHelpers.SubtractByteOffset(ptr, _alignedSlabSize + bitMask * _fullNodeSize + _alignedNodeSize);
            ref var segment = ref slab->Bitmap;
            segment &= ~(1UL << bitMask);
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

                if ((segment | (1UL << bitMask)) == ulong.MaxValue)
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
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
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
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            TrimExcessInternal(0);
            return 0;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            TrimExcessInternal(capacity);
            return _freeSlabs;
        }

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
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
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemorySlab* Create(int alignedSlabSize, int fullNodeSize, int alignment)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + 64 * fullNodeSize), (uint)alignment);
            Initialize(slab, alignedSlabSize, fullNodeSize);
            return slab;
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab, int alignedSlabSize, int fullNodeSize)
        {
            var buffer = UnsafeHelpers.AddByteOffset(slab, alignedSlabSize);
            for (var i = 0; i < 64; ++i)
                Unsafe.AsRef<nint>(UnsafeHelpers.AddByteOffset(buffer, i * fullNodeSize)) = i;
            slab->Bitmap = 0UL;
        }

        /// <summary>
        ///     Represents a slab in the memory pool, containing a linked list of nodes and metadata.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemorySlab
        {
            /// <summary>
            ///     Pointer to the next slab in the linked list.
            /// </summary>
            public MemorySlab* Next;

            /// <summary>
            ///     Pointer to the previous slab in the linked list.
            /// </summary>
            public MemorySlab* Previous;

            /// <summary>
            ///     Bitmap indicating which nodes in the slab are allocated
            ///     (bit = 1) or free (bit = 0).
            /// </summary>
            public ulong Bitmap;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeU64MemoryPool Empty => default;
    }
}