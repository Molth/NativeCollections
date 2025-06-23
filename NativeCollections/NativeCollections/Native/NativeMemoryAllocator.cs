using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory allocator
    /// </summary>
    [Customizable("public static void* Alloc(uint byteCount)", "public static void* AllocZeroed(uint byteCount)", "public static void Free(void* ptr)")]
    public static unsafe class NativeMemoryAllocator
    {
        /// <summary>
        ///     Alloc
        /// </summary>
        private static delegate* managed<uint, void*> _alloc;

        /// <summary>
        ///     AllocZeroed
        /// </summary>
        private static delegate* managed<uint, void*> _allocZeroed;

        /// <summary>
        ///     Free
        /// </summary>
        private static delegate* managed<void*, void> _free;

        /// <summary>
        ///     Abort
        /// </summary>
        private static delegate* managed<void> _abort;

        /// <summary>
        ///     Configures custom memory allocation handlers.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<uint, void*> alloc, delegate* managed<uint, void*> allocZeroed, delegate* managed<void*, void> free, delegate* managed<void> abort = null)
        {
            _alloc = alloc;
            _allocZeroed = allocZeroed;
            _free = free;
            _abort = abort;
        }

        /// <summary>Allocates a block of memory of the specified size, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <returns>A pointer to the allocated block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(uint byteCount)
        {
            var alloc = _alloc;
            if (alloc != null)
            {
                var ptr = alloc(byteCount);
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
                return NativeMemory.Alloc(byteCount);
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
            try
            {
                return (void*)Marshal.AllocHGlobal((nint)byteCount);
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
#endif
        }

        /// <summary>Allocates and zeroes a block of memory of the specified size, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <returns>A pointer to the allocated and zeroed block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(uint byteCount)
        {
            void* ptr;
            var allocZeroed = _allocZeroed;
            if (allocZeroed != null)
            {
                ptr = allocZeroed(byteCount);
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

            var alloc = _alloc;
            if (alloc != null)
            {
                ptr = alloc(byteCount);
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

                Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
                return ptr;
            }

#if NET6_0_OR_GREATER
            try
            {
                return NativeMemory.AllocZeroed(byteCount, 1);
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
            try
            {
                ptr = (void*)Marshal.AllocHGlobal((nint)byteCount);
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

            Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
            return ptr;
#endif
        }

        /// <summary>Frees a block of memory.</summary>
        /// <param name="ptr">A pointer to the block of memory that should be freed.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
            var free = _free;
            if (free != null)
            {
                free(ptr);
                return;
            }

#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif
        }

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
            var ptr = Alloc(byteCount + (uint)byteOffset);
            if (ptr == null)
                return null;
            var result = (void*)(((nint)ptr + (nint)byteOffset) & ~((nint)alignment - 1));
            ((void**)result)[-1] = ptr;
            return result;
        }

        /// <summary>Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.</summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <param name="alignment">The alignment, in bytes, of the block to allocate. This must be a power of <c>2</c>.</param>
        /// <returns>A pointer to the allocated and zeroed aligned block of memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AlignedAllocZeroed(uint byteCount, uint alignment)
        {
            if (!BitOperationsHelpers.IsPow2(alignment))
                throw new ArgumentException("AlignmentMustBePow2", nameof(alignment));
            var byteOffset = (nuint)alignment - 1 + (nuint)sizeof(nint);
            var ptr = AllocZeroed(byteCount + (uint)byteOffset);
            if (ptr == null)
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
            Free(((void**)ptr)[-1]);
        }

        /// <summary>
        ///     Copies bytes from the source address to the destination address without assuming architecture dependent alignment
        ///     of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* destination, void* source, uint byteCount) => Unsafe.CopyBlockUnaligned(destination, source, byteCount);

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
            fixed (byte* pinnedDestination = &destination)
            {
                fixed (byte* pinnedSource = &source)
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
        public static void Set(void* startAddress, byte value, uint byteCount) => Unsafe.InitBlockUnaligned(startAddress, value, byteCount);

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

        /// <summary>Aligns a size to the platform's native integer size.</summary>
        /// <param name="size">The size, in bytes, to align.</param>
        /// <returns>The size aligned to the platform's native integer size.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Align(nuint size) => AlignUp(size, (nuint)sizeof(nint));

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