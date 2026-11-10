using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces
    ///     a sequence of numbers that meet certain statistical requirements for randomness.
    /// </summary>
    internal static unsafe class RandomHelpers
    {
        /// <summary>
        ///     Initializes the state using random bytes, retrying until the state is successfully created.
        /// </summary>
        /// <typeparam name="TRandom">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize<TRandom>(ref TRandom random) where TRandom : unmanaged, IIsCreated, IRandom
        {
            var data = MemoryMarshalHelpers.AsBytes(ref random);
            while (true)
            {
                NativeRandom.NextBytes(data);

                if (random.IsCreated)
                    break;
            }
        }

        /// <summary>
        ///     Reads a value of type <typeparamref name="TState" />
        ///     from the given byte span without assuming alignment.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="buffer">The byte span containing the raw data to read.</param>
        /// <returns>The pseudo-random number generator.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the length of <paramref name="buffer" /> is
        ///     less than the size of <typeparamref name="TState" />.
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
        ///     Returns a random 64-bit double-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive, or
        ///     (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned when
        ///     <paramref name="maxValue" /> is positive.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is
        ///     negative. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF64{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpF64<TState>(ref this TState random, double maxValue) where TState : unmanaged, IRandomState => random.NextF64() * maxValue;

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is, the range of return values
        ///     includes <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF64{TState}(ref TState)" />
        /// <remarks>
        ///     Due to floating-point rounding, the returned value may equal
        ///     <paramref name="maxValue" /> even though the range is nominally exclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double LerpF64<TState>(ref this TState random, double minValue, double maxValue) where TState : unmanaged, IRandomState => random.NextF64() * (maxValue - minValue) + minValue;

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive, or
        ///     (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned when
        ///     <paramref name="maxValue" /> is positive.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is
        ///     negative. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF32{TState}(ref TState)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF32<TState>(ref this TState random, float maxValue) where TState : unmanaged, IRandomState => random.NextF32() * maxValue;

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is, the range of return values
        ///     includes <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF32{TState}(ref TState)" />
        /// <remarks>
        ///     Due to floating-point rounding, the returned value may equal
        ///     <paramref name="maxValue" /> even though the range is nominally exclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpF32<TState>(ref this TState random, float minValue, float maxValue) where TState : unmanaged, IRandomState => random.NextF32() * (maxValue - minValue) + minValue;

        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <returns>
        ///     A string populated with items selected at random from <paramref name="source" />.
        ///     If <paramref name="stringLength" /> is zero, an empty string is returned.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="stringLength" /> is negative.
        /// </exception>
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

            var destination = new string(Char.FromI32Unchecked(0), stringLength);
            random.GetItems(source, MemoryMarshalHelpers.AsSpan(destination.AsSpan()));
            return destination;
        }

        /// <summary>
        ///     Creates a string filled with random hexadecimal characters.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="stringLength">The length of string to create.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase. The default is <see langword="false" />.
        /// </param>
        /// <returns>A string populated with random hexadecimal characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="stringLength" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHexString<TState>(ref this TState random, int stringLength, bool lowercase = false) where TState : unmanaged, IRandomState => random.GetString(GetHexChoices(lowercase), stringLength);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase. The default is <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetHexString<TState>(ref this TState random, Span<char> destination, bool lowercase = false) where TState : unmanaged, IRandomState => random.GetItems(GetHexChoices(lowercase), destination);

        /// <summary>
        ///     Gets all possible hex characters for the specified casing.
        /// </summary>
        /// <param name="lowercase">
        ///     <see langword="true" /> to return lowercase hex characters;
        ///     <see langword="false" /> to return uppercase hex characters.
        /// </param>
        /// <returns>The hexadecimal digit characters in the requested case.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<char> GetHexChoices(bool lowercase) => lowercase ? "0123456789abcdef" : "0123456789ABCDEF";

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <remarks>
        ///     This method uses <see cref="NextI32{TState}(ref TState, int, int)" />
        ///     to choose values for shuffling. This method is an O(n) operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<TState, T>(ref this TState random, Span<T> buffer) where TState : unmanaged, IRandomState
        {
            var length = buffer.Length;
            for (var i = 0; i < length.WrappingSub(1); i = i.WrappingAdd(1))
            {
                var j = random.NextI32(i, length);
                if (j != i)
                    (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
        }

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static void GetItems<TState, T>(ref this TState random, ReadOnlySpan<T> source, Span<T> destination) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(source, ExceptionArgument.source);
            if (source.Length <= 256)
            {
                Span<byte> buffer = stackalloc byte[512];
                if (BitOperationsHelpers.IsPow2(U32.FromI32Unchecked(source.Length)))
                {
                    var num = source.Length.WrappingSub(1);
                    for (; !destination.IsEmpty; destination = destination.Slice(buffer.Length))
                    {
                        if (destination.Length < buffer.Length)
                            buffer = buffer.Slice(0, destination.Length);

                        random.NextBytes(buffer);
                        for (var index = 0; index < buffer.Length; index = index.WrappingAdd(1))
                            destination[index] = source[I32.FromU8Unchecked(buffer[index]) & num];
                    }
                }
                else
                {
                    var num1 = I32.FromU32Unchecked(BitOperationsHelpers.RoundUpToPowerOf2(U32.FromI32Unchecked(source.Length))).WrappingSub(1);
                    int start;
                    for (; !destination.IsEmpty; destination = destination.Slice(start))
                    {
                        if (destination.Length < buffer.Length >> 1)
                            buffer = buffer.Slice(0, destination.Length.WrappingMul(2));

                        random.NextBytes(buffer);
                        start = 0;
                        var span = buffer;
                        for (var index1 = 0; index1 < span.Length; index1 = index1.WrappingAdd(1))
                        {
                            var num2 = span[index1];
                            if (U32.FromI32Unchecked(start) < U32.FromI32Unchecked(destination.Length))
                            {
                                var index2 = U8.FromU32Unchecked(U32.FromU8Unchecked(num2) & U32.FromI32Unchecked(num1));
                                if (U32.FromU8Unchecked(index2) < U32.FromI32Unchecked(source.Length))
                                    destination[(start = start.WrappingAdd(1)).WrappingSub(1)] = source[I32.FromU8Unchecked(index2)];
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                for (var index = 0; index < destination.Length; index = index.WrappingAdd(1))
                    destination[index] = source[random.NextI32(source.Length)];
            }
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Choose<TState, T>(ref this TState random, Span<T> buffer) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfSpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(random.NextI32(length)));
        }

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref readonly T ChooseReadOnly<TState, T>(ref this TState random, ReadOnlySpan<T> buffer) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfReadOnlySpanEmpty(buffer, ExceptionArgument.buffer);
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var length = buffer.Length;
            return ref length == 1 ? ref reference : ref Unsafe.Add(ref reference, Isize.FromI32Unchecked(random.NextI32(length)));
        }

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            uint num;
            while (true)
            {
                num = random.Next32();

                if (num != uint.MaxValue)
                    break;
            }

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
        ///     A 32-bit unsigned integer that is greater than or equal to 0, and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes 0 but not
        ///     <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random, uint maxValue) where TState : unmanaged, IRandomState
        {
            var num1 = U64.FromU32Unchecked(maxValue).WrappingMul(U64.FromU32Unchecked(random.Next32()));
            var num2 = U32.FromU64Unchecked(num1);
            if (num2 < maxValue)
            {
                for (var num3 = maxValue.WrappingNeg() % maxValue; num2 < num3; num2 = U32.FromU64Unchecked(num1))
                    num1 = U64.FromU32Unchecked(maxValue).WrappingMul(U64.FromU32Unchecked(random.Next32()));
            }

            return U32.FromU64Unchecked(num1 >> 32);
        }

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NextU32<TState>(ref this TState random, uint minValue, uint maxValue) where TState : unmanaged, IRandomState => random.NextU32(maxValue.WrappingSub(minValue)).WrappingAdd(minValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or
        ///     equal to 0 and less than <see cref="ulong.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            ulong num;
            while (true)
            {
                num = random.Next64();

                if (num != ulong.MaxValue)
                    break;
            }

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
        ///     A 64-bit unsigned integer that is greater than or equal to 0, and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes 0 but not
        ///     <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random, ulong maxValue) where TState : unmanaged, IRandomState
        {
            var num1 = MathHelpers.BigMul(maxValue, random.Next64(), out var num2);
            if (num2 < maxValue)
            {
                var num3 = maxValue.WrappingNeg() % maxValue;
                while (num2 < num3)
                    num1 = MathHelpers.BigMul(maxValue, random.Next64(), out num2);
            }

            return num1;
        }

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong NextU64<TState>(ref this TState random, ulong minValue, ulong maxValue) where TState : unmanaged, IRandomState => random.NextU64(maxValue.WrappingSub(minValue)).WrappingAdd(minValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            uint num;
            while (true)
            {
                num = random.Next32() >> 1;

                if (num != U32.FromI32Unchecked(int.MaxValue))
                    break;
            }

            return I32.FromU32Unchecked(num);
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
        ///     A 32-bit signed integer that is greater than or equal to 0, and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes 0 but not
        ///     <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random, int maxValue) where TState : unmanaged, IRandomState => I32.FromU32Unchecked(random.NextU32(U32.FromI32Unchecked(maxValue)));

        /// <summary>
        ///     Returns a random integer that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextI32<TState>(ref this TState random, int minValue, int maxValue) where TState : unmanaged, IRandomState => I32.FromU32Unchecked(random.NextU32(U32.FromI32Unchecked(maxValue.WrappingSub(minValue)))).WrappingAdd(minValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random) where TState : unmanaged, IRandomState
        {
            ulong num;
            while (true)
            {
                num = random.Next64() >> 1;

                if (num != U64.FromI64Unchecked(long.MaxValue))
                    break;
            }

            return I64.FromU64Unchecked(num);
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
        ///     A 64-bit signed integer that is greater than or equal to 0, and less than
        ///     <paramref name="maxValue" />; that is, the range of return values includes 0 but not
        ///     <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random, long maxValue) where TState : unmanaged, IRandomState => I64.FromU64Unchecked(random.NextU64(U64.FromI64Unchecked(maxValue)));

        /// <summary>
        ///     Returns a random integer that is within a specified range.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long NextI64<TState>(ref this TState random, long minValue, long maxValue) where TState : unmanaged, IRandomState => I64.FromU64Unchecked(random.NextU64(U64.FromI64Unchecked(maxValue.WrappingSub(minValue)))).WrappingAdd(minValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double NextF64<TState>(ref this TState random) where TState : unmanaged, IRandomState => F64.FromU64Unchecked(random.Next64() >> 11) * 1.1102230246251565E-16;

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>
        ///     A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextF32<TState>(ref this TState random) where TState : unmanaged, IRandomState => F32.FromU32Unchecked(random.Next32() >> 8) * 5.9604645E-08F;

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes<TState>(ref this TState random, void* startAddress, nuint byteCount) where TState : unmanaged, IRandomState => random.NextBytes(ref Unsafe.AsRef<byte>(startAddress), byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void NextBytes<TState>(ref this TState random, ref byte startAddress, nuint byteCount) where TState : unmanaged, IRandomState
        {
            for (nuint count; byteCount > Usize.FromI32Unchecked(0); byteCount = byteCount.WrappingSub(count), startAddress = ref Unsafe.AddByteOffset(ref startAddress, Isize.FromUsizeUnchecked(count)))
            {
                count = byteCount > Usize.FromI32Unchecked(int.MaxValue) ? Usize.FromI32Unchecked(int.MaxValue) : byteCount;
                random.NextBytes(MemoryMarshal.CreateSpan(ref startAddress, I32.FromUsizeUnchecked(count)));
            }
        }

        /// <summary>
        ///     Generates a random <see cref="bool" /> value.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="trueProbability">
        ///     The probability of returning <see langword="true" />, in the range [0.0, 1.0].
        /// </param>
        /// <returns><see langword="true" /> or <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="trueProbability" /> is outside the range [0.0, 1.0].
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NextBool<TState>(ref this TState random, double trueProbability) where TState : unmanaged, IRandomState
        {
            ThrowHelpers.ThrowIfProbabilityOutOfRange(trueProbability, ExceptionArgument.trueProbability);
            return random.NextF64() >= 1.0 - trueProbability;
        }

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        public static T Next<TState, T>(ref this TState random) where TState : unmanaged, IRandomState where T : unmanaged
        {
            Unsafe.SkipInit(out T result);
            random.Next(ref result);
            return result;
        }

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <typeparam name="TState">The type of pseudo-random number generator.</typeparam>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Next<TState, T>(ref this TState random, ref T destination) where TState : unmanaged, IRandomState where T : unmanaged => random.NextBytes(MemoryMarshalHelpers.AsBytes(ref destination));
    }
}