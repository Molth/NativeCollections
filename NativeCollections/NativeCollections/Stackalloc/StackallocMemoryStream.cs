using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Stackalloc memory stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StackallocCollection(FromType.Standard)]
    public unsafe struct StackallocMemoryStream : IIsCreated, IEquatable<StackallocMemoryStream>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private readonly byte* _buffer;

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
        private readonly int _capacity;

        /// <summary>
        ///     Is created
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(_buffer);

        /// <summary>
        ///     Is empty
        /// </summary>
        public readonly bool IsEmpty => _length == 0;

        /// <summary>
        ///     Length
        /// </summary>
        public readonly int Length => _length;

        /// <summary>
        ///     Position
        /// </summary>
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
        ///     Capacity
        /// </summary>
        public readonly int Capacity => _capacity;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(index));
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackallocMemoryStream([MustBePinned] Span<byte> buffer)
        {
            _buffer = UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(buffer));
            _capacity = buffer.Length;
            _position = 0;
            _length = 0;
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(StackallocMemoryStream other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is StackallocMemoryStream other && other.Equals(this);

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "StackallocMemoryStream";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(StackallocMemoryStream left, StackallocMemoryStream right) => left.Equals(right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(StackallocMemoryStream left, StackallocMemoryStream right) => !left.Equals(right);

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> GetBuffer() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _capacity);

        /// <summary>
        ///     Seek
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Seek origin</param>
        /// <returns>Position</returns>
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
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetLength(int length)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)length, (uint)int.MaxValue, ExceptionArgument.length);
            ThrowHelpers.ThrowIfStreamTooLong(length);
            if (length > Capacity)
                return false;
            if (length > _length)
                Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(length - _length));
            _length = length;
            _position = Math.Min(_position, length);
            return true;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));
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
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), (uint)n);
            _position += n;
            return n;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <returns>Byte</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _position >= _length ? -1 : Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position++));

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            return Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(ReadOnlySpan<byte> buffer)
        {
            var i = _position + buffer.Length;
            ThrowHelpers.ThrowIfStreamTooLong(i);
            if (i > _length)
            {
                if (i > Capacity)
                    return false;
                var mustZero = _position > _length;
                if (mustZero)
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(i - _length));
                _length = i;
            }

            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
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
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(_position - _length));
                _length = newLength;
            }

            Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position++)) = value;
            return true;
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer), _length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), _length - start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(_buffer), _length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), _length - start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(start)), length);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(StackallocMemoryStream stackallocMemoryStream) => stackallocMemoryStream.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(StackallocMemoryStream stackallocMemoryStream) => stackallocMemoryStream.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static StackallocMemoryStream Empty => new();
    }
}