using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a stream whose backing store is a contiguous unmanaged memory buffer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeMemoryStream))]
    public readonly unsafe struct NativeMemoryStream : IIsCreated, IDisposable, IEquatable<NativeMemoryStream>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeMemoryStream* _handle;

        /// <summary>
        ///     Initializes a new instance of this class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemoryStream(UnsafeMemoryStream* handle) => _handle = handle;

        /// <summary>
        ///     Creates a stream with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of bytes that the instance can hold. Must be non-negative.
        /// </param>
        /// <returns>A newly allocated stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemoryStream Create(int capacity)
        {
            var value = UnsafeMemoryStream.Create(capacity);
            return new NativeMemoryStream(Box.New(ref value));
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public bool IsEmpty => _handle->IsEmpty;

        /// <summary>
        ///     Gets a reference to the byte at the specified byte offset.
        /// </summary>
        /// <param name="index">The byte offset from the start of the buffer at which to access a byte.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Capacity" />.
        /// </exception>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<UnsafeMemoryStream>(_handle)[index];
        }

        /// <summary>
        ///     Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public bool CanRead => IsCreated;

        /// <summary>
        ///     Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public bool CanSeek => IsCreated;

        /// <summary>
        ///     Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public bool CanWrite => IsCreated;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        public int Length => _handle->Length;

        /// <summary>
        ///     Gets or sets the current position within the stream.
        /// </summary>
        /// <value>The current position within the stream.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the position is set to a negative value.
        /// </exception>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Any non-negative value is accepted; the position is not
        ///                 constrained by <see cref="Capacity" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 Writing past the current length grows the buffer and zero-initializes the bytes in between.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Position = value;
        }

        /// <summary>
        ///     Gets or sets the total number of bytes the internal data structure can hold.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the capacity is set to a value less than the current <see cref="Length" />.
        /// </exception>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Capacity = value;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeMemoryStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeMemoryStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeMemoryStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeMemoryStream left, NativeMemoryStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeMemoryStream left, NativeMemoryStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Returns the array of unsigned bytes from which this stream was created.
        /// </summary>
        /// <returns>The array of unsigned bytes from which this stream was created.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer() => _handle->GetBuffer();

        /// <summary>
        ///     Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">
        ///     The new position within the stream. This is relative to the
        ///     <paramref name="loc" /> parameter, and can be positive or negative.
        /// </param>
        /// <param name="loc">A value of type <see cref="SeekOrigin" />, which acts as the seek reference point.</param>
        /// <returns>
        ///     The new position within the stream, calculated by combining the initial reference point and the offset.
        /// </returns>
        /// <exception cref="IOException">
        ///     Thrown when seeking is attempted before the beginning of the stream, including
        ///     when <paramref name="offset" /> overflows the 32-bit position and wraps below zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="loc" /> is not a valid <see cref="SeekOrigin" />.
        /// </exception>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Any non-negative offset is accepted, including offsets beyond <see cref="Capacity" />.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 A subsequent write grows the buffer and zero-initializes the gap.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The position is a 32-bit signed integer.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc) => _handle->Seek(offset, loc);

        /// <summary>
        ///     Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="length">The new length of the stream in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length) => _handle->SetLength(length);

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
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer that receives the bytes read from the stream.</param>
        /// <returns>The number of bytes read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Reads a byte from the current stream.
        /// </summary>
        /// <returns>The next byte from the stream, or -1 if the end of the stream has been reached.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _handle->ReadByte();

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
        ///     Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value) => _handle->WriteByte(value);

        /// <summary>
        ///     Sets the capacity of this to the specified number of bytes.
        /// </summary>
        /// <param name="capacity">The new capacity in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current length.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity) => _handle->SetCapacity(capacity);

        /// <summary>
        ///     Returns a span over a portion of this.
        /// </summary>
        /// <returns>A span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => _handle->AsSpan();

        /// <summary>
        ///     Returns a read-only span over a portion of this.
        /// </summary>
        /// <returns>A read-only span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => _handle->AsReadOnlySpan();
    }
}