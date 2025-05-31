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
        /// <param name="allocator">Allocator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int capacity, int maxLength, CustomMemoryAllocator allocator)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBePositive");
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "MustBeNonNegative");
            if (maxLength > 1073741824)
                maxLength = 1073741824;
            else if (maxLength < 16)
                maxLength = 16;
            var length = SelectBucketIndex(maxLength) + 1;
            var extremeLength = capacity * sizeof(T*);
            var buffer = (byte*)NativeMemoryAllocator.Alloc((uint)(length * (sizeof(NativeArrayPoolBucket) + extremeLength)));
            var buckets = (NativeArrayPoolBucket*)buffer;
            buffer += length * sizeof(NativeArrayPoolBucket);
            Unsafe.InitBlockUnaligned(buffer, 0, (uint)(length * extremeLength));
            for (var i = 0; i < length; ++i)
            {
                ref var bucket = ref buckets[i];
                bucket.Buffer = (T**)(buffer + i * extremeLength);
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
        public bool IsCreated => _buckets != null;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Max length
        /// </summary>
        public int MaxLength => 16 << (_length - 1);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeArrayPool<T> other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArrayPool<T> nativeArrayPool && nativeArrayPool == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_buckets).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeArrayPool<{typeof(T).Name}>";

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
                buckets[i].Dispose(_capacity, (CustomMemoryAllocator*)Unsafe.AsPointer(ref _allocator));
            NativeMemoryAllocator.Free(buckets);
        }

        /// <summary>
        ///     Rent buffer
        /// </summary>
        /// <param name="minimumLength">Minimum buffer length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Rent(int minimumLength)
        {
            if (minimumLength < 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "MustBeNonNegative");
            var index = SelectBucketIndex(minimumLength);
            if (index < _length)
                return _buckets[index].Rent(_capacity, (CustomMemoryAllocator*)Unsafe.AsPointer(ref _allocator));
            throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "BiggerThanCollection");
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
                nativeArray = _buckets[index].Rent(_capacity, (CustomMemoryAllocator*)Unsafe.AsPointer(ref _allocator));
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
        public void Return(NativeArray<T> nativeArray) => Return(nativeArray.Length, nativeArray.Buffer);

        /// <summary>
        ///     Try return buffer
        /// </summary>
        /// <param name="nativeArray">Buffer</param>
        /// <returns>Returned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(NativeArray<T> nativeArray) => TryReturn(nativeArray.Length, nativeArray.Buffer);

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(int length, T* buffer)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                throw new ArgumentException("BufferNotFromPool", nameof(buffer));
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                throw new ArgumentException("BufferNotFromPool", nameof(buffer));
            _buckets[bucket].Return(buffer, (CustomMemoryAllocator*)Unsafe.AsPointer(ref _allocator));
        }

        /// <summary>
        ///     Try return buffer
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returned</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReturn(int length, T* buffer)
        {
            if (length < 16 || (length & (length - 1)) != 0)
                return false;
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                return false;
            _buckets[bucket].Return(buffer, (CustomMemoryAllocator*)Unsafe.AsPointer(ref _allocator));
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
            public T** Buffer;

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
            public void Dispose(int capacity, CustomMemoryAllocator* allocator)
            {
                for (var i = capacity - 1; i >= 0; --i)
                {
                    var buffer = Buffer[i];
                    if (buffer == null)
                        break;
                    allocator->Free(buffer);
                }
            }

            /// <summary>
            ///     Rent buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Rent(int capacity, CustomMemoryAllocator* allocator)
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
                        ptr = buffer[index];
                        buffer[index++] = null;
                    }

                    if (ptr == null)
                        ptr = (T*)allocator->Alloc((uint)Length);
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
            public void Return(T* ptr, CustomMemoryAllocator* allocator)
            {
                ref var spinLock = ref SpinLock;
                ref var index = ref Index;
                var lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);
                    if (index != 0)
                        Buffer[--index] = ptr;
                    else
                        allocator->Free(ptr);
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