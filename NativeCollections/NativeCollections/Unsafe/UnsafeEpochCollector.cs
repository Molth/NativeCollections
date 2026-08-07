using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An epoch-based memory reclamation (EBR) collector.
    ///     Implements a lock-free garbage collection mechanism for concurrent data structures.
    ///     This implementation uses three epochs (0, 1, 2) and per-slot bags of retired objects.
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/Threading/Epoch.cs
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    [BindingType(typeof(EpochCollector))]
    public unsafe struct UnsafeEpochCollector : IIsCreated, IDisposable, IEquatable<UnsafeEpochCollector>
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     Handle
        /// </summary>
        private EpochCollector _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeEpochCollector other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeEpochCollector other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeEpochCollector";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeEpochCollector left, UnsafeEpochCollector right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeEpochCollector left, UnsafeEpochCollector right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorScope EnterScope() => _handle.EnterScope();

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorRefScope EnterRefScope() => _handle.EnterRefScope();

        /// <summary>
        ///     Enters the current epoch and returns the epoch identifier.
        /// </summary>
        /// <remarks>
        ///     This method is thread-safe and must be paired with a call to <see cref="Unpin" />.
        /// </remarks>
        /// <returns>The current epoch number that the caller is pinned to.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pin() => _handle.Pin();

        /// <summary>
        ///     Exits the pinned epoch and triggers garbage collection if necessary.
        /// </summary>
        /// <param name="epoch">The epoch value returned from a prior call to <see cref="Pin" />.</param>
        /// <remarks>
        ///     This method decrements the pin counter for the specified epoch. If the unpinning count
        ///     reaches the threshold, a collection attempt is made to reclaim retired objects.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unpin(uint epoch) => _handle.Unpin(epoch);

        /// <summary>
        ///     Retires a pointer to be freed when it is safe to do so.
        /// </summary>
        /// <param name="epoch">The epoch value returned from <see cref="Pin" />.</param>
        /// <param name="data">The pointer to be freed.</param>
        /// <remarks>
        ///     The pointer will be deallocated using <see cref="NativeMemoryAllocator.AlignedFree" />.
        ///     This method is thread-safe and does not block the caller.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data) => _handle.Retire(epoch, data);

        /// <summary>
        ///     Retires a pointer to be freed using a custom deallocation function when it is safe to do so.
        /// </summary>
        /// <param name="epoch">The epoch value returned from <see cref="Pin" />.</param>
        /// <param name="data">The pointer to be freed.</param>
        /// <param name="call">A function pointer that deallocates the memory pointed to by <paramref name="data" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="call" /> is <see langword="null" />.</exception>
        /// <remarks>
        ///     This method is thread-safe and does not block the caller. The deallocation callback
        ///     will be invoked exactly once after the epoch advances.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data, delegate* managed<void*, void> call) => _handle.Retire(epoch, data, call);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeEpochCollector Empty => default;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeEpochCollector Create() => new();
    }
}