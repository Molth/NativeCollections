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
    [BindingType(typeof(UnsafeReaderWriterLock))]
    public readonly unsafe struct NativeReaderWriterLock : IIsCreated, IDisposable, IEquatable<NativeReaderWriterLock>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeReaderWriterLock* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeReaderWriterLock(UnsafeReaderWriterLock* buffer) => _handle = buffer;

        /// <summary>
        ///     Wraps external lock storage.
        /// </summary>
        /// <param name="buffer">A pointer to the lock storage.</param>
        /// <returns>A lock wrapper for <paramref name="buffer" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReaderWriterLock From(UnsafeReaderWriterLock* buffer) => new(buffer);

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
        public bool Equals(NativeReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeReaderWriterLock left, NativeReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeReaderWriterLock left, NativeReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockScope EnterReadScope() => _handle->EnterReadScope();

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
        public NativeReaderWriterLockScope EnterReadScope(int sleep1Threshold) => _handle->EnterReadScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockScope EnterWriteScope() => _handle->EnterWriteScope();

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
        public NativeReaderWriterLockScope EnterWriteScope(int sleep1Threshold) => _handle->EnterWriteScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterReadRefScope() => _handle->EnterReadRefScope();

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
        public NativeReaderWriterLockRefScope EnterReadRefScope(int sleep1Threshold) => _handle->EnterReadRefScope(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        /// <returns>A scope that releases the lock when disposed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRefScope EnterWriteRefScope() => _handle->EnterWriteRefScope();

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
        public NativeReaderWriterLockRefScope EnterWriteRefScope(int sleep1Threshold) => _handle->EnterWriteRefScope(sleep1Threshold);

        /// <summary>
        ///     Attempts to enter the lock in read mode.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the lock was acquired; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterRead() => _handle->TryEnterRead();

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
        public bool TryEnterWrite() => _handle->TryEnterWrite();

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
        public bool TryEnterWrite(int sleep1Threshold) => _handle->TryEnterWrite(sleep1Threshold);

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
        public void ExitRead() => _handle->ExitRead();

        /// <summary>
        ///     Exit the lock.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => _handle->ExitWrite();

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <returns>A new instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReaderWriterLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeReaderWriterLock>(Usize.FromI32Unchecked(1)));
    }
}