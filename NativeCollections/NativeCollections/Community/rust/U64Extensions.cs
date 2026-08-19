using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable All

namespace rust
{
    internal static class U64Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong wrapping_add(this ulong value, ulong other) => unchecked(value + other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong wrapping_sub(this ulong value, ulong other) => unchecked(value - other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong wrapping_mul(this ulong value, ulong other) => unchecked(value * other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong rotate_left(this ulong value, int offset) => BitOperationsHelpers.RotateLeft(value, offset);
    }
}