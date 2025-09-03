using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

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
        public NativeConcurrentReaderWriterLock(void* buffer) => _handle = (UnsafeConcurrentReaderWriterLock*)buffer;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> ReadLock(int sleepThreshold)
        {
            var handle = _handle;
            handle->Read(sleepThreshold);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeDisposable<UnsafeConcurrentReaderWriterLock> WriteLock(int sleepThreshold)
        {
            var handle = _handle;
            handle->Write(sleepThreshold);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(int sleepThreshold) => _handle->Read(sleepThreshold);

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write() => _handle->Write();

        /// <summary>
        ///     Write
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int sleepThreshold) => _handle->Write(sleepThreshold);

        /// <summary>
        ///     Exit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exit() => _handle->Exit();

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(void* buffer) => new(buffer);

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(Span<byte> span) => new(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)));

        /// <summary>
        ///     As native concurrent spinLock
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeConcurrentReaderWriterLock(ReadOnlySpan<byte> readOnlySpan) => new(Unsafe.AsPointer(ref MemoryMarshal.GetReference(readOnlySpan)));

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