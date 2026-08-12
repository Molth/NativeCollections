using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper functions.
    /// </summary>
    internal static class RingBufferHelpers
    {
        /// <summary>
        ///     Copies bytes from the source address to the destination address without assuming architecture dependent alignment
        ///     of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, ref T source, int size, int length, int head) where T : unmanaged
        {
            if (size == 0)
                return;
            var length1 = length - head;
            var length2 = Math.Min(length1, size);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref source, (nint)head)), (uint)(length2 * Unsafe.SizeOf<T>()));
            var length3 = size - length2;
            if (length3 > 0)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref destination, (nint)length1)), ref Unsafe.As<T, byte>(ref source), (uint)(length3 * Unsafe.SizeOf<T>()));
        }

        /// <summary>
        ///     Computes the actual buffer index for a given logical index in a ring buffer,
        ///     taking into account the current head position and handling wrap-around.
        /// </summary>
        /// <param name="index">The logical index (0‑based) of the element within the ring buffer.</param>
        /// <param name="head">The current head position (starting offset) of the ring buffer.</param>
        /// <param name="length">The total length of the ring buffer.</param>
        /// <returns>The actual linear offset in the underlying buffer that corresponds to the logical index.</returns>
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