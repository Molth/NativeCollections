using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    [BindingType(typeof(UnsafeMemoryPool))]
    public unsafe struct UnsafeMemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeMemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
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
        ///     Gets the number of elements.
        /// </summary>
        public readonly int Size => _handle.Size;

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
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryPool(int size, int maxFreeSlabs) => _handle = new UnsafeMemoryPool(size, Unsafe.SizeOf<T>(), maxFreeSlabs, (int)NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryPool(int size, int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfLessThan(length, Unsafe.SizeOf<T>(), ExceptionArgument.length);
            ThrowHelpers.ThrowIfLessThan(alignment, (int)NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _handle = new UnsafeMemoryPool(size, length, maxFreeSlabs, alignment);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryPool<T> left, UnsafeMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryPool<T> left, UnsafeMemoryPool<T> right) => !left.Equals(right);

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
        ///     Empty
        /// </summary>
        public static UnsafeMemoryPool<T> Empty => default;
    }
}