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
    ///     Provides atomic operations on a native-sized signed integer (<see cref="IntPtr" />).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicIsize
    {
        /// <summary>
        ///     Gets the underlying value.
        /// </summary>
        private nint _value;

        /// <summary>
        ///     Initializes a new instance of this class with the specified value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        private UnsafeAtomicIsize(nint value) => _value = value;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <returns>A new atomic value initialized to <paramref name="value" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeAtomicIsize Create(nint value) => new(value);

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        /// <returns>A reference to the atomic value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref nint AsRef() => ref _value;

        /// <summary>
        ///     Bitwise "nands" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Nand(nint value) => InterlockedHelpers.Nand(ref _value, value);

        /// <summary>
        ///     Bitwise "ands" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint And(nint value) => InterlockedHelpers.And(ref _value, value);

        /// <summary>
        ///     Bitwise "ors" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Or(nint value) => InterlockedHelpers.Or(ref _value, value);

        /// <summary>
        ///     Bitwise "xors" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to combine with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Xor(nint value) => InterlockedHelpers.Xor(ref _value, value);

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to add to the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Add(nint value) => InterlockedHelpers.Add(ref _value, value);

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to subtract from the current value.</param>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Sub(nint value) => Add(value.WrappingNeg());

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Max(nint value) => AtomicHelpers.Update(ref _value, value, &MathHelpers.Max);

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <param name="value">The value to compare with the current value.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Min(nint value) => AtomicHelpers.Update(ref _value, value, &MathHelpers.Min);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <param name="order">The memory ordering constraint applied to the load operation.</param>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Load(Ordering order) => AtomicHelpers.Load(ref _value, order);

        /// <summary>
        ///     Sets a variable to a specified value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="order">The memory ordering constraint applied to the store operation.</param>
        /// <exception cref="NotSupportedException">Thrown when <paramref name="order" /> is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(nint value, Ordering order) => AtomicHelpers.Store(ref _value, value, order);

        /// <summary>
        ///     Sets a variable to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint Exchange(nint value) => Interlocked.Exchange(ref _value, value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value, as an atomic operation.
        /// </summary>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at the destination.</param>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nint CompareExchange(nint value, nint comparand) => Interlocked.CompareExchange(ref _value, value, comparand);

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
        public readonly override string ToString() => "UnsafeAtomicIsize";
    }
}