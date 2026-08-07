using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides the base class for enumerations.
    /// </summary>
    internal static class EnumHelpers
    {
        /// <summary>
        ///     Is signed
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSigned<T>() where T : unmanaged, Enum => EnumHelper<T>.IsSigned;

        /// <summary>
        ///     Enum helper
        /// </summary>
        private static class EnumHelper<T> where T : unmanaged, Enum
        {
            /// <summary>
            ///     Is signed
            /// </summary>
            public static readonly bool IsSigned = IsSignedPrivate();

            /// <summary>
            ///     Is signed
            /// </summary>
            private static bool IsSignedPrivate()
            {
                var type = Enum.GetUnderlyingType(typeof(T));
                return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(nint);
            }
        }
    }
}