using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe fixed size stack memory pool
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeStackMemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeFixedSizeStackMemoryPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly T* _buffer;

        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly int* _index;

        /// <summary>
        ///     Capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Size
        /// </summary>
        private int _size;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSizeStackMemoryPool(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            var alignment = (uint)Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<T>()), alignment);
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc((uint)(bufferByteCount + capacity * Unsafe.SizeOf<int>()), alignment);
            _index = UnsafeHelpers.AddByteOffset<int>(_buffer, (nint)bufferByteCount);
            _capacity = capacity;
            _size = capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeFixedSizeStackMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeFixedSizeStackMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => SR.Format("UnsafeFixedSizeStackMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeFixedSizeStackMemoryPool<T> left, UnsafeFixedSizeStackMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeFixedSizeStackMemoryPool<T> left, UnsafeFixedSizeStackMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _size = _capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out T* ptr)
        {
            var size = _size - 1;
            if ((uint)size >= (uint)_capacity)
            {
                ptr = null;
                return false;
            }

            _size = size;
            var index = Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)size);
            ptr = UnsafeHelpers.Add<T>(_buffer, index);
            return true;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var index = byteOffset / Unsafe.SizeOf<T>();
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)_size++) = (int)index;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSizeStackMemoryPool<T> Empty => new();
    }
}