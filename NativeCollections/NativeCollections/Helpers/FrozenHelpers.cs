using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;
#if NET5_0_OR_GREATER
using System.Text;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides internal structures.
    /// </summary>
    internal static unsafe class FrozenHelpers
    {
        /// <summary>
        ///     Determines whether the specified type <typeparamref name="T" /> is a primitive or well-known comparable type.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to check.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if <typeparamref name="T" /> is a primitive type (including enums)
        ///     or a known framework type that implements <see cref="IEquatable{T}" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKnownComparable<T>() where T : unmanaged, IEquatable<T>
        {
            return typeof(T) == typeof(bool) || typeof(T) == typeof(sbyte) || typeof(T) == typeof(byte) || typeof(T) == typeof(char) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(long) || typeof(T) == typeof(ulong) || typeof(T) == typeof(decimal) || typeof(T) == typeof(float) || typeof(T) == typeof(double) || typeof(T) == typeof(decimal) || typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(Guid) ||
#if NET5_0_OR_GREATER
                   typeof(T) == typeof(Rune) ||
                   typeof(T) == typeof(Half) ||
#if NET6_0_OR_GREATER
                   typeof(T) == typeof(DateOnly) ||
                   typeof(T) == typeof(TimeOnly) ||

#if NET7_0_OR_GREATER
                   typeof(T) == typeof(Int128) ||
                   typeof(T) == typeof(UInt128) ||
#endif
#endif
#endif
                   typeof(T) == typeof(nint) || typeof(T) == typeof(nuint) || typeof(T).IsEnum;
        }

        /// <summary>
        ///     Determines whether the specified type <typeparamref name="T" /> can be used
        ///     directly as a hash code value without requiring additional hashing.
        /// </summary>
        /// <typeparam name="T">The unmanaged type to check.</typeparam>
        /// <returns>
        ///     <see langword="true" /> if <typeparamref name="T" /> is a
        ///     small integer type (byte, sbyte, short, ushort, int, uint)
        ///     or a native-sized integer type (nint, nuint) on a 32‑bit process;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool KeysAreHashCodes<T>() where T : unmanaged, IEquatable<T>
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                return true;
            return (typeof(T) == typeof(nint) || typeof(T) == typeof(nuint)) && !Environment.Is64BitProcess;
        }

        /// <summary>
        ///     Represents a frozen hash table that stores hash codes and provides fast lookup for matching entries.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FrozenHashTable : IDisposable
        {
            /// <summary>
            ///     A array containing the reordered hash codes.
            /// </summary>
            private readonly NativeArray<int> _hashCodes;

            /// <summary>
            ///     A array of bucket descriptors.
            /// </summary>
            private readonly NativeArray<Bucket> _buckets;

            /// <summary>
            ///     Pre-computed multiplier for use on 64-bit performing faster modulo operations.
            /// </summary>
            private readonly HashHelpers.FastModImpl _fastModImpl;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            /// <param name="hashCodes">A array containing the reordered hash codes.</param>
            /// <param name="buckets">A array of bucket descriptors.</param>
            /// <param name="fastModImpl">A fast modulo implementation used for bucket indexing.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private FrozenHashTable(NativeArray<int> hashCodes, NativeArray<Bucket> buckets, HashHelpers.FastModImpl fastModImpl)
            {
                _hashCodes = hashCodes;
                _buckets = buckets;
                _fastModImpl = fastModImpl;
            }

            /// <summary>
            ///     Builds a frozen hash table from the provided span of hash codes.
            /// </summary>
            /// <param name="hashCodes">The input hash codes. This span will be modified to store order information.</param>
            /// <param name="hashCodesAreUnique">
            ///     <see langword="true" /> if all hash codes are known to be distinct;
            ///     otherwise, <see langword="false" />.
            ///     If <see langword="false" />, duplicate handling will be performed.
            /// </param>
            /// <returns>A new <see cref="FrozenHashTable" /> instance.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FrozenHashTable Create(Span<int> hashCodes, bool hashCodesAreUnique)
            {
                var num1 = CalculateNumBuckets(hashCodes, hashCodesAreUnique);
                var fastModImpl = HashHelpers.GetFastModImpl((uint)num1);
                var array = ArrayPool<int>.Shared.Rent(num1 + hashCodes.Length);
                var span1 = array.AsSpan(0, num1);
                var span2 = array.AsSpan(num1, hashCodes.Length);
                span1.Fill(-1);
                for (var index1 = 0; index1 < hashCodes.Length; ++index1)
                {
                    var index2 = (int)fastModImpl.GetRemainder((uint)hashCodes[index1], (uint)span1.Length);
                    ref var local = ref span1[index2];
                    span2[index1] = local;
                    local = index1;
                }

                var hashCodesByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(hashCodes.Length * Unsafe.SizeOf<int>()), CACHE_LINE_SIZE);
                var hashCodes1Ptr = (int*)NativeMemoryAllocator.AlignedAllocZeroed(hashCodesByteCount + (uint)span1.Length * (uint)Unsafe.SizeOf<Bucket>(), CACHE_LINE_SIZE);
                var bucketsPtr = UnsafeHelpers.AddByteOffset<Bucket>(hashCodes1Ptr, (nint)hashCodesByteCount);
                var hashCodes1 = new NativeArray<int>(hashCodes1Ptr, hashCodes.Length);
                var buckets = new NativeArray<Bucket>(bucketsPtr, span1.Length);
                var index3 = 0;
                for (var index4 = 0; index4 < buckets.Length; ++index4)
                {
                    var num2 = span1[index4];
                    if (num2 >= 0)
                    {
                        var count = 0;
                        var index5 = num2;
                        var startIndex = index3;
                        for (; index5 >= 0; index5 = span2[index5])
                        {
                            ref var local = ref hashCodes[index5];
                            hashCodes1[index3] = local;
                            local = index3;
                            ++index3;
                            ++count;
                        }

                        buckets[index4] = new Bucket(startIndex, count);
                    }
                }

                ArrayPool<int>.Shared.Return(array);
                return new FrozenHashTable(hashCodes1, buckets, fastModImpl);
            }

            /// <summary>
            ///     Locates the range of entries in the hash table that match the given hash code.
            /// </summary>
            /// <param name="hashCode">The hash code to look up.</param>
            /// <param name="startIndex">When this method returns, contains the starting index of matching entries.</param>
            /// <param name="endIndex">When this method returns, contains the ending index of matching entries.</param>
            /// <remarks>
            ///     The matching entries are stored contiguously in the <see cref="HashCodes" /> array between
            ///     <paramref name="startIndex" /> and <paramref name="endIndex" /> (inclusive).
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FindMatchingEntries(int hashCode, out int startIndex, out int endIndex)
            {
                var buckets = _buckets.AsReadOnlySpan();
                ref readonly var local = ref buckets[(int)_fastModImpl.GetRemainder((uint)hashCode, (uint)buckets.Length)];
                startIndex = local.StartIndex;
                endIndex = local.EndIndex;
            }

            /// <summary>
            ///     Gets the number of elements contained in this.
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes.Length;
            }

            /// <summary>
            ///     Gets a read-only span of the reordered hash codes stored in the table.
            /// </summary>
            public ReadOnlySpan<int> HashCodes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes;
            }

            /// <summary>
            ///     Calculates the optimal number of buckets to use for the frozen hash table based on the input hash codes.
            /// </summary>
            /// <param name="hashCodes">The hash codes to be stored.</param>
            /// <param name="hashCodesAreUnique">Whether the hash codes are already unique.</param>
            /// <returns>The recommended bucket count.</returns>
            /// <remarks>
            ///     This method attempts to balance the number of buckets against collision probability
            ///     to achieve good performance for lookup operations.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int CalculateNumBuckets(ReadOnlySpan<int> hashCodes, bool hashCodesAreUnique)
            {
                var intSet = NativeHashSet<int>.Empty;
                using var autoDisposable = new UnsafeDisposable<NativeHashSet<int>>(&intSet);
                var min = hashCodes.Length;
                ReadOnlySpan<int> readOnlySpan;
                if (!hashCodesAreUnique)
                {
                    intSet = new NativeHashSet<int>(hashCodes.Length);
                    readOnlySpan = hashCodes;
                    for (var index = 0; index < readOnlySpan.Length; ++index)
                    {
                        var num = readOnlySpan[index];
                        intSet.Add(num);
                    }

                    min = intSet.Count;
                }

                var num1 = min * 2;
                var primes = HashHelpers.Primes;
                var index1 = 0;
                while ((uint)index1 < (uint)primes.Length && num1 > primes[index1])
                    ++index1;
                if (index1 >= primes.Length)
                    return HashHelpers.GetPrime(min);
                var num2 = min * (min >= 1000 ? 3 : 16);
                var index2 = index1;
                while ((uint)index2 < (uint)primes.Length && num2 > primes[index2])
                    ++index2;
                if (index2 < primes.Length)
                    num2 = primes[index2 - 1];
                var seenBuckets = ArrayPool<int>.Shared.Rent(num2 / 32 + 1);
                var num3 = num2;
                var bestNumCollisions = min;
                int numBuckets;
                int numCollisions;
                for (var index3 = index1; index3 < index2; ++index3)
                {
                    numBuckets = primes[index3];
                    Array.Clear(seenBuckets, 0, Math.Min(numBuckets, seenBuckets.Length));
                    numCollisions = 0;
                    if (intSet.IsCreated && min != hashCodes.Length)
                    {
                        foreach (var code in intSet)
                        {
                            if (!IsBucketFirstVisit(code))
                                break;
                        }
                    }
                    else
                    {
                        readOnlySpan = hashCodes;
                        var index4 = 0;
                        while (index4 < readOnlySpan.Length && IsBucketFirstVisit(readOnlySpan[index4]))
                            ++index4;
                    }

                    if (numCollisions < bestNumCollisions)
                    {
                        num3 = numBuckets;
                        if (numCollisions / (double)min > 0.05)
                            bestNumCollisions = numCollisions;
                        else
                            break;
                    }
                }

                ArrayPool<int>.Shared.Return(seenBuckets);
                return num3;

                // <summary>
                //     Checks if the hash code's bucket has been visited before,
                //     updates collision count accordingly.
                // </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool IsBucketFirstVisit(int code)
                {
                    var num = (uint)code % (uint)numBuckets;
                    if ((seenBuckets[(int)(num / 32U)] & (1 << (int)num)) != 0)
                    {
                        numCollisions++;
                        if (numCollisions >= bestNumCollisions)
                            return false;
                    }
                    else
                    {
                        seenBuckets[(int)(num / 32U)] |= 1 << (int)num;
                    }

                    return true;
                }
            }

            /// <summary>
            ///     Represents a bucket entry in the frozen hash table, storing the range of hash codes that map to the same bucket.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private readonly struct Bucket
            {
                /// <summary>
                ///     The starting index of the bucket's range within the hash codes array.
                /// </summary>
                public readonly int StartIndex;

                /// <summary>
                ///     The ending index of the bucket's range (inclusive).
                /// </summary>
                public readonly int EndIndex;

                /// <summary>
                ///     Initializes a new instance of this class struct.
                /// </summary>
                /// <param name="startIndex">The starting index of the range.</param>
                /// <param name="count">The number of entries in the bucket.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Bucket(int startIndex, int count)
                {
                    StartIndex = startIndex;
                    EndIndex = startIndex + count - 1;
                }
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _hashCodes.Dispose();
        }
    }
}