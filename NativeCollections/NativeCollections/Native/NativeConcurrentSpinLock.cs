using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrent spinLock
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
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRef EnterLock() => _handle->EnterLock();

        /// <summary>
        ///     Enter
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentSpinLockRef EnterLock(int sleep1Threshold) => _handle->EnterLock(sleep1Threshold);

        /// <summary>
        ///     Acquire
        /// </summary>
        /// <returns>Sequence number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Acquire() => _handle->Acquire();

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Wait(int sequenceNumber) => _handle->Wait(sequenceNumber);

        /// <summary>
        ///     Wait
        /// </summary>
        /// <param name="sequenceNumber">Sequence number</param>
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
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enter() => _handle->Enter();

        /// <summary>
        ///     Enter
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
        ///     Exit
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