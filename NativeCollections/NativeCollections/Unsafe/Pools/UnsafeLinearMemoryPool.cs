using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory pool that provides reusable dynamic-size memory blocks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeLinearMemoryPool : IIsCreated, IDisposable, IEquatable<UnsafeLinearMemoryPool>
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
        ///     Gets the size of each slab in bytes.
        /// </summary>
        private readonly int _size;

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
        ///     Gets the maximum usable length (in bytes) of a user allocation that can fit
        ///     within a single slab, assuming the default alignment of <see cref="IntPtr" />.
        /// </summary>
        /// <remarks>
        ///     This value is determined by the total slab size minus the overhead required for the slab header
        ///     and the alignment adjustment for a <see cref="IntPtr" />‑aligned pointer. It represents the largest
        ///     contiguous block of memory that can be allocated in one slab when using the default alignment.
        /// </remarks>
        public readonly int MaxLength => GetMaxLength(I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<nint>()));

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="sentinel">Sentinel node of the linked list of active slabs.</param>
        /// <param name="freeList">Head of the free list of slabs that are available for reuse.</param>
        /// <param name="slabs">The total number of slabs currently allocated in the pool.</param>
        /// <param name="freeSlabs">The number of slabs that are currently free and available for reuse.</param>
        /// <param name="maxFreeSlabs">The maximum number of free slabs retained by the pool.</param>
        /// <param name="size">The number of nodes in each slab.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeLinearMemoryPool(MemorySlab* sentinel, MemorySlab* freeList, int slabs, int freeSlabs, int maxFreeSlabs, int size)
        {
            _sentinel = sentinel;
            _freeList = freeList;
            _slabs = slabs;
            _freeSlabs = freeSlabs;
            _maxFreeSlabs = maxFreeSlabs;
            _size = size;
        }

        /// <summary>
        ///     Creates a memory pool with the specified maximum allocation length and
        ///     maximum number of free slabs to retain.
        /// </summary>
        /// <param name="maxLength">
        ///     The payload budget (in bytes) of each slab. Must be greater than zero.
        ///     This is not the maximum size of a single allocation: every rental also needs
        ///     <c>alignment - 1</c> bytes of padding plus a <see cref="nint" /> header, so the
        ///     largest single allocation is <c>maxLength - (alignment - 1)</c>. With the default
        ///     alignment of 8 that is <c>maxLength - 7</c> bytes; with an alignment of 16 it is
        ///     <c>maxLength - 15</c> bytes. Requesting more makes <see cref="Rent(int, int)" />
        ///     throw <see cref="ArgumentOutOfRangeException" />.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to keep in the free list. Must be non‑negative.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="maxLength" /> is less than or
        ///     equal to zero or <paramref name="maxFreeSlabs" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the slab size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeLinearMemoryPool Create(int maxLength, int maxFreeSlabs)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(maxLength, ExceptionArgument.maxLength);
            ThrowHelpers.ThrowIfNegative(maxFreeSlabs, ExceptionArgument.maxFreeSlabs);
            var size = Unsafe.SizeOf<nint>() + maxLength;
            var slab = CreateSlab(size);
            slab->Next = slab;
            slab->Previous = slab;
            return new UnsafeLinearMemoryPool(slab, null, 1, 0, maxFreeSlabs, size);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeLinearMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeLinearMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeLinearMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeLinearMemoryPool left, UnsafeLinearMemoryPool right) => !left.Equals(right);

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
        ///     Gets the maximum user data length that can be allocated with the specified alignment.
        /// </summary>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>The maximum length in bytes, or 0 if even a zero‑length allocation is not possible.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="alignment" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int GetMaxLength(int alignment)
        {
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2(Usize.FromI32Unchecked(alignment), ExceptionArgument.alignment);
            alignment = Math.Max(alignment, I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<nint>()));
            var byteOffset = alignment.WrappingSub(1).WrappingAdd(Unsafe.SizeOf<nint>());
            return Math.Max(0, _size.WrappingSub(byteOffset));
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
                    Initialize(temp);
                    temp->Next = _freeList;
                    _freeList = temp;
                    _freeSlabs = _freeSlabs.WrappingAdd(1);
                }
            }

            Initialize(node);
            node->Next = node;
            node->Previous = node;
            _sentinel = node;
        }

        /// <summary>
        ///     Allocates a memory block of the specified length with the specified alignment.
        /// </summary>
        /// <param name="length">
        ///     The number of bytes to allocate. Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The alignment, in bytes, for the allocated block. Must be a power of two.
        /// </param>
        /// <returns>A pointer to the allocated memory block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is
        ///     negative or <paramref name="length" /> exceeds the maximum allowed within a slab.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length, int alignment)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2(Usize.FromI32Unchecked(alignment), ExceptionArgument.alignment);
            alignment = Math.Max(alignment, I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<nint>()));
            var byteOffset = alignment.WrappingSub(1).WrappingAdd(Unsafe.SizeOf<nint>());
            var bytes = length.WrappingAdd(byteOffset);
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(bytes), U32.FromI32Unchecked(_size), ExceptionArgument.length);
            var slab = _sentinel;
            if (U64.FromI32Unchecked(slab->Length).WrappingAdd(U64.FromI32Unchecked(bytes)) > U64.FromI32Unchecked(_size))
            {
                if (_freeSlabs == 0)
                {
                    slab = CreateSlab(_size);
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

            var startAddress = Isize.FromPtrUnchecked(slab).WrappingAdd(Isize.FromI32Unchecked(Unsafe.SizeOf<MemorySlab>())).WrappingAdd(Isize.FromI32Unchecked(slab->Length));
            var result = (void*)NativeMemoryAllocator.AlignUp(Usize.FromIsizeUnchecked(startAddress.WrappingAdd(Isize.FromI32Unchecked(Unsafe.SizeOf<nint>()))), Usize.FromI32Unchecked(alignment));
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = Isize.FromPtrUnchecked(slab);
            slab->Count = slab->Count.WrappingAdd(1);
            slab->Length = slab->Length.WrappingAdd(bytes);
            return result;
        }

        /// <summary>
        ///     Allocates a memory block large enough to hold the specified number of elements of type
        ///     <typeparamref name="T" />, using the natural alignment of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the elements.</typeparam>
        /// <param name="elementCount">
        ///     The number of elements to allocate. Must be non‑negative.
        /// </param>
        /// <returns>A pointer to the allocated memory block, typed as <typeparamref name="T" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="elementCount" /> is negative or the
        ///     required byte length exceeds the maximum allowed within a slab.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte length exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent<T>(int elementCount) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(elementCount, ExceptionArgument.elementCount);
            return (T*)Rent(elementCount * Unsafe.SizeOf<T>(), I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<T>()));
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var slab = (MemorySlab*)Unsafe.Subtract(ref Unsafe.AsRef<nint>(ptr), 1);
            slab->Count = slab->Count.WrappingSub(1);
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
                        _freeSlabs = _freeSlabs.WrappingAdd(1);
                    }

                    _slabs = _slabs.WrappingSub(1);
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
                var slab = CreateSlab(_size);
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
        /// <param name="size">The number of bytes in the slab data region, excluding the slab header.</param>
        /// <returns>A pointer to the newly allocated and initialized slab.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MemorySlab* CreateSlab(int size)
        {
            var slab = (MemorySlab*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(Unsafe.SizeOf<MemorySlab>()).WrappingAdd(Usize.FromI32Unchecked(size)), NativeMemoryAllocator.AlignOf<nint>());
            Initialize(slab);
            return slab;
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="slab">A pointer to the slab to initialize.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(MemorySlab* slab)
        {
            slab->Count = 0;
            slab->Length = 0;
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
            ///     Gets the number of active allocations in this slab.
            /// </summary>
            public int Count;

            /// <summary>
            ///     Gets the number of bytes currently allocated from this slab.
            /// </summary>
            public int Length;
        }
    }
}