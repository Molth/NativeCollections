using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a contiguous region of arbitrary native memory.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type of the pinned memory.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeMemory<T> : IIsCreated, IDisposable, IEquatable<NativeMemory<T>> where T : unmanaged
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        public NativeMemoryManager<T> Manager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeMemoryManager<T>)_handle.Target!;
        }

        /// <summary>
        ///     Gets the memory block handled by this <see cref="MemoryManager{T}" />.
        /// </summary>
        /// <value>The memory block handled by this <see cref="MemoryManager{T}" />.</value>
        public Memory<T> Memory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Manager.Memory;
        }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="memoryManager">The memory manager to wrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemory(NativeMemoryManager<T> memoryManager) => _handle = GCHandle.Alloc(memoryManager);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="memoryManager">The memory manager to wrap.</param>
        /// <param name="type">The type of GC handle to create.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemory(NativeMemoryManager<T> memoryManager, GCHandleType type) => _handle = GCHandle.Alloc(memoryManager, type);

        /// <summary>
        ///     Creates a native memory wrapper for the specified memory manager.
        /// </summary>
        /// <param name="memoryManager">The memory manager to wrap.</param>
        /// <returns>A native memory wrapper that holds a GC handle to <paramref name="memoryManager" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemory<T> From(NativeMemoryManager<T> memoryManager) => new(memoryManager);

        /// <summary>
        ///     Creates a native memory wrapper for the specified memory manager and handle type.
        /// </summary>
        /// <param name="memoryManager">The memory manager to wrap.</param>
        /// <param name="type">The type of GC handle to allocate.</param>
        /// <returns>
        ///     A native memory wrapper that holds a GC handle of
        ///     <paramref name="type" /> to <paramref name="memoryManager" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemory<T> From(NativeMemoryManager<T> memoryManager, GCHandleType type) => new(memoryManager, type);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeMemory<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeMemory<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeMemory<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Releases the GC handle that this instance owns.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The native buffer held by <see cref="Manager" /> is not released; call
        ///                 <see cref="Dispose(bool)" /> with <see langword="true" /> to release it.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;

            handle.Free();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 The GC handle is always released.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The native buffer held by <see cref="Manager" /> is released as
        ///                 well when <paramref name="disposing" /> is <see langword="true" />.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public void Dispose(bool disposing)
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;

            if (disposing)
                Manager.Dispose();

            handle.Free();
        }
    }
}