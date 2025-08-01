using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif
#if NET5_0_OR_GREATER
using System.Numerics;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Bit operations helpers
    /// </summary>
    internal static class BitOperationsHelpers
    {
        /// <summary>
        ///     Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(uint value) => (value & (value - 1)) == 0 && value != 0;

        /// <summary>Rotates the specified value left by the specified number of bits.</summary>
        /// <param name="value">The value to rotate.</param>
        /// <param name="offset">
        ///     The number of bits to rotate by. Any value outside the range [0..31] is treated as congruent mod 32.
        /// </param>
        /// <returns>The rotated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));

        /// <summary>
        ///     Log2 ceiling
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Log2 ceiling</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2Ceiling(ulong value)
        {
            var num = Log2(value);
            if (PopCount(value) != 1)
                ++num;
            return num;
        }

        /// <summary>
        ///     Returns the population count (number of bits set) of an unsigned 64-bit integer mask
        /// </summary>
        /// <param name="value">The mask</param>
        /// <returns>The population count of the mask</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.PopCount(value);
#else
            if (Unsafe.SizeOf<nint>() == 8)
            {
                value -= (value >> 1) & 6148914691236517205UL;
                value = (ulong)(((long)value & 3689348814741910323L) + ((long)(value >> 2) & 3689348814741910323L));
                value = (ulong)(long)((ulong)((((long)value + (long)(value >> 4)) & 1085102592571150095L) * 72340172838076673L) >> 56);
                return (int)value;
            }

            var value1 = (uint)value;
            value1 -= (value1 >> 1) & 0x_55555555u;
            value1 = (value1 & 0x_33333333u) + ((value1 >> 2) & 0x_33333333u);
            value1 = (((value1 + (value1 >> 4)) & 0x_0F0F0F0Fu) * 0x_01010101u) >> 24;
            var value2 = (uint)(value >> 32);
            value2 -= (value2 >> 1) & 0x_55555555u;
            value2 = (value2 & 0x_33333333u) + ((value2 >> 2) & 0x_33333333u);
            value2 = (((value2 + (value2 >> 4)) & 0x_0F0F0F0Fu) * 0x_01010101u) >> 24;
            return (int)value1 + (int)value2;
#endif
        }

        /// <summary>
        ///     Count the number of leading zero bits in a mask
        ///     Similar in behavior to the x86 instruction LZCNT
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Leading zero count</returns>
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
        ///     Count the number of leading zero bits in a mask
        ///     Similar in behavior to the x86 instruction LZCNT
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Leading zero count</returns>
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
        ///     Count the number of trailing zero bits in an integer value
        ///     Similar in behavior to the x86 instruction TZCNT
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Trailing zero count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.TrailingZeroCount(value);
#else
            var low = (uint)value;
            return low == 0 ? 32 + TrailingZeroCount((uint)(value >> 32)) : Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn), (nint)(int)(((low & (uint)-(int)low) * 125613361U) >> 27));
#endif
        }

        /// <summary>
        ///     Count the number of trailing zero bits in an integer value
        ///     Similar in behavior to the x86 instruction TZCNT
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Trailing zero count</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount(uint value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.TrailingZeroCount(value);
#else
            return value == 0 ? 32 : Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(TrailingZeroCountDeBruijn), (nint)(int)(((value & (uint)-(int)value) * 125613361U) >> 27));
#endif
        }

        /// <summary>
        ///     Log2
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Log2</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.Log2(value);
#else
            value |= 1UL;
            var num = (uint)(value >> 32);
            return num == 0U ? Log2((uint)value) : 32 + Log2(num);
#endif
        }

        /// <summary>
        ///     Log2
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Log2</returns>
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

        /// <summary>
        ///     And
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void And(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] &= source[6];
                    goto case 6;
                case 6:
                    destination[5] &= source[5];
                    goto case 5;
                case 5:
                    destination[4] &= source[4];
                    goto case 4;
                case 4:
                    destination[3] &= source[3];
                    goto case 3;
                case 3:
                    destination[2] &= source[2];
                    goto case 2;
                case 2:
                    destination[1] &= source[1];
                    goto case 1;
                case 1:
                    destination[0] &= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
#if NET7_0_OR_GREATER
            uint i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                var n = count - 7;
                for (; i < n; i += 8)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) & Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var n = count - 3;
                for (; i < n; i += 4)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) & Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }

            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) &= Unsafe.Add(ref right, (nint)i);
#else
            var i = 0;
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) &= Unsafe.Add(ref right, (nint)i);
#endif
        }

        /// <summary>
        ///     Or
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Or(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] |= source[6];
                    goto case 6;
                case 6:
                    destination[5] |= source[5];
                    goto case 5;
                case 5:
                    destination[4] |= source[4];
                    goto case 4;
                case 4:
                    destination[3] |= source[3];
                    goto case 3;
                case 3:
                    destination[2] |= source[2];
                    goto case 2;
                case 2:
                    destination[1] |= source[1];
                    goto case 1;
                case 1:
                    destination[0] |= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
#if NET7_0_OR_GREATER
            uint i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                var n = count - 7;
                for (; i < n; i += 8)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) | Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var n = count - 3;
                for (; i < n; i += 4)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) | Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }

            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) |= Unsafe.Add(ref right, (nint)i);
#else
            var i = 0;
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) |= Unsafe.Add(ref right, (nint)i);
#endif
        }

        /// <summary>
        ///     Xor
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="source">Source</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Xor(Span<int> destination, Span<int> source, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] ^= source[6];
                    goto case 6;
                case 6:
                    destination[5] ^= source[5];
                    goto case 5;
                case 5:
                    destination[4] ^= source[4];
                    goto case 4;
                case 4:
                    destination[3] ^= source[3];
                    goto case 3;
                case 3:
                    destination[2] ^= source[2];
                    goto case 2;
                case 2:
                    destination[1] ^= source[1];
                    goto case 1;
                case 1:
                    destination[0] ^= source[0];
                    return;
                case 0:
                    return;
            }

            ref var left = ref MemoryMarshal.GetReference(destination);
            ref var right = ref MemoryMarshal.GetReference(source);
#if NET7_0_OR_GREATER
            uint i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                var n = count - 7;
                for (; i < n; i += 8)
                {
                    var result = Vector256.LoadUnsafe(ref left, i) ^ Vector256.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var n = count - 3;
                for (; i < n; i += 4)
                {
                    var result = Vector128.LoadUnsafe(ref left, i) ^ Vector128.LoadUnsafe(ref right, i);
                    result.StoreUnsafe(ref left, i);
                }
            }

            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) ^= Unsafe.Add(ref right, (nint)i);
#else
            var i = 0;
            for (; i < count; ++i)
                Unsafe.Add(ref left, (nint)i) ^= Unsafe.Add(ref right, (nint)i);
#endif
        }

        /// <summary>
        ///     Not
        /// </summary>
        /// <param name="destination">Destination</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Not(Span<int> destination, uint count)
        {
            switch (count)
            {
                case 7:
                    destination[6] = ~destination[6];
                    goto case 6;
                case 6:
                    destination[5] = ~destination[5];
                    goto case 5;
                case 5:
                    destination[4] = ~destination[4];
                    goto case 4;
                case 4:
                    destination[3] = ~destination[3];
                    goto case 3;
                case 3:
                    destination[2] = ~destination[2];
                    goto case 2;
                case 2:
                    destination[1] = ~destination[1];
                    goto case 1;
                case 1:
                    destination[0] = ~destination[0];
                    return;
                case 0:
                    return;
            }

            ref var value = ref MemoryMarshal.GetReference(destination);
#if NET7_0_OR_GREATER
            uint i = 0;
            if (Vector256.IsHardwareAccelerated)
            {
                var n = count - 7;
                for (; i < n; i += 8)
                {
                    var result = ~Vector256.LoadUnsafe(ref value, i);
                    result.StoreUnsafe(ref value, i);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                var n = count - 3;
                for (; i < n; i += 4)
                {
                    var result = ~Vector128.LoadUnsafe(ref value, i);
                    result.StoreUnsafe(ref value, i);
                }
            }

            for (; i < count; ++i)
                Unsafe.Add(ref value, (nint)i) = ~ Unsafe.Add(ref value, (nint)i);
#else
            var i = 0;
            for (; i < count; ++i)
                Unsafe.Add(ref value, (nint)i) = ~ Unsafe.Add(ref value, (nint)i);
#endif
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        ///     DeBruijn sequence
        /// </summary>
        private static ReadOnlySpan<byte> TrailingZeroCountDeBruijn => new byte[32]
        {
            0, 1, 28, 2, 29, 14, 24, 3,
            30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7,
            26, 12, 18, 6, 11, 5, 10, 9
        };

        /// <summary>
        ///     DeBruijn sequence
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