using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a lock that is used to manage access to a resource,
    ///     allowing multiple threads for reading or exclusive access for writing.
    /// </summary>
    /// <remarks>
    ///     This lock does not verify thread ownership, and write acquisition is not
    ///     reentrant: calling <see cref="EnterWrite(int)" /> while the same thread already
    ///     holds a read or write lock waits forever, because the ticket it already holds
    ///     is not released.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeConcurrentReaderWriterLock : IEquatable<UnsafeConcurrentReaderWriterLock>
    {
        /// <summary>
        ///     Internal spin lock used to synchronize updates to the shared state.
        /// </summary>
        private UnsafeConcurrentSpinLock _spinLock;

        /// <summary>
        ///     Indicates whether the last published ticket is a write ticket.
        /// </summary>
        private bool _isWriter;

        /// <summary>
        ///     Sequence number captured at the time of acquiring a read
        ///     lock, used to wait for any pending write lock to complete.
        /// </summary>
        private uint _readSequenceNumber;

        /// <summary>
        ///     Monotonically increasing sequence number that changes on every lock acquisition.
        /// </summary>
        private uint _sequenceNumber;

        /// <summary>
        ///     Sequence number of the last released lock, used to determine when a lock can be acquired.
        /// </summary>
        private uint _nextSequenceNumber;

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset()
        {
            _spinLock.Reset();
            _isWriter = false;
            _readSequenceNumber = 0U;
            _sequenceNumber = 0U;
            _nextSequenceNumber = 0U;
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockScope EnterReadScope() => EnterReadScope(-1);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that releases the lock when disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockScope EnterReadScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return NativeConcurrentReaderWriterLockScope.From(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockScope EnterWriteScope() => EnterWriteScope(-1);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that releases the lock when disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockScope EnterWriteScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return NativeConcurrentReaderWriterLockScope.From(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope() => EnterReadRefScope(-1);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that releases the lock when disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return NativeConcurrentReaderWriterLockRefScope.From(NativeRef<UnsafeConcurrentReaderWriterLock>.From(ref this));
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterWriteRefScope() => EnterWriteRefScope(-1);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that releases the lock when disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterWriteRefScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return NativeConcurrentReaderWriterLockRefScope.From(NativeRef<UnsafeConcurrentReaderWriterLock>.From(ref this));
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead() => EnterRead(-1);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     The threshold is validated before the reader ticket is published. Otherwise an
        ///     invalid argument under contention would throw after the ticket had been issued,
        ///     leaving <see cref="_nextSequenceNumber" /> permanently behind and blocking all
        ///     later lock acquisitions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            _spinLock.Enter();
            var wasWriter = _isWriter;
            _isWriter = false;
            if (wasWriter)
                _readSequenceNumber = _sequenceNumber;

            var readSequenceNumber = _readSequenceNumber;
            _sequenceNumber = _sequenceNumber.WrappingAdd(1U);
            _spinLock.Exit();
            UnsafeSpinWait spinWait = default;
            while (I32.FromU32Unchecked(Volatile.Read(ref _nextSequenceNumber).WrappingSub(readSequenceNumber)) < 0)
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite() => EnterWrite(-1);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     The threshold is validated before the writer ticket is published. Otherwise an
        ///     invalid argument under contention would throw after the ticket had been issued,
        ///     leaving <see cref="_nextSequenceNumber" /> permanently behind and blocking all
        ///     later lock acquisitions.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            _spinLock.Enter();
            _isWriter = true;
            var sequenceNumber = _sequenceNumber;
            _sequenceNumber = _sequenceNumber.WrappingAdd(1U);
            _spinLock.Exit();
            UnsafeSpinWait spinWait = default;
            while (sequenceNumber != Volatile.Read(ref _nextSequenceNumber))
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Increment(ref Unsafe.As<uint, int>(ref _nextSequenceNumber));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeConcurrentReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => !left.Equals(right);
    }
}