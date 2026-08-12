using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a resource pool that enables reusing instances of native-arrays.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArrayPool<T> : IIsCreated, IDisposable, IEquatable<NativeArrayPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Array of bucket.
        /// </summary>
        private readonly NativeArrayPoolBucket* _buckets;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     The custom memory allocator used for allocating and deallocating buffers.
        /// </summary>
        private readonly CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified capacity and maximum length,
        ///     using the default memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained per bucket.
        ///     Must be greater than zero.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum allowed buffer length (in elements).
        ///     The actual length will be clamped to the nearest power of two.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or equal to zero,
        ///     or when <paramref name="maxLength" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int capacity, int maxLength) : this(capacity, maxLength, CustomMemoryAllocator.Default)
        {
        }

        /// <summary>
        ///     Initializes a new instance of this class
        ///     with the specified capacity, maximum length, and custom memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained per bucket.
        ///     Must be greater than zero.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum allowed buffer length (in elements).
        ///     The actual length will be clamped to the nearest power of two.
        /// </param>
        /// <param name="allocator">The custom memory allocator to use for all allocations and deallocations.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or equal to zero,
        ///     or when <paramref name="maxLength" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int capacity, int maxLength, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(maxLength, ExceptionArgument.maxLength);
            maxLength = Math.Clamp(maxLength, 16, 1073741824);
            var length = SelectBucketIndex(maxLength) + 1;
            var alignment = Math.Max(NativeMemoryAllocator.AlignOf<NativeArrayPoolBucket>(), NativeMemoryAllocator.AlignOf<nint>());
            var bucketsLength = (uint)NativeMemoryAllocator.AlignUp((nuint)(length * Unsafe.SizeOf<NativeArrayPoolBucket>()), alignment);
            var extremeLength = capacity * Unsafe.SizeOf<nint>();
            var buffer = NativeMemoryAllocator.AlignedAlloc(bucketsLength + (uint)length * (uint)extremeLength, alignment);
            var buckets = (NativeArrayPoolBucket*)buffer;
            buffer = UnsafeHelpers.AddByteOffset(buffer, (nint)bucketsLength);
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(buffer), 0, (uint)(length * extremeLength));
            for (var i = 0; i < length; ++i)
            {
                ref var bucket = ref Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), (nint)i);
                bucket = new NativeArrayPoolBucket(UnsafeHelpers.AddByteOffset<nint>(buffer, i * extremeLength), 16 << i);
            }

            _buckets = buckets;
            _length = length;
            _capacity = capacity;
            _allocator = allocator;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buckets);

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Gets the maximum allowed buffer length (in elements) supported by this pool.
        /// </summary>
        /// <remarks>
        ///     The value is the highest power of two that the pool can manage,
        ///     calculated as <c>16 &lt;&lt; (bucketCount - 1)</c>.
        /// </remarks>
        public int MaxLength => 16 << (_length - 1);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeArrayPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeArrayPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeArrayPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeArrayPool<T> left, NativeArrayPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeArrayPool<T> left, NativeArrayPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buckets = _buckets;
            if (UnsafeHelpers.IsNull(buckets))
                return;
            for (var i = 0; i < _length; ++i)
                Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), (nint)i).Dispose(_capacity, _allocator);
            NativeMemoryAllocator.AlignedFree(buckets);
        }

        /// <summary>
        ///     Retrieves a buffer that is at least the requested length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Rent(int minimumLength)
        {
            ThrowHelpers.ThrowIfNegative(minimumLength, ExceptionArgument.minimumLength);
            var index = SelectBucketIndex(minimumLength);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _length, ExceptionArgument.minimumLength);
            return Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, _allocator);
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(int minimumLength, out NativeArray<T> nativeArray)
        {
            if (minimumLength < 0)
            {
                nativeArray = default;
                return false;
            }

            var index = SelectBucketIndex(minimumLength);
            if (index < _length)
            {
                nativeArray = Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, _allocator);
                return true;
            }

            nativeArray = default;
            return false;
        }

        /// <summary>
        ///     Returns to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(NativeArray<T> nativeArray) => Return(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Attempts to return to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(NativeArray<T> nativeArray) => TryReturn(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Returns to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* buffer, int length)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, _allocator);
        }

        /// <summary>
        ///     Attempts to return to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(T* buffer, int length)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                return false;
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                return false;
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, _allocator);
            return true;
        }

        /// <summary>
        ///     Determines the bucket index for a given buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size (in elements).</param>
        /// <returns>
        ///     The zero‑based index of the bucket that can accommodate the specified size.
        ///     Buckets correspond to powers of two, starting from 16.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SelectBucketIndex(int bufferSize) => BitOperationsHelpers.Log2(((uint)bufferSize - 1) | 15) - 3;

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeArrayPool<T> Empty => default;

        /// <summary>
        ///     Represents a bucket that stores free buffers of a fixed size (a power of two).
        /// </summary>
        /// <remarks>
        ///     Each bucket is responsible for managing up to <c>capacity</c> buffers of the same length.
        ///     The bucket uses a <see cref="SpinLock" /> to synchronize concurrent access.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeArrayPoolBucket
        {
            /// <summary>
            ///     Represents a contiguous region of arbitrary memory.
            /// </summary>
            [NativePointer(typeof(void*))] private readonly nint* _buffer;

            /// <summary>
            ///     Gets the total numbers of elements the internal data structure can hold.
            /// </summary>
            private readonly int _length;

            /// <summary>
            ///     The current index.
            /// </summary>
            private int _index;

            /// <summary>
            ///     Synchronization lock to protect concurrent access to the bucket's internal state.
            /// </summary>
            private SpinLock _spinLock;

            /// <summary>
            ///     Initializes a new instance of this class.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArrayPoolBucket(nint* buffer, int length)
            {
                _buffer = buffer;
                _length = length;
                _index = 0;
                _spinLock = new SpinLock();
            }

            /// <summary>
            ///     Performs application-defined tasks associated with freeing,
            ///     releasing, or resetting unmanaged resources.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose(int capacity, in CustomMemoryAllocator allocator)
            {
                for (var i = _index; i < capacity; ++i)
                {
                    var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)i);
                    if (UnsafeHelpers.IsNull(buffer))
                        break;
                    allocator.AlignedFree(buffer);
                }
            }

            /// <summary>
            ///     Retrieves a buffer that is at least the requested length.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Rent(int capacity, in CustomMemoryAllocator allocator)
            {
                void* ptr = null;
                var lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    if (_index < capacity)
                    {
                        ptr = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index);
                        Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)_index++) = 0;
                    }

                    if (UnsafeHelpers.IsNull(ptr))
                        ptr = allocator.AlignedAlloc((uint)_length, NativeMemoryAllocator.AlignOf<T>());
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }

                return new NativeArray<T>((T*)ptr, _length);
            }

            /// <summary>
            ///     Returns to the pool an array that was previously obtained.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Return(void* ptr, in CustomMemoryAllocator allocator)
            {
                var lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    if (_index != 0)
                        Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), (nint)(--_index)) = (nint)ptr;
                    else
                        allocator.AlignedFree(ptr);
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }
            }
        }
    }
}