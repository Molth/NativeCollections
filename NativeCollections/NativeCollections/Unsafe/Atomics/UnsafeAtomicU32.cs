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
    ///     Provides atomic operations on a <see cref="uint" /> value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicU32
    {
        /// <summary>
        ///     Gets the underlying value.
        /// </summary>
        private UnsafeAtomicI32 _value;

        /// <summary>
        ///     Initializes a new instance of this class with the specified value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        private UnsafeAtomicU32(UnsafeAtomicI32 value) => _value = value;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>A new atomic value initialized to <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicU32 Create(uint value) => new(UnsafeAtomicI32.Create(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref uint AsRef() => ref Unsafe.As<UnsafeAtomicI32, uint>(ref _value);

        /// <summary>
        ///     Bitwise "nands" two 32-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Nand(uint value) => U32.FromI32Unchecked(_value.Nand(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Bitwise "ands" two 32-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint And(uint value) => U32.FromI32Unchecked(_value.And(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Bitwise "ors" two 32-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Or(uint value) => U32.FromI32Unchecked(_value.Or(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Bitwise "xors" two 32-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Xor(uint value) => U32.FromI32Unchecked(_value.Xor(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to add to the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint value) => U32.FromI32Unchecked(_value.Add(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to subtract from the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Sub(uint value) => U32.FromI32Unchecked(_value.Sub(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Max(uint value) => U32.FromI32Unchecked(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max));

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Min(uint value) => U32.FromI32Unchecked(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min));

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Load(Ordering order) => U32.FromI32Unchecked(_value.Load(order));

        /// <summary>
        ///     Sets a variable to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(uint value, Ordering order) => _value.Store(I32.FromU32Unchecked(value), order);

        /// <summary>
        ///     Sets a variable to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Exchange(uint value) => U32.FromI32Unchecked(_value.Exchange(I32.FromU32Unchecked(value)));

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value, as an atomic operation.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint CompareExchange(uint value, uint comparand) => U32.FromI32Unchecked(_value.CompareExchange(I32.FromU32Unchecked(value), I32.FromU32Unchecked(comparand)));

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
        public readonly override string ToString() => "UnsafeAtomicU32";
    }
}