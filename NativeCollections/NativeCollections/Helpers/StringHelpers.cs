using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for strings.
    /// </summary>
    internal static class StringHelpers
    {
        /// <summary>
        ///     Gets a mutable span over the characters of the specified string.
        /// </summary>
        /// <param name="value">The string to access.</param>
        /// <returns>A mutable span that represents the character buffer of the string.</returns>
        /// <remarks>
        ///     This method bypasses the immutability of strings. Any modification to the returned span will
        ///     directly modify the original string, which may lead to unexpected behavior.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<char> AsSpan(string value) => MemoryMarshalHelpers.AsSpan(value.AsSpan());
    }
}