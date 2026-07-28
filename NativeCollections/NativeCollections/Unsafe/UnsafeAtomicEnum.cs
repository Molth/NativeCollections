using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using static NativeCollections.EnumHelpers;

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
    [BindingType(typeof(Interlocked))]
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
        ///     Bitwise "nands" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Nand(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Nand(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Nand(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Nand(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Nand(CastToOther<ulong>(value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Bitwise "ands" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T And(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().And(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().And(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().And(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().And(CastToOther<ulong>(value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Bitwise "ors" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Or(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Or(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Or(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Or(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Or(CastToOther<ulong>(value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Bitwise "xors" two integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Xor(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Xor(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Xor(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Xor(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Xor(CastToOther<ulong>(value)));

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
                return CastFromOther(AsU8().Add(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Add(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Add(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Add(CastToOther<ulong>(value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Sub(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Sub(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Sub(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Sub(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Sub(CastToOther<ulong>(value)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Finds the maximum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Max(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI8().Max(CastToOther<sbyte>(value)));

                return CastFromOther(AsU8().Max(CastToOther<byte>(value)));
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI16().Max(CastToOther<short>(value)));

                return CastFromOther(AsU16().Max(CastToOther<ushort>(value)));
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI32().Max(CastToOther<int>(value)));

                return CastFromOther(AsU32().Max(CastToOther<uint>(value)));
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI64().Max(CastToOther<long>(value)));

                return CastFromOther(AsU64().Max(CastToOther<ulong>(value)));
            }

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Finds the minimum of the current value and the argument, and sets the new value to the result.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Min(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI8().Min(CastToOther<sbyte>(value)));

                return CastFromOther(AsU8().Min(CastToOther<byte>(value)));
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI16().Min(CastToOther<short>(value)));

                return CastFromOther(AsU16().Min(CastToOther<ushort>(value)));
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI32().Min(CastToOther<int>(value)));

                return CastFromOther(AsU32().Min(CastToOther<uint>(value)));
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                if (IsSigned<T>())
                    return CastFromOther(AsI64().Min(CastToOther<long>(value)));

                return CastFromOther(AsU64().Min(CastToOther<ulong>(value)));
            }

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Load(Ordering order)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Load(order));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Load(order));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Load(order));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Load(order));

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
                AsU8().Store(CastToOther<byte>(value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                AsU16().Store(CastToOther<ushort>(value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                AsU32().Store(CastToOther<uint>(value), order);
                return;
            }

            if (Unsafe.SizeOf<T>() == 8)
            {
                AsU64().Store(CastToOther<ulong>(value), order);
                return;
            }

            ThrowHelpers.ThrowNotSupportedException();
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Swap(T value)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return CastFromOther(AsU8().Swap(CastToOther<byte>(value)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().Swap(CastToOther<ushort>(value)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().Swap(CastToOther<uint>(value)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().Swap(CastToOther<ulong>(value)));

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
                return CastFromOther(AsU8().CompareExchange(CastToOther<byte>(value), CastToOther<byte>(comparand)));

            if (Unsafe.SizeOf<T>() == 2)
                return CastFromOther(AsU16().CompareExchange(CastToOther<ushort>(value), CastToOther<ushort>(comparand)));

            if (Unsafe.SizeOf<T>() == 4)
                return CastFromOther(AsU32().CompareExchange(CastToOther<uint>(value), CastToOther<uint>(comparand)));

            if (Unsafe.SizeOf<T>() == 8)
                return CastFromOther(AsU64().CompareExchange(CastToOther<ulong>(value), CastToOther<ulong>(comparand)));

            ThrowHelpers.ThrowNotSupportedException();
            return default;
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
        public readonly override string ToString() => SR.Format("UnsafeAtomicEnum<{0}>", SR.GetTypeName(typeof(T)));

        /// <summary>
        ///     Cast from other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T CastFromOther<TFrom>(TFrom value) where TFrom : unmanaged => UnsafeHelpers.BitCast<TFrom, T>(value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TTo CastToOther<TTo>(T value) where TTo : unmanaged => UnsafeHelpers.BitCast<T, TTo>(value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicI8 AsI8() => ref Unsafe.As<T, UnsafeAtomicI8>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicU8 AsU8() => ref Unsafe.As<T, UnsafeAtomicU8>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicI16 AsI16() => ref Unsafe.As<T, UnsafeAtomicI16>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicU16 AsU16() => ref Unsafe.As<T, UnsafeAtomicU16>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicI32 AsI32() => ref Unsafe.As<T, UnsafeAtomicI32>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicU32 AsU32() => ref Unsafe.As<T, UnsafeAtomicU32>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicI64 AsI64() => ref Unsafe.As<T, UnsafeAtomicI64>(ref _value);

        /// <summary>
        ///     Cast to other
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref UnsafeAtomicU64 AsU64() => ref Unsafe.As<T, UnsafeAtomicU64>(ref _value);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeAtomicEnum<T> Empty => default;
    }
}