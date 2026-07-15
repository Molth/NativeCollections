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
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard | FromType.Rust)]
    [SupportedTypes(typeof(IntPtr), typeof(UIntPtr), typeof(bool), typeof(byte), typeof(sbyte), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(float), typeof(uint), typeof(double), typeof(long), typeof(ulong))]
    [MaybeSupportedTypes(typeof(Enum))]
    public unsafe struct UnsafeAtomic64<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        private long _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomic64(T value)
        {
            CheckType();
            _value = AtomicHelpers.CastToInt64(value);
        }

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
        public T Load(Ordering order)
        {
            CheckType();
            var newInt64 = AtomicHelpers.LoadInt64(ref _value, order);
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T value, Ordering order)
        {
            CheckType();
            AtomicHelpers.StoreInt64(ref _value, AtomicHelpers.CastToInt64(value), order);
        }

        /// <summary>
        ///     Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T And(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                ThrowHelpers.ThrowNotSupportedException();

            var newInt64 = InterlockedHelpers.AndInt64(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Or(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                ThrowHelpers.ThrowNotSupportedException();

            var newInt64 = InterlockedHelpers.OrInt64(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Bitwise "xors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Xor(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                ThrowHelpers.ThrowNotSupportedException();

            var newInt64 = InterlockedHelpers.XorInt64(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read()
        {
            CheckType();
            var newInt64 = Interlocked.Read(ref _value);
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            CheckType();
            var newInt64 = Interlocked.Exchange(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            CheckType();
            var newInt64 = Interlocked.CompareExchange(ref _value, AtomicHelpers.CastToInt64(value), AtomicHelpers.CastToInt64(comparand));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float))
            {
                var newInt32 = AtomicHelpers.AddFloat32(ref _value, Unsafe.As<T, float>(ref value));
                return AtomicHelpers.CastFromInt32<T>(newInt32);
            }

            var newInt64 = typeof(T) == typeof(double) ? AtomicHelpers.AddFloat64(ref _value, Unsafe.As<T, double>(ref value)) : Interlocked.Add(ref _value, AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Subtract(T value)
        {
            CheckType();
            if (typeof(T) == typeof(float))
            {
                var newInt32 = AtomicHelpers.AddFloat32(ref _value, -Unsafe.As<T, float>(ref value));
                return AtomicHelpers.CastFromInt32<T>(newInt32);
            }

            var newInt64 = typeof(T) == typeof(double) ? AtomicHelpers.AddFloat64(ref _value, -Unsafe.As<T, double>(ref value)) : Interlocked.Add(ref _value, -AtomicHelpers.CastToInt64(value));
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Increment()
        {
            CheckType();
            if (typeof(T) == typeof(float))
            {
                var newInt32 = AtomicHelpers.AddFloat32(ref _value, 1.0f);
                return AtomicHelpers.CastFromInt32<T>(newInt32);
            }

            var newInt64 = typeof(T) == typeof(double) ? AtomicHelpers.AddFloat64(ref _value, 1.0) : Interlocked.Increment(ref _value);
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decrement()
        {
            CheckType();
            if (typeof(T) == typeof(float))
            {
                var newInt32 = AtomicHelpers.AddFloat32(ref _value, -1.0f);
                return AtomicHelpers.CastFromInt32<T>(newInt32);
            }

            var newInt64 = typeof(T) == typeof(double) ? AtomicHelpers.AddFloat64(ref _value, -1.0) : Interlocked.Decrement(ref _value);
            return AtomicHelpers.CastFromInt64<T>(newInt64);
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckType()
        {
            if (AtomicHelpers.IsSupported64<T>())
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
        public readonly override string ToString() => SR.Format("UnsafeAtomic64<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomic64<T> Empty => default;
    }
}