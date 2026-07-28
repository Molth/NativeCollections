using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native reader writer lock ref
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly unsafe struct NativeReaderWriterLockRef : IIsCreated, IDisposable, IEquatable<NativeReaderWriterLockRef>
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
        public NativeReaderWriterLockRef(UnsafeReaderWriterLock* handle, bool isReader)
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
        public bool Equals(NativeReaderWriterLockRef other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeReaderWriterLockRef other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeReaderWriterLockRef";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeReaderWriterLockRef left, NativeReaderWriterLockRef right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeReaderWriterLockRef left, NativeReaderWriterLockRef right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReaderWriterLockRef Empty => default;
    }
}