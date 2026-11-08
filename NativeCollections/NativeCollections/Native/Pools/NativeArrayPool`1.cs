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
    /// <typeparam name="T">The unmanaged element type allocated by the pool.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArrayPool<T> : IIsCreated, IDisposable, IEquatable<NativeArrayPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Array of bucket.
        /// </summary>
        private readonly NativeArrayPoolBucket* _buckets;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     The custom memory allocator used for allocating and deallocating buffers.
        /// </summary>
        private readonly CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="buckets">Array of bucket.</param>
        /// <param name="length">The total number of elements the internal data structure can hold.</param>
        /// <param name="capacity">The total number of elements the internal data structure can hold.</param>
        /// <param name="allocator">The custom memory allocator used for allocating and deallocating buffers.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeArrayPool(NativeArrayPoolBucket* buckets, int length, int capacity, CustomMemoryAllocator allocator)
        {
            _buckets = buckets;
            _length = length;
            _capacity = capacity;
            _allocator = allocator;
        }

        /// <summary>
        ///     Creates an array pool using the default memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained per bucket. Must be greater than zero.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum allowed buffer length (in elements). The
        ///     actual length will be clamped to the nearest power of two.
        /// </param>
        /// <returns>A newly allocated array pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or
        ///     equal to zero or <paramref name="maxLength" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayPool<T> Create(int capacity, int maxLength) => Create(capacity, maxLength, CustomMemoryAllocator.Default);

        /// <summary>
        ///     Creates an array pool using a custom memory allocator.
        /// </summary>
        /// <param name="capacity">
        ///     The maximum number of buffers that can be retained per bucket. Must be greater than zero.
        /// </param>
        /// <param name="maxLength">
        ///     The maximum allowed buffer length (in elements). The actual length will be clamped to the nearest power of two.
        /// </param>
        /// <param name="allocator">
        ///     The custom memory allocator used to allocate and free the per-bucket element buffers.
        ///     The pool's own metadata, including the bucket table and the bucket storage that backs
        ///     those buffers, is allocated and freed with the static <see cref="NativeMemoryAllocator" />
        ///     instead, so this allocator only observes the buffers handed out by <see cref="Rent" />
        ///     and reclaimed by <see cref="Return(NativeArray{T})" />.
        /// </param>
        /// <returns>A newly allocated array pool.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than or
        ///     equal to zero or <paramref name="maxLength" /> is negative.
        /// </exception>
        /// <remarks>
        ///     Because part of the pool is allocated with the static <see cref="NativeMemoryAllocator" />,
        ///     a counting or tracking custom allocator will not see a balanced allocation count for the
        ///     pool as a whole; only the element buffers it manages are reported to
        ///     <paramref name="allocator" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArrayPool<T> Create(int capacity, int maxLength, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(maxLength, ExceptionArgument.maxLength);
            maxLength = Math.Clamp(maxLength, 16, 1073741824);
            var length = SelectBucketIndex(maxLength).WrappingAdd(1);
            var alignment = MathHelpers.Max(NativeMemoryAllocator.AlignOf<NativeArrayPoolBucket>(), NativeMemoryAllocator.AlignOf<nint>());
            var bucketsLength = NativeMemoryAllocator.AlignUp(Usize.FromI32Unchecked(length).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<NativeArrayPoolBucket>())), alignment);
            var extremeLength = Usize.FromI32Unchecked(capacity) * Usize.FromI32Unchecked(Unsafe.SizeOf<nint>());
            var buffer = NativeMemoryAllocator.AlignedAlloc(bucketsLength + Usize.FromI32Unchecked(length) * extremeLength, alignment);
            var buckets = (NativeArrayPoolBucket*)buffer;
            buffer = UnsafeHelpers.AddByteOffset(buffer, Isize.FromUsizeUnchecked(bucketsLength));
            SpanHelpers.Set(ref Unsafe.AsRef<byte>(buffer), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(length).WrappingMul(extremeLength));
            for (var i = 0; i < length; i = i.WrappingAdd(1))
            {
                ref var bucket = ref Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), Isize.FromI32Unchecked(i));
                bucket = NativeArrayPoolBucket.From(UnsafeHelpers.AddByteOffset<nint>(buffer, Isize.FromUsizeUnchecked(Usize.FromI32Unchecked(i).WrappingMul(extremeLength))), 16 << i);
            }

            return new NativeArrayPool<T>(buckets, length, capacity, allocator);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buckets != null;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Gets the maximum allowed buffer length (in elements) supported by this pool.
        /// </summary>
        /// <remarks>
        ///     The value is the highest power of two that the pool can
        ///     manage, calculated as <c>16 &lt;&lt; (bucketCount - 1)</c>.
        /// </remarks>
        public int MaxLength => 16 << _length.WrappingSub(1);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeArrayPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeArrayPool<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeArrayPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeArrayPool<T> left, NativeArrayPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeArrayPool<T> left, NativeArrayPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var buckets = _buckets;
            if (buckets == null)
                return;

            for (var i = 0; i < _length; i = i.WrappingAdd(1))
                Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), Isize.FromI32Unchecked(i)).Dispose(_capacity, _allocator);

            NativeMemoryAllocator.AlignedFree(buckets);
        }

        /// <summary>
        ///     Retrieves a buffer that is at least the requested length.
        /// </summary>
        /// <param name="minimumLength">The minimum number of elements in the returned buffer.</param>
        /// <returns>A rented buffer whose length is at least <paramref name="minimumLength" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="minimumLength" /> is negative
        ///     or greater than the maximum length supported by this pool.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Rent(int minimumLength)
        {
            ThrowHelpers.ThrowIfNegative(minimumLength, ExceptionArgument.minimumLength);
            var index = SelectBucketIndex(minimumLength);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _length, ExceptionArgument.minimumLength);
            return Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), Isize.FromI32Unchecked(index)).Rent(_capacity, _allocator);
        }

        /// <summary>
        ///     Attempts to retrieve a buffer that is at least the requested length.
        /// </summary>
        /// <param name="minimumLength">The minimum number of elements in the returned buffer.</param>
        /// <param name="nativeArray">
        ///     When this method returns, contains the rented array if
        ///     the operation succeeds; otherwise, the default value.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if a buffer was successfully retrieved; otherwise, <see langword="false" />.
        /// </returns>
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
                nativeArray = Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), Isize.FromI32Unchecked(index)).Rent(_capacity, _allocator);
                return true;
            }

            nativeArray = default;
            return false;
        }

        /// <summary>
        ///     Returns to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="nativeArray">The array to return to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(NativeArray<T> nativeArray) => Return(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Attempts to return to the pool an array that was previously
        ///     obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="nativeArray">The array to return to the pool.</param>
        /// <returns>
        ///     <see langword="true" /> if the buffer was successfully
        ///     returned to the pool; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(NativeArray<T> nativeArray) => TryReturn(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Returns to the pool an array that was previously obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer to return to the pool.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="length" /> is less than <c>16</c>, is not a
        ///     power of two, or is greater than the maximum length supported by this pool.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* buffer, int length)
        {
            if (length < 16 || (length & length.WrappingSub(1)) != 0)
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);

            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);

            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), Isize.FromI32Unchecked(bucket)).Return(buffer, _allocator);
        }

        /// <summary>
        ///     Attempts to return to the pool an array that was previously
        ///     obtained via <see cref="Rent" /> on the same instance.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer to return to the pool.</param>
        /// <param name="length">The length of the buffer.</param>
        /// <returns>
        ///     <see langword="true" /> if the buffer was successfully
        ///     returned to the pool; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(T* buffer, int length)
        {
            if (length < 16 || (length & length.WrappingSub(1)) != 0)
                return false;

            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                return false;

            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), Isize.FromI32Unchecked(bucket)).Return(buffer, _allocator);
            return true;
        }

        /// <summary>
        ///     Determines the bucket index for a given buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size (in elements).</param>
        /// <returns>
        ///     The zero‑based index of the bucket that can accommodate the specified
        ///     size. Buckets correspond to powers of two, starting from 16.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SelectBucketIndex(int bufferSize) => BitOperationsHelpers.Log2(U32.FromI32Unchecked(Math.Max(bufferSize, 1)).WrappingSub(U32.FromI32Unchecked(1)) | U32.FromI32Unchecked(15)).WrappingSub(3);

        /// <summary>
        ///     Represents a bucket that stores free buffers of a fixed size (a power of two).
        /// </summary>
        /// <remarks>
        ///     Each bucket is responsible for managing up to <c>capacity</c> buffers of the same
        ///     length. The bucket uses a <see cref="SpinLock" /> to synchronize concurrent access.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeArrayPoolBucket
        {
            /// <summary>
            ///     Represents a contiguous region of arbitrary memory.
            /// </summary>
            [BindingType(typeof(void*))] private readonly nint* _buffer;

            /// <summary>
            ///     Gets the total number of elements the internal data structure can hold.
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
            /// <param name="buffer">The native buffer that backs the bucket.</param>
            /// <param name="length">The number of elements in the buffer.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private NativeArrayPoolBucket(nint* buffer, int length)
            {
                _buffer = buffer;
                _length = length;
                _index = 0;
                _spinLock = default;
            }

            /// <summary>
            ///     Creates a new instance of this class that wraps the specified buffer.
            /// </summary>
            /// <param name="buffer">The native buffer that backs the bucket.</param>
            /// <param name="length">The number of elements in the buffer.</param>
            /// <returns>A new instance of this class.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static NativeArrayPoolBucket From(nint* buffer, int length) => new(buffer, length);

            /// <summary>
            ///     Performs application-defined tasks associated with
            ///     freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <param name="capacity">The maximum number of cached buffers to release.</param>
            /// <param name="allocator">The allocator used to free the cached buffers.</param>
            public readonly void Dispose(int capacity, CustomMemoryAllocator allocator)
            {
                for (var i = _index; i < capacity; i = i.WrappingAdd(1))
                {
                    var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(i));
                    if (buffer == null)
                        break;

                    allocator.AlignedFree(buffer);
                }
            }

            /// <summary>
            ///     Retrieves a buffer that is at least the requested length.
            /// </summary>
            /// <param name="capacity">The maximum number of cached buffers the bucket can hold.</param>
            /// <param name="allocator">
            ///     The allocator used to allocate a new buffer when no cached buffer is available.
            /// </param>
            /// <returns>A rented buffer of the bucket's configured length.</returns>
            public NativeArray<T> Rent(int capacity, CustomMemoryAllocator allocator)
            {
                void* ptr = null;
                var lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    if (_index < capacity)
                    {
                        ptr = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(_index));
                        Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked((_index = _index.WrappingAdd(1)).WrappingSub(1))) = Isize.FromI32Unchecked(0);
                    }

                    if (ptr == null)
                        ptr = allocator.AlignedAlloc<T>(Usize.FromI32Unchecked(_length));
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }

                return NativeArray<T>.From((T*)ptr, _length);
            }

            /// <summary>
            ///     Returns to the pool an array that was previously obtained.
            /// </summary>
            /// <param name="ptr">A pointer to the buffer to return.</param>
            /// <param name="allocator">
            ///     The allocator used to free the buffer when the bucket
            ///     already holds its maximum number of cached buffers.
            /// </param>
            public void Return(void* ptr, CustomMemoryAllocator allocator)
            {
                var lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    if (_index != 0)
                        Unsafe.Add(ref Unsafe.AsRef<nint>(_buffer), Isize.FromI32Unchecked(_index = _index.WrappingSub(1))) = Isize.FromPtrUnchecked(ptr);
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