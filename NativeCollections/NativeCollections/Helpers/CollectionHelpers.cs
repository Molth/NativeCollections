using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for common collection operations.
    /// </summary>
    internal static class CollectionHelpers
    {
        /// <summary>
        ///     Computes a new capacity that is sufficient for the specified required size.
        /// </summary>
        /// <param name="capacity">The current capacity.</param>
        /// <param name="newCapacity">The minimum required capacity.</param>
        /// <returns>A new capacity that is at least <paramref name="newCapacity" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EnsureCapacity(int capacity, int newCapacity)
        {
            var result = 2 * capacity;
            if ((uint)result > ArrayHelpers.MaxLength)
                result = ArrayHelpers.MaxLength;
            var expected = capacity + 4;
            result = Math.Max(result, expected);
            result = Math.Max(result, newCapacity);
            return result;
        }
    }
}