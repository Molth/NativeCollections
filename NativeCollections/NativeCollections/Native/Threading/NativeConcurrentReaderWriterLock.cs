using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeConcurrentReaderWriterLock* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentReaderWriterLock(UnsafeConcurrentReaderWriterLock* buffer) => _handle = buffer;

        /// <summary>
        ///     Wraps external lock storage.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        /// <returns>A lock wrapper for <paramref name="buffer" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentReaderWriterLock From(UnsafeConcurrentReaderWriterLock* buffer) => new(buffer);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => NativeMemoryAllocator.AlignedFree(_handle);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeConcurrentReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockScope EnterReadScope() => _handle->EnterReadScope();

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
        public NativeConcurrentReaderWriterLockScope EnterReadScope(int sleep1Threshold) => _handle->EnterReadScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockScope EnterWriteScope() => _handle->EnterWriteScope();

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
        public NativeConcurrentReaderWriterLockScope EnterWriteScope(int sleep1Threshold) => _handle->EnterWriteScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope() => _handle->EnterReadRefScope();

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
        public NativeConcurrentReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold) => _handle->EnterReadRefScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeConcurrentReaderWriterLockRefScope EnterWriteRefScope() => _handle->EnterWriteRefScope();

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
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
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
        ///     A minimum spin count after which <see cref="Thread.Sleep(int)" /> may be
        ///     used. A value of -1 disables the use of <see cref="Thread.Sleep(int)" />.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite(int sleep1Threshold) => _handle->EnterWrite(sleep1Threshold);

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentReaderWriterLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeConcurrentReaderWriterLock>(Usize.FromI32Unchecked(1)));
    }
}