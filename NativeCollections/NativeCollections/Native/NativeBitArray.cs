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
    [NativeCollection(FromType.Standard)]
    [BindingType(typeof(UnsafeBitArray))]
    public readonly unsafe struct NativeBitArray : IIsCreated, IDisposable, IEquatable<NativeBitArray>
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly UnsafeBitArray* _handle;

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
        public NativeBitArray(int length)
        {
            var value = new UnsafeBitArray(length);
            _handle = Box.New(ref value);
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
        public NativeBitArray(int length, bool defaultValue)
        {
            var value = new UnsafeBitArray(length, defaultValue);
            _handle = Box.New(ref value);
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
        public NativeBitArray([MustBePinned] Span<int> buffer, int length)
        {
            var value = new UnsafeBitArray(buffer, length);
            _handle = Box.New(ref value);
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
        public NativeBitArray([MustBePinned] Span<int> buffer, int length, bool defaultValue)
        {
            var value = new UnsafeBitArray(buffer, length, defaultValue);
            _handle = Box.New(ref value);
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => !UnsafeHelpers.IsNull(_handle);

        /// <summary>
        ///     Represents a contiguous region of arbitrary memory.
        /// </summary>
        public NativeArray<int> Buffer => _handle->Buffer;

        /// <summary>
        ///     Gets or sets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _handle->Length = value;
        }

        /// <summary>
        ///     Gets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _handle->Length;
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to a value.
        /// </summary>
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
        public bool Equals(NativeBitArray other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is NativeBitArray other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public override string ToString() => "NativeBitArray";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(NativeBitArray left, NativeBitArray right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(NativeBitArray left, NativeBitArray right) => !left.Equals(right);

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Box.Drop(_handle);

        /// <summary>
        ///     Casts to a ReadOnlySpan of byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsBytes() => _handle->AsBytes();

        /// <summary>
        ///     Sets the number of elements in this.
        /// </summary>
        /// <value>The number of elements in this.</value>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLength(int length) => _handle->SetLength(length);

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
        public bool Get(int index) => _handle->Get(index);

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
        public void Set(int index, bool value) => _handle->Set(index, value);

        /// <summary>
        ///     Sets all bits in this to the specified value.
        /// </summary>
        /// <param name="value">The bool value to assign to all bits.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAll(bool value) => _handle->SetAll(value);

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
        public NativeBitArray And(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->And(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
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
        public NativeBitArray And(UnsafeBitArray value)
        {
            _handle->And(value);
            return this;
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
        public NativeBitArray Or(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->Or(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
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
        public NativeBitArray Or(UnsafeBitArray value)
        {
            _handle->Or(value);
            return this;
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
        public NativeBitArray Xor(NativeBitArray value)
        {
            ThrowHelpers.ThrowIfNotCreated(ref value, ExceptionArgument.value);
            _handle->Xor(Unsafe.AsRef<UnsafeBitArray>(value._handle));
            return this;
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
        public NativeBitArray Xor(UnsafeBitArray value)
        {
            _handle->Xor(value);
            return this;
        }

        /// <summary>
        ///     Inverts all the bit values in the current, so that elements set to true are changed to false,
        ///     and elements set to false are changed to true.
        /// </summary>
        /// <returns>The current instance with inverted bit values.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray Not()
        {
            _handle->Not();
            return this;
        }

        /// <summary>
        ///     Shifts all the bit values of the current to the right on <paramref name="count" /> bits.
        /// </summary>
        /// <param name="count">The number of shifts to make for each bit.</param>
        /// <returns>The current.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray RightShift(int count)
        {
            _handle->RightShift(count);
            return this;
        }

        /// <summary>
        ///     Shifts all the bit values of the current to the left on <paramref name="count" /> bits.
        /// </summary>
        /// <param name="count">The number of shifts to make for each bit.</param>
        /// <returns>The current.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than zero.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeBitArray LeftShift(int count)
        {
            _handle->LeftShift(count);
            return this;
        }

        /// <summary>
        ///     Determines whether all bits in this are set to <c>true</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if every bit in this is set to <c>true</c>,
        ///     or if this is empty; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllSet() => _handle->HasAllSet();

        /// <summary>
        ///     Determines whether any bit in this is set to <c>true</c>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this is not empty and at least one of its bit is set to <c>true</c>;
        ///     otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnySet() => _handle->HasAnySet();

        /// <summary>
        ///     Gets the value associated with the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <exception cref="ArgumentOutOfRangeException">The property is set to a value that is less than zero.</exception>
        public NativeBitArraySlot GetSlot(int index) => _handle->GetSlot(index);

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
        public bool TryGetSlot(int index, out NativeBitArraySlot slot) => _handle->TryGetSlot(index, out slot);

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static NativeBitArray Empty => default;
    }
}