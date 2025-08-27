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
    public struct UnsafeConcurrentReaderWriterLock
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
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentReaderWriterLock Empty => new();
    }
}