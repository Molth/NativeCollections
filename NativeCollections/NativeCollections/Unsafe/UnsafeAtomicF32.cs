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
    ///     Provides atomic operations on a <see cref="float" /> value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [BindingType(typeof(Interlocked))]
    public unsafe struct UnsafeAtomicF32
    {
        /// <summary>
        ///     Gets the value to the underlying object.
        /// </summary>
        private UnsafeAtomicI32 _value;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicF32(float value) => _value = new UnsafeAtomicI32(BitConverter.SingleToInt32Bits(value));

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref float AsRef() => ref Unsafe.As<UnsafeAtomicI32, float>(ref _value);

        /// <summary>
        ///     Adds two values and replaces the first value with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Add(float value) => InterlockedHelpers.AddFloat32(ref _value.AsRef(), value);

        /// <summary>
        ///     Subtracts two values and replaces the first value with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Sub(float value) => Add(-value);

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Max(float value)
        {
            var newInt32 = AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Max);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Min(float value)
        {
            var newInt32 = AtomicHelpers.Update(ref _value.AsRef(), value, &Math.Min);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Load(Ordering order)
        {
            var newInt32 = _value.Load(order);
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(float value, Ordering order) => _value.Store(BitConverter.SingleToInt32Bits(value), order);

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Exchange(float value)
        {
            var newInt32 = _value.Exchange(BitConverter.SingleToInt32Bits(value));
            return BitConverter.Int32BitsToSingle(newInt32);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
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
        public readonly override string ToString() => "UnsafeAtomicF32";

        /// <summary>
        ///     Gets an empty instance.
        /// </summary>
        public static UnsafeAtomicF32 Empty => default;
    }
}