using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrent chunked stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeConcurrentChunkedStream : IDisposable, IEquatable<NativeConcurrentChunkedStream>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeConcurrentChunkedStreamHandle
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
            ///     Spin lock
            /// </summary>
            public fixed byte SpinLock[8];
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
        private readonly NativeConcurrentChunkedStreamHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentChunkedStream(int size, int maxFreeChunks)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (maxFreeChunks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeChunks), maxFreeChunks, "MustBeNonNegative");
            var handle = (NativeConcurrentChunkedStreamHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeConcurrentChunkedStreamHandle));
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
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Reset();
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
        public bool Equals(NativeConcurrentChunkedStream other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentChunkedStream nativeConcurrentChunkedStream && nativeConcurrentChunkedStream == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeConcurrentChunkedStream";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => left._handle != right._handle;

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
        public int Read(byte* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = _handle;
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Enter();
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

            spinLock.Exit();
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
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Enter();
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
            spinLock.Exit();
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
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Enter();
            while (handle->FreeChunks < capacity)
            {
                handle->FreeChunks++;
                var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + handle->Size));
                chunk->Next = handle->FreeList;
                handle->FreeList = chunk;
            }

            var freeChunks = handle->FreeChunks;
            spinLock.Exit();
            return freeChunks;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var handle = _handle;
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Enter();
            var node = handle->FreeList;
            while (handle->FreeChunks > 0)
            {
                handle->FreeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            handle->FreeList = node;
            spinLock.Exit();
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
            NativeConcurrentSpinLock spinLock = handle->SpinLock;
            spinLock.Enter();
            var node = handle->FreeList;
            while (handle->FreeChunks > capacity)
            {
                handle->FreeChunks--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            handle->FreeList = node;
            var freeChunks = handle->FreeChunks;
            spinLock.Exit();
            return freeChunks;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentChunkedStream Empty => new();
    }
}