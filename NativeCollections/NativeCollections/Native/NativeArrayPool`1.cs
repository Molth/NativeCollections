using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native array pool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeArrayPool<T> : IIsCreated, IDisposable, IEquatable<NativeArrayPool<T>> where T : unmanaged
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
        private readonly CustomMemoryAllocator _allocator;

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
            ThrowHelpers.ThrowIfNegativeOrZero(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfNegative(maxLength, ExceptionArgument.maxLength);
            maxLength = Math.Clamp(maxLength, 16, 1073741824);
            var length = SelectBucketIndex(maxLength) + 1;
            var alignment = Math.Max((uint)NativeMemoryAllocator.AlignOf<NativeArrayPoolBucket>(), (uint)NativeMemoryAllocator.AlignOf<nint>());
            var bucketsLength = (uint)NativeMemoryAllocator.AlignUp((nuint)(length * Unsafe.SizeOf<NativeArrayPoolBucket>()), alignment);
            var extremeLength = capacity * Unsafe.SizeOf<nint>();
            var buffer = NativeMemoryAllocator.AlignedAlloc((uint)(bucketsLength + length * extremeLength), alignment);
            var buckets = (NativeArrayPoolBucket*)buffer;
            buffer = UnsafeHelpers.AddByteOffset(buffer, (nint)bucketsLength);
            Unsafe.InitBlockUnaligned(ref Unsafe.AsRef<byte>(buffer), 0, (uint)(length * extremeLength));
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
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buckets);

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
        public bool Equals(NativeArrayPool<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeArrayPool<T> other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeArrayPool<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeArrayPool<T> left, NativeArrayPool<T> right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeArrayPool<T> left, NativeArrayPool<T> right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
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
        ///     Rent buffer
        /// </summary>
        /// <param name="minimumLength">Minimum buffer length</param>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> Rent(int minimumLength)
        {
            ThrowHelpers.ThrowIfNegative(minimumLength, ExceptionArgument.minimumLength);
            var index = SelectBucketIndex(minimumLength);
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, _length, ExceptionArgument.minimumLength);
            return Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, _allocator);
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
                nativeArray = Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)index).Rent(_capacity, _allocator);
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
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                ThrowHelpers.ThrowBufferNotFromPoolException(ExceptionArgument.buffer);
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, _allocator);
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
            Unsafe.Add(ref Unsafe.AsRef<NativeArrayPoolBucket>(_buckets), (nint)bucket).Return(buffer, _allocator);
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
            [NativePointer(typeof(void*))] private readonly nint* _buffer;

            /// <summary>
            ///     Length
            /// </summary>
            private readonly int _length;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     State lock
            /// </summary>
            private SpinLock _spinLock;

            /// <summary>
            ///     Structure
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
            ///     Dispose
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
            ///     Rent buffer
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
                        ptr = allocator.AlignedAlloc((uint)_length, (uint)NativeMemoryAllocator.AlignOf<T>());
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }

                return new NativeArray<T>((T*)ptr, _length);
            }

            /// <summary>
            ///     Return buffer
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