using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a thread-safe <see cref="UnsafeXoshiro256PlusPlus" />
    ///     instance that may be used concurrently from any thread.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpecializedCollection(FromType.Standard)]
    public unsafe struct ThreadSafeXoshiro256PlusPlus : IRandom, IEquatable<ThreadSafeXoshiro256PlusPlus>
    {
        /// <summary>
        ///     The underlying generator implementation.
        /// </summary>
        private static ref UnsafeXoshiro256PlusPlus LocalRandom => ref ThreadSafeRandom<UnsafeXoshiro256PlusPlus>.GetLocalRandomRef();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(ThreadSafeXoshiro256PlusPlus other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is ThreadSafeXoshiro256PlusPlus other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "ThreadSafeXoshiro256PlusPlus";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(ThreadSafeXoshiro256PlusPlus left, ThreadSafeXoshiro256PlusPlus right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(ThreadSafeXoshiro256PlusPlus left, ThreadSafeXoshiro256PlusPlus right) => !left.Equals(right);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string GetString(ReadOnlySpan<char> source, int stringLength) => LocalRandom.GetString(source, stringLength);

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
        public readonly string GetHexString(int stringLength, bool lowercase = false) => LocalRandom.GetHexString(stringLength, lowercase);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase. The default is <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void GetHexString(Span<char> destination, bool lowercase = false) => LocalRandom.GetHexString(destination, lowercase);

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="buffer">The buffer to shuffle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Shuffle<T>(Span<T> buffer) => LocalRandom.Shuffle(buffer);

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="source" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination) => LocalRandom.GetItems(source, destination);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T Choose<T>(Span<T> buffer) => ref LocalRandom.Choose(buffer);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T ChooseReadOnly<T>(ReadOnlySpan<T> buffer) => ref LocalRandom.ChooseReadOnly(buffer);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double LerpF64(double maxValue) => LocalRandom.LerpF64(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double LerpF64(double minValue, double maxValue) => LocalRandom.LerpF64(minValue, maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float LerpF32(float maxValue) => LocalRandom.LerpF32(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float LerpF32(float minValue, float maxValue) => LocalRandom.LerpF32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint NextU32() => LocalRandom.NextU32();

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
        public readonly uint NextU32(uint maxValue) => LocalRandom.NextU32(maxValue);

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
        public readonly uint NextU32(uint minValue, uint maxValue) => LocalRandom.NextU32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 64-bit unsigned integer that is greater than or
        ///     equal to 0 and less than <see cref="ulong.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong NextU64() => LocalRandom.NextU64();

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
        public readonly ulong NextU64(ulong maxValue) => LocalRandom.NextU64(maxValue);

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
        public readonly ulong NextU64(ulong minValue, ulong maxValue) => LocalRandom.NextU64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int NextI32() => LocalRandom.NextI32();

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
        public readonly int NextI32(int maxValue) => LocalRandom.NextI32(maxValue);

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
        public readonly int NextI32(int minValue, int maxValue) => LocalRandom.NextI32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>
        ///     A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly long NextI64() => LocalRandom.NextI64();

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
        public readonly long NextI64(long maxValue) => LocalRandom.NextI64(maxValue);

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
        public readonly long NextI64(long minValue, long maxValue) => LocalRandom.NextI64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>
        ///     A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly double NextF64() => LocalRandom.NextF64();

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point
        ///     number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>
        ///     A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float NextF32() => LocalRandom.NextF32();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NextBytes(Span<byte> buffer) => LocalRandom.NextBytes(buffer);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NextBytes(void* startAddress, nuint byteCount) => LocalRandom.NextBytes(startAddress, byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void NextBytes(ref byte startAddress, nuint byteCount) => LocalRandom.NextBytes(ref startAddress, byteCount);

        /// <summary>
        ///     Generates a random <see cref="bool" /> value.
        /// </summary>
        /// <returns><see langword="true" /> or <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool NextBool() => LocalRandom.NextBool();

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
        public readonly bool NextBool(double trueProbability) => LocalRandom.NextBool(trueProbability);

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T Next<T>() where T : unmanaged => LocalRandom.Next<T>();

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Next<T>(ref T destination) where T : unmanaged => LocalRandom.Next(ref destination);
    }
}