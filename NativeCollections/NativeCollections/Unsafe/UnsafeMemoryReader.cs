using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe memory reader
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryReader : IIsCreated, IDisposable, IEquatable<UnsafeMemoryReader>
    {
        /// <summary>
        ///     Buffer
        /// </summary>
        public readonly byte* Buffer;

        /// <summary>
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Position
        /// </summary>
        private int _position;

        /// <summary>
        ///     Structure
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeMemoryReader(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => Box.Free(Buffer);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => !UnsafeHelpers.IsNull(Buffer);

        /// <summary>
        ///     Gets the current position within the stream.
        /// </summary>
        public readonly int Position => _position;

        /// <summary>
        ///     Gets the number of remaining bytes available in the bucket.
        /// </summary>
        public readonly int Remaining => Length - _position;

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, index);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
        public readonly byte* this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => UnsafeHelpers.AddByteOffset<byte>(Buffer, (nint)index);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeMemoryReader other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryReader other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeMemoryReader";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeMemoryReader left, UnsafeMemoryReader right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeMemoryReader left, UnsafeMemoryReader right) => !left.Equals(right);

        /// <summary>
        ///     Notifies this that <paramref name="count" /> data items were written to the output.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     The position is set to a negative value or a value greater than
        ///     <see cref="Length"></see>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            var newPosition = _position + count;
            ThrowHelpers.ThrowIfGreaterThan((uint)newPosition, (uint)Length, ExceptionArgument.count);
            _position = newPosition;
        }

        /// <summary>
        ///     Notifies this that <paramref name="count" /> data items were written to the output.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvance(int count)
        {
            var newPosition = _position + count;
            if ((uint)newPosition > (uint)Length)
                return false;
            _position = newPosition;
            return true;
        }

        /// <summary>
        ///     Sets the current position within the stream.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     The position is set to a negative value or a value greater than
        ///     <see cref="Length"></see>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            ThrowHelpers.ThrowIfGreaterThan((uint)position, (uint)Length, ExceptionArgument.position);
            _position = position;
        }

        /// <summary>
        ///     Sets the current position within the stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(int position)
        {
            if ((uint)position > (uint)Length)
                return false;
            _position = position;
            return true;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + Unsafe.SizeOf<T>(), Length, ExceptionArgument._dummy);
            var obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, _position));
            _position += Unsafe.SizeOf<T>();
            return obj;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + Unsafe.SizeOf<T>(), Length, ExceptionArgument.obj);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(obj), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)Unsafe.SizeOf<T>());
            _position += Unsafe.SizeOf<T>();
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj) where T : unmanaged
        {
            if (_position + Unsafe.SizeOf<T>() > Length)
                return false;
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(obj), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)Unsafe.SizeOf<T>());
            _position += Unsafe.SizeOf<T>();
            return true;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ReadSpan(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            return TryReadSpan(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSpan<T>(Span<T> buffer) where T : unmanaged
        {
            var count = buffer.Length * Unsafe.SizeOf<T>();
            ThrowHelpers.ThrowIfGreaterThan(_position + count, Length, ExceptionArgument.buffer);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)count);
            _position += count;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadSpan<T>(Span<T> buffer) where T : unmanaged
        {
            var count = buffer.Length * Unsafe.SizeOf<T>();
            if (_position + count > Length)
                return false;
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)count);
            _position += count;
            return true;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + Unsafe.SizeOf<T>(), Length, ExceptionArgument.obj);
            obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, _position));
            _position += Unsafe.SizeOf<T>();
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(ref T obj) where T : unmanaged
        {
            if (_position + Unsafe.SizeOf<T>() > Length)
                return false;
            obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, _position));
            _position += Unsafe.SizeOf<T>();
            return true;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + length, Length, ExceptionArgument.length);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)length);
            _position += length;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(byte* buffer, int length)
        {
            if (_position + length > Length)
                return false;
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)length);
            _position += length;
            return true;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(Span<byte> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(_position + buffer.Length, Length, ExceptionArgument.buffer);
            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)buffer.Length);
            _position += buffer.Length;
        }

        /// <summary>
        ///     Reads a sequence of bytes from the current memory stream,
        ///     and advances the position within the memory stream by the number of bytes read.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(Span<byte> buffer)
        {
            if (_position + buffer.Length > Length)
                return false;
            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), (uint)buffer.Length);
            _position += buffer.Length;
            return true;
        }

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan(int start, int length) => MemoryMarshal.CreateSpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), Length - start);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan(int start, int length) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(start)), length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator UnsafeMemoryReader([MustBePinned] Span<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<byte>(UnsafeMemoryReader value) => value.AsSpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(nameof(value))]
        public static implicit operator UnsafeMemoryReader([MustBePinned] ReadOnlySpan<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

        /// <summary>
        ///     Creates a new read-only span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(UnsafeMemoryReader value) => value.AsReadOnlySpan();

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryReader(NativeArray<byte> value) => new(value.Buffer, value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryReader(NativeMemoryArray<byte> value) => new(value.Buffer, value.Length);

        /// <summary>
        ///     Creates a new span over a portion of a regular managed object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnsafeMemoryReader(NativeSlice<byte> value) => new(UnsafeHelpers.AddByteOffset<byte>(value.Buffer, value.Offset), value.Count);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeMemoryReader Empty => default;
    }
}