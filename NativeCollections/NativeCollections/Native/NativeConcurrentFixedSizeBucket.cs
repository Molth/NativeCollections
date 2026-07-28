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
        ///     Gets the total numbers of elements the internal data structure can hold.
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
            _buffer = UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _length = capacity;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <value>
        ///     true if this is empty;
        ///     otherwise, false.
        /// </value>
        public bool IsEmpty => Remaining == 0;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _length;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => Unsafe.AsRef<int>(_buffer) - Unsafe.Add(ref Unsafe.AsRef<int>(_buffer), (nint)1);

        /// <summary>
        ///     Gets the number of remaining free slots available in the bucket.
        /// </summary>
        public int Remaining => _length - Count;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeConcurrentFixedSizeBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeConcurrentFixedSizeBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeConcurrentFixedSizeBucket[{0}]", _length);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_buffer);

        /// <summary>
        ///     Attempts to retrieve a object.
        /// </summary>
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
            if ((uint)id >= (uint)_length)
            {
                Interlocked.Decrement(ref location);
                index = -1;
                return false;
            }

            index = id;
            return true;
        }

        /// <summary>
        ///     Returns to the pool an index that was previously obtained via <see cref="TryRent" /> on the same instance.
        /// </summary>
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
        public static NativeConcurrentFixedSizeBucket Empty => default;
    }
}