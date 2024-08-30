using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory allocator
    /// </summary>
    public static unsafe class NativeMemoryAllocator
    {
        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(int byteCount)
        {
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc((nuint)byteCount);
#else
            return (void*)Marshal.AllocHGlobal(byteCount);
#endif
        }

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(int byteCount)
        {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed((nuint)byteCount, 1);
#else
            var ptr = (void*)Marshal.AllocHGlobal(byteCount);
            Unsafe.InitBlockUnaligned(ptr, 0, (uint)byteCount);
            return ptr;
#endif
        }

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif
        }

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(nint ptr)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free((void*)ptr);
#else
            Marshal.FreeHGlobal(ptr);
#endif
        }
    }
}