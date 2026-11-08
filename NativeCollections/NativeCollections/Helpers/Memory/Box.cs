using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides allocation and deallocation utilities for native memory, including copying values.
    /// </summary>
    internal static unsafe class Box
    {
        /// <summary>
        ///     Allocates an aligned block of memory of the specified value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to copy into the allocated memory.</typeparam>
        /// <param name="value">The value to copy into the newly allocated memory.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* New<T>(ref T value) where T : unmanaged
        {
            var ptr = NativeMemoryAllocator.AlignedAlloc<T>(Usize.FromI32Unchecked(1));
            Unsafe.AsRef<T>(ptr) = value;
            return ptr;
        }

        /// <summary>
        ///     Allocates an aligned block of memory of the specified value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to copy into the allocated memory.</typeparam>
        /// <param name="value">The value to copy into the newly allocated memory.</param>
        /// <param name="alignment">
        ///     The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.
        /// </param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="alignment" /> is less than
        ///     the natural alignment of <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* New<T>(ref T value, nuint alignment) where T : unmanaged
        {
            ThrowHelpers.ThrowIfLessThan(alignment, NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            var ptr = (T*)NativeMemoryAllocator.AlignedAlloc(Usize.FromI32Unchecked(Unsafe.SizeOf<T>()), alignment);
            Unsafe.AsRef<T>(ptr) = value;
            return ptr;
        }

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to dispose before its memory is freed.</typeparam>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Drop<T>(T* ptr) where T : unmanaged, IDisposable
        {
            if (ptr == null)
                return;

            Unsafe.AsRef<T>(ptr).Dispose();
            NativeMemoryAllocator.AlignedFree(ptr);
        }
    }
}