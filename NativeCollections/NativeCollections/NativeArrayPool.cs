#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Threading;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Numerics;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable PossibleNullReferenceException
// ReSharper disable MemberHidesStaticFromOuterClass

namespace NativeCollections
{
    /// <summary>
    ///     NativeMemoryPool
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct NativeArrayPool<T> : IDisposable, IEquatable<NativeArrayPool<T>> where T : unmanaged
    {
        /// <summary>
        ///     Buckets
        /// </summary>
        private readonly NativeArrayPoolBucket** _buckets;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Size
        /// </summary>
        private readonly int _size;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="size">Size</param>
        /// <param name="maxLength">Max length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArrayPool(int size, int maxLength)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "MustBePositive");
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "MustBeNonNegative");
            if (maxLength > 1073741824)
                maxLength = 1073741824;
            else if (maxLength < 16)
                maxLength = 16;
            var length = SelectBucketIndex(maxLength) + 1;
            var buckets = (NativeArrayPoolBucket**)NativeMemoryAllocator.Alloc(length * sizeof(NativeArrayPoolBucket*));
            for (var i = 0; i < length; ++i)
            {
                var bucket = (NativeArrayPoolBucket*)NativeMemoryAllocator.Alloc(sizeof(NativeArrayPoolBucket));
                bucket->Initialize(size, 16 << i);
                buckets[i] = bucket;
            }

            _buckets = buckets;
            _length = length;
            _size = size;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buckets != null;

        /// <summary>
        ///     Size
        /// </summary>
        public int Size => _size;

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
        public override int GetHashCode() => (int)(nint)_buckets;

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
            if (_buckets == null)
                return;
            for (var i = 0; i < _length; ++i)
                _buckets[i]->Dispose();
            NativeMemoryAllocator.Free(_buckets);
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
                return _buckets[index]->Rent();
            throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "BiggerThanCollection");
        }

        /// <summary>
        ///     Return buffer
        /// </summary>
        /// <param name="array">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(in NativeArray<T> array)
        {
            var length = array.Length;
            if (length < 16 || (length & (length - 1)) != 0)
                throw new ArgumentException("BufferNotFromPool", nameof(array));
            var bucket = SelectBucketIndex(length);
            if (bucket >= _length)
                throw new ArgumentException("BufferNotFromPool", nameof(array));
            _buckets[bucket]->Return(length, array.Array);
        }

        /// <summary>
        ///     Select bucket index
        /// </summary>
        /// <param name="bufferSize">Buffer size</param>
        /// <returns>Bucket index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SelectBucketIndex(int bufferSize)
        {
#if NET5_0_OR_GREATER
            var value = (bufferSize - 1) | 15;
            return BitOperations.Log2(Unsafe.As<int, uint>(ref value)) - 3;
#else
            var value = (bufferSize - 1) | 15 | 1;
            var count = 0;
            if ((value & -65536) == 0)
            {
                count += 16;
                value <<= 16;
            }

            if ((value & -16777216) == 0)
            {
                count += 8;
                value <<= 8;
            }

            if ((value & -268435456) == 0)
            {
                count += 4;
                value <<= 4;
            }

            if ((value & -1073741824) == 0)
            {
                count += 2;
                value <<= 2;
            }

            if ((value & -2147483648) == 0)
                ++count;
            return (31 ^ count) - 3;
#endif
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeArrayPool<T> Empty => new();

        /// <summary>
        ///     NativeArrayPool bucket
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeArrayPoolBucket : IDisposable
        {
            /// <summary>
            ///     Size
            /// </summary>
            private int _size;

            /// <summary>
            ///     Length
            /// </summary>
            private int _length;

            /// <summary>
            ///     Buffers
            /// </summary>
            private T** _array;

            /// <summary>
            ///     Index
            /// </summary>
            private int _index;

            /// <summary>
            ///     Memory pool
            /// </summary>
            private NativeMemoryPool _memoryPool;

            /// <summary>
            ///     State lock
            /// </summary>
            private SpinLock _lock;

            /// <summary>
            ///     Structure
            /// </summary>
            /// <param name="size">Size</param>
            /// <param name="length">Length</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Initialize(int size, int length)
            {
                _size = size;
                _length = length;
                _array = (T**)NativeMemoryAllocator.AllocZeroed(size * sizeof(T*));
                _index = 0;
                _memoryPool = new NativeMemoryPool(size, length * sizeof(T), 0);
                _lock = new SpinLock();
            }

            /// <summary>
            ///     Dispose
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                NativeMemoryAllocator.Free(_array);
                _memoryPool.Dispose();
            }

            /// <summary>
            ///     Rent buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NativeArray<T> Rent()
            {
                T* buffer = null;
                var lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_index < _size)
                    {
                        buffer = _array[_index];
                        _array[_index++] = null;
                    }

                    if (buffer == null)
                        buffer = (T*)_memoryPool.Rent();
                }
                finally
                {
                    if (lockTaken)
                        _lock.Exit(false);
                }

                return new NativeArray<T>(buffer, _length);
            }

            /// <summary>
            ///     Return buffer
            /// </summary>
            /// <param name="length">Length</param>
            /// <param name="ptr">Pointer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Return(int length, T* ptr)
            {
                var lockTaken = false;
                try
                {
                    _lock.Enter(ref lockTaken);
                    if (_index != 0)
                        _array[--_index] = ptr;
                    else
                        _memoryPool.Return(ptr);
                }
                finally
                {
                    if (lockTaken)
                        _lock.Exit(false);
                }
            }
        }
    }
}