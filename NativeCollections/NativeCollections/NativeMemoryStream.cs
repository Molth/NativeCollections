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
    [NativeCollection]
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
            get => ref _handle->Array[index];
        }

        /// <summary>
        ///     Get reference
        /// </summary>
        /// <param name="index">Index</param>
        public ref byte this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _handle->Array[index];
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
            get
            {
                EnsureNotClosed();
                return _handle->Length;
            }
        }

        /// <summary>
        ///     Position
        /// </summary>
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotClosed();
                return _handle->Position;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Position), value, "MustBeNonNegative");
                EnsureNotClosed();
                _handle->Position = value;
            }
        }

        /// <summary>
        ///     Capacity
        /// </summary>
        public int Capacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                EnsureNotClosed();
                return _handle->Capacity;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                EnsureNotClosed();
                var handle = _handle;
                if (value < handle->Length)
                    throw new ArgumentOutOfRangeException(nameof(Capacity), value, "SmallCapacity");
                if (value != handle->Capacity)
                {
                    if (value > 0)
                    {
                        var newBuffer = (byte*)NativeMemoryAllocator.Alloc((uint)value);
                        if (handle->Length > 0)
                            Unsafe.CopyBlockUnaligned(newBuffer, handle->Array, (uint)handle->Length);
                        NativeMemoryAllocator.Free(handle->Array);
                        handle->Array = newBuffer;
                    }
                    else
                    {
                        NativeMemoryAllocator.Free(handle->Array);
                        handle->Array = (byte*)NativeMemoryAllocator.Alloc(0);
                    }

                    handle->Capacity = value;
                }
            }
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
        ///     As span
        /// </summary>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan()
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *handle->Array, handle->Length);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start)
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *(handle->Array + start), handle->Length - start);
        }

        /// <summary>
        ///     As span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan(int start, int length)
        {
            var handle = _handle;
            return MemoryMarshal.CreateSpan(ref *(handle->Array + start), length);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan()
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *handle->Array, handle->Length);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + start), handle->Length - start);
        }

        /// <summary>
        ///     As readOnly span
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="length">Length</param>
        /// <returns>ReadOnlySpan</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsReadOnlySpan(int start, int length)
        {
            var handle = _handle;
            return MemoryMarshal.CreateReadOnlySpan(ref *(handle->Array + start), length);
        }

        /// <summary>
        ///     Get buffer
        /// </summary>
        /// <returns>Buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetBuffer() => _handle->Array;

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
            EnsureNotClosed();
            var handle = _handle;
            switch (loc)
            {
                case SeekOrigin.Begin:
                {
                    if (offset < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    handle->Position = offset;
                    break;
                }
                case SeekOrigin.Current:
                {
                    var tempPosition = unchecked(handle->Position + offset);
                    if (tempPosition < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    handle->Position = tempPosition;
                    break;
                }
                case SeekOrigin.End:
                {
                    var tempPosition = unchecked(handle->Length + offset);
                    if (tempPosition < 0)
                        throw new IOException("IO_SeekBeforeBegin");
                    handle->Position = tempPosition;
                    break;
                }
                default:
                    throw new ArgumentException("InvalidSeekOrigin");
            }

            return handle->Position;
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
            EnsureNotClosed();
            var handle = _handle;
            var allocatedNewArray = EnsureCapacity(length);
            if (!allocatedNewArray && length > handle->Length)
                Unsafe.InitBlockUnaligned(handle->Array + handle->Length, 0, (uint)(length - handle->Length));
            handle->Length = length;
            if (handle->Position > length)
                handle->Position = length;
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
            EnsureNotClosed();
            var handle = _handle;
            var n = handle->Length - handle->Position;
            if (n > count)
                n = count;
            if (n <= 0)
                return 0;
            if (n <= 8)
            {
                var byteCount = n;
                while (--byteCount >= 0)
                    buffer[offset + byteCount] = handle->Array[handle->Position + byteCount];
            }
            else
            {
                Unsafe.CopyBlockUnaligned(buffer + offset, handle->Array + handle->Position, (uint)n);
            }

            handle->Position += n;
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
            EnsureNotClosed();
            var handle = _handle;
            var size = handle->Length - handle->Position;
            var n = size < buffer.Length ? size : buffer.Length;
            if (n <= 0)
                return 0;
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(buffer), ref *(handle->Array + handle->Position), (uint)n);
            handle->Position += n;
            return n;
        }

        /// <summary>
        ///     Read
        /// </summary>
        /// <returns>Byte</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadByte()
        {
            EnsureNotClosed();
            var handle = _handle;
            return handle->Position >= handle->Length ? -1 : handle->Array[handle->Position++];
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* buffer, int offset, int count)
        {
            EnsureNotClosed();
            var handle = _handle;
            var i = handle->Position + count;
            if (i < 0)
                throw new IOException("IO_StreamTooLong");
            if (i > handle->Length)
            {
                var mustZero = handle->Position > handle->Length;
                if (i > handle->Capacity)
                {
                    EnsureCapacity(i);
                    mustZero = false;
                }

                if (mustZero)
                    Unsafe.InitBlockUnaligned(handle->Array + handle->Length, 0, (uint)(i - handle->Length));
                handle->Length = i;
            }

            if (count <= 8 && buffer != handle->Array)
            {
                var byteCount = count;
                while (--byteCount >= 0)
                    handle->Array[handle->Position + byteCount] = buffer[offset + byteCount];
            }
            else
            {
                Unsafe.CopyBlockUnaligned(handle->Array + handle->Position, buffer + offset, (uint)count);
            }

            handle->Position = i;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureNotClosed();
            var handle = _handle;
            var i = handle->Position + buffer.Length;
            if (i < 0)
                throw new IOException("IO_StreamTooLong");
            if (i > handle->Length)
            {
                var mustZero = handle->Position > handle->Length;
                if (i > handle->Capacity)
                {
                    EnsureCapacity(i);
                    mustZero = false;
                }

                if (mustZero)
                    Unsafe.InitBlockUnaligned(handle->Array + handle->Length, 0, (uint)(i - handle->Length));
                handle->Length = i;
            }

            Unsafe.CopyBlockUnaligned(ref *(handle->Array + handle->Position), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);
            handle->Position = i;
        }

        /// <summary>
        ///     Write
        /// </summary>
        /// <param name="value">Byte</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            EnsureNotClosed();
            var handle = _handle;
            if (handle->Position >= handle->Length)
            {
                var newLength = handle->Position + 1;
                var mustZero = handle->Position > handle->Length;
                if (newLength > handle->Capacity)
                {
                    EnsureCapacity(newLength);
                    mustZero = false;
                }

                if (mustZero)
                    Unsafe.InitBlockUnaligned(handle->Array + handle->Length, 0, (uint)(handle->Position - handle->Length));
                handle->Length = newLength;
            }

            handle->Array[handle->Position++] = value;
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
            var handle = _handle;
            if (capacity > handle->Capacity)
            {
                var newCapacity = capacity > 256 ? capacity : 256;
                if (newCapacity < handle->Capacity * 2)
                    newCapacity = handle->Capacity * 2;
                if ((uint)(handle->Capacity * 2) > 2147483591)
                    newCapacity = capacity > 2147483591 ? capacity : 2147483591;
                Capacity = newCapacity;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Ensure not closed
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotClosed()
        {
            if (_handle == null)
                throw new ObjectDisposedException("StreamClosed");
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeMemoryStream Empty => new();
    }
}