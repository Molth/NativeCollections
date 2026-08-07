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
        ///     Buffer
        /// </summary>
        private readonly NativeArray<T> _buffer;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryManager(NativeArray<T> buffer) => _buffer = buffer;

        /// <summary>
        ///     Buffer
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
        ///     Get span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Span<T> GetSpan() => _buffer.AsSpan();

        /// <summary>
        ///     Pin
        /// </summary>
        /// <param name="elementIndex">Element index</param>
        /// <returns>MemoryHandle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MemoryHandle Pin(int elementIndex = 0) => new(UnsafeHelpers.Add<T>(_buffer.Buffer, elementIndex));

        /// <summary>
        ///     Unpin
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Unpin()
        {
        }
    }
}