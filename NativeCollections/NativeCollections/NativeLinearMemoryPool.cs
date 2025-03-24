using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native linear memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe struct NativeLinearMemoryPool : IDisposable, IEquatable<NativeLinearMemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeLinearMemoryPoolHandle
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
            ///     Nodes
            /// </summary>
            public int Nodes;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;
        }

        /// <summary>
        ///     Node
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryNode
        {
            /// <summary>
            ///     Slab
            /// </summary>
            public NativeMemorySlab* Slab;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeLinearMemoryPoolHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="maxLength">Max length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeLinearMemoryPool(int maxLength, int maxFreeSlabs)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "MustBePositive");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            var size = sizeof(NativeMemoryNode) + maxLength;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size));
            var slab = (NativeMemorySlab*)array;
            slab->Next = slab;
            slab->Previous = slab;
            slab->Nodes = 0;
            slab->Length = 0;
            var handle = (NativeLinearMemoryPoolHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeLinearMemoryPoolHandle));
            handle->Sentinel = slab;
            handle->FreeList = null;
            handle->Slabs = 1;
            handle->FreeSlabs = 0;
            handle->MaxFreeSlabs = maxFreeSlabs;
            handle->Size = size;
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
        ///     Max length
        /// </summary>
        public int MaxLength => _handle->Size - sizeof(NativeMemoryNode);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeLinearMemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeLinearMemoryPool nativeLinearMemoryPool && nativeLinearMemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeLinearMemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeLinearMemoryPool left, NativeLinearMemoryPool right) => left._handle != right._handle;

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
        /// <param name="length">Length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            var handle = _handle;
            var size = handle->Size;
            if (length + sizeof(NativeMemoryNode) > size)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeLessOrEqual");
            var slab = handle->Sentinel;
            if (slab->Length + sizeof(NativeMemoryNode) + length > size)
            {
                if (handle->FreeSlabs == 0)
                {
                    var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size));
                    slab = (NativeMemorySlab*)array;
                }
                else
                {
                    slab = handle->FreeList;
                    handle->FreeList = slab->Next;
                    handle->FreeSlabs--;
                }

                slab->Next = handle->Sentinel;
                slab->Previous = handle->Sentinel->Previous;
                slab->Nodes = 0;
                slab->Length = 0;
                handle->Sentinel->Previous->Next = slab;
                handle->Sentinel->Previous = slab;
                handle->Sentinel = slab;
                handle->Slabs++;
            }

            var node = (NativeMemoryNode*)((byte*)slab + sizeof(NativeMemorySlab) + slab->Length);
            node->Slab = slab;
            slab->Nodes++;
            slab->Length += sizeof(NativeMemoryNode) + length;
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
            slab->Nodes--;
            if (slab->Nodes == 0 && slab != handle->Sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (handle->FreeSlabs == handle->MaxFreeSlabs)
                {
                    NativeMemoryAllocator.Free(slab);
                }
                else
                {
                    slab->Next = handle->FreeList;
                    handle->FreeList = slab;
                    handle->FreeSlabs++;
                }

                handle->Slabs--;
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
            var handle = _handle;
            if (capacity > handle->MaxFreeSlabs)
                capacity = handle->MaxFreeSlabs;
            var size = handle->Size;
            while (handle->FreeSlabs < capacity)
            {
                handle->FreeSlabs++;
                var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeMemorySlab) + size));
                var slab = (NativeMemorySlab*)array;
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
        public static NativeLinearMemoryPool Empty => new();
    }
}