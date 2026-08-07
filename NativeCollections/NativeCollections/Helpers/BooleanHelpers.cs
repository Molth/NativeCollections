using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a boolean (<see langword="true" /> or <see langword="false" />) value.
    /// </summary>
    internal static class BooleanHelpers
    {
        /// <summary>
        ///     Into byte
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte IntoU8(bool value) => value ? (byte)1 : (byte)0;

        /// <summary>
        ///     From byte
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FromU8(byte value) => value != 0;
    }
}