using System.Runtime.CompilerServices;
#if NET5_0_OR_GREATER
using System.Numerics;
#else
using System.Runtime.InteropServices;
#endif
#if !NET7_0_OR_GREATER
using System;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Utility methods for intrinsic bit-twiddling operations.
    ///     The methods use hardware intrinsics when available on the underlying platform,
    ///     otherwise they use optimized software fallbacks.
    /// </summary>
    internal static class BitOperationsHelpers
    {
        /// <summary>
        ///     Round the given integral value up to a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     The smallest power of 2 which is greater than or equal to <paramref name="value" />.
        ///     If <paramref name="value" /> is 0 or the result overflows, returns 0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundUpToPowerOf2(uint value)
        {
#if NET6_0_OR_GREATER
            return BitOperations.RoundUpToPowerOf2(value);
#else
            // Based on https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
            --value;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value + 1U;
#endif
        }

        /// <summary>
        ///     Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(uint value)
        {
#if NET6_0_OR_GREATER
            return BitOperations.IsPow2(value);
#else
            return (value & (value - 1)) == 0 && value != 0;
#endif
        }

        /// <summary>
        ///     Rotates the specified value left by the specified number of bits.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">
        ///     The number of bits to rotate by.
        ///     Any value outside the range [0..31] is treated as congruent mod 32 on a 32-bit process,
        ///     and any value outside the range [0..63] is treated as congruent mod 64 on a 64-bit process.
        /// </param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint RotateLeft(nuint value, int offset)
        {
#if NET7_0_OR_GREATER
            return BitOperations.RotateLeft(value, offset);
#else
            return Environment.Is64BitProcess ? (nuint)RotateLeft((ulong)value, offset) : RotateLeft((uint)value, offset);
#endif
        }

        /// <summary>
        ///     Rotates the specified value left by the specified number of bits.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">
        ///     The number of bits to rotate by.
        ///     Any value outside the range [0..63] is treated as congruent mod 64.
        /// </param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(ulong value, int offset)
        {
#if NET5_0_OR_GREATER
            return BitOperations.RotateLeft(value, offset);
#else
            return (value << offset) | (value >> (64 - offset));
#endif
        }

        /// <summary>
        ///     Rotates the specified value left by the specified number of bits.
        /// </summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">
        ///     The number of bits to rotate by.
        ///     Any value outside the range [0..31] is treated as congruent mod 32.
        /// </param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset)
        {
#if NET5_0_OR_GREATER
            return BitOperations.RotateLeft(value, offset);
#else
            return (value << offset) | (value >> (32 - offset));
#endif
        }

        /// <summary>
        ///     Count the number of leading zero bits in a mask.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.LeadingZeroCount(value);
#else
            var high = (uint)(value >> 32);
            return high == 0 ? 32 + LeadingZeroCount((uint)value) : 31 ^ Log2(high);
#endif
        }

        /// <summary>
        ///     Count the number of leading zero bits in a mask.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LeadingZeroCount(uint value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.LeadingZeroCount(value);
#else
            return value == 0 ? 32 : 31 ^ Log2(value);
#endif
        }

        /// <summary>
        ///     Count the number of trailing zero bits in a mask.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.TrailingZeroCount(value);
#else
            var low = (uint)value;
            return low == 0 ? 32 + TrailingZeroCount((uint)(value >> 32)) : TrailingZeroCount(low);
#endif
        }

        /// <summary>
        ///     Count the number of trailing zero bits in an integer value.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(uint value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.TrailingZeroCount(value);
#else
            return value == 0 ? 32 : Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn), (nint)(int)(((value & unchecked((uint)-(int)value)) * 125613361U) >> 27));
#endif
        }

        /// <summary>
        ///     Returns the integer (floor) log of the specified value, base 2.
        ///     Note that by convention, input value 0 returns 0 since log(0) is undefined.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.Log2(value);
#else
            value |= 1;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(Log2DeBruijn), (nint)(int)((value * 130329821U) >> 27));
#endif
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        ///     Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_0111_1100_1011_0101_0011_0001u
        /// </summary>
        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            0, 1, 28, 2, 29, 14, 24, 3,
            30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7,
            26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        ///     Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
        /// </summary>
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29,
            11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7,
            19, 27, 23, 6, 26, 5, 4, 31
        };
#endif
    }
}