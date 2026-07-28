using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

// ReSharper disable All

namespace crossbeam
{
    internal static class Result
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Ok<T>(T value) => new(true, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Err<T>(T value) => new(false, value);
    }
}