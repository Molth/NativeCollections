using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native fixed size queue memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeFixedSizeQueueMemoryPool<>))]
    public readonly unsafe struct NativeFixedSizeQueueMemoryPool<T> : IIsCreated, IDisposable, IEquatable<NativeFixedSizeQueueMemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeFixedSizeQueueMemoryPool<T>* _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _handle->Count;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _handle->Capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeFixedSizeQueueMemoryPool(int capacity)
        {
            var value = new UnsafeFixedSizeQueueMemoryPool<T>(capacity);
            var handle = NativeMemoryAllocator.AlignedAlloc<UnsafeFixedSizeQueueMemoryPool<T>>(1);
            Unsafe.AsRef<UnsafeFixedSizeQueueMemoryPool<T>>(handle) = value;
            _handle = handle;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeFixedSizeQueueMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeFixedSizeQueueMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeFixedSizeQueueMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeFixedSizeQueueMemoryPool<T> left, NativeFixedSizeQueueMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeFixedSizeQueueMemoryPool<T> left, NativeFixedSizeQueueMemoryPool<T> right) => !left.Equals(right);

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
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr) => _handle->TryRent(out ptr);

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle->Return(ptr);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeFixedSizeQueueMemoryPool<T> Empty => new();
    }
}