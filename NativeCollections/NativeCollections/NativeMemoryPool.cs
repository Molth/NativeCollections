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
        [StructLayout(LayoutKind.Explicit, Size = 8)]
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
            _handle = (NativeMemoryPoolHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeMemoryPoolHandle));
            _handle->Sentinel = slab;
            _handle->FreeList = null;
            _handle->Slabs = 1;
            _handle->FreeSlabs = 0;
            _handle->MaxFreeSlabs = maxFreeSlabs;
            _handle->Size = size;
            _handle->Length = length;
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
        public override int GetHashCode() => (int)(nint)_handle;

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
            if (_handle == null)
                return;
            var node = _handle->Sentinel;
            while (_handle->Slabs > 0)
            {
                _handle->Slabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            node = _handle->FreeList;
            while (_handle->FreeSlabs > 0)
            {
                _handle->FreeSlabs--;
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
            NativeMemoryNode* node;
            var slab = _handle->Sentinel;
            if (slab->Count == 0)
            {
                _handle->Sentinel = slab->Next;
                slab = _handle->Sentinel;
                if (slab->Count == 0)
                {
                    var size = _handle->Size;
                    if (_handle->FreeSlabs == 0)
                    {
                        var nodeSize = sizeof(NativeMemoryNode) + _handle->Length;
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
                        slab = _handle->FreeList;
                        _handle->FreeList = slab->Next;
                        _handle->FreeSlabs--;
                    }

                    slab->Next = _handle->Sentinel;
                    slab->Previous = _handle->Sentinel->Previous;
                    slab->Count = size;
                    _handle->Sentinel->Previous->Next = slab;
                    _handle->Sentinel->Previous = slab;
                    _handle->Sentinel = slab;
                    _handle->Slabs++;
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
            var node = (NativeMemoryNode*)((byte*)ptr - sizeof(NativeMemoryNode));
            var slab = node->Slab;
            slab->Count++;
            if (slab->Count == _handle->Size && slab != _handle->Sentinel)
            {
                slab->Previous->Next = slab->Next;
                slab->Next->Previous = slab->Previous;
                if (_handle->FreeSlabs == _handle->MaxFreeSlabs)
                {
                    NativeMemoryAllocator.Free(slab);
                }
                else
                {
                    node->Next = slab->Sentinel;
                    slab->Sentinel = node;
                    slab->Next = _handle->FreeList;
                    _handle->FreeList = slab;
                    _handle->FreeSlabs++;
                }

                _handle->Slabs--;
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
            if (capacity > _handle->MaxFreeSlabs)
                capacity = _handle->MaxFreeSlabs;
            var size = _handle->Size;
            var nodeSize = sizeof(NativeMemoryNode) + _handle->Length;
            while (_handle->FreeSlabs < capacity)
            {
                _handle->FreeSlabs++;
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
                slab->Count = size;
                slab->Next = _handle->FreeList;
                _handle->FreeList = slab;
            }

            return _handle->FreeSlabs;
        }

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess()
        {
            var node = _handle->FreeList;
            while (_handle->FreeSlabs > 0)
            {
                _handle->FreeSlabs--;
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
            while (_handle->FreeSlabs > capacity)
            {
                _handle->FreeSlabs--;
                var temp = node;
                node = node->Next;
                NativeMemoryAllocator.Free(temp);
            }

            _handle->FreeList = node;
            return _handle->FreeSlabs;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryPool Empty => new();
    }
}