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
    ///     Provides atomic operations on an <see cref="sbyte" /> value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicI8
    {
        /// <summary>
        ///     Gets the underlying value.
        /// </summary>
        private UnsafeAtomicU8 _value;

        /// <summary>
        ///     Initializes a new instance of this class with the specified value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        private UnsafeAtomicI8(UnsafeAtomicU8 value) => _value = value;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>A new atomic value initialized to <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicI8 Create(sbyte value) => new(UnsafeAtomicU8.Create(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref sbyte AsRef() => ref Unsafe.As<UnsafeAtomicU8, sbyte>(ref _value);

        /// <summary>
        ///     Bitwise "nands" two 8-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Nand(sbyte value) => I8.FromU8Unchecked(_value.Nand(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Bitwise "ands" two 8-bit signed integers and replaces the
        ///     first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte And(sbyte value) => I8.FromU8Unchecked(_value.And(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Bitwise "ors" two 8-bit signed integers and replaces the
        ///     first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Or(sbyte value) => I8.FromU8Unchecked(_value.Or(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Bitwise "xors" two 8-bit signed integers and replaces the
        ///     first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Xor(sbyte value) => I8.FromU8Unchecked(_value.Xor(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to add to the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Add(sbyte value) => I8.FromU8Unchecked(_value.Add(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to subtract from the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Sub(sbyte value) => I8.FromU8Unchecked(_value.Sub(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Max(sbyte value) => I8.FromU8Unchecked(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max));

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Min(sbyte value) => I8.FromU8Unchecked(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min));

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Load(Ordering order) => I8.FromU8Unchecked(_value.Load(order));

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(sbyte value, Ordering order) => _value.Store(U8.FromI8Unchecked(value), order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte Exchange(sbyte value) => I8.FromU8Unchecked(_value.Exchange(U8.FromI8Unchecked(value)));

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte CompareExchange(sbyte value, sbyte comparand) => I8.FromU8Unchecked(_value.CompareExchange(U8.FromI8Unchecked(value), U8.FromI8Unchecked(comparand)));

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
        public readonly override string ToString() => "UnsafeAtomicI8";
    }
}