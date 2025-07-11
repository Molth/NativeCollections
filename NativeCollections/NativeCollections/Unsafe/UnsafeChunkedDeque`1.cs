using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe chunked deque
    ///     (Slower than Deque, disable Enumerator)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedDeque<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        ///     Head
        /// </summary>
        private MemoryChunk* _head;

        /// <summary>
        ///     Tail
        /// </summary>
        private MemoryChunk* _tail;

        /// <summary>
        ///     Free list
        /// </summary>
        private MemoryChunk* _freeList;

        /// <summary>
        ///     Chunks
        /// </summary>
        private int _chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        private int _freeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        private int _maxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Read offset
        /// </summary>
        private int _readOffset;

        /// <summary>
        ///     Write offset
        /// </summary>
        private int _writeOffset;

        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _count == 0;

        /// <summary>
        ///     Chunks
        /// </summary>
        public readonly int Chunks => _chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        public readonly int FreeChunks => _freeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        public readonly int MaxFreeChunks => _maxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        public readonly int Size => _size;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _count;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunkedDeque(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, nameof(size));
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, nameof(maxFreeChunks));
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
            var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemoryChunk), alignment);
            var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + size * sizeof(T)), alignment);
            chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
            _head = chunk;
            _tail = chunk;
            _freeList = null;
            _chunks = 1;
            _freeChunks = 0;
            _maxFreeChunks = maxFreeChunks;
            _size = size;
            _readOffset = 0;
            _writeOffset = 0;
            _count = 0;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var node = _head;
            while (_chunks > 0)
            {
                _chunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }
        }

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_chunks != 1)
            {
                _freeChunks += _chunks - 1;
                _chunks = 1;
                var chunk = _head->Next;
                chunk->Next = _freeList;
                _freeList = chunk;
                TrimExcess(_maxFreeChunks);
                _tail = _head;
            }

            _readOffset = 0;
            _writeOffset = 0;
            _count = 0;
        }

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueHead(in T item)
        {
            if (_readOffset == 0)
            {
                _readOffset = _size;
                if (_count != 0)
                {
                    MemoryChunk* chunk;
                    if (_freeChunks == 0)
                    {
                        var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                        var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemoryChunk), alignment);
                        chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * sizeof(T)), alignment);
                        chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        --_freeChunks;
                    }

                    chunk->Next = _head;
                    _head->Previous = chunk;
                    _head = chunk;
                    ++_chunks;
                }
                else
                {
                    _writeOffset = _size;
                }
            }

            ++_count;
            Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)(--_readOffset)) = item;
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item)
        {
            if (_writeOffset == _size)
            {
                _writeOffset = 0;
                MemoryChunk* chunk;
                if (_freeChunks == 0)
                {
                    var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                    var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemoryChunk), alignment);
                    chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * sizeof(T)), alignment);
                    chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                }
                else
                {
                    chunk = _freeList;
                    _freeList = chunk->Next;
                    --_freeChunks;
                }

                chunk->Previous = _tail;
                _tail->Next = chunk;
                _tail = chunk;
                ++_chunks;
            }

            ++_count;
            Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)_writeOffset++) = item;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            --_count;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)_readOffset++);
            if (_readOffset == _size)
            {
                _readOffset = 0;
                if (_chunks != 1)
                {
                    var chunk = _head;
                    _head = chunk->Next;
                    if (_freeChunks == _maxFreeChunks)
                    {
                        NativeMemoryAllocator.AlignedFree(chunk);
                    }
                    else
                    {
                        chunk->Next = _freeList;
                        _freeList = chunk;
                        ++_freeChunks;
                    }

                    --_chunks;
                }
                else
                {
                    _writeOffset = 0;
                }
            }

            return true;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueTail(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }

            --_count;
            result = Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)(--_writeOffset));
            if (_writeOffset == 0 && _chunks != 1)
            {
                _writeOffset = _size;
                var chunk = _tail;
                _tail = chunk->Previous;
                if (_freeChunks == _maxFreeChunks)
                {
                    NativeMemoryAllocator.AlignedFree(chunk);
                }
                else
                {
                    chunk->Next = _freeList;
                    _freeList = chunk;
                    ++_freeChunks;
                }

                --_chunks;
            }

            return true;
        }

        /// <summary>
        ///     Try peek head
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekHead(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_head->Buffer), (nint)_readOffset);
            return true;
        }

        /// <summary>
        ///     Try peek tail
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekTail(out T result)
        {
            if (_size == 0)
            {
                result = default;
                return false;
            }

            result = Unsafe.Add(ref Unsafe.AsRef<T>(_tail->Buffer), (nint)(_writeOffset - 1));
            return true;
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            if (capacity > _maxFreeChunks)
                capacity = _maxFreeChunks;
            while (_freeChunks < capacity)
            {
                _freeChunks++;
                var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<MemoryChunk>(), NativeMemoryAllocator.AlignOf<T>());
                var chunkByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)sizeof(MemoryChunk), alignment);
                var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(chunkByteCount + _size * sizeof(T)), alignment);
                chunk->Buffer = UnsafeHelpers.AddByteOffset<T>(chunk, (nint)chunkByteCount);
                chunk->Next = _freeList;
                _freeList = chunk;
            }

            return _freeChunks;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var node = _freeList;
            while (_freeChunks > 0)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            var node = _freeList;
            while (_freeChunks > capacity)
            {
                _freeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.AlignedFree(temp);
            }

            _freeList = node;
            return _freeChunks;
        }

        /// <summary>
        ///     Chunk
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryChunk
        {
            /// <summary>
            ///     Next
            /// </summary>
            public MemoryChunk* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public MemoryChunk* Previous;

            /// <summary>
            ///     Buffer
            /// </summary>
            public T* Buffer;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeChunkedDeque<T> Empty => new();
    }
}