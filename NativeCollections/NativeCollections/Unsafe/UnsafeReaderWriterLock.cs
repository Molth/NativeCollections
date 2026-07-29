using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe reader writer lock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeReaderWriterLock : IEquatable<UnsafeReaderWriterLock>
    {
        /// <summary>
        ///     Writer mask
        /// </summary>
        private const uint WRITER_MASK = unchecked((uint)(1 << 31));

        /// <summary>
        ///     Max readers
        /// </summary>
        private const uint MAX_READERS = WRITER_MASK - 1;

        /// <summary>
        ///     State
        /// </summary>
        private UnsafeAtomicU32 _state;

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _state = new UnsafeAtomicU32();

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeReaderWriterLockScope EnterReadScope() => EnterReadScope(-1);

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
        public NativeReaderWriterLockScope EnterReadScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return new NativeReaderWriterLockScope(UnsafeHelpers.AsPointer(ref this), true);
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeReaderWriterLockScope EnterWriteScope() => EnterWriteScope(-1);

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
        public NativeReaderWriterLockScope EnterWriteScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return new NativeReaderWriterLockScope(UnsafeHelpers.AsPointer(ref this), false);
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterReadRefScope() => EnterReadRefScope(-1);

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
        public NativeReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return new NativeReaderWriterLockRefScope(NativeRef<UnsafeReaderWriterLock>.Create(ref this), true);
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterWriteRefScope() => EnterWriteRefScope(-1);

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
        public NativeReaderWriterLockRefScope EnterWriteRefScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return new NativeReaderWriterLockRefScope(NativeRef<UnsafeReaderWriterLock>.Create(ref this), false);
        }

        /// <summary>
        ///     Attempts to enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterRead()
        {
            var state = _state.Load(Ordering.Acquire);
            return state < MAX_READERS && _state.CompareExchange(state + 1, state) == state;
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
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                if (TryEnterRead())
                    break;

                spinWait.SpinOnce(sleep1Threshold);
            }
        }

        /// <summary>
        ///     Spin until unlocked
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SpinUntilUnlocked(int sleep1Threshold)
        {
            var spinWait = new UnsafeSpinWait();
            while ((_state.Load(Ordering.Acquire) & ~WRITER_MASK) != 0)
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Acquire writer lock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcquireWriterLock()
        {
            var state = _state.Load(Ordering.Acquire);
            return (state & WRITER_MASK) == 0 && _state.CompareExchange(state | WRITER_MASK, state) == state;
        }

        /// <summary>
        ///     Attempts to enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterWrite() => TryEnterWrite(-1);

        /// <summary>
        ///     Attempts to enter the lock in write mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterWrite(int sleep1Threshold)
        {
            if (!AcquireWriterLock())
                return false;

            SpinUntilUnlocked(sleep1Threshold);
            return true;
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
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                if (AcquireWriterLock())
                    break;

                spinWait.SpinOnce(sleep1Threshold);
            }

            SpinUntilUnlocked(sleep1Threshold);
        }

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => _state.Sub(1);

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => _state.Store(0, Ordering.Release);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeReaderWriterLock left, UnsafeReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeReaderWriterLock left, UnsafeReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeReaderWriterLock Empty => default;
    }
}