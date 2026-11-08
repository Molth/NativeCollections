using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory pool that provides reusable dynamic-size memory blocks.
    /// </summary>
    /// <remarks>
    ///     https://github.com/mattconte/tlsf
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community | FromType.C)]
    public readonly unsafe struct NativeDynamicMemoryPool : IIsCreated, IDisposable, IEquatable<NativeDynamicMemoryPool>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly void* _handle;

        /// <summary>
        ///     Total size (in bytes) of the memory pool.
        /// </summary>
        private readonly nuint _size;

        /// <summary>
        ///     Expected maximum number of blocks that can be allocated
        ///     from the pool (used for overhead pre‑computation).
        /// </summary>
        private readonly nuint _blocks;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="size">Total size (in bytes) of the memory pool.</param>
        /// <param name="blocks">
        ///     Expected maximum number of blocks that can be allocated from the pool (used for overhead
        ///     pre-computation).
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private NativeDynamicMemoryPool(void* handle, nuint size, nuint blocks)
        {
            _handle = handle;
            _size = size;
            _blocks = blocks;
        }

        /// <summary>
        ///     Creates a dynamic memory pool with the specified pool size and block count.
        /// </summary>
        /// <param name="size">Total size (in bytes) of the memory pool.</param>
        /// <param name="blocks">
        ///     The expected maximum number of blocks. This is used to reserve overhead for block management.
        /// </param>
        /// <returns>A newly allocated dynamic memory pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the alignment or size requirements are not met, or when TLSF pool creation fails.
        /// </exception>
        /// <remarks>
        ///     Creating a pool whose total allocation reaches the exact upper bound of the TLSF
        ///     block index (4 GiB on 64-bit processes, 1 GiB on 32-bit processes) fails with
        ///     <see cref="ArgumentOutOfRangeException" /> instead of succeeding.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeDynamicMemoryPool Create(nuint size, nuint blocks)
        {
            nuint bytes;
            void* buffer;
            void* handle;
            if (Environment.Is64BitProcess)
            {
                var totalSize = Tlsf64.TlsfSize().WrappingAdd(Tlsf64.TlsfPoolOverhead()) + U64.FromUsizeUnchecked(blocks) * Tlsf64.TlsfAllocOverhead() + U64.FromUsizeUnchecked(size);
                bytes = Usize.FromU64Unchecked(Tlsf64.AlignUp(totalSize, Tlsf64.TlsfAlignSize()));
                buffer = NativeMemoryAllocator.AlignedAlloc(bytes, NativeMemoryAllocator.AlignOf<Tlsf64.Control>());
                handle = Tlsf64.TlsfCreateWithPool(buffer, U64.FromUsizeUnchecked(bytes));
                if (handle == null)
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    ThrowHelpers.ThrowMustBeAlignedToException(Tlsf64.TlsfAlignSize(), ExceptionArgument.size);
                }
            }
            else
            {
                var totalSize = Tlsf32.TlsfSize().WrappingAdd(Tlsf32.TlsfPoolOverhead()) + U32.FromUsizeUnchecked(blocks) * Tlsf32.TlsfAllocOverhead() + U32.FromUsizeUnchecked(size);
                bytes = Usize.FromU32Unchecked(Tlsf32.AlignUp(totalSize, Tlsf32.TlsfAlignSize()));
                buffer = NativeMemoryAllocator.AlignedAlloc(bytes, NativeMemoryAllocator.AlignOf<Tlsf32.Control>());
                handle = Tlsf32.TlsfCreateWithPool(buffer, U32.FromUsizeUnchecked(bytes));
                if (handle == null)
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    ThrowHelpers.ThrowMustBeAlignedToException(Tlsf32.TlsfAlignSize(), ExceptionArgument.size);
                }
            }

            return new NativeDynamicMemoryPool(handle, size, blocks);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Total size (in bytes) of the memory pool.
        /// </summary>
        public nuint Size => _size;

        /// <summary>
        ///     Expected maximum number of blocks that can be allocated
        ///     from the pool (used for overhead pre‑computation).
        /// </summary>
        public nuint Blocks => _blocks;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeDynamicMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeDynamicMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeDynamicMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_handle);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset()
        {
            var size = _size;
            var blocks = _blocks;
            nuint bytes;
            var buffer = _handle;
            if (Environment.Is64BitProcess)
            {
                var totalSize = Tlsf64.TlsfSize().WrappingAdd(Tlsf64.TlsfPoolOverhead()) + U64.FromUsizeUnchecked(blocks) * Tlsf64.TlsfAllocOverhead() + U64.FromUsizeUnchecked(size);
                bytes = Usize.FromU64Unchecked(Tlsf64.AlignUp(totalSize, Tlsf64.TlsfAlignSize()));
                Tlsf64.TlsfCreateWithPool(buffer, U64.FromUsizeUnchecked(bytes));
            }
            else
            {
                var totalSize = Tlsf32.TlsfSize().WrappingAdd(Tlsf32.TlsfPoolOverhead()) + U32.FromUsizeUnchecked(blocks) * Tlsf32.TlsfAllocOverhead() + U32.FromUsizeUnchecked(size);
                bytes = Usize.FromU32Unchecked(Tlsf32.AlignUp(totalSize, Tlsf32.TlsfAlignSize()));
                Tlsf32.TlsfCreateWithPool(buffer, U32.FromUsizeUnchecked(bytes));
            }
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        /// <param name="size">The minimum number of bytes to allocate.</param>
        /// <param name="alignment">The required alignment of the allocated buffer, in bytes.</param>
        /// <param name="ptr">
        ///     When this method returns, contains the allocated buffer if
        ///     the operation succeeds; otherwise, <see langword="null" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a buffer was successfully retrieved; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(nuint size, nuint alignment, out void* ptr) => TryRent(size, alignment, out ptr, out _);

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        /// <param name="size">The minimum number of bytes to allocate.</param>
        /// <param name="alignment">The required alignment of the allocated buffer, in bytes.</param>
        /// <param name="ptr">
        ///     When this method returns, contains the allocated buffer if
        ///     the operation succeeds; otherwise, <see langword="null" />.
        /// </param>
        /// <param name="bytes">
        ///     When this method returns, contains the user-available capacity of
        ///     the allocated buffer if the operation succeeds; otherwise, zero.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a buffer was successfully retrieved; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="alignment" /> is not a power of two.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(nuint size, nuint alignment, out void* ptr, out nuint bytes)
        {
            ThrowHelpers.ThrowIfAlignmentNotBePow2(alignment, ExceptionArgument.alignment);

            if (Environment.Is64BitProcess)
            {
                var overhead = Tlsf64.TlsfAllocOverhead();
                var nativeSize = U64.FromUsizeUnchecked(size);

                if (nativeSize > Tlsf64.BLOCK_SIZE_MAX.WrappingSub(overhead))
                {
                    ptr = null;
                    bytes = default;
                    return false;
                }

                var requestSize = size == 0 ? U64.FromI32Unchecked(0) : nativeSize.WrappingAdd(overhead);
                ptr = Tlsf64.TlsfMemalign(_handle, U64.FromUsizeUnchecked(alignment), requestSize);
                if (ptr == null)
                {
                    bytes = default;
                    return false;
                }

                bytes = Usize.FromU64Unchecked(Tlsf64.TlsfBlockSize(ptr).WrappingSub(overhead));
            }
            else
            {
                var overhead = Tlsf32.TlsfAllocOverhead();
                var nativeSize = U32.FromUsizeUnchecked(size);

                if (nativeSize > Tlsf32.BLOCK_SIZE_MAX.WrappingSub(overhead))
                {
                    ptr = null;
                    bytes = default;
                    return false;
                }

                var requestSize = size == 0 ? U32.FromI32Unchecked(0) : nativeSize.WrappingAdd(overhead);
                ptr = Tlsf32.TlsfMemalign(_handle, U32.FromUsizeUnchecked(alignment), requestSize);
                if (ptr == null)
                {
                    bytes = default;
                    return false;
                }

                bytes = Usize.FromU32Unchecked(Tlsf32.TlsfBlockSize(ptr).WrappingSub(overhead));
            }

            return true;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via
        ///     <see cref="TryRent(nuint, nuint, out void*)" /> on the same instance.
        /// </summary>
        /// <param name="ptr">The pointer to the buffer to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (Environment.Is64BitProcess)
                Tlsf64.TlsfFree(_handle, ptr);
            else
                Tlsf32.TlsfFree(_handle, ptr);
        }
    }
}