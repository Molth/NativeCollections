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
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeReaderWriterLock : IEquatable<UnsafeReaderWriterLock>
    {
        /// <summary>
        ///     Bit mask indicating that the writer lock is held (the highest bit).
        /// </summary>
        private const uint WRITER_MASK = 1U << 31;

        /// <summary>
        ///     Sentinel upper bound for the reader count stored in the low 31 bits.
        /// </summary>
        private const uint MAX_READERS = WRITER_MASK - 1U;

        /// <summary>
        ///     Represents the state.
        /// </summary>
        private UnsafeAtomicU32 _state;

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset() => _state = UnsafeAtomicU32.Create(0U);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeReaderWriterLockScope EnterReadScope() => EnterReadScope(-1);

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
        public NativeReaderWriterLockScope EnterReadScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return NativeReaderWriterLockScope.From(UnsafeHelpers.AsPointer(ref this), true);
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public NativeReaderWriterLockScope EnterWriteScope() => EnterWriteScope(-1);

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
        public NativeReaderWriterLockScope EnterWriteScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return NativeReaderWriterLockScope.From(UnsafeHelpers.AsPointer(ref this), false);
        }

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterReadRefScope() => EnterReadRefScope(-1);

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
        public NativeReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold)
        {
            EnterRead(sleep1Threshold);
            return NativeReaderWriterLockRefScope.From(NativeRef<UnsafeReaderWriterLock>.From(ref this), true);
        }

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterWriteRefScope() => EnterWriteRefScope(-1);

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
        public NativeReaderWriterLockRefScope EnterWriteRefScope(int sleep1Threshold)
        {
            EnterWrite(sleep1Threshold);
            return NativeReaderWriterLockRefScope.From(NativeRef<UnsafeReaderWriterLock>.From(ref this), false);
        }

        /// <summary>
        ///     Attempts to enter the lock in read mode.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterRead()
        {
            var state = _state.Load(Ordering.Acquire);
            return state < MAX_READERS && _state.CompareExchange(state.WrappingAdd(1U), state) == state;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            UnsafeSpinWait spinWait = default;
            while (true)
            {
                if (TryEnterRead())
                    break;

                spinWait.SpinOnce(sleep1Threshold);
            }
        }

        /// <summary>
        ///     Spins until there are no active readers, indicating the writer lock can be granted.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SpinUntilUnlocked(int sleep1Threshold)
        {
            UnsafeSpinWait spinWait = default;
            while ((_state.Load(Ordering.Acquire) & ~WRITER_MASK) != 0U)
                spinWait.SpinOnce(sleep1Threshold);
        }

        /// <summary>
        ///     Attempts to atomically acquire the write lock bit.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the write lock bit was acquired; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AcquireWriterLock()
        {
            var state = _state.Load(Ordering.Acquire);
            return (state & WRITER_MASK) == 0U && _state.CompareExchange(state | WRITER_MASK, state) == state;
        }

        /// <summary>
        ///     Attempts to enter the lock in write mode.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 This method can block: it first excludes other writers, then waits until all
        ///                 current readers release the lock before returning <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 A concurrent reader that modifies the lock state can make this method return
        ///                 <see langword="false" /> even though no writer holds the lock; the caller must retry.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterWrite() => TryEnterWrite(-1);

        /// <summary>
        ///     Attempts to enter the lock in write mode.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 This method can block: it first excludes other writers, then waits until all
        ///                 current readers release the lock before returning <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 A concurrent reader that modifies the lock state can make this method return
        ///                 <see langword="false" /> even though no writer holds the lock; the caller must retry.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterWrite(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
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
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite(int sleep1Threshold)
        {
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            UnsafeSpinWait spinWait = default;
            while (true)
            {
                if (AcquireWriterLock())
                    break;

                spinWait.SpinOnce(sleep1Threshold);
            }

            SpinUntilUnlocked(sleep1Threshold);
        }

        /// <summary>
        ///     Exits the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => _state.Sub(1U);

        /// <summary>
        ///     Exits the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => _state.Store(0U, Ordering.Release);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeReaderWriterLock left, UnsafeReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeReaderWriterLock left, UnsafeReaderWriterLock right) => !left.Equals(right);
    }
}