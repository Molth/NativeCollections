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
    public readonly unsafe struct NativeConcurrentReaderWriterLock : IDisposable, IEquatable<NativeConcurrentReaderWriterLock>
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
            if (handle == null)
                return;
            NativeMemoryAllocator.AlignedFree(handle);
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeConcurrentReaderWriterLock other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentReaderWriterLock nativeConcurrentReaderWriterLock && nativeConcurrentReaderWriterLock == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

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
        public static bool operator ==(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeConcurrentReaderWriterLock left, NativeConcurrentReaderWriterLock right) => left._handle != right._handle;

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _handle->Reset();

        /// <summary>
        ///     Read
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock()
        {
            var handle = _handle;
            handle->Read();
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
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
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock(int sleep1Threshold)
        {
            var handle = _handle;
            handle->Read(sleep1Threshold);
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
        }

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock()
        {
            var handle = _handle;
            handle->Write();
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
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
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock(int sleep1Threshold)
        {
            var handle = _handle;
            handle->Write(sleep1Threshold);
            return new NativeDisposable<UnsafeConcurrentReaderWriterLock>(handle);
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
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(UnsafeConcurrentReaderWriterLock* buffer) => new(buffer);

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(Span<byte> span) => new((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)));

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(ReadOnlySpan<byte> readOnlySpan) => new((UnsafeConcurrentReaderWriterLock*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)));

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