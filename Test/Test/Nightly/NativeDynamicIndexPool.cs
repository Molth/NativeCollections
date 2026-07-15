using System;
using System.Numerics;
using NativeCollections;

namespace Examples
{
    public struct NativeDynamicIndexPool : IDisposable
    {
        private UnsafeList<Block> _blocks;
        private UnsafeList<int> _indexes;
        private int _fullCount;

        public NativeDynamicIndexPool(int initialBlocks = 1)
        {
            _blocks = new UnsafeList<Block>(initialBlocks);
            _indexes = new UnsafeList<int>(initialBlocks);
            _fullCount = 0;

            for (var i = 0; i < initialBlocks; i++)
                AddNewBlock();
        }

        private void AddNewBlock()
        {
            var position = _blocks.Count;
            _blocks.Add(new Block(position));
            _indexes.Add(position);
        }

        public void Dispose()
        {
            _blocks.Dispose();
            _indexes.Dispose();
        }

        public int Rent()
        {
            if (_fullCount == _indexes.Count)
                AddNewBlock();

            var blockIndex = _indexes[_fullCount];
            ref var block = ref _blocks[blockIndex];
            var segment = block.Bitmap;
            var bitMask = BitOperations.TrailingZeroCount(~segment);
            block.Bitmap |= 1U << bitMask;
            var index = (blockIndex << 5) + bitMask;

            if (block.Bitmap == uint.MaxValue)
                _fullCount += 1;

            return index;
        }

        public void Return(int index)
        {
            var blockIndex = index >> 5;

            if ((uint)blockIndex >= (uint)_blocks.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            ref var block = ref _blocks[blockIndex];
            var wasFull = block.Bitmap == uint.MaxValue;
            block.Bitmap &= ~(1U << (index & 31));

            if (wasFull)
            {
                var position = block.Position;

                if (position != _indexes.Count - 1)
                {
                    var lastFullPosition = _fullCount != _indexes.Count ? _fullCount : _fullCount - 1;

                    var firstNonFullBlock = _indexes[lastFullPosition];
                    _indexes[position] = firstNonFullBlock;
                    _indexes[lastFullPosition] = blockIndex;
                    _blocks[firstNonFullBlock].Position = position;
                    _blocks[blockIndex].Position = lastFullPosition;
                }

                _fullCount -= 1;
            }
        }

        private struct Block : IEquatable<Block>
        {
            public int Position;
            public uint Bitmap;

            public Block(int position = 0)
            {
                Position = position;
                Bitmap = 0;
            }

            public bool Equals(Block other) => UnsafeBitwise<Block>.Equals(ref this, ref other);

            public override bool Equals(object? obj) => obj is Block other && Equals(other);

            public override int GetHashCode() => NativeHashCode.GetHashCode(this);
        }
    }
}