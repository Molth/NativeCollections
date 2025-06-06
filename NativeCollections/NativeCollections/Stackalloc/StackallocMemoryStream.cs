﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2208
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc memory stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe ref struct StackallocMemoryStream
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly Span<byte> _buffer;

        /// <summary>
        ///     Position
        /// </summary>
        private int _position;

        /// <summary>
        ///     Length
        /// </summary>
        private int _length;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _length == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public int Length => _length;

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
        public int Capacity => _buffer.Length;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _buffer[index];
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocMemoryStream(Span<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
            _length = 0;
        }

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetBuffer() => _buffer;

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
        public bool SetLength(int length)
        {
            if ((uint)length > 2147483647)
                throw new ArgumentOutOfRangeException(nameof(length), length, "StreamLength");
            if (length < 0)
                throw new IOException("IO_StreamTooLong");
            if (length > Capacity)
                return false;
            if (length > _length)
            {
                nint offset = _length;
                Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), offset), 0, (uint)(length - _length));
            }

            _length = length;
            if (_position > length)
                _position = length;
            return true;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => Read(MemoryMarshal.CreateSpan(ref *buffer, length));

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
            nint offset = _position;
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), offset), (uint)n);
            _position += n;
            return n;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <returns>Byte</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _length ? -1 : _buffer[_position++];

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int length) => Write(MemoryMarshal.CreateReadOnlySpan(ref *buffer, length));

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(ReadOnlySpan<byte> buffer)
        {
            var i = _position + buffer.Length;
            if (i < 0)
                throw new IOException("IO_StreamTooLong");
            if (i > _length)
            {
                if (i > Capacity)
                    return false;
                var mustZero = _position > _length;
                if (mustZero)
                {
                    nint offset1 = _length;
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), offset1), 0, (uint)(i - _length));
                }

                _length = i;
            }

            nint offset2 = _position;
            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), offset2), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            _position = i;
            return true;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="value">Byte</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool WriteByte(byte value)
        {
            if (_position >= _length)
            {
                var newLength = _position + 1;
                if (newLength > Capacity)
                    return false;
                var mustZero = _position > _length;
                if (mustZero)
                {
                    nint offset = _length;
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(_buffer), offset), 0, (uint)(_position - _length));
                }

                _length = newLength;
            }

            _buffer[_position++] = value;
            return true;
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => _buffer.Slice(0, _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => _buffer.Slice(start, _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => _buffer.Slice(start, length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(in StackallocMemoryStream stackallocMemoryStream) => stackallocMemoryStream.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(in StackallocMemoryStream stackallocMemoryStream) => stackallocMemoryStream.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocMemoryStream Empty => new();
    }
}