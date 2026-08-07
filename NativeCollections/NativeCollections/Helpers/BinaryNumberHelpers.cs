using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Defines a number that is represented in a base-2 format.
    /// </summary>
    internal static class BinaryNumberHelpers
    {
        /// <summary>
        ///     Determines if a value represents an odd integral number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOddInteger<T>(T value) where T : unmanaged
#if NET7_0_OR_GREATER
            , IBinaryNumber<T>
#endif
        {
#if NET7_0_OR_GREATER
            return T.IsOddInteger(value);
#else
            if (Unsafe.SizeOf<T>() == 1)
                return (Unsafe.As<T, byte>(ref value) & 1) != 0;

            if (Unsafe.SizeOf<T>() == 2)
                return (Unsafe.As<T, ushort>(ref value) & 1) != 0;

            if (Unsafe.SizeOf<T>() == 4)
                return (Unsafe.As<T, uint>(ref value) & 1) != 0;

            if (Unsafe.SizeOf<T>() == 8)
                return (Unsafe.As<T, ulong>(ref value) & 1) != 0;

            ThrowHelpers.ThrowNotSupportedException();
            return default;
#endif
        }
    }
}