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
    [UnsafeCollection(FromType.None)]
    [BindingType(typeof(UnsafeChunkedStream))]
    public unsafe struct UnsafeConcurrentChunkedStream : IIsCreated, IDisposable, IEquatable<UnsafeConcurrentChunkedStream>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private UnsafeChunkedStream _handle;

        /// <summary>
        ///     Internal spin lock used to synchronize updates to the shared state.
        /// </summary>
        private UnsafeConcurrentSpinLock _spinLock;

        /// <summary>
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        /// <param name="spinLock">The internal spin lock used to synchronize updates to the shared state.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeConcurrentChunkedStream(UnsafeChunkedStream handle, UnsafeConcurrentSpinLock spinLock)
        {
            _handle = handle;
            _spinLock = spinLock;
        }

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
        public static UnsafeConcurrentChunkedStream Create(int size, int maxFreeChunks) => new(UnsafeChunkedStream.Create(size, maxFreeChunks), default);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _handle.IsCreated;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public readonly bool IsEmpty => _handle.IsEmpty;

        /// <summary>
        ///     Gets the number of chunks currently in use by the stream, excluding free chunks.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public readonly int Chunks => _handle.Chunks;

        /// <summary>
        ///     Gets the number of chunks that are currently free and available for reuse.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public readonly int FreeChunks => _handle.FreeChunks;

        /// <summary>
        ///     Gets the maximum number of free chunks that can be retained before excess chunks are freed.
        /// </summary>
        public readonly int MaxFreeChunks => _handle.MaxFreeChunks;

        /// <summary>
        ///     Gets the number of bytes each chunk can hold.
        /// </summary>
        public readonly int Size => _handle.Size;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        /// <remarks>
        ///     This property is read without taking the internal lock, so the
        ///     value may be stale when it is observed concurrently.
        /// </remarks>
        public readonly int Length => _handle.Length;

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeConcurrentChunkedStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeConcurrentChunkedStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeConcurrentChunkedStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeConcurrentChunkedStream left, UnsafeConcurrentChunkedStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeConcurrentChunkedStream left, UnsafeConcurrentChunkedStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => _handle.Dispose();

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
        public int Read(byte* buffer, int length)
        {
            using (_spinLock.EnterRefScope())
            {
                return _handle.Read(buffer, length);
            }
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer containing the bytes to write to the stream.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        public void Write([MustNotSelfAlias] byte* buffer, int length)
        {
            using (_spinLock.EnterRefScope())
            {
                _handle.Write(buffer, length);
            }
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer that receives the bytes read from the stream.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(Span<byte> buffer)
        {
            using (_spinLock.EnterRefScope())
            {
                return _handle.Read(buffer);
            }
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer containing the bytes to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        public void Write([MustNotSelfAlias] ReadOnlySpan<byte> buffer)
        {
            using (_spinLock.EnterRefScope())
            {
                _handle.Write(buffer);
            }
        }

        /// <summary>
        ///     Advances the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        public int Read(int length)
        {
            using (_spinLock.EnterRefScope())
            {
                return _handle.Read(length);
            }
        }

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
        public void Write(int length)
        {
            using (_spinLock.EnterRefScope())
            {
                _handle.Write(length);
            }
        }

        /// <summary>
        ///     Ensures that this retains at least the specified number of free chunks, up to the configured maximum.
        /// </summary>
        /// <param name="capacity">The minimum number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained by this.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        public int EnsureCapacity(int capacity)
        {
            using (_spinLock.EnterRefScope())
            {
                return _handle.EnsureCapacity(capacity);
            }
        }

        /// <summary>
        ///     Releases all free chunks from this.
        /// </summary>
        /// <returns>The number of free chunks retained.</returns>
        public int TrimExcess()
        {
            using (_spinLock.EnterRefScope())
            {
                _handle.TrimExcess();
                return 0;
            }
        }

        /// <summary>
        ///     Trims the retained free-chunk capacity of this to the specified number of chunks.
        /// </summary>
        /// <param name="capacity">The number of free chunks to retain.</param>
        /// <returns>The number of free chunks retained.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        public int TrimExcess(int capacity)
        {
            using (_spinLock.EnterRefScope())
            {
                return _handle.TrimExcess(capacity);
            }
        }
    }
}