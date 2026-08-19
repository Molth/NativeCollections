using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
    ///     that meet certain statistical requirements for randomness.
    /// </summary>
    internal static unsafe class RandomHelpers
    {
        /// <summary>
        ///     Reads a value of type <typeparamref name="TState" /> from the given byte span without assuming alignment.
        /// </summary>
        /// <param name="buffer">The byte span containing the raw data to read.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>The pseudo-random number generator.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="buffer" />'s length is less than than the size of <typeparamref name="TState" />.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the pseudo-random number generator indicates it is not created.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TState ReadUnaligned<TState>(ReadOnlySpan<byte> buffer) where TState : unmanaged, IIsCreated, IRandomState
        {
            ThrowHelpers.ThrowIfLessThan(buffer.Length, Unsafe.SizeOf<TState>(), ExceptionArgument.buffer);
            var random = Unsafe.ReadUnaligned<TState>(ref MemoryMarshal.GetReference(buffer));
            ThrowHelpers.ThrowIfNotCreated(ref random, ExceptionArgument.buffer);
            return random;
        }

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize<TState>(ref this TState random) where TState : unmanaged, IIsCreated, IRandomState
        {
            var data = MemoryMarshalHelpers.AsBytes(ref random);
            do
            {
                NativeRandom.NextBytes(data);
            } while (!random.IsCreated);
        }

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number
        ///     that is less than the specified maximum.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 64-bit double-precision floating point number
        ///     in the range [0, <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF64{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpF64<TState>(ref this TState random, double maxValue) where TState : unmanaged, IRandomState => random.NextF64() * maxValue;

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number
        ///     that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 64-bit double-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF64{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpF64<TState>(ref this TState random, double minValue, double maxValue) where TState : unmanaged, IRandomState => random.NextF64() * (maxValue - minValue) + minValue;

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number
        ///     that is less than the specified maximum.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 32-bit single-precision floating point number
        ///     in the range [0, <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF32{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF32<TState>(ref this TState random, float maxValue) where TState : unmanaged, IRandomState => random.NextF32() * maxValue;

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number
        ///     that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 32-bit single-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF32{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF32<TState>(ref this TState random, float minValue, float maxValue) where TState : unmanaged, IRandomState => random.NextF32() * (maxValue - minValue) + minValue;

        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stringLength" /> is not zero or a positive number.</exception>
        /// <seealso cref="GetItems{TState, T}(ref TState, ReadOnlySpan{T}, Span{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString<TState>(ref this TState random, ReadOnlySpan<char> source, int stringLength) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(source, ExceptionArgument.source);
            if (stringLength <= 0)
            {
                ThrowHelpers.ThrowIfNegative(stringLength, ExceptionArgument.stringLength);
                return "";
            }

            var destination = new string((char)0, stringLength);
            random.GetItems(source, MemoryMarshalHelpers.AsSpan(destination.AsSpan()));
            return destination;
        }

        /// <summary>
        ///     Creates a string filled with random hexadecimal characters.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="stringLength">The length of string to create.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>A string populated with random hexadecimal characters.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHexString<TState>(ref this TState random, int stringLength, bool lowercase = false) where TState : unmanaged, IRandomState => random.GetString(GetHexChoices(lowercase), stringLength);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetHexString<TState>(ref this TState random, Span<char> destination, bool lowercase = false) where TState : unmanaged, IRandomState => random.GetItems(GetHexChoices(lowercase), destination);

        /// <summary>
        ///     Gets all possible hex characters for the specified casing.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetHexChoices(bool lowercase) => lowercase ? "0123456789abcdef" : "0123456789ABCDEF";

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <remarks>
        ///     This method uses <see cref="NextI32{TState}(ref TState, int, int)" /> to choose values for shuffling.
        ///     This method is an O(n) operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<TState, T>(ref this TState random, Span<T> buffer) where TState : unmanaged, IRandomState
        {
            var length = buffer.Length;
            for (var i = 0; i < length - 1; ++i)
            {
                var j = random.NextI32(i, length);
                if (j != i)
                    (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="source" /> is empty.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetItems<TState, T>(ref this TState random, ReadOnlySpan<T> source, Span<T> destination) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(source, ExceptionArgument.source);
            if (source.Length <= 256)
            {
                Span<byte> buffer = stackalloc byte[512];
                if (BitOperationsHelpers.IsPow2((uint)source.Length))
                {
                    var num = source.Length - 1;
                    for (; !destination.IsEmpty; destination = destination.Slice(buffer.Length))
                    {
                        if (destination.Length < buffer.Length)
                            buffer = buffer.Slice(0, destination.Length);
                        random.NextBytes(buffer);
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
                        random.NextBytes(buffer);
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
                    destination[index] = source[random.NextI32(source.Length)];
            }
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Choose<TState, T>(ref this TState random, Span<T> buffer) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfSpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, (nint)random.NextI32(length));
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T ChooseReadOnly<TState, T>(ref this TState random, ReadOnlySpan<T> buffer) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, (nint)random.NextI32(length));
        }

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            uint num;
            do
            {
                num = random.Next32();
            } while (num == uint.MaxValue);

            return num;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />. However, if
        ///     <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random, uint maxValue) where TState : unmanaged, IRandomState
        {
            var num1 = maxValue * (ulong)random.Next32();
            var num2 = (uint)num1;
            if (num2 < maxValue)
            {
                for (var num3 = unchecked(0U - maxValue); num2 < num3; num2 = (uint)num1)
                    num1 = maxValue * (ulong)random.Next32();
            }

            return (uint)(num1 >> 32);
        }

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 32-bit unsigned integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random, uint minValue, uint maxValue) where TState : unmanaged, IRandomState => random.NextU32(maxValue - minValue) + minValue;

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            ulong num;
            do
            {
                num = random.Next64();
            } while (num == ulong.MaxValue);

            return num;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />. However, if
        ///     <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random, ulong maxValue) where TState : unmanaged, IRandomState
        {
            var num1 = MathHelpers.BigMul(maxValue, random.Next64(), out var num2);
            if (num2 < maxValue)
            {
                var num3 = unchecked(0UL - maxValue) % maxValue;
                while (num2 < num3)
                    num1 = MathHelpers.BigMul(maxValue, random.Next64(), out num2);
            }

            return num1;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 64-bit unsigned integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random, ulong minValue, ulong maxValue) where TState : unmanaged, IRandomState => random.NextU64(maxValue - minValue) + minValue;

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            uint num;
            do
            {
                num = random.Next32() >> 1;
            } while (num == int.MaxValue);

            return (int)num;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />. However, if
        ///     <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random, int maxValue) where TState : unmanaged, IRandomState => (int)random.NextU32((uint)maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 32-bit signed integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random, int minValue, int maxValue) where TState : unmanaged, IRandomState => (int)random.NextU32((uint)(maxValue - minValue)) + minValue;

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            ulong num;
            do
            {
                num = random.Next64() >> 1;
            } while (num == long.MaxValue);

            return (long)num;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />. However, if
        ///     <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random, long maxValue) where TState : unmanaged, IRandomState => (long)random.NextU64((ulong)maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>
        ///     A 64-bit signed integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random, long minValue, long maxValue) where TState : unmanaged, IRandomState => (long)random.NextU64((ulong)(maxValue - minValue)) + minValue;

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NextF64<TState>(ref this TState random) where TState : unmanaged, IRandomState => (random.Next64() >> 11) * 1.1102230246251565E-16;

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextF32<TState>(ref this TState random) where TState : unmanaged, IRandomState => (random.Next32() >> 8) * 5.9604645E-08f;

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes<TState>(ref this TState random, void* startAddress, uint byteCount) where TState : unmanaged, IRandomState => random.NextBytes(ref Unsafe.AsRef<byte>(startAddress), byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes<TState>(ref this TState random, ref byte startAddress, uint byteCount) where TState : unmanaged, IRandomState
        {
            for (uint count; byteCount > 0; byteCount -= count, startAddress = ref Unsafe.AddByteOffset(ref startAddress, (nint)count))
            {
                count = byteCount > int.MaxValue ? int.MaxValue : byteCount;
                random.NextBytes(MemoryMarshal.CreateSpan(ref startAddress, (int)count));
            }
        }

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="trueProbability">A probability of <see langword="true" /> result, should be in the range [0.0, 1.0].</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <returns>True, or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trueProbability" /> value is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBool<TState>(ref this TState random, double trueProbability) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfProbabilityOutOfRange(trueProbability, ExceptionArgument.trueProbability);
            return random.NextF64() >= 1.0 - trueProbability;
        }

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Next<TState, T>(ref this TState random) where TState : unmanaged, IRandomState where T : unmanaged
        {
            Unsafe.SkipInit(out T result);
            random.Next(ref result);
            return result;
        }

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Next<TState, T>(ref this TState random, ref T destination) where TState : unmanaged, IRandomState where T : unmanaged => random.NextBytes(MemoryMarshalHelpers.AsBytes(ref destination));
    }
}