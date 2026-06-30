using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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
        [MustBePinned(SR.parameter_this)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock()
        {
            Read();
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Read
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
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock(int sleep1Threshold)
        {
            Read(sleep1Threshold);
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock()
        {
            Write();
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Write
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
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock(int sleep1Threshold)
        {
            Write(sleep1Threshold);
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(UnsafeHelpers.AsPointer(ref this));
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
            var spinWait = new UnsafeSpinWait();
            while ((int)(Volatile.Read(ref _nextSequenceNumber) - readSequenceNumber) < 0)
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(int sleep1Threshold)
        {
            _spinLock.Enter();
            var state = _readWriteState;
            _readWriteState = 0;
            if (state != 0)
                _readSequenceNumber = _sequenceNumber;
            var readSequenceNumber = _readSequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new UnsafeSpinWait();
            while ((int)(Volatile.Read(ref _nextSequenceNumber) - readSequenceNumber) < 0)
                spinWait.SpinOnce(sleep1Threshold);
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
            var spinWait = new UnsafeSpinWait();
            while (sequenceNumber != Volatile.Read(ref _nextSequenceNumber))
                spinWait.SpinOnce(-1);
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int sleep1Threshold)
        {
            _spinLock.Enter();
            _readWriteState = 1;
            var sequenceNumber = _sequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new UnsafeSpinWait();
            while (sequenceNumber != Volatile.Read(ref _nextSequenceNumber))
                spinWait.SpinOnce(sleep1Threshold);
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
        public readonly bool Equals(UnsafeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentReaderWriterLock other && other.Equals(this);

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
        public static bool operator ==(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentReaderWriterLock Empty => new();
    }
}