﻿using System;
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
    [NativeCollection(NativeCollectionType.None)]
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
                if (length >= Length)
                {
                    length = Length;
                    if (length == 0)
                        return 0;
                    var size = Size;
                    var byteCount = size - ReadOffset;
                    if (byteCount > length)
                    {
                        Unsafe.CopyBlockUnaligned(buffer, Head->Array + ReadOffset, (uint)length);
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(buffer, Head->Array + ReadOffset, (uint)byteCount);
                        if (byteCount != length)
                        {
                            NativeMemoryChunk* chunk;
                            var count = length - byteCount;
                            var chunks = count / size;
                            var remaining = count % size;
                            for (var i = 0; i < chunks; ++i)
                            {
                                chunk = Head;
                                Head = chunk->Next;
                                if (FreeChunks == MaxFreeChunks)
                                {
                                    NativeMemoryAllocator.Free(chunk);
                                }
                                else
                                {
                                    chunk->Next = FreeList;
                                    FreeList = chunk;
                                    ++FreeChunks;
                                }

                                --Chunks;
                                Unsafe.CopyBlockUnaligned(buffer + byteCount, Head->Array, (uint)size);
                                byteCount += size;
                            }

                            if (remaining != 0)
                            {
                                chunk = Head;
                                Head = chunk->Next;
                                if (FreeChunks == MaxFreeChunks)
                                {
                                    NativeMemoryAllocator.Free(chunk);
                                }
                                else
                                {
                                    chunk->Next = FreeList;
                                    FreeList = chunk;
                                    ++FreeChunks;
                                }

                                --Chunks;
                                Unsafe.CopyBlockUnaligned(buffer + byteCount, Head->Array, (uint)remaining);
                            }
                        }
                    }

                    ReadOffset = 0;
                    WriteOffset = 0;
                    Length = 0;
                }
                else
                {
                    if (length == 0)
                        return 0;
                    var size = Size;
                    var byteCount = size - ReadOffset;
                    if (byteCount > length)
                    {
                        Unsafe.CopyBlockUnaligned(buffer, Head->Array + ReadOffset, (uint)length);
                        ReadOffset += length;
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(buffer, Head->Array + ReadOffset, (uint)byteCount);
                        NativeMemoryChunk* chunk;
                        var count = length - byteCount;
                        var chunks = count / size;
                        var remaining = count % size;
                        for (var i = 0; i < chunks; ++i)
                        {
                            chunk = Head;
                            Head = chunk->Next;
                            if (FreeChunks == MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = FreeList;
                                FreeList = chunk;
                                ++FreeChunks;
                            }

                            --Chunks;
                            Unsafe.CopyBlockUnaligned(buffer + byteCount, Head->Array, (uint)size);
                            byteCount += size;
                        }

                        if (remaining != 0)
                        {
                            chunk = Head;
                            Head = chunk->Next;
                            if (FreeChunks == MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = FreeList;
                                FreeList = chunk;
                                ++FreeChunks;
                            }

                            --Chunks;
                            Unsafe.CopyBlockUnaligned(buffer + byteCount, Head->Array, (uint)remaining);
                        }

                        ReadOffset = remaining;
                    }

                    Length -= length;
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
                var size = Size;
                var byteCount = size - WriteOffset;
                if (byteCount >= length)
                {
                    Unsafe.CopyBlockUnaligned(Tail->Array + WriteOffset, buffer, (uint)length);
                    WriteOffset += length;
                }
                else
                {
                    if (byteCount != 0)
                        Unsafe.CopyBlockUnaligned(Tail->Array + WriteOffset, buffer, (uint)byteCount);
                    NativeMemoryChunk* chunk;
                    var count = length - byteCount;
                    var chunks = count / size;
                    var remaining = count % size;
                    for (var i = 0; i < chunks; ++i)
                    {
                        if (FreeChunks == 0)
                        {
                            chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                        }
                        else
                        {
                            chunk = FreeList;
                            FreeList = chunk->Next;
                            --FreeChunks;
                        }

                        Tail->Next = chunk;
                        Tail = chunk;
                        ++Chunks;
                        Unsafe.CopyBlockUnaligned(Tail->Array, buffer + byteCount, (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        if (FreeChunks == 0)
                        {
                            chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                        }
                        else
                        {
                            chunk = FreeList;
                            FreeList = chunk->Next;
                            --FreeChunks;
                        }

                        Tail->Next = chunk;
                        Tail = chunk;
                        ++Chunks;
                        Unsafe.CopyBlockUnaligned(Tail->Array, buffer + byteCount, (uint)remaining);
                    }

                    WriteOffset = remaining;
                }

                Length += length;
            }

            /// <summary>
            ///     Read
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <returns>Bytes</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Read(Span<byte> buffer)
            {
                var length = buffer.Length;
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                if (length >= Length)
                {
                    length = Length;
                    if (length == 0)
                        return 0;
                    var size = Size;
                    var byteCount = size - ReadOffset;
                    if (byteCount > length)
                    {
                        Unsafe.CopyBlockUnaligned(ref reference, ref *(Head->Array + ReadOffset), (uint)length);
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(ref reference, ref *(Head->Array + ReadOffset), (uint)byteCount);
                        if (byteCount != length)
                        {
                            NativeMemoryChunk* chunk;
                            var count = length - byteCount;
                            var chunks = count / size;
                            var remaining = count % size;
                            for (var i = 0; i < chunks; ++i)
                            {
                                chunk = Head;
                                Head = chunk->Next;
                                if (FreeChunks == MaxFreeChunks)
                                {
                                    NativeMemoryAllocator.Free(chunk);
                                }
                                else
                                {
                                    chunk->Next = FreeList;
                                    FreeList = chunk;
                                    ++FreeChunks;
                                }

                                --Chunks;
                                Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref *Head->Array, (uint)size);
                                byteCount += size;
                            }

                            if (remaining != 0)
                            {
                                chunk = Head;
                                Head = chunk->Next;
                                if (FreeChunks == MaxFreeChunks)
                                {
                                    NativeMemoryAllocator.Free(chunk);
                                }
                                else
                                {
                                    chunk->Next = FreeList;
                                    FreeList = chunk;
                                    ++FreeChunks;
                                }

                                --Chunks;
                                Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref *Head->Array, (uint)remaining);
                            }
                        }
                    }

                    ReadOffset = 0;
                    WriteOffset = 0;
                    Length = 0;
                }
                else
                {
                    if (length == 0)
                        return 0;
                    var size = Size;
                    var byteCount = size - ReadOffset;
                    if (byteCount > length)
                    {
                        Unsafe.CopyBlockUnaligned(ref reference, ref *(Head->Array + ReadOffset), (uint)length);
                        ReadOffset += length;
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(ref reference, ref *(Head->Array + ReadOffset), (uint)byteCount);
                        NativeMemoryChunk* chunk;
                        var count = length - byteCount;
                        var chunks = count / size;
                        var remaining = count % size;
                        for (var i = 0; i < chunks; ++i)
                        {
                            chunk = Head;
                            Head = chunk->Next;
                            if (FreeChunks == MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = FreeList;
                                FreeList = chunk;
                                ++FreeChunks;
                            }

                            --Chunks;
                            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref *Head->Array, (uint)size);
                            byteCount += size;
                        }

                        if (remaining != 0)
                        {
                            chunk = Head;
                            Head = chunk->Next;
                            if (FreeChunks == MaxFreeChunks)
                            {
                                NativeMemoryAllocator.Free(chunk);
                            }
                            else
                            {
                                chunk->Next = FreeList;
                                FreeList = chunk;
                                ++FreeChunks;
                            }

                            --Chunks;
                            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref reference, byteCount), ref *Head->Array, (uint)remaining);
                        }

                        ReadOffset = remaining;
                    }

                    Length -= length;
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
                if (length < 0)
                    throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
                if (length == 0)
                    return;
                ref var reference = ref MemoryMarshal.GetReference(buffer);
                var size = Size;
                var byteCount = size - WriteOffset;
                if (byteCount >= length)
                {
                    Unsafe.CopyBlockUnaligned(ref *(Tail->Array + WriteOffset), ref reference, (uint)length);
                    WriteOffset += length;
                }
                else
                {
                    if (byteCount != 0)
                        Unsafe.CopyBlockUnaligned(ref *(Tail->Array + WriteOffset), ref reference, (uint)byteCount);
                    NativeMemoryChunk* chunk;
                    var count = length - byteCount;
                    var chunks = count / size;
                    var remaining = count % size;
                    for (var i = 0; i < chunks; ++i)
                    {
                        if (FreeChunks == 0)
                        {
                            chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                        }
                        else
                        {
                            chunk = FreeList;
                            FreeList = chunk->Next;
                            --FreeChunks;
                        }

                        Tail->Next = chunk;
                        Tail = chunk;
                        ++Chunks;
                        Unsafe.CopyBlockUnaligned(ref *Tail->Array, ref Unsafe.AddByteOffset(ref reference, byteCount), (uint)size);
                        byteCount += size;
                    }

                    if (remaining != 0)
                    {
                        if (FreeChunks == 0)
                        {
                            chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size));
                        }
                        else
                        {
                            chunk = FreeList;
                            FreeList = chunk->Next;
                            --FreeChunks;
                        }

                        Tail->Next = chunk;
                        Tail = chunk;
                        ++Chunks;
                        Unsafe.CopyBlockUnaligned(ref *Tail->Array, ref Unsafe.AddByteOffset(ref reference, byteCount), (uint)remaining);
                    }

                    WriteOffset = remaining;
                }

                Length += length;
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
                if (capacity > MaxFreeChunks)
                    capacity = MaxFreeChunks;
                while (FreeChunks < capacity)
                {
                    FreeChunks++;
                    var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + Size));
                    chunk->Next = FreeList;
                    FreeList = chunk;
                }

                return FreeChunks;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TrimExcess()
            {
                var node = FreeList;
                while (FreeChunks > 0)
                {
                    FreeChunks--;
                    var temp = node;
                    node = node->Next;
                    NativeMemoryAllocator.Free(temp);
                }

                FreeList = node;
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
                var node = FreeList;
                while (FreeChunks > capacity)
                {
                    FreeChunks--;
                    var temp = node;
                    node = node->Next;
                    NativeMemoryAllocator.Free(temp);
                }

                FreeList = node;
                return FreeChunks;
            }
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
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => _handle->Read(buffer, length);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length) => _handle->Write(buffer, length);

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer) => _handle->Write(buffer);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeChunkedStream Empty => new();
    }
}