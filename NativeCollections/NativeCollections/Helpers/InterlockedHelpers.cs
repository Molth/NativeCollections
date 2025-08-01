using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Interlocked helpers
    /// </summary>
    internal static class InterlockedHelpers
    {
        /// <summary>
        ///     Adds two 64-bit signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Add(ref nint location, nint value) => Unsafe.SizeOf<nint>() == 8 ? (nint)Interlocked.Add(ref Unsafe.As<nint, long>(ref location), value) : Interlocked.Add(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Increment(ref nint location) => Unsafe.SizeOf<nint>() == 8 ? (nint)Interlocked.Increment(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Increment(ref Unsafe.As<nint, int>(ref location));

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Decrement(ref nint location) => Unsafe.SizeOf<nint>() == 8 ? (nint)Interlocked.Decrement(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Decrement(ref Unsafe.As<nint, int>(ref location));
    }
}