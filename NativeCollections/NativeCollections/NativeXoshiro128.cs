using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

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
    public unsafe struct NativeXoshiro128 : IEquatable<NativeXoshiro128>
    {
        /// <summary>
        ///     State0
        /// </summary>
        private uint _s0;

        /// <summary>
        ///     State1
        /// </summary>
        private uint _s1;

        /// <summary>
        ///     State2
        /// </summary>
        private uint _s2;

        /// <summary>
        ///     State3
        /// </summary>
        private uint _s3;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !(((int)_s0 | (int)_s1 | (int)_s2 | (int)_s3) == 0);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public NativeXoshiro128(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < sizeof(NativeXoshiro128))
                throw new ArgumentOutOfRangeException(nameof(buffer), $"Requires size is {sizeof(NativeXoshiro128)}, but buffer length is {buffer.Length}.");
            var random = Unsafe.ReadUnaligned<NativeXoshiro128>(ref MemoryMarshal.GetReference(buffer));
            if (!random.IsCreated)
                throw new InvalidDataException("Cannot be entirely zero.");
            this = random;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeXoshiro128 other)
        {
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<NativeXoshiro128, byte>(ref this)) == Vector128.LoadUnsafe(ref Unsafe.As<NativeXoshiro128, byte>(ref other));
#endif
            ref var left = ref Unsafe.As<NativeXoshiro128, int>(ref this);
            ref var right = ref Unsafe.As<NativeXoshiro128, int>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2) && Unsafe.Add(ref left, 3) == Unsafe.Add(ref right, 3);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeXoshiro128 nativeXoshiro128 && nativeXoshiro128 == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode()
        {
            ref var local = ref Unsafe.As<NativeXoshiro128, int>(ref this);
            return local ^ Unsafe.Add(ref local, 1) ^ Unsafe.Add(ref local, 2) ^ Unsafe.Add(ref local, 3);
        }

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeXoshiro128";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeXoshiro128 left, NativeXoshiro128 right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeXoshiro128 left, NativeXoshiro128 right) => !left.Equals(right);

        /// <summary>
        ///     Initialize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Initialize()
        {
            var data = MemoryMarshal.CreateSpan(ref Unsafe.As<uint, byte>(ref _s0), 16);
            do
            {
                RandomNumberGenerator.Fill(data);
            } while (((int)_s0 | (int)_s1 | (int)_s2 | (int)_s3) == 0);
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 32-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Next32()
        {
            var s0 = _s0;
            var s1 = _s1;
            var s2 = _s2;
            var s3 = _s3;
            var num1 = s1 * 5U;
            var num2 = (((int)num1 << 7) | (int)(num1 >> 25)) * 9;
            var num3 = s1 << 9;
            var num4 = s2 ^ s0;
            var num5 = s3 ^ s1;
            var num6 = s1 ^ num4;
            var num7 = s0 ^ num5;
            var num8 = num4 ^ num3;
            var num9 = (num5 << 11) | (num5 >> 21);
            _s0 = num7;
            _s1 = num6;
            _s2 = num8;
            _s3 = num9;
            return (uint)num2;
        }

        /// <summary>Returns a non-negative random integer.</summary>
        /// <returns>A 64-bit unsigned integer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong Next64() => ((ulong)Next32() << 32) | Next32();

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
        public void Shuffle<T>(Span<T> buffer)
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
        public uint NextUInt32()
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
        public uint NextUInt32(uint maxValue)
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
        public uint NextUInt32(uint minValue, uint maxValue)
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
        public ulong NextUInt64()
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
        public ulong NextUInt64(ulong maxValue)
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
        public ulong NextUInt64(ulong minValue, ulong maxValue)
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
        public int NextInt32()
        {
            uint num;
            do
            {
                num = Next32() >> 1;
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
                num2 = Next64() >> (64 - num1);
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
                num2 = Next64() >> (64 - num1);
            } while (num2 >= maxValue1);

            return (long)num2 + minValue;
        }

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => (Next64() >> 11) * 1.1102230246251565E-16;

        /// <summary>Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.</summary>
        /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextSingle() => (Next32() >> 8) * 5.9604645E-08f;

        /// <summary>Fills the elements of a specified span of bytes with random numbers.</summary>
        /// <param name="buffer">The array to be filled with random numbers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(Span<byte> buffer)
        {
            var s0 = _s0;
            var s1 = _s1;
            var num1 = _s2;
            var num2 = _s3;
            for (; buffer.Length >= 4; buffer = buffer.Slice(4))
            {
                var num3 = s1 * 5U;
                Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), (uint)((((int)num3 << 7) | (int)(num3 >> 25)) * 9));
                var num4 = s1 << 9;
                var num5 = num1 ^ s0;
                var num6 = num2 ^ s1;
                s1 ^= num5;
                s0 ^= num6;
                num1 = num5 ^ num4;
                num2 = (num6 << 11) | (num6 >> 21);
            }

            if (!buffer.IsEmpty)
            {
                var num7 = s1 * 5U;
                var num8 = (uint)((((int)num7 << 7) | (int)(num7 >> 25)) * 9);
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref *(byte*)&num8, (uint)buffer.Length);
                var num9 = s1 << 9;
                var num10 = num1 ^ s0;
                var num11 = num2 ^ s1;
                s1 ^= num10;
                s0 ^= num11;
                num1 = num10 ^ num9;
                num2 = (num11 << 11) | (num11 >> 21);
            }

            _s0 = s0;
            _s1 = s1;
            _s2 = num1;
            _s3 = num2;
        }

        /// <summary>Returns a boolean.</summary>
        /// <returns>True, or false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBoolean() => (Next32() & 1) == 0;

        /// <summary>
        ///     Create
        /// </summary>
        /// <returns>NativeXoshiro128</returns>
        public static NativeXoshiro128 Create()
        {
            var random = new NativeXoshiro128();
            random.Initialize();
            return random;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeXoshiro128 Empty => new();
    }
}