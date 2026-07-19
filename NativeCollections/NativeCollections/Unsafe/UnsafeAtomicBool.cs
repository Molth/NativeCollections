using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static NativeCollections.BooleanHelpers;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe atomic 8
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    public unsafe struct UnsafeAtomicBool
    {
        /// <summary>
        ///     Value
        /// </summary>
        private UnsafeAtomicU8 _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicBool(bool value) => _value = new UnsafeAtomicU8(IntoU8(value));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref bool AsRef() => ref Unsafe.As<UnsafeAtomicU8, bool>(ref _value);

        /// <summary>
        ///     Bitwise "nands" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Nand(bool value) => FromU8(_value.Nand(IntoU8(value)));

        /// <summary>
        ///     Bitwise "ands" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool And(bool value) => FromU8(_value.And(IntoU8(value)));

        /// <summary>
        ///     Bitwise "ors" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Or(bool value) => FromU8(_value.Or(IntoU8(value)));

        /// <summary>
        ///     Bitwise "xors" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Xor(bool value) => FromU8(_value.Xor(IntoU8(value)));

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Load(Ordering order) => FromU8(_value.Load(order));

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(bool value, Ordering order) => _value.Store(IntoU8(value), order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Swap(bool value) => FromU8(_value.Swap(IntoU8(value)));

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CompareExchange(bool value, bool comparand) => FromU8(_value.CompareExchange(IntoU8(value), IntoU8(comparand)));

        /// <summary>
        ///     Equals
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
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "UnsafeAtomicBool";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicBool Empty => default;
    }
}