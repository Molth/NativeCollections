using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe concurrent spinLock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeConcurrentSpinLock : IDisposable, IEquatable<UnsafeConcurrentSpinLock>
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
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Exit();

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _sequenceNumber = 0;
            _nextSequenceNumber = 0;
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public UnsafeDisposable<UnsafeConcurrentSpinLock> EnterLock()
        {
            Enter();
            return new UnsafeDisposable<UnsafeConcurrentSpinLock>(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public UnsafeDisposable<UnsafeConcurrentSpinLock> EnterLock(int sequenceNumber)
        {
            Enter(sequenceNumber);
            return new UnsafeDisposable<UnsafeConcurrentSpinLock>(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Acquire
        /// </summary>
        /// <returns>Sequence number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => Interlocked.Increment(ref _sequenceNumber) - 1;

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber)
        {
            var spinWait = new UnsafeSpinWait();
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
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
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var spinWait = new UnsafeSpinWait();
            var sequenceNumber = Interlocked.Increment(ref _sequenceNumber) - 1;
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleep1Threshold)
        {
            var spinWait = new UnsafeSpinWait();
            var sequenceNumber = Interlocked.Increment(ref _sequenceNumber) - 1;
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Increment(ref _nextSequenceNumber);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeConcurrentSpinLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentSpinLock Empty => new();
    }
}