using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_2021_3_OR_NEWER || GODOT
using System;
using System.Threading;
#endif

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentBucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeConcurrentBucket : IDisposable, IEquatable<NativeConcurrentBucket>
    {
        /// <summary>
        ///     Array
        /// </summary>
        private readonly int* _array;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentBucket(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            _array = (int*)NativeMemoryAllocator.AllocZeroed((uint)((2 + capacity) * sizeof(int)));
            _length = capacity;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _array != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _array[0] - _array[1] == _length;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Remaining
        /// </summary>
        public int Remaining => _length - (_array[0] - _array[1]);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentBucket other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentBucket nativeConcurrentBucket && nativeConcurrentBucket == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => (int)(nint)_array;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentBucket[{_length}]";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentBucket left, NativeConcurrentBucket right) => left._array == right._array;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentBucket left, NativeConcurrentBucket right) => left._array != right._array;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (_array == null)
                return;
            NativeMemoryAllocator.Free(_array);
        }

        /// <summary>
        ///     Try rent
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Rented</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out int index)
        {
            var count = 0;
            var array = _array;
            ref var segment = ref array[1];
            var id = segment - 1;
            while (id >= 0 && Interlocked.CompareExchange(ref segment, id, id + 1) != id + 1)
            {
                id = segment - 1;
                if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                {
                    var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                    if (yieldsSoFar % 5 == 4)
                        Thread.Sleep(0);
                    else
                        Thread.Yield();
                }
                else
                {
                    var iterations = Environment.ProcessorCount / 2;
                    if (count <= 30 && 1 << count < iterations)
                        iterations = 1 << count;
                    Thread.SpinWait(iterations);
                }

                count = count == int.MaxValue ? 10 : count + 1;
            }

            if (id >= 0)
            {
                count = 0;
                var value = 0;
                segment = ref array[2 + id];
                while (value == 0)
                {
                    value = Interlocked.Exchange(ref segment, 0);
                    if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                    {
                        var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                        if (yieldsSoFar % 5 == 4)
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                    else
                    {
                        var iterations = Environment.ProcessorCount / 2;
                        if (count <= 30 && 1 << count < iterations)
                            iterations = 1 << count;
                        Thread.SpinWait(iterations);
                    }

                    count = count == int.MaxValue ? 10 : count + 1;
                }

                index = value - 1;
                return true;
            }

            segment = ref array[0];
            id = Interlocked.Increment(ref segment) - 1;
            if (id >= _length)
            {
                Interlocked.Decrement(ref segment);
                index = -1;
                return false;
            }

            index = id;
            return true;
        }

        /// <summary>
        ///     Return
        /// </summary>
        /// <param name="index">Index</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(int index)
        {
            var count = 0;
            var array = _array;
            var id = Interlocked.Increment(ref array[1]) - 1;
            ref var segment = ref array[id + 2];
            var value = index + 1;
            while (Interlocked.CompareExchange(ref segment, value, 0) != 0)
            {
                if ((count >= 10 && (count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
                {
                    var yieldsSoFar = count >= 10 ? (count - 10) / 2 : count;
                    if (yieldsSoFar % 5 == 4)
                        Thread.Sleep(0);
                    else
                        Thread.Yield();
                }
                else
                {
                    var iterations = Environment.ProcessorCount / 2;
                    if (count <= 30 && 1 << count < iterations)
                        iterations = 1 << count;
                    Thread.SpinWait(iterations);
                }

                count = count == int.MaxValue ? 10 : count + 1;
            }
        }
    }
}