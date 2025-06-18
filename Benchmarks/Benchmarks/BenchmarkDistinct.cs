using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using NativeCollections;

#pragma warning disable CS8618
#pragma warning disable CS9081

/*
| Method        | Count  | Mean           | Error         | StdDev      | Median         | Ratio | RatioSD |
|-------------- |------- |---------------:|--------------:|------------:|---------------:|------:|--------:|
| TestDistinct1 | 100    |       2.367 us |      7.373 us |   0.4041 us |       2.300 us |  0.83 |    0.15 |
| TestDistinct2 | 100    |       4.500 us |      1.824 us |   0.1000 us |       4.500 us |  1.59 |    0.17 |
| TestDistinct3 | 100    |       2.867 us |      6.907 us |   0.3786 us |       2.700 us |  1.01 |    0.16 |
|               |        |                |               |             |                |       |         |
| TestDistinct1 | 1000   |      13.533 us |    152.903 us |   8.3811 us |       9.100 us |  0.45 |    0.27 |
| TestDistinct2 | 1000   |      54.633 us |     35.673 us |   1.9553 us |      54.800 us |  1.82 |    0.44 |
| TestDistinct3 | 1000   |      31.933 us |    190.228 us |  10.4270 us |      27.100 us |  1.06 |    0.40 |
|               |        |                |               |             |                |       |         |
| TestDistinct1 | 10000  |      69.033 us |     24.769 us |   1.3577 us |      68.300 us |  0.50 |    0.05 |
| TestDistinct2 | 10000  |   2,353.167 us |  1,243.876 us |  68.1810 us |   2,318.700 us | 17.08 |    1.58 |
| TestDistinct3 | 10000  |     138.767 us |    264.026 us |  14.4722 us |     136.600 us |  1.01 |    0.13 |
|               |        |                |               |             |                |       |         |
| TestDistinct1 | 100000 |     986.900 us |    706.880 us |  38.7465 us |     991.400 us |  0.60 |    0.04 |
| TestDistinct2 | 100000 | 152,346.800 us | 13,427.332 us | 735.9974 us | 152,652.600 us | 93.34 |    4.47 |
| TestDistinct3 | 100000 |   1,635.400 us |  1,624.211 us |  89.0285 us |   1,647.100 us |  1.00 |    0.07 |
*/

namespace Benchmarks
{
    [ShortRunJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class BenchmarkDistinct
    {
        public List<int> Input;

        [Params(100, 1000, 10000, 100000)] public int Count { get; set; }

        [IterationSetup]
        public void Init()
        {
            Input = new List<int>(Count);
            for (var i = 0; i < Count; ++i)
            {
                var item = Random.Shared.Next(Count);
                Input.Add(item);
            }
        }

        [BenchmarkCategory("Test")]
        [Benchmark(Baseline = false)]
        public void TestDistinct1()
        {
            if (Input.Count != Count)
                throw new InvalidOperationException();

            var span = CollectionsMarshal.AsSpan(Input);
            Distinct1(ref span);
            CollectionsMarshal.SetCount(Input, span.Length);
        }

        [BenchmarkCategory("Test")]
        [Benchmark(Baseline = false)]
        public void TestDistinct2()
        {
            if (Input.Count != Count)
                throw new InvalidOperationException();

            var span = CollectionsMarshal.AsSpan(Input);
            Distinct2(ref span);
            CollectionsMarshal.SetCount(Input, span.Length);
        }

        [BenchmarkCategory("Test")]
        [Benchmark(Baseline = true)]
        public void TestDistinct3()
        {
            if (Input.Count != Count)
                throw new InvalidOperationException();

            var span = CollectionsMarshal.AsSpan(Input);
            Distinct3(ref span);
            CollectionsMarshal.SetCount(Input, span.Length);
        }

        public static void Distinct1<T>(ref Span<T> source) where T : unmanaged, IEquatable<T>
        {
            var byteCount = StackallocHashSet<T>.GetByteCount(source.Length);
            Span<byte> bytes;
            var buffer = NativeTempPinnedBuffer<byte>.Empty;
            if (byteCount <= 1024)
            {
                Span<byte> bytes2 = stackalloc byte[byteCount];
                bytes = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(bytes2), byteCount);
            }
            else
            {
                buffer = new NativeTempPinnedBuffer<byte>(byteCount, true);
                bytes = buffer.AsSpan();
            }

            var hashSet = new StackallocHashSet<T>(bytes, source.Length);
            var index = 0;
            ref var reference = ref MemoryMarshal.GetReference(source);
            for (var i = 0; i < source.Length; ++i)
            {
                ref var item = ref Unsafe.Add(ref reference, i);
                var result = hashSet.TryAdd(item);
                if (result == InsertResult.Success)
                {
                    if (index != i)
                        source[index] = source[i];
                    ++index;
                }
            }

            buffer.Dispose();
            source = MemoryMarshal.CreateSpan(ref reference, index);
        }

        public static void Distinct2<T>(ref Span<T> source) where T : unmanaged, IEquatable<T>
        {
            var index = 0;
            for (var i = 0; i < source.Length; ++i)
            {
                if (!source.Contains(source[i]))
                {
                    if (index != i)
                        source[index] = source[i];
                    ++index;
                }
            }

            source = source.Slice(0, index);
        }

        public static void Distinct3<T>(ref Span<T> source) where T : unmanaged, IEquatable<T>
        {
            var hashSet = new HashSet<T>(source.Length);
            var index = 0;
            ref var reference = ref MemoryMarshal.GetReference(source);
            for (var i = 0; i < source.Length; ++i)
            {
                ref var item = ref Unsafe.Add(ref reference, i);
                var result = hashSet.Add(item);
                if (result)
                {
                    if (index != i)
                        source[index] = source[i];
                    ++index;
                }
            }

            source = MemoryMarshal.CreateSpan(ref reference, index);
        }
    }
}