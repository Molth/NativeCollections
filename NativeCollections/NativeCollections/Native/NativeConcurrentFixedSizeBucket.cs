using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrentFixedSizeBucket
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeConcurrentFixedSizeBucket : IIsCreated, IDisposable, IEquatable<NativeConcurrentFixedSizeBucket>
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
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            _buffer = NativeMemoryAllocator.AlignedAllocZeroed<int>((uint)(2 + capacity));
            _length = capacity;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="capacity">Capacity</param>
        [MustBeZeroed(nameof(buffer))]
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentFixedSizeBucket([MustBeZeroed] [MustBePinned] Span<int> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfLessThan((uint)buffer.Length, (uint)(2 + capacity), ExceptionArgument.buffer);
            _buffer = (int*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _length = capacity;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => Remaining == 0;

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => Unsafe.AsRef<int>(_buffer) - Unsafe.Add(ref Unsafe.AsRef<int>(_buffer), (nint)1);

        /// <summary>
        ///     Remaining
        /// </summary>
        public int Remaining => _length - Count;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentFixedSizeBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentFixedSizeBucket other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => SR.Format("NativeConcurrentFixedSizeBucket[{0}]", _length);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => !left.Equals(right);

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var buffer = _buffer;
            if (UnsafeHelpers.IsNull(buffer))
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
            var spinWait = new UnsafeSpinWait();
            ref var buffer = ref Unsafe.AsRef<int>(_buffer);
            ref var location = ref Unsafe.Add(ref buffer, (nint)1);
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
                location = ref Unsafe.Add(ref buffer, (nint)(2 + id));
                do
                {
                    value = Interlocked.Exchange(ref location, 0);
                    spinWait.SpinOnce(-1);
                } while (value == 0);

                index = value - 1;
                return true;
            }

            location = ref buffer;
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
            var spinWait = new UnsafeSpinWait();
            ref var buffer = ref Unsafe.AsRef<int>(_buffer);
            var id = Interlocked.Increment(ref Unsafe.Add(ref buffer, (nint)1)) - 1;
            ref var location = ref Unsafe.Add(ref buffer, (nint)(2 + id));
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