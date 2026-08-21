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
    public readonly ref struct NativeConcurrentSpinLockRefScope
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable
#endif
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeRef<UnsafeConcurrentSpinLock> _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeConcurrentSpinLockRefScope(NativeRef<UnsafeConcurrentSpinLock> handle) => _handle = handle;

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
            handle.AsRef().Exit();
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
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
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
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
        public override string ToString() => "NativeConcurrentSpinLockRefScope";

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeConcurrentSpinLockRefScope Empty => default;
    }
}