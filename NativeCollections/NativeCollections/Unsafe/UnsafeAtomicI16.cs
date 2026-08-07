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
    ///     Provides atomic operations on a <see cref="short" /> value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicI16
    {
        /// <summary>
        ///     Value
        /// </summary>
        private UnsafeAtomicU16 _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicI16(short value) => _value = new UnsafeAtomicU16((ushort)value);

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref short AsRef() => ref Unsafe.As<UnsafeAtomicU16, short>(ref _value);

        /// <summary>
        ///     Bitwise "nands" two 16-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Nand(short value) => (short)_value.Nand((ushort)value);

        /// <summary>
        ///     Bitwise "ands" two 16-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short And(short value) => (short)_value.And((ushort)value);

        /// <summary>
        ///     Bitwise "ors" two 16-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Or(short value) => (short)_value.Or((ushort)value);

        /// <summary>
        ///     Bitwise "xors" two 16-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Xor(short value) => (short)_value.Xor((ushort)value);

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Add(short value) => (short)_value.Add((ushort)value);

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Sub(short value) => (short)_value.Sub((ushort)value);

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Max(short value) => (short)AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max);

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Min(short value) => (short)AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Load(Ordering order) => (short)_value.Load(order);

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(short value, Ordering order) => _value.Store((ushort)value, order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short Exchange(short value) => (short)_value.Exchange((ushort)value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short CompareExchange(short value, short comparand) => (short)_value.CompareExchange((ushort)value, (ushort)comparand);

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
        public readonly override string ToString() => "UnsafeAtomicI16";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicI16 Empty => default;
    }
}