using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces
    ///     a sequence of numbers that meet certain statistical requirements for randomness.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpecializedCollection(FromType.Standard)]
    [BindingType(typeof(RandomNumberGenerator))]
    public unsafe struct CryptographyRandom : IRandom, IEquatable<CryptographyRandom>
    {
        /// <summary>
        ///     Represents the state.
        /// </summary>
        private State _state;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(CryptographyRandom other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is CryptographyRandom other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "CryptographyRandom";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(CryptographyRandom left, CryptographyRandom right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(CryptographyRandom left, CryptographyRandom right) => !left.Equals(right);

        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="stringLength" /> is not zero or a positive number.
        /// </exception>
        /// <seealso cref="GetItems{T}(ReadOnlySpan{T}, Span{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(ReadOnlySpan<char> source, int stringLength) => _state.GetString(source, stringLength);

        /// <summary>
        ///     Creates a string filled with random hexadecimal characters.
        /// </summary>
        /// <param name="stringLength">The length of string to create.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase. The default is <see langword="false" />.
        /// </param>
        /// <returns>A string populated with random hexadecimal characters.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetHexString(int stringLength, bool lowercase = false) => _state.GetHexString(stringLength, lowercase);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase. The default is <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetHexString(Span<char> destination, bool lowercase = false) => _state.GetHexString(destination, lowercase);

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <remarks>
        ///     This method uses <see cref="NextI32(int, int)" /> to choose
        ///     values for shuffling. This method is an O(n) operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shuffle<T>(Span<T> buffer) => _state.Shuffle(buffer);

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination) => _state.GetItems(source, destination);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Choose<T>(Span<T> buffer) => ref _state.Choose(buffer);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T ChooseReadOnly<T>(ReadOnlySpan<T> buffer) => ref _state.ChooseReadOnly(buffer);

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is
        ///     negative. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF64()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double maxValue) => _state.LerpF64(maxValue);

        /// <summary>
        ///     Returns a random 64-bit double-precision floating point number that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit double-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is, the range of return values
        ///     includes <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF64()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double minValue, double maxValue) => _state.LerpF64(minValue, maxValue);

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number in the range [0,
        ///     <paramref name="maxValue" />) if <paramref name="maxValue" /> is positive,
        ///     or (<paramref name="maxValue" />, 0] if <paramref name="maxValue" /> is
        ///     negative. However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        /// <seealso cref="NextF32()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float maxValue) => _state.LerpF32(maxValue);

        /// <summary>
        ///     Returns a random 32-bit single-precision floating point number that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit single-precision floating point number greater than or equal to <paramref name="minValue" />,
        ///     and less than <paramref name="maxValue" />; that is, the range of return values
        ///     includes <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        /// <seealso cref="NextF32()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float minValue, float maxValue) => _state.LerpF32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32() => _state.NextU32();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
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
        public uint NextU32(uint maxValue) => _state.NextU32(maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit unsigned integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32(uint minValue, uint maxValue) => _state.NextU32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or
        ///     equal to 0 and less than <see cref="ulong.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64() => _state.NextU64();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
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
        public ulong NextU64(ulong maxValue) => _state.NextU64(maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit unsigned integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64(ulong minValue, ulong maxValue) => _state.NextU64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32() => _state.NextI32();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
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
        public int NextI32(int maxValue) => _state.NextI32(maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 32-bit signed integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32(int minValue, int maxValue) => _state.NextI32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64() => _state.NextI64();

        /// <summary>
        ///     Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
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
        public long NextI64(long maxValue) => _state.NextI64(maxValue);

        /// <summary>
        ///     Returns a non-negative random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        ///     The exclusive upper bound of the random number returned. <paramref name="maxValue" />
        ///     must be greater than or equal to <paramref name="minValue" />.
        /// </param>
        /// <returns>
        ///     A 64-bit signed integer greater than or equal to <paramref name="minValue" />, and
        ///     less than <paramref name="maxValue" />; that is, the range of return values includes
        ///     <paramref name="minValue" /> but not <paramref name="maxValue" />. However, if
        ///     minValue equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64(long minValue, long maxValue) => _state.NextI64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>
        ///     A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF64() => _state.NextF64();

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>
        ///     A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF32() => _state.NextF32();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NextBytes(Span<byte> buffer) => _state.NextBytes(buffer);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(void* startAddress, nuint byteCount) => _state.NextBytes(startAddress, byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(ref byte startAddress, nuint byteCount) => _state.NextBytes(ref startAddress, byteCount);

        /// <summary>
        ///     Generates a random <see cref="bool" /> value.
        /// </summary>
        /// <returns><see langword="true" /> or <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => _state.NextBool();

        /// <summary>
        ///     Generates a random <see cref="bool" /> value.
        /// </summary>
        /// <param name="trueProbability">
        ///     The probability of returning <see langword="true" />, in the range [0.0, 1.0].
        /// </param>
        /// <returns><see langword="true" /> or <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="trueProbability" /> is outside the range [0.0, 1.0].
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool(double trueProbability) => _state.NextBool(trueProbability);

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Next<T>() where T : unmanaged => _state.Next<State, T>();

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next<T>(ref T destination) where T : unmanaged => _state.Next(ref destination);

        /// <summary>
        ///     Represents a pseudo-random number generator, which is an algorithm that produces
        ///     a sequence of numbers that meet certain statistical requirements for randomness.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct State : IRandomState
        {
            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>An 8-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte Next8() => this.Next<State, byte>();

            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>A 32-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint Next32() => this.Next<State, uint>();

            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>A 64-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong Next64() => this.Next<State, ulong>();

            /// <summary>
            ///     Fills the elements of a specified buffer of bytes with random numbers.
            /// </summary>
            /// <param name="buffer">The buffer to be filled with random numbers.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void NextBytes(Span<byte> buffer) => RandomNumberGenerator.Fill(buffer);

            /// <summary>
            ///     Generates a random <see cref="bool" /> value.
            /// </summary>
            /// <returns><see langword="true" /> or <see langword="false" />.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool NextBool() => BinaryNumberHelpers.IsOddInteger(Next8());
        }
    }
}