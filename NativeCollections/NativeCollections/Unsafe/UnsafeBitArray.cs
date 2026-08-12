using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Manages a compact array of bit values, which are represented as <see cref="bool" />, where
    ///     <see langword="true" /> indicates that the bit is on (1) and <see langword="false" /> indicates
    ///     the bit is off (0).
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
        ///     Gets the total number of elements in all the dimensions of the instance.
        /// </summary>
        private int _bitLength;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public readonly bool IsCreated => _buffer.IsCreated;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public readonly NativeArray<int> Buffer => _buffer;

        /// <summary>
        ///     Gets or sets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => _bitLength;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetLength(value);
        }

        /// <summary>
        ///     Gets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        public readonly int Count => Length;

        /// <summary>
        ///     Gets or sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (_buffer[index >> 5] & (1 << index)) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var bitMask = 1 << index;
                ref var segment = ref _buffer[index >> 5];
                if (value)
                    segment |= bitMask;
                else
                    segment &= ~bitMask;
            }
        }

        /// <summary>
        ///     Gets or sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get or set.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        public bool this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => this[(int)index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[(int)index] = value;
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified number of bits,
        ///     using the natural alignment and zero-initializing the underlying storage.
        /// </summary>
        /// <param name="length">
        ///     The number of bits to store.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length), true);
            _bitLength = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class with the specified number of bits
        ///     and the initial value for all bits.
        /// </summary>
        /// <param name="length">
        ///     The number of bits to store.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to assign to all bits
        ///     (<see langword="true" /> for set, <see langword="false" /> for cleared).
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray(int length, bool defaultValue)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            _buffer = new NativeArray<int>(GetInt32ArrayLengthFromBitLength(length));
            _bitLength = length;
            if (defaultValue)
            {
                _buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    _buffer[^1] = (1 << extraBits) - 1;
            }
            else
            {
                _buffer.Clear();
            }
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps a user-provided buffer of 32‑bit integers,
        ///     with the specified number of bits.
        ///     The buffer must be large enough to hold all bits.
        /// </summary>
        /// <param name="buffer">
        ///     The buffer to use as storage.
        ///     It must be pinned in memory.
        /// </param>
        /// <param name="length">
        ///     The number of bits to store.
        ///     Must be non‑negative.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length" /> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided <paramref name="buffer" /> is smaller than required for the specified
        ///     <paramref name="length" />.
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray([MustBePinned] Span<int> buffer, int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            ThrowHelpers.ThrowIfLessThan(buffer.Length, intCount, ExceptionArgument.buffer);
            _buffer = buffer;
            _bitLength = length;
        }

        /// <summary>
        ///     Initializes a new instance of this class that wraps a user-provided buffer of 32‑bit integers,
        ///     with the specified number of bits and initial value for all bits.
        ///     The buffer must be large enough to hold all bits.
        /// </summary>
        /// <param name="buffer">
        ///     The buffer to use as storage.
        ///     It must be pinned in memory.
        /// </param>
        /// <param name="length">
        ///     The number of bits to store.
        ///     Must be non‑negative.
        /// </param>
        /// <param name="defaultValue">
        ///     The value to assign to all bits
        ///     (<see langword="true" /> for set, <see langword="false" /> for cleared).
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length" /> is negative.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the provided <paramref name="buffer" /> is smaller than required for the specified
        ///     <paramref name="length" />.
        /// </exception>
        [MustBePinned(nameof(buffer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeBitArray([MustBePinned] Span<int> buffer, int length, bool defaultValue)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var intCount = GetInt32ArrayLengthFromBitLength(length);
            ThrowHelpers.ThrowIfLessThan(buffer.Length, intCount, ExceptionArgument.buffer);
            _buffer = buffer;
            _bitLength = length;
            if (defaultValue)
            {
                _buffer.AsSpan().Fill(-1);
                Div32Rem(length, out var extraBits);
                if (extraBits > 0)
                    _buffer[^1] = (1 << extraBits) - 1;
            }
            else
            {
                _buffer.Clear();
            }
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeBitArray other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeBitArray other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeBitArray";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeBitArray left, UnsafeBitArray right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeBitArray left, UnsafeBitArray right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => _buffer.Dispose();

        /// <summary>
        ///     Casts to a ReadOnlySpan of byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Span<byte> AsBytes() => MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(_buffer.Buffer), GetByteArrayLengthFromBitLength(_bitLength));

        /// <summary>
        ///     Sets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length)
        {
            ThrowHelpers.ThrowIfNegative(length, ExceptionArgument.length);
            var newLength = GetInt32ArrayLengthFromBitLength(length);
            if (newLength > _buffer.Length || newLength + 256 < _buffer.Length)
            {
                var buffer = new NativeArray<int>(newLength);
                SpanHelpers.Copy(ref Unsafe.AsRef<byte>(buffer.Buffer), ref Unsafe.AsRef<byte>(_buffer.Buffer), (uint)(_buffer.Length * Unsafe.SizeOf<int>()));
                SpanHelpers.Set(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(buffer.Buffer), (nint)buffer.Length)), 0, (uint)(newLength - _buffer.Length));
                _buffer.Dispose();
                _buffer = buffer;
            }

            if (length > _bitLength)
            {
                var last = (_bitLength - 1) >> 5;
                Div32Rem(_bitLength, out var bits);
                if (bits > 0)
                    _buffer[last] &= (1 << bits) - 1;
                _buffer.AsSpan(last + 1, newLength - last - 1).Clear();
            }

            _bitLength = length;
        }

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is greater than or equal to
        ///     <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Get(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_bitLength, ExceptionArgument.index);
            return this[index];
        }

        /// <summary>
        ///     Sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <param name="value">The bool value to assign to the bit.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is greater than or equal to
        ///     <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_bitLength, ExceptionArgument.index);
            this[index] = value;
        }

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is greater than or equal to
        ///     <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Get(uint index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, (uint)_bitLength, ExceptionArgument.index);
            return this[index];
        }

        /// <summary>
        ///     Sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <param name="value">The bool value to assign to the bit.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is greater than or equal to
        ///     <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index, bool value)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, (uint)_bitLength, ExceptionArgument.index);
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
            var span = _buffer.AsSpan(0, length);
            if (value)
            {
                span.Fill(-1);
                Div32Rem(_bitLength, out var extraBits);
                if (extraBits > 0)
                    span[^1] &= (1 << extraBits) - 1;
            }
            else
            {
                span.Clear();
            }
        }

        /// <summary>
        ///     Performs the bitwise AND operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise AND operation.</param>
        /// <returns>An array containing the result of the bitwise AND operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void And(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            And(new UnsafeBitArray(value.Buffer, value.Length));
        }

        /// <summary>
        ///     Performs the bitwise AND operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise AND operation.</param>
        /// <returns>An array containing the result of the bitwise AND operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void And(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value._buffer.Length)
                ThrowHelpers.ThrowArrayLengthsDifferException();
            BitArrayHelpers.And(_buffer, value._buffer, (uint)count);
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise OR operation.</param>
        /// <returns>An array containing the result of the bitwise OR operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Or(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            Or(new UnsafeBitArray(value.Buffer, value.Length));
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise OR operation.</param>
        /// <returns>An array containing the result of the bitwise OR operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Or(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value._buffer.Length)
                ThrowHelpers.ThrowArrayLengthsDifferException();
            BitArrayHelpers.Or(_buffer, value._buffer, (uint)count);
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise XOR operation.</param>
        /// <returns>An array containing the result of the bitwise XOR operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Xor(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            Xor(new UnsafeBitArray(value.Buffer, value.Length));
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the elements of the current object and the
        ///     corresponding elements in the specified array. The current object will be modified to
        ///     store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The array with which to perform the bitwise XOR operation.</param>
        /// <returns>An array containing the result of the bitwise XOR operation, which is a reference to the current object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="value" /> and the current do not have the same number of elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Xor(UnsafeBitArray value)
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (_bitLength != value._bitLength || (uint)count > (uint)_buffer.Length || (uint)count > (uint)value._buffer.Length)
                ThrowHelpers.ThrowArrayLengthsDifferException();
            BitArrayHelpers.Xor(_buffer, value._buffer, (uint)count);
        }

        /// <summary>
        ///     Inverts all the bit values in the current, so that elements set to true are changed to false,
        ///     and elements set to false are changed to true.
        /// </summary>
        /// <returns>The current instance with inverted bit values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Not()
        {
            var count = GetInt32ArrayLengthFromBitLength(_bitLength);
            BitArrayHelpers.Not(_buffer, (uint)count);
        }

        /// <summary>
        ///     Shifts all the bit values of the current to the right on <paramref name="count" /> bits.
        /// </summary>
        /// <param name="count">The number of shifts to make for each bit.</param>
        /// <returns>The current.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than zero.</exception>
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
                    unchecked
                    {
                        var mask = uint.MaxValue >> (32 - extraBits);
                        _buffer[length - 1] &= (int)mask;
                    }

                    SpanHelpers.Copy(ref Unsafe.AsRef<byte>(_buffer.Buffer), ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(_buffer.Buffer), (nint)fromIndex)), (uint)((length - fromIndex) * Unsafe.SizeOf<int>()));
                    toIndex = length - fromIndex;
                }
                else
                {
                    var lastIndex = length - 1;
                    unchecked
                    {
                        while (fromIndex < lastIndex)
                        {
                            var right = (uint)_buffer[fromIndex] >> shiftCount;
                            var left = _buffer[++fromIndex] << (32 - shiftCount);
                            _buffer[toIndex++] = left | (int)right;
                        }

                        var mask = uint.MaxValue >> (32 - extraBits);
                        mask &= (uint)_buffer[fromIndex];
                        _buffer[toIndex++] = (int)(mask >> shiftCount);
                    }
                }
            }

            _buffer.AsSpan(toIndex, length - toIndex).Clear();
        }

        /// <summary>
        ///     Shifts all the bit values of the current to the left on <paramref name="count" /> bits.
        /// </summary>
        /// <param name="count">The number of shifts to make for each bit.</param>
        /// <returns>The current.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LeftShift(int count)
        {
            ThrowHelpers.ThrowIfNegative(count, ExceptionArgument.count);
            if (count == 0)
                return;
            int lengthToClear;
            if (count < _bitLength)
            {
                var lastIndex = (_bitLength - 1) >> 5;
                lengthToClear = Div32Rem(count, out var shiftCount);
                if (shiftCount == 0)
                {
                    SpanHelpers.Copy(ref Unsafe.As<int, byte>(ref Unsafe.Add(ref Unsafe.AsRef<int>(_buffer.Buffer), (nint)lengthToClear)), ref Unsafe.AsRef<byte>(_buffer.Buffer), (uint)((lastIndex + 1 - lengthToClear) * Unsafe.SizeOf<int>()));
                }
                else
                {
                    var fromIndex = lastIndex - lengthToClear;
                    unchecked
                    {
                        while (fromIndex > 0)
                        {
                            var left = _buffer[fromIndex] << shiftCount;
                            var right = (uint)_buffer[--fromIndex] >> (32 - shiftCount);
                            _buffer[lastIndex] = left | (int)right;
                            lastIndex--;
                        }

                        _buffer[lastIndex] = _buffer[fromIndex] << shiftCount;
                    }
                }
            }
            else
            {
                lengthToClear = GetInt32ArrayLengthFromBitLength(_bitLength);
            }

            _buffer.AsSpan(0, lengthToClear).Clear();
        }

        /// <summary>
        ///     Determines whether all bits in this are set to <c>true</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if every bit in this is set to <c>true</c>,
        ///     or if this is empty; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasAllSet()
        {
            Div32Rem(_bitLength, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (extraBits != 0)
                intCount--;
            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan(0, intCount), -1))
                return false;
            if (extraBits == 0)
                return true;
            var mask = (1 << extraBits) - 1;
            return (_buffer[intCount] & mask) == mask;
        }

        /// <summary>
        ///     Determines whether any bit in this is set to <c>true</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this is not empty and at least one of its bit is set to <c>true</c>;
        ///     otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool HasAnySet()
        {
            Div32Rem(_bitLength, out var extraBits);
            var intCount = GetInt32ArrayLengthFromBitLength(_bitLength);
            if (extraBits != 0)
                intCount--;
            if (SpanHelpers.ContainsAnyExcept(_buffer.AsReadOnlySpan(0, intCount), 0))
                return true;
            return extraBits != 0 && (_buffer[intCount] & ((1 << extraBits) - 1)) != 0;
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        public readonly NativeBitArraySlot GetSlot(int index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual((uint)index, (uint)_bitLength, ExceptionArgument.index);
            return new NativeBitArraySlot(UnsafeHelpers.Add<int>(_buffer.Buffer, index >> 5), 1 << index);
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="slot">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="slot" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool TryGetSlot(int index, out NativeBitArraySlot slot)
        {
            if ((uint)index >= (uint)_bitLength)
            {
                slot = default;
                return false;
            }

            slot = new NativeBitArraySlot(UnsafeHelpers.Add<int>(_buffer.Buffer, index >> 5), 1 << index);
            return true;
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        public readonly NativeBitArraySlot GetSlot(uint index)
        {
            ThrowHelpers.ThrowIfGreaterThanOrEqual(index, (uint)_bitLength, ExceptionArgument.index);
            return new NativeBitArraySlot(UnsafeHelpers.Add<int>(_buffer.Buffer, (nint)index >> 5), 1 << (int)index);
        }

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <param name="slot">
        ///     When this method returns, contains the value associated with the specified key, if the key is
        ///     found; otherwise, the default value for the type of the <paramref name="slot" /> parameter.
        ///     This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if this contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool TryGetSlot(uint index, out NativeBitArraySlot slot)
        {
            if (index >= (uint)_bitLength)
            {
                slot = default;
                return false;
            }

            slot = new NativeBitArraySlot(UnsafeHelpers.Add<int>(_buffer.Buffer, (nint)(index >> 5)), 1 << (int)index);
            return true;
        }

        /// <summary>
        ///     Calculates the minimum number of bytes required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of bytes needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteArrayLengthFromBitLength(int n) => (int)(((uint)n + 7) >> 3);

        /// <summary>
        ///     Calculates the minimum number of 32‑bit signed integers required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of 32‑bit signed integers needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt32ArrayLengthFromBitLength(int n) => (int)((uint)(n - 1 + (1 << 5)) >> 5);

        /// <summary>
        ///     Divide by 32 and get remainder
        /// </summary>
        /// <param name="number">Number</param>
        /// <param name="remainder">Remainder</param>
        /// <returns>Quotient</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Div32Rem(int number, out int remainder)
        {
            var quotient = (uint)number / 32;
            remainder = number & (32 - 1);
            return (int)quotient;
        }

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeBitArray Empty => default;
    }
}