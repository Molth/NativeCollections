using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory helpers
    /// </summary>
    internal static unsafe class NativeMemoryHelpers
    {
        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAlloc(uint byteCount, uint alignment)
        {
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var byteOffset = (nuint)alignment - 1 + (nuint)sizeof(nint);
            void* ptr;
            if ((ptr = NativeMemoryAllocator.Alloc(byteCount + (uint)byteOffset)) == null)
                return null;
            var result = (void*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            ((void**)result)[-1] = ptr;
            return result;
        }

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlignedFree(void* ptr)
        {
            if (ptr == null)
                return;
            NativeMemoryAllocator.Free(((void**)ptr)[-1]);
        }
    }
}