using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a lock that is used to manage access to a resource,
    ///     allowing multiple threads for reading or exclusive access for writing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeConcurrentReaderWriterLock))]
    public readonly unsafe struct NativeConcurrentReaderWriterLock : IIsCreated, IDisposable, IEquatable<NativeConcurrentReaderWriterLock>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentReaderWriterLock* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLock(UnsafeConcurrentReaderWriterLock* buffer) => _handle = buffer;

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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeConcurrentReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeConcurrentReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockScope EnterReadScope() => _handle->EnterReadScope();

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
        public NativeConcurrentReaderWriterLockScope EnterReadScope(int sleep1Threshold) => _handle->EnterReadScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockScope EnterWriteScope() => _handle->EnterWriteScope();

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
        public NativeConcurrentReaderWriterLockScope EnterWriteScope(int sleep1Threshold) => _handle->EnterWriteScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope() => _handle->EnterReadRefScope();

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
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold) => _handle->EnterReadRefScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterWriteRefScope() => _handle->EnterWriteRefScope();

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
        public NativeConcurrentReaderWriterLockRefScope EnterWriteRefScope(int sleep1Threshold) => _handle->EnterWriteRefScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead() => _handle->EnterRead();

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
        public void EnterRead(int sleep1Threshold) => _handle->EnterRead(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite() => _handle->EnterWrite();

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
        public void EnterWrite(int sleep1Threshold) => _handle->EnterWrite(sleep1Threshold);

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentReaderWriterLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeConcurrentReaderWriterLock>(1));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentReaderWriterLock Empty => default;
    }
}