using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeChunkedStream : IDisposable
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
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

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
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeChunkedStream(int size, int maxFreeChunks)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(size, nameof(size));
            ThrowHelpers.ThrowIfNegative(maxFreeChunks, nameof(maxFreeChunks));
            var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemoryChunk) + size), (uint)NativeMemoryAllocator.AlignOf<MemoryChunk>());
            _head = chunk;
            _tail = chunk;
            _freeList = null;
            _chunks = 1;
            _freeChunks = 0;
            _maxFreeChunks = maxFreeChunks;
            _size = size;
            _readOffset = 0;
            _writeOffset = 0;
            _length = 0;
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
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length) => Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            var length = buffer.Length;
            ThrowHelpers.ThrowIfNegative(length, nameof(buffer));
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            if (length >= _length)
            {
                length = _length;
                if (length == 0)
                    return 0;
                var size = _size;
                var byteCount = UnsafeHelpers.ToIntPtr(size - _readOffset);
                if (byteCount > length)
                {
                    Unsafe.CopyBlockUnaligned(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)length);
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)byteCount);
                    if (byteCount != length)
                    {
                        MemoryChunk* chunk;
                        var count = length - byteCount;
                        var (chunks, remaining) = MathHelpers.DivRem(count, size);
                        for (var i = 0; i < chunks; ++i)
                        {
                            chunk = _head;
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
                            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)size);
                            byteCount += size;
                        }

                        if (remaining != 0)
                        {
                            chunk = _head;
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
                            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)remaining);
                        }
                    }
                }

                _readOffset = 0;
                _writeOffset = 0;
                _length = 0;
            }
            else
            {
                if (length == 0)
                    return 0;
                var size = _size;
                var byteCount = UnsafeHelpers.ToIntPtr(size - _readOffset);
                if (byteCount > length)
                {
                    Unsafe.CopyBlockUnaligned(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)length);
                    _readOffset += length;
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(ref reference, ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_head->Buffer), new IntPtr(_readOffset)), (uint)byteCount);
                    MemoryChunk* chunk;
                    var count = length - byteCount;
                    var (chunks, remaining) = MathHelpers.DivRem(count, size);
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = _head;
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
                        Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = _head;
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
                        Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref Unsafe.AsRef<byte>(_head->Buffer), (uint)remaining);
                    }

                    _readOffset = (int)remaining;
                }

                _length -= length;
            }

            return length;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            var length = buffer.Length;
            ThrowHelpers.ThrowIfNegative(length, nameof(buffer));
            if (length == 0)
                return;
            ref var reference = ref MemoryMarshal.GetReference(buffer);
            var size = _size;
            var byteCount = UnsafeHelpers.ToIntPtr(size - _writeOffset);
            if (byteCount >= length)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, (uint)length);
                _writeOffset += length;
            }
            else
            {
                if (byteCount != 0)
                    Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_tail->Buffer), new IntPtr(_writeOffset)), ref reference, (uint)byteCount);
                MemoryChunk* chunk;
                var count = length - byteCount;
                var (chunks, remaining) = MathHelpers.DivRem(count, size);
                for (var i = 0; i < chunks; ++i)
                {
                    if (_freeChunks == 0)
                    {
                        chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemoryChunk) + size), (uint)NativeMemoryAllocator.AlignOf<MemoryChunk>());
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        --_freeChunks;
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                    Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, byteCount), (uint)size);
                    byteCount += size;
                }

                if (remaining != 0)
                {
                    if (_freeChunks == 0)
                    {
                        chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemoryChunk) + size), (uint)NativeMemoryAllocator.AlignOf<MemoryChunk>());
                    }
                    else
                    {
                        chunk = _freeList;
                        _freeList = chunk->Next;
                        --_freeChunks;
                    }

                    _tail->Next = chunk;
                    _tail = chunk;
                    ++_chunks;
                    Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(_tail->Buffer), ref Unsafe.AddByteOffset(ref reference, byteCount), (uint)remaining);
                }

                _writeOffset = (int)remaining;
            }

            _length += length;
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
            var size = _size;
            while (_freeChunks < capacity)
            {
                _freeChunks++;
                var chunk = (MemoryChunk*)NativeMemoryAllocator.AlignedAlloc((uint)(sizeof(MemoryChunk) + size), (uint)NativeMemoryAllocator.AlignOf<MemoryChunk>());
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
            ///     Buffer
            /// </summary>
            public fixed byte Buffer[1];
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeChunkedStream Empty => new();
    }
}