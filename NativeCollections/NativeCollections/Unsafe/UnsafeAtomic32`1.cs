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
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [SupportedTypes(typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(float), typeof(uint))]
    [MaybeSupportedTypes(typeof(Enum), typeof(IntPtr), typeof(UIntPtr))]
    public unsafe struct UnsafeAtomic32<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        private int _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomic32(T value)
        {
            CheckType();
            _value = AtomicHelpers.CastToInt32(value);
        }

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref int AsRef() => ref _value;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load(Ordering order)
        {
            CheckType();
            var newInt32 = AtomicHelpers.LoadInt32(ref _value, order);
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T value, Ordering order)
        {
            CheckType();
            AtomicHelpers.StoreInt32(ref _value, AtomicHelpers.CastToInt32(value), order);
        }

        /// <summary>
        ///     Bitwise "ands" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T And(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float))
                ThrowHelpers.ThrowNotSupportedException();
            var newInt32 = InterlockedHelpers.AndInt32(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Bitwise "ors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Or(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float))
                ThrowHelpers.ThrowNotSupportedException();
            var newInt32 = InterlockedHelpers.OrInt32(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Bitwise "xors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Xor(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float))
                ThrowHelpers.ThrowNotSupportedException();
            var newInt32 = InterlockedHelpers.XorInt32(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read()
        {
            CheckType();
            var newInt32 = InterlockedHelpers.Read(ref _value);
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            CheckType();
            var newInt32 = Interlocked.Exchange(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            CheckType();
            var newInt32 = Interlocked.CompareExchange(ref _value, AtomicHelpers.CastToInt32(value), AtomicHelpers.CastToInt32(comparand));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T value)
        {
            CheckType();
            var newInt32 = typeof(T) == typeof(float) ? AtomicHelpers.AddFloat(ref _value, Unsafe.As<T, float>(ref value)) : Interlocked.Add(ref _value, AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Subtract(T value)
        {
            CheckType();
            var newInt32 = typeof(T) == typeof(float) ? AtomicHelpers.AddFloat(ref _value, -Unsafe.As<T, float>(ref value)) : Interlocked.Add(ref _value, -AtomicHelpers.CastToInt32(value));
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Increment()
        {
            CheckType();
            var newInt32 = typeof(T) == typeof(float) ? AtomicHelpers.AddFloat(ref _value, 1.0f) : Interlocked.Increment(ref _value);
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decrement()
        {
            CheckType();
            var newInt32 = typeof(T) == typeof(float) ? AtomicHelpers.AddFloat(ref _value, -1.0f) : Interlocked.Decrement(ref _value);
            return AtomicHelpers.CastFromInt32<T>(newInt32);
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckType()
        {
            if (AtomicHelpers.IsSupported32<T>())
                return;
            ThrowHelpers.ThrowNotSupportedException();
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
        public readonly override string ToString() => SR.Format("UnsafeAtomic32<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomic32<T> Empty => default;
    }
}