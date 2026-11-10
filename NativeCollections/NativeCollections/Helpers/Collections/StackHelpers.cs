using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a helper method that copies the elements of a stack in reverse order.
    /// </summary>
    internal static class StackHelpers
    {
        /// <summary>
        ///     Copies elements from the source address to the destination address in reverse order,
        ///     so that <c>destination[count - 1 - i] = source[i]</c>, without assuming architecture
        ///     dependent alignment of the addresses.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to copy.</typeparam>
        /// <param name="destination">A reference to the first element of the destination memory.</param>
        /// <param name="source">A reference to the first element of the source memory.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <remarks>
        ///     This method performs no bounds validation; callers must ensure that both
        ///     references point to blocks of at least <paramref name="count" /> elements.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, ref T source, int count) where T : unmanaged
        {
            var num1 = 0;
            var num2 = count;
            while (num1 < count)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref destination, Isize.FromI32Unchecked(num2 = num2.WrappingSub(1))), Unsafe.Add(ref source, Isize.FromI32Unchecked((num1 = num1.WrappingAdd(1)).WrappingSub(1))));
        }
    }
}