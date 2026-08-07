using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a disposable scope that holds a reference to a resource and releases it when disposed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeConcurrentReaderWriterLockScope : IIsCreated, IDisposable, IEquatable<NativeConcurrentReaderWriterLockScope>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeConcurrentReaderWriterLock* _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeConcurrentReaderWriterLockScope(UnsafeConcurrentReaderWriterLock* handle) => _handle = handle;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Exit();
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeConcurrentReaderWriterLockScope other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeConcurrentReaderWriterLockScope other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeConcurrentReaderWriterLockScope";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeConcurrentReaderWriterLockScope left, NativeConcurrentReaderWriterLockScope right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeConcurrentReaderWriterLockScope left, NativeConcurrentReaderWriterLockScope right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeConcurrentReaderWriterLockScope Empty => default;
    }
}