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
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeBitArray))]
    public readonly unsafe struct NativeBitArray : IIsCreated, IDisposable, IEquatable<NativeBitArray>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeBitArray* _handle;

        /// <summary>
        ///     Initializes an instance from its final handle.
        /// </summary>
        /// <param name="handle">The handle to the underlying object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeBitArray(UnsafeBitArray* handle) => _handle = handle;

        /// <summary>
        ///     Creates a bit array with the specified number of bits, initially cleared.
        /// </summary>
        /// <param name="length">The number of bits to store. Must be non-negative.</param>
        /// <returns>A native bit array with <paramref name="length" /> bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBitArray Create(int length)
        {
            var value = UnsafeBitArray.Create(length);
            return new NativeBitArray(Box.New(ref value));
        }

        /// <summary>
        ///     Creates a bit array with the specified number of bits and initial value.
        /// </summary>
        /// <param name="length">The number of bits to store. Must be non-negative.</param>
        /// <param name="defaultValue">The value to assign to all bits.</param>
        /// <returns>A native bit array with <paramref name="length" /> bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeBitArray Create(int length, bool defaultValue)
        {
            var value = UnsafeBitArray.Create(length, defaultValue);
            return new NativeBitArray(Box.New(ref value));
        }

        /// <summary>
        ///     Calculates the minimum number of bytes required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of bytes needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteArrayLengthFromBitLength(int n) => UnsafeBitArray.GetByteArrayLengthFromBitLength(n);

        /// <summary>
        ///     Calculates the minimum number of 32‑bit signed integers required to store the specified number of bits.
        /// </summary>
        /// <param name="n">The number of bits.</param>
        /// <returns>The number of 32‑bit signed integers needed to store <paramref name="n" /> bits.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetInt32ArrayLengthFromBitLength(int n) => UnsafeBitArray.GetInt32ArrayLengthFromBitLength(n);

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public NativeArray<int> Buffer => _handle->Buffer;

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
            get => _handle->Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Length = value;
        }

        /// <summary>
        ///     Gets the number of bits in this.
        /// </summary>
        /// <value>The number of bits in this.</value>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
        }

        /// <summary>
        ///     Gets or sets the bit at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Length" />.
        /// </exception>
        public bool this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Unsafe.AsRef<UnsafeBitArray>(_handle)[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Unsafe.AsRef<UnsafeBitArray>(_handle)[index] = value;
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(NativeBitArray other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => obj is NativeBitArray other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => "NativeBitArray";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(NativeBitArray left, NativeBitArray right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(NativeBitArray left, NativeBitArray right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Returns the bits in this as a span of bytes.
        /// </summary>
        /// <returns>A span of bytes containing the bits in this.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsBytes() => _handle->AsBytes();

        /// <summary>
        ///     Sets the number of bits in this.
        /// </summary>
        /// <param name="length">The number of bits to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="length" /> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length) => _handle->SetLength(length);

        /// <summary>
        ///     Gets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The value of the bit at position <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index) => _handle->Get(index);

        /// <summary>
        ///     Sets the value of the bit at a specific position in this.
        /// </summary>
        /// <param name="index">The zero-based index of the value to set.</param>
        /// <param name="value">The bool value to assign to the bit.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is negative or greater than or equal to <see cref="Count" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Sets all bits in this to the specified value.
        /// </summary>
        /// <param name="value">The bool value to assign to all bits.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value) => _handle->SetAll(value);

        /// <summary>
        ///     Performs the bitwise AND operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise AND operation.</param>
        /// <returns>The current instance after applying the bitwise AND operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> indicates it is not created.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray And(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->And(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
        }

        /// <summary>
        ///     Performs the bitwise AND operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise AND operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise AND operation.</param>
        /// <returns>The current instance after applying the bitwise AND operation.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray And(UnsafeBitArray value)
        {
            _handle->And(value);
            return this;
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise OR operation.</param>
        /// <returns>The current instance after applying the bitwise OR operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> indicates it is not created.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Or(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->Or(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
        }

        /// <summary>
        ///     Performs the bitwise OR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise OR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise OR operation.</param>
        /// <returns>The current instance after applying the bitwise OR operation.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Or(UnsafeBitArray value)
        {
            _handle->Or(value);
            return this;
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise XOR operation.</param>
        /// <returns>The current instance after applying the bitwise XOR operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="value" /> indicates it is not created.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Xor(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->Xor(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
        }

        /// <summary>
        ///     Performs the bitwise XOR operation between the bits in this and the corresponding bits in the
        ///     specified bit array. The current instance is modified to store the result of the bitwise XOR operation.
        /// </summary>
        /// <param name="value">The bit array with which to perform the bitwise XOR operation.</param>
        /// <returns>The current instance after applying the bitwise XOR operation.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="value" /> and the current instance do not have the same number of bits.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Xor(UnsafeBitArray value)
        {
            _handle->Xor(value);
            return this;
        }

        /// <summary>
        ///     Inverts all bits in this, so that bits set to <see langword="true" />
        ///     are changed to <see langword="false" />, and bits set to
        ///     <see langword="false" /> are changed to <see langword="true" />.
        /// </summary>
        /// <returns>The current instance with inverted bit values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Not()
        {
            _handle->Not();
            return this;
        }

        /// <summary>
        ///     Shifts the bits in this to the right by <paramref name="count" /> positions.
        /// </summary>
        /// <param name="count">The number of positions to shift.</param>
        /// <returns>The current instance after shifting the bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray RightShift(int count)
        {
            _handle->RightShift(count);
            return this;
        }

        /// <summary>
        ///     Shifts the bits in this to the left by <paramref name="count" /> positions.
        /// </summary>
        /// <param name="count">The number of positions to shift.</param>
        /// <returns>The current instance after shifting the bits.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray LeftShift(int count)
        {
            _handle->LeftShift(count);
            return this;
        }

        /// <summary>
        ///     Determines whether all bits in this are set to <see langword="true" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if every bit in this is set to <see langword="true" />,
        ///     or if this is empty; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllSet() => _handle->HasAllSet();

        /// <summary>
        ///     Determines whether any bit in this is set to <see langword="true" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this is not empty and at least one of its bits
        ///     is set to <see langword="true" />; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnySet() => _handle->HasAnySet();

        /// <summary>
        ///     Gets the storage slot for the bit at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to get.</param>
        /// <returns>The storage slot for the bit at <paramref name="index" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="index" /> is outside the range of this.
        /// </exception>
        public NativeBitArraySlot GetSlot(int index) => _handle->GetSlot(index);

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
        public bool TryGetSlot(int index, out NativeBitArraySlot slot) => _handle->TryGetSlot(index, out slot);
    }
}