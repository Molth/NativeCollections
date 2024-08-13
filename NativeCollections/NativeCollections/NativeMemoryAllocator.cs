using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Native.Collections
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
        public static void* Alloc(int byteCount) =>
#if NET6_0_OR_GREATER
            NativeMemory.Alloc((nuint)byteCount);
#else
            (void*)Marshal.AllocHGlobal(byteCount);
#endif

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="byteCount">Byte count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(int byteCount)
        {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed((nuint)byteCount);
#else
            var ptr = (void*)Marshal.AllocHGlobal(byteCount);
            Unsafe.InitBlockUnaligned(ptr, 0, (uint)byteCount);
            return ptr;
#endif
        }

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc<T>() where T : unmanaged =>
#if NET6_0_OR_GREATER
            NativeMemory.Alloc((nuint)sizeof(T));
#else
            (void*)Marshal.AllocHGlobal(sizeof(T));
#endif

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed<T>() where T : unmanaged
        {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed((nuint)sizeof(T));
#else
            var ptr = (void*)Marshal.AllocHGlobal(sizeof(T));
            Unsafe.InitBlockUnaligned(ptr, 0, (uint)sizeof(T));
            return ptr;
#endif
        }

        /// <summary>
        ///     Alloc
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc<T>(int count) where T : unmanaged =>
#if NET6_0_OR_GREATER
            NativeMemory.Alloc((nuint)(count * sizeof(T)));
#else
            (void*)Marshal.AllocHGlobal(count * sizeof(T));
#endif

        /// <summary>
        ///     Alloc zeroed
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Memory</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed<T>(int count) where T : unmanaged
        {
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed((nuint)(count * sizeof(T)));
#else
            var ptr = (void*)Marshal.AllocHGlobal(count * sizeof(T));
            Unsafe.InitBlockUnaligned(ptr, 0, (uint)(count * sizeof(T)));
            return ptr;
#endif
        }

        /// <summary>
        ///     Free
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* ptr) =>
#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif
    }
}