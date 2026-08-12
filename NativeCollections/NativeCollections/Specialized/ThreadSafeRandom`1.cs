using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a thread-safe instance that may be used concurrently from any thread.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SpecializedCollection(FromType.Standard)]
    public unsafe struct ThreadSafeRandom<TRandom> : IRandom, IEquatable<ThreadSafeRandom<TRandom>> where TRandom : struct, IIsCreated, IInitializable, IRandom
    {
        /// <summary>
        ///     The underlying generator implementation.
        /// </summary>
        [ThreadStatic] private static TRandom _impl;

        /// <summary>
        ///     The underlying generator implementation.
        /// </summary>
        private static ref TRandom LocalRandom => ref EnsureInitialized(ref _impl);

        /// <summary>
        ///     Ensures the random is initialized, and returns a reference to it.
        /// </summary>
        /// <param name="random">The pseudo-random number generator.</param>
        /// <returns>The pseudo-random number generator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref TRandom EnsureInitialized(ref TRandom random)
        {
            if (!random.IsCreated)
                random.Initialize();
            return ref random;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(ThreadSafeRandom<TRandom> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is ThreadSafeRandom<TRandom> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("ThreadSafeRandom<{0}>", SR.GetTypeName(typeof(TRandom)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ThreadSafeRandom<TRandom> left, ThreadSafeRandom<TRandom> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ThreadSafeRandom<TRandom> left, ThreadSafeRandom<TRandom> right) => !left.Equals(right);

        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stringLength" /> is not zero or a positive number.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(ReadOnlySpan<char> source, int stringLength) => LocalRandom.GetString(source, stringLength);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetHexString(int stringLength, bool lowercase = false) => LocalRandom.GetHexString(stringLength, lowercase);

        /// <summary>
        ///     Fills a buffer with random hexadecimal characters.
        /// </summary>
        /// <param name="destination">The buffer to receive the characters.</param>
        /// <param name="lowercase">
        ///     <see langword="true" /> if the hexadecimal characters should be lowercase;
        ///     <see langword="false" /> if they should be uppercase.
        ///     The default is <see langword="false" />.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetHexString(Span<char> destination, bool lowercase = false) => LocalRandom.GetHexString(destination, lowercase);

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shuffle<T>(Span<T> buffer) => LocalRandom.Shuffle(buffer);

        /// <summary>
        ///     Fills the elements of a specified buffer with items chosen at random from the provided set of choices.
        /// </summary>
        /// <param name="source">The items to use to populate the buffer.</param>
        /// <param name="destination">The buffer to be filled with items.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="source" /> is empty.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination) => LocalRandom.GetItems(source, destination);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Choose<T>(Span<T> buffer) => ref LocalRandom.Choose(buffer);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T ChooseReadOnly<T>(ReadOnlySpan<T> buffer) => ref LocalRandom.ChooseReadOnly(buffer);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double maxValue) => LocalRandom.LerpF64(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double minValue, double maxValue) => LocalRandom.LerpF64(minValue, maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float maxValue) => LocalRandom.LerpF32(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float minValue, float maxValue) => LocalRandom.LerpF32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32() => LocalRandom.NextU32();

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32(uint maxValue) => LocalRandom.NextU32(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32(uint minValue, uint maxValue) => LocalRandom.NextU32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64() => LocalRandom.NextU64();

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64(ulong maxValue) => LocalRandom.NextU64(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64(ulong minValue, ulong maxValue) => LocalRandom.NextU64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32() => LocalRandom.NextI32();

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32(int maxValue) => LocalRandom.NextI32(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32(int minValue, int maxValue) => LocalRandom.NextI32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64() => LocalRandom.NextI64();

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64(long maxValue) => LocalRandom.NextI64(maxValue);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64(long minValue, long maxValue) => LocalRandom.NextI64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF64() => LocalRandom.NextF64();

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF32() => LocalRandom.NextF32();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(Span<byte> buffer) => LocalRandom.NextBytes(buffer);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(void* startAddress, uint byteCount) => LocalRandom.NextBytes(startAddress, byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(ref byte startAddress, uint byteCount) => LocalRandom.NextBytes(ref startAddress, byteCount);

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <returns>True, or false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => LocalRandom.NextBool();

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <param name="trueProbability">A probability of <see langword="true" /> result, should be in the range [0.0, 1.0].</param>
        /// <returns>True, or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trueProbability" /> value is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool(double trueProbability) => LocalRandom.NextBool(trueProbability);

        /// <summary>
        ///     Generates a random value of blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <returns>The randomly generated value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Next<T>() where T : unmanaged => LocalRandom.Next<T>();

        /// <summary>
        ///     Fills the specified reference with a random value of the specified blittable type.
        /// </summary>
        /// <typeparam name="T">The blittable type.</typeparam>
        /// <param name="destination">The reference to the memory location to fill with random data.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Next<T>(ref T destination) where T : unmanaged => LocalRandom.Next(ref destination);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static ThreadSafeRandom<TRandom> Empty => default;
    }
}