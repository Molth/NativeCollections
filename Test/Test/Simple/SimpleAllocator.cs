using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NativeCollections;

namespace Examples
{
    public static unsafe class SimpleAllocator
    {
        public static int AllocCount;

        public static void Custom() => NativeMemoryAllocator.Custom(&AlignedAlloc, &AlignedAllocZeroed, &AlignedFree);

        public static void* AlignedAlloc(uint byteCount, uint alignment)
        {
            Interlocked.Increment(ref AllocCount);
            return NativeMemory.AlignedAlloc(byteCount, alignment);
        }

        public static void* AlignedAllocZeroed(uint byteCount, uint alignment)
        {
            Interlocked.Increment(ref AllocCount);
            var ptr = NativeMemory.AlignedAlloc(byteCount, alignment);
            Unsafe.InitBlockUnaligned(ptr, 0, byteCount);
            return ptr;
        }

        public static void AlignedFree(void* ptr)
        {
            if (ptr == null)
                return;

            Interlocked.Decrement(ref AllocCount);
            NativeMemory.AlignedFree(ptr);
        }
    }
}