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
        /// <typeparam name="T">The type of the value to test.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="value" /> represents
        ///     an odd integral number; otherwise, <see langword="false" />.
        /// </returns>
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
                return (I32.FromU8Unchecked(Unsafe.As<T, byte>(ref value)) & 1) != 0;

            if (Unsafe.SizeOf<T>() == 2)
                return (I32.FromU16Unchecked(Unsafe.As<T, ushort>(ref value)) & 1) != 0;

            if (Unsafe.SizeOf<T>() == 4)
                return (Unsafe.As<T, uint>(ref value) & U32.FromI32Unchecked(1)) != U32.FromI32Unchecked(0);

            if (Unsafe.SizeOf<T>() == 8)
                return (Unsafe.As<T, ulong>(ref value) & U64.FromI32Unchecked(1)) != U64.FromI32Unchecked(0);

            ThrowHelpers.ThrowNotSupportedException();
            return default;
#endif
        }
    }
}