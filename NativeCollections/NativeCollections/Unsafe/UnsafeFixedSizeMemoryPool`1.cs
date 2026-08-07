using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory pool that provides reusable fixed-size memory blocks.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeFixedSizeMemoryPool<T> : IIsCreated, IDisposable, IEquatable<UnsafeFixedSizeMemoryPool<T>> where T : unmanaged
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
        ///     Bit buffer
        /// </summary>
        private readonly int* _bitArray;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Bit buffer length
        /// </summary>
        private readonly int _bitArrayLength;

        /// <summary>
        ///     Gets the number of elements.
        /// </summary>
        private int _size;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _size == 0;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public readonly int Count => _size;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFixedSizeMemoryPool(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            var extremeLength = UnsafeBitArray.GetInt32ArrayLengthFromBitLength(capacity);
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<T>(), NativeMemoryAllocator.AlignOf<int>());
            var bufferByteCount = (uint)NativeMemoryAllocator.AlignUp((nuint)(capacity * Unsafe.SizeOf<T>()), alignment);
            _buffer = (T*)NativeMemoryAllocator.AlignedAlloc(bufferByteCount + (uint)(capacity + extremeLength) * (uint)Unsafe.SizeOf<int>(), alignment);
            _index = UnsafeHelpers.AddByteOffset<int>(_buffer, (nint)bufferByteCount);
            _bitArray = UnsafeHelpers.AddByteOffset<int>(_index, capacity * Unsafe.SizeOf<int>());
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(_bitArray), 0, (uint)(extremeLength * Unsafe.SizeOf<int>()));
            _capacity = capacity;
            _bitArrayLength = extremeLength;
            _size = capacity;
            for (var i = 0; i < capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeFixedSizeMemoryPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeFixedSizeMemoryPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => SR.Format("UnsafeFixedSizeMemoryPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeFixedSizeMemoryPool<T> left, UnsafeFixedSizeMemoryPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeFixedSizeMemoryPool<T> left, UnsafeFixedSizeMemoryPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(_bitArray), 0, (uint)(_bitArrayLength * Unsafe.SizeOf<int>()));
            _size = _capacity;
            for (var i = 0; i < _capacity; ++i)
                Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)i) = i;
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
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
            ref var segment = ref Unsafe.Add(ref Unsafe.AsRef<int>(_bitArray), (nint)(index >> 5));
            var bitMask = 1 << index;
            segment |= bitMask;
            ptr = UnsafeHelpers.Add<T>(_buffer, index);
            return true;
        }

        /// <summary>
        ///     Returns to the pool an object that was previously obtained via <see cref="TryRent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var (index, remainder) = MathHelpers.DivRem(byteOffset, Unsafe.SizeOf<T>());
            if ((ulong)index >= (ulong)_capacity || remainder != 0)
                ThrowHelpers.ThrowMismatchException();
            ref var segment = ref Unsafe.Add(ref Unsafe.AsRef<int>(_bitArray), index >> 5);
            var bitMask = 1 << (int)index;
            if ((segment & bitMask) == 0)
                ThrowHelpers.ThrowDuplicateException();
            segment &= ~bitMask;
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)_size++) = (int)index;
        }

        /// <summary>
        ///     Try return buffer
        /// </summary>
        /// <param name="ptr">Pointer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(T* ptr)
        {
            var byteOffset = UnsafeHelpers.ByteOffset(_buffer, ptr);
            var (index, remainder) = MathHelpers.DivRem(byteOffset, Unsafe.SizeOf<T>());
            if ((ulong)index >= (ulong)_capacity || remainder != 0)
                return false;
            ref var segment = ref Unsafe.Add(ref Unsafe.AsRef<int>(_bitArray), index >> 5);
            var bitMask = 1 << (int)index;
            if ((segment & bitMask) == 0)
                return false;
            segment &= ~bitMask;
            Unsafe.Add(ref Unsafe.AsRef<int>(_index), (nint)_size++) = (int)index;
            return true;
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeFixedSizeMemoryPool<T> Empty => default;
    }
}