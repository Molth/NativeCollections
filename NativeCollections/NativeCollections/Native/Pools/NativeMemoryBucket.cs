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
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeMemoryBucket))]
    public readonly unsafe struct NativeMemoryBucket : IIsCreated, IDisposable, IEquatable<NativeMemoryBucket>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeMemoryBucket* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemoryBucket(UnsafeMemoryBucket* handle) => _handle = handle;

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
        ///     Thrown when <paramref name="capacity" /> or <paramref name="length" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryBucket Create(int capacity, int length, int alignment)
        {
            var value = UnsafeMemoryBucket.Create(capacity, length, alignment);
            return new NativeMemoryBucket(Box.New(ref value));
        }

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
        ///     Thrown when <paramref name="capacity" /> or <paramref name="length" /> is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryBucket Create(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            var value = UnsafeMemoryBucket.Create(capacity, length, alignment, allocator);
            return new NativeMemoryBucket(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets the maximum number of buffers that can be retained in the bucket.
        /// </summary>
        public int Capacity => _handle->Capacity;

        /// <summary>
        ///     Gets the size in bytes of each buffer.
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public int Alignment => _handle->Alignment;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeMemoryBucket";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeMemoryBucket left, NativeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeMemoryBucket left, NativeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        /// <returns>A pointer to a buffer with <see cref="Length" /> bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent() => _handle->Rent();

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr) => _handle->Return(ptr);
    }
}