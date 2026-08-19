using System.Runtime.CompilerServices;

// ReSharper disable All

namespace rust
{
    internal static class Option
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> Some<T>(T value) where T : unmanaged => new(true, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> None<T>() where T : unmanaged => new(false, default);
    }
}