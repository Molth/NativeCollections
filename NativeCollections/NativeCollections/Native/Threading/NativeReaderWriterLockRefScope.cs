using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a disposable scope that holds a reference to a resource and releases it when disposed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [IsAssignableTo(typeof(IIsCreated), typeof(IDisposable))]
    public readonly ref struct NativeReaderWriterLockRefScope
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable
#endif
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeRef<UnsafeReaderWriterLock> _handle;

        /// <summary>
        ///     Indicates whether the lock was acquired in read mode
        ///     (<see langword="true" />) or write mode (<see langword="false" />).
        /// </summary>
        private readonly bool _isReader;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="isReader">
        ///     Indicates whether the lock was acquired in read mode
        ///     (<see langword="true" />) or write mode (<see langword="false" />).
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeReaderWriterLockRefScope(NativeRef<UnsafeReaderWriterLock> handle, bool isReader)
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
        internal static NativeReaderWriterLockRefScope From(NativeRef<UnsafeReaderWriterLock> handle, bool isReader) => new(handle, isReader);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsCreated)
                return;

            if (_isReader)
            {
                handle.AsRef().ExitRead();
                return;
            }

            handle.AsRef().ExitWrite();
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeReaderWriterLockRefScope";
    }
}