using BenchmarkDotNet.Running;

namespace Benchmarks
{
    internal sealed class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<FrozenDictionaryBenchmark>();
        }
    }
}