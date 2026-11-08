using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a custom memory allocator that uses user-provided callbacks and a context pointer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpecializedCollection(FromType.Standard)]
    [BindingType(typeof(CustomMemoryCallbacks))]
    public readonly unsafe struct CustomMemoryAllocator : IEquatable<CustomMemoryAllocator>
    {
        /// <summary>
        ///     The user-defined context pointer used by the callbacks.
        /// </summary>
        private readonly void* _user;

        /// <summary>
        ///     The memory callbacks used by the allocator.
        /// </summary>
        private readonly CustomMemoryCallbacks _callbacks;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="user">The user-defined context pointer passed to the memory callbacks.</param>
        /// <param name="alignedAlloc">The callback used to allocate aligned memory.</param>
        /// <param name="alignedAllocZeroed">The callback used to allocate zeroed aligned memory.</param>
        /// <param name="alignedFree">The callback used to free aligned memory.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomMemoryAllocator(void* user, delegate* managed<void*, nuint, nuint, void*> alignedAlloc, delegate* managed<void*, nuint, nuint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree)
        {
            _user = user;
            _callbacks = CustomMemoryCallbacks.From(alignedAlloc, alignedAllocZeroed, alignedFree);
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="user">The user-defined context pointer passed to the memory callbacks.</param>
        /// <param name="callbacks">The memory callbacks used by the allocator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CustomMemoryAllocator(void* user, CustomMemoryCallbacks callbacks)
        {
            _user = user;
            _callbacks = callbacks;
        }

        /// <summary>
        ///     Creates an allocator from a user context pointer and callback function pointers.
        /// </summary>
        /// <param name="user">The user-defined context pointer passed to the memory callbacks.</param>
        /// <param name="alignedAlloc">The callback used to allocate aligned memory.</param>
        /// <param name="alignedAllocZeroed">The callback used to allocate zeroed aligned memory.</param>
        /// <param name="alignedFree">The callback used to free aligned memory.</param>
        /// <returns>An allocator that uses <paramref name="user" /> and the specified callbacks.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CustomMemoryAllocator From(void* user, delegate* managed<void*, nuint, nuint, void*> alignedAlloc, delegate* managed<void*, nuint, nuint, void*> alignedAllocZeroed, delegate* managed<void*, void*, void> alignedFree) => new(user, alignedAlloc, alignedAllocZeroed, alignedFree);

        /// <summary>
        ///     Creates an allocator from a user context pointer and callback set.
        /// </summary>
        /// <param name="user">The user-defined context pointer passed to the memory callbacks.</param>
        /// <param name="callbacks">The memory callbacks used by the allocator.</param>
        /// <returns>An allocator that uses <paramref name="user" /> and <paramref name="callbacks" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CustomMemoryAllocator From(void* user, CustomMemoryCallbacks callbacks) => new(user, callbacks);

        /// <summary>
        ///     Gets the user-provided context pointer that is passed to the memory callbacks.
        /// </summary>
        public void* User => _user;

        /// <summary>
        ///     Gets the memory callbacks used by the allocator.
        /// </summary>
        public CustomMemoryCallbacks Callbacks => _callbacks;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(CustomMemoryAllocator other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is CustomMemoryAllocator other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "CustomMemoryAllocator";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(CustomMemoryAllocator left, CustomMemoryAllocator right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(CustomMemoryAllocator left, CustomMemoryAllocator right) => !left.Equals(right);

        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the block to allocate.</typeparam>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the byte count required for <paramref name="elementCount" /> elements
        ///     exceeds the maximum native-sized value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAlloc<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAlloc(byteCount, NativeMemoryAllocator.AlignOf<T>());
        }

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the block to allocate.</typeparam>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the byte count required for <paramref name="elementCount" /> elements
        ///     exceeds the maximum native-sized value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* AlignedAllocZeroed<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAllocZeroed(byteCount, NativeMemoryAllocator.AlignOf<T>());
        }

        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">
        ///     The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.
        /// </param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAlloc(nuint byteCount, nuint alignment) => _callbacks.AlignedAlloc(_user, byteCount, alignment);

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">
        ///     The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.
        /// </param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* AlignedAllocZeroed(nuint byteCount, nuint alignment) => _callbacks.AlignedAllocZeroed(_user, byteCount, alignment);

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlignedFree(void* ptr) => _callbacks.AlignedFree(_user, ptr);

        /// <summary>
        ///     Gets a memory allocator that uses the default native memory allocation functions.
        /// </summary>
        public static CustomMemoryAllocator Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return From(null, &AlignedAlloc, &AlignedAllocZeroed, &AlignedFree);

                static void* AlignedAlloc(void* user, nuint byteCount, nuint alignment) => NativeMemoryAllocator.AlignedAlloc(byteCount, alignment);
                static void* AlignedAllocZeroed(void* user, nuint byteCount, nuint alignment) => NativeMemoryAllocator.AlignedAllocZeroed(byteCount, alignment);
                static void AlignedFree(void* user, void* ptr) => NativeMemoryAllocator.AlignedFree(ptr);
            }
        }
    }
}