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
        public NativeDisposable<UnsafeConcurrentSpinLock> EnterLock()
        {
            Enter();
            return new NativeDisposable<UnsafeConcurrentSpinLock>((UnsafeConcurrentSpinLock*)Unsafe.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentSpinLock> EnterLock(int sequenceNumber)
        {
            Enter(sequenceNumber);
            return new NativeDisposable<UnsafeConcurrentSpinLock>((UnsafeConcurrentSpinLock*)Unsafe.AsPointer(ref this));
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
            var spinWait = new NativeSpinWait();
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Wait(int sequenceNumber, int sleepThreshold)
        {
            var spinWait = new NativeSpinWait();
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(sleepThreshold);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter()
        {
            var spinWait = new NativeSpinWait();
            var sequenceNumber = Interlocked.Increment(ref _sequenceNumber) - 1;
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleepThreshold)
        {
            var spinWait = new NativeSpinWait();
            var sequenceNumber = Interlocked.Increment(ref _sequenceNumber) - 1;
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(sleepThreshold);
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
        public readonly bool Equals(UnsafeConcurrentSpinLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentSpinLock unsafeConcurrentSpinLock && unsafeConcurrentSpinLock == this;

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
        public static bool operator ==(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right)
        {
            ref var local1 = ref Unsafe.As<UnsafeConcurrentSpinLock, int>(ref left);
            ref var local2 = ref Unsafe.As<UnsafeConcurrentSpinLock, int>(ref right);
            return local1 == local2 && Unsafe.Add(ref local1, (nint)1) == Unsafe.Add(ref local2, (nint)1);
        }

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentSpinLock left, UnsafeConcurrentSpinLock right)
        {
            ref var local1 = ref Unsafe.As<UnsafeConcurrentSpinLock, int>(ref left);
            ref var local2 = ref Unsafe.As<UnsafeConcurrentSpinLock, int>(ref right);
            return local1 != local2 || Unsafe.Add(ref local1, (nint)1) != Unsafe.Add(ref local2, (nint)1);
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentSpinLock Empty => new();
    }
}