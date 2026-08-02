using System.Runtime.CompilerServices;

// ReSharper disable All

namespace crossbeam
{
    internal static class Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Ok<T>(T value) where T : unmanaged => new(true, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Err<T>(T value) where T : unmanaged => new(false, value);
    }
}