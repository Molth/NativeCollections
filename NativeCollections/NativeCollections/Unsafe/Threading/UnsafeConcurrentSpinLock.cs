using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a lock that uses an atomic sequence number to manage exclusive access to a resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeConcurrentSpinLock : IEquatable<UnsafeConcurrentSpinLock>
    {
        /// <summary>
        ///     The current sequence number, incremented each time a lock is acquired.
        /// </summary>
        private volatile int _sequenceNumber;

        /// <summary>
        ///     The sequence number of the next ticket to be released. The lock is free
        ///     when this equals <see cref="_sequenceNumber" />.
        /// </summary>
        private volatile int _nextSequenceNumber;

        /// <summary>
        ///     Gets the current sequence number.
        /// </summary>
        public readonly int SequenceNumber => _sequenceNumber;

        /// <summary>
        ///     Gets the sequence number of the next ticket to be released.
        /// </summary>
        public readonly int NextSequenceNumber => _nextSequenceNumber;

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset()
        {
            _sequenceNumber = 0;
            _nextSequenceNumber = 0;
        }

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <returns>A scope that holds the lock until it is disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentSpinLockScope EnterScope() => EnterScope(-1);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that holds the lock until it is disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentSpinLockScope EnterScope(int sleep1Threshold)
        {
            Enter(sleep1Threshold);
            return NativeConcurrentSpinLockScope.From(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <returns>A reference scope that holds the lock until it is disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope() => EnterRefScope(-1);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A reference scope that holds the lock until it is disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope(int sleep1Threshold)
        {
            Enter(sleep1Threshold);
            return NativeConcurrentSpinLockRefScope.From(NativeRef<UnsafeConcurrentSpinLock>.From(ref this));
        }

        /// <summary>
        ///     Atomically acquires the current sequence number.
        /// </summary>
        /// <returns>The current sequence number at the time of acquisition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => Interlocked.Increment(ref _sequenceNumber).WrappingSub(1);

        /// <summary>
        ///     Spins until the lock's released sequence number reaches the specified value.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber) => Wait(sequenceNumber, -1);

        /// <summary>
        ///     Spins until the lock's released sequence number reaches
        ///     the specified value, with configurable sleep behavior.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     The threshold is validated before the first spin, so an invalid argument
        ///     never starts spinning. A ticket already obtained from <see cref="Acquire" />
        ///     is not released automatically, however, and must still be exited.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber, int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            UnsafeSpinWait spinWait = default;
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => Enter(-1);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     The threshold is validated before <see cref="Acquire" /> is called. Otherwise an
        ///     invalid argument under contention would throw after a ticket had been issued,
        ///     leaving <see cref="_nextSequenceNumber" /> permanently behind and deadlocking
        ///     every later <see cref="Enter()" /> call.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            Wait(Acquire(), sleep1Threshold);
        }

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Increment(ref _nextSequenceNumber);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeConcurrentSpinLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => !left.Equals(right);
    }
}