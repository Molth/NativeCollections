using System.Runtime.CompilerServices;

// ReSharper disable All

namespace crossbeam
{
    internal static class Option
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> Some<T>(T? value) => new(true, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> None<T>() => new(false, default);
    }
}