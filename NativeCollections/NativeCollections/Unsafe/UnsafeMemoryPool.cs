using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory pool
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
        ///     Size
        /// </summary>
        private readonly int _size;

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
            ThrowHelpers.ThrowIfNegativeOrZero(size, ExceptionArgument.size);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, Math.Max((int)NativeMemoryAllocator.AlignOf<MemorySlab>(), (int)NativeMemoryAllocator.AlignOf<MemoryNode>()));
            var alignedSlabSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemorySlab>(), (nuint)alignment);
            var alignedNodeSize = (int)NativeMemoryAllocator.AlignUp((nuint)Unsafe.SizeOf<MemoryNode>(), (nuint)alignment);
            var alignedLength = (int)NativeMemoryAllocator.AlignUp((nuint)length, (nuint)alignment);
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
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeMemoryPool left, UnsafeMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeMemoryPool left, UnsafeMemoryPool right) => !left.Equals(right);

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
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
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
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
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
                var slab = Create(_alignedSlabSize, _size, _fullNodeSize, _alignment);
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