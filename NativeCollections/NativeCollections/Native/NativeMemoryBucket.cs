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
        public NativeMemoryBucket(int capacity, int length, int alignment)
        {
            var value = new UnsafeMemoryBucket(capacity, length, alignment);
            _handle = Box.New(ref value);
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
        public NativeMemoryBucket(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            var value = new UnsafeMemoryBucket(capacity, length, alignment, allocator);
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _handle->Capacity;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Gets the alignment requirement (in bytes) for allocations managed by this pool.
        /// </summary>
        public int Alignment => _handle->Alignment;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeMemoryBucket";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeMemoryBucket left, NativeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeMemoryBucket left, NativeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent() => _handle->Rent();

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr) => _handle->Return(ptr);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeMemoryBucket Empty => default;
    }
}