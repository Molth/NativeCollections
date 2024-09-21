using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native fixed size 32 memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeFixedSize32MemoryPool : IDisposable, IEquatable<NativeFixedSize32MemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFixedSize32MemoryPoolHandle
        {
            /// <summary>
            ///     Sentinel
            /// </summary>
            public NativeFixedSize32MemoryBlock* Sentinel;

            /// <summary>
            ///     Free list
            /// </summary>
            public NativeFixedSize32MemoryBlock* FreeList;

            /// <summary>
            ///     Blocks
            /// </summary>
            public int Blocks;

            /// <summary>
            ///     Free blocks
            /// </summary>
            public int FreeBlocks;

            /// <summary>
            ///     Max free blocks
            /// </summary>
            public int MaxFreeBlocks;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Block
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFixedSize32MemoryBlock
        {
            /// <summary>
            ///     Next
            /// </summary>
            public NativeFixedSize32MemoryBlock* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public NativeFixedSize32MemoryBlock* Previous;

            /// <summary>
            ///     Bitmap
            /// </summary>
            public uint Bitmap;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFixedSize32MemoryPoolHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeBlocks">Max free blocks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFixedSize32MemoryPool(int length, int maxFreeBlocks)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (maxFreeBlocks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeBlocks), maxFreeBlocks, "MustBeNonNegative");
            var nodeSize = 1 + length;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemoryBlock) + 32 * nodeSize));
            var block = (NativeFixedSize32MemoryBlock*)array;
            block->Next = block;
            block->Previous = block;
            block->Bitmap = 0U;
            array += sizeof(NativeFixedSize32MemoryBlock);
            for (byte i = 0; i < 32; ++i)
                *(array + i * nodeSize) = i;
            _handle = (NativeFixedSize32MemoryPoolHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeFixedSize32MemoryPoolHandle));
            _handle->Sentinel = block;
            _handle->FreeList = null;
            _handle->Blocks = 1;
            _handle->FreeBlocks = 0;
            _handle->MaxFreeBlocks = maxFreeBlocks;
            _handle->Length = length;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Blocks
        /// </summary>
        public int Blocks => _handle->Blocks;

        /// <summary>
        ///     Free blocks
        /// </summary>
        public int FreeBlocks => _handle->FreeBlocks;

        /// <summary>
        ///     Max free blocks
        /// </summary>
        public int MaxFreeBlocks => _handle->MaxFreeBlocks;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFixedSize32MemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFixedSize32MemoryPool nativeFixedSize32MemoryPool && nativeFixedSize32MemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeFixedSize32MemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFixedSize32MemoryPool left, NativeFixedSize32MemoryPool right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFixedSize32MemoryPool left, NativeFixedSize32MemoryPool right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            var node = _handle->Sentinel;
            while (_handle->Blocks > 0)
            {
                _handle->Blocks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            node = _handle->FreeList;
            while (_handle->FreeBlocks > 0)
            {
                _handle->FreeBlocks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            var block = _handle->Sentinel;
            var nodeSize = 1 + _handle->Length;
            if (block->Bitmap == uint.MaxValue)
            {
                _handle->Sentinel = block->Next;
                block = _handle->Sentinel;
                if (block->Bitmap == uint.MaxValue)
                {
                    if (_handle->FreeBlocks == 0)
                    {
                        var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemoryBlock) + 32 * nodeSize));
                        block = (NativeFixedSize32MemoryBlock*)array;
                        block->Bitmap = 0U;
                        array += sizeof(NativeFixedSize32MemoryBlock);
                        for (byte i = 0; i < 32; ++i)
                            *(array + i * nodeSize) = i;
                    }
                    else
                    {
                        block = _handle->FreeList;
                        _handle->FreeList = block->Next;
                        _handle->FreeBlocks--;
                    }

                    block->Next = _handle->Sentinel;
                    block->Previous = _handle->Sentinel->Previous;
                    _handle->Sentinel->Previous->Next = block;
                    _handle->Sentinel->Previous = block;
                    _handle->Sentinel = block;
                    _handle->Blocks++;
                }
            }

            ref var segment = ref block->Bitmap;
            var id = BitOperationsHelpers.TrailingZeroCount(~segment);
            segment |= 1U << id;
            return (byte*)block + sizeof(NativeFixedSize32MemoryBlock) + id * nodeSize + 1;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var array = (byte*)ptr;
            var id = *(array - 1);
            array -= sizeof(NativeFixedSize32MemoryBlock) + id * (1 + _handle->Length) + 1;
            var block = (NativeFixedSize32MemoryBlock*)array;
            ref var segment = ref block->Bitmap;
            segment &= ~(1U << id);
            if (segment == 0 && block != _handle->Sentinel)
            {
                block->Previous->Next = block->Next;
                block->Next->Previous = block->Previous;
                if (_handle->FreeBlocks == _handle->MaxFreeBlocks)
                {
                    NativeMemoryAllocator.Free(block);
                }
                else
                {
                    block->Next = _handle->FreeList;
                    _handle->FreeList = block;
                    _handle->FreeBlocks++;
                }

                _handle->Blocks--;
            }
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity > _handle->MaxFreeBlocks)
                capacity = _handle->MaxFreeBlocks;
            var nodeSize = 1 + _handle->Length;
            while (_handle->FreeBlocks < capacity)
            {
                _handle->FreeBlocks++;
                var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemoryBlock) + 32 * nodeSize));
                var block = (NativeFixedSize32MemoryBlock*)array;
                block->Bitmap = 0U;
                array += sizeof(NativeFixedSize32MemoryBlock);
                for (byte i = 0; i < 32; ++i)
                    *(array + i * nodeSize) = i;
                block->Next = _handle->FreeList;
                _handle->FreeList = block;
            }

            return _handle->FreeBlocks;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var node = _handle->FreeList;
            while (_handle->FreeBlocks > 0)
            {
                _handle->FreeBlocks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            _handle->FreeList = node;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            var node = _handle->FreeList;
            while (_handle->FreeBlocks > capacity)
            {
                _handle->FreeBlocks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            _handle->FreeList = node;
            return _handle->FreeBlocks;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeFixedSize32MemoryPool Empty => new();
    }
}