using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe linear memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeLinearMemoryPool : IIsCreated, IDisposable, IEquatable<UnsafeLinearMemoryPool>
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
        ///     Max length
        /// </summary>
        public readonly int MaxLength => _size - Unsafe.SizeOf<nint>();

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="maxLength">Max length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeLinearMemoryPool(int maxLength, int maxFreeSlabs)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(maxLength, ExceptionArgument.maxLength);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            var size = Unsafe.SizeOf<nint>() + maxLength;
            var slab = Create(size);
            slab->Next = slab;
            slab->Previous = slab;
            _sentinel = slab;
            _freeList = null;
            _slabs = 1;
            _freeSlabs = 0;
            _maxFreeSlabs = maxFreeSlabs;
            _size = size;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeLinearMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeLinearMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeLinearMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => !left.Equals(right);

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
                    Initialize(temp);
                    temp->Next = _freeList;
                    _freeList = temp;
                    _freeSlabs++;
                }
            }

            Initialize(node);
            node->Next = node;
            node->Previous = node;
            _sentinel = node;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, (int)NativeMemoryAllocator.AlignOf<nint>());
            var byteCount = Unsafe.SizeOf<nint>() + (alignment - 1) + length;
            if (byteCount > _size)
                ThrowHelpers.ThrowMustBeLessOrEqualException(length, ExceptionArgument.length);
            var slab = _sentinel;
            if (slab->Length + byteCount > _size)
            {
                if (_freeSlabs == 0)
                {
                    slab = Create(_size);
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

            var endAddress = (nint)slab + Unsafe.SizeOf<MemorySlab>() + slab->Length;
            var result = (void*)(nint)NativeMemoryAllocator.AlignUp((nuint)(endAddress + Unsafe.SizeOf<nint>()), (nuint)alignment);
            var byteOffset = UnsafeHelpers.ByteOffset(slab, result);
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = byteOffset;
            slab->Count++;
            slab->Length = (int)((nint)result - (nint)slab) - Unsafe.SizeOf<MemorySlab>() + length;
            return result;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var byteOffset = Unsafe.Subtract(ref Unsafe.AsRef<nint>(ptr), 1);
            var slab = UnsafeHelpers.SubtractByteOffset<MemorySlab>(ptr, byteOffset);
            slab->Count--;
            if (slab->Count == 0 && slab != _sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (_freeSlabs == _maxFreeSlabs)
                {
                    NativeMemoryAllocator.AlignedFree(slab);
                }
                else
                {
                    slab->Length = 0;
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Min(capacity, _maxFreeSlabs);
            while (_freeSlabs < capacity)
            {
                _freeSlabs++;
                var slab = Create(_size);
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
        private static MemorySlab* Create(int size)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc((uint)(Unsafe.SizeOf<MemorySlab>() + size), (uint)NativeMemoryAllocator.AlignOf<nint>());
            Initialize(slab);
            return slab;
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab)
        {
            slab->Count = 0;
            slab->Length = 0;
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
            ///     Count
            /// </summary>
            public int Count;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeLinearMemoryPool Empty => new();
    }
}