using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native chunked stack
    ///     (Slower than Stack, disable Enumerator)
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe struct NativeChunkedStack<T> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeChunkedStackHandle
        {
            /// <summary>
            ///     Sentinel
            /// </summary>
            public NativeMemoryChunk* Sentinel;

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
            ///     Count
            /// </summary>
            public int Count;

            /// <summary>
            ///     Clear
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Clear()
            {
                if (Chunks != 1)
                {
                    FreeChunks += Chunks - 1;
                    Chunks = 1;
                    var chunk = Sentinel->Next;
                    chunk->Next = FreeList;
                    FreeList = chunk;
                    TrimExcess(MaxFreeChunks);
                }

                Count = 0;
            }

            /// <summary>
            ///     Push
            /// </summary>
            /// <param name="item">Item</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(in T item)
            {
                var index = Count % Size;
                if (index == 0 && Count != 0)
                {
                    NativeMemoryChunk* chunk;
                    if (FreeChunks == 0)
                    {
                        chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + Size * sizeof(T)));
                    }
                    else
                    {
                        chunk = FreeList;
                        FreeList = chunk->Next;
                        --FreeChunks;
                    }

                    chunk->Next = Sentinel;
                    Sentinel = chunk;
                    ++Chunks;
                }

                ++Count;
                ((T*)&Sentinel->Array)[index] = item;
            }

            /// <summary>
            ///     Try pop
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Popped</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPop(out T result)
            {
                if (Count == 0)
                {
                    result = default;
                    return false;
                }

                --Count;
                var index = Count % Size;
                result = ((T*)&Sentinel->Array)[index];
                if (index == 0 && Chunks != 1)
                {
                    var chunk = Sentinel;
                    Sentinel = chunk->Next;
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
                }

                return true;
            }

            /// <summary>
            ///     Try peek
            /// </summary>
            /// <param name="result">Item</param>
            /// <returns>Peeked</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryPeek(out T result)
            {
                if (Size == 0)
                {
                    result = default;
                    return false;
                }

                var index = (Count - 1) % Size;
                result = ((T*)&Sentinel->Array)[index];
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
            public nint Array;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeChunkedStackHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxFreeChunks">Max free chunks</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeChunkedStack(int size, int maxFreeChunks)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (maxFreeChunks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeChunks), maxFreeChunks, "MustBeNonNegative");
            var handle = (NativeChunkedStackHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeChunkedStackHandle));
            var chunk = (NativeMemoryChunk*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemoryChunk) + size * sizeof(T)));
            handle->Sentinel = chunk;
            handle->FreeList = null;
            handle->Chunks = 1;
            handle->FreeChunks = 0;
            handle->MaxFreeChunks = maxFreeChunks;
            handle->Size = size;
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
        public bool Equals(NativeChunkedStack<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeChunkedStack<T> nativeChunkedStack && nativeChunkedStack == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeChunkedStack<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeChunkedStack<T> left, NativeChunkedStack<T> right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeChunkedStack<T> left, NativeChunkedStack<T> right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            var node = handle->Sentinel;
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
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Push
        /// </summary>
        /// <param name="item">Item</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T item) => _handle->Push(item);

        /// <summary>
        ///     Try pop
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Popped</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result) => _handle->TryPop(out result);

        /// <summary>
        ///     Try peek
        /// </summary>
        /// <param name="result">Item</param>
        /// <returns>Peeked</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result) => _handle->TryPeek(out result);

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
        public static NativeChunkedStack<T> Empty => new();
    }
}