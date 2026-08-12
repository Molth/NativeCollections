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
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        private readonly int _alignment;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        [NativePointer(typeof(void*))] private readonly nint* _buffer;

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
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
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
        ///     Initializes a new instance of this class
        ///     with the specified capacity, node length, and alignment,
        ///     using the default memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained in the bucket.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="length">
        ///     The size (in bytes) of each allocated buffer.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment (in bytes) for each buffer.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" />, <paramref name="length" />,
        ///     or <paramref name="alignment" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length, int alignment) : this(capacity, length, alignment, CustomMemoryAllocator.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified capacity, node length, alignment, and custom memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained in the bucket.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="length">
        ///     The size (in bytes) of each allocated buffer.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment (in bytes) for each buffer.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="allocator">The custom memory allocator to use for allocations and deallocations.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" />, <paramref name="length" />,
        ///     or <paramref name="alignment" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryBucket(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfNegative(alignment, ExceptionArgument.alignment);
            _capacity = capacity;
            _length = length;
            _alignment = alignment;
            _buffer = NativeMemoryAllocator.AlignedAllocZeroed<nint>((uint)capacity);
            _index = 0;
            _allocator = allocator;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeMemoryBucket";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryBucket left, UnsafeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose()
        {
            for (var i = _index; i < _capacity; ++i)
            {
                var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)i);
                if (UnsafeHelpers.IsNull(buffer))
                    break;
                _allocator.AlignedFree(buffer);
            }

            NativeMemoryAllocator.AlignedFree(_buffer);
        }

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* ptr = null;
            if (_index < _capacity)
            {
                ptr = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index);
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index++) = 0;
            }

            if (UnsafeHelpers.IsNull(ptr))
                ptr = _allocator.AlignedAlloc((uint)_length, (uint)_alignment);
            return ptr;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (_index != 0)
                Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)(--_index)) = (nint)ptr;
            else
                _allocator.AlignedFree(ptr);
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeMemoryBucket Empty => default;
    }
}