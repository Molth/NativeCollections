using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

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
        ///     Handle
        /// </summary>
        private readonly NativeRef<UnsafeReaderWriterLock> _handle;

        /// <summary>
        ///     Is reader
        /// </summary>
        private readonly bool _isReader;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeReaderWriterLockRefScope(NativeRef<UnsafeReaderWriterLock> handle, bool isReader)
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
        /// <returns>Equals</returns>
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
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeReaderWriterLockRefScope";

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeReaderWriterLockRefScope Empty => default;
    }
}