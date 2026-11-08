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
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeReaderWriterLock* _handle;

        /// <summary>
        ///     Indicates whether the lock was acquired in read mode
        ///     (<see langword="true" />) or write mode (<see langword="false" />).
        /// </summary>
        private readonly bool _isReader;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="isReader">
        ///     Indicates whether the lock was acquired in read mode
        ///     (<see langword="true" />) or write mode (<see langword="false" />).
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeReaderWriterLockScope(UnsafeReaderWriterLock* handle, bool isReader)
        {
            _handle = handle;
            _isReader = isReader;
        }

        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="isReader">
        ///     Indicates whether the lock was acquired in read mode
        ///     (<see langword="true" />) or write mode (<see langword="false" />).
        /// </param>
        /// <returns>A new instance of this class.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeReaderWriterLockScope From(UnsafeReaderWriterLock* handle, bool isReader) => new(handle, isReader);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
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
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeReaderWriterLockScope other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeReaderWriterLockScope other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeReaderWriterLockScope";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeReaderWriterLockScope left, NativeReaderWriterLockScope right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeReaderWriterLockScope left, NativeReaderWriterLockScope right) => !left.Equals(right);
    }
}