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
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeU32MemoryPool<>))]
    public readonly unsafe struct NativeU32MemoryPool<T> : IIsCreated, IDisposable, IEquatable<NativeU32MemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeU32MemoryPool<T>* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeU32MemoryPool(UnsafeU32MemoryPool<T>* handle) => _handle = handle;

        /// <summary>
        ///     Creates a 32-bit bitmap memory pool using the natural length and alignment of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain in the free list. Must be non‑negative.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="maxFreeSlabs" /> is negative.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the aligned node size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        /// <remarks>
        ///     Each slab contains exactly 32 nodes, as the allocation bitmap is stored as a <see cref="uint" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeU32MemoryPool<T> Create(int maxFreeSlabs)
        {
            var value = UnsafeU32MemoryPool<T>.Create(maxFreeSlabs);
            return new NativeU32MemoryPool<T>(Box.New(ref value));
        }

        /// <summary>
        ///     Creates a 32-bit bitmap memory pool with the specified node length,
        ///     maximum free slabs, and alignment.
        /// </summary>
        /// <param name="length">
        ///     The length (in bytes) of the data region of each node. Must be at least <see cref="Unsafe.SizeOf{T}" />.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain in the free list. Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment, in bytes, for allocations. Must be a power
        ///     of two and at least <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </param>
        /// <returns>A newly allocated memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is less than <see cref="Unsafe.SizeOf{T}" />,
        ///     <paramref name="maxFreeSlabs" /> is negative, or <paramref name="alignment" />
        ///     is less than <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <remarks>
        ///     Each slab contains exactly 32 nodes, as the allocation bitmap is stored as a <see cref="uint" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeU32MemoryPool<T> Create(int length, int maxFreeSlabs, int alignment)
        {
            var value = UnsafeU32MemoryPool<T>.Create(length, maxFreeSlabs, alignment);
            return new NativeU32MemoryPool<T>(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets the total number of slabs currently allocated in the pool.
        /// </summary>
        public int Slabs => _handle->Slabs;

        /// <summary>
        ///     Gets the number of slabs that are currently free and available for reuse.
        /// </summary>
        public int FreeSlabs => _handle->FreeSlabs;

        /// <summary>
        ///     Gets the maximum number of free slabs that can be retained before excess slabs are freed.
        /// </summary>
        public int MaxFreeSlabs => _handle->MaxFreeSlabs;

        /// <summary>
        ///     Gets the length in bytes of the data region within each node.
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public int Alignment => _handle->Alignment;

        /// <summary>
        ///     Gets the aligned length (in bytes) of the data portion of each node after alignment.
        /// </summary>
        public int AlignedLength => _handle->AlignedLength;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeU32MemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeU32MemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeU32MemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeU32MemoryPool<T> left, NativeU32MemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeU32MemoryPool<T> left, NativeU32MemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle->Clear(capacity);

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        /// <returns>A pointer to a buffer with <see cref="Length" /> bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent() => _handle->Rent();

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle->Return(ptr);

        /// <summary>
        ///     Ensures that this retains at least the specified number of free slabs, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Releases all free slabs from this.
        /// </summary>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the retained free-slab capacity of this to the specified number of slabs.
        /// </summary>
        /// <param name="capacity">The maximum number of free slabs to retain.</param>
        /// <returns>The new capacity of this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);
    }
}