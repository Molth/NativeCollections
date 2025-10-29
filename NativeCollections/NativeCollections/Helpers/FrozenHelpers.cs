using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.ArchitectureHelpers;
#if NET5_0_OR_GREATER
using System.Text;
#else
using System.Collections.Generic;
#endif

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Frozen helpers
    /// </summary>
    internal static unsafe class FrozenHelpers
    {
        /// <summary>
        ///     Is known comparable
        /// </summary>
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
        ///     Keys are hash codes
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool KeysAreHashCodes<T>() where T : unmanaged, IEquatable<T>
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                return true;
            return (typeof(T) == typeof(nint) || typeof(T) == typeof(nuint)) && !Environment.Is64BitProcess;
        }

        /// <summary>
        ///     Frozen hash table
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FrozenHashTable : IDisposable
        {
            /// <summary>
            ///     Hash codes
            /// </summary>
            private readonly NativeArray<int> _hashCodes;

            /// <summary>
            ///     Buckets
            /// </summary>
            private readonly NativeArray<Bucket> _buckets;

            /// <summary>
            ///     Fast mod multiplier
            /// </summary>
            private readonly ulong _fastModMultiplier;

            /// <summary>
            ///     Frozen hash table
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private FrozenHashTable(NativeArray<int> hashCodes, NativeArray<Bucket> buckets, ulong fastModMultiplier)
            {
                _hashCodes = hashCodes;
                _buckets = buckets;
                _fastModMultiplier = fastModMultiplier;
            }

            /// <summary>
            ///     Create frozen hash table
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FrozenHashTable Create(Span<int> hashCodes, bool hashCodesAreUnique)
            {
                var num1 = CalculateNumBuckets(hashCodes, hashCodesAreUnique);
                var fastModMultiplier = Environment.Is64BitProcess ? HashHelpers.GetFastModMultiplier((uint)num1) : 0;
                var array = ArrayPool<int>.Shared.Rent(num1 + hashCodes.Length);
                var span1 = array.AsSpan(0, num1);
                var span2 = array.AsSpan(num1, hashCodes.Length);
                span1.Fill(-1);
                for (var index1 = 0; index1 < hashCodes.Length; ++index1)
                {
                    var index2 = Environment.Is64BitProcess ? (int)HashHelpers.FastMod((uint)hashCodes[index1], (uint)span1.Length, fastModMultiplier) : (int)((uint)hashCodes[index1] % (uint)span1.Length);
                    ref var local = ref span1[index2];
                    span2[index1] = local;
                    local = index1;
                }

                var hashCodesByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(hashCodes.Length * sizeof(int)), CACHE_LINE_SIZE);
                var hashCodes1Ptr = (int*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(hashCodesByteCount + span1.Length * sizeof(Bucket)), CACHE_LINE_SIZE);
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
                return new FrozenHashTable(hashCodes1, buckets, fastModMultiplier);
            }

            /// <summary>
            ///     Find matching entries
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FindMatchingEntries(int hashCode, out int startIndex, out int endIndex)
            {
                var buckets = _buckets.AsReadOnlySpan();
                ref readonly var local = ref Environment.Is64BitProcess ? ref buckets[(int)HashHelpers.FastMod((uint)hashCode, (uint)buckets.Length, _fastModMultiplier)] : ref buckets[(int)((uint)hashCode % (uint)buckets.Length)];
                startIndex = local.StartIndex;
                endIndex = local.EndIndex;
            }

            /// <summary>
            ///     Count
            /// </summary>
            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes.Length;
            }

            /// <summary>
            ///     Hash codes
            /// </summary>
            public ReadOnlySpan<int> HashCodes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes;
            }

            /// <summary>
            ///     Calculate num buckets
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int CalculateNumBuckets(ReadOnlySpan<int> hashCodes, bool hashCodesAreUnique)
            {
                var intSet = NativeHashSet<int>.Empty;
                using var autoDisposable = new NativeDisposable<NativeHashSet<int>>(&intSet);
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
                //     Is bucket first visit
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
            ///     Bucket
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            private readonly struct Bucket
            {
                /// <summary>
                ///     Start index
                /// </summary>
                public readonly int StartIndex;

                /// <summary>
                ///     End index
                /// </summary>
                public readonly int EndIndex;

                /// <summary>
                ///     Bucket
                /// </summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Bucket(int startIndex, int count)
                {
                    StartIndex = startIndex;
                    EndIndex = startIndex + count - 1;
                }
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => _hashCodes.Dispose();
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        ///     Key value pair comparer
        /// </summary>
        public sealed class KeyValuePairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            /// <summary>
            ///     Default
            /// </summary>
            public static KeyValuePairComparer<TKey, TValue> Default { get; } = new();

            /// <summary>
            ///     Compare
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => Comparer<TKey>.Default.Compare(x.Key, y.Key);
        }
#endif
    }
}