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
    [NativeCollection(FromType.None)]
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
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            _buffer = NativeMemoryAllocator.AlignedAllocZeroed<int>((uint)(2 + capacity));
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
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
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
                return Unsafe.AsRef<int>(buffer) - Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)1) == _length;
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
                return Unsafe.AsRef<int>(buffer) - Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)1);
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
                return _length - (Unsafe.AsRef<int>(buffer) - Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)1));
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
            NativeMemoryAllocator.AlignedFree(buffer);
        }

        /// <summary>
        ///     Try rent
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Rented</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out int index)
        {
            var spinWait = new NativeSpinWait();
            var buffer = _buffer;
            ref var location = ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)1);
            var id = location - 1;
            while (id >= 0 && Interlocked.CompareExchange(ref location, id, id + 1) != id + 1)
            {
                id = location - 1;
                spinWait.SpinOnce(-1);
            }

            if (id >= 0)
            {
                spinWait.Reset();
                int value;
                location = ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)(2 + id));
                do
                {
                    value = Interlocked.Exchange(ref location, 0);
                    spinWait.SpinOnce(-1);
                } while (value == 0);

                index = value - 1;
                return true;
            }

            location = ref Unsafe.AsRef<int>(buffer);
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
            var spinWait = new NativeSpinWait();
            var buffer = _buffer;
            var id = Interlocked.Increment(ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)1)) - 1;
            ref var location = ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer), (nint)(id + 2));
            var value = index + 1;
            while (Interlocked.CompareExchange(ref location, value, 0) != 0)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentFixedSizeBucket Empty => new();
    }
}