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
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    public unsafe struct UnsafeAtomicI64
    {
        /// <summary>
        ///     Value
        /// </summary>
        private long _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicI64(long value) => _value = value;

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref long AsRef() => ref _value;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Load(Ordering order) => AtomicHelpers.Load(ref _value, order);

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(long value, Ordering order) => AtomicHelpers.Store(ref _value, value, order);

        /// <summary>
        ///     Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long And(long value) => InterlockedHelpers.And(ref _value, value);

        /// <summary>
        ///     Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Or(long value) => InterlockedHelpers.Or(ref _value, value);

        /// <summary>
        ///     Bitwise "xors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Xor(long value) => InterlockedHelpers.Xor(ref _value, value);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Read() => Interlocked.Read(ref _value);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Exchange(long value) => Interlocked.Exchange(ref _value, value);

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long CompareExchange(long value, long comparand) => Interlocked.CompareExchange(ref _value, value, comparand);

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Add(long value) => Interlocked.Add(ref _value, value);

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Subtract(long value) => Interlocked.Add(ref _value, -value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Increment() => Interlocked.Increment(ref _value);

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Decrement() => Interlocked.Decrement(ref _value);

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
        public readonly override string ToString() => "UnsafeAtomicI64";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicI64 Empty => default;
    }
}