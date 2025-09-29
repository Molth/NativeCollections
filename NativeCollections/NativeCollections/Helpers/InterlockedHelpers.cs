using System;
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
        public static nint Add(ref nint location, nint value) => Environment.Is64BitProcess ? (nint)Interlocked.Add(ref Unsafe.As<nint, long>(ref location), value) : Interlocked.Add(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Increment(ref nint location) => Environment.Is64BitProcess ? (nint)Interlocked.Increment(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Increment(ref Unsafe.As<nint, int>(ref location));

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Decrement(ref nint location) => Environment.Is64BitProcess ? (nint)Interlocked.Decrement(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Decrement(ref Unsafe.As<nint, int>(ref location));

        /// <summary>
        ///     Sets a platform-specific handle or pointer to a specified value and returns the original value, as an atomic
        ///     operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Exchange(ref nuint location, nuint value)
        {
#if NET7_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            return Environment.Is64BitProcess ? (nuint)Interlocked.Exchange(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Interlocked.Exchange(ref Unsafe.As<nuint, int>(ref location), (int)value);
#endif
        }

        /// <summary>
        ///     Compares two platform-specific handles or pointers for equality and, if they are equal, replaces the first one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CompareExchange(ref nuint location, nuint value, nuint comparand)
        {
#if NET7_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            return Environment.Is64BitProcess ? (nuint)Interlocked.CompareExchange(ref Unsafe.As<nuint, long>(ref location), (long)value, (long)comparand) : (nuint)Interlocked.CompareExchange(ref Unsafe.As<nuint, int>(ref location), (int)value, (int)comparand);
#endif
        }

        /// <summary>
        ///     Adds two 64-bit signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Add(ref nuint location, nuint value) => Environment.Is64BitProcess ? (nuint)Interlocked.Add(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Interlocked.Add(ref Unsafe.As<nuint, int>(ref location), (int)value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Increment(ref nuint location) => Environment.Is64BitProcess ? (nuint)Interlocked.Increment(ref Unsafe.As<nuint, long>(ref location)) : (nuint)Interlocked.Increment(ref Unsafe.As<nuint, int>(ref location));

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Decrement(ref nuint location) => Environment.Is64BitProcess ? (nuint)Interlocked.Decrement(ref Unsafe.As<nuint, long>(ref location)) : (nuint)Interlocked.Decrement(ref Unsafe.As<nuint, int>(ref location));
    }
}