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
    [StackallocCollection(FromType.None)]
    public unsafe struct StackallocFixedSizeStackMemoryPool<T> : IIsCreated, IEquatable<StackallocFixedSizeStackMemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly int* _index;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of elements,
        ///     taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<T>()), alignment);
            return (int)(bufferByteCount + capacity * Unsafe.SizeOf<int>() + alignment - 1);
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        ///     It must be large enough to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the stack can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="capacity" /> is negative, or if <paramref name="buffer" /> is too small
        ///     to hold the required number of elements (including alignment padding).
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocFixedSizeStackMemoryPool([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<T>()), alignment);
            _buffer = (T*)NativeArray<byte>.Create(buffer, alignment).Buffer;
            _index = UnsafeHelpers.AddByteOffset<int>(_buffer, (nint)bufferByteCount);
            _capacity = capacity;
            _count = capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(StackallocFixedSizeStackMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is StackallocFixedSizeStackMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("StackallocFixedSizeStackMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(StackallocFixedSizeStackMemoryPool<T> left, StackallocFixedSizeStackMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(StackallocFixedSizeStackMemoryPool<T> left, StackallocFixedSizeStackMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _count = _capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            var size = _count - 1;
            if ((uint)size >= (uint)_capacity)
            {
                ptr = null;
                return false;
            }

            _count = size;
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)size);
            ptr = UnsafeHelpers.Add<T>(_buffer, index);
            return true;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="TryRent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var index = byteOffset / Unsafe.SizeOf<T>();
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)_count++) = (int)index;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static StackallocFixedSizeStackMemoryPool<T> Empty => default;
    }
}