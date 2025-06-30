using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory allocator
    /// </summary>
    [Customizable("public static void* AlignedAlloc(uint byteCount, uint alignment)", "public static void* AlignedAllocZeroed(uint byteCount, uint alignment)", "public static void AlignedFree(void* ptr)")]
    public static unsafe class NativeMemoryAllocator
    {
        /// <summary>
        ///     AlignedAlloc
        /// </summary>
        private static delegate* managed<uint, uint, void*> _alignedAlloc;

        /// <summary>
        ///     AlignedAllocZeroed
        /// </summary>
        private static delegate* managed<uint, uint, void*> _alignedAllocZeroed;

        /// <summary>
        ///     AlignedFree
        /// </summary>
        private static delegate* managed<void*, void> _alignedFree;

        /// <summary>
        ///     Abort
        /// </summary>
        private static delegate* managed<void> _abort;

        /// <summary>
        ///     Configures custom memory allocation handlers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<uint, uint, void*> alignedAlloc, delegate* managed<uint, uint, void*> alignedAllocZeroed, delegate* managed<void*, void> alignedFree, delegate* managed<void> abort = null)
        {
            _alignedAlloc = alignedAlloc;
            _alignedAllocZeroed = alignedAllocZeroed;
            _alignedFree = alignedFree;
            _abort = abort;
        }

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAlloc<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)AlignOf<T>();
            return (T*)AlignedAlloc(byteCount, alignment);
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="elementCount">The count, in elements, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAllocZeroed<T>(uint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * (uint)sizeof(T);
            var alignment = (uint)AlignOf<T>();
            return (T*)AlignedAllocZeroed(byteCount, alignment);
        }

        /// <summary>Allocates an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAlloc(uint byteCount, uint alignment)
        {
            void* ptr;
            var alignedAlloc = _alignedAlloc;
            if (alignedAlloc != null)
            {
                if (!BitOperationsHelpers.IsPow2(alignment))
                    throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
                ptr = alignedAlloc(byteCount, alignment);
                if (ptr == null)
                {
                    var abort = _abort;
                    if (abort != null)
                    {
                        abort();
                        return null;
                    }

                    throw new OutOfMemoryException();
                }

                return ptr;
            }

#if NET6_0_OR_GREATER
            try
            {
                return NativeMemory.AlignedAlloc(byteCount, alignment);
            }
            catch
            {
                var abort = _abort;
                if (abort != null)
                {
                    abort();
                    return null;
                }

                throw;
            }
#else
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var byteOffset = (nuint)alignment - 1 + (nuint)sizeof(nint);
            try
            {
                ptr = (void*)Marshal.AllocHGlobal((nint)(byteCount + (uint)byteOffset));
            }
            catch
            {
                var abort = _abort;
                if (abort != null)
                {
                    abort();
                    return null;
                }

                throw;
            }

            var result = (void*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = (nint)ptr;
            return result;
#endif
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAllocZeroed(uint byteCount, uint alignment)
        {
            void* ptr;
            var alignedAllocZeroed = _alignedAllocZeroed;
            if (alignedAllocZeroed != null)
            {
                if (!BitOperationsHelpers.IsPow2(alignment))
                    throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
                ptr = alignedAllocZeroed(byteCount, alignment);
                if (ptr == null)
                {
                    var abort = _abort;
                    if (abort != null)
                    {
                        abort();
                        return null;
                    }

                    throw new OutOfMemoryException();
                }

                return ptr;
            }

            var alignedAlloc = _alignedAlloc;
            if (alignedAlloc != null)
            {
                if (!BitOperationsHelpers.IsPow2(alignment))
                    throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
                ptr = alignedAlloc(byteCount, alignment);
                if (ptr == null)
                {
                    var abort = _abort;
                    if (abort != null)
                    {
                        abort();
                        return null;
                    }

                    throw new OutOfMemoryException();
                }

                return ptr;
            }

#if NET6_0_OR_GREATER
            try
            {
                ptr = NativeMemory.AlignedAlloc(byteCount, alignment);
            }
            catch
            {
                var abort = _abort;
                if (abort != null)
                {
                    abort();
                    return null;
                }

                throw;
            }

            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(ptr), 0, byteCount);
            return ptr;
#else
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var byteOffset = (nuint)alignment - 1 + (nuint)sizeof(nint);
            try
            {
                ptr = (void*)Marshal.AllocHGlobal((nint)(byteCount + (uint)byteOffset));
            }
            catch
            {
                var abort = _abort;
                if (abort != null)
                {
                    abort();
                    return null;
                }

                throw;
            }

            var result = (void*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = (nint)ptr;
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(result), 0, byteCount);
            return result;
#endif
        }

        /// <summary>Frees an aligned block of memory.</summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AlignedFree(void* ptr)
        {
            var alignedFree = _alignedFree;
            if (alignedFree != null)
            {
                alignedFree(ptr);
                return;
            }

#if NET6_0_OR_GREATER
            NativeMemory.AlignedFree(ptr);
#else
            if (ptr == null)
                return;
            Marshal.FreeHGlobal(Unsafe.Subtract(ref Unsafe.AsRef<nint>(ptr), 1));
#endif
        }

        /// <summary>
        ///     Copies bytes from the source address to the destination address without assuming architecture dependent alignment
        ///     of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* destination, void* source, uint byteCount) => Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(destination), ref Unsafe.AsRef<byte>(source), byteCount);

        /// <summary>
        ///     Copies bytes from the source address to the destination address without assuming architecture dependent alignment
        ///     of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref byte destination, ref byte source, uint byteCount) => Unsafe.CopyBlockUnaligned(ref destination, ref source, byteCount);

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(void* destination, void* source, uint byteCount)
        {
#if NET7_0_OR_GREATER
            NativeMemory.Copy(source, destination, byteCount);
#else
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
#endif
        }

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(ref byte destination, ref byte source, uint byteCount)
        {
            fixed (void* pinnedDestination = &destination)
            {
                fixed (void* pinnedSource = &source)
                {
                    Move(pinnedDestination, pinnedSource, byteCount);
                }
            }
        }

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial value
        ///     without assuming architecture dependent alignment of the address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(void* startAddress, byte value, uint byteCount) => Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(startAddress), value, byteCount);

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial value
        ///     without assuming architecture dependent alignment of the address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ref byte startAddress, byte value, uint byteCount) => Unsafe.InitBlockUnaligned(ref startAddress, value, byteCount);

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(void* left, void* right, uint byteCount)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                throw new ArgumentNullException(left == null ? nameof(left) : nameof(right));
            return SpanHelpers.Compare(ref Unsafe.AsRef<byte>(left), ref Unsafe.AsRef<byte>(right), byteCount);
        }

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(ref byte left, ref byte right, uint byteCount)
        {
            if (Unsafe.IsNullRef(ref left) && Unsafe.IsNullRef(ref right))
                return true;
            if (Unsafe.IsNullRef(ref left) || Unsafe.IsNullRef(ref right))
                throw new ArgumentNullException(Unsafe.IsNullRef(ref left) ? nameof(left) : nameof(right));
            return SpanHelpers.Compare(ref left, ref right, byteCount);
        }

        /// <summary>Rounds a size up to the specified alignment boundary.</summary>
        /// <param name="size">The size, in bytes, to align.</param>
        /// <param name="alignment">The alignment boundary.</param>
        /// <returns>The aligned size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignUp(nuint size, nuint alignment) => (size + (alignment - 1)) & ~(alignment - 1);

        /// <summary>Rounds a size down to the specified alignment boundary.</summary>
        /// <param name="size">The size, in bytes, to align.</param>
        /// <param name="alignment">The alignment boundary.</param>
        /// <returns>The aligned size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignDown(nuint size, nuint alignment) => size - (size & (alignment - 1));

        /// <summary>
        ///     Gets the alignment, in bytes, of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type whose alignment is to be determined.</typeparam>
        /// <returns>The alignment, in bytes, of type <typeparamref name="T" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignOf<T>() where T : unmanaged => (nuint)sizeof(AlignOfHelper<T>) - (nuint)sizeof(T);

        /// <summary>
        ///     Determines whether the specified unmanaged type <typeparamref name="T" /> is naturally aligned,
        ///     meaning its size is a multiple of its alignment requirement.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to check for alignment.</typeparam>
        /// <returns><c>true</c> if the size of <typeparamref name="T" /> is a multiple of its alignment; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAligned<T>() where T : unmanaged => (nuint)sizeof(T) % AlignOf<T>() == 0;

        /// <summary>
        ///     Determines whether a pointer is correctly aligned for the specified unmanaged type <typeparamref name="T" />,
        ///     optionally taking into account the number of elements to be accessed.
        /// </summary>
        /// <typeparam name="T">
        ///     The unmanaged type whose alignment requirements are being enforced.
        /// </typeparam>
        /// <param name="ptr">
        ///     A pointer to the memory location to check for alignment.
        /// </param>
        /// <param name="elementCount">
        ///     The number of contiguous <typeparamref name="T" /> elements that will be accessed.
        ///     If greater than 1, this method also verifies that <typeparamref name="T" /> itself is naturally aligned
        ///     (its size is a multiple of its alignment). If 1 or 0, only the pointer’s alignment is checked.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the pointer address is a multiple of <typeparamref name="T" />'s alignment boundary,
        ///     and—when <paramref name="elementCount" /> &gt; 1—if <typeparamref name="T" /> is naturally aligned; otherwise,
        ///     <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAligned<T>(void* ptr, nuint elementCount) where T : unmanaged => elementCount > 1 ? IsAligned<T>() && (nint)ptr % (nint)AlignOf<T>() == 0 : (nint)ptr % (nint)AlignOf<T>() == 0;

        /// <summary>Helper structure for calculating type alignment.</summary>
        /// <typeparam name="T">The unmanaged type being measured.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        private struct AlignOfHelper<T> where T : unmanaged
        {
            /// <summary>
            ///     Padding byte used for alignment calculation.
            /// </summary>
            private byte _dummy;

            /// <summary>
            ///     The typed data used for alignment measurement.
            /// </summary>
            private T _data;
        }
    }
}