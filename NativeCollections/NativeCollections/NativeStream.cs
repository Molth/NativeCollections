using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native memory stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public unsafe struct NativeStream : IEquatable<NativeStream>
    {
        /// <summary>
        ///     Array
        /// </summary>
        private byte* _array;

        /// <summary>
        ///     Position
        /// </summary>
        private int _position;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStream(byte* buffer, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "MustBeNonNegative");
            _array = buffer;
            _position = 0;
            _length = length;
            _capacity = length;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _array != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _array[index];
        }

        /// <summary>
        ///     Can read
        /// </summary>
        public bool CanRead => IsCreated;

        /// <summary>
        ///     Can seek
        /// </summary>
        public bool CanSeek => IsCreated;

        /// <summary>
        ///     Can write
        /// </summary>
        public bool CanWrite => IsCreated;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        /// <summary>
        ///     Position
        /// </summary>
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Position), value, "MustBeNonNegative");
                _position = value;
            }
        }

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeStream other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeStream nativeStream && nativeStream == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_array).GetHashCode() ^ _position ^ _length ^ _capacity;

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeStream";

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeStream nativeList) => nativeList.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeStream nativeList) => nativeList.AsReadOnlySpan();

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeStream left, NativeStream right) => left._array == right._array && left._position == right._position && left._length == right.Length && left._capacity == right._capacity;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeStream left, NativeStream right) => left._array != right._array || left._position != right._position || left._length != right.Length || left._capacity != right._capacity;

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref *_array, _length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(_array + start), _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(_array + start), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *_array, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(_array + start), length);

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetBuffer() => _array;

        /// <summary>
        ///     Seek
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Seek origin</param>
        /// <returns>Position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc)
        {
            if (offset > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "StreamLength");
            switch (loc)
            {
                case SeekOrigin.Begin:
                {
                    if (offset < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    _position = offset;
                    break;
                }
                case SeekOrigin.Current:
                {
                    var tempPosition = unchecked(_position + offset);
                    if (tempPosition < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    _position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    var tempPosition = unchecked(_length + offset);
                    if (tempPosition < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    _position = tempPosition;
                    break;
                }
                default:
                    throw new ArgumentException("InvalidSeekOrigin");
            }

            return _position;
        }

        /// <summary>
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            if (length < 0 || length > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(length), length, "StreamLength");
            if (length > _capacity)
                throw new ArgumentOutOfRangeException(nameof(length), length, "StreamLength");
            if (length > _length)
                Unsafe.InitBlockUnaligned(_array + _length, 0, (uint)(length - _length));
            _length = length;
            if (_position > length)
                _position = length;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int offset, int count)
        {
            var n = _length - _position;
            if (n > count)
                n = count;
            if (n <= 0)
                return 0;
            if (n <= 8)
            {
                var byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = _array[_position + byteCount];
            }
            else
            {
                Unsafe.CopyBlockUnaligned(buffer + offset, _array + _position, (uint)n);
            }

            _position += n;
            return n;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer)
        {
            var size = _length - _position;
            var n = size < buffer.Length ? size : buffer.Length;
            if (n <= 0)
                return 0;
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref *(_array + _position), (uint)n);
            _position += n;
            return n;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <returns>Byte</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _length ? -1 : _array[_position++];

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int offset, int count)
        {
            var i = _position + count;
            if (i < 0)
                throw new IOException("IO_StreamTooLong");
            if (i > _length)
            {
                if (i > _capacity)
                    throw new ArgumentOutOfRangeException(nameof(count), count, "StreamLength");
                var mustZero = _position > _length;
                if (mustZero)
                    Unsafe.InitBlockUnaligned(_array + _length, 0, (uint)(i - _length));
                _length = i;
            }

            if (count <= 8 && buffer != _array)
            {
                var byteCount = count;
                while (--byteCount >= 0)
                    _array[_position + byteCount] = buffer[offset + byteCount];
            }
            else
            {
                Unsafe.CopyBlockUnaligned(_array + _position, buffer + offset, (uint)count);
            }

            _position = i;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            var i = _position + buffer.Length;
            if (i < 0)
                throw new IOException("IO_StreamTooLong");
            if (i > _length)
            {
                if (i > _capacity)
                    throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, "StreamLength");
                var mustZero = _position > _length;
                if (mustZero)
                    Unsafe.InitBlockUnaligned(_array + _length, 0, (uint)(i - _length));
                _length = i;
            }

            Unsafe.CopyBlockUnaligned(ref *(_array + _position), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            _position = i;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="value">Byte</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            if (_position >= _length)
            {
                var newLength = _position + 1;
                if (newLength > _capacity)
                    throw new ArgumentOutOfRangeException(nameof(value), 1, "StreamLength");
                var mustZero = _position > _length;
                if (mustZero)
                    Unsafe.InitBlockUnaligned(_array + _length, 0, (uint)(_position - _length));
                _length = newLength;
            }

            _array[_position++] = value;
        }

        /// <summary>
        ///     As native stream
        /// </summary>
        /// <returns>NativeStream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeStream(Span<byte> span) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     As native stream
        /// </summary>
        /// <returns>NativeStream</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NativeStream(ReadOnlySpan<byte> span) => new((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeStream Empty => new();
    }
}