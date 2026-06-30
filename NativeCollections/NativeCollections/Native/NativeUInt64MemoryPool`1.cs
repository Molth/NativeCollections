using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native ulong bitmap memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeUInt64MemoryPool<>))]
    public readonly unsafe struct NativeUInt64MemoryPool<T> : IIsCreated, IDisposable, IEquatable<NativeUInt64MemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeUInt64MemoryPool<T>* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUInt64MemoryPool(int maxFreeSlabs)
        {
            var value = new UnsafeUInt64MemoryPool<T>(maxFreeSlabs);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeUInt64MemoryPool<T>>(1);
            Unsafe.AsRef<UnsafeUInt64MemoryPool<T>>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeUInt64MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            var value = new UnsafeUInt64MemoryPool<T>(length, maxFreeSlabs, alignment);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeUInt64MemoryPool<T>>(1);
            Unsafe.AsRef<UnsafeUInt64MemoryPool<T>>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

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
        ///     Alignment
        /// </summary>
        public int Alignment => _handle->Alignment;

        /// <summary>
        ///     Aligned length
        /// </summary>
        public int AlignedLength => _handle->AlignedLength;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeUInt64MemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeUInt64MemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeUInt64MemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeUInt64MemoryPool<T> left, NativeUInt64MemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeUInt64MemoryPool<T> left, NativeUInt64MemoryPool<T> right) => !left.Equals(right);

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
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle->Clear();

        /// <summary>
        ///     Clear
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle->Clear(capacity);

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent() => _handle->Rent();

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle->Return(ptr);

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
        public static NativeUInt64MemoryPool<T> Empty => new();
    }
}