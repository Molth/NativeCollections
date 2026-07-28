using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe utility
    /// </summary>
    internal static class UnsafeUtility
    {
        /// <summary>
        ///     Rounds a value up to the specified alignment boundary.
        /// </summary>
        /// <param name="value">The value, in bytes, to align.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>A value at or after value that is a multiple of alignment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignUp(nuint value, nuint alignment) => (value + (alignment - 1)) & ~(alignment - 1);

        /// <summary>
        ///     Rounds a value down to the specified alignment boundary.
        /// </summary>
        /// <param name="value">The value, in bytes, to align.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>A value at or before value that is a multiple of alignment.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint AlignDown(nuint value, nuint alignment) => value - (value & (alignment - 1));

        /// <summary>
        ///     Determines whether the specified value is aligned to the given alignment boundary.
        /// </summary>
        /// <param name="value">The value to check for alignment.</param>
        /// <param name="alignment">The alignment, in bytes. This must be a power of <c>2</c>.</param>
        /// <returns>
        ///     <c>true</c> if <paramref name="value" /> is a multiple of <paramref name="alignment" />;
        ///     otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAligned(nuint value, nuint alignment) => (value & (alignment - 1)) == 0;
    }
}