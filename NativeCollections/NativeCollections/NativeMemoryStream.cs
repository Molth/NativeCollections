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
    [NativeCollection(NativeCollectionType.Standard)]
    public readonly unsafe struct NativeMemoryStream : IDisposable, IEquatable<NativeMemoryStream>
    {
        /// <summary>
        ///     Handle
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeMemoryStreamHandle
        {
            /// <summary>
            ///     Array
            /// </summary>
            public byte* Array;

            /// <summary>
            ///     Position
            /// </summary>
            public int Position;

            /// <summary>
            ///     Length
            /// </summary>
            public int Length;

            /// <summary>
            ///     Capacity
            /// </summary>
            public int Capacity;

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref byte this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[index];
            }

            /// <summary>
            ///     Get reference
            /// </summary>
            /// <param name="index">Index</param>
            public ref byte this[uint index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref Array[index];
            }

            /// <summary>
            ///     Get buffer
            /// </summary>
            /// <returns>Buffer</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public byte* GetBuffer() => Array;

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
                        Position = offset;
                        break;
                    }
                    case SeekOrigin.Current:
                    {
                        var tempPosition = unchecked(Position + offset);
                        if (tempPosition < 0)
                            throw new IOException("IO_SeekBeforeBegin");
                        Position = tempPosition;
                        break;
                    }
                    case SeekOrigin.End:
                    {
                        var tempPosition = unchecked(Length + offset);
                        if (tempPosition < 0)
                            throw new IOException("IO_SeekBeforeBegin");
                        Position = tempPosition;
                        break;
                    }
                    default:
                        throw new ArgumentException("InvalidSeekOrigin");
                }

                return Position;
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
                var allocatedNewArray = EnsureCapacity(length);
                if (!allocatedNewArray && length > Length)
                    Unsafe.InitBlockUnaligned(Array + Length, 0, (uint)(length - Length));
                Length = length;
                if (Position > length)
                    Position = length;
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
                var n = Length - Position;
                if (n > count)
                    n = count;
                if (n <= 0)
                    return 0;
                if (n <= 8)
                {
                    var byteCount = n;
                    while (--byteCount >= 0)
                        buffer[offset + byteCount] = Array[Position + byteCount];
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(buffer + offset, Array + Position, (uint)n);
                }

                Position += n;
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
                var size = Length - Position;
                var n = size < buffer.Length ? size : buffer.Length;
                if (n <= 0)
                    return 0;
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref *(Array + Position), (uint)n);
                Position += n;
                return n;
            }

            /// <summary>
            ///     Read
            /// </summary>
            /// <returns>Byte</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int ReadByte() => Position >= Length ? -1 : Array[Position++];

            /// <summary>
            ///     Write
            /// </summary>
            /// <param name="buffer">Buffer</param>
            /// <param name="offset">Offset</param>
            /// <param name="count">Count</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(byte* buffer, int offset, int count)
            {
                var i = Position + count;
                if (i < 0)
                    throw new IOException("IO_StreamTooLong");
                if (i > Length)
                {
                    var mustZero = Position > Length;
                    if (i > Capacity)
                    {
                        EnsureCapacity(i);
                        mustZero = false;
                    }

                    if (mustZero)
                        Unsafe.InitBlockUnaligned(Array + Length, 0, (uint)(i - Length));
                    Length = i;
                }

                if (count <= 8 && buffer != Array)
                {
                    var byteCount = count;
                    while (--byteCount >= 0)
                        Array[Position + byteCount] = buffer[offset + byteCount];
                }
                else
                {
                    Unsafe.CopyBlockUnaligned(Array + Position, buffer + offset, (uint)count);
                }

                Position = i;
            }

            /// <summary>
            ///     Write
            /// </summary>
            /// <param name="buffer">Buffer</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write(ReadOnlySpan<byte> buffer)
            {
                var i = Position + buffer.Length;
                if (i < 0)
                    throw new IOException("IO_StreamTooLong");
                if (i > Length)
                {
                    var mustZero = Position > Length;
                    if (i > Capacity)
                    {
                        EnsureCapacity(i);
                        mustZero = false;
                    }

                    if (mustZero)
                        Unsafe.InitBlockUnaligned(Array + Length, 0, (uint)(i - Length));
                    Length = i;
                }

                Unsafe.CopyBlockUnaligned(ref *(Array + Position), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
                Position = i;
            }

            /// <summary>
            ///     Write
            /// </summary>
            /// <param name="value">Byte</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void WriteByte(byte value)
            {
                if (Position >= Length)
                {
                    var newLength = Position + 1;
                    var mustZero = Position > Length;
                    if (newLength > Capacity)
                    {
                        EnsureCapacity(newLength);
                        mustZero = false;
                    }

                    if (mustZero)
                        Unsafe.InitBlockUnaligned(Array + Length, 0, (uint)(Position - Length));
                    Length = newLength;
                }

                Array[Position++] = value;
            }

            /// <summary>
            ///     Set capacity
            /// </summary>
            /// <param name="capacity">Capacity</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetCapacity(int capacity)
            {
                if (capacity < Length)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), capacity, "SmallCapacity");
                if (capacity != Capacity)
                {
                    if (capacity > 0)
                    {
                        var newBuffer = (byte*)NativeMemoryAllocator.Alloc((uint)capacity);
                        if (Length > 0)
                            Unsafe.CopyBlockUnaligned(newBuffer, Array, (uint)Length);
                        NativeMemoryAllocator.Free(Array);
                        Array = newBuffer;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(Array);
                        Array = (byte*)NativeMemoryAllocator.Alloc(0);
                    }

                    Capacity = capacity;
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
                if (capacity < 0)
                    throw new IOException("IO_StreamTooLong");
                if (capacity > Capacity)
                {
                    var newCapacity = capacity > 256 ? capacity : 256;
                    if (newCapacity < Capacity * 2)
                        newCapacity = Capacity * 2;
                    if ((uint)(Capacity * 2) > 2147483591)
                        newCapacity = capacity > 2147483591 ? capacity : 2147483591;
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
            public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref *Array, Length);

            /// <summary>
            ///     As span
            /// </summary>
            /// <param name="start">Start</param>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref *(Array + start), Length - start);

            /// <summary>
            ///     As span
            /// </summary>
            /// <param name="start">Start</param>
            /// <param name="length">Length</param>
            /// <returns>Span</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref *(Array + start), length);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref *Array, Length);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref *(Array + start), Length - start);

            /// <summary>
            ///     As readOnly span
            /// </summary>
            /// <param name="start">Start</param>
            /// <param name="length">Length</param>
            /// <returns>ReadOnlySpan</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref *(Array + start), length);
        }

        /// <summary>
        ///     Handle
        /// </summary>
        private readonly NativeMemoryStreamHandle* _handle;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="capacity">Capacity</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeMemoryStream(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "MustBeNonNegative");
            if (capacity < 4)
                capacity = 4;
            var handle = (NativeMemoryStreamHandle*)NativeMemoryAllocator.Alloc((uint)sizeof(NativeMemoryStreamHandle));
            handle->Array = (byte*)NativeMemoryAllocator.Alloc((uint)capacity);
            handle->Position = 0;
            handle->Length = 0;
            handle->Capacity = capacity;
            _handle = handle;
        }

        /// <summary>
        ///     Is created
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Is empty
        /// </summary>
        public bool IsEmpty => _handle->Length == 0;

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref (*_handle)[index];
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
            get => _handle->Length;
        }

        /// <summary>
        ///     Position
        /// </summary>
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Position), value, "MustBeNonNegative");
                _handle->Position = value;
            }
        }

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Capacity;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->SetCapacity(value);
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeMemoryStream other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeMemoryStream nativeMemoryStream && nativeMemoryStream == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => ((nint)_handle).GetHashCode();

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeMemoryStream";

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(NativeMemoryStream nativeList) => nativeList.AsSpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(NativeMemoryStream nativeList) => nativeList.AsReadOnlySpan();

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeMemoryStream left, NativeMemoryStream right) => left._handle == right._handle;

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeMemoryStream left, NativeMemoryStream right) => left._handle != right._handle;

        /// <summary>
        ///     Dispose
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var handle = _handle;
            if (handle == null)
                return;
            NativeMemoryAllocator.Free(handle->Array);
            NativeMemoryAllocator.Free(handle);
        }

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetBuffer() => _handle->GetBuffer();

        /// <summary>
        ///     Seek
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="loc">Seek origin</param>
        /// <returns>Position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Seek(int offset, SeekOrigin loc) => _handle->Seek(offset, loc);

        /// <summary>
        ///     Set length
        /// </summary>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length) => _handle->SetLength(length);

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(byte* buffer, int offset, int count) => _handle->Read(buffer, offset, count);

        /// <summary>
        ///     Read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Bytes</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Read(Span<byte> buffer) => _handle->Read(buffer);

        /// <summary>
        ///     Read
        /// </summary>
        /// <returns>Byte</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte() => _handle->ReadByte();

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int offset, int count) => _handle->Write(buffer, offset, count);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer) => _handle->Write(buffer);

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="value">Byte</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value) => _handle->WriteByte(value);

        /// <summary>
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => _handle->AsSpan();

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start) => _handle->AsSpan(start);

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length) => _handle->AsSpan(start, length);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan() => _handle->AsReadOnlySpan();

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start) => _handle->AsReadOnlySpan(start);

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => _handle->AsReadOnlySpan(start, length);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryStream Empty => new();
    }
}