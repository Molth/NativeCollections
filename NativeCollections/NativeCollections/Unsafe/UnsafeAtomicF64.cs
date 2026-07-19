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
    ///     Unsafe atomic 64
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeAtomicF64
    {
        /// <summary>
        ///     Value
        /// </summary>
        private long _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicF64(double value) => _value = AtomicHelpers.CastToInt64(value);

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref double AsRef() => ref Unsafe.As<long, double>(ref _value);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Load(Ordering order)
        {
            var newInt64 = AtomicHelpers.Load(ref _value, order);
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(double value, Ordering order) => AtomicHelpers.Store(ref _value, AtomicHelpers.CastToInt64(value), order);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Read()
        {
            var newInt64 = Interlocked.Read(ref _value);
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Exchange(double value)
        {
            var newInt64 = Interlocked.Exchange(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CompareExchange(double value, double comparand)
        {
            var newInt64 = Interlocked.CompareExchange(ref _value, AtomicHelpers.CastToInt64(value), AtomicHelpers.CastToInt64(comparand));
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Add(double value)
        {
            var newInt64 = AtomicHelpers.AddFloat64(ref _value, Unsafe.As<double, double>(ref value));
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Subtract(double value)
        {
            var newInt64 = AtomicHelpers.AddFloat64(ref _value, -Unsafe.As<double, double>(ref value));
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Increment()
        {
            var newInt64 = AtomicHelpers.AddFloat64(ref _value, 1.0);
            return AtomicHelpers.CastFromInt64<double>(newInt64);
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Decrement()
        {
            var newInt64 = AtomicHelpers.AddFloat64(ref _value, -1.0);
            return AtomicHelpers.CastFromInt64<double>(newInt64);
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