using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides atomic operations on a native-sized unsigned integer (<see cref="nuint" />).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicUsize
    {
        /// <summary>
        ///     Value
        /// </summary>
        private nuint _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicUsize(nuint value) => _value = value;

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref nuint AsRef() => ref _value;

        /// <summary>
        ///     Bitwise "nands" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Nand(nuint value) => InterlockedHelpers.Nand(ref _value, value);

        /// <summary>
        ///     Bitwise "ands" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint And(nuint value) => InterlockedHelpers.And(ref _value, value);

        /// <summary>
        ///     Bitwise "ors" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Or(nuint value) => InterlockedHelpers.Or(ref _value, value);

        /// <summary>
        ///     Bitwise "xors" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Xor(nuint value) => InterlockedHelpers.Xor(ref _value, value);

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Add(nuint value) => InterlockedHelpers.Add(ref _value, value);

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Sub(nuint value) => Add(unchecked((nuint)(-(nint)value)));

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Max(nuint value) => AtomicHelpers.Update(ref _value, value, &MathHelpers.Max);

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Min(nuint value) => AtomicHelpers.Update(ref _value, value, &MathHelpers.Min);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Load(Ordering order) => AtomicHelpers.Load(ref _value, order);

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(nuint value, Ordering order) => AtomicHelpers.Store(ref _value, value, order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint Exchange(nuint value) => InterlockedHelpers.Exchange(ref _value, value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public nuint CompareExchange(nuint value, nuint comparand) => InterlockedHelpers.CompareExchange(ref _value, value, comparand);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        [Obsolete(SR.parameter_obsolete)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeAtomicUsize";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicUsize Empty => default;
    }
}