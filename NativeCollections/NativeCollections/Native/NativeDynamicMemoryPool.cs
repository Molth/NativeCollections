using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native dynamic (Two-Level Segregated Fit) memory pool
    /// </summary>
    /// <remarks>
    ///     https://github.com/mattconte/tlsf
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community | FromType.C)]
    public readonly unsafe struct NativeDynamicMemoryPool : IIsCreated, IDisposable, IEquatable<NativeDynamicMemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly void* _handle;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private readonly nuint _size;

        /// <summary>
        ///     Blocks
        /// </summary>
        private readonly nuint _blocks;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="blocks">Blocks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDynamicMemoryPool(nuint size, nuint blocks)
        {
            nuint bytes;
            void* buffer;
            void* handle;
            if (Environment.Is64BitProcess)
            {
                bytes = (nuint)Tlsf64.align_up(Tlsf64.tlsf_size() + Tlsf64.tlsf_pool_overhead() + blocks * Tlsf64.tlsf_alloc_overhead() + size, 8);
                buffer = NativeMemoryAllocator.AlignedAlloc((uint)bytes, NativeMemoryAllocator.AlignOf<Tlsf32.control_t>());
                handle = Tlsf64.tlsf_create_with_pool(buffer, bytes);
                if (UnsafeHelpers.IsNull(handle))
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    ThrowHelpers.ThrowMustBeAlignedToException(8, ExceptionArgument.size);
                }
            }
            else
            {
                bytes = Tlsf32.align_up((uint)(Tlsf32.tlsf_size() + Tlsf32.tlsf_pool_overhead() + blocks * Tlsf32.tlsf_alloc_overhead() + size), 4);
                buffer = NativeMemoryAllocator.AlignedAlloc((uint)bytes, NativeMemoryAllocator.AlignOf<Tlsf64.control_t>());
                handle = Tlsf32.tlsf_create_with_pool(buffer, (uint)bytes);
                if (UnsafeHelpers.IsNull(handle))
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    ThrowHelpers.ThrowMustBeAlignedToException(4, ExceptionArgument.size);
                }
            }

            _handle = handle;
            _size = size;
            _blocks = blocks;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        public nuint Size => _size;

        /// <summary>
        ///     Blocks
        /// </summary>
        public nuint Blocks => _blocks;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeDynamicMemoryPool other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeDynamicMemoryPool other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeDynamicMemoryPool";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_handle);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            var size = _size;
            var blocks = _blocks;
            nuint bytes;
            var buffer = _handle;
            if (Environment.Is64BitProcess)
            {
                bytes = (nuint)Tlsf64.align_up(Tlsf64.tlsf_size() + Tlsf64.tlsf_pool_overhead() + blocks * Tlsf64.tlsf_alloc_overhead() + size, 8);
                Tlsf64.tlsf_create_with_pool(buffer, bytes);
            }
            else
            {
                bytes = Tlsf32.align_up((uint)(Tlsf32.tlsf_size() + Tlsf32.tlsf_pool_overhead() + blocks * Tlsf32.tlsf_alloc_overhead() + size), 4);
                Tlsf32.tlsf_create_with_pool(buffer, (uint)bytes);
            }
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(nuint size, nuint alignment, out void* ptr)
        {
            ptr = Environment.Is64BitProcess ? Tlsf64.tlsf_memalign(_handle, alignment, size) : Tlsf32.tlsf_memalign(_handle, (uint)alignment, (uint)size);
            return !UnsafeHelpers.IsNull(ptr);
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(nuint size, nuint alignment, out void* ptr, out nuint bytes)
        {
            if (Environment.Is64BitProcess)
            {
                ptr = Tlsf64.tlsf_memalign(_handle, alignment, size);
                if (!UnsafeHelpers.IsNull(ptr))
                {
                    bytes = (nuint)Tlsf64.tlsf_block_size(ptr);
                    return true;
                }

                bytes = 0;
                return false;
            }

            ptr = Tlsf32.tlsf_memalign(_handle, (uint)alignment, (uint)size);
            if (!UnsafeHelpers.IsNull(ptr))
            {
                bytes = Tlsf32.tlsf_block_size(ptr);
                return true;
            }

            bytes = 0;
            return false;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via 'TryRent' on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (Environment.Is64BitProcess)
                Tlsf64.tlsf_free(_handle, ptr);
            else
                Tlsf32.tlsf_free(_handle, ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeDynamicMemoryPool Empty => default;
    }
}