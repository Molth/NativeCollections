using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

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
        ///     Sequence number
        /// </summary>
        private volatile int _sequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        private volatile int _nextSequenceNumber;

        /// <summary>
        ///     Sequence number
        /// </summary>
        public readonly int SequenceNumber => _sequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        public readonly int NextSequenceNumber => _nextSequenceNumber;

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _sequenceNumber = 0;
            _nextSequenceNumber = 0;
        }

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentSpinLockScope EnterScope() => EnterScope(-1);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentSpinLockScope EnterScope(int sleep1Threshold)
        {
            Enter(sleep1Threshold);
            return new NativeConcurrentSpinLockScope(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope() => EnterRefScope(-1);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope(int sleep1Threshold)
        {
            Enter(sleep1Threshold);
            return new NativeConcurrentSpinLockRefScope(NativeRef<UnsafeConcurrentSpinLock>.Create(ref this));
        }

        /// <summary>
        ///     Atomically acquires the current sequence number.
        /// </summary>
        /// <returns>The current sequence number at the time of acquisition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => Interlocked.Increment(ref _sequenceNumber) - 1;

        /// <summary>
        ///     Spins until the lock's sequence number advances beyond the specified value.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber) => Wait(sequenceNumber, -1);

        /// <summary>
        ///     Spins until the lock's sequence number advances beyond the specified value, with configurable sleep behavior.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber, int sleep1Threshold)
        {
            var spinWait = new UnsafeSpinWait();
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
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleep1Threshold) => Wait(Acquire(), sleep1Threshold);

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Increment(ref _nextSequenceNumber);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeConcurrentSpinLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentSpinLock Empty => default;
    }
}