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
    ///     Unsafe memory stream
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeMemoryStream : IDisposable
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        private byte* _buffer;

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
                ThrowHelpers.ThrowIfNegative(value, nameof(value));
                _position = value;
            }
        }

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetCapacity(value);
        }

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
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public readonly ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), (nint)index);
        }

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryStream(int capacity)
        {
            ThrowHelpers.ThrowIfNegative(capacity, nameof(capacity));
            capacity = Math.Max(capacity, 4);
            _buffer = NativeMemoryAllocator.AlignedAlloc<byte>((uint)capacity);
            _position = 0;
            _length = 0;
            _capacity = capacity;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(_buffer);

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte* GetBuffer() => _buffer;

        /// <summary>
        ///     Seek
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Seek origin</param>
        /// <returns>Position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc)
        {
            ThrowHelpers.ThrowIfGreaterThan(offset, 2147483647, nameof(offset));
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
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)length, 2147483647U, nameof(length));
            var allocatedNewArray = EnsureCapacity(length);
            if (!allocatedNewArray && length > _length)
                Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(length - _length));
            _length = length;
            _position = Math.Min(_position, length);
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int length) => Read(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(buffer), length));

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
        public void Write(byte* buffer, int length) => Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(buffer), length));

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
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
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(i - _length));
                _length = i;
            }

            Unsafe.CopyBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
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
                var mustZero = _position > _length;
                if (newLength > _capacity)
                {
                    EnsureCapacity(newLength);
                    mustZero = false;
                }

                if (mustZero)
                    Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_length)), 0, (uint)(_position - _length));
                _length = newLength;
            }

            Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(_buffer), new IntPtr(_position++)) = value;
        }

        /// <summary>
        ///     Set capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfLessThan(capacity, _length, nameof(capacity));
            if (capacity != _capacity)
            {
                var newBuffer = NativeMemoryAllocator.AlignedAlloc<byte>((uint)capacity);
                if (_length > 0)
                    Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(newBuffer), ref Unsafe.AsRef<byte>(_buffer), (uint)_length);
                NativeMemoryAllocator.AlignedFree(_buffer);
                _buffer = newBuffer;
                _capacity = capacity;
            }
        }

        /// <summary>
        ///     Ensure capacity
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <returns>Ensured</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EnsureCapacity(int capacity)
        {
            ThrowHelpers.ThrowIfStreamTooLong(capacity);
            if (capacity > _capacity)
            {
                var newCapacity = Math.Max(capacity, 256);
                var expected = _capacity * 2;
                newCapacity = Math.Max(newCapacity, expected);
                if ((uint)expected > 2147483591)
                    newCapacity = Math.Max(capacity, 2147483591);
                SetCapacity(newCapacity);
                return true;
            }

            return false;
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
        public static implicit operator Span<byte>(UnsafeMemoryStream unsafeMemoryStream) => unsafeMemoryStream.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(UnsafeMemoryStream unsafeMemoryStream) => unsafeMemoryStream.AsReadOnlySpan();

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryStream Empty => new();
    }
}