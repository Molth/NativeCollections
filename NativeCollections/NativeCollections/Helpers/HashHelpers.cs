using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Hash helpers
    /// </summary>
    internal static class HashHelpers
    {
        /// <summary>
        ///     Primes
        /// </summary>
        public static ReadOnlySpan<int> Primes => new int[96]
        {
            0, 0, 1, 2, 3, 6, 9, 12, 16, 19, 23, 27, 31, 34, 38, 42, 46, 50, 53, 57, 61, 65, 69, 0,
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        };

        /// <summary>
        ///     Is prime
        /// </summary>
        /// <param name="candidate">Candidate</param>
        /// <returns>Is prime</returns>
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
        ///     Get prime
        /// </summary>
        /// <param name="min">Min</param>
        /// <returns>Prime</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPrime(int min)
        {
            ThrowHelpers.ThrowIfHashtableCapacityOverflow(min);
            if (min <= 7199369)
            {
                ref var local1 = ref MemoryMarshal.GetReference(Primes);
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
                if (IsPrime(i) && (i - 1) % 101 != 0)
                    return i;
            }

            return min;
        }

        /// <summary>
        ///     Expand prime
        /// </summary>
        /// <param name="oldSize">Old size</param>
        /// <returns>Prime</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExpandPrime(int oldSize)
        {
            var newSize = 2 * oldSize;
            return (uint)newSize > 2147483587 && 2147483587 > oldSize ? 2147483587 : GetPrime(newSize);
        }

        /// <summary>
        ///     Get fast mod multiplier
        /// </summary>
        /// <param name="divisor">Divisor</param>
        /// <returns>Fast mod multiplier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetFastModMultiplier(uint divisor) => ulong.MaxValue / divisor + 1;

        /// <summary>
        ///     Fast mod
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="divisor">Divisor</param>
        /// <param name="multiplier">Multiplier</param>
        /// <returns>Mod</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FastMod(uint value, uint divisor, ulong multiplier) => (uint)(((((multiplier * value) >> 32) + 1) * divisor) >> 32);
    }
}