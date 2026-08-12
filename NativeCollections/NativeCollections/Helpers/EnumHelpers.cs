using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper utilities for working with enum types.
    /// </summary>
    internal static class EnumHelpers
    {
        /// <summary>
        ///     Determines whether the underlying type of
        ///     the enum <typeparamref name="T" /> is a signed integer type.
        /// </summary>
        /// <typeparam name="T">The enum type to check.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the underlying type is signed (sbyte, short, int, long, or nint);
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSigned<T>() where T : unmanaged, Enum => EnumHelper<T>.IsSigned;

        /// <summary>
        ///     Caches metadata for the enum type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        private static class EnumHelper<T> where T : unmanaged, Enum
        {
            /// <summary>
            ///     Cached value indicating whether the underlying type of <typeparamref name="T" /> is signed.
            /// </summary>
            public static readonly bool IsSigned = IsSignedPrivate();

            /// <summary>
            ///     Determines whether the underlying type of
            ///     the enum <typeparamref name="T" /> is a signed integer type.
            /// </summary>
            /// <returns>
            ///     <see langword="true" /> if the underlying type is signed;
            ///     otherwise, <see langword="false" />.
            /// </returns>
            private static bool IsSignedPrivate()
            {
                var type = Enum.GetUnderlyingType(typeof(T));
                return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(nint);
            }
        }
    }
}