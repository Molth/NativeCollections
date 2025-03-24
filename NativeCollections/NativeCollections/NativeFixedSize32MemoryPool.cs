using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native fixed size 32 memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(NativeCollectionType.None)]
    public readonly unsafe struct NativeFixedSize32MemoryPool : IDisposable, IEquatable<NativeFixedSize32MemoryPool>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFixedSize32MemoryPoolHandle
        {
            /// <summary>
            ///     Sentinel
            /// </summary>
            public NativeFixedSize32MemorySlab* Sentinel;

            /// <summary>
            ///     Free list
            /// </summary>
            public NativeFixedSize32MemorySlab* FreeList;

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
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Rent buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void* Rent()
            {
                var slab = Sentinel;
                var nodeSize = 1 + Length;
                if (slab->Bitmap == uint.MaxValue)
                {
                    Sentinel = slab->Next;
                    slab = Sentinel;
                    if (slab->Bitmap == uint.MaxValue)
                    {
                        if (FreeSlabs == 0)
                        {
                            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemorySlab) + 32 * nodeSize));
                            slab = (NativeFixedSize32MemorySlab*)array;
                            array += sizeof(NativeFixedSize32MemorySlab);
                            for (byte i = 0; i < 32; ++i)
                                *(array + i * nodeSize) = i;
                        }
                        else
                        {
                            slab = FreeList;
                            FreeList = slab->Next;
                            FreeSlabs--;
                        }

                        slab->Next = Sentinel;
                        slab->Previous = Sentinel->Previous;
                        slab->Bitmap = 0U;
                        Sentinel->Previous->Next = slab;
                        Sentinel->Previous = slab;
                        Sentinel = slab;
                        Slabs++;
                    }
                }

                ref var segment = ref slab->Bitmap;
                var id = BitOperationsHelpers.TrailingZeroCount(~segment);
                segment |= 1U << id;
                return (byte*)slab + sizeof(NativeFixedSize32MemorySlab) + id * nodeSize + 1;
            }

            /// <summary>
            ///     Return buffer
            /// </summary>
            /// <param name="ptr">Pointer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Return(void* ptr)
            {
                var array = (byte*)ptr;
                var id = *(array - 1);
                array -= sizeof(NativeFixedSize32MemorySlab) + id * (1 + Length) + 1;
                var slab = (NativeFixedSize32MemorySlab*)array;
                ref var segment = ref slab->Bitmap;
                segment &= ~(1U << id);
                if (segment == 0 && slab != Sentinel)
                {
                    slab->Previous->Next = slab->Next;
                    slab->Next->Previous = slab->Previous;
                    if (FreeSlabs == MaxFreeSlabs)
                    {
                        NativeMemoryAllocator.Free(slab);
                    }
                    else
                    {
                        slab->Next = FreeList;
                        FreeList = slab;
                        FreeSlabs++;
                    }

                    Slabs--;
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
                if (capacity > MaxFreeSlabs)
                    capacity = MaxFreeSlabs;
                var nodeSize = 1 + Length;
                while (FreeSlabs < capacity)
                {
                    FreeSlabs++;
                    var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemorySlab) + 32 * nodeSize));
                    var slab = (NativeFixedSize32MemorySlab*)array;
                    array += sizeof(NativeFixedSize32MemorySlab);
                    for (byte i = 0; i < 32; ++i)
                        *(array + i * nodeSize) = i;
                    slab->Next = FreeList;
                    FreeList = slab;
                }

                return FreeSlabs;
            }

            /// <summary>
            ///     Trim excess
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TrimExcess()
            {
                var node = FreeList;
                while (FreeSlabs > 0)
                {
                    FreeSlabs--;
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
                while (FreeSlabs > capacity)
                {
                    FreeSlabs--;
                    var temp = node;
                    node = node->Next;
                    NativeMemoryAllocator.Free(temp);
                }

                FreeList = node;
                return FreeSlabs;
            }
        }

        /// <summary>
        ///     Slab
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFixedSize32MemorySlab
        {
            /// <summary>
            ///     Next
            /// </summary>
            public NativeFixedSize32MemorySlab* Next;

            /// <summary>
            ///     Previous
            /// </summary>
            public NativeFixedSize32MemorySlab* Previous;

            /// <summary>
            ///     Bitmap
            /// </summary>
            public uint Bitmap;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeFixedSize32MemoryPoolHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFixedSize32MemoryPool(int length, int maxFreeSlabs)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            if (maxFreeSlabs < 0)
                throw new ArgumentOutOfRangeException(nameof(maxFreeSlabs), maxFreeSlabs, "MustBeNonNegative");
            var nodeSize = 1 + length;
            var array = (byte*)NativeMemoryAllocator.Alloc((uint)(sizeof(NativeFixedSize32MemorySlab) + 32 * nodeSize));
            var slab = (NativeFixedSize32MemorySlab*)array;
            slab->Next = slab;
            slab->Previous = slab;
            slab->Bitmap = 0U;
            array += sizeof(NativeFixedSize32MemorySlab);
            for (byte i = 0; i < 32; ++i)
                *(array + i * nodeSize) = i;
            var handle = (NativeFixedSize32MemoryPoolHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeFixedSize32MemoryPoolHandle));
            handle->Sentinel = slab;
            handle->FreeList = null;
            handle->Slabs = 1;
            handle->FreeSlabs = 0;
            handle->MaxFreeSlabs = maxFreeSlabs;
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
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFixedSize32MemoryPool other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFixedSize32MemoryPool nativeFixedSize32MemoryPool && nativeFixedSize32MemoryPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeFixedSize32MemoryPool";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFixedSize32MemoryPool left, NativeFixedSize32MemoryPool right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFixedSize32MemoryPool left, NativeFixedSize32MemoryPool right) => left._handle != right._handle;

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
        public void* Rent() => _handle->Rent();

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr) => _handle->Return(ptr);

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
        public static NativeFixedSize32MemoryPool Empty => new();
    }
}