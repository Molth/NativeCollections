using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CA2231 // Overload operator equals on overriding ValueType.Equals
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Unsafe atomic 64
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeAtomicF64
    {
        /// <summary>
        ///     Value
        /// </summary>
        private UnsafeAtomicI64 _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicF64(double value) => _value = new UnsafeAtomicI64(BitConverter.DoubleToInt64Bits(value));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref double AsRef() => ref Unsafe.As<UnsafeAtomicI64, double>(ref _value);

        /// <summary>
        ///     Adds two values and replaces the first value with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Add(double value)
        {
            var newInt64 = AtomicHelpers.AddFloat64(ref _value.AsRef(), value);
            return BitConverter.Int64BitsToDouble(newInt64);
        }

        /// <summary>
        ///     Subtracts two values and replaces the first value with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Sub(double value) => Add(-value);

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Max(double value) => BitConverter.Int64BitsToDouble(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max));

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Min(double value) => BitConverter.Int64BitsToDouble(AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min));

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Load(Ordering order)
        {
            var newInt64 = _value.Load(order);
            return BitConverter.Int64BitsToDouble(newInt64);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(double value, Ordering order) => _value.Store(BitConverter.DoubleToInt64Bits(value), order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Swap(double value)
        {
            var newInt64 = _value.Swap(BitConverter.DoubleToInt64Bits(value));
            return BitConverter.Int64BitsToDouble(newInt64);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CompareExchange(double value, double comparand)
        {
            var newInt64 = _value.CompareExchange(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(comparand));
            return BitConverter.Int64BitsToDouble(newInt64);
        }

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
        public readonly override string ToString() => "UnsafeAtomicF64";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicF64 Empty => default;
    }
}