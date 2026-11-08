using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a sequential writer over a native memory buffer with position tracking.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryWriter : IIsCreated, IDisposable, IEquatable<UnsafeMemoryWriter>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly byte* Buffer;

        /// <summary>
        ///     Gets the length of the buffer in bytes.
        /// </summary>
        public readonly int Length;

        /// <summary>
        ///     Gets the current position within the buffer.
        /// </summary>
        private int _position;

        /// <summary>
        ///     Initializes a new instance of this class with the specified buffer and length.
        /// </summary>
        /// <param name="buffer">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="length">
        ///     The size of the buffer in bytes. Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryWriter(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Creates a writer view over the specified native buffer.
        /// </summary>
        /// <param name="buffer">Represents a contiguous region of arbitrary memory.</param>
        /// <param name="length">The size of the buffer in bytes. Must be non-negative.</param>
        /// <returns>
        ///     A writer over <paramref name="buffer" /> whose total size is
        ///     <paramref name="length" /> bytes and whose current position is zero.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryWriter From(byte* buffer, int length) => new(buffer, length);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => NativeMemoryAllocator.AlignedFree(Buffer);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => Buffer != null;

        /// <summary>
        ///     Gets the current position within the buffer.
        /// </summary>
        public readonly int Position => _position;

        /// <summary>
        ///     Gets the number of remaining bytes in the buffer.
        /// </summary>
        public readonly int Remaining => Length.WrappingSub(_position);

        /// <summary>
        ///     Gets a pointer to the byte at the specified byte offset.
        /// </summary>
        /// <param name="index">The byte offset from the start of the buffer at which to access a byte.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public readonly byte* this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(Length), ExceptionArgument.index);
                return UnsafeHelpers.AddByteOffset<byte>(Buffer, Isize.FromI32Unchecked(index));
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeMemoryWriter other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryWriter other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeMemoryWriter";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryWriter left, UnsafeMemoryWriter right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryWriter left, UnsafeMemoryWriter right) => !left.Equals(right);

        /// <summary>
        ///     Advances the current position by <paramref name="count" /> bytes.
        /// </summary>
        /// <param name="count">The number of bytes by which to advance the position.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="count" /> is negative or the resulting position
        ///     would exceed <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            var newPosition = _position.WrappingAdd(count);
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(newPosition), U32.FromI32Unchecked(Length), ExceptionArgument.count);
            _position = newPosition;
        }

        /// <summary>
        ///     Attempts to advance the current position by <paramref name="count" /> bytes.
        /// </summary>
        /// <param name="count">The number of bytes by which to advance the position.</param>
        /// <returns>
        ///     <see langword="true" /> if the position was advanced; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A negative <paramref name="count" /> is rejected: the method returns
        ///     <see langword="false" /> and the position is not changed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvance(int count)
        {
            if (count < 0)
                return false;

            var newPosition = _position.WrappingAdd(count);
            if (U32.FromI32Unchecked(newPosition) > U32.FromI32Unchecked(Length))
                return false;

            _position = newPosition;
            return true;
        }

        /// <summary>
        ///     Sets the current position within the buffer.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the position is set to a negative value or a value greater than <see cref="Length" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int position)
        {
            ThrowHelpers.ThrowIfGreaterThan(U32.FromI32Unchecked(position), U32.FromI32Unchecked(Length), ExceptionArgument.position);
            _position = position;
        }

        /// <summary>
        ///     Attempts to set the current position within the buffer.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <returns><see langword="true" /> if the position was set; otherwise, <see langword="false" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetPosition(int position)
        {
            if (U32.FromI32Unchecked(position) > U32.FromI32Unchecked(Length))
                return false;

            _position = position;
            return true;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
        /// <param name="obj">The source value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining space is insufficient to write a value of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T* obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())), I64.FromI32Unchecked(Length), ExceptionArgument.obj);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(obj), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
        /// <param name="obj">The source value to write.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was written; otherwise,
        ///     <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(T* obj) where T : unmanaged
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(obj), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
            return true;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to write.</typeparam>
        /// <param name="obj">The source values to write.</param>
        /// <param name="count">The number of values to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="count" /> is negative or the remaining space is insufficient
        ///     to write <paramref name="count" /> values of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            WriteSpan(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to write.</typeparam>
        /// <param name="count">The number of values to write.</param>
        /// <param name="obj">The source values to write.</param>
        /// <returns>
        ///     <see langword="true" /> if all values were written; otherwise,
        ///     <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            return TryWriteSpan(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the source buffer.</typeparam>
        /// <param name="buffer">The source buffer containing the values to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining space is insufficient to write <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> buffer) where T : unmanaged
        {
            var count = I64.FromI32Unchecked(buffer.Length).WrappingMul(I64.FromI32Unchecked(Unsafe.SizeOf<T>()));
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(count), I64.FromI32Unchecked(Length), ExceptionArgument.buffer);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI64Unchecked(count));
            _position = _position.WrappingAdd(I32.FromI64Unchecked(count));
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the source buffer.</typeparam>
        /// <param name="buffer">The source buffer containing the values to write.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="buffer" /> was written;
        ///     otherwise, <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteSpan<T>(ReadOnlySpan<T> buffer) where T : unmanaged
        {
            var count = I64.FromI32Unchecked(buffer.Length).WrappingMul(I64.FromI32Unchecked(Unsafe.SizeOf<T>()));
            if (I64.FromI32Unchecked(_position).WrappingAdd(count) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), Usize.FromI64Unchecked(count));
            _position = _position.WrappingAdd(I32.FromI64Unchecked(count));
            return true;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
        /// <param name="obj">The source value to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining space is insufficient to write a value of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())), I64.FromI32Unchecked(Length), ExceptionArgument.obj);
            Unsafe.WriteUnaligned(UnsafeHelpers.AddByteOffset(Buffer, new IntPtr(_position)), obj);
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to write.</typeparam>
        /// <param name="obj">The source value to write.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was written; otherwise,
        ///     <see langword="false" /> if the remaining space is insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWrite<T>(T obj) where T : unmanaged
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())) > I64.FromI32Unchecked(Length))
                return false;

            Unsafe.WriteUnaligned(UnsafeHelpers.AddByteOffset(Buffer, new IntPtr(_position)), obj);
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
            return true;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The source buffer containing the bytes to write.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or the remaining
        ///     space is insufficient to write <paramref name="length" /> bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(length)), I64.FromI32Unchecked(Length), ExceptionArgument.length);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(buffer), Usize.FromI32Unchecked(length));
            _position = _position.WrappingAdd(length);
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The source buffer containing the bytes to write.</param>
        /// <param name="length">The number of bytes to write.</param>
        /// <returns>
        ///     <see langword="true" /> if the requested bytes were
        ///     written successfully; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(length)) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref Unsafe.AsRef<byte>(buffer), Usize.FromI32Unchecked(length));
            _position = _position.WrappingAdd(length);
            return true;
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The source buffer containing the bytes to write.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining space is insufficient to write <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(buffer.Length)), I64.FromI32Unchecked(Length), ExceptionArgument.buffer);
            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), Usize.FromI32Unchecked(buffer.Length));
            _position = _position.WrappingAdd(buffer.Length);
        }

        /// <summary>
        ///     Writes the sequence of bytes into the current memory stream, and advances the
        ///     current position within this memory stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The source buffer containing the bytes to write.</param>
        /// <returns>
        ///     <see langword="true" /> if the requested bytes were
        ///     written successfully; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryWriteBytes(ReadOnlySpan<byte> buffer)
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(buffer.Length)) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), ref MemoryMarshal.GetReference(buffer), Usize.FromI32Unchecked(buffer.Length));
            _position = _position.WrappingAdd(buffer.Length);
            return true;
        }

        /// <summary>
        ///     Clears the contents of this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _position = 0;

        /// <summary>
        ///     Returns a span over a portion of this.
        /// </summary>
        /// <returns>A span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), _position);

        /// <summary>
        ///     Returns a read-only span over a portion of this.
        /// </summary>
        /// <returns>A read-only span over a portion of this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), _position);

        /// <summary>
        ///     Creates a new instance that references the specified span.
        /// </summary>
        /// <param name="value">The span to reference as the writer buffer.</param>
        /// <returns>
        ///     A writer over <paramref name="value" /> whose total size
        ///     is the span length and whose current position is zero.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryWriter From([MustBePinned] Span<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);
    }
}