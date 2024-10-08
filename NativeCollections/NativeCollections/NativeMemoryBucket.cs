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
    ///     Native memory bucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeMemoryBucket : IDisposable, IEquatable<NativeMemoryBucket>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryBucketHandle
        {
            /// <summary>
            ///     Size
            /// </summary>
            public int Size;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Array
            /// </summary>
            public void** Array;

            /// <summary>
            ///     Index
            /// </summary>
            public int Index;

            /// <summary>
            ///     Memory pool
            /// </summary>
            public NativeMemoryPool MemoryPool;
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeMemoryBucketHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryBucket(int size, int length, int maxFreeSlabs)
        {
            var memoryPool = new NativeMemoryPool(size, length, maxFreeSlabs);
            _handle = (NativeMemoryBucketHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeMemoryBucketHandle));
            _handle->Size = size;
            _handle->Length = length;
            _handle->Array = (void**)NativeMemoryAllocator.AllocZeroed((uint)(size * sizeof(void*)));
            _handle->Index = 0;
            _handle->MemoryPool = memoryPool;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

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
        public bool Equals(NativeMemoryBucket other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemoryBucket nativeMemoryBucket && nativeMemoryBucket == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_handle;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeMemoryBucket";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryBucket left, NativeMemoryBucket right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryBucket left, NativeMemoryBucket right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_handle == null)
                return;
            NativeMemoryAllocator.Free(_handle->Array);
            _handle->MemoryPool.Dispose();
            NativeMemoryAllocator.Free(_handle);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void* Rent()
        {
            void* buffer = null;
            if (_handle->Index < _handle->Size)
            {
                buffer = _handle->Array[_handle->Index];
                _handle->Array[_handle->Index++] = null;
            }

            if (buffer == null)
                buffer = _handle->MemoryPool.Rent();
            return buffer;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(void* ptr)
        {
            if (_handle->Index != 0)
                _handle->Array[--_handle->Index] = ptr;
            else
                _handle->MemoryPool.Return(ptr);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryBucket Empty => new();
    }
}