﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native random
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public unsafe ref struct NativeXorshift32
    {
        /// <summary>
        ///     State
        /// </summary>
        private uint _state;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeXorshift32");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("Cannot call GetHashCode on NativeXorshift32");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeXorshift32";

        /// <summary>
        ///     Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize() => _state = (uint)Stopwatch.GetTimestamp();

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt32()
        {
            var state = (int)_state;
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (uint)state;
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number to be generated. <paramref name="maxValue" />
        ///     must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values ordinarily
        ///     includes 0 but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0,
        ///     <paramref name="maxValue" /> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt32(uint maxValue)
        {
            var num1 = maxValue * (ulong)NextUInt32();
            var num2 = (uint)num1;
            if (num2 < maxValue)
            {
                for (var index = (uint)-(int)maxValue % maxValue; num2 < index; num2 = (uint)num1)
                    num1 = maxValue * (ulong)NextUInt32();
            }

            return (uint)(num1 >> 32);
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be
        ///     greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer greater than or equal to <paramref name="minValue" /> and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" />
        ///     but not <paramref name="maxValue" />. If minValue equals <paramref name="maxValue" />, <paramref name="minValue" />
        ///     is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="minValue" /> is greater than <paramref name="maxValue" />
        ///     .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextUInt32(uint minValue, uint maxValue)
        {
            var num1 = maxValue - minValue;
            var num2 = num1 * (ulong)NextUInt32();
            var num3 = (uint)num2;
            if (num3 < num1)
            {
                for (var index = (uint)-(int)num1 % num1; num3 < index; num3 = (uint)num2)
                    num2 = num1 * (ulong)NextUInt32();
            }

            return (uint)(num2 >> 32) + minValue;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextUInt64() => ((ulong)NextUInt32() << 32) | NextUInt32();

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number to be generated. <paramref name="maxValue" />
        ///     must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values ordinarily
        ///     includes 0 but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0,
        ///     <paramref name="maxValue" /> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextUInt64(ulong maxValue)
        {
            ulong low;
            var num1 = MathHelpers.BigMul(maxValue, NextUInt64(), out low);
            if (low < maxValue)
            {
                var num2 = unchecked(0UL - maxValue) % maxValue;
                while (low < num2)
                    num1 = MathHelpers.BigMul(maxValue, NextUInt64(), out low);
            }

            return num1;
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be
        ///     greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer greater than or equal to <paramref name="minValue" /> and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" />
        ///     but not <paramref name="maxValue" />. If minValue equals <paramref name="maxValue" />, <paramref name="minValue" />
        ///     is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="minValue" /> is greater than <paramref name="maxValue" />
        ///     .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            var a = maxValue - minValue;
            ulong low;
            var num1 = MathHelpers.BigMul(a, NextUInt64(), out low);
            if (low < a)
            {
                var num2 = unchecked(0UL - a) % a;
                while (low < num2)
                    num1 = MathHelpers.BigMul(a, NextUInt64(), out low);
            }

            return num1 + minValue;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt32()
        {
            uint num;
            do
            {
                num = NextUInt32() >> 1;
            } while (num == int.MaxValue);

            return (int)num;
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number to be generated. <paramref name="maxValue" />
        ///     must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values ordinarily
        ///     includes 0 but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0,
        ///     <paramref name="maxValue" /> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt32(int maxValue) => (int)NextUInt32((uint)maxValue);

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be
        ///     greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer greater than or equal to <paramref name="minValue" /> and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" />
        ///     but not <paramref name="maxValue" />. If minValue equals <paramref name="maxValue" />, <paramref name="minValue" />
        ///     is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="minValue" /> is greater than <paramref name="maxValue" />
        ///     .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt32(int minValue, int maxValue) => (int)NextUInt32((uint)(maxValue - minValue)) + minValue;

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextInt64()
        {
            ulong num;
            do
            {
                num = NextUInt64() >> 1;
            } while (num == long.MaxValue);

            return (long)num;
        }

        /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number to be generated. <paramref name="maxValue" />
        ///     must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values ordinarily
        ///     includes 0 but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0,
        ///     <paramref name="maxValue" /> is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextInt64(long maxValue)
        {
            if (maxValue <= int.MaxValue)
                return NextInt32((int)maxValue);
            if (maxValue <= 1L)
                return 0;
            var num1 = BitOperationsHelpers.Log2Ceiling((ulong)maxValue);
            ulong num2;
            do
            {
                num2 = NextUInt64() >> (64 - num1);
            } while (num2 >= (ulong)maxValue);

            return (long)num2;
        }

        /// <summary>Returns a random integer that is within a specified range.</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be
        ///     greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer greater than or equal to <paramref name="minValue" /> and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" />
        ///     but not <paramref name="maxValue" />. If minValue equals <paramref name="maxValue" />, <paramref name="minValue" />
        ///     is returned.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="minValue" /> is greater than <paramref name="maxValue" />
        ///     .
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextInt64(long minValue, long maxValue)
        {
            var maxValue1 = (ulong)(maxValue - minValue);
            if (maxValue1 <= int.MaxValue)
                return NextInt32((int)maxValue1) + minValue;
            if (maxValue1 <= 1UL)
                return minValue;
            var num1 = BitOperationsHelpers.Log2Ceiling(maxValue1);
            ulong num2;
            do
            {
                num2 = NextUInt64() >> (64 - num1);
            } while (num2 >= maxValue1);

            return (long)num2 + minValue;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextSingle() => (NextUInt32() >> 8) * 5.9604645E-08f;

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => (NextUInt64() >> 11) * 1.1102230246251565E-16;

        /// <summary>Fills the elements of a specified span of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(Span<byte> buffer)
        {
            var span = MemoryMarshal.CreateSpan(ref Unsafe.As<uint, byte>(ref _state), 4);
            uint num1 = span[0];
            uint num2 = span[1];
            uint num3 = span[2];
            uint num4 = span[3];
            for (; buffer.Length >= 4; buffer = buffer.Slice(4))
            {
                var num5 = num2 * 5U;
                Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), (uint)((((int)num5 << 7) | (int)(num5 >> 25)) * 9));
                var num6 = num2 << 9;
                var num7 = num3 ^ num1;
                var num8 = num4 ^ num2;
                num2 ^= num7;
                num1 ^= num8;
                num3 = num7 ^ num6;
                num4 = (num8 << 11) | (num8 >> 21);
            }

            if (!buffer.IsEmpty)
            {
                var num9 = num2 * 5U;
                var num10 = (uint)((((int)num9 << 7) | (int)(num9 >> 25)) * 9);
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref *(byte*)&num10, (uint)buffer.Length);
                var num11 = num2 << 9;
                var num12 = num3 ^ num1;
                var num13 = num4 ^ num2;
                num2 ^= num12;
                num1 ^= num13;
                num3 = num12 ^ num11;
                num4 = (num13 << 11) | (num13 >> 21);
            }

            span[0] = (byte)num1;
            span[1] = (byte)num2;
            span[2] = (byte)num3;
            span[3] = (byte)num4;
        }
    }
}