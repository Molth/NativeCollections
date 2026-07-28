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
    public unsafe struct UnsafeConcurrentReaderWriterLock : IEquatable<UnsafeConcurrentReaderWriterLock>
    {
        /// <summary>
        ///     Spin lock
        /// </summary>
        private UnsafeConcurrentSpinLock _spinLock;

        /// <summary>
        ///     Is writer
        /// </summary>
        private bool _isWriter;

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
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _spinLock.Reset();
            _isWriter = false;
            _readSequenceNumber = 0;
            _sequenceNumber = 0;
            _nextSequenceNumber = 0;
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockRef EnterReadLock() => EnterReadLock(-1);

        /// <summary>
        ///     Enter the lock in read mode.
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
        public NativeConcurrentReaderWriterLockRef EnterReadLock(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return new NativeConcurrentReaderWriterLockRef(UnsafeHelpers.AsPointer(ref this));
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeConcurrentReaderWriterLockRef EnterWriteLock() => EnterWriteLock(-1);

        /// <summary>
        ///     Enter the lock in write mode.
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
        public NativeConcurrentReaderWriterLockRef EnterWriteLock(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return new NativeConcurrentReaderWriterLockRef(UnsafeHelpers.AsPointer(ref this));
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
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead(int sleep1Threshold)
        {
            _spinLock.Enter();
            var wasWriter = _isWriter;
            _isWriter = false;
            if (wasWriter)
                _readSequenceNumber = _sequenceNumber;
            var readSequenceNumber = _readSequenceNumber;
            ++_sequenceNumber;
            _spinLock.Exit();
            var spinWait = new UnsafeSpinWait();
            while ((int)(Volatile.Read(ref _nextSequenceNumber) - readSequenceNumber) < 0)
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
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite(int sleep1Threshold)
        {
            _spinLock.Enter();
            _isWriter = true;
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeConcurrentReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeConcurrentReaderWriterLock left, UnsafeConcurrentReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeConcurrentReaderWriterLock Empty => default;
    }
}