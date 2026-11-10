using System;
using System.Buffers;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a memory manager that exposes a <see cref="NativeArray{T}" /> as
    ///     <see cref="Memory{T}" /> and <see cref="Span{T}" />. The wrapped native array
    ///     is disposed when this instance is disposed.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type exposed by the memory manager.</typeparam>
    public sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly NativeArray<T> _buffer;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The native array to wrap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemoryManager(NativeArray<T> buffer) => _buffer = buffer;

        /// <summary>
        ///     Creates a memory manager over an existing native array.
        /// </summary>
        /// <param name="buffer">The native array to wrap.</param>
        /// <returns>A memory manager over <paramref name="buffer" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryManager<T> From(NativeArray<T> buffer) => new(buffer);

        /// <summary>
        ///     Gets the native array wrapped by this memory manager.
        /// </summary>
        public NativeArray<T> Buffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => SR.Format("NativeMemoryManager<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value that is ignored; the wrapped native array is disposed regardless of its value.</param>
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
        ///     The offset to the element in the memory buffer at which the returned <see cref="MemoryHandle" /> points.
        /// </param>
        /// <returns>A handle to the memory that has been pinned.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="elementIndex" /> is negative
        ///     or greater than the length of <see cref="Buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override MemoryHandle Pin(int elementIndex = 0) => new(_buffer.Slice(elementIndex).Buffer);

        /// <summary>
        ///     Unpins pinned memory so that the garbage collector is free to move it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Unpin()
        {
        }
    }
}