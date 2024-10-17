using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeChunkedStream : IDisposable, IEquatable<NativeChunkedStream>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeChunkedStreamHandle
        {
            /// <summary>
            ///     Head
            /// </summary>
            public NativeMemoryChunk* Head;

            /// <summary>
            ///     Tail
            /// </summary>
            public NativeMemoryChunk* Tail;

            /// <summary>
            ///     Free list
            /// </summary>
            public NativeMemoryChunk* FreeList;

            /// <summary>
            ///     Chunks
            /// </summary>
            public int Chunks;

            /// <summary>
            ///     Free chunks
            /// </summary>
            public int FreeChunks;

            /// <summary>
            ///     Max free chunks
            /// </summary>
            public int MaxFreeChunks;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Read offset
            /// </summary>
            public int ReadOffset;

            /// <summary>
            ///     Write offset
            /// </summary>
            public int WriteOffset;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Chunk
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryChunk
        {
            /// <summary>
            ///     Next
            /// </summary>
            public NativeMemoryChunk* Next;

            /// <summary>
            ///     Array
            /// </summary>
            public fixed byte Array[1];
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeChunkedStreamHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeChunkedStream(int size, int maxFreeChunks)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (maxFreeChunks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeChunks), maxFreeChunks, "MustBeNonNegative");
            var handle = (NativeChunkedStreamHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeChunkedStreamHandle));
            var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
            handle->Head = chunk;
            handle->Tail = chunk;
            handle->FreeList = null;
            handle->Chunks = 1;
            handle->FreeChunks = 0;
            handle->MaxFreeChunks = maxFreeChunks;
            handle->Size = size;
            handle->ReadOffset = 0;
            handle->WriteOffset = 0;
            handle->Length = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Length == 0;

        /// <summary>
        ///     Chunks
        /// </summary>
        public int Chunks => _handle->Chunks;

        /// <summary>
        ///     Free chunks
        /// </summary>
        public int FreeChunks => _handle->FreeChunks;

        /// <summary>
        ///     Max free chunks
        /// </summary>
        public int MaxFreeChunks => _handle->MaxFreeChunks;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _handle->Size;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeChunkedStream other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeChunkedStream nativeChunkedStream && nativeChunkedStream == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeChunkedStream";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeChunkedStream left, NativeChunkedStream right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeChunkedStream left, NativeChunkedStream right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            var node = handle->Head;
            while (handle->Chunks > 0)
            {
                handle->Chunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            node = handle->FreeList;
            while (handle->FreeChunks > 0)
            {
                handle->FreeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = _handle;
            if (length >= handle->Length)
            {
                length = handle->Length;
                if (length == 0)
                    return 0;
                var size = handle->Size;
                var byteCount = size - handle->ReadOffset;
                if (byteCount < length)
                {
                    NativeMemoryChunk* chunk;
                    var count = length - byteCount;
                    var chunks = ((count + size - 1) / size) | 0;
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = handle->Head;
                        handle->Head = chunk->Next;
                        if (handle->FreeChunks == handle->MaxFreeChunks)
                        {
                            NativeMemoryAllocator.Free(chunk);
                        }
                        else
                        {
                            chunk->Next = handle->FreeList;
                            handle->FreeList = chunk;
                            ++handle->FreeChunks;
                        }

                        --handle->Chunks;
                    }
                }

                handle->ReadOffset = 0;
                handle->WriteOffset = 0;
                handle->Length = 0;
            }
            else
            {
                if (length == 0)
                    return 0;
                var size = handle->Size;
                var byteCount = size - handle->ReadOffset;
                if (byteCount > length)
                {
                    handle->ReadOffset += length;
                }
                else
                {
                    NativeMemoryChunk* chunk;
                    var count = length - byteCount;
                    var chunks = ((count + size - 1) / size) | 0;
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = handle->Head;
                        handle->Head = chunk->Next;
                        if (handle->FreeChunks == handle->MaxFreeChunks)
                        {
                            NativeMemoryAllocator.Free(chunk);
                        }
                        else
                        {
                            chunk->Next = handle->FreeList;
                            handle->FreeList = chunk;
                            ++handle->FreeChunks;
                        }

                        --handle->Chunks;
                    }

                    handle->ReadOffset = count % size;
                }

                handle->Length -= length;
            }

            return length;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (length == 0)
                return;
            var handle = _handle;
            var size = handle->Size;
            var byteCount = size - handle->WriteOffset;
            if (byteCount >= length)
            {
                handle->WriteOffset += length;
            }
            else
            {
                NativeMemoryChunk* chunk;
                var count = length - byteCount;
                var chunks = ((count + size - 1) / size) | 0;
                for (var i = 0; i < chunks; ++i)
                {
                    if (handle->FreeChunks == 0)
                    {
                        chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                    }
                    else
                    {
                        chunk = handle->FreeList;
                        handle->FreeList = chunk->Next;
                        --handle->FreeChunks;
                    }

                    handle->Tail->Next = chunk;
                    handle->Tail = chunk;
                    ++handle->Chunks;
                }

                handle->WriteOffset = count % size;
            }

            handle->Length += length;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = _handle;
            if (length >= handle->Length)
            {
                length = handle->Length;
                if (length == 0)
                    return 0;
                var size = handle->Size;
                var byteCount = size - handle->ReadOffset;
                if (byteCount > length)
                {
                    Unsafe.CopyBlockUnaligned(buffer, handle->Head->Array + handle->ReadOffset, (uint)length);
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(buffer, handle->Head->Array + handle->ReadOffset, (uint)byteCount);
                    if (byteCount != length)
                    {
                        NativeMemoryChunk* chunk;
                        var count = length - byteCount;
                        var chunks = count / size;
                        var remaining = count % size;
                        for (var i = 0; i < chunks; ++i)
                        {
                            chunk = handle->Head;
                            handle->Head = chunk->Next;
                            if (handle->FreeChunks == handle->MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = handle->FreeList;
                                handle->FreeList = chunk;
                                ++handle->FreeChunks;
                            }

                            --handle->Chunks;
                            Unsafe.CopyBlockUnaligned(buffer + byteCount, handle->Head->Array, (uint)size);
                            byteCount += size;
                        }

                        if (remaining != 0)
                        {
                            chunk = handle->Head;
                            handle->Head = chunk->Next;
                            if (handle->FreeChunks == handle->MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = handle->FreeList;
                                handle->FreeList = chunk;
                                ++handle->FreeChunks;
                            }

                            --handle->Chunks;
                            Unsafe.CopyBlockUnaligned(buffer + byteCount, handle->Head->Array, (uint)remaining);
                        }
                    }
                }

                handle->ReadOffset = 0;
                handle->WriteOffset = 0;
                handle->Length = 0;
            }
            else
            {
                if (length == 0)
                    return 0;
                var size = handle->Size;
                var byteCount = size - handle->ReadOffset;
                if (byteCount > length)
                {
                    Unsafe.CopyBlockUnaligned(buffer, handle->Head->Array + handle->ReadOffset, (uint)length);
                    handle->ReadOffset += length;
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(buffer, handle->Head->Array + handle->ReadOffset, (uint)byteCount);
                    NativeMemoryChunk* chunk;
                    var count = length - byteCount;
                    var chunks = count / size;
                    var remaining = count % size;
                    for (var i = 0; i < chunks; ++i)
                    {
                        chunk = handle->Head;
                        handle->Head = chunk->Next;
                        if (handle->FreeChunks == handle->MaxFreeChunks)
                        {
                            NativeMemoryAllocator.Free(chunk);
                        }
                        else
                        {
                            chunk->Next = handle->FreeList;
                            handle->FreeList = chunk;
                            ++handle->FreeChunks;
                        }

                        --handle->Chunks;
                        Unsafe.CopyBlockUnaligned(buffer + byteCount, handle->Head->Array, (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        chunk = handle->Head;
                        handle->Head = chunk->Next;
                        if (handle->FreeChunks == handle->MaxFreeChunks)
                        {
                            NativeMemoryAllocator.Free(chunk);
                        }
                        else
                        {
                            chunk->Next = handle->FreeList;
                            handle->FreeList = chunk;
                            ++handle->FreeChunks;
                        }

                        --handle->Chunks;
                        Unsafe.CopyBlockUnaligned(buffer + byteCount, handle->Head->Array, (uint)remaining);
                    }

                    handle->ReadOffset = remaining;
                }

                handle->Length -= length;
            }

            return length;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (length == 0)
                return;
            var handle = _handle;
            var size = handle->Size;
            var byteCount = size - handle->WriteOffset;
            if (byteCount >= length)
            {
                Unsafe.CopyBlockUnaligned(handle->Tail->Array + handle->WriteOffset, buffer, (uint)length);
                handle->WriteOffset += length;
            }
            else
            {
                if (byteCount != 0)
                    Unsafe.CopyBlockUnaligned(handle->Tail->Array + handle->WriteOffset, buffer, (uint)byteCount);
                NativeMemoryChunk* chunk;
                var count = length - byteCount;
                var chunks = count / size;
                var remaining = count % size;
                for (var i = 0; i < chunks; ++i)
                {
                    if (handle->FreeChunks == 0)
                    {
                        chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                    }
                    else
                    {
                        chunk = handle->FreeList;
                        handle->FreeList = chunk->Next;
                        --handle->FreeChunks;
                    }

                    handle->Tail->Next = chunk;
                    handle->Tail = chunk;
                    ++handle->Chunks;
                    Unsafe.CopyBlockUnaligned(handle->Tail->Array, buffer + byteCount, (uint)size);
                    byteCount += size;
                }

                if (remaining != 0)
                {
                    if (handle->FreeChunks == 0)
                    {
                        chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                    }
                    else
                    {
                        chunk = handle->FreeList;
                        handle->FreeList = chunk->Next;
                        --handle->FreeChunks;
                    }

                    handle->Tail->Next = chunk;
                    handle->Tail = chunk;
                    ++handle->Chunks;
                    Unsafe.CopyBlockUnaligned(handle->Tail->Array, buffer + byteCount, (uint)remaining);
                }

                handle->WriteOffset = remaining;
            }

            handle->Length += length;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => Read((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), buffer.Length);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer) => Write((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), buffer.Length);

#if NET7_0_OR_GREATER
        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(ref byte buffer, int length) => Read((byte*)Unsafe.AsPointer(ref buffer), length);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ref byte buffer, int length) => Write((byte*)Unsafe.AsPointer(ref buffer), length);
#endif

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
            var handle = _handle;
            if (capacity > handle->MaxFreeChunks)
                capacity = handle->MaxFreeChunks;
            while (handle->FreeChunks < capacity)
            {
                handle->FreeChunks++;
                var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + handle->Size));
                chunk->Next = handle->FreeList;
                handle->FreeList = chunk;
            }

            return handle->FreeChunks;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var handle = _handle;
            var node = handle->FreeList;
            while (handle->FreeChunks > 0)
            {
                handle->FreeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            handle->FreeList = node;
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
            var handle = _handle;
            var node = handle->FreeList;
            while (handle->FreeChunks > capacity)
            {
                handle->FreeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            handle->FreeList = node;
            return handle->FreeChunks;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeChunkedStream Empty => new();
    }
}