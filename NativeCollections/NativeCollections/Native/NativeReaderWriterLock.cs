using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native reader writer lock
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeReaderWriterLock))]
    public readonly unsafe struct NativeReaderWriterLock : IIsCreated, IDisposable, IEquatable<NativeReaderWriterLock>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeReaderWriterLock* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLock(UnsafeReaderWriterLock* buffer) => _handle = buffer;

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
        public bool Equals(NativeReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeReaderWriterLock";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeReaderWriterLock left, NativeReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeReaderWriterLock left, NativeReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Sets this to its initial position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Enter the lock in read mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRef EnterReadLock() => _handle->EnterReadLock();

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
        public NativeReaderWriterLockRef EnterReadLock(int sleep1Threshold) => _handle->EnterReadLock(sleep1Threshold);

        /// <summary>
        ///     Enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReaderWriterLockRef EnterWriteLock() => _handle->EnterWriteLock();

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
        public NativeReaderWriterLockRef EnterWriteLock(int sleep1Threshold) => _handle->EnterWriteLock(sleep1Threshold);

        /// <summary>
        ///     Attempts to enter the lock in read mode.
        /// </summary>
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
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterRead(int sleep1Threshold) => _handle->EnterRead(sleep1Threshold);

        /// <summary>
        ///     Attempts to enter the lock in write mode.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryEnterWrite() => _handle->TryEnterWrite();

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
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnterWrite(int sleep1Threshold) => _handle->EnterWrite(sleep1Threshold);

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitRead() => _handle->ExitRead();

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExitWrite() => _handle->ExitWrite();

        /// <summary>
        ///     Create
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReaderWriterLock Create() => new(NativeMemoryAllocator.AlignedAllocZeroed<UnsafeReaderWriterLock>(1));

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReaderWriterLock Empty => default;
    }
}