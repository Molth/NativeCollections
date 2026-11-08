using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Manages a compact array of bit values, which are represented as
    ///     <see cref="bool" />, where <see langword="true" /> indicates that the
    ///     bit is on (1) and <see langword="false" /> indicates the bit is off (0).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeBitArray : IIsCreated, IDisposable, IEquatable<UnsafeBitArray>
    {
        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        private NativeArray<int> _buffer;

        /// <summary>
        ///     Gets the number of bits represented by this bit array.
        /// </summary>
        private int _bitLength;

        /// <summary>
        ///     Initializes an instance from its final storage state.
        /// </summary>
        /// <param name="buffer">The backing integer buffer.</param>
        /// <param name="bitLength">The number of bits represented by the buffer.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UnsafeBitArray(NativeArray<int> buffer, int bitLength)
        {
            _buffer = buffer;
            _bitLength = bitLength;
        }

        /// <summary>
        ///     Creates a bit array with the specified number of bits, initially cleared.
        /// </summary>
        /// <param name="length">The number of bits to store. Must be non-negative.</param>
        /// <returns>A cleared bit array with <paramref name="length" /> bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeBitArray Create(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var buffer = NativeArray<int>.Create(GetInt32ArrayLengthFromBitLength(length), true);
            return new UnsafeBitArray(buffer, length);
        }

        /// <summary>
        ///     Creates a bit array with the specified number of bits and initial value.
        /// </summary>
        /// <param name="length">The number of bits to store. Must be non-negative.</param>
        /// <param name="defaultValue">
        ///     The value to assign to all bits.
        /// </param>
        /// <returns>A bit array with <paramref name="length" /> bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeBitArray Create(int length, bool defaultValue)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var buffer = NativeArray<int>.Create(GetInt32ArrayLengthFromBitLength(length));
            if (defaultValue)
            {
                buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    buffer[^1] = (1 << extraBits).WrappingSub(1);
            }
            else
            {
                buffer.Clear();
            }

            return new UnsafeBitArray(buffer, length);
        }

        /// <summary>
        ///     Creates an unsafe bit array over an external integer buffer.
        /// </summary>
        /// <param name="buffer">The pinned integer buffer to wrap.</param>
        /// <param name="length">The number of bits to store.</param>
        /// <returns>An unsafe bit array that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or the buffer is too small.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeBitArray From([MustBePinned] Span<int> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            ThrowHelpers.ThrowIfLessThan(buffer.Length, intCount, ExceptionArgument.buffer);
            return new UnsafeBitArray(NativeArray<int>.From(buffer), length);
        }

        /// <summary>
        ///     Creates an unsafe bit array over an external integer buffer with an initial value.
        /// </summary>
        /// <param name="buffer">The pinned integer buffer to wrap.</param>
        /// <param name="length">The number of bits to store.</param>
        /// <param name="defaultValue">The initial value for all bits.</param>
        /// <returns>An unsafe bit array that wraps <paramref name="buffer" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative or the buffer is too small.
        /// </exception>
        /// <remarks>
        ///     The initial value is applied to the entire backing integer buffer, including storage
        ///     beyond the <paramref name="length" /> visible bits. When <paramref name="defaultValue" />
        ///     is <see langword="true" /> all such storage is set to all ones; otherwise it is cleared.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeBitArray From([MustBePinned] Span<int> buffer, int length, bool defaultValue)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            ThrowHelpers.ThrowIfLessThan(buffer.Length, intCount, ExceptionArgument.buffer);
            var value = new UnsafeBitArray(NativeArray<int>.From(buffer), length);
            if (defaultValue)
            {
                value._buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    value._buffer[intCount.WrappingSub(1)] = (1 << extraBits).WrappingSub(1);
            }
            else
            {
                value._buffer.Clear();
            }

            return value;
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer.IsCreated;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly NativeArray<int> Buffer => _buffer;

        /// <summary>
        ///     Gets or sets the number of bits in this.
        /// </summary>
        /// <value>The number of bits in this.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the property is set to a negative value.
        /// </exception>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _bitLength;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLength(value);
        }

        /// <summary>
        ///     Gets the number of bits in this.
        /// </summary>
        /// <value>The number of bits in this.</value>
        public readonly int Count => Length;

        /// <summary>
        ///     Gets or sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <value>The value of the bit at position <paramref name="index" />.</value>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_bitLength), ExceptionArgument.index);
                return (_buffer[index >> 5] & (1 << index)) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_bitLength), ExceptionArgument.index);
                var bitMask = 1 << index;
                ref var segment = ref _buffer[index >> 5];
                if (value)
                    segment |= bitMask;
                else
                    segment &= ~bitMask;
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
        public readonly bool Equals(UnsafeBitArray other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeBitArray other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeBitArray";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeBitArray left, UnsafeBitArray right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeBitArray left, UnsafeBitArray right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() => _buffer.Dispose();

        /// <summary>
        ///     Returns the bits in this as a span of bytes.
        /// </summary>
        /// <returns>A span of bytes containing the bits in this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsBytes() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer.Buffer), GetByteArrayLengthFromBitLength(_bitLength));

        /// <summary>
        ///     Sets the number of bits in this.
        /// </summary>
        /// <param name="length">The number of bits to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        /// <remarks>
        ///     Growing this instance reallocates the backing buffer and releases the previous one.
        ///     If this instance was created from a caller-supplied span via the <c>From</c> overloads,
        ///     the caller's buffer is released by this method; the caller must not access or free it
        ///     afterwards.
        /// </remarks>
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var newLength = GetInt32ArrayLengthFromBitLength(length);
            if (newLength > _buffer.Length || newLength.WrappingAdd(256) < _buffer.Length)
            {
                var oldLength = _buffer.Length;
                var buffer = NativeArray<int>.Create(newLength);
                var copyLength = Math.Min(oldLength, newLength);
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer.Buffer), ref Unsafe.AsRef<byte>(_buffer.Buffer), Usize.FromI32Unchecked(copyLength).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));
                if (newLength > oldLength)
                    SpanHelpers.Set(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer.Buffer), Isize.FromI32Unchecked(oldLength))), U8.FromI32Unchecked(0), Usize.FromI32Unchecked(newLength.WrappingSub(oldLength)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));

                _buffer.Dispose();
                _buffer = buffer;
            }

            if (length > _bitLength)
            {
                var last = _bitLength.WrappingSub(1) >> 5;
                Div32Rem(_bitLength, out var bits);
                if (bits > 0)
                    _buffer[last] &= (1 << bits).WrappingSub(1);

                _buffer.AsSpan().Slice(last.WrappingAdd(1), newLength.WrappingSub(last).WrappingSub(1)).Clear();
            }

            Div32Rem(length, out var extraBits);
            if (extraBits > 0)
                _buffer[newLength.WrappingSub(1)] &= (1 << extraBits).WrappingSub(1);

            _bitLength = length;
        }

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Get(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_bitLength), ExceptionArgument.index);
            return this[index];
        }

        /// <summary>
        ///     Sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <param name="value">The bool value to assign to the bit.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_bitLength), ExceptionArgument.index);
            this[index] = value;
        }

        /// <summary>
        ///     Sets all bits in this to the specified value.
        /// </summary>
        /// <param name="value">The bool value to assign to all bits.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void SetAll(bool value)
        {
            var length = GetInt32ArrayLengthFromBitLength(_bitLength);
            var span = _buffer.AsSpan().Slice(0, length);
            if (value)
            {
                span.Fill(-1);
                Div32Rem(_bitLength, out var extraBits);
                if (extraBits > 0)
                    span[^1] &= (1 << extraBits).WrappingSub(1);
            }
            else
            {
                span.Clear();
            }
        }

        /// <summary>
        ///     Performs the bitwise AND operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise AND operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void And(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            And(From(value.Buffer.AsSpan(), value.Length));
        }

        /// <summary>
        ///     Performs the bitwise AND operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise AND operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void And(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(_buffer.Length) || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(value._buffer.Length))
                ThrowHelpers.ThrowArrayLengthsDifferException();

            BitArrayHelpers.And(_buffer.AsSpan(), value._buffer.AsSpan(), U32.FromI32Unchecked(count));
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise OR operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Or(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            Or(From(value.Buffer.AsSpan(), value.Length));
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise OR operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Or(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(_buffer.Length) || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(value._buffer.Length))
                ThrowHelpers.ThrowArrayLengthsDifferException();

            BitArrayHelpers.Or(_buffer.AsSpan(), value._buffer.AsSpan(), U32.FromI32Unchecked(count));
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise XOR operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Xor(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            Xor(From(value.Buffer.AsSpan(), value.Length));
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise XOR operation.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Xor(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(_buffer.Length) || U32.FromI32Unchecked(count) > U32.FromI32Unchecked(value._buffer.Length))
                ThrowHelpers.ThrowArrayLengthsDifferException();

            BitArrayHelpers.Xor(_buffer.AsSpan(), value._buffer.AsSpan(), U32.FromI32Unchecked(count));
        }

        /// <summary>
        ///     Inverts all bits in this, so that bits set to <see langword="true" />
        ///     are changed to <see langword="false" />, and bits set to
        ///     <see langword="false" /> are changed to <see langword="true" />.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Not()
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            BitArrayHelpers.Not(_buffer.AsSpan(), U32.FromI32Unchecked(count));
            Div32Rem(_bitLength, out var extraBits);
            if (extraBits > 0)
                _buffer[count.WrappingSub(1)] &= (1 << extraBits).WrappingSub(1);
        }

        /// <summary>
        ///     Shifts the bits in this to the right by <paramref name="count" /> positions.
        /// </summary>
        /// <param name="count">The number of positions to shift.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RightShift(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (count == 0)
                return;

            var toIndex = 0;
            var length = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (count < _bitLength)
            {
                var fromIndex = Div32Rem(count, out var shiftCount);
                Div32Rem(_bitLength, out var extraBits);
                if (shiftCount == 0)
                {
                    var mask = uint.MaxValue >> 32.WrappingSub(extraBits);
                    _buffer[length.WrappingSub(1)] &= I32.FromU32Unchecked(mask);

                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_buffer.Buffer), ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(_buffer.Buffer), Isize.FromI32Unchecked(fromIndex))), Usize.FromI32Unchecked(length.WrappingSub(fromIndex)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));
                    toIndex = length.WrappingSub(fromIndex);
                }
                else
                {
                    var lastIndex = length.WrappingSub(1);
                    if (extraBits > 0)
                    {
                        var tailMask = uint.MaxValue >> 32.WrappingSub(extraBits);
                        _buffer[lastIndex] &= I32.FromU32Unchecked(tailMask);
                    }

                    while (fromIndex < lastIndex)
                    {
                        var right = U32.FromI32Unchecked(_buffer[fromIndex]) >> shiftCount;
                        var left = _buffer[fromIndex = fromIndex.WrappingAdd(1)] << 32.WrappingSub(shiftCount);
                        _buffer[(toIndex = toIndex.WrappingAdd(1)).WrappingSub(1)] = left | I32.FromU32Unchecked(right);
                    }

                    var mask = uint.MaxValue >> 32.WrappingSub(extraBits);
                    mask &= U32.FromI32Unchecked(_buffer[fromIndex]);
                    _buffer[(toIndex = toIndex.WrappingAdd(1)).WrappingSub(1)] = I32.FromU32Unchecked(mask >> shiftCount);
                }
            }

            _buffer.AsSpan().Slice(toIndex, length.WrappingSub(toIndex)).Clear();
        }

        /// <summary>
        ///     Shifts the bits in this to the left by <paramref name="count" /> positions.
        /// </summary>
        /// <param name="count">The number of positions to shift.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        public void LeftShift(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (count == 0)
                return;

            int lengthToClear;
            if (count < _bitLength)
            {
                var lastIndex = _bitLength.WrappingSub(1) >> 5;
                lengthToClear = Div32Rem(count, out var shiftCount);
                if (shiftCount == 0)
                {
                    SpanHelpers.Copy(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(_buffer.Buffer), Isize.FromI32Unchecked(lengthToClear))), ref Unsafe.AsRef<byte>(_buffer.Buffer), Usize.FromI32Unchecked(lastIndex.WrappingAdd(1).WrappingSub(lengthToClear)).WrappingMul(Usize.FromI32Unchecked(Unsafe.SizeOf<int>())));
                }
                else
                {
                    var fromIndex = lastIndex.WrappingSub(lengthToClear);
                    while (fromIndex > 0)
                    {
                        var left = _buffer[fromIndex] << shiftCount;
                        var right = U32.FromI32Unchecked(_buffer[fromIndex = fromIndex.WrappingSub(1)]) >> 32.WrappingSub(shiftCount);
                        _buffer[lastIndex] = left | I32.FromU32Unchecked(right);
                        lastIndex = lastIndex.WrappingSub(1);
                    }

                    _buffer[lastIndex] = _buffer[fromIndex] << shiftCount;
                }
            }
            else
            {
                lengthToClear = GetInt32ArrayLengthFromBitLength(_bitLength);
            }

            _buffer.AsSpan().Slice(0, lengthToClear).Clear();
            Div32Rem(_bitLength, out var extraBits);
            if (extraBits > 0)
                _buffer[_bitLength.WrappingSub(1) >> 5] &= (1 << extraBits).WrappingSub(1);
        }

        /// <summary>
        ///     Determines whether all bits in this are set to <see langword="true" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if every bit in this is set to <see langword="true" />,
        ///     or if this is empty; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasAllSet()
        {
            Div32Rem(_bitLength, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (extraBits != 0)
                intCount = intCount.WrappingSub(1);

            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan().Slice(0, intCount), -1))
                return false;

            if (extraBits == 0)
                return true;

            var mask = (1 << extraBits).WrappingSub(1);
            return (_buffer[intCount] & mask) == mask;
        }

        /// <summary>
        ///     Determines whether any bit in this is set to <see langword="true" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this is not empty and at least one of its bits
        ///     is set to <see langword="true" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasAnySet()
        {
            Div32Rem(_bitLength, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (extraBits != 0)
                intCount = intCount.WrappingSub(1);

            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan().Slice(0, intCount), 0))
                return true;

            return extraBits != 0 && (_buffer[intCount] & (1 << extraBits).WrappingSub(1)) != 0;
        }

        /// <summary>
        ///     Gets the storage slot for the bit at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to get.</param>
        /// <returns>The storage slot for the bit at <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the range of this.
        /// </exception>
        public readonly NativeBitArraySlot GetSlot(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(U32.FromI32Unchecked(index), U32.FromI32Unchecked(_bitLength), ExceptionArgument.index);
            return NativeBitArraySlot.From(UnsafeHelpers.Add<int>(_buffer.Buffer, Isize.FromI32Unchecked(index >> 5)), 1 << index);
        }

        /// <summary>
        ///     Gets the storage slot for the bit at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to get.</param>
        /// <param name="slot">
        ///     When this method returns, contains the storage slot for the bit at the specified
        ///     index, if the index is valid; otherwise, the default value for the type of the
        ///     <paramref name="slot" /> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="index" />
        ///     identifies a bit in this; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool TryGetSlot(int index, out NativeBitArraySlot slot)
        {
            if (U32.FromI32Unchecked(index) >= U32.FromI32Unchecked(_bitLength))
            {
                slot = default;
                return false;
            }

            slot = NativeBitArraySlot.From(UnsafeHelpers.Add<int>(_buffer.Buffer, Isize.FromI32Unchecked(index >> 5)), 1 << index);
            return true;
        }

        /// <summary>
        ///     Calculates the minimum number of bytes required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of bytes needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteArrayLengthFromBitLength(int n) => I32.FromU32Unchecked(U32.FromI32Unchecked(n).WrappingAdd(U32.FromI32Unchecked(7)) >> 3);

        /// <summary>
        ///     Calculates the minimum number of 32‑bit signed integers required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of 32‑bit signed integers needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt32ArrayLengthFromBitLength(int n) => I32.FromU32Unchecked(U32.FromI32Unchecked(n.WrappingSub(1).WrappingAdd(1 << 5)) >> 5);

        /// <summary>
        ///     Divide by 32 and get the remainder.
        /// </summary>
        /// <param name="number">Number.</param>
        /// <param name="remainder">Remainder.</param>
        /// <returns>Quotient.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Div32Rem(int number, out int remainder)
        {
            var quotient = U32.FromI32Unchecked(number) / U32.FromI32Unchecked(32);
            remainder = number & 32.WrappingSub(1);
            return I32.FromU32Unchecked(quotient);
        }
    }
}