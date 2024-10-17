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
    ///     Native concurrentFixedSizeBucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection]
    public readonly unsafe struct NativeConcurrentFixedSizeBucket : IDisposable, IEquatable<NativeConcurrentFixedSizeBucket>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly int* _buffer;

        /// <summary>
        ///     Length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentFixedSizeBucket(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            _buffer = (int*)NativeMemoryAllocator.AllocZeroed((uint)((2 + capacity) * sizeof(int)));
            _length = capacity;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentFixedSizeBucket(int* buffer, int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            _buffer = buffer;
            _length = capacity;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                var buffer = _buffer;
                return buffer[0] - buffer[1] == _length;
            }
        }

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            get
            {
                var buffer = _buffer;
                return buffer[0] - buffer[1];
            }
        }

        /// <summary>
        ///     Remaining
        /// </summary>
        public int Remaining
        {
            get
            {
                var buffer = _buffer;
                return _length - (buffer[0] - buffer[1]);
            }
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentFixedSizeBucket other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentFixedSizeBucket nativeConcurrentFixedSizeBucket && nativeConcurrentFixedSizeBucket == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_buffer).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => $"NativeConcurrentFixedSizeBucket[{_length}]";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => left._buffer == right._buffer;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => left._buffer != right._buffer;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (buffer == null)
                return;
            NativeMemoryAllocator.Free(buffer);
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
            var buffer = _buffer;
            ref var location = ref buffer[1];
            var id = location - 1;
            while (id >= 0 && Interlocked.CompareExchange(ref location, id, id + 1) != id + 1)
            {
                id = location - 1;
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
                location = ref buffer[2 + id];
                while (value == 0)
                {
                    value = Interlocked.Exchange(ref location, 0);
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

            location = ref buffer[0];
            id = Interlocked.Increment(ref location) - 1;
            if (id >= _length)
            {
                Interlocked.Decrement(ref location);
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
            var buffer = _buffer;
            var id = Interlocked.Increment(ref buffer[1]) - 1;
            ref var location = ref buffer[id + 2];
            var value = index + 1;
            while (Interlocked.CompareExchange(ref location, value, 0) != 0)
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