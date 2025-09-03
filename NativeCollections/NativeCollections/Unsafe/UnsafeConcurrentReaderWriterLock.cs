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
    ///     Unsafe concurrent reader writer lock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeConcurrentReaderWriterLock : IDisposable, IEquatable<UnsafeConcurrentReaderWriterLock>
    {
        /// <summary>
        ///     Spin lock
        /// </summary>
        private UnsafeConcurrentSpinLock _spinLock;

        /// <summary>
        ///     Read write state
        /// </summary>
        private uint _readWriteState;

        /// <summary>
        ///     Read sequence number
        /// </summary>
        private uint _readSequenceNumber;

        /// <summary>
        ///     Sequence number
        /// </summary>
        private uint _sequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        private uint _nextSequenceNumber;

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
            _spinLock.Reset();
            _readWriteState = 0;
            _readSequenceNumber = 0;
            _sequenceNumber = 0;
            _nextSequenceNumber = 0;
        }

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock()
        {
            Read();
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref this));
        }

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock(int sleepThreshold)
        {
            Read(sleepThreshold);
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref this));
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock()
        {
            Write();
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref this));
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock(int sleepThreshold)
        {
            Write(sleepThreshold);
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref this));
        }

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read()
        {
            _spinLock.Enter();
            var state = _readWriteState;
            _readWriteState = 0;
            if (state != 0)
                _readSequenceNumber = _sequenceNumber;
            var readSequenceNumber = _readSequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new NativeSpinWait();
            while ((int)(Volatile.Read(ref _nextSequenceNumber) - readSequenceNumber) < 0)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(int sleepThreshold)
        {
            _spinLock.Enter();
            var state = _readWriteState;
            _readWriteState = 0;
            if (state != 0)
                _readSequenceNumber = _sequenceNumber;
            var readSequenceNumber = _readSequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new NativeSpinWait();
            while ((int)(Volatile.Read(ref _nextSequenceNumber) - readSequenceNumber) < 0)
                spinWait.SpinOnce(sleepThreshold);
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write()
        {
            _spinLock.Enter();
            _readWriteState = 1;
            var sequenceNumber = _sequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new NativeSpinWait();
            while (sequenceNumber != Volatile.Read(ref _nextSequenceNumber))
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int sleepThreshold)
        {
            _spinLock.Enter();
            _readWriteState = 1;
            var sequenceNumber = _sequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new NativeSpinWait();
            while (sequenceNumber != Volatile.Read(ref _nextSequenceNumber))
                spinWait.SpinOnce(sleepThreshold);
        }

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => Interlocked.Increment(ref Unsafe.As<uint, int>(ref _nextSequenceNumber));

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(UnsafeConcurrentReaderWriterLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentReaderWriterLock unsafeConcurrentReaderWriterLock && unsafeConcurrentReaderWriterLock == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeConcurrentReaderWriterLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right)
        {
            ref var local1 = ref Unsafe.As<UnsafeConcurrentReaderWriterLock, byte>(ref left);
            ref var local2 = ref Unsafe.As<UnsafeConcurrentReaderWriterLock, byte>(ref right);
            return SpanHelpers.Compare(ref local1, ref local2, (nuint)sizeof(UnsafeConcurrentReaderWriterLock));
        }

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right)
        {
            ref var local1 = ref Unsafe.As<UnsafeConcurrentReaderWriterLock, byte>(ref left);
            ref var local2 = ref Unsafe.As<UnsafeConcurrentReaderWriterLock, byte>(ref right);
            return !SpanHelpers.Compare(ref local1, ref local2, (nuint)sizeof(UnsafeConcurrentReaderWriterLock));
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentReaderWriterLock Empty => new();
    }
}