using System;
using System.Buffers;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a contiguous region of arbitrary native memory.
    /// </summary>
    [IsReferenceOrContainsReferences]
    public sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly NativeArray<T> _buffer;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryManager(NativeArray<T> buffer) => _buffer = buffer;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public NativeArray<T> Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer;
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(NativeMemoryManager<T> value) => value.Memory.Span;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(NativeMemoryManager<T> value) => value.Memory.Span;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Memory<T>(NativeMemoryManager<T> value) => value.Memory;

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(NativeMemoryManager<T> value) => value.Memory;

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeArray<T>(NativeMemoryManager<T> value) => value._buffer;

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => SR.Format("NativeMemoryManager<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Dispose(true);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Disposing</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool disposing) => _buffer.Dispose();

        /// <summary>
        ///     Returns a memory span that wraps the underlying memory buffer.
        /// </summary>
        /// <returns>A memory span that wraps the underlying memory buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Span<T> GetSpan() => _buffer.AsSpan();

        /// <summary>
        ///     Returns a handle to the memory that has been pinned and whose address can be taken.
        /// </summary>
        /// <param name="elementIndex">
        ///     The offset to the element in the memory buffer
        ///     at which the returned <see cref="T:System.Buffers.MemoryHandle" /> points.
        /// </param>
        /// <returns>A handle to the memory that has been pinned.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MemoryHandle Pin(int elementIndex = 0) => new(UnsafeHelpers.Add<T>(_buffer.Buffer, elementIndex));

        /// <summary>
        ///     Unpins pinned memory so that the garbage collector is free to move it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Unpin()
        {
        }
    }
}