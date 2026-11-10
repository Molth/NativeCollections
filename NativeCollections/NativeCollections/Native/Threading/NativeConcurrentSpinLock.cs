using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a lock that uses an atomic sequence number to manage exclusive access to a resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeConcurrentSpinLock))]
    public readonly unsafe struct NativeConcurrentSpinLock : IIsCreated, IDisposable, IEquatable<NativeConcurrentSpinLock>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeConcurrentSpinLock* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentSpinLock(UnsafeConcurrentSpinLock* buffer) => _handle = buffer;

        /// <summary>
        ///     Wraps external lock storage.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        /// <returns>A lock wrapper for <paramref name="buffer" />.</returns>
        /// <remarks>
        ///     The returned wrapper owns <paramref name="buffer" /> (the lock storage);
        ///     <see cref="Dispose" /> releases it with <see cref="NativeMemoryAllocator.AlignedFree" />.
        ///     Therefore, <paramref name="buffer" /> must be allocated by
        ///     <see cref="NativeMemoryAllocator" /> or a compatible allocator, and the caller must not
        ///     release it separately while the returned wrapper is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentSpinLock From(UnsafeConcurrentSpinLock* buffer) => new(buffer);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentSpinLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeConcurrentSpinLock>(Usize.FromI32Unchecked(1)));

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets the current sequence number.
        /// </summary>
        public int SequenceNumber => _handle->SequenceNumber;

        /// <summary>
        ///     Gets the sequence number corresponding to the last released lock.
        /// </summary>
        public int NextSequenceNumber => _handle->NextSequenceNumber;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeConcurrentSpinLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => !left.Equals(right);

        /// <summary>
        ///     Resets this to the state of a newly created lock.
        /// </summary>
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enters the lock and returns a <see cref="NativeConcurrentSpinLockScope" />.
        /// </summary>
        /// <returns>A scope that holds the lock until it is disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockScope EnterScope() => _handle->EnterScope();

        /// <summary>
        ///     Enters the lock and returns a <see cref="NativeConcurrentSpinLockScope" />.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A scope that holds the lock until it is disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockScope EnterScope(int sleep1Threshold) => _handle->EnterScope(sleep1Threshold);

        /// <summary>
        ///     Enters the lock and returns a <see cref="NativeConcurrentSpinLockRefScope" />.
        /// </summary>
        /// <returns>A reference scope that holds the lock until it is disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope() => _handle->EnterRefScope();

        /// <summary>
        ///     Enters the lock and returns a <see cref="NativeConcurrentSpinLockRefScope" />.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <returns>A reference scope that holds the lock until it is disposed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope(int sleep1Threshold) => _handle->EnterRefScope(sleep1Threshold);

        /// <summary>
        ///     Atomically acquires the current sequence number.
        /// </summary>
        /// <returns>The current sequence number at the time of acquisition.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => _handle->Acquire();

        /// <summary>
        ///     Spins until the lock's released sequence number reaches the specified value.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber) => _handle->Wait(sequenceNumber);

        /// <summary>
        ///     Spins until the lock's released sequence number reaches
        ///     the specified value, with configurable sleep behavior.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber, int sleep1Threshold) => _handle->Wait(sequenceNumber, sleep1Threshold);

        /// <summary>
        ///     Enters the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => _handle->Enter();

        /// <summary>
        ///     Enters the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleep1Threshold) => _handle->Enter(sleep1Threshold);

        /// <summary>
        ///     Exits the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();
    }
}