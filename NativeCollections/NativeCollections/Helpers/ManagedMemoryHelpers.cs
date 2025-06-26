using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if !NET5_0_OR_GREATER
using System;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Managed memory helpers
    /// </summary>
    internal static unsafe class ManagedMemoryHelpers
    {
        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAlloc<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            return (T*)AlignedAlloc(byteCount, alignment);
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAllocZeroed<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)NativeMemoryAllocator.AlignOf<T>();
            return (T*)AlignedAllocZeroed(byteCount, alignment);
        }

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAlloc(uint byteCount, uint alignment)
        {
            var byteOffset = alignment - 1 + (uint)sizeof(nint);
            var array = ArrayPool<DummyByteHelper>.Shared.Rent((int)(byteCount + byteOffset));
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
#if NET5_0_OR_GREATER
            ref var reference = ref MemoryMarshal.GetArrayDataReference(array);
#else
            ref var reference = ref MemoryMarshal.GetReference(array.AsSpan());
#endif
            var ptr = (byte*)Unsafe.AsPointer(ref reference);
            var result = (byte*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            Unsafe.WriteUnaligned(UnsafeHelpers.SubtractByteOffset<byte>(result, sizeof(GCHandle)), gcHandle);
            return result;
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAllocZeroed(uint byteCount, uint alignment)
        {
            var byteOffset = alignment - 1 + (uint)sizeof(nint);
            var array = ArrayPool<DummyByteHelper>.Shared.Rent((int)(byteCount + byteOffset));
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
#if NET5_0_OR_GREATER
            ref var reference = ref MemoryMarshal.GetArrayDataReference(array);
#else
            ref var reference = ref MemoryMarshal.GetReference(array.AsSpan());
#endif
            var ptr = (byte*)Unsafe.AsPointer(ref reference);
            var result = (byte*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            Unsafe.WriteUnaligned(UnsafeHelpers.SubtractByteOffset<byte>(result, sizeof(GCHandle)), gcHandle);
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(result), 0, byteCount);
            return result;
        }

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlignedFree(void* ptr)
        {
            if (ptr == null)
                return;
            var gcHandle = Unsafe.ReadUnaligned<GCHandle>(UnsafeHelpers.SubtractByteOffset<byte>(ptr, sizeof(GCHandle)));
            var array = (DummyByteHelper[])gcHandle.Target!;
            gcHandle.Free();
            ArrayPool<DummyByteHelper>.Shared.Return(array);
        }

        /// <summary>
        ///     Helper struct for byte-level memory operations with ArrayPool.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct DummyByteHelper
        {
            /// <summary>
            ///     Unused field to satisfy compiler requirements.
            /// </summary>
            private byte _dummy;
        }
    }
}