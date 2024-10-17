using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeMemoryPool : IDisposable, IEquatable<NativeMemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryPoolHandle
        {
            /// <summary>
            ///     Sentinel
            /// </summary>
            public NativeMemorySlab* Sentinel;

            /// <summary>
            ///     Free list
            /// </summary>
            public NativeMemorySlab* FreeList;

            /// <summary>
            ///     Slabs
            /// </summary>
            public int Slabs;

            /// <summary>
            ///     Free slabs
            /// </summary>
            public int FreeSlabs;

            /// <summary>
            ///     Max free slabs
            /// </summary>
            public int MaxFreeSlabs;

            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Slab
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemorySlab
        {
            /// <summary>
            ///     Next
            /// </summary>
            public NativeMemorySlab* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public NativeMemorySlab* Previous;

            /// <summary>
            ///     Sentinel
            /// </summary>
            public NativeMemoryNode* Sentinel;

            /// <summary>
            ///     Count
            /// </summary>
            public int Count;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct NativeMemoryNode
        {
            /// <summary>
            ///     Slab
            /// </summary>
            [FieldOffset(0)] public NativeMemorySlab* Slab;

            /// <summary>
            ///     Next
            /// </summary>
            [FieldOffset(0)] public NativeMemoryNode* Next;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeMemoryPoolHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryPool(int size, int length, int maxFreeSlabs)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            var nodeSize = sizeof(NativeMemoryNode) + length;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size * nodeSize));
            var slab = (NativeMemorySlab*)array;
            slab->Next = slab;
            slab->Previous = slab;
            array += sizeof(NativeMemorySlab);
            NativeMemoryNode* next = null;
            for (var i = size - 1; i >= 0; --i)
            {
                var node = (NativeMemoryNode*)(array + i * nodeSize);
                node->Next = next;
                next = node;
            }

            slab->Sentinel = next;
            slab->Count = size;
            var handle = (NativeMemoryPoolHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeMemoryPoolHandle));
            handle->Sentinel = slab;
            handle->FreeList = null;
            handle->Slabs = 1;
            handle->FreeSlabs = 0;
            handle->MaxFreeSlabs = maxFreeSlabs;
            handle->Size = size;
            handle->Length = length;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Slabs
        /// </summary>
        public int Slabs => _handle->Slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        public int FreeSlabs => _handle->FreeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        public int MaxFreeSlabs => _handle->MaxFreeSlabs;

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
        public bool Equals(NativeMemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemoryPool nativeMemoryPool && nativeMemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryPool left, NativeMemoryPool right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryPool left, NativeMemoryPool right) => left._handle != right._handle;

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
            while (handle->Slabs > 0)
            {
                handle->Slabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            node = handle->FreeList;
            while (handle->FreeSlabs > 0)
            {
                handle->FreeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            var handle = _handle;
            NativeMemoryNode* node;
            var slab = handle->Sentinel;
            if (slab->Count == 0)
            {
                handle->Sentinel = slab->Next;
                slab = handle->Sentinel;
                if (slab->Count == 0)
                {
                    var size = handle->Size;
                    if (handle->FreeSlabs == 0)
                    {
                        var nodeSize = sizeof(NativeMemoryNode) + handle->Length;
                        var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size * nodeSize));
                        slab = (NativeMemorySlab*)array;
                        array += sizeof(NativeMemorySlab);
                        NativeMemoryNode* next = null;
                        for (var i = size - 1; i >= 0; --i)
                        {
                            node = (NativeMemoryNode*)(array + i * nodeSize);
                            node->Next = next;
                            next = node;
                        }

                        slab->Sentinel = next;
                    }
                    else
                    {
                        slab = handle->FreeList;
                        handle->FreeList = slab->Next;
                        handle->FreeSlabs--;
                    }

                    slab->Next = handle->Sentinel;
                    slab->Previous = handle->Sentinel->Previous;
                    slab->Count = size;
                    handle->Sentinel->Previous->Next = slab;
                    handle->Sentinel->Previous = slab;
                    handle->Sentinel = slab;
                    handle->Slabs++;
                }
            }

            node = slab->Sentinel;
            slab->Sentinel = node->Next;
            node->Slab = slab;
            slab->Count--;
            return (byte*)node + sizeof(NativeMemoryNode);
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            var handle = _handle;
            var node = (NativeMemoryNode*)((byte*)ptr - sizeof(NativeMemoryNode));
            var slab = node->Slab;
            slab->Count++;
            if (slab->Count == handle->Size && slab != handle->Sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (handle->FreeSlabs == handle->MaxFreeSlabs)
                {
                    NativeMemoryAllocator.Free(slab);
                }
                else
                {
                    node->Next = slab->Sentinel;
                    slab->Sentinel = node;
                    slab->Next = handle->FreeList;
                    handle->FreeList = slab;
                    handle->FreeSlabs++;
                }

                handle->Slabs--;
                return;
            }

            node->Next = slab->Sentinel;
            slab->Sentinel = node;
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
            if (capacity > handle->MaxFreeSlabs)
                capacity = handle->MaxFreeSlabs;
            var size = handle->Size;
            var nodeSize = sizeof(NativeMemoryNode) + handle->Length;
            while (handle->FreeSlabs < capacity)
            {
                handle->FreeSlabs++;
                var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size * nodeSize));
                var slab = (NativeMemorySlab*)array;
                array += sizeof(NativeMemorySlab);
                NativeMemoryNode* next = null;
                for (var i = size - 1; i >= 0; --i)
                {
                    var node = (NativeMemoryNode*)(array + i * nodeSize);
                    node->Next = next;
                    next = node;
                }

                slab->Sentinel = next;
                slab->Next = handle->FreeList;
                handle->FreeList = slab;
            }

            return handle->FreeSlabs;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var handle = _handle;
            var node = handle->FreeList;
            while (handle->FreeSlabs > 0)
            {
                handle->FreeSlabs--;
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
            while (handle->FreeSlabs > capacity)
            {
                handle->FreeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            handle->FreeList = node;
            return handle->FreeSlabs;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryPool Empty => new();
    }
}