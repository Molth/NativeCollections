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
    [UnsafeCollection(FromType.None)]
    [BindingType(typeof(UnsafeU32MemoryPool))]
    public unsafe struct UnsafeU32MemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeU32MemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private UnsafeU32MemoryPool _handle;

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
        ///     Gets the total number of elements in all the dimensions of the instance.
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
        ///     Initializes a new instance of the this class
        ///     with the specified maximum free slabs,
        ///     using the natural length and alignment of type <typeparamref name="T" />.
        /// </summary>
        /// <remarks>
        ///     Each slab contains exactly 32 nodes,
        ///     as the allocation bitmap is stored as a <see cref="uint" />.
        /// </remarks>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain in the free list.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxFreeSlabs" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeU32MemoryPool(int maxFreeSlabs) => _handle = new UnsafeU32MemoryPool(Unsafe.SizeOf<T>(), maxFreeSlabs, (int)NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Initializes a new instance of the this class
        ///     with the specified node length, maximum free slabs, and alignment.
        /// </summary>
        /// <remarks>
        ///     Each slab contains exactly 32 nodes,
        ///     as the allocation bitmap is stored as a <see cref="uint" />.
        /// </remarks>
        /// <param name="length">
        ///     The length (in bytes) of the data region of each node.
        ///     Must be at least <see cref="Unsafe.SizeOf{T}" />.
        /// </param>
        /// <param name="maxFreeSlabs">
        ///     The maximum number of free slabs to retain in the free list.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="alignment">
        ///     The required alignment, in bytes, for allocations.
        ///     Must be a power of two and at least <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> is less than <see cref="Unsafe.SizeOf{T}" />,
        ///     or if <paramref name="maxFreeSlabs" /> is negative,
        ///     or if <paramref name="alignment" /> is less than <see cref="NativeMemoryAllocator.AlignOf{T}" />.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="alignment" /> is not a power of two.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeU32MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfLessThan(length, Unsafe.SizeOf<T>(), ExceptionArgument.length);
            ThrowHelpers.ThrowIfLessThan(alignment, (int)NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _handle = new UnsafeU32MemoryPool(length, maxFreeSlabs, alignment);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeU32MemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeU32MemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeU32MemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeU32MemoryPool<T> left, UnsafeU32MemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeU32MemoryPool<T> left, UnsafeU32MemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle.Clear();

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle.Clear(capacity);

        /// <summary>
        ///     Retrieves a buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent() => (T*)_handle.Rent();

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle.Return(ptr);

        /// <summary>
        ///     Ensures that the capacity of this is at least the specified <paramref name="capacity" />.
        ///     If the current capacity of this is less than specified <paramref name="capacity" />,
        ///     the capacity is increased to at least <paramref name="capacity" />.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle.EnsureCapacity(capacity);

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle.TrimExcess();

        /// <summary>
        ///     Trims the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle.TrimExcess(capacity);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeU32MemoryPool<T> Empty => default;
    }
}