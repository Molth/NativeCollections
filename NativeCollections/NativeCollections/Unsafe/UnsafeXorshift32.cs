using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
    ///     that meet certain statistical requirements for randomness.
    /// </summary>
    /// <remarks>https://www.jstatsoft.org/article/view/v008i14</remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community | FromType.C)]
    public unsafe struct UnsafeXorshift32 : IIsCreated, IInitializable, IRandom, IEquatable<UnsafeXorshift32>
    {
        /// <summary>
        ///     Represents the state.
        /// </summary>
        private State _state;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _state.IsCreated;

        /// <summary>
        ///     Initializes a new instance of this class from states.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the deserialized state
        ///     represents an uninitialized (all‑zero) state.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeXorshift32(uint s0) => _state = new State(s0);

        /// <summary>
        ///     Initializes a new instance of this class from bytes.
        /// </summary>
        /// <param name="buffer">The byte span containing the serialized state data.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="buffer" />
        ///     is shorter than the size of the state.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the deserialized state
        ///     represents an uninitialized (all‑zero) state.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeXorshift32(ReadOnlySpan<byte> buffer) => _state = new State(buffer);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeXorshift32 other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeXorshift32 other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeXorshift32";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeXorshift32 left, UnsafeXorshift32 right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeXorshift32 left, UnsafeXorshift32 right) => !left.Equals(right);

        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize() => _state.Initialize();

        /// <summary>
        ///     Creates a string populated with characters chosen at random from <paramref name="source" />.
        /// </summary>
        /// <param name="source">The characters to use to populate the string.</param>
        /// <param name="stringLength">The length of string to return.</param>
        /// <returns>A string populated with items selected at random from <paramref name="source" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="source" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="stringLength" /> is not zero or a positive number.</exception>
        /// <seealso cref="GetItems{T}(ReadOnlySpan{T}, Span{T})" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetString(ReadOnlySpan<char> source, int stringLength) => _state.GetString(source, stringLength);

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
        public string GetHexString(int stringLength, bool lowercase = false) => _state.GetHexString(stringLength, lowercase);

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
        public void GetHexString(Span<char> destination, bool lowercase = false) => _state.GetHexString(destination, lowercase);

        /// <summary>
        ///     Performs an in-place shuffle of a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to shuffle.</param>
        /// <typeparam name="T">The type of buffer.</typeparam>
        /// <remarks>
        ///     This method uses <see cref="NextI32(int, int)" /> to choose values for shuffling.
        ///     This method is an O(n) operation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Shuffle<T>(Span<T> buffer) => _state.Shuffle(buffer);

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
        public void GetItems<T>(ReadOnlySpan<T> source, Span<T> destination) => _state.GetItems(source, destination);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Choose<T>(Span<T> buffer) => ref _state.Choose(buffer);

        /// <summary>
        ///     Chooses the random element in the buffer.
        /// </summary>
        /// <typeparam name="T">The type of elements in the buffer.</typeparam>
        /// <param name="buffer">The buffer of elements.</param>
        /// <returns>Randomly selected element from the buffer.</returns>
        /// <exception cref="ArgumentException"><paramref name="buffer" /> is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref readonly T ChooseReadOnly<T>(ReadOnlySpan<T> buffer) => ref _state.ChooseReadOnly(buffer);

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
        /// <seealso cref="NextF64()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double maxValue) => _state.LerpF64(maxValue);

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
        /// <seealso cref="NextF64()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double LerpF64(double minValue, double maxValue) => _state.LerpF64(minValue, maxValue);

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
        /// <seealso cref="NextF32()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float maxValue) => _state.LerpF32(maxValue);

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
        /// <seealso cref="NextF32()" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpF32(float minValue, float maxValue) => _state.LerpF32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit unsigned integer that is greater than or equal to 0 and less than <see cref="uint.MaxValue" />.</returns>
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
        ///     A 32-bit unsigned integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32(uint maxValue) => _state.NextU32(maxValue);

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
        public uint NextU32(uint minValue, uint maxValue) => _state.NextU32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit unsigned integer that is greater than or equal to 0 and less than <see cref="ulong.MaxValue" />.</returns>
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
        ///     A 64-bit unsigned integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64(ulong maxValue) => _state.NextU64(maxValue);

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
        public ulong NextU64(ulong minValue, ulong maxValue) => _state.NextU64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue" />.</returns>
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
        ///     A 32-bit signed integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextI32(int maxValue) => _state.NextI32(maxValue);

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
        public int NextI32(int minValue, int maxValue) => _state.NextI32(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random integer.
        /// </summary>
        /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue" />.</returns>
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
        ///     A 64-bit signed integer that is greater than or equal to 0,
        ///     and less than <paramref name="maxValue" />; that is,
        ///     the range of return values includes 0 but not <paramref name="maxValue" />.
        ///     However, if <paramref name="maxValue" /> equals 0, 0 is returned.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextI64(long maxValue) => _state.NextI64(maxValue);

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
        public long NextI64(long minValue, long maxValue) => _state.NextI64(minValue, maxValue);

        /// <summary>
        ///     Returns a non-negative random 64-bit double-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 64-bit double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF64() => _state.NextF64();

        /// <summary>
        ///     Returns a non-negative random 32-bit single-precision floating point number
        ///     that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns>A 32-bit single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF32() => _state.NextF32();

        /// <summary>
        ///     Fills the elements of a specified buffer of bytes with random numbers.
        /// </summary>
        /// <param name="buffer">The buffer to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(Span<byte> buffer) => _state.NextBytes(buffer);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(void* startAddress, uint byteCount) => _state.NextBytes(startAddress, byteCount);

        /// <summary>
        ///     Fills a specified memory block with random bytes.
        /// </summary>
        /// <param name="startAddress">A pointer to the memory location where the random bytes will be written.</param>
        /// <param name="byteCount">The number of bytes to fill with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(ref byte startAddress, uint byteCount) => _state.NextBytes(ref startAddress, byteCount);

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <returns>True, or false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => _state.NextBool();

        /// <summary>
        ///     Returns a bool.
        /// </summary>
        /// <param name="trueProbability">A probability of <see langword="true" /> result, should be in the range [0.0, 1.0].</param>
        /// <returns>True, or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="trueProbability" /> value is invalid.</exception>
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
        ///     Creates a new instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeXorshift32 Create()
        {
            Unsafe.SkipInit(out UnsafeXorshift32 random);
            random.Initialize();
            return random;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeXorshift32 Empty => default;

        /// <summary>
        ///     Provides a thread-safe instance that may be used concurrently from any thread.
        /// </summary>
        public static ThreadSafeRandom<UnsafeXorshift32> Shared => new();

        /// <summary>
        ///     Represents a pseudo-random number generator, which is an algorithm that produces a sequence of numbers
        ///     that meet certain statistical requirements for randomness.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct State : IIsCreated, IRandomState
        {
            /// <summary>
            ///     Represents the states.
            /// </summary>
            private uint _s0;

            /// <summary>
            ///     Gets a value that indicates whether this has been allocated or initialized.
            /// </summary>
            public readonly bool IsCreated => !((int)_s0 == 0);

            /// <summary>
            ///     Initializes a new instance of this class from states.
            /// </summary>
            /// <exception cref="ArgumentNullException">
            ///     Thrown when the deserialized state
            ///     represents an uninitialized (all‑zero) state.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public State(uint s0)
            {
                _s0 = s0;
                ThrowHelpers.ThrowIfNotCreated(ref this, ExceptionArgument._dummy);
            }

            /// <summary>
            ///     Initializes a new instance of this class from bytes.
            /// </summary>
            /// <param name="buffer">The byte span containing the serialized state data.</param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///     Thrown when <paramref name="buffer" />
            ///     is shorter than the size of the state.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            ///     Thrown when the deserialized state
            ///     represents an uninitialized (all‑zero) state.
            /// </exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public State(ReadOnlySpan<byte> buffer) => this = RandomHelpers.ReadUnaligned<State>(buffer);

            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>A 32-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private uint Next()
            {
                var x = _s0;
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                _s0 = x;
                return x;
            }

            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>A 32-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint Next32()
            {
                var state = this;
                var result = state.Next();
                this = state;
                return result;
            }

            /// <summary>
            ///     Returns a non-negative random integer.
            /// </summary>
            /// <returns>A 64-bit unsigned integer.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong Next64() => ((ulong)Next32() << 32) | Next32();

            /// <summary>
            ///     Fills the elements of a specified buffer of bytes with random numbers.
            /// </summary>
            /// <param name="buffer">The buffer to be filled with random numbers.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void NextBytes(Span<byte> buffer)
            {
                var state = this;
                for (; buffer.Length >= 4; buffer = buffer.Slice(4))
                {
                    var num1 = state.Next();
                    Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), num1);
                }

                if (!buffer.IsEmpty)
                {
                    var num2 = state.Next();
                    SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.As<uint, byte>(ref num2), (uint)buffer.Length);
                }

                this = state;
            }

            /// <summary>
            ///     Returns a bool.
            /// </summary>
            /// <returns>True, or false.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool NextBool() => BinaryNumberHelpers.IsOddInteger(Next32());
        }
    }
}