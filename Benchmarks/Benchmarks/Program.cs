using BenchmarkDotNet.Running;
using mimalloc;
using NativeCollections;

namespace Benchmarks
{
    internal sealed class Program
    {
        private static void Main()
        {
            Custom();
            
            BenchmarkRunner.Run<BenchmarkDistinct>();
        }
        
        private static unsafe void Custom()
        {
            try
            {
             _=  MiMalloc.mi_version();
            }
            catch
            {
                return;
            }
            
            NativeMemoryAllocator.Custom(&Alloc, &AllocZeroed, &Free);
            return;

            static void* Alloc(uint byteCount) => MiMalloc.mi_malloc(byteCount);
            static void* AllocZeroed(uint byteCount) => MiMalloc.mi_zalloc(byteCount);
            static void Free(void* ptr) => MiMalloc.mi_free(ptr);
        }
    }
}