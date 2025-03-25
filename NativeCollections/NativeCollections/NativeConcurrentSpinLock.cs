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
    ///     Native concurrent spinLock
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    [NativeCollection(NativeCollectionType.None)]
    public struct NativeConcurrentSpinLock
    {
        /// <summary>
        ///     Sequence number
        /// </summary>
        [FieldOffset(0)] private volatile int _sequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        [FieldOffset(4)] private volatile int _nextSequenceNumber;

        /// <summary>
        ///     Sequence number
        /// </summary>
        public int SequenceNumber => _sequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        public int NextSequenceNumber => _nextSequenceNumber;

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _sequenceNumber = 0;
            _nextSequenceNumber = 1;
        }

        /// <summary>
        ///     Acquire
        /// </summary>
        /// <returns>Sequence number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => Interlocked.Add(ref _sequenceNumber, 1);

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber)
        {
            var spinWait = new NativeSpinWait();
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce();
        }

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber, int sleepThreshold)
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
            var sequenceNumber = Interlocked.Add(ref _sequenceNumber, 1);
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce();
        }

        /// <summary>
        ///     Enter
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleepThreshold)
        {
            var spinWait = new NativeSpinWait();
            var sequenceNumber = Interlocked.Add(ref _sequenceNumber, 1);
            while (sequenceNumber != _nextSequenceNumber)
                spinWait.SpinOnce(sleepThreshold);
        }

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Add(ref _nextSequenceNumber, 1);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentSpinLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => throw new NotSupportedException("Cannot call Equals on NativeConcurrentSpinLock");

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => throw new NotSupportedException("Cannot call GetHashCode on NativeConcurrentSpinLock");

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeConcurrentSpinLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => Unsafe.As<NativeConcurrentSpinLock, long>(ref left) == Unsafe.As<NativeConcurrentSpinLock, long>(ref right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => Unsafe.As<NativeConcurrentSpinLock, long>(ref left) != Unsafe.As<NativeConcurrentSpinLock, long>(ref right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentSpinLock Empty => new();
    }
}