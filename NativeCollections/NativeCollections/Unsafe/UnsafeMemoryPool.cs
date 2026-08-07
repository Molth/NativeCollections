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
    public unsafe struct UnsafeMemoryPool : IIsCreated, IDisposable, IEquatable<UnsafeMemoryPool>
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
        ///     Gets the number of elements.
        /// </summary>
        private readonly int _size;

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
        ///     Gets the number of elements.
        /// </summary>
        public readonly int Size => _size;

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
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryPool(int size, int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, Math.Max((int)NativeMemoryAllocator.AlignOf<MemorySlab>(), (int)NativeMemoryAllocator.AlignOf<MemoryNode>()));
            var alignedSlabSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemorySlab>(), (uint)alignment);
            var alignedNodeSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryNode>(), (uint)alignment);
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (uint)alignment);
            var fullNodeSize = alignedNodeSize + alignedLength;
            var slab = Create(alignedSlabSize, size, fullNodeSize, alignment);
            slab->Next = slab;
            slab->Previous = slab;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _size = size;
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
        public readonly bool Equals(UnsafeMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryPool left, UnsafeMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryPool left, UnsafeMemoryPool right) => !left.Equals(right);

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
                    Initialize(temp, _alignedSlabSize, _size, _fullNodeSize);
                    temp->Next = _freeList;
                    _freeList = temp;
                    _freeSlabs++;
                }
            }

            Initialize(node, _alignedSlabSize, _size, _fullNodeSize);
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
            if (slab->Count == 0)
            {
                _sentinel = slab->Next;
                slab = _sentinel;
                if (slab->Count == 0)
                {
                    if (_freeSlabs == 0)
                    {
                        slab = Create(_alignedSlabSize, _size, _fullNodeSize, _alignment);
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

            var node = slab->Sentinel;
            slab->Sentinel = node->Next;
            node->Slab = slab;
            slab->Count--;
            return UnsafeHelpers.AddByteOffset(node, _alignedNodeSize);
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var node = UnsafeHelpers.SubtractByteOffset<MemoryNode>(ptr, _alignedNodeSize);
            var slab = node->Slab;
            slab->Count++;
            if (slab != _sentinel)
            {
                if (slab->Count == _size)
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

                if (slab->Count == 1)
                {
                    slab->Previous->Next = slab->Next;
                    slab->Next->Previous = slab->Previous;
                    slab->Next = _sentinel->Next;
                    slab->Previous = _sentinel;
                    _sentinel->Next->Previous = slab;
                    _sentinel->Next = slab;
                }
            }

            node->Next = slab->Sentinel;
            slab->Sentinel = node;
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
                var slab = Create(_alignedSlabSize, _size, _fullNodeSize, _alignment);
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
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemorySlab* Create(int alignedSlabSize, int size, int fullNodeSize, int alignment)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc((uint)(alignedSlabSize + size * fullNodeSize), (uint)alignment);
            Initialize(slab, alignedSlabSize, size, fullNodeSize);
            return slab;
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab, int alignedSlabSize, int size, int fullNodeSize)
        {
            var buffer = UnsafeHelpers.AddByteOffset(slab, alignedSlabSize);
            MemoryNode* next = null;
            for (var i = size - 1; i >= 0; --i)
            {
                var node = UnsafeHelpers.AddByteOffset<MemoryNode>(buffer, i * fullNodeSize);
                node->Next = next;
                next = node;
            }

            slab->Sentinel = next;
            slab->Count = size;
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
            ///     Gets the number of elements contained in this.
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
        public static UnsafeMemoryPool Empty => default;
    }
}