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
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Is created
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
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentSpinLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentSpinLock other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

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
        public static bool operator ==(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentSpinLock left, NativeConcurrentSpinLock right) => !left.Equals(right);

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentSpinLock> EnterLock()
        {
            var handle = _handle;
            handle->Enter();
            return new UnsafeDisposable<UnsafeConcurrentSpinLock>(handle);
        }

        /// <summary>
        ///     Enter
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentSpinLock> EnterLock(int sequenceNumber)
        {
            var handle = _handle;
            handle->Enter(sequenceNumber);
            return new UnsafeDisposable<UnsafeConcurrentSpinLock>(handle);
        }

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
        public static NativeConcurrentSpinLock Empty => new();
    }
}