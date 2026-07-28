using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS8604 // Possible null reference argument.

// ReSharper disable All

namespace crossbeam
{
    internal static class Option
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> Some<T>(T value) => new(true, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> None<T>() => new(false, default);
    }
}