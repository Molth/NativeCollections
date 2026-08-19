using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable All

namespace rust
{
    internal static class U32Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint wrapping_add(this uint value, uint other) => unchecked(value + other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint wrapping_sub(this uint value, uint other) => unchecked(value - other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint wrapping_mul(this uint value, uint other) => unchecked(value * other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint rotate_left(this uint value, int offset) => BitOperationsHelpers.RotateLeft(value, offset);
    }
}