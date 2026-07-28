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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeLinearMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeLinearMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeLinearMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => !left.Equals(right);

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
        ///     Gets the maximum user data length that can be allocated with the specified alignment.
        /// </summary>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>The maximum length in bytes, or 0 if even a zero‑length allocation is not possible.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetMaxLength(int alignment)
        {
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2((uint)alignment, ExceptionArgument.alignment);
            alignment = Math.Max(alignment, (int)NativeMemoryAllocator.AlignOf<nint>());
            var byteOffset = alignment - 1 + Unsafe.SizeOf<nint>();
            return Math.Max(0, _size - byteOffset);
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
            var byteOffset = alignment - 1 + Unsafe.SizeOf<nint>();
            var bytes = length + byteOffset;
            if ((uint)bytes > (uint)_size)
                ThrowHelpers.ThrowMustBeLessOrEqualException(length, ExceptionArgument.length);
            var slab = _sentinel;
            if ((ulong)slab->Length + (ulong)bytes > (ulong)_size)
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

            var startAddress = (nint)slab + Unsafe.SizeOf<MemorySlab>() + slab->Length;
            var result = (void*)NativeMemoryAllocator.AlignUp((nuint)(startAddress + Unsafe.SizeOf<nint>()), (uint)alignment);
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = (nint)slab;
            slab->Count++;
            slab->Length += bytes;
            return result;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent<T>(int elementCount) where T : unmanaged => (T*)Rent(elementCount * Unsafe.SizeOf<T>(), (int)NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var slab = (MemorySlab*)Unsafe.Subtract(ref Unsafe.AsRef<nint>(ptr), 1);
            slab->Count--;
            if (slab->Count == 0)
            {
                slab->Length = 0;
                if (slab != _sentinel)
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
                var slab = Create(_size);
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
        private static MemorySlab* Create(int size)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc((uint)(Unsafe.SizeOf<MemorySlab>() + size), NativeMemoryAllocator.AlignOf<nint>());
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
            ///     Gets the number of elements contained in this.
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
        public static UnsafeLinearMemoryPool Empty => default;
    }
}