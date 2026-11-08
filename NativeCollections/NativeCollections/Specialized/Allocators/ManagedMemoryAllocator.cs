using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     This class contains methods that are mainly used to manage pinned-managed memory.
    /// </summary>
    [BindingType(typeof(ArrayPool<DummyByteHelper>))]
    public static unsafe class ManagedMemoryAllocator
    {
        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the block to allocate.</typeparam>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        /// <exception cref="OverflowException">
        ///     Thrown when the required allocation size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAlloc<T>(nuint elementCount) where T : unmanaged
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
        ///     Thrown when the required allocation size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAllocZeroed<T>(nuint elementCount) where T : unmanaged
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
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required allocation size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* AlignedAlloc(nuint byteCount, nuint alignment)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var byteOffset = alignment.WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<GCHandle>()).WrappingSub(Usize.FromI32Unchecked(1)));
            var allocationSize = I32.FromUsize(byteCount + byteOffset);
            var array = ArrayPool<DummyByteHelper>.Shared.Rent(allocationSize);
            var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            ref var reference = ref MemoryMarshalHelpers.GetArrayDataReference(array);
            var ptr = Unsafe.AsPointer(ref reference);
            var result = (void*)Isize.FromUsizeUnchecked((Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(ptr)) + byteOffset) & ~alignment.WrappingSub(Usize.FromI32Unchecked(1)));
            Unsafe.WriteUnaligned(UnsafeHelpers.SubtractByteOffset(result, Isize.FromI32Unchecked(Unsafe.SizeOf<GCHandle>())), gcHandle);
            return result;
        }

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">
        ///     The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.
        /// </param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the required allocation size exceeds <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* AlignedAllocZeroed(nuint byteCount, nuint alignment)
        {
            var result = AlignedAlloc(byteCount, alignment);
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(result), U8.FromI32Unchecked(0), byteCount);
            return result;
        }

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AlignedFree(void* ptr)
        {
            if (ptr == null)
                return;

            var gcHandle = Unsafe.ReadUnaligned<GCHandle>(UnsafeHelpers.SubtractByteOffset(ptr, Isize.FromI32Unchecked(Unsafe.SizeOf<GCHandle>())));
            var array = (DummyByteHelper[])gcHandle.Target!;
            gcHandle.Free();
            ArrayPool<DummyByteHelper>.Shared.Return(array);
        }

        /// <summary>
        ///     Helper struct for byte-level memory operations with ArrayPool.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct DummyByteHelper
        {
            /// <summary>
            ///     Unused field to satisfy compiler requirements.
            /// </summary>
            private readonly byte _dummy;
        }
    }
}