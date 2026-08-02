using System.Runtime.CompilerServices;

// ReSharper disable All

namespace crossbeam
{
    internal static class U32Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint wrapping_add(this uint value, uint other) => unchecked(value + other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint wrapping_sub(this uint value, uint other) => unchecked(value - other);
    }
}