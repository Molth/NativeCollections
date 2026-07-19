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
    ///     Unsafe atomic 32
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeAtomicF32
    {
        /// <summary>
        ///     Value
        /// </summary>
        private int _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicF32(float value) => _value = AtomicHelpers.CastToInt32(value);

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref float AsRef() => ref Unsafe.As<int, float>(ref _value);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Load(Ordering order)
        {
            var newInt32 = AtomicHelpers.Load(ref _value, order);
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(float value, Ordering order) => AtomicHelpers.Store(ref _value, AtomicHelpers.CastToInt32(value), order);

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Read()
        {
            var newInt32 = InterlockedHelpers.Read(ref _value);
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Exchange(float value)
        {
            var newInt32 = Interlocked.Exchange(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CompareExchange(float value, float comparand)
        {
            var newInt32 = Interlocked.CompareExchange(ref _value, AtomicHelpers.CastToInt32(value), AtomicHelpers.CastToInt32(comparand));
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Add(float value)
        {
            var newInt32 = AtomicHelpers.AddFloat32(ref _value, Unsafe.As<float, float>(ref value));
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Subtract(float value)
        {
            var newInt32 = AtomicHelpers.AddFloat32(ref _value, -Unsafe.As<float, float>(ref value));
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Increment()
        {
            var newInt32 = AtomicHelpers.AddFloat32(ref _value, 1.0f);
            return AtomicHelpers.CastFromInt32<float>(newInt32);
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Decrement()
        {
            var newInt32 = AtomicHelpers.AddFloat32(ref _value, -1.0f);
            return AtomicHelpers.CastFromInt32<float>(newInt32);
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
        public readonly override string ToString() => "UnsafeAtomicF32";

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicF32 Empty => default;
    }
}