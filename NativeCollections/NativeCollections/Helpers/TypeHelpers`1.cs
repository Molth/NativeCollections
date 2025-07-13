using System;
using System.Runtime.CompilerServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Type helpers
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    internal static unsafe class TypeHelpers<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        ///     Is write atomic
        /// </summary>
        public static bool IsWriteAtomic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsWriteAtomicPrivate();
        }

        /// <summary>
        ///     Is write atomic
        /// </summary>
        /// <returns>Is write atomic</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsWriteAtomicPrivate()
        {
            if (typeof(T).IsEnum)
                return true;

            if (typeof(T) == typeof(nint) || typeof(T) == typeof(nuint))
                return true;

            if (typeof(T) == typeof(bool) || typeof(T) == typeof(byte) || typeof(T) == typeof(char) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(float) || typeof(T) == typeof(ushort) || typeof(T) == typeof(uint))
                return true;

            return (typeof(T) == typeof(double) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong)) && sizeof(nint) == 8;
        }
    }
}