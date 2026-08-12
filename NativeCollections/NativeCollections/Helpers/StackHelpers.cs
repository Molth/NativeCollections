using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper functions.
    /// </summary>
    internal static class StackHelpers
    {
        /// <summary>
        ///     Copies bytes from the source address to the destination address without assuming architecture dependent alignment
        ///     of the addresses.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(ref T destination, ref T source, int count) where T : unmanaged
        {
            var num1 = 0;
            var num2 = count;
            while (num1 < count)
                UnsafeHelpers.WriteUnaligned(ref Unsafe.Add(ref destination, (nint)(--num2)), Unsafe.Add(ref source, (nint)num1++));
        }
    }
}