using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Ring buffer helpers
    /// </summary>
    internal static class RingBufferHelpers
    {
        /// <summary>
        ///     Copy
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, ref T source, int size, int length, int head) where T : unmanaged
        {
            if (size == 0)
                return;
            var length1 = length - head;
            var length2 = Math.Min(length1, size);
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref source, (nint)head)), (uint)(length2 * Unsafe.SizeOf<T>()));
            var length3 = size - length2;
            if (length3 > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref destination, (nint)length1)), ref Unsafe.As<T, byte>(ref source), (uint)(length3 * Unsafe.SizeOf<T>()));
        }

        /// <summary>
        ///     Get element offset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint GetElementOffset(nint index, nint head, nint length)
        {
            var elementOffset = index + head;
            if ((nuint)elementOffset >= (nuint)length)
                elementOffset -= length;
            return elementOffset;
        }
    }
}