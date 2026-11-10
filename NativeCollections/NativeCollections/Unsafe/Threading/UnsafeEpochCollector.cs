using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.PaddingHelpers;

#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     An epoch-based memory reclamation (EBR) collector. Implements a lock-free
    ///     garbage collection mechanism for concurrent data structures. This
    ///     implementation uses three epochs (0, 1, 2) and per-slot bags of retired objects.
    /// </summary>
    /// <remarks>
    ///     https://github.com/dotnet/dotNext/blob/master/src/DotNext/Threading/Epoch.cs
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Community)]
    [BindingType(typeof(RawEpochCollector))]
    public unsafe struct UnsafeEpochCollector : IIsCreated, IDisposable, IEquatable<UnsafeEpochCollector>
    {
        /// <summary>
        ///     Padding to avoid false sharing with adjacent data.
        /// </summary>
        private readonly CachePadding _padding;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private RawEpochCollector _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Always <see langword="true" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The default value of this type is a valid empty collector: the epoch slots start
        ///                 zeroed, so there is no separate initialization step.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeEpochCollector other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeEpochCollector other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeEpochCollector";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeEpochCollector left, UnsafeEpochCollector right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeEpochCollector left, UnsafeEpochCollector right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorScope EnterScope() => _handle.EnterScope();

        /// <summary>
        ///     Enters the current epoch and returns a disposable scope that automatically exits the epoch on disposal.
        /// </summary>
        /// <returns>A disposable scope representing the pinned epoch.</returns>
        /// <remarks>
        ///     This method is thread-safe. The returned scope must be disposed to unpin the epoch.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeEpochCollectorRefScope EnterRefScope() => _handle.EnterRefScope();

        /// <summary>
        ///     Enters the current epoch and returns the epoch identifier.
        /// </summary>
        /// <returns>The current epoch number that the caller is pinned to.</returns>
        /// <remarks>
        ///     This method is thread-safe and must be paired with a call to <see cref="Unpin" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Pin() => _handle.Pin();

        /// <summary>
        ///     Exits the pinned epoch and triggers garbage collection if necessary.
        /// </summary>
        /// <param name="epoch">The epoch value returned from a prior call to <see cref="Pin" />.</param>
        /// <remarks>
        ///     This method decrements the pin counter for the specified epoch. The unpinning
        ///     counter is incremented by one per call, and a collection attempt is made whenever
        ///     the resulting count reaches a multiple of 64 (that is, on the 64th, 128th,
        ///     192nd unpinning of a slot).
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
        /// <param name="call">
        ///     A function pointer that deallocates the memory pointed to by <paramref name="data" />.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="call" /> is <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     This method is thread-safe and does not block the caller. The deallocation
        ///     callback will be invoked exactly once after the epoch advances. Disposing the
        ///     collector instead invokes all pending callbacks immediately on the disposing
        ///     thread, without advancing the epoch and regardless of pin counts.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Retire(uint epoch, void* data, delegate* managed<void*, void> call) => _handle.Retire(epoch, data, call);
    }
}