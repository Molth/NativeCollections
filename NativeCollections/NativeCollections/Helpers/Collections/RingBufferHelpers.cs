using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for copying ring buffer contents and computing element offsets.
    /// </summary>
    internal static class RingBufferHelpers
    {
        /// <summary>
        ///     Copies bytes from the source address to the destination address
        ///     without assuming architecture dependent alignment of the addresses.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to copy.</typeparam>
        /// <param name="destination">A reference to the first element of the destination memory.</param>
        /// <param name="source">A reference to the first element of the source ring buffer.</param>
        /// <param name="size">The number of elements to copy.</param>
        /// <param name="length">The total number of elements in the source ring buffer.</param>
        /// <param name="head">The zero-based index of the first element to copy within the source ring buffer.</param>
        /// <remarks>
        ///     This method performs no range validation; callers must ensure that <paramref name="head" /> is
        ///     less than <paramref name="length" />, that the source ring buffer holds <paramref name="length" />
        ///     elements, and that <paramref name="destination" /> has room for <paramref name="size" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, ref T source, int size, int length, int head) where T : unmanaged
        {
            if (size == 0)
                return;

            var length1 = length.WrappingSub(head);
            var length2 = Math.Min(length1, size);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref destination), ref Unsafe.As<T, byte>(ref Unsafe.Add(ref source, Isize.FromI32Unchecked(head))), Usize.FromI32Unchecked(length2).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
            var length3 = size.WrappingSub(length2);
            if (length3 > 0)
                SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref Unsafe.Add(ref destination, Isize.FromI32Unchecked(length1))), ref Unsafe.As<T, byte>(ref source), Usize.FromI32Unchecked(length3).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<T>())));
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
            var elementOffset = index.WrappingAdd(head);
            if (Usize.FromIsizeUnchecked(elementOffset) >= Usize.FromIsizeUnchecked(length))
                elementOffset = elementOffset.WrappingSub(length);

            return elementOffset;
        }
    }
}