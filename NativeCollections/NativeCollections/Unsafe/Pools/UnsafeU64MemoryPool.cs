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
        ///     Gets the length in bytes of the data region within each node.
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
        public readonly bool IsCreated => _sentinel != null;

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
        ///     Gets the length in bytes of the data region within each node.
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
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="sentinel">Sentinel node of the linked list of active slabs.</param>
        /// <param name="freeList">Head of the free list of slabs that are available for reuse.</param>
        /// <param name="slabs">The total number of slabs currently allocated in the pool.</param>
        /// <param name="freeSlabs">The number of slabs that are currently free and available for reuse.</param>
        /// <param name="maxFreeSlabs">The maximum number of free slabs retained by the pool.</param>
        /// <param name="length">The length (in bytes) of the data region within each node.</param>
        /// <param name="alignment">The alignment requirement (in bytes) for allocations managed by the pool.</param>
        /// <param name="alignedSlabSize">The aligned size (in bytes) of the per-slab overhead.</param>
        /// <param name="alignedNodeSize">The aligned size (in bytes) of the per-node header.</param>
        /// <param name="alignedLength">The aligned length (in bytes) of the data region within each node.</param>
        /// <param name="fullNodeSize">
        ///     The total size (in bytes) of a node, including its header and data region.
        /// </param>
        private UnsafeU64MemoryPool(MemorySlab* sentinel, MemorySlab* freeList, int slabs, int freeSlabs, int maxFreeSlabs, int length, int alignment, int alignedSlabSize, int alignedNodeSize, int alignedLength, int fullNodeSize)
        {
            _sentinel = sentinel;
            _freeList = freeList;
            _slabs = slabs;
            _freeSlabs = freeSlabs;
            _maxFreeSlabs = maxFreeSlabs;
            _length = length;
            _alignment = alignment;
            _alignedSlabSize = alignedSlabSize;
            _alignedNodeSize = alignedNodeSize;
            _alignedLength = alignedLength;
            _fullNodeSize = fullNodeSize;
        }

        /// <summary>
        ///     Creates a 64-bit bitmap memory pool with the specified node length,
        ///     maximum free slabs, and alignment.
        /// </summary>
        /// <param name="length">The length in bytes of each node's data region.</param>
        /// <param name="maxFreeSlabs">The maximum number of free slabs to retain.</param>
        /// <param name="alignment">The required allocation alignment.</param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when an argument is negative.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="alignment" /> is not a power of two.</exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the aligned node size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        /// <remarks>
        ///     The per-node stride is multiplied with pointer-width arithmetic when a node is
        ///     rented or returned, so a slab whose total byte size exceeds <see cref="int.MaxValue" />
        ///     is still addressed correctly on 64-bit runtimes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeU64MemoryPool Create(int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2(Usize.FromI32Unchecked(alignment), ExceptionArgument.alignment);
            alignment = Math.Max(alignment, I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<MemorySlab>()));
            var alignedSlabSize = I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(Unsafe.SizeOf<MemorySlab>()), Usize.FromI32Unchecked(alignment)));
            var alignedNodeSize = I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(Unsafe.SizeOf<nint>()), Usize.FromI32Unchecked(alignment)));
            var alignedLength = I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(length), Usize.FromI32Unchecked(alignment)));
            var fullNodeSize = alignedNodeSize + alignedLength;
            var slab = CreateSlab(alignedSlabSize, fullNodeSize, alignment);
            slab->Next = slab;
            slab->Previous = slab;
            return new UnsafeU64MemoryPool(slab, null, 1, 0, maxFreeSlabs, length, alignment, alignedSlabSize, alignedNodeSize, alignedLength, fullNodeSize);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeU64MemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeU64MemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeU64MemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeU64MemoryPool left, UnsafeU64MemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeU64MemoryPool left, UnsafeU64MemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var node = _sentinel;
            while (_slabs > 0)
            {
                _slabs = _slabs.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            node = _freeList;
            while (_freeSlabs > 0)
            {
                _freeSlabs = _freeSlabs.WrappingSub(1);
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
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
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
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        private void ClearInternal(int capacity)
        {
            TrimExcessInternal(capacity);
            var node = _sentinel;
            while (_slabs > 1)
            {
                _slabs = _slabs.WrappingSub(1);
                var temp = node;
                node = node->Next;
                if (_freeSlabs == capacity)
                {
                    NativeMemoryAllocator.AlignedFree(temp);
                }
                else
                {
                    Initialize(temp, _alignedSlabSize, _fullNodeSize);
                    temp->Next = _freeList;
                    _freeList = temp;
                    _freeSlabs = _freeSlabs.WrappingAdd(1);
                }
            }

            Initialize(node, _alignedSlabSize, _fullNodeSize);
            node->Next = node;
            node->Previous = node;
            _sentinel = node;
        }

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        /// <returns>A pointer to a buffer with <see cref="Length" /> bytes.</returns>
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
                        slab = CreateSlab(_alignedSlabSize, _fullNodeSize, _alignment);
                    }
                    else
                    {
                        slab = _freeList;
                        _freeList = slab->Next;
                        _freeSlabs = _freeSlabs.WrappingSub(1);
                    }

                    slab->Next = _sentinel;
                    slab->Previous = _sentinel->Previous;
                    _sentinel->Previous->Next = slab;
                    _sentinel->Previous = slab;
                    _sentinel = slab;
                    _slabs = _slabs.WrappingAdd(1);
                }
            }

            ref var segment = ref slab->Bitmap;
            var bitMask = BitOperationsHelpers.TrailingZeroCount(~segment);
            segment |= 1UL << bitMask;
            return UnsafeHelpers.AddByteOffset(slab, Isize.FromUsizeUnchecked(Usize.FromI32Unchecked(_alignedSlabSize).WrappingAdd(Usize.FromI32Unchecked(bitMask).WrappingMul(Usize.FromI32Unchecked(_fullNodeSize))).WrappingAdd(Usize.FromI32Unchecked(_alignedNodeSize))));
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var bitMask = I32.FromIsizeUnchecked(Unsafe.AsRef<nint>(UnsafeHelpers.SubtractByteOffset(ptr, Isize.FromI32Unchecked(_alignedNodeSize))));
            var slab = (MemorySlab*)UnsafeHelpers.SubtractByteOffset(ptr, Isize.FromUsizeUnchecked(Usize.FromI32Unchecked(_alignedSlabSize).WrappingAdd(Usize.FromI32Unchecked(bitMask).WrappingMul(Usize.FromI32Unchecked(_fullNodeSize))).WrappingAdd(Usize.FromI32Unchecked(_alignedNodeSize))));
            ref var segment = ref slab->Bitmap;
            segment &= ~(1UL << bitMask);
            if (slab != _sentinel)
            {
                if (segment == 0UL)
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
                        _freeSlabs = _freeSlabs.WrappingAdd(1);
                    }

                    _slabs = _slabs.WrappingSub(1);
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
        ///     Ensures that this retains at least the specified number of free slabs, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeSlabs);
            while (_freeSlabs < capacity)
            {
                _freeSlabs = _freeSlabs.WrappingAdd(1);
                var slab = CreateSlab(_alignedSlabSize, _fullNodeSize, _alignment);
                slab->Next = _freeList;
                _freeList = slab;
            }

            return _freeSlabs;
        }

        /// <summary>
        ///     Releases all free slabs from this.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess()
        {
            TrimExcessInternal(0);
            return 0;
        }

        /// <summary>
        ///     Trims the retained free-slab capacity of this to the specified number of slabs.
        /// </summary>
        /// <param name="capacity">The maximum number of free slabs to retain.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            TrimExcessInternal(capacity);
            return _freeSlabs;
        }

        /// <summary>
        ///     Trims the retained free-slab capacity of this to the specified number of slabs.
        /// </summary>
        /// <param name="capacity">The maximum number of free slabs to retain.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TrimExcessInternal(int capacity)
        {
            var node = _freeList;
            while (_freeSlabs > capacity)
            {
                _freeSlabs = _freeSlabs.WrappingSub(1);
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="alignedSlabSize">The aligned size, in bytes, of the slab header.</param>
        /// <param name="fullNodeSize">
        ///     The total size, in bytes, of one node, including its header and aligned data region.
        /// </param>
        /// <param name="alignment">The alignment, in bytes, of the allocated slab.</param>
        /// <returns>A pointer to the newly allocated slab whose bitmap has 64 free slots.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemorySlab* CreateSlab(int alignedSlabSize, int fullNodeSize, int alignment)
        {
            var byteCount = Usize.FromI32Unchecked(alignedSlabSize) + Usize.FromU32Unchecked(64U) * Usize.FromI32Unchecked(fullNodeSize);
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc(byteCount, Usize.FromI32Unchecked(alignment));
            Initialize(slab, alignedSlabSize, fullNodeSize);
            return slab;
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="slab">A pointer to the slab to initialize.</param>
        /// <param name="alignedSlabSize">The aligned size, in bytes, of the slab header.</param>
        /// <param name="fullNodeSize">
        ///     The total size, in bytes, of one node, including its header and aligned data region.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab, int alignedSlabSize, int fullNodeSize)
        {
            var buffer = UnsafeHelpers.AddByteOffset(slab, Isize.FromI32Unchecked(alignedSlabSize));
            for (var i = 0; i < 64; i = i.WrappingAdd(1))
                Unsafe.AsRef<nint>(UnsafeHelpers.AddByteOffset(buffer, Isize.FromUsizeUnchecked(Usize.FromI32Unchecked(i).WrappingMul(Usize.FromI32Unchecked(fullNodeSize))))) = Isize.FromI32Unchecked(i);

            slab->Bitmap = 0UL;
        }

        /// <summary>
        ///     Represents a slab in the memory pool, containing the linked-list pointers
        ///     and the allocation bitmap.
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
            ///     Bitmap indicating which nodes in the slab are allocated (bit = 1) or free (bit = 0).
            /// </summary>
            public ulong Bitmap;
        }
    }
}