using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a resource pool that enables reusing instances of native-arrays.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryBucket : IIsCreated, IDisposable, IEquatable<UnsafeMemoryBucket>
    {
        /// <summary>
        ///     Gets the maximum number of buffers that can be retained in the bucket.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets the size in bytes of each buffer.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        private readonly int _alignment;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        [BindingType(typeof(void*))] private readonly nint* _buffer;

        /// <summary>
        ///     The current index.
        /// </summary>
        private int _index;

        /// <summary>
        ///     The custom memory allocator used for allocating and deallocating buffers.
        /// </summary>
        private readonly CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets the maximum number of buffers that can be retained in the bucket.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Gets the size in bytes of each buffer.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public readonly int Alignment => _alignment;

        /// <summary>
        ///     The custom memory allocator used for allocating and deallocating buffers.
        /// </summary>
        public readonly CustomMemoryAllocator Allocator => _allocator;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="capacity">The maximum number of buffers that can be retained in the bucket.</param>
        /// <param name="length">The total number of elements the internal data structure can hold.</param>
        /// <param name="alignment">The alignment requirement (in bytes) for allocations managed by the bucket.</param>
        /// <param name="buffer">The native buffer that backs the bucket.</param>
        /// <param name="index">The index of the next free slot within the buffer.</param>
        /// <param name="allocator">The custom memory allocator used for allocating and deallocating buffers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryBucket(int capacity, int length, int alignment, nint* buffer, int index, CustomMemoryAllocator allocator)
        {
            _capacity = capacity;
            _length = length;
            _alignment = alignment;
            _buffer = buffer;
            _index = index;
            _allocator = allocator;
        }

        /// <summary>
        ///     Creates a bucket using the default memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained in the bucket. Must be non‑negative.
        /// </param>
        /// <param name="length">
        ///     The size (in bytes) of each allocated buffer. Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment (in bytes) for each buffer. Must be a power of two.
        /// </param>
        /// <returns>A newly allocated bucket.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" />, <paramref name="length" />, or <paramref name="alignment" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryBucket Create(int capacity, int length, int alignment) => Create(capacity, length, alignment, CustomMemoryAllocator.Default);

        /// <summary>
        ///     Creates a bucket using a custom memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained in the bucket. Must be non‑negative.
        /// </param>
        /// <param name="length">
        ///     The size (in bytes) of each allocated buffer. Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment (in bytes) for each buffer. Must be a power of two.
        /// </param>
        /// <param name="allocator">The custom memory allocator to use for allocations and deallocations.</param>
        /// <returns>A newly allocated bucket.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" />, <paramref name="length" />, or <paramref name="alignment" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryBucket Create(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            ThrowHelpers.ThrowIfAlignmentNotBePow2(Usize.FromI32Unchecked(alignment), ExceptionArgument.alignment);
            var buffer = NativeMemoryAllocator.AlignedAllocZeroed<nint>(Usize.FromI32Unchecked(capacity));
            return new UnsafeMemoryBucket(capacity, length, alignment, buffer, 0, allocator);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeMemoryBucket";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose()
        {
            for (var i = 0; i < _capacity; i = i.WrappingAdd(1))
            {
                var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(i));
                if (buffer == null)
                    continue;

                _allocator.AlignedFree(buffer);
            }

            NativeMemoryAllocator.AlignedFree(_buffer);
        }

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        /// <returns>A pointer to a buffer with <see cref="Length" /> bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* ptr = null;
            if (_index < _capacity)
            {
                ptr = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(_index));
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked((_index = _index.WrappingAdd(1)).WrappingSub(1))) = Isize.FromI32Unchecked(0);
            }

            if (ptr == null)
                ptr = _allocator.AlignedAlloc(Usize.FromI32Unchecked(_length), Usize.FromI32Unchecked(_alignment));

            return ptr;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (_index != 0)
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(_index = _index.WrappingSub(1))) = Isize.FromPtrUnchecked(ptr);
            else
                _allocator.AlignedFree(ptr);
        }
    }
}