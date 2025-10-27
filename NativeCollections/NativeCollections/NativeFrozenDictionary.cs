using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Text;
#endif

#pragma warning disable CS1591

namespace NativeCollections
{
    internal static unsafe class NativeFrozenDictionary
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly TValue GetValueRefOrNullRef<T, TKey, TValue>(void* ptr, in TKey key) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => ref Unsafe.AsRef<T>(ptr).GetValueRefOrNullRef(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TKey> Keys<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Keys();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeArray<TValue> Values<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Values();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Count<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Dispose<T, TKey, TValue>(void* ptr) where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged => Unsafe.AsRef<T>(ptr).Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFrozenDictionaryHandle<TKey, TValue> GetHandle<T, TKey, TValue>() where T : unmanaged, IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            NativeFrozenDictionaryHandle<TKey, TValue> handle;
            handle.GetValueRefOrNullRef = &GetValueRefOrNullRef<T, TKey, TValue>;
            handle.Keys = &Keys<T, TKey, TValue>;
            handle.Values = &Values<T, TKey, TValue>;
            handle.Count = &Count<T, TKey, TValue>;
            handle.GetEnumerator = &GetEnumerator<T, TKey, TValue>;
            handle.Dispose = &Dispose<T, TKey, TValue>;
            return handle;
        }

        public interface IFrozenDictionary<TKey, TValue> : IDisposable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            ref readonly TValue GetValueRefOrNullRef(in TKey key);
            NativeArray<TKey> Keys();
            NativeArray<TValue> Values();
            NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator();
            int Count();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeFrozenDictionaryHandle<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            public delegate* managed<void*, in TKey, ref readonly TValue> GetValueRefOrNullRef;
            public delegate* managed<void*, NativeArray<TKey>> Keys;
            public delegate* managed<void*, NativeArray<TValue>> Values;
            public delegate* managed<void*, int> Count;
            public delegate* managed<void*, NativeFrozenDictionary<TKey, TValue>.Enumerator> GetEnumerator;
            public delegate* managed<void*, void> Dispose;
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct EmptyFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key) => ref Unsafe.NullRef<TValue>();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => NativeArray<TKey>.Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => NativeArray<TValue>.Empty;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(NativeArray<TKey>.Empty, NativeArray<TValue>.Empty);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            private readonly NativeArray<TKey> _keys;
            private readonly NativeArray<TValue> _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallFrozenDictionary(NativeArray<TKey> keys, NativeArray<TValue> values)
            {
                _keys = keys;
                _values = values;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                var index = _keys.AsSpan().IndexOf(key);
                return ref index >= 0 ? ref _values[index] : ref Unsafe.NullRef<TValue>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _keys.Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _keys.Dispose();
                _values.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct SmallComparableFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            private readonly NativeArray<TKey> _keys;
            private readonly NativeArray<TValue> _values;
            private readonly TKey _max;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SmallComparableFrozenDictionary(ReadOnlySpan<KeyValuePair<TKey, TValue>> source)
            {
                var keys = new NativeArray<TKey>(source.Length);
                var values = new NativeArray<TValue>(source.Length);
                for (var i = 0; i < source.Length; ++i)
                {
                    ref readonly var entry = ref source[i];
                    keys[i] = entry.Key;
                    values[i] = entry.Value;
                }

                _keys = keys;
                _values = values;
                _max = keys[^1];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                if (Comparer<TKey>.Default.Compare(key, _max) <= 0)
                {
                    var keys = _keys;
                    for (var index = 0; index < keys.Length; ++index)
                    {
                        var num = Comparer<TKey>.Default.Compare(key, keys[index]);
                        if (num <= 0)
                        {
                            if (num == 0)
                                return ref _values[index];
                            break;
                        }
                    }
                }

                return ref Unsafe.NullRef<TValue>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _keys.Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _keys.Dispose();
                _values.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Int32FrozenDictionary<TValue> : IFrozenDictionary<int, TValue> where TValue : unmanaged
        {
            private readonly FrozenHashTable _hashTable;
            private readonly NativeArray<TValue> _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Int32FrozenDictionary(NativeArray<KeyValuePair<int, TValue>> source)
            {
                _values = new NativeArray<TValue>(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; ++index)
                    hashCodes[index] = source[index].Key;
                _hashTable = FrozenHashTable.Create(hashCodes, true);
                for (var index = 0; index < hashCodes.Length; ++index)
                    _values[hashCodes[index]] = source[index].Value;
                ArrayPool<int>.Shared.Return(array);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in int key)
            {
                _hashTable.FindMatchingEntries(key, out var startIndex, out var endIndex);
                var hashCodes = _hashTable.HashCodes;
                for (; startIndex <= endIndex; ++startIndex)
                {
                    if (key == hashCodes[startIndex])
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<int> Keys() => _hashTable.HashCodes;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _hashTable.Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<int, TValue>.Enumerator GetEnumerator() => new(_hashTable.HashCodes, _values);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _hashTable.Dispose();
                _values.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct DefaultFrozenDictionary<TKey, TValue> : IFrozenDictionary<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
        {
            private readonly FrozenHashTable _hashTable;
            private readonly NativeArray<TKey> _keys;
            private readonly NativeArray<TValue> _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public DefaultFrozenDictionary(NativeArray<KeyValuePair<TKey, TValue>> source)
            {
                var keysAreHashCodes = Constants.KeysAreHashCodes<TKey>();
                _keys = new NativeArray<TKey>(source.Length);
                _values = new NativeArray<TValue>(source.Length);
                var array = ArrayPool<int>.Shared.Rent(source.Length);
                var hashCodes = array.AsSpan(0, source.Length);
                for (var index = 0; index < source.Length; ++index)
                    hashCodes[index] = source[index].Key.GetHashCode();
                _hashTable = FrozenHashTable.Create(hashCodes, keysAreHashCodes);
                for (var index1 = 0; index1 < hashCodes.Length; ++index1)
                {
                    var index2 = hashCodes[index1];
                    _keys[index2] = source[index1].Key;
                    _values[index2] = source[index1].Value;
                }

                ArrayPool<int>.Shared.Return(array);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref readonly TValue GetValueRefOrNullRef(in TKey key)
            {
                var hashCode = key.GetHashCode();
                var hashCodes = _hashTable.HashCodes;
                for (_hashTable.FindMatchingEntries(hashCode, out var startIndex, out var endIndex); startIndex <= endIndex; ++startIndex)
                {
                    if (hashCode == hashCodes[startIndex] && key.Equals(_keys[startIndex]))
                        return ref _values[startIndex];
                }

                return ref Unsafe.NullRef<TValue>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TKey> Keys() => _keys;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<TValue> Values() => _values;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count() => _hashTable.Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeFrozenDictionary<TKey, TValue>.Enumerator GetEnumerator() => new(_keys, _values);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _hashTable.Dispose();
                _keys.Dispose();
                _values.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FrozenHashTable : IDisposable
        {
            private readonly NativeArray<int> _hashCodes;
            private readonly NativeArray<Bucket> _buckets;
            private readonly ulong _fastModMultiplier;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private FrozenHashTable(NativeArray<int> hashCodes, NativeArray<Bucket> buckets, ulong fastModMultiplier)
            {
                _hashCodes = hashCodes;
                _buckets = buckets;
                _fastModMultiplier = fastModMultiplier;
            }

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

                var hashCodes1 = new NativeArray<int>(hashCodes.Length, true);
                var buckets = new NativeArray<Bucket>(span1.Length, true);
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FindMatchingEntries(int hashCode, out int startIndex, out int endIndex)
            {
                var buckets = _buckets;
                ref var local = ref Environment.Is64BitProcess ? ref buckets[(int)HashHelpers.FastMod((uint)hashCode, (uint)buckets.Length, _fastModMultiplier)] : ref buckets[(int)((uint)hashCode % (uint)buckets.Length)];
                startIndex = local.StartIndex;
                endIndex = local.EndIndex;
            }

            public int Count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes.Length;
            }

            public ReadOnlySpan<int> HashCodes
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _hashCodes;
            }

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

            [StructLayout(LayoutKind.Sequential)]
            private readonly struct Bucket
            {
                public readonly int StartIndex;
                public readonly int EndIndex;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public Bucket(int startIndex, int count)
                {
                    StartIndex = startIndex;
                    EndIndex = startIndex + count - 1;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _buckets.Dispose();
                _hashCodes.Dispose();
            }
        }

        public static class Constants
        {
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool KeysAreHashCodes<T>() where T : unmanaged, IEquatable<T>
            {
                if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
                    return true;
                return (typeof(T) == typeof(nint) || typeof(T) == typeof(nuint)) && !Environment.Is64BitProcess;
            }
        }

#if !NET5_0_OR_GREATER
        public sealed class KeyValuePairComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            public static KeyValuePairComparer<TKey, TValue> Default { get; } = new();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => Comparer<TKey>.Default.Compare(x.Key, y.Key);
        }
#endif
    }
}