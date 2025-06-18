using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using NativeCollections;

#pragma warning disable CS8618

/*
| Method       | Count | Mean        | Error        | StdDev     | Ratio | RatioSD |
|------------- |------ |------------:|-------------:|-----------:|------:|--------:|
| TestCompare1 | 100   |    36.17 us |     8.999 us |   0.493 us |  0.10 |    0.01 |
| TestCompare2 | 100   |   377.57 us |   557.555 us |  30.561 us |  1.00 |    0.10 |
|              |       |             |              |            |       |         |
| TestCompare1 | 1000  |   254.80 us |   105.183 us |   5.765 us |  0.07 |    0.00 |
| TestCompare2 | 1000  | 3,648.73 us |   682.084 us |  37.387 us |  1.00 |    0.01 |
|              |       |             |              |            |       |         |
| TestCompare1 | 10000 | 1,591.93 us | 2,097.371 us | 114.964 us |  0.20 |    0.01 |
| TestCompare2 | 10000 | 7,863.60 us | 1,922.438 us | 105.375 us |  1.00 |    0.02 |
 */

namespace Benchmarks
{
    [ShortRunJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public unsafe class BenchmarkCompare
    {
        public long Result1;
        public long Result2;

        private List<byte[]> _input;
        private byte[] _buffer;

        [Params(100, 1000, 10000)] public int Count { get; set; }

        [IterationSetup]
        public void Init()
        {
            _buffer = new byte[2048];
            _input = new List<byte[]>(Count);
            for (var i = 0; i < Count; i++)
            {
                var bytes = new byte[_buffer.Length];
                bytes[Random.Shared.Next(bytes.Length)] = (byte)((Random.Shared.Next() & 1) == 0 ? 1 : 0);
                _input.Add(bytes);
            }
        }

        [BenchmarkCategory("Test")]
        [Benchmark(Baseline = false)]
        public void TestCompare1()
        {
            foreach (var guid in _input)
            {
                fixed (byte* left = guid)
                {
                    fixed (byte* right = _buffer)
                    {
                        Result1 ^= NativeMemoryAllocator.Compare(left, right, (uint)guid.Length) ? 1 : 0;
                    }
                }
            }
        }

        [BenchmarkCategory("Test")]
        [Benchmark(Baseline = true)]
        public void TestCompare2()
        {
            foreach (var guid in _input)
            {
                fixed (byte* left = guid)
                {
                    fixed (byte* right = _buffer)
                    {
                        Result2 ^= Compare(left, right, (uint)guid.Length) ? 1 : 0;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(void* left, void* right, uint byteCount)
        {
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                throw new ArgumentNullException(left == null ? nameof(left) : nameof(right));

            if (left == right)
                return true;

            var bytesLeft = (byte*)left;
            var bytesRight = (byte*)right;

            for (uint i = 0; i < byteCount; ++i)
            {
                if (bytesLeft[i] != bytesRight[i])
                    return false;
            }

            return true;
        }
    }
}