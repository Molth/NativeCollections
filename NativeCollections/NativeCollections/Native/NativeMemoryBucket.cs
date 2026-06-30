using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory bucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeMemoryBucket))]
    public readonly unsafe struct NativeMemoryBucket : IIsCreated, IDisposable, IEquatable<NativeMemoryBucket>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeMemoryBucket* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryBucket(int capacity, int length, int alignment)
        {
            var value = new UnsafeMemoryBucket(capacity, length, alignment);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeMemoryBucket>(1);
            Unsafe.AsRef<UnsafeMemoryBucket>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="length">Length</param>
        /// <param name="alignment">Alignment</param>
        /// <param name="allocator">Memory allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryBucket(int capacity, int length, int alignment, CustomMemoryAllocator allocator)
        {
            var value = new UnsafeMemoryBucket(capacity, length, alignment, allocator);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeMemoryBucket>(1);
            Unsafe.AsRef<UnsafeMemoryBucket>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _handle->Capacity;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Alignment
        /// </summary>
        public int Alignment => _handle->Alignment;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeMemoryBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemoryBucket other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

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
        public static bool operator ==(NativeMemoryBucket left, NativeMemoryBucket right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryBucket left, NativeMemoryBucket right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Dispose();
            NativeMemoryAllocator.AlignedFree(handle);
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
        ///     Empty
        /// </summary>
        public static NativeMemoryBucket Empty => new();
    }
}