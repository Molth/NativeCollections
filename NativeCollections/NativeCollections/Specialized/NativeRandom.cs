using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native random
    /// </summary>
    [BindingType(typeof(RandomNumberGenerator))]
    public static unsafe class NativeRandom
    {
        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Next32() => Next<uint>();

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Next64() => Next<ulong>();

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
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

        /// <summary>Fills the elements of a specified buffer with items chosen at random from the provided set of choices.</summary>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="source" /> is empty.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination)
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(source, ExceptionArgument.source);
            if (source.Length <= 256)
            {
                Span<byte> buffer = stackalloc byte[512];
                if (BitOperationsHelpers.IsPow2(source.Length))
                {
                    var num = source.Length - 1;
                    for (; !destination.IsEmpty; destination = destination.Slice(buffer.Length))
                    {
                        if (destination.Length < buffer.Length)
                            buffer = buffer.Slice(0, destination.Length);
                        NextBytes(buffer);
                        for (var index = 0; index < buffer.Length; ++index)
                            destination[index] = source[buffer[index] & num];
                    }
                }
                else
                {
                    var num1 = (int)BitOperationsHelpers.RoundUpToPowerOf2((uint)source.Length) - 1;
                    int start;
                    for (; !destination.IsEmpty; destination = destination.Slice(start))
                    {
                        if (destination.Length * 2 < buffer.Length)
                            buffer = buffer.Slice(0, destination.Length * 2);
                        NextBytes(buffer);
                        start = 0;
                        var span = buffer;
                        for (var index1 = 0; index1 < span.Length; ++index1)
                        {
                            var num2 = span[index1];
                            if ((uint)start < (uint)destination.Length)
                            {
                                var index2 = (byte)(num2 & (uint)num1);
                                if (index2 < (uint)source.Length)
                                    destination[start++] = source[index2];
                            }
                            else
                                break;
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < destination.Length; ++index)
                    destination[index] = source[NextInt32(source.Length)];
            }
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Sample<T>(Span<T> buffer)
        {
            ThrowHelpers.ThrowIfSpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, (nint)NextInt32(length));
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T Peek<T>(ReadOnlySpan<T> buffer)
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, (nint)NextInt32(length));
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
            if (Environment.Is64BitProcess)
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
        public static float NextSingle() => (Environment.Is64BitProcess ? Next64() >> 40 : Next32() >> 8) * 5.9604645E-08f;

        /// <summary>Fills the elements of a specified buffer of bytes with random numbers.</summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes(Span<byte> buffer) => RandomNumberGenerator.Fill(buffer);

        /// <summary>Fills a specified memory block with random bytes.</summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes(void* startAddress, uint byteCount) => NextBytes(ref Unsafe.AsRef<byte>(startAddress), byteCount);

        /// <summary>Fills a specified memory block with random bytes.</summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes(ref byte startAddress, uint byteCount)
        {
            for (uint count; byteCount > 0; byteCount -= count, startAddress = ref Unsafe.AddByteOffset(ref startAddress, (nint)count))
            {
                count = byteCount > int.MaxValue ? int.MaxValue : byteCount;
                NextBytes(MemoryMarshal.CreateSpan(ref startAddress, (int)count));
            }
        }

        /// <summary>Returns a boolean.</summary>
        /// <returns>True, or false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBoolean() => ((Environment.Is64BitProcess ? Next64() : Next32()) & 1) == 0;

        /// <summary>
        ///     Generates a random boolean value.
        /// </summary>
        /// <param name="trueProbability">A probability of <see langword="true" /> result (should be between 0.0 and 1.0).</param>
        /// <returns>Randomly generated boolean value.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trueProbability" /> value is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBoolean(double trueProbability)
        {
            ThrowHelpers.ThrowIfProbabilityOutOfRange(trueProbability, ExceptionArgument.trueProbability);
            return NextDouble() >= 1.0 - trueProbability;
        }

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Next<T>() where T : unmanaged
        {
            Unsafe.SkipInit(out T result);
            Next(ref result);
            return result;
        }

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Next<T>(ref T destination) => NextBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref destination), Unsafe.SizeOf<T>()));
    }
}