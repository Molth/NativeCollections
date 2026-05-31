using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NativeCollections
{
    public unsafe struct ArenaAllocator : IDisposable
    {
        private const int CACHE_LINE_SIZE = 128;
        private const int MAX_SIZE = 64;
        private UnsafeConcurrentSpinLock _spinLock;
        private NativeArray<MemoryBlock> _blocks;
        private int _size;
        private uint _length;

        public void Initialize(uint length)
        {
            _spinLock = new UnsafeConcurrentSpinLock();
            _blocks = new NativeArray<MemoryBlock>(MAX_SIZE);
            _size = 0;
            _length = length;
        }

        public void Dispose()
        {
            for (var i = 0; i < _size; ++i)
            {
                ref var block = ref _blocks[i];
                block.Dispose();
            }

            _blocks.Dispose();
        }

        public void Rewind()
        {
            for (var i = 0; i < _size; ++i)
            {
                ref var block = ref _blocks[i];
                block.Current = block.StartAddress;
            }
        }

        public void* AlignedAlloc(uint byteCount, uint alignment)
        {
            void* ptr;
            ref var block = ref Unsafe.NullRef<MemoryBlock>();
            var size = Volatile.Read(ref _size);
            for (var i = size - 1; i >= 0; --i)
            {
                block = ref _blocks[i];
                ptr = AlignedAlloc(ref block, byteCount, alignment);
                if (ptr != null)
                    return ptr;
            }

            if (size == MAX_SIZE)
                return null;

            _spinLock.Enter();
            var newSize = Volatile.Read(ref _size);
            for (var i = newSize - 1; i >= size; --i)
            {
                block = ref _blocks[i];
                ptr = AlignedAlloc(ref block, byteCount, alignment);
                if (ptr != null)
                {
                    _spinLock.Exit();
                    return ptr;
                }
            }

            if (newSize == MAX_SIZE)
            {
                _spinLock.Exit();
                return null;
            }

            var newBlock = new MemoryBlock(_length);
            _blocks[newSize] = newBlock;
            block = ref _blocks[newSize];
            ptr = AlignedAlloc(ref block, byteCount, alignment);
            Volatile.Write(ref _size, newSize + 1);
            _spinLock.Exit();
            return ptr;
        }

        private static void* AlignedAlloc(ref MemoryBlock block, uint byteCount, uint alignment)
        {
            while (true)
            {
                var current = Volatile.Read(ref block.Current);
                var newCurrent = (ulong)NativeMemoryAllocator.AlignUp((nuint)current, alignment);
                if (newCurrent + byteCount > block.EndAddress)
                    return null;
                if (Interlocked.CompareExchange(ref block.Current, newCurrent + byteCount, current) == current)
                    return (void*)(nint)newCurrent;
            }
        }

        private struct MemoryBlock : IDisposable
        {
            public readonly ulong StartAddress;
            public readonly ulong EndAddress;
            public ulong Current;

            public MemoryBlock(uint length)
            {
                var ptr = (ulong)(nint)NativeMemoryAllocator.AlignedAlloc(length, CACHE_LINE_SIZE);
                StartAddress = ptr;
                EndAddress = ptr + length;
                Current = ptr;
            }

            public void Dispose() => NativeMemoryAllocator.AlignedFree((void*)StartAddress);
        }
    }
}