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
    public readonly unsafe struct NativeEpochCollectorScope : IIsCreated, IDisposable, IEquatable<NativeEpochCollectorScope>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly EpochCollector* _handle;

        /// <summary>
        ///     The current epoch number that the caller is pinned to.
        /// </summary>
        private readonly uint _epoch;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     The current epoch number that the caller is pinned to.
        /// </summary>
        public uint Epoch => _epoch;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeEpochCollectorScope(EpochCollector* handle, uint epoch)
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
            if (UnsafeHelpers.IsNull(handle))
                return;
            handle->Unpin(_epoch);
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
        public void Retire(void* data) => _handle->Retire(_epoch, data);

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
        public void Retire(void* data, delegate* managed<void*, void> call) => _handle->Retire(_epoch, data, call);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeEpochCollectorScope other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeEpochCollectorScope other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeEpochCollectorScope";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeEpochCollectorScope left, NativeEpochCollectorScope right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeEpochCollectorScope left, NativeEpochCollectorScope right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeEpochCollectorScope Empty => default;
    }
}