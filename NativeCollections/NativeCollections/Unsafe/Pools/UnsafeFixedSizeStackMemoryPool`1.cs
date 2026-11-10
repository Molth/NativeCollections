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
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeStackMemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeFixedSizeStackMemoryPool<T>> where T : unmanaged
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
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="buffer">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="index">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="capacity">The total number of elements the internal data structure can hold.</param>
        /// <param name="count">The number of elements contained in this.</param>
        private UnsafeFixedSizeStackMemoryPool(T* buffer, int* index, int capacity, int count)
        {
            _buffer = buffer;
            _index = index;
            _capacity = capacity;
            _count = count;
        }

        /// <summary>
        ///     Creates a fixed-size stack memory pool with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeFixedSizeStackMemoryPool<T> Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), alignment);
            var buffer = (T*)NativeMemoryAllocator.AlignedAlloc(bufferByteCount + Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<int>()), alignment);
            var index = UnsafeHelpers.AddByteOffset<int>(buffer, Isize.FromUsizeUnchecked(bufferByteCount));
            for (var i = 0; i < capacity; i = i.WrappingAdd(1))
                Unsafe.Add(ref Unsafe.AsRef<int>(index), Isize.FromI32Unchecked(i)) = i;

            return new UnsafeFixedSizeStackMemoryPool<T>(buffer, index, capacity, capacity);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeFixedSizeStackMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeFixedSizeStackMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeFixedSizeStackMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeFixedSizeStackMemoryPool<T> left, UnsafeFixedSizeStackMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeFixedSizeStackMemoryPool<T> left, UnsafeFixedSizeStackMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

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