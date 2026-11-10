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
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeLinearMemoryPool))]
    public readonly unsafe struct NativeLinearMemoryPool : IIsCreated, IDisposable, IEquatable<NativeLinearMemoryPool>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeLinearMemoryPool* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeLinearMemoryPool(UnsafeLinearMemoryPool* handle) => _handle = handle;

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
        public static NativeLinearMemoryPool Create(int maxLength, int maxFreeSlabs)
        {
            var value = UnsafeLinearMemoryPool.Create(maxLength, maxFreeSlabs);
            return new NativeLinearMemoryPool(Box.New(ref value));
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
        ///     Gets the maximum usable length (in bytes) of a user allocation that can fit
        ///     within a single slab, assuming the default alignment of <see cref="IntPtr" />.
        /// </summary>
        /// <remarks>
        ///     This value is determined by the slab data region size minus the <c>alignment - 1</c> bytes
        ///     of padding and the <see cref="nint" /> header required for each allocation. It represents the largest
        ///     contiguous block of memory that can be allocated in one slab when using the default alignment.
        /// </remarks>
        public int MaxLength => _handle->MaxLength;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeLinearMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeLinearMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeLinearMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

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
        public int GetMaxLength(int alignment) => _handle->GetMaxLength(alignment);

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The maximum number of free slabs to retain.</param>
        /// <returns>The number of free slabs retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle->Clear(capacity);

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
        public void* Rent(int length, int alignment) => _handle->Rent(length, alignment);

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
        public T* Rent<T>(int elementCount) where T : unmanaged => _handle->Rent<T>(elementCount);

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr) => _handle->Return(ptr);

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