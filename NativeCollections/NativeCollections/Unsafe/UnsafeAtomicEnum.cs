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
    ///     Unsafe atomic enum
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    public unsafe struct UnsafeAtomicEnum<T> where T : unmanaged, Enum
    {
        /// <summary>
        ///     Value
        /// </summary>
        private T _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeAtomicEnum(T value) => _value = value;

        /// <summary>
        ///     Reinterprets the given location as a reference to this.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MustBePinned(SR.parameter_this)]
        public ref T AsRef() => ref _value;

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load(Ordering order)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Load(order));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Load(order));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Load(order));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Load(order));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Store(T value, Ordering order)
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                Unsafe.As<T, UnsafeAtomicU8>(ref _value).Store(Unsafe.As<T, byte>(ref value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                Unsafe.As<T, UnsafeAtomicU16>(ref _value).Store(Unsafe.As<T, ushort>(ref value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                Unsafe.As<T, UnsafeAtomicI32>(ref _value).Store(Unsafe.As<T, int>(ref value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                Unsafe.As<T, UnsafeAtomicI64>(ref _value).Store(Unsafe.As<T, long>(ref value), order);
                return;
            }

            ThrowHelpers.ThrowNotSupportedException();
        }

        /// <summary>
        ///     Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T And(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).And(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).And(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).And(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).And(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Or(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Or(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Or(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Or(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Or(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Bitwise "xors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Xor(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Xor(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Xor(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Xor(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Xor(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read()
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Read());

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Read());

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Read());

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Read());

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Exchange(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Exchange(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Exchange(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Exchange(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).CompareExchange(Unsafe.As<T, byte>(ref value), Unsafe.As<T, byte>(ref comparand)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).CompareExchange(Unsafe.As<T, ushort>(ref value), Unsafe.As<T, ushort>(ref comparand)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).CompareExchange(Unsafe.As<T, int>(ref value), Unsafe.As<T, int>(ref comparand)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).CompareExchange(Unsafe.As<T, long>(ref value), Unsafe.As<T, long>(ref comparand)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Add(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Add(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Add(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Add(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Subtract(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Subtract(Unsafe.As<T, byte>(ref value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Subtract(Unsafe.As<T, ushort>(ref value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Subtract(Unsafe.As<T, int>(ref value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Subtract(Unsafe.As<T, long>(ref value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Increment()
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Increment());

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Increment());

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Increment());

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Increment());

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decrement()
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU8>(ref _value).Decrement());

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicU16>(ref _value).Decrement());

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI32>(ref _value).Decrement());

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(Unsafe.As<T, UnsafeAtomicI64>(ref _value).Decrement());

            ThrowHelpers.ThrowNotSupportedException();
            return default;
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
        public readonly override string ToString() => SR.Format("UnsafeAtomicEnum<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Cast from other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T CastFromOther<TOther>(TOther other) => Unsafe.As<TOther, T>(ref other);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicEnum<T> Empty => default;
    }
}