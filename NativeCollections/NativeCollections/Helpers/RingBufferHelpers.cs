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