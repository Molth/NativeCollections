using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a stream whose backing store is the caller-provided memory buffer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocMemoryStream : IIsCreated, IEquatable<StackallocMemoryStream>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private readonly byte* _buffer;

        /// <summary>
        ///     Gets the current position within the stream.
        /// </summary>
        private int _position;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets the total number of bytes the internal data structure can hold.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer != null;

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets or sets the current position within the stream.
        /// </summary>
        /// <value>The current position within the stream.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the position is set to a negative value.
        /// </exception>
        /// <remarks>
        ///     Any non-negative value is accepted; the position is not constrained by
        ///     <see cref="Capacity" />. Positions beyond the capacity cause subsequent
        ///     writes to fail.
        /// </remarks>
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelpers.ThrowIfNegative(value, ExceptionArgument.value);
                _position = value;
            }
        }

        /// <summary>
        ///     Gets the total number of bytes the internal data structure can hold.
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Gets a reference to the byte at the specified byte offset.
        /// </summary>
        /// <param name="index">The byte offset from the start of the buffer at which to access a byte.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Capacity" />.
        /// </exception>
        public readonly ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_capacity), ExceptionArgument.index);
                return ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(index));
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class that uses a caller-provided byte buffer as storage.
        /// </summary>
        /// <param name="buffer">The byte buffer to use as underlying storage.</param>
        /// <returns>A memory stream that uses <paramref name="buffer" /> as its storage.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StackallocMemoryStream FromBytes([MustBePinned] Span<byte> buffer) => new(buffer);

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="buffer">The byte buffer to use as underlying storage.</param>
        private StackallocMemoryStream([MustBePinned] Span<byte> buffer)
        {
            _buffer = UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _capacity = buffer.Length;
            _position = 0;
            _length = 0;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(StackallocMemoryStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is StackallocMemoryStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "StackallocMemoryStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(StackallocMemoryStream left, StackallocMemoryStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(StackallocMemoryStream left, StackallocMemoryStream right) => !left.Equals(right);

        /// <summary>
        ///     Returns a writable span over the entire capacity buffer of this stream.
        /// </summary>
        /// <returns>
        ///     A span covering <see cref="Capacity" /> bytes. Bytes beyond <see cref="Length" />
        ///     are included and are not guaranteed to be zeroed.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> GetBuffer() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _capacity);

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
        ///     Any non-negative offset is accepted, including offsets beyond <see cref="Capacity" />;
        ///     writes at such positions later fail. The position is a 32-bit signed integer.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc)
        {
            switch (loc)
            {
                case SeekOrigin.Begin:
                {
                    ThrowHelpers.ThrowIfSeekBeforeBegin(offset);
                    _position = offset;
                    break;
                }

                case SeekOrigin.Current:
                {
                    var tempPosition = _position.WrappingAdd(offset);
                    ThrowHelpers.ThrowIfSeekBeforeBegin(tempPosition);
                    _position = tempPosition;
                    break;
                }

                case SeekOrigin.End:
                {
                    var tempPosition = _length.WrappingAdd(offset);
                    ThrowHelpers.ThrowIfSeekBeforeBegin(tempPosition);
                    _position = tempPosition;
                    break;
                }

                default:
                {
                    ThrowHelpers.ThrowInvalidSeekOriginException();
                    return default;
                }
            }

            return _position;
        }

        /// <summary>
        ///     Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="length">The new length of the stream in bytes.</param>
        /// <returns>
        ///     <see langword="true" /> if the length was set; otherwise,
        ///     <see langword="false" /> if the specified length exceeds the capacity.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLength(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (length > Capacity)
                return false;

            if (length > _length)
                SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(length.WrappingSub(_length)));

            _length = length;
            _position = Math.Min(_position, length);
            return true;
        }

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
        public int Read(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream, and advances
        ///     the position within the memory stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer that receives the bytes read from the stream.</param>
        /// <returns>The number of bytes read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            var size = _length.WrappingSub(_position);
            var n = size < buffer.Length ? size : buffer.Length;
            if (n <= 0)
                return 0;

            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), Usize.FromI32Unchecked(n));
            _position = _position.WrappingAdd(n);
            return n;
        }

        /// <summary>
        ///     Reads a byte from the current stream.
        /// </summary>
        /// <returns>The next byte from the stream, or -1 if the end of the stream has been reached.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _length ? -1 : I32.FromU8Unchecked(Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr((_position = _position.WrappingAdd(1)).WrappingSub(1))));

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">A pointer to the buffer containing the bytes to write to the stream.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <returns>
        ///     <see langword="true" /> if all bytes were written; otherwise,
        ///     <see langword="false" /> if the stream does not have sufficient capacity.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer containing the bytes to write to the stream.</param>
        /// <returns>
        ///     <see langword="true" /> if all bytes were written; otherwise,
        ///     <see langword="false" /> if the stream does not have sufficient capacity.
        /// </returns>
        /// <exception cref="IOException">
        ///     Thrown when the resulting position overflows the 32-bit range and wraps below zero,
        ///     that is when the write would advance the position beyond <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(ReadOnlySpan<byte> buffer)
        {
            var i = _position.WrappingAdd(buffer.Length);
            ThrowHelpers.ThrowIfStreamTooLong(i);
            if (i > _length)
            {
                if (i > Capacity)
                    return false;

                var mustZero = _position > _length;
                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(i.WrappingSub(_length)));

                _length = i;
            }

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), Usize.FromI32Unchecked(buffer.Length));
            _position = i;
            return true;
        }

        /// <summary>
        ///     Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <returns><see langword="true" /> if the byte was written; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteByte(byte value)
        {
            if (_position >= _length)
            {
                if (_position == int.MaxValue)
                    return false;

                var newLength = _position.WrappingAdd(1);
                if (newLength > Capacity)
                    return false;

                var mustZero = _position > _length;
                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(_position.WrappingSub(_length)));

                _length = newLength;
            }

            Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr((_position = _position.WrappingAdd(1)).WrappingSub(1))) = value;
            return true;
        }

        /// <summary>
        ///     Returns a span over the entire contents of this.
        /// </summary>
        /// <returns>A span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _length);

        /// <summary>
        ///     Returns a read-only span over the entire contents of this.
        /// </summary>
        /// <returns>A read-only span over the entire contents of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(_buffer), _length);
    }
}