using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace crossbeam
{
    public static class UsizeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint wrapping_add(this nuint value, nuint other) => unchecked(value + other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint wrapping_sub(this nuint value, nuint other) => unchecked(value - other);
    }
}