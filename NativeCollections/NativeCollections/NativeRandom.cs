using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native random
    /// </summary>
    public static unsafe class NativeRandom
    {
        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Next32()
        {
            uint num;
            RandomNumberGenerator.Fill(MemoryMarshal.CreateSpan(ref *(byte*)&num, 4));
            return num;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Next64()
        {
            ulong num;
            RandomNumberGenerator.Fill(MemoryMarshal.CreateSpan(ref *(byte*)&num, 8));
            return num;
        }

        /// <summary>
        ///     Performs an in-place shuffle of a span.
        /// </summary>
        /// <param name="buffer">The span to shuffle.</param>
        /// <typeparam name="T">The type of span.</typeparam>
        /// <remarks>
        ///     This method uses <see cref="NextInt32(int, int)" /> to choose values for shuffling.
        ///     This method is an O(n) operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(Span<T> buffer)
        {
            var length = buffer.Length;
            for (var i = 0; i < length - 1; ++i)
            {
                var j = NextInt32(i, length);
                if (j != i)
                    (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextUInt32()
        {
            uint num;
            do
            {
                num = Next32();
            } while (num == uint.MaxValue);

            return num;
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
        public static uint NextUInt32(uint maxValue)
        {
            var num1 = maxValue * (ulong)Next32();
            var num2 = (uint)num1;
            if (num2 < maxValue)
            {
                for (var index = (uint)-(int)maxValue % maxValue; num2 < index; num2 = (uint)num1)
                    num1 = maxValue * (ulong)Next32();
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
        public static uint NextUInt32(uint minValue, uint maxValue)
        {
            var num1 = maxValue - minValue;
            var num2 = num1 * (ulong)Next32();
            var num3 = (uint)num2;
            if (num3 < num1)
            {
                for (var index = (uint)-(int)num1 % num1; num3 < index; num3 = (uint)num2)
                    num2 = num1 * (ulong)Next32();
            }

            return (uint)(num2 >> 32) + minValue;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextUInt64()
        {
            ulong num;
            do
            {
                num = Next64();
            } while (num == ulong.MaxValue);

            return num;
        }

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
        public static ulong NextUInt64(ulong maxValue)
        {
            ulong low;
            var num1 = MathHelpers.BigMul(maxValue, Next64(), out low);
            if (low < maxValue)
            {
                var num2 = unchecked(0UL - maxValue) % maxValue;
                while (low < num2)
                    num1 = MathHelpers.BigMul(maxValue, Next64(), out low);
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
        public static ulong NextUInt64(ulong minValue, ulong maxValue)
        {
            var a = maxValue - minValue;
            ulong low;
            var num1 = MathHelpers.BigMul(a, Next64(), out low);
            if (low < a)
            {
                var num2 = unchecked(0UL - a) % a;
                while (low < num2)
                    num1 = MathHelpers.BigMul(a, Next64(), out low);
            }

            return num1 + minValue;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextInt32()
        {
            if (IntPtr.Size == 8)
            {
                ulong num;
                do
                {
                    num = Next64() >> 33;
                } while (num == int.MaxValue);

                return (int)num;
            }
            else
            {
                uint num;
                do
                {
                    num = Next32() >> 1;
                } while (num == int.MaxValue);

                return (int)num;
            }
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
        public static int NextInt32(int maxValue)
        {
            var num1 = (ulong)maxValue * Next32();
            var num2 = (uint)num1;
            if (num2 < maxValue)
            {
                for (var index = (uint)((uint)-maxValue % (ulong)maxValue); num2 < index; num2 = (uint)num1)
                    num1 = (ulong)maxValue * Next32();
            }

            return (int)(num1 >> 32);
        }

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
        public static int NextInt32(int minValue, int maxValue)
        {
            var num1 = (uint)(maxValue - minValue);
            var num2 = num1 * (ulong)Next32();
            var num3 = (uint)num2;
            if (num3 < num1)
            {
                for (var index = (uint)-(int)num1 % num1; num3 < index; num3 = (uint)num2)
                    num2 = num1 * (ulong)Next32();
            }

            return (int)(uint)(num2 >> 32) + minValue;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextInt64()
        {
            ulong num;
            do
            {
                num = Next64() >> 1;
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
        public static long NextInt64(long maxValue)
        {
            var a = (ulong)maxValue;
            ulong low;
            var num1 = MathHelpers.BigMul(a, Next64(), out low);
            if (low < a)
            {
                var num2 = unchecked(0UL - a) % a;
                while (low < num2)
                    num1 = MathHelpers.BigMul(a, Next64(), out low);
            }

            return (long)num1;
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
        public static long NextInt64(long minValue, long maxValue)
        {
            var a = (ulong)(maxValue - minValue);
            ulong low;
            var num1 = MathHelpers.BigMul(a, Next64(), out low);
            if (low < a)
            {
                var num2 = unchecked(0UL - a) % a;
                while (low < num2)
                    num1 = MathHelpers.BigMul(a, Next64(), out low);
            }

            return (long)num1 + minValue;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NextDouble() => (Next64() >> 11) * 1.1102230246251565E-16;

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextSingle() => (IntPtr.Size == 8 ? Next64() >> 40 : Next32() >> 8) * 5.9604645E-08f;

        /// <summary>Fills the elements of a specified span of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes(Span<byte> buffer) => RandomNumberGenerator.Fill(buffer);

        /// <summary>Returns a boolean.</summary>
        /// <returns>True, or false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBoolean() => ((IntPtr.Size == 8 ? Next64() : Next32()) & 1) == 0;
    }
}