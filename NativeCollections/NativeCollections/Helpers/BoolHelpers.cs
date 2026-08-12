using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a bool (<see langword="true" /> or <see langword="false" />) value.
    /// </summary>
    internal static class BoolHelpers
    {
        /// <summary>
        ///     Converts a <see cref="bool" /> value to a <see cref="byte" /> value,
        ///     where <see langword="true" /> becomes 1 and <see langword="false" /> becomes 0.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <returns>
        ///     1 if <paramref name="value" /> is <see langword="true" />;
        ///     otherwise, 0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte IntoU8(bool value) => value ? (byte)1 : (byte)0;

        /// <summary>
        ///     Converts a <see cref="byte" /> value to a <see cref="bool" /> value,
        ///     where any non‑zero value becomes <see langword="true" />.
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="value" /> is non‑zero;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FromU8(byte value) => value != 0;
    }
}