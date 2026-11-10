using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents a sequential reader over a native memory buffer with position tracking.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.None)]
    public unsafe struct UnsafeMemoryReader : IIsCreated, IDisposable, IEquatable<UnsafeMemoryReader>
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
        /// <param name="buffer">
        ///     A pointer to the start of the region to read from. The memory must remain valid
        ///     while this reader is in use.
        /// </param>
        /// <param name="length">
        ///     The size of the buffer in bytes. Must be non-negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeMemoryReader(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            Buffer = buffer;
            Length = length;
            _position = 0;
        }

        /// <summary>
        ///     Creates a reader view over the specified native buffer.
        /// </summary>
        /// <param name="buffer">
        ///     A pointer to the start of the region to read from. The memory must remain valid
        ///     while the returned reader is in use.
        /// </param>
        /// <param name="length">The size of the buffer in bytes. Must be non-negative.</param>
        /// <returns>
        ///     A reader over <paramref name="buffer" /> whose total size is
        ///     <paramref name="length" /> bytes and whose current position is zero.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <remarks>
        ///     The returned reader owns <paramref name="buffer" />; <see cref="Dispose" /> releases it with
        ///     <see cref="NativeMemoryAllocator.AlignedFree" />. Therefore, <paramref name="buffer" /> must
        ///     be allocated by <see cref="NativeMemoryAllocator" /> or a compatible allocator, and the
        ///     caller must not release it separately while the returned reader is in use.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryReader From(byte* buffer, int length) => new(buffer, length);

        /// <summary>
        ///     Creates a new instance that references the specified span.
        /// </summary>
        /// <param name="value">The span to reference as the reader buffer.</param>
        /// <returns>
        ///     A reader over <paramref name="value" /> whose total size
        ///     is the span length and whose current position is zero.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemoryReader From([MustBePinned] Span<byte> value) => new(UnsafeHelpers.AsPointer(ref MemoryMarshal.GetReference(value)), value.Length);

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
        public readonly bool Equals(UnsafeMemoryReader other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeMemoryReader other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeMemoryReader";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeMemoryReader left, UnsafeMemoryReader right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeMemoryReader left, UnsafeMemoryReader right) => !left.Equals(right);

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
        ///     Reads a value of type <typeparamref name="T" /> from the current position and advances
        ///     the position by the size of the value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to read.</typeparam>
        /// <returns>
        ///     The value read at the current position, after which the position is advanced by the size of the value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining bytes are insufficient to read a value of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())), I64.FromI32Unchecked(Length), ExceptionArgument._dummy);
            var obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, Isize.FromI32Unchecked(_position)));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
            return obj;
        }

        /// <summary>
        ///     Reads a value of type <typeparamref name="T" /> into the specified destination and
        ///     advances the position by the size of the value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to read.</typeparam>
        /// <param name="obj">The destination to which to copy the value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining bytes are insufficient to read a value of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())), I64.FromI32Unchecked(Length), ExceptionArgument.obj);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(obj), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
        }

        /// <summary>
        ///     Attempts to read a value of type <typeparamref name="T" /> into the specified destination.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to read.</typeparam>
        /// <param name="obj">The destination to which to copy the value.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was read; otherwise,
        ///     <see langword="false" /> if the remaining bytes are insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj) where T : unmanaged
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(obj), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(Unsafe.SizeOf<T>()));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
            return true;
        }

        /// <summary>
        ///     Reads <paramref name="count" /> values of type <typeparamref name="T" /> into the
        ///     specified destination and advances the position by their total size.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to read.</typeparam>
        /// <param name="obj">The destination to which to copy the values.</param>
        /// <param name="count">The number of values to read.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="count" /> is negative or the remaining bytes are
        ///     insufficient to read <paramref name="count" /> values of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            ReadSpan(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Attempts to read <paramref name="count" /> values of type <typeparamref name="T" />
        ///     into the specified destination.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the values to read.</typeparam>
        /// <param name="obj">The destination to which to copy the values.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>
        ///     <see langword="true" /> if all values were read; otherwise,
        ///     <see langword="false" /> if the remaining bytes are insufficient.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(T* obj, int count) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            return TryReadSpan(MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>(obj), count));
        }

        /// <summary>
        ///     Reads values of type <typeparamref name="T" /> into the specified span and advances
        ///     the position by their total size.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the destination buffer.</typeparam>
        /// <param name="buffer">The destination buffer to fill with values.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining bytes are insufficient to fill <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSpan<T>(Span<T> buffer) where T : unmanaged
        {
            var count = I64.FromI32Unchecked(buffer.Length).WrappingMul(I64.FromI32Unchecked(Unsafe.SizeOf<T>()));
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(count), I64.FromI32Unchecked(Length), ExceptionArgument.buffer);
            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI64Unchecked(count));
            _position = _position.WrappingAdd(I32.FromI64Unchecked(count));
        }

        /// <summary>
        ///     Attempts to read values of type <typeparamref name="T" /> into the specified span.
        /// </summary>
        /// <typeparam name="T">The unmanaged element type of the destination buffer.</typeparam>
        /// <param name="buffer">The destination buffer to fill with values.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="buffer" /> was filled; otherwise,
        ///     <see langword="false" /> if the remaining bytes are insufficient to fill it.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadSpan<T>(Span<T> buffer) where T : unmanaged
        {
            var count = I64.FromI32Unchecked(buffer.Length).WrappingMul(I64.FromI32Unchecked(Unsafe.SizeOf<T>()));
            if (I64.FromI32Unchecked(_position).WrappingAdd(count) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer)), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI64Unchecked(count));
            _position = _position.WrappingAdd(I32.FromI64Unchecked(count));
            return true;
        }

        /// <summary>
        ///     Reads a value of type <typeparamref name="T" /> into the specified reference and
        ///     advances the position by the size of the value.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to read.</typeparam>
        /// <param name="obj">The destination reference to which to copy the value.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining bytes are insufficient to read a value of type <typeparamref name="T" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T obj) where T : unmanaged
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())), I64.FromI32Unchecked(Length), ExceptionArgument.obj);
            obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, Isize.FromI32Unchecked(_position)));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
        }

        /// <summary>
        ///     Attempts to read a value of type <typeparamref name="T" /> into the specified reference.
        /// </summary>
        /// <typeparam name="T">The unmanaged type of the value to read.</typeparam>
        /// <param name="obj">The destination reference to which to copy the value.</param>
        /// <returns>
        ///     <see langword="true" /> if the value was read; otherwise,
        ///     <see langword="false" /> if the remaining bytes are insufficient.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(ref T obj) where T : unmanaged
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(Unsafe.SizeOf<T>())) > I64.FromI32Unchecked(Length))
                return false;

            obj = Unsafe.ReadUnaligned<T>(UnsafeHelpers.AddByteOffset(Buffer, Isize.FromI32Unchecked(_position)));
            _position = _position.WrappingAdd(Unsafe.SizeOf<T>());
            return true;
        }

        /// <summary>
        ///     Reads the specified number of bytes into the destination and advances the position
        ///     by that number.
        /// </summary>
        /// <param name="buffer">The destination buffer to fill.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or the remaining
        ///     bytes are insufficient to read <paramref name="length" /> bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(length)), I64.FromI32Unchecked(Length), ExceptionArgument.length);
            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(length));
            _position = _position.WrappingAdd(length);
        }

        /// <summary>
        ///     Attempts to read the specified number of bytes into the destination.
        /// </summary>
        /// <param name="buffer">The destination buffer to fill.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>
        ///     <see langword="true" /> if the requested bytes were
        ///     read successfully; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(byte* buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(length)) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(length));
            _position = _position.WrappingAdd(length);
            return true;
        }

        /// <summary>
        ///     Reads bytes into the specified span and advances the position by the span length.
        /// </summary>
        /// <param name="buffer">The destination buffer to fill.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the remaining bytes are insufficient to fill <paramref name="buffer" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(Span<byte> buffer)
        {
            ThrowHelpers.ThrowIfGreaterThan(I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(buffer.Length)), I64.FromI32Unchecked(Length), ExceptionArgument.buffer);
            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(buffer.Length));
            _position = _position.WrappingAdd(buffer.Length);
        }

        /// <summary>
        ///     Attempts to read bytes into the specified span.
        /// </summary>
        /// <param name="buffer">The destination buffer to fill.</param>
        /// <returns>
        ///     <see langword="true" /> if the requested bytes were
        ///     read successfully; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBytes(Span<byte> buffer)
        {
            if (I64.FromI32Unchecked(_position).WrappingAdd(I64.FromI32Unchecked(buffer.Length)) > I64.FromI32Unchecked(Length))
                return false;

            SpanHelpers.Copy(ref MemoryMarshal.GetReference(buffer), ref Unsafe.AddByteOffset(ref Unsafe.AsRef<byte>(Buffer), new IntPtr(_position)), Usize.FromI32Unchecked(buffer.Length));
            _position = _position.WrappingAdd(buffer.Length);
            return true;
        }

        /// <summary>
        ///     Returns a span over the entire buffer of this reader.
        /// </summary>
        /// <returns>
        ///     A span over the whole buffer, <see cref="Length" /> bytes long, regardless of the
        ///     current position.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(Buffer), Length);

        /// <summary>
        ///     Returns a read-only span over the entire buffer of this reader.
        /// </summary>
        /// <returns>
        ///     A read-only span over the whole buffer, <see cref="Length" /> bytes long, regardless
        ///     of the current position.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(Buffer), Length);
    }
}