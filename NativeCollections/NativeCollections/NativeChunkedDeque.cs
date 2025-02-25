﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native chunked deque
    ///     (Slower than Deque, disable Enumerator)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeChunkedDeque<T> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeChunkedDequeHandle
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
            ///     Count
            /// </summary>
            public int Count;
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
            ///     Previous
            /// </summary>
            public NativeMemoryChunk* Previous;

            /// <summary>
            ///     Array
            /// </summary>
            public nint Array;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeChunkedDequeHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeChunkedDeque(int size, int maxFreeChunks)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (maxFreeChunks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeChunks), maxFreeChunks, "MustBeNonNegative");
            var handle = (NativeChunkedDequeHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeChunkedDequeHandle));
            var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size * sizeof(T)));
            handle->Head = chunk;
            handle->Tail = chunk;
            handle->FreeList = null;
            handle->Chunks = 1;
            handle->FreeChunks = 0;
            handle->MaxFreeChunks = maxFreeChunks;
            handle->Size = size;
            handle->ReadOffset = 0;
            handle->WriteOffset = 0;
            handle->Count = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Count == 0;

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
        ///     Count
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeChunkedDeque<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeChunkedDeque<T> nativeChunkedDeque && nativeChunkedDeque == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeChunkedDeque<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeChunkedDeque<T> left, NativeChunkedDeque<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeChunkedDeque<T> left, NativeChunkedDeque<T> right) => left._handle != right._handle;

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
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            var handle = _handle;
            if (handle->Chunks != 1)
            {
                handle->FreeChunks += handle->Chunks - 1;
                handle->Chunks = 1;
                var chunk = handle->Head->Next;
                chunk->Next = handle->FreeList;
                handle->FreeList = chunk;
                TrimExcess(handle->MaxFreeChunks);
                handle->Tail = handle->Head;
            }

            handle->ReadOffset = 0;
            handle->WriteOffset = 0;
            handle->Count = 0;
        }

        /// <summary>
        ///     Enqueue head
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueHead(in T item)
        {
            var handle = _handle;
            if (handle->ReadOffset == 0)
            {
                handle->ReadOffset = handle->Size;
                if (handle->Count != 0)
                {
                    NativeMemoryChunk* chunk;
                    if (handle->FreeChunks == 0)
                    {
                        chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + handle->Size * sizeof(T)));
                    }
                    else
                    {
                        chunk = handle->FreeList;
                        handle->FreeList = chunk->Next;
                        --handle->FreeChunks;
                    }

                    chunk->Next = handle->Head;
                    handle->Head->Previous = chunk;
                    handle->Head = chunk;
                    ++handle->Chunks;
                }
                else
                {
                    handle->WriteOffset = handle->Size;
                }
            }

            ++handle->Count;
            ((T*)&handle->Head->Array)[--handle->ReadOffset] = item;
        }

        /// <summary>
        ///     Enqueue tail
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnqueueTail(in T item)
        {
            var handle = _handle;
            if (handle->WriteOffset == handle->Size)
            {
                handle->WriteOffset = 0;
                NativeMemoryChunk* chunk;
                if (handle->FreeChunks == 0)
                {
                    chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + handle->Size * sizeof(T)));
                }
                else
                {
                    chunk = handle->FreeList;
                    handle->FreeList = chunk->Next;
                    --handle->FreeChunks;
                }

                chunk->Previous = handle->Tail;
                handle->Tail->Next = chunk;
                handle->Tail = chunk;
                ++handle->Chunks;
            }

            ++handle->Count;
            ((T*)&handle->Tail->Array)[handle->WriteOffset++] = item;
        }

        /// <summary>
        ///     Try dequeue
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Dequeued</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeueHead(out T result)
        {
            var handle = _handle;
            if (handle->Count == 0)
            {
                result = default;
                return false;
            }

            --handle->Count;
            result = ((T*)&handle->Head->Array)[handle->ReadOffset++];
            if (handle->ReadOffset == handle->Size)
            {
                handle->ReadOffset = 0;
                if (handle->Chunks != 1)
                {
                    var chunk = handle->Head;
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
            var handle = _handle;
            if (handle->Count == 0)
            {
                result = default;
                return false;
            }

            --handle->Count;
            result = ((T*)&handle->Tail->Array)[--handle->WriteOffset];
            if (handle->WriteOffset == 0 && handle->Chunks != 1)
            {
                handle->WriteOffset = handle->Size;
                var chunk = handle->Tail;
                handle->Tail = chunk->Previous;
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
            var handle = _handle;
            if (handle->Size == 0)
            {
                result = default;
                return false;
            }

            result = ((T*)&handle->Head->Array)[handle->ReadOffset];
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
            var handle = _handle;
            if (handle->Size == 0)
            {
                result = default;
                return false;
            }

            result = ((T*)&handle->Tail->Array)[handle->WriteOffset - 1];
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
        public static NativeChunkedDeque<T> Empty => new();
    }
}