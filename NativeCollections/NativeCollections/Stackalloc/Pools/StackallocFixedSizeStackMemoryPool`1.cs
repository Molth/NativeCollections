using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory pool that provides reusable fixed-size memory blocks.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type allocated by the pool.</typeparam>
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
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _count;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Calculates the minimum number of bytes required to store a specified number of
        ///     elements, taking into account alignment requirements for the underlying buffer.
        /// </summary>
        /// <param name="capacity">The number of elements to store. Must be non-negative.</param>
        /// <returns>
        ///     The minimum byte count needed to allocate a buffer capable of
        ///     holding <paramref name="capacity" /> elements with proper alignment.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteCount(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var nativeCapacity = Usize.FromI32Unchecked(capacity);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = NativeMemoryAllocator.AlignUp(nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), alignment);
            var byteCount = bufferByteCount + nativeCapacity * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()) + alignment.WrappingSub(Usize.FromI32Unchecked(1));
            return I32.FromUsize(byteCount);
        }

        /// <summary>
        ///     Initializes a new instance of this class that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage. It must be large enough
        ///     to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the stack can hold. Must be non-negative.
        /// </param>
        /// <returns>
        ///     A fixed-size stack memory pool that uses <paramref name="buffer" />
        ///     as its storage and holds up to <paramref name="capacity" /> elements.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        public static StackallocFixedSizeStackMemoryPool<T> FromBytes([MustBePinned] Span<byte> buffer, int capacity) => new(buffer, capacity);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the stack can hold.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" />
        ///     is too small to hold the required number of elements (including alignment padding).
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required byte count exceeds <see cref="int.MaxValue" />.
        /// </exception>
        private StackallocFixedSizeStackMemoryPool([MustBePinned] Span<byte> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, GetByteCount(capacity), ExceptionArgument.capacity);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), alignment);
            _buffer = (T*)NativeArray<byte>.FromBytes(buffer, alignment).Buffer;
            _index = UnsafeHelpers.AddByteOffset<int>(_buffer, Isize.FromUsizeUnchecked(bufferByteCount));
            _capacity = capacity;
            _count = capacity;
            for (var i = 0; i < _capacity; i = i.WrappingAdd(1))
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), Isize.FromI32Unchecked(i)) = i;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(StackallocFixedSizeStackMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocFixedSizeStackMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("StackallocFixedSizeStackMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocFixedSizeStackMemoryPool<T> left, StackallocFixedSizeStackMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(StackallocFixedSizeStackMemoryPool<T> left, StackallocFixedSizeStackMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset()
        {
            _count = _capacity;
            for (var i = 0; i < _capacity; i = i.WrappingAdd(1))
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), Isize.FromI32Unchecked(i)) = i;
        }

        /// <summary>
        ///     Attempts to retrieve a single block from the pool.
        /// </summary>
        /// <param name="ptr">
        ///     When this method returns, contains a pointer to the rented block if
        ///     the operation succeeds; otherwise, <see langword="null" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a buffer was successfully retrieved; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            var size = _count.WrappingSub(1);
            if (U32.FromI32Unchecked(size) >= U32.FromI32Unchecked(_capacity))
            {
                ptr = null;
                return false;
            }

            _count = size;
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_index), Isize.FromI32Unchecked(size));
            ptr = UnsafeHelpers.Add<T>(_buffer, Isize.FromI32Unchecked(index));
            return true;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously
        ///     obtained via <see cref="TryRent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var index = byteOffset / Isize.FromI32Unchecked(Unsafe.SizeOf<T>());
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), Isize.FromI32Unchecked((_count = _count.WrappingAdd(1)).WrappingSub(1))) = I32.FromIsizeUnchecked(index);
        }
    }
}