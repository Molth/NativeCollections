using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using NativeCollections;

namespace Examples
{
    public unsafe struct NativeFixedSizeBitmapPtrPool<T> : IDisposable where T : unmanaged
    {
        private readonly NativeArray<int> _indexes;
        private readonly NativeArray<Block> _blocks;
        private readonly NativeMemoryArray<T> _buffer;
        private int _fullCount;

        public NativeFixedSizeBitmapPtrPool(int blocks)
        {
            var capacity = blocks * 32;
            var alignment = Math.Max((uint)NativeMemoryAllocator.AlignOf<int>(), (uint)NativeMemoryAllocator.AlignOf<T>());
            alignment = Math.Max(alignment, (uint)NativeMemoryAllocator.AlignOf<Block>());
            var indexesByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(blocks * Unsafe.SizeOf<int>()), alignment);
            var blocksByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(blocks * Unsafe.SizeOf<Block>()), alignment);
            var ptr = (byte*)NativeMemoryAllocator.AlignedAllocZeroed((uint)(indexesByteCount + blocksByteCount + capacity * Unsafe.SizeOf<T>()), alignment);
            _indexes = new NativeArray<int>((int*)ptr, blocks);
            ptr += indexesByteCount;
            _blocks = new NativeArray<Block>((Block*)ptr, blocks);
            ptr += blocksByteCount;
            _buffer = new NativeMemoryArray<T>((T*)ptr, capacity);
            _fullCount = 0;
            for (var i = 0; i < blocks; ++i)
            {
                _indexes[i] = i;
                _blocks[i] = new Block(i);
            }
        }

        public void Dispose() => _indexes.Dispose();

        public bool TryRent(out T* ptr)
        {
            if (_fullCount == _indexes.Length)
            {
                ptr = null;
                return false;
            }

            ref var block = ref _blocks[_indexes[0]];
            ref var segment = ref block.Bitmap;
            var bitMask = BitOperations.TrailingZeroCount(~segment);
            segment |= 1U << bitMask;
            var index = block.Index1 * 32 + bitMask;
            ptr = _buffer[index];
            if (segment == uint.MaxValue)
            {
                _fullCount += 1;
                if (_fullCount != _indexes.Length)
                {
                    var firstFullPosition = _indexes.Length - _fullCount;
                    var block1 = _indexes[0];
                    var block2 = _indexes[firstFullPosition];
                    _indexes[0] = block2;
                    _indexes[firstFullPosition] = block1;
                    _blocks[block1].Index2 = firstFullPosition;
                    _blocks[block2].Index2 = 0;
                }
            }

            return true;
        }

        public void Return(void* ptr)
        {
            var index = (int)((T*)ptr - _buffer[0]);
            ref var block = ref _blocks[index >> 5];
            var wasFull = block.Bitmap == uint.MaxValue;
            block.Bitmap &= ~(1U << (index & 31));
            if (wasFull)
            {
                var position = block.Index2;
                var firstFullPosition = _indexes.Length - _fullCount;
                var block1 = _indexes[position];
                var block2 = _indexes[firstFullPosition];
                _indexes[position] = block2;
                _indexes[firstFullPosition] = block1;
                _blocks[block1].Index2 = firstFullPosition;
                _blocks[block2].Index2 = position;
                _fullCount -= 1;
            }
        }

        private struct Block
        {
            public readonly int Index1;
            public int Index2;
            public uint Bitmap;

            public Block(int index)
            {
                Index1 = index;
                Index2 = index;
                Bitmap = 0;
            }
        }
    }
}