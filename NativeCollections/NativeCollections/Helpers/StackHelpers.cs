using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Ring buffer helpers
    /// </summary>
    internal static class StackHelpers
    {
        /// <summary>
        ///     Copy
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