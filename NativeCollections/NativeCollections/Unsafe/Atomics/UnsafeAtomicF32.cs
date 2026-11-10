using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals.
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member.
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference.

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides atomic operations on a <see cref="float" /> value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicF32
    {
        /// <summary>
        ///     Gets the underlying value.
        /// </summary>
        private UnsafeAtomicI32 _value;

        /// <summary>
        ///     Initializes a new instance of this class with the specified value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        private UnsafeAtomicF32(UnsafeAtomicI32 value) => _value = value;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>A new atomic value initialized to <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicF32 Create(float value) => new(UnsafeAtomicI32.Create(BitConverter.SingleToInt32Bits(value)));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref float AsRef() => ref Unsafe.As<UnsafeAtomicI32, float>(ref _value);

        /// <summary>
        ///     Adds two values and replaces the first value with the sum, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to add to the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Add(float value) => InterlockedHelpers.AddFloat32(ref _value.AsRef(), value);

        /// <summary>
        ///     Subtracts two values and replaces the first value with the difference, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to subtract from the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sub(float value) => Add(-value);

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        /// <remarks>
        ///     If either operand is <see cref="float.NaN" />, the stored value becomes
        ///     <see cref="float.NaN" />, matching <see cref="Math.Max(float, float)" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(float value)
        {
            var newInt32 = AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        /// <remarks>
        ///     If either operand is <see cref="float.NaN" />, the stored value becomes
        ///     <see cref="float.NaN" />, matching <see cref="Math.Min(float, float)" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(float value)
        {
            var newInt32 = AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Load(Ordering order)
        {
            var newInt32 = _value.Load(order);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Sets a variable to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(float value, Ordering order) => _value.Store(BitConverter.SingleToInt32Bits(value), order);

        /// <summary>
        ///     Sets a variable to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Exchange(float value)
        {
            var newInt32 = _value.Exchange(BitConverter.SingleToInt32Bits(value));
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value, as an atomic operation.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CompareExchange(float value, float comparand)
        {
            var newInt32 = _value.CompareExchange(BitConverter.SingleToInt32Bits(value), BitConverter.SingleToInt32Bits(comparand));
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeAtomicF32";
    }
}