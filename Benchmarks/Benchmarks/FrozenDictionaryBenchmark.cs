using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using NativeCollections;

#pragma warning disable CS8618

// | Method                             | Count  | HitRatio | Mean     | Error      | StdDev    | Ratio | RatioSD |
// |----------------------------------- |------- |--------- |---------:|-----------:|----------:|------:|--------:|
// | Dictionary_TryGetValue             | 100000 | 0.2      | 3.844 ms |  5.9718 ms | 0.3273 ms |  1.00 |    0.10 |
// | NativeDictionary_TryGetValue       | 100000 | 0.2      | 2.980 ms |  5.6071 ms | 0.3073 ms |  0.78 |    0.09 |
// | FrozenDictionary_TryGetValue       | 100000 | 0.2      | 2.518 ms |  3.4447 ms | 0.1888 ms |  0.66 |    0.06 |
// | NativeFrozenDictionary_TryGetValue | 100000 | 0.2      | 2.071 ms |  0.7927 ms | 0.0435 ms |  0.54 |    0.04 |
// | ConcurrentDictionary_TryGetValue   | 100000 | 0.2      | 4.068 ms | 11.1064 ms | 0.6088 ms |  1.06 |    0.16 |

namespace Benchmarks
{
    [ShortRunJob]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class FrozenDictionaryBenchmark
    {
        private FrozenDictionary<Guid, Guid> _frozenDict;
        private NativeFrozenDictionary<Guid, Guid> _frozenDict2;
        private Dictionary<Guid, Guid> _dict;
        private NativeDictionary<Guid, Guid> _dict2;
        private ConcurrentDictionary<Guid, Guid> _concurrentDict;
        private Guid[] _keysToLookup;
        private int _result1;
        private int _result2;
        private int _result3;
        private int _result4;

        [Params(100000)] public int Count { get; set; }

        [Params(0.2)] public double HitRatio { get; set; }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _dict2.Dispose();
            _frozenDict2.Dispose();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var allKeys = new Guid[Count];
            for (var i = 0; i < Count; i++)
            {
                allKeys[i] = Guid.NewGuid();
            }

            var data = new Dictionary<Guid, Guid>();
            _dict2 = new NativeDictionary<Guid, Guid>(allKeys.Length);
            foreach (var key in allKeys)
            {
                data[key] = Guid.NewGuid();
                _dict2[key] = data[key];
            }

            _frozenDict = data.ToFrozenDictionary();
            _frozenDict2 = NativeFrozenDictionary<Guid, Guid>.Create(data);
            _dict = new Dictionary<Guid, Guid>(data);
            _concurrentDict = new ConcurrentDictionary<Guid, Guid>(data);

            var keysToLookup = new List<Guid>();
            var hitCount = (int)(Count * HitRatio);
            var missCount = Count - hitCount;

            for (var i = 0; i < hitCount; i++)
            {
                keysToLookup.Add(allKeys[i]);
            }

            for (var i = 0; i < missCount; i++)
            {
                keysToLookup.Add(Guid.NewGuid());
            }

            _keysToLookup = keysToLookup.OrderBy(_ => Random.Shared.Next()).ToArray();
        }

        [BenchmarkCategory("TryGetValue")]
        [Benchmark(Baseline = true)]
        public void Dictionary_TryGetValue()
        {
            foreach (var key in _keysToLookup)
            {
                if (_dict.TryGetValue(key, out var value))
                {
                    _result1 ^= value.GetHashCode();
                }
            }
        }

        [BenchmarkCategory("TryGetValue")]
        [Benchmark]
        public void NativeDictionary_TryGetValue()
        {
            foreach (var key in _keysToLookup)
            {
                if (_dict2.TryGetValue(key, out var value))
                {
                    _result1 ^= value.GetHashCode();
                }
            }
        }

        [BenchmarkCategory("TryGetValue")]
        [Benchmark]
        public void FrozenDictionary_TryGetValue()
        {
            foreach (var key in _keysToLookup)
            {
                if (_frozenDict.TryGetValue(key, out var value))
                {
                    _result2 ^= value.GetHashCode();
                }
            }
        }

        [BenchmarkCategory("TryGetValue")]
        [Benchmark]
        public void NativeFrozenDictionary_TryGetValue()
        {
            foreach (var key in _keysToLookup)
            {
                if (_frozenDict2.TryGetValue(key, out var value))
                {
                    _result4 ^= value.GetHashCode();
                }
            }
        }

        [BenchmarkCategory("TryGetValue")]
        [Benchmark]
        public void ConcurrentDictionary_TryGetValue()
        {
            foreach (var key in _keysToLookup)
            {
                if (_concurrentDict.TryGetValue(key, out var value))
                {
                    _result3 ^= value.GetHashCode();
                }
            }
        }
    }
}