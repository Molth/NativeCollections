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
    public readonly unsafe ref struct NativeEpochCollectorRefScope
#if NET9_0_OR_GREATER
        : IIsCreated, IDisposable
#endif
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeRef<EpochCollector> _handle;

        /// <summary>
        ///     The current epoch number that the caller is pinned to.
        /// </summary>
        private readonly uint _epoch;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     The current epoch number that the caller is pinned to.
        /// </summary>
        public uint Epoch => _epoch;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeEpochCollectorRefScope(NativeRef<EpochCollector> handle, uint epoch)
        {
            _handle = handle;
            _epoch = epoch;
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
            handle.AsRef().Unpin(_epoch);
        }

        /// <summary>
        ///     Retires a pointer to be freed when it is safe to do so.
        /// </summary>
        /// <param name="data">The pointer to be freed.</param>
        /// <remarks>
        ///     The pointer will be deallocated using <see cref="NativeMemoryAllocator.AlignedFree" />.
        ///     This method is thread-safe and does not block the caller.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(void* data) => _handle.AsRef().Retire(_epoch, data);

        /// <summary>
        ///     Retires a pointer to be freed using a custom deallocation function when it is safe to do so.
        /// </summary>
        /// <param name="data">The pointer to be freed.</param>
        /// <param name="call">A function pointer that deallocates the memory pointed to by <paramref name="data" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="call" /> is <see langword="null" />.</exception>
        /// <remarks>
        ///     This method is thread-safe and does not block the caller. The deallocation callback
        ///     will be invoked exactly once after the epoch advances.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(void* data, delegate* managed<void*, void> call) => _handle.AsRef().Retire(_epoch, data, call);

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
        public override string ToString() => "NativeEpochCollectorRefScope";

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeEpochCollectorRefScope Empty => default;
    }
}