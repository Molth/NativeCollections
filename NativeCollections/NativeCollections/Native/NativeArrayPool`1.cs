using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     NativeMemoryPool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public unsafe struct NativeArrayPool<T> : IDisposable, IEquatable<NativeArrayPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Buckets
        /// </summary>
        private readonly NativeArrayPoolBucket* _buckets;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Allocator
        /// </summary>
        private CustomMemoryAllocator _allocator;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="maxLength">Max length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int capacity, int maxLength) : this(capacity, maxLength, CustomMemoryAllocator.Default)
        {
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="maxLength">Max length</param>
        /// <param name="allocator">Allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int capacity, int maxLength, CustomMemoryAllocator allocator)
        {
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, nameof(capacity));
            ThrowHelpers.ThrowIfNegative(maxLength, nameof(maxLength));
            if (maxLength > 1073741824)
                maxLength = 1073741824;
            else if (maxLength < 16)
                maxLength = 16;
            var length = SelectBucketIndex(maxLength) + 1;
            var alignment = Math.Max((uint)NativeMemoryAllocator.AlignOf<NativeArrayPoolBucket>(), (uint)NativeMemoryAllocator.AlignOf<nint>());
            var bucketsLength = (uint)NativeMemoryAllocator.AlignUp((nuint)(length * sizeof(NativeArrayPoolBucket)), alignment);
            var extremeLength = capacity * sizeof(nint);
            var buffer = NativeMemoryAllocator.AlignedAlloc((uint)(bucketsLength + length * extremeLength), alignment);
            var buckets = (NativeArrayPoolBucket*)buffer;
            buffer = UnsafeHelpers.AddByteOffset(buffer, (nint)bucketsLength);
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(buffer), 0, (uint)(length * extremeLength));
            for (var i = 0; i < length; ++i)
            {
                ref var bucket = ref Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), (nint)i);
                bucket.Buffer = UnsafeHelpers.AddByteOffset<nint>(buffer, i * extremeLength);
                bucket.Length = 16 << i;
                bucket.Index = 0;
                bucket.SpinLock = new SpinLock();
            }

            _buckets = buckets;
            _length = length;
            _capacity = capacity;
            _allocator = allocator;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => _buckets != null;

        /// <summary>
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Max length
        /// </summary>
        public readonly int MaxLength => 16 << (_length - 1);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(NativeArrayPool<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is NativeArrayPool<T> nativeArrayPool && nativeArrayPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => ((nint)_buckets).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => $"NativeArrayPool<{typeof(T).Name}>";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArrayPool<T> left, NativeArrayPool<T> right) => left._buckets == right._buckets;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArrayPool<T> left, NativeArrayPool<T> right) => left._buckets != right._buckets;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buckets = _buckets;
            if (buckets == null)
                return;
            for (var i = 0; i < _length; ++i)
                Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(buckets), (nint)i).Dispose(_capacity, ref _allocator);
            NativeMemoryAllocator.AlignedFree(buckets);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="minimumLength">Minimum buffer length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Rent(int minimumLength)
        {
            ThrowHelpers.ThrowIfNegative(minimumLength, nameof(minimumLength));
            var index = SelectBucketIndex(minimumLength);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _length, nameof(minimumLength));
            return Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, ref _allocator);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="minimumLength">Minimum buffer length</param>
        /// <param name="nativeArray">Buffer</param>
        /// <returns>Rented</returns>
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
                nativeArray = Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, ref _allocator);
                return true;
            }

            nativeArray = default;
            return false;
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="nativeArray">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(NativeArray<T> nativeArray) => Return(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Try return buffer
        /// </summary>
        /// <param name="nativeArray">Buffer</param>
        /// <returns>Returned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(NativeArray<T> nativeArray) => TryReturn(nativeArray.Buffer, nativeArray.Length);

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* buffer, int length)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                ThrowHelpers.ThrowBufferNotFromPoolException(nameof(buffer));
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                ThrowHelpers.ThrowBufferNotFromPoolException(nameof(buffer));
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, ref _allocator);
        }

        /// <summary>
        ///     Try return buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Returned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(T* buffer, int length)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                return false;
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                return false;
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, ref _allocator);
            return true;
        }

        /// <summary>
        ///     Select bucket index
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bucket index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int SelectBucketIndex(int bufferSize) => BitOperationsHelpers.Log2(((uint)bufferSize - 1) | 15) - 3;

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArrayPool<T> Empty => new();

        /// <summary>
        ///     NativeArrayPool bucket
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeArrayPoolBucket
        {
            /// <summary>
            ///     Buffers
            /// </summary>
            public nint* Buffer;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Index
            /// </summary>
            public int Index;

            /// <summary>
            ///     State lock
            /// </summary>
            public SpinLock SpinLock;

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public readonly void Dispose(int capacity, ref CustomMemoryAllocator allocator)
            {
                for (var i = capacity - 1; i >= 0; --i)
                {
                    var buffer = (void*)Unsafe.Add(ref Unsafe.AsRef<nint>(Buffer), (nint)i);
                    if (buffer == null)
                        break;
                    allocator.AlignedFree(buffer);
                }
            }

            /// <summary>
            ///     Rent buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Rent(int capacity, ref CustomMemoryAllocator allocator)
            {
                var buffer = Buffer;
                T* ptr = null;
                ref var spinLock = ref SpinLock;
                ref var index = ref Index;
                var lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    if (index < capacity)
                    {
                        ptr = (T*)Unsafe.Add(ref Unsafe.AsRef<nint>(buffer), (nint)index);
                        Unsafe.Add(ref Unsafe.AsRef<nint>(buffer), (nint)index++) = 0;
                    }

                    if (ptr == null)
                        ptr = (T*)allocator.AlignedAlloc((uint)Length, (uint)NativeMemoryAllocator.AlignOf<T>());
                }
                finally
                {
                    if (lockTaken)
                        spinLock.Exit(false);
                }

                return new NativeArray<T>(ptr, Length);
            }

            /// <summary>
            ///     Return buffer
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Return(T* ptr, ref CustomMemoryAllocator allocator)
            {
                ref var spinLock = ref SpinLock;
                ref var index = ref Index;
                var lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    if (index != 0)
                        Unsafe.Add(ref Unsafe.AsRef<nint>(Buffer), (nint)(--index)) = (nint)ptr;
                    else
                        allocator.AlignedFree(ptr);
                }
                finally
                {
                    if (lockTaken)
                        spinLock.Exit(false);
                }
            }
        }
    }
}