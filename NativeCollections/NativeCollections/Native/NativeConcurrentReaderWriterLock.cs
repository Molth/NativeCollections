using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native concurrent reader writer lock
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
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentReaderWriterLock other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentReaderWriterLock other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeConcurrentReaderWriterLock";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => !left.Equals(right);

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock()
        {
            var handle = _handle;
            handle->Read();
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock(int sleep1Threshold)
        {
            var handle = _handle;
            handle->Read(sleep1Threshold);
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock()
        {
            var handle = _handle;
            handle->Write();
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock(int sleep1Threshold)
        {
            var handle = _handle;
            handle->Write(sleep1Threshold);
            return new UnsafeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
        }

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read() => _handle->Read();

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(int sleep1Threshold) => _handle->Read(sleep1Threshold);

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write() => _handle->Write();

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <see langword="Thread.Sleep(1)" /> may be used. A value
        ///     of -1 disables the use of <see langword="Thread.Sleep(1)" />.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int sleep1Threshold) => _handle->Write(sleep1Threshold);

        /// <summary>
        ///     Exit
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
        public static NativeConcurrentReaderWriterLock Empty => new();
    }
}