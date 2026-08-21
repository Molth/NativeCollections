using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Creates a stream whose backing store is memory.
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
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _length;

        /// <summary>
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Gets a value that indicates whether this is empty.
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Gets or sets the current position within the stream.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     The position is set to a negative value or a value greater than
        ///     <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.
        /// </exception>
        /// <returns>The current position within the stream.</returns>
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
        ///     Gets the total numbers of elements the internal data structure can hold.
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(index));
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Initializes a new instance of the class with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The initial number of elements that the instance can hold.
        ///     Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryStream(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, ExceptionArgument.capacity);
            capacity = Math.Max(capacity, 4);
            _buffer = NativeMemoryAllocator.AlignedAlloc<byte>((uint)capacity);
            _position = 0;
            _length = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeMemoryStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryStream other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeMemoryStream";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryStream left, UnsafeMemoryStream right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryStream left, UnsafeMemoryStream right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Returns the array of unsigned bytes from which this stream was created.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> GetBuffer() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _capacity);

        /// <summary>
        ///     Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">
        ///     The new position within the stream. This is relative to the <paramref name="loc" /> parameter, and
        ///     can be positive or negative.
        /// </param>
        /// <param name="loc">A value of type <see cref="T:System.IO.SeekOrigin" />, which acts as the seek reference point.</param>
        /// <exception cref="T:System.IO.IOException">Seeking is attempted before the beginning of the stream.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="offset" /> is greater than <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     There is an invalid <see cref="T:System.IO.SeekOrigin" />.
        ///     -or- <paramref name="offset" /> caused an arithmetic overflow.
        /// </exception>
        /// <returns>The new position within the stream, calculated by combining the initial reference point and the offset.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc)
        {
            ThrowHelpers.ThrowIfGreaterThan(offset, int.MaxValue, ExceptionArgument.offset);
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
                    var tempPosition = unchecked(_position + offset);
                    ThrowHelpers.ThrowIfSeekBeforeBegin(tempPosition);
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    var tempPosition = unchecked(_length + offset);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)length, (uint)int.MaxValue, ExceptionArgument.length);
            var allocatedNewArray = EnsureCapacity(length);
            if (!allocatedNewArray && length > _length)
                SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(length - _length));
            _length = length;
            _position = Math.Min(_position, length);
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            var size = _length - _position;
            var n = size < buffer.Length ? size : buffer.Length;
            if (n <= 0)
                return 0;
            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), (uint)n);
            _position += n;
            return n;
        }

        /// <summary>
        ///     Reads a byte from the current stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _length ? -1 : Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position++));

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream,
        ///     and advances the current position within this memory stream by the number of bytes written.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            var i = _position + buffer.Length;
            ThrowHelpers.ThrowIfStreamTooLong(i);
            if (i > _length)
            {
                var mustZero = _position > _length;
                if (i > _capacity)
                {
                    EnsureCapacity(i);
                    mustZero = false;
                }

                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(i - _length));
                _length = i;
            }

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            _position = i;
        }

        /// <summary>
        ///     Writes a byte to the current stream at the current position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            if (_position >= _length)
            {
                var newLength = _position + 1;
                var mustZero = _position > _length;
                if (newLength > _capacity)
                {
                    EnsureCapacity(newLength);
                    mustZero = false;
                }

                if (mustZero)
                    SpanHelpers.Set(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(_position - _length));
                _length = newLength;
            }

            Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position++)) = value;
        }

        /// <summary>
        ///     Sets the capacity of this to the specified number of entries.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Passed capacity is lower than entries count.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, ExceptionArgument.capacity);
            if (capacity != _capacity)
            {
                var newBuffer = NativeMemoryAllocator.AlignedAlloc<byte>((uint)capacity);
                if (_length > 0)
                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(newBuffer), ref Unsafe.AsRef<byte>(_buffer), (uint)_length);
                NativeMemoryAllocator.AlignedFree(_buffer);
                _buffer = newBuffer;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Ensures that the stream's internal buffer
        ///     is large enough to accommodate the specified capacity,
        ///     reallocating and copying data if necessary.
        /// </summary>
        /// <param name="capacity">The minimum required capacity in bytes.</param>
        /// <returns>
        ///     <see langword="true" /> if the buffer was reallocated to a larger size;
        ///     otherwise, <see langword="false" /> if the current capacity already satisfies the request.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfStreamTooLong(capacity);
            if (capacity > _capacity)
            {
                var newCapacity = CollectionHelpers.EnsureCapacity(_capacity, 256);
                SetCapacity(newCapacity);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), _length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(_buffer), _length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), _length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(UnsafeMemoryStream value) => value.AsSpan();

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(UnsafeMemoryStream value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeMemoryStream Empty => default;
    }
}