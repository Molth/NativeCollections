using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentSpinLock* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLock(UnsafeConcurrentSpinLock* buffer) => _handle = buffer;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Free(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Sequence number
        /// </summary>
        public int SequenceNumber => _handle->SequenceNumber;

        /// <summary>
        ///     Next sequence number
        /// </summary>
        public int NextSequenceNumber => _handle->NextSequenceNumber;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeConcurrentSpinLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockScope EnterScope() => _handle->EnterScope();

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockScope EnterScope(int sleep1Threshold) => _handle->EnterScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRefScope EnterRefScope() => _handle->EnterRefScope();

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
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
        ///     Spins until the lock's sequence number advances beyond the specified value.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber) => _handle->Wait(sequenceNumber);

        /// <summary>
        ///     Spins until the lock's sequence number advances beyond the specified value, with configurable sleep behavior.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number obtained from <see cref="Acquire" />.</param>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber, int sleep1Threshold) => _handle->Wait(sequenceNumber, sleep1Threshold);

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => _handle->Enter();

        /// <summary>
        ///     Enter the lock.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter(int sleep1Threshold) => _handle->Enter(sleep1Threshold);

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentSpinLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeConcurrentSpinLock>(1));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentSpinLock Empty => default;
    }
}