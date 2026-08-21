using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for building collections dynamically.
    /// </summary>
    internal static class BuilderHelpers
    {
        /// <summary>
        ///     Computes a new capacity that is sufficient for the specified required size.
        /// </summary>
        /// <param name="capacity">The current capacity.</param>
        /// <param name="newCapacity">The minimum required capacity.</param>
        /// <returns>A new capacity that is at least <paramref name="newCapacity" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GrowCapacity(int capacity, int newCapacity)
        {
            var result = Math.Max(capacity != 0 ? capacity * 2 : 4, newCapacity);
            if ((uint)result > ArrayHelpers.MaxLength)
                result = Math.Max(Math.Max(capacity + 1, ArrayHelpers.MaxLength), capacity);
            return result;
        }
    }
}