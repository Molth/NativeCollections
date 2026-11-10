using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a thread-safe allocator that rents and returns the indices of fixed-size slots.
    /// </summary>
    /// <remarks>
    ///     The bucket does not store objects. <see cref="TryRent" /> hands out slot indices in the range
    ///     [0, <see cref="Capacity" />), and <see cref="Return" /> makes a slot index available again.
    ///     The underlying storage holds two additional metadata slots beyond the capacity.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeConcurrentFixedSizeBucket : IIsCreated, IDisposable, IEquatable<NativeConcurrentFixedSizeBucket>
    {
        /// <summary>
        ///     The underlying storage: element 0 holds the number of slot indices handed out so far,
        ///     element 1 holds the number of free slots and acts as the free-list head, and the
        ///     remaining elements hold the free-list entries.
        /// </summary>
        private readonly int* _buffer;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Initializes an instance from its final storage state.
        /// </summary>
        /// <param name="buffer">The backing integer buffer.</param>
        /// <param name="capacity">The number of elements the bucket can hold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentFixedSizeBucket(int* buffer, int capacity)
        {
            _buffer = buffer;
            _capacity = capacity;
        }

        /// <summary>
        ///     Creates a fixed-size bucket with zeroed storage.
        /// </summary>
        /// <param name="capacity">The number of elements the bucket can hold.</param>
        /// <returns>A new fixed-size bucket.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentFixedSizeBucket Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            var buffer = NativeMemoryAllocator.AlignedAllocZeroed<int>(Usize.FromI32Unchecked(capacity).WrappingAdd(Usize.FromI32Unchecked(2)));
            return new NativeConcurrentFixedSizeBucket(buffer, capacity);
        }

        /// <summary>
        ///     Initializes a new instance of this class that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">
        ///     The byte buffer to use as underlying storage. It must be large enough
        ///     to store the specified number of elements with proper alignment.
        /// </param>
        /// <param name="capacity">
        ///     The maximum number of elements the bucket can hold. Must be non-negative.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentFixedSizeBucket([MustBeZeroed] [MustBePinned] Span<int> buffer, int capacity)
        {
            _buffer = UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _capacity = capacity;
        }

        /// <summary>
        ///     Wraps an external zeroed integer buffer as a fixed-size bucket.
        /// </summary>
        /// <param name="buffer">The pinned zeroed integer buffer to wrap.</param>
        /// <param name="capacity">The number of elements the bucket can hold.</param>
        /// <returns>A fixed-size bucket that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative or <paramref name="buffer" /> has a length
        ///     less than 2 + <paramref name="capacity" /> (two metadata slots followed by the slot storage).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentFixedSizeBucket From([MustBeZeroed] [MustBePinned] Span<int> buffer, int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            ThrowHelpers.ThrowIfLessThan(U32.FromI32Unchecked(buffer.Length), U32.FromI32Unchecked(2.WrappingAdd(capacity)), ExceptionArgument.buffer);
            return new NativeConcurrentFixedSizeBucket(buffer, capacity);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <remarks>
        ///     The result is derived from <see cref="Count" /> and is only an observation;
        ///     it may be stale immediately when other threads are renting or returning slots.
        /// </remarks>
        public bool IsEmpty => Count == 0;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold.
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        ///     Gets the number of elements contained in this.
        /// </summary>
        public int Count => Unsafe.AsRef<int>(_buffer).WrappingSub(Unsafe.Add(ref Unsafe.AsRef<int>(_buffer), Isize.FromI32Unchecked(1)));

        /// <summary>
        ///     Gets the number of remaining free slots available in the bucket.
        /// </summary>
        public int Remaining => _capacity.WrappingSub(Count);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeConcurrentFixedSizeBucket other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentFixedSizeBucket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeConcurrentFixedSizeBucket[{0}]", _capacity);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeConcurrentFixedSizeBucket left, NativeConcurrentFixedSizeBucket right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Attempts to rent a free slot.
        /// </summary>
        /// <param name="index">
        ///     When this method returns, contains the index of the rented slot, or -1 if no slot was available.
        /// </param>
        /// <returns><see langword="true" /> if a slot was rented; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRent(out int index)
        {
            UnsafeSpinWait spinWait = default;
            ref var buffer = ref Unsafe.AsRef<int>(_buffer);
            ref var location = ref Unsafe.Add(ref buffer, Isize.FromI32Unchecked(1));
            var id = location.WrappingSub(1);
            while (id >= 0 && Interlocked.CompareExchange(ref location, id, id.WrappingAdd(1)) != id.WrappingAdd(1))
            {
                id = location.WrappingSub(1);
                spinWait.SpinOnce(-1);
            }

            if (id >= 0)
            {
                spinWait.Reset();
                int value;
                location = ref Unsafe.Add(ref buffer, Isize.FromI32Unchecked(2.WrappingAdd(id)));
                while (true)
                {
                    value = Interlocked.Exchange(ref location, 0);
                    spinWait.SpinOnce(-1);

                    if (value != 0)
                        break;
                }

                index = value.WrappingSub(1);
                return true;
            }

            location = ref buffer;
            id = Interlocked.Increment(ref location).WrappingSub(1);
            if (U32.FromI32Unchecked(id) >= U32.FromI32Unchecked(_capacity))
            {
                Interlocked.Decrement(ref location);
                index = -1;
                return false;
            }

            index = id;
            return true;
        }

        /// <summary>
        ///     Returns a rented slot index to this bucket. The index must have been
        ///     obtained via <see cref="TryRent" /> on the same instance.
        /// </summary>
        /// <param name="index">The index of the slot to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(int index)
        {
            UnsafeSpinWait spinWait = default;
            ref var buffer = ref Unsafe.AsRef<int>(_buffer);
            var id = Interlocked.Increment(ref Unsafe.Add(ref buffer, Isize.FromI32Unchecked(1))).WrappingSub(1);
            ref var location = ref Unsafe.Add(ref buffer, Isize.FromI32Unchecked(2.WrappingAdd(id)));
            var value = index.WrappingAdd(1);
            while (Interlocked.CompareExchange(ref location, value, 0) != 0)
                spinWait.SpinOnce(-1);
        }
    }
}