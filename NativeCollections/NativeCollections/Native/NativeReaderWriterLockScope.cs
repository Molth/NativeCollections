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
    public readonly unsafe struct NativeReaderWriterLockScope : IIsCreated, IDisposable, IEquatable<NativeReaderWriterLockScope>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly UnsafeReaderWriterLock* _handle;

        /// <summary>
        ///     Is reader
        /// </summary>
        private readonly bool _isReader;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeReaderWriterLockScope(UnsafeReaderWriterLock* handle, bool isReader)
        {
            _handle = handle;
            _isReader = isReader;
        }

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
            if (_isReader)
            {
                handle->ExitRead();
                return;
            }

            handle->ExitWrite();
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeReaderWriterLockScope other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeReaderWriterLockScope other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeReaderWriterLockScope";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeReaderWriterLockScope left, NativeReaderWriterLockScope right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeReaderWriterLockScope left, NativeReaderWriterLockScope right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReaderWriterLockScope Empty => default;
    }
}