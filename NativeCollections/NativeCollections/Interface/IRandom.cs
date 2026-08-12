using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
    ///     that meet certain statistical requirements for randomness.
    /// </summary>
    public unsafe interface IRandom
    {
        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stringLength" /> is not zero or a positive number.</exception>
        string GetString(ReadOnlySpan<char> source, int stringLength);

        /// <summary>
        ///     Creates a string filled with random hexadecimal characters.
        /// </summary>
        /// <param name="stringLength">The length of string to create.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        /// <returns>A string populated with random hexadecimal characters.</returns>
        string GetHexString(int stringLength, bool lowercase = false);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        void GetHexString(Span<char> destination, bool lowercase = false);

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        void Shuffle<T>(Span<T> buffer);

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="source" /> is empty.
        /// </exception>
        void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        ref T Choose<T>(Span<T> buffer);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        ref readonly T ChooseReadOnly<T>(ReadOnlySpan<T> buffer);

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number
        ///     that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number
        ///     in the range [0, <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        double LerpF64(double maxValue);

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number
        ///     that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        double LerpF64(double minValue, double maxValue);

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number
        ///     that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number
        ///     in the range [0, <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is negative.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        float LerpF32(float maxValue);

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number
        ///     that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        float LerpF32(float minValue, float maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
        uint NextU32();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        uint NextU32(uint maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        uint NextU32(uint minValue, uint maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
        ulong NextU64();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        ulong NextU64(ulong maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        ulong NextU64(ulong minValue, ulong maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
        int NextI32();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        int NextI32(int maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        int NextI32(int minValue, int maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
        long NextI64();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to 0.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        long NextI64(long maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        ///     <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />.
        ///     However, if minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        long NextI64(long minValue, long maxValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        double NextF64();

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        float NextF32();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        void NextBytes(Span<byte> buffer);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        void NextBytes(void* startAddress, uint byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        void NextBytes(ref byte startAddress, uint byteCount);

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <returns>True, or false.</returns>
        bool NextBool();

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <param name="trueProbability">A probability of <see langword="true" /> result, should be in the range [0.0, 1.0].</param>
        /// <returns>True, or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trueProbability" /> value is invalid.</exception>
        bool NextBool(double trueProbability);

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        T Next<T>() where T : unmanaged;

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        void Next<T>(ref T destination) where T : unmanaged;
    }
}