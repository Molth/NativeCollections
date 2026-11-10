using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a stream that stores its data in a chain of fixed-size unmanaged
    ///     memory chunks and synchronizes access with an internal spin lock.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    [BindingType(typeof(UnsafeConcurrentChunkedStream))]
    public readonly unsafe struct NativeConcurrentChunkedStream : IIsCreated, IDisposable, IEquatable<NativeConcurrentChunkedStream>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeConcurrentChunkedStream* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeConcurrentChunkedStream(UnsafeConcurrentChunkedStream* handle) => _handle = handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified
        ///     chunk size and maximum number of free chunks to retain.
        /// </summary>
        /// <param name="size">
        ///     The number of bytes each chunk can hold. Must be greater than zero.
        /// </param>
        /// <param name="maxFreeChunks">
        ///     The maximum number of free chunks to keep in the free list. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated concurrent chunked stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="size" /> is less than or equal
        ///     to zero or <paramref name="maxFreeChunks" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeConcurrentChunkedStream Create(int size, int maxFreeChunks)
        {
            var value = UnsafeConcurrentChunkedStream.Create(size, maxFreeChunks);
            return new NativeConcurrentChunkedStream(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets the total number of chunks currently allocated in the stream.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public int Chunks => _handle->Chunks;

        /// <summary>
        ///     Gets the number of chunks that are currently free and available for reuse.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public int FreeChunks => _handle->FreeChunks;

        /// <summary>
        ///     Gets the maximum number of free chunks that can be retained before excess chunks are freed.
        /// </summary>
        public int MaxFreeChunks => _handle->MaxFreeChunks;

        /// <summary>
        ///     Gets the number of bytes each chunk can hold.
        /// </summary>
        public int Size => _handle->Size;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public int Length => _handle->Length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeConcurrentChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeConcurrentChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeConcurrentChunkedStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeConcurrentChunkedStream left, NativeConcurrentChunkedStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer that receives the bytes read from the stream.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => _handle->Read(buffer, length);

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer containing the bytes to write to the stream.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write([MustNotSelfAlias] byte* buffer, int length) => _handle->Write(buffer, length);

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer that receives the bytes read from the stream.</param>
        /// <returns>The number of bytes read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer containing the bytes to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write([MustNotSelfAlias] ReadOnlySpan<byte> buffer) => _handle->Write(buffer);

        /// <summary>
        ///     Advances the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(int length) => _handle->Read(length);

        /// <summary>
        ///     Advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="length">The number of bytes by which to advance the position.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <remarks>
        ///     This method advances the current position without writing any data. The newly exposed
        ///     range is left untouched, so it may contain data previously written to recycled chunks
        ///     instead of zero bytes.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int length) => _handle->Write(length);

        /// <summary>
        ///     Ensures that this retains at least the specified number of free chunks, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int EnsureCapacity(int capacity) => _handle->EnsureCapacity(capacity);

        /// <summary>
        ///     Releases all free chunks from this.
        /// </summary>
        /// <returns>The number of free chunks retained.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess() => _handle->TrimExcess();

        /// <summary>
        ///     Trims the retained free-chunk capacity of this to the specified number of chunks.
        /// </summary>
        /// <param name="capacity">The number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TrimExcess(int capacity) => _handle->TrimExcess(capacity);
    }
}