using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe ulong bitmap memory pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    [BindingType(typeof(UnsafeUInt64MemoryPool))]
    public unsafe struct UnsafeUInt64MemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeUInt64MemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private UnsafeUInt64MemoryPool _handle;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Slabs
        /// </summary>
        public readonly int Slabs => _handle.Slabs;

        /// <summary>
        ///     Free slabs
        /// </summary>
        public readonly int FreeSlabs => _handle.FreeSlabs;

        /// <summary>
        ///     Max free slabs
        /// </summary>
        public readonly int MaxFreeSlabs => _handle.MaxFreeSlabs;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _handle.Length;

        /// <summary>
        ///     Alignment
        /// </summary>
        public readonly int Alignment => _handle.Alignment;

        /// <summary>
        ///     Aligned length
        /// </summary>
        public readonly int AlignedLength => _handle.AlignedLength;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUInt64MemoryPool(int maxFreeSlabs) => _handle = new UnsafeUInt64MemoryPool(Unsafe.SizeOf<T>(), maxFreeSlabs, (int)NativeMemoryAllocator.AlignOf<T>());

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="maxFreeSlabs">Max free slabs</param>
        /// <param name="alignment">Alignment</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeUInt64MemoryPool(int length, int maxFreeSlabs, int alignment)
        {
            ThrowHelpers.ThrowIfLessThan(length, Unsafe.SizeOf<T>(), ExceptionArgument.length);
            ThrowHelpers.ThrowIfLessThan(alignment, (int)NativeMemoryAllocator.AlignOf<T>(), ExceptionArgument.alignment);
            _handle = new UnsafeUInt64MemoryPool(length, maxFreeSlabs, alignment);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeUInt64MemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeUInt64MemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeUInt64MemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeUInt64MemoryPool<T> left, UnsafeUInt64MemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeUInt64MemoryPool<T> left, UnsafeUInt64MemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Clear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _handle.Clear();

        /// <summary>
        ///     Clear
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Clear(int capacity) => _handle.Clear(capacity);

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Rent() => (T*)_handle.Rent();

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr) => _handle.Return(ptr);

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>New capacity</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle.EnsureCapacity(capacity);

        /// <summary>
        ///     Trim excess
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrimExcess() => _handle.TrimExcess();

        /// <summary>
        ///     Trim excess
        /// </summary>
        /// <param name="capacity">Remaining free slabs</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle.TrimExcess(capacity);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeUInt64MemoryPool<T> Empty => new();
    }
}