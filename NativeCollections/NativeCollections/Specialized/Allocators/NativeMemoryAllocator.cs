using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     This class contains methods that are mainly used to manage native memory.
    /// </summary>
    [Customizable("public static void* AlignedAlloc(nuint byteCount, nuint alignment)", "public static void* AlignedAllocZeroed(nuint byteCount, nuint alignment)", "public static void AlignedFree(void* ptr)")]
#if NET6_0_OR_GREATER
    [BindingType(typeof(NativeMemory))]
#else
    [BindingType(typeof(Marshal))]
#endif
    public static unsafe class NativeMemoryAllocator
    {
        /// <summary>
        ///     Allocates an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        private static delegate* managed<nuint, nuint, void*> _alignedAlloc;

        /// <summary>
        ///     Allocates and zeroes an aligned block of memory of the specified size and alignment, in bytes.
        /// </summary>
        private static delegate* managed<nuint, nuint, void*> _alignedAllocZeroed;

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        private static delegate* managed<void*, void> _alignedFree;

        /// <summary>
        ///     Abort callback invoked when memory allocation fails.
        /// </summary>
        private static delegate* managed<void> _abort;

        /// <summary>
        ///     Configures custom memory allocation handlers.
        /// </summary>
        /// <param name="alignedAlloc">The callback used to allocate aligned memory.</param>
        /// <param name="alignedAllocZeroed">The callback used to allocate zeroed aligned memory.</param>
        /// <param name="alignedFree">The callback used to free aligned memory.</param>
        /// <param name="abort">
        ///     The callback invoked when an allocation fails, or <see langword="null" /> to use the default behavior.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Custom(delegate* managed<nuint, nuint, void*> alignedAlloc, delegate* managed<nuint, nuint, void*> alignedAllocZeroed, delegate* managed<void*, void> alignedFree, delegate* managed<void> abort = null)
        {
            _alignedAlloc = alignedAlloc;
            _alignedAllocZeroed = alignedAllocZeroed;
            _alignedFree = alignedFree;
            _abort = abort;
        }

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
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAlloc<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAlloc(byteCount, AlignOf<T>());
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
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* AlignedAllocZeroed<T>(nuint elementCount) where T : unmanaged
        {
            var byteCount = elementCount * Usize.FromI32Unchecked(Unsafe.SizeOf<T>());
            return (T*)AlignedAllocZeroed(byteCount, AlignOf<T>());
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
        ///     Thrown when the required allocation size overflows a native-sized integer
        ///     on .NET versions earlier than .NET 6.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        [Customizable]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* AlignedAlloc(nuint byteCount, nuint alignment)
        {
            void* ptr;
            var alignedAlloc = _alignedAlloc;
            if (alignedAlloc != null)
            {
                ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
                ptr = alignedAlloc(byteCount, alignment);
                if (ptr == null)
                {
                    var abort = _abort;
                    if (abort != null)
                        abort();

                    ThrowHelpers.ThrowOutOfMemoryException();
                }

                return ptr;
            }

#if NET6_0_OR_GREATER
            try
            {
                return NativeMemory.AlignedAlloc(byteCount, alignment);
            }
            catch (OutOfMemoryException)
            {
                var abort = _abort;
                if (abort != null)
                    abort();

                throw;
            }
#else
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            var byteOffset = alignment.WrappingAdd(Usize.FromI32Unchecked(Unsafe.SizeOf<nint>()).WrappingSub(Usize.FromI32Unchecked(1)));
            try
            {
                var allocationSize = Isize.FromUsize(byteCount + byteOffset);
                ptr = (void*)Marshal.AllocHGlobal(allocationSize);
            }
            catch (OutOfMemoryException)
            {
                var abort = _abort;
                if (abort != null)
                    abort();

                throw;
            }

            var result = (void*)Isize.FromUsizeUnchecked(Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(ptr)).WrappingAdd(byteOffset) & ~alignment.WrappingSub(Usize.FromI32Unchecked(1)));
            Unsafe.Subtract(ref Unsafe.AsRef<nint>(result), 1) = Isize.FromPtrUnchecked(ptr);
            return result;
#endif
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
        ///     Thrown when the required allocation size overflows a native-sized integer
        ///     on .NET versions earlier than .NET 6.
        /// </exception>
        /// <exception cref="OutOfMemoryException">Thrown when memory allocation fails.</exception>
        [Customizable]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void* AlignedAllocZeroed(nuint byteCount, nuint alignment)
        {
            var alignedAllocZeroed = _alignedAllocZeroed;
            if (alignedAllocZeroed != null)
            {
                ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
                var ptr = alignedAllocZeroed(byteCount, alignment);
                if (ptr == null)
                {
                    var abort = _abort;
                    if (abort != null)
                        abort();

                    ThrowHelpers.ThrowOutOfMemoryException();
                }

                return ptr;
            }

            var result = AlignedAlloc(byteCount, alignment);
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(result), U8.FromI32Unchecked(0), byteCount);
            return result;
        }

        /// <summary>
        ///     Frees an aligned block of memory.
        /// </summary>
        /// <param name="ptr">A pointer to the aligned block of memory that should be freed.</param>
        [Customizable]
        [MethodImpl(MethodImplOptions.NoInlining)]
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
        ///     Copies bytes from the source address to the destination address
        ///     without assuming architecture dependent alignment of the addresses.
        /// </summary>
        /// <param name="destination">A pointer to the destination memory location.</param>
        /// <param name="source">A pointer to the source memory location.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(void* destination, void* source, nuint byteCount) => SpanHelpers.Copy(ref Unsafe.AsRef<byte>(destination), ref Unsafe.AsRef<byte>(source), byteCount);

        /// <summary>
        ///     Copies bytes from the source address to the destination address
        ///     without assuming architecture dependent alignment of the addresses.
        /// </summary>
        /// <param name="destination">A reference to the destination memory location.</param>
        /// <param name="source">A reference to the source memory location.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref byte destination, ref byte source, nuint byteCount) => SpanHelpers.Copy(ref destination, ref source, byteCount);

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">A pointer to the destination memory location.</param>
        /// <param name="source">A pointer to the source memory location.</param>
        /// <param name="byteCount">The number of bytes to move.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(void* destination, void* source, nuint byteCount) => SpanHelpers.Move(ref Unsafe.AsRef<byte>(destination), ref Unsafe.AsRef<byte>(source), byteCount);

        /// <summary>
        ///     Copies a block of memory from memory location <paramref name="source" />
        ///     to memory location <paramref name="destination" />.
        /// </summary>
        /// <param name="destination">A reference to the destination memory location.</param>
        /// <param name="source">A reference to the source memory location.</param>
        /// <param name="byteCount">The number of bytes to move.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Move(ref byte destination, ref byte source, nuint byteCount) => SpanHelpers.Move(ref destination, ref source, byteCount);

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial
        ///     value without assuming architecture dependent alignment of the address.
        /// </summary>
        /// <param name="startAddress">A pointer to the first byte to initialize.</param>
        /// <param name="value">The value to assign to each byte.</param>
        /// <param name="byteCount">The number of bytes to initialize.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(void* startAddress, byte value, nuint byteCount) => SpanHelpers.Set(ref Unsafe.AsRef<byte>(startAddress), value, byteCount);

        /// <summary>
        ///     Initializes a block of memory at the given location with a given initial
        ///     value without assuming architecture dependent alignment of the address.
        /// </summary>
        /// <param name="startAddress">A reference to the first byte to initialize.</param>
        /// <param name="value">The value to assign to each byte.</param>
        /// <param name="byteCount">The number of bytes to initialize.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(ref byte startAddress, byte value, nuint byteCount) => SpanHelpers.Set(ref startAddress, value, byteCount);

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        /// <param name="left">A pointer to the first sequence.</param>
        /// <param name="right">A pointer to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns><see langword="true" /> if the sequences are equal; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="left" /> or <paramref name="right" /> is
        ///     <see langword="null" /> and the other is not <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(void* left, void* right, nuint byteCount)
        {
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                ThrowHelpers.ThrowArgumentNullException(left == null ? ExceptionArgument.left : ExceptionArgument.right);

            return SpanHelpers.Equals(ref Unsafe.AsRef<byte>(left), ref Unsafe.AsRef<byte>(right), byteCount);
        }

        /// <summary>
        ///     Determines whether two sequences are equal.
        /// </summary>
        /// <param name="left">A reference to the first sequence.</param>
        /// <param name="right">A reference to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns><see langword="true" /> if the sequences are equal; otherwise, <see langword="false" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="left" /> or <paramref name="right" /> is
        ///     <see langword="null" /> and the other is not <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Equals(ref byte left, ref byte right, nuint byteCount)
        {
            if (Unsafe.IsNullRef(ref left) && Unsafe.IsNullRef(ref right))
                return true;

            if (Unsafe.IsNullRef(ref left) || Unsafe.IsNullRef(ref right))
                ThrowHelpers.ThrowArgumentNullException(Unsafe.IsNullRef(ref left) ? ExceptionArgument.left : ExceptionArgument.right);

            return SpanHelpers.Equals(ref left, ref right, byteCount);
        }

        /// <summary>
        ///     Determines the relative order of the sequences.
        /// </summary>
        /// <param name="left">A pointer to the first sequence.</param>
        /// <param name="right">A pointer to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="left" /> or <paramref name="right" /> is
        ///     <see langword="null" /> and the other is not <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(void* left, void* right, nuint byteCount)
        {
            if (left == null && right == null)
                return 0;

            if (left == null || right == null)
                ThrowHelpers.ThrowArgumentNullException(left == null ? ExceptionArgument.left : ExceptionArgument.right);

            return SpanHelpers.Compare(ref Unsafe.AsRef<byte>(left), ref Unsafe.AsRef<byte>(right), byteCount);
        }

        /// <summary>
        ///     Determines the relative order of the sequences.
        /// </summary>
        /// <param name="left">A reference to the first sequence.</param>
        /// <param name="right">A reference to the second sequence.</param>
        /// <param name="byteCount">The number of bytes to compare.</param>
        /// <returns>A value that indicates the relative order of the sequences.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="left" /> or <paramref name="right" /> is
        ///     <see langword="null" /> and the other is not <see langword="null" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Compare(ref byte left, ref byte right, nuint byteCount)
        {
            if (Unsafe.IsNullRef(ref left) && Unsafe.IsNullRef(ref right))
                return 0;

            if (Unsafe.IsNullRef(ref left) || Unsafe.IsNullRef(ref right))
                ThrowHelpers.ThrowArgumentNullException(Unsafe.IsNullRef(ref left) ? ExceptionArgument.left : ExceptionArgument.right);

            return SpanHelpers.Compare(ref left, ref right, byteCount);
        }

        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the elements to fill.</typeparam>
        /// <param name="startAddress">A pointer to the first element to fill.</param>
        /// <param name="elementCount">The number of elements to fill.</param>
        /// <param name="value">The value to assign to each element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(void* startAddress, nuint elementCount, T value) where T : unmanaged => SpanHelpers.Fill(ref Unsafe.AsRef<T>(startAddress), elementCount, value);

        /// <summary>
        ///     Fills the contents of this buffer with the given value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the elements to fill.</typeparam>
        /// <param name="startAddress">A reference to the first element to fill.</param>
        /// <param name="elementCount">The number of elements to fill.</param>
        /// <param name="value">The value to assign to each element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fill<T>(ref byte startAddress, nuint elementCount, T value) where T : unmanaged => SpanHelpers.Fill(ref Unsafe.As<byte, T>(ref startAddress), elementCount, value);

        /// <summary>
        ///     Rounds a value up to the specified alignment boundary.
        /// </summary>
        /// <param name="value">The value, in bytes, to align.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>A value at or after value that is a multiple of alignment.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        /// <exception cref="OverflowException">
        ///     Thrown when the aligned value exceeds the maximum native-sized value.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignUp(nuint value, nuint alignment)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            return (value + alignment.WrappingSub(Usize.FromI32Unchecked(1))) & ~alignment.WrappingSub(Usize.FromI32Unchecked(1));
        }

        /// <summary>
        ///     Rounds a value down to the specified alignment boundary.
        /// </summary>
        /// <param name="value">The value, in bytes, to align.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>A value at or before value that is a multiple of alignment.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignDown(nuint value, nuint alignment)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            return value.WrappingSub(value & alignment.WrappingSub(Usize.FromI32Unchecked(1)));
        }

        /// <summary>
        ///     Gets the alignment, in bytes, of the specified unmanaged type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type whose alignment is to be determined.</typeparam>
        /// <returns>The alignment, in bytes, of type <typeparamref name="T" />.</returns>
        /// <remarks>
        ///     If <typeparamref name="T" /> is not naturally aligned, as determined by
        ///     <see cref="IsAligned{T}()" />, this method may return an incorrect value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignOf<T>() where T : unmanaged => Usize.FromI32Unchecked(Unsafe.SizeOf<AlignOfHelper<T>>().WrappingSub(Unsafe.SizeOf<T>()));

        /// <summary>
        ///     Determines whether the specified unmanaged type <typeparamref name="T" /> is
        ///     naturally aligned, meaning its size is a multiple of its alignment requirement.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to check for alignment.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the size of <typeparamref name="T" /> is
        ///     a multiple of its alignment; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static bool IsAligned<T>() where T : unmanaged
        {
            Unsafe.SkipInit(out AlignOfHelper<T> helper);
            var alignment = Usize.FromIsizeUnchecked(Unsafe.ByteOffset(ref Unsafe.AsRef(in helper.Dummy), ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in helper.Data))));
            return (Usize.FromI32Unchecked(Unsafe.SizeOf<T>()) & alignment.WrappingSub(Usize.FromI32Unchecked(1))) == Usize.FromI32Unchecked(0);
        }

        /// <summary>
        ///     Determines whether the specified memory pointer is aligned to the given alignment boundary.
        /// </summary>
        /// <param name="ptr">The pointer to the memory address to check for alignment.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>
        ///     <see langword="true" /> if the address of <paramref name="ptr" /> is a
        ///     multiple of <paramref name="alignment" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAligned(void* ptr, nuint alignment)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);
            return (Usize.FromIsizeUnchecked(Isize.FromPtrUnchecked(ptr)) & alignment.WrappingSub(Usize.FromI32Unchecked(1))) == Usize.FromI32Unchecked(0);
        }

        /// <summary>
        ///     Returns the address of the specified by-ref parameter.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="reference">The object whose address is obtained.</param>
        /// <returns>The address of the specified by-ref parameter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint AddressOf<T>([MustBePinned] ref T reference) where T : unmanaged => Isize.FromPtrUnchecked(Unsafe.AsPointer(ref reference));

        /// <summary>
        ///     Helper structure for calculating type alignment.
        /// </summary>
        /// <typeparam name="T">The unmanaged type being measured.</typeparam>
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct AlignOfHelper<T> where T : unmanaged
        {
            /// <summary>
            ///     Padding byte used for alignment calculation.
            /// </summary>
            public readonly byte Dummy;

            /// <summary>
            ///     The typed data used for alignment measurement.
            /// </summary>
            public readonly T Data;
        }
    }
}