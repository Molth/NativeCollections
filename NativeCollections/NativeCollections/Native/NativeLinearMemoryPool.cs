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
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

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
        ///     Gets the maximum usable length (in bytes) of a user allocation that can fit within a single slab,
        ///     assuming the default alignment of <see cref="IntPtr" />.
        /// </summary>
        /// <remarks>
        ///     This value is determined by the total slab size minus the overhead required for the slab header and
        ///     the alignment adjustment for a <see cref="IntPtr" />‑aligned pointer. It represents the largest contiguous
        ///     block of memory that can be allocated in one slab when using the default alignment.
        /// </remarks>
        public int MaxLength => _handle->MaxLength;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified maximum allocation length
        ///     and the maximum number of free slabs to retain.
        /// </summary>
        /// <param name="maxLength">
        ///     The maximum size (in bytes) of a single allocation
        ///     that can be accommodated within a slab.
        ///     Must be greater than zero.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to keep in the free list.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxLength" /> is less than or equal to zero,
        ///     or if <paramref name="maxFreeSlabs" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinearMemoryPool(int maxLength, int maxFreeSlabs)
        {
            var value = new UnsafeLinearMemoryPool(maxLength, maxFreeSlabs);
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeLinearMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeLinearMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeLinearMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Gets the maximum user data length that can be allocated with the specified alignment.
        /// </summary>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>The maximum length in bytes, or 0 if even a zero‑length allocation is not possible.</returns>
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
        /// <param name="capacity">The minimum capacity to ensure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle->Clear(capacity);

        /// <summary>
        ///     Allocates a memory block of the specified length with the specified alignment.
        /// </summary>
        /// <param name="length">
        ///     The number of bytes to allocate.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The alignment, in bytes, for the allocated block.
        ///     Must be a power of two.
        /// </param>
        /// <returns>A pointer to the allocated memory block.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> or <paramref name="alignment" /> is negative,
        ///     or when <paramref name="length" /> exceeds the maximum allowed within a slab.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length, int alignment) => _handle->Rent(length, alignment);

        /// <summary>
        ///     Allocates a memory block large enough
        ///     to hold the specified number of elements of type <typeparamref name="T" />,
        ///     using the natural alignment of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the elements.</typeparam>
        /// <param name="elementCount">
        ///     The number of elements to allocate.
        ///     Must be non‑negative.
        /// </param>
        /// <returns>A pointer to the allocated memory block, typed as <typeparamref name="T" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="elementCount" /> is negative,
        ///     or when the required byte length exceeds the maximum allowed within a slab.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent<T>(int elementCount) where T : unmanaged => _handle->Rent<T>(elementCount);

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr) => _handle->Return(ptr);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeLinearMemoryPool Empty => default;
    }
}