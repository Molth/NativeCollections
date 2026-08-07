using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a contiguous region of arbitrary native memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public readonly struct NativeMemory<T> : IIsCreated, IDisposable, IEquatable<NativeMemory<T>> where T : unmanaged
    {
        /// <summary>
        ///     Handle
        /// </summary>
        private readonly GCHandle _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsAllocated;

        /// <summary>
        ///     Manager
        /// </summary>
        public NativeMemoryManager<T> Manager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (NativeMemoryManager<T>)_handle.Target!;
        }

        /// <summary>
        ///     Memory
        /// </summary>
        public Memory<T> Memory
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Manager.Memory;
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="memoryManager">Native memory manager</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemory(NativeMemoryManager<T> memoryManager) => _handle = GCHandle.Alloc(memoryManager);

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="memoryManager">Native memory manager</param>
        /// <param name="type">Type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemory(NativeMemoryManager<T> memoryManager, GCHandleType type) => _handle = GCHandle.Alloc(memoryManager, type);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(NativeMemory<T> other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeMemory<T> other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeMemory<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right) => !left.Equals(right);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeMemory<T> value) => value.Memory.Span;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeMemory<T> value) => value.Memory.Span;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>(NativeMemory<T> value) => value.Memory;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(NativeMemory<T> value) => value.Memory;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeMemory<T> value) => value.Manager.Buffer;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            handle.Free();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Disposing</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose(bool disposing)
        {
            var handle = _handle;
            if (!handle.IsAllocated)
                return;
            if (disposing)
                Manager.Dispose();
            handle.Free();
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemory<T> Empty => default;
    }
}