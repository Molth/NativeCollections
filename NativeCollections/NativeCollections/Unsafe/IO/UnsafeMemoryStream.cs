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
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeMemoryStream : IIsCreated, IDisposable, IEquatable<UnsafeMemoryStream>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private byte* _buffer;

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
        private int _capacity;

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
            readonly get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelpers.ThrowIfNegative(value, ExceptionArgument.value);
                _position = value;
            }
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
            readonly get => _capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

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
        ///     Initializes a new instance of this class with the specified state.
        /// </summary>
        /// <param name="buffer">The buffer that backs the stream.</param>
        /// <param name="position">The current position within the stream.</param>
        /// <param name="length">The number of bytes written to the stream.</param>
        /// <param name="capacity">The total number of elements the internal data structure can hold.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryStream(byte* buffer, int position, int length, int capacity)
        {
            _buffer = buffer;
            _position = position;
            _length = length;
            _capacity = capacity;
        }

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
        /// <remarks>
        ///     The requested capacity is clamped to a minimum of 2; read <c>Capacity</c> for the effective value.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryStream Create(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 2);
            var buffer = NativeMemoryAllocator.AlignedAlloc<byte>(Usize.FromI32Unchecked(capacity));
            return new UnsafeMemoryStream(buffer, 0, 0, capacity);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeMemoryStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeMemoryStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryStream left, UnsafeMemoryStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryStream left, UnsafeMemoryStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            EnsureCapacity(length);
            if (length > _length)
                SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(length.WrappingSub(_length)));

            _length = length;
            _position = Math.Min(_position, length);
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
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write([MustNotSelfAlias] byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer containing the bytes to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting position overflows the 32-bit range and wraps below zero,
        ///     that is when the write would advance the position beyond <see cref="int.MaxValue" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write([MustNotSelfAlias] ReadOnlySpan<byte> buffer)
        {
            var i = _position.WrappingAdd(buffer.Length);
            ThrowHelpers.ThrowIfStreamTooLong(i);
            if (i > _length)
            {
                var mustZero = _position > _length;
                if (i > _capacity)
                    EnsureCapacity(i);

                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(i.WrappingSub(_length)));

                _length = i;
            }

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), Usize.FromI32Unchecked(buffer.Length));
            _position = i;
        }

        /// <summary>
        ///     Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        /// <exception cref="IOException">
        ///     Thrown when the resulting stream length exceeds <see cref="int.MaxValue" /> bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            if (_position >= _length)
            {
                if (_position == int.MaxValue)
                    ThrowHelpers.ThrowStreamTooLongException();

                var newLength = _position.WrappingAdd(1);
                var mustZero = _position > _length;
                if (newLength > _capacity)
                    EnsureCapacity(newLength);

                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(_position.WrappingSub(_length)));

                _length = newLength;
            }

            Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr((_position = _position.WrappingAdd(1)).WrappingSub(1))) = value;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of bytes.
        /// </summary>
        /// <param name="capacity">The new capacity in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="capacity" /> is less than the current length.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var newBuffer = NativeMemoryAllocator.AlignedAlloc<byte>(Usize.FromI32Unchecked(capacity));
                if (_length > 0)
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newBuffer), ref Unsafe.AsRef<byte>(_buffer), Usize.FromI32Unchecked(_length));

                NativeMemoryAllocator.AlignedFree(_buffer);
                _buffer = newBuffer;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Ensures that the stream's internal buffer is large enough to accommodate
        ///     the specified capacity, reallocating and copying data if necessary.
        /// </summary>
        /// <param name="capacity">The minimum required capacity in bytes.</param>
        /// <returns>
        ///     <see langword="true" /> if the buffer was reallocated to a larger size; otherwise,
        ///     <see langword="false" /> if the current capacity already satisfies the request.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfStreamTooLong(capacity);
            if (capacity > _capacity)
            {
                var newCapacity = CollectionHelpers.GrowCapacity(_capacity, capacity);
                SetCapacity(newCapacity);
                return true;
            }

            return false;
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