using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Box
    /// </summary>
    internal static unsafe class Box
    {
        /// <summary>Allocates an aligned block of memory of the specified value.</summary>
        /// <param name="value">The value to copy into the newly allocated memory.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        /// <exception cref="OutOfMemoryException">Allocating memory failed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* New<T>(ref T value) where T : unmanaged
        {
            var ptr = NativeMemoryAllocator.AlignedAlloc<T>(1);
            Unsafe.AsRef<T>(ptr) = value;
            return ptr;
        }

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Drop<T>(T* ptr) where T : unmanaged, IDisposable
        {
            if (UnsafeHelpers.IsNull(ptr))
                return;
            Unsafe.AsRef<T>(ptr).Dispose();
            NativeMemoryAllocator.AlignedFree(ptr);
        }

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
            if (UnsafeHelpers.IsNull(ptr))
                return;
            NativeMemoryAllocator.AlignedFree(ptr);
        }
    }
}