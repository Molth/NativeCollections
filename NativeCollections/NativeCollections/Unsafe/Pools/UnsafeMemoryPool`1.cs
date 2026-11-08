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
    [BindingType(typeof(UnsafeMemoryPool))]
    public unsafe struct UnsafeMemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeMemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private UnsafeMemoryPool _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets the total number of slabs currently allocated in the pool.
        /// </summary>
        public readonly int Slabs => _handle.Slabs;

        /// <summary>
        ///     Gets the number of slabs that are currently free and available for reuse.
        /// </summary>
        public readonly int FreeSlabs => _handle.FreeSlabs;

        /// <summary>
        ///     Gets the maximum number of free slabs that can be retained before excess slabs are freed.
        /// </summary>
        public readonly int MaxFreeSlabs => _handle.MaxFreeSlabs;

        /// <summary>
        ///     Gets the number of nodes in each slab.
        /// </summary>
        public readonly int Size => _handle.Size;

        /// <summary>
        ///     Gets the length in bytes of the data region within each node.
        /// </summary>
        public readonly int Length => _handle.Length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public readonly int Alignment => _handle.Alignment;

        /// <summary>
        ///     Gets the aligned length (in bytes) of the data portion of each node after alignment.
        /// </summary>
        public readonly int AlignedLength => _handle.AlignedLength;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryPool(UnsafeMemoryPool handle) => _handle = handle;

        /// <summary>
        ///     Creates a memory pool using the natural alignment and node length of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="size">
        ///     The number of nodes each slab can hold. Must be greater than zero.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="size" /> is less than or equal
        ///     to zero or <paramref name="maxFreeSlabs" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the aligned node size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryPool<T> Create(int size, int maxFreeSlabs)
        {
            var handle = UnsafeMemoryPool.Create(size, Unsafe.SizeOf<T>(), maxFreeSlabs, I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<T>()));
            return new UnsafeMemoryPool<T>(handle);
        }

        /// <summary>
        ///     Creates a memory pool with the specified slab capacity, node length,
        ///     maximum free slabs, and alignment.
        /// </summary>
        /// <param name="size">
        ///     The number of nodes each slab can hold. Must be greater than zero.
        /// </param>
        /// <param name="length">
        ///     The length (in bytes) of the data region of each node. Must be at least <see cref="Unsafe.SizeOf{T}" />.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain. Must be non-negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment, in bytes. Must be a power of two and
        ///     at least <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="size" /> is less than or equal to zero, <paramref name="maxFreeSlabs" />
        ///     is negative, <paramref name="length" /> is less than <see cref="Unsafe.SizeOf{T}" />, or
        ///     <paramref name="alignment" /> is less than <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the aligned node size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryPool<T> Create(int size, int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfLessThan(length, Unsafe.SizeOf<T>(), ExceptionArgument.length);
            ThrowHelpers.ThrowIfLessThan(alignment, I32.FromUsizeUnchecked(NativeMemoryAllocator.AlignOf<T>()), ExceptionArgument.alignment);
            return new UnsafeMemoryPool<T>(UnsafeMemoryPool.Create(size, length, maxFreeSlabs, alignment));
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => SR.Format("UnsafeMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryPool<T> left, UnsafeMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryPool<T> left, UnsafeMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle.Clear();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle.Clear(capacity);

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        /// <returns>A pointer to a buffer with <see cref="Length" /> bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent() => (T*)_handle.Rent();

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle.Return(ptr);

        /// <summary>
        ///     Ensures that this retains at least the specified number of free slabs, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle.EnsureCapacity(capacity);

        /// <summary>
        ///     Releases all free slabs from this.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle.TrimExcess();

        /// <summary>
        ///     Trims the retained free-slab capacity of this to the specified number of slabs.
        /// </summary>
        /// <param name="capacity">The maximum number of free slabs to retain.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle.TrimExcess(capacity);
    }
}