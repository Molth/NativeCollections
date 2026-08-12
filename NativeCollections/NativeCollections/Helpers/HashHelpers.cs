using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides helper methods for prime number calculations and hash table capacity management.
    /// </summary>
    internal static class HashHelpers
    {
        /// <summary>
        ///     This is the maximum prime smaller than Array.MaxLength.
        /// </summary>
        private const int MAX_PRIME_ARRAY_LENGTH = 2147483587;

        /// <summary>
        ///     A prime constant used to ensure hash table sizes are prime and relatively prime to the hash function.
        /// </summary>
        private const int HASH_PRIME = 101;

        /// <summary>
        ///     A lookup table containing 24 index offsets followed by 72 prime numbers.
        /// </summary>
        /// <remarks>
        ///     The first 24 values are used to map small capacities to a starting index in the prime list.
        ///     The remaining 72 values are precomputed primes for hash table sizing.
        /// </remarks>
        private static ReadOnlySpan<int> LookupTable => new int[96]
        {
            0, 0, 1, 2, 3, 6, 9, 12, 16, 19, 23, 27, 31, 34, 38, 42, 46, 50, 53, 57, 61, 65, 69, 0,
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        };

        // Table of prime numbers to use as hash table sizes.
        // A typical resize algorithm would pick the smallest prime number in this array
        // that is larger than twice the previous capacity.
        // Suppose our Hashtable currently has capacity x and enough elements are added
        // such that a resize needs to occur. Resizing first computes 2x then finds the
        // first prime in the table greater than 2x, i.e. if primes are ordered
        // p_1, p_2, ..., p_i, ..., it finds p_n such that p_n-1 < 2x < p_n.
        // Doubling is important for preserving the asymptotic complexity of the
        // hashtable operations such as add.  Having a prime guarantees that double
        // hashing does not lead to infinite loops.  IE, your hash function will be
        // h1(key) + i*h2(key), 0 <= i < size.  h2 and the size must be relatively prime.
        // We prefer the low computation costs of higher prime numbers over the increased
        // memory allocation of a fixed prime number i.e. when right sizing a HashSet.
        public static ReadOnlySpan<int> Primes => LookupTable.Slice(24);

        /// <summary>
        ///     Determines whether the specified integer is a prime number.
        /// </summary>
        /// <param name="candidate">The integer to test for primality.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="candidate" /> is prime;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                var limit = (int)Math.Sqrt(candidate);
                for (var divisor = 3; divisor <= limit; divisor += 2)
                {
                    if (candidate % divisor == 0)
                        return false;
                }

                return true;
            }

            return candidate == 2;
        }

        /// <summary>
        ///     Returns the smallest prime number that is greater than or equal to the specified minimum value.
        /// </summary>
        /// <param name="min">The minimum value that the returned prime must be at least.</param>
        /// <returns>A prime number that is greater than or equal to <paramref name="min" />.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="min" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPrime(int min)
        {
            ThrowHelpers.ThrowIfHashtableCapacityOverflow(min);
            if (min <= 7199369)
            {
                ref var local1 = ref MemoryMarshal.GetReference(LookupTable);
                var num = Unsafe.Add(ref local1, (nint)BitOperationsHelpers.Log2((uint)min));
                ref var local2 = ref Unsafe.Add(ref local1, (nint)24);
                for (var elementOffset = num; elementOffset < 72; ++elementOffset)
                {
                    var prime = Unsafe.Add(ref local2, (nint)elementOffset);
                    if (prime >= min)
                        return prime;
                }
            }

            for (var i = min | 1; i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && (i - 1) % HASH_PRIME != 0)
                    return i;
            }

            return min;
        }

        /// <summary>
        ///     Returns size of hashtable to grow to.
        /// </summary>
        /// <remarks>
        ///     Allow the hashtables to grow to maximum possible size (~2G elements) before encountering capacity overflow.
        ///     Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExpandPrime(int oldSize)
        {
            var newSize = 2 * oldSize;
            return (uint)newSize > MAX_PRIME_ARRAY_LENGTH && MAX_PRIME_ARRAY_LENGTH > oldSize ? MAX_PRIME_ARRAY_LENGTH : GetPrime(newSize);
        }

        /// <summary>
        ///     Returns approximate reciprocal of the divisor: ceil(2**64 / divisor).
        /// </summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;

        /// <summary>
        ///     Performs a mod operation using the multiplier pre-computed with <see cref="GetFastModMultiplier" />.
        /// </summary>
        /// <remarks>This should only be used on 64-bit.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastMod(uint value, uint divisor, ulong multiplier) => (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);

        /// <summary>
        ///     Creates a new <see cref="FastModImpl" /> instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastModImpl GetFastModImpl(uint divisor) => new(divisor);

        /// <summary>
        ///     Provides a mod operation.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FastModImpl
        {
            /// <summary>
            ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
            /// </summary>
            private readonly ulong _fastModMultiplier;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public FastModImpl(uint divisor) => _fastModMultiplier = Environment.Is64BitProcess ? GetFastModMultiplier(divisor) : 0;

            /// <summary>
            ///     Performs a mod operation.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetRemainder(uint value, uint divisor) => Environment.Is64BitProcess ? FastMod(value, divisor, _fastModMultiplier) : value % divisor;
        }
    }
}