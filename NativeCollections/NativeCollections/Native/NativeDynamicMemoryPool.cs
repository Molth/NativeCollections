using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.TLSF;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native dynamic (Two-Level Segregated Fit) memory pool
    ///     https://github.com/mattconte/tlsf
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Community | FromType.C)]
    public readonly unsafe struct NativeDynamicMemoryPool : IDisposable, IEquatable<NativeDynamicMemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly void* _handle;

        /// <summary>
        ///     Size
        /// </summary>
        private readonly ulong _size;

        /// <summary>
        ///     Blocks
        /// </summary>
        private readonly ulong _blocks;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="blocks">Blocks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDynamicMemoryPool(ulong size, ulong blocks)
        {
            ulong bytes;
            void* buffer;
            void* handle;
            if (sizeof(nint) == 8)
            {
                bytes = TLSF64.align_up(TLSF64.tlsf_size() + TLSF64.tlsf_pool_overhead() + blocks * TLSF64.tlsf_alloc_overhead() + size, 8);
                buffer = NativeMemoryAllocator.AlignedAlloc((uint)bytes, (uint)NativeMemoryAllocator.AlignOf<TLSF32.control_t>());
                handle = TLSF64.tlsf_create_with_pool(buffer, bytes);
                if (handle == null)
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    throw new ArgumentOutOfRangeException(nameof(size), "Memory must be aligned to 8 bytes.");
                }
            }
            else
            {
                bytes = TLSF32.align_up((uint)(TLSF32.tlsf_size() + TLSF32.tlsf_pool_overhead() + blocks * TLSF32.tlsf_alloc_overhead() + size), 4);
                buffer = NativeMemoryAllocator.AlignedAlloc((uint)bytes, (uint)NativeMemoryAllocator.AlignOf<TLSF64.control_t>());
                handle = TLSF32.tlsf_create_with_pool(buffer, (uint)bytes);
                if (handle == null)
                {
                    NativeMemoryAllocator.AlignedFree(buffer);
                    throw new ArgumentOutOfRangeException(nameof(size), "Memory must be aligned to 4 bytes.");
                }
            }

            _handle = handle;
            _size = size;
            _blocks = blocks;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Size
        /// </summary>
        public ulong Size => _size;

        /// <summary>
        ///     Blocks
        /// </summary>
        public ulong Blocks => _blocks;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeDynamicMemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeDynamicMemoryPool nativeDynamicMemoryPool && nativeDynamicMemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeDynamicMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeDynamicMemoryPool left, NativeDynamicMemoryPool right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            var size = _size;
            var blocks = _blocks;
            ulong bytes;
            var buffer = _handle;
            if (sizeof(nint) == 8)
            {
                bytes = TLSF64.align_up(TLSF64.tlsf_size() + TLSF64.tlsf_pool_overhead() + blocks * TLSF64.tlsf_alloc_overhead() + size, 8);
                TLSF64.tlsf_create_with_pool(buffer, bytes);
            }
            else
            {
                bytes = TLSF32.align_up((uint)(TLSF32.tlsf_size() + TLSF32.tlsf_pool_overhead() + blocks * TLSF32.tlsf_alloc_overhead() + size), 4);
                TLSF32.tlsf_create_with_pool(buffer, (uint)bytes);
            }
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="alignment">Alignment</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(ulong size, uint alignment) => sizeof(nint) == 8 ? TLSF64.tlsf_memalign(_handle, size, alignment) : TLSF32.tlsf_memalign(_handle, (uint)size, alignment);

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="bytes">Actual size</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(ulong size, out ulong bytes)
        {
            if (sizeof(nint) == 8)
            {
                var ptr = TLSF64.tlsf_malloc(_handle, size);
                bytes = ptr != null ? TLSF64.tlsf_block_size(ptr) : 0;
                return ptr;
            }
            else
            {
                var ptr = TLSF32.tlsf_malloc(_handle, (uint)size);
                bytes = ptr != null ? TLSF32.tlsf_block_size(ptr) : 0;
                return ptr;
            }
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (sizeof(nint) == 8)
                TLSF64.tlsf_free(_handle, ptr);
            else
                TLSF32.tlsf_free(_handle, ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeDynamicMemoryPool Empty => new();
    }
}