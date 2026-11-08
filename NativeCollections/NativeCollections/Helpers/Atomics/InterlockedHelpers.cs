using System;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides atomic operations for variables that are shared by multiple threads.
    /// </summary>
    internal static
#if !NET9_0_OR_GREATER
        unsafe
#endif
        class InterlockedHelpers
    {
        /// <summary>
        ///     Returns a reference value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The reference value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Read(ref object? location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>
        ///     Returns a native-sized signed value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The native-sized signed value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Read(ref nint location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>
        ///     Adds two native-sized signed integers and replaces the
        ///     first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum
        ///     of the two values is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Add(ref nint location, nint value) => Environment.Is64BitProcess ? Isize.FromI64Unchecked(Interlocked.Add(ref Unsafe.As<nint, long>(ref location), I64.FromIsizeUnchecked(value))) : Isize.FromI32Unchecked(Interlocked.Add(ref Unsafe.As<nint, int>(ref location), I32.FromIsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "nands" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Nand(ref nint location, nint value) => Environment.Is64BitProcess ? Isize.FromI64Unchecked(Nand(ref Unsafe.As<nint, long>(ref location), I64.FromIsizeUnchecked(value))) : Isize.FromI32Unchecked(Nand(ref Unsafe.As<nint, int>(ref location), I32.FromIsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "ands" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint And(ref nint location, nint value) => Environment.Is64BitProcess ? Isize.FromI64Unchecked(And(ref Unsafe.As<nint, long>(ref location), I64.FromIsizeUnchecked(value))) : Isize.FromI32Unchecked(And(ref Unsafe.As<nint, int>(ref location), I32.FromIsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "ors" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Or(ref nint location, nint value) => Environment.Is64BitProcess ? Isize.FromI64Unchecked(Or(ref Unsafe.As<nint, long>(ref location), I64.FromIsizeUnchecked(value))) : Isize.FromI32Unchecked(Or(ref Unsafe.As<nint, int>(ref location), I32.FromIsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "xors" two native-sized signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Xor(ref nint location, nint value) => Environment.Is64BitProcess ? Isize.FromI64Unchecked(Xor(ref Unsafe.As<nint, long>(ref location), I64.FromIsizeUnchecked(value))) : Isize.FromI32Unchecked(Xor(ref Unsafe.As<nint, int>(ref location), I32.FromIsizeUnchecked(value)));

        /// <summary>
        ///     Returns a native-sized unsigned value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The native-sized unsigned value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Read(ref nuint location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Sets a native-sized unsigned integer to a specified value
        ///     and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="location">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <returns>The original value of <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Exchange(ref nuint location, nuint value)
        {
#if NET7_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            return Environment.Is64BitProcess ? Usize.FromI64Unchecked(Interlocked.Exchange(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(Interlocked.Exchange(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));
#endif
        }

        /// <summary>
        ///     Compares two native-sized unsigned integers for equality and, if
        ///     they are equal, replaces the first one, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced.
        /// </param>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CompareExchange(ref nuint location, nuint value, nuint comparand)
        {
#if NET7_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            return Environment.Is64BitProcess ? Usize.FromI64Unchecked(Interlocked.CompareExchange(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value), I64.FromUsizeUnchecked(comparand))) : Usize.FromI32Unchecked(Interlocked.CompareExchange(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value), I32.FromUsizeUnchecked(comparand)));
#endif
        }

        /// <summary>
        ///     Adds two native-sized unsigned integers and replaces
        ///     the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum
        ///     of the two values is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Add(ref nuint location, nuint value) => Environment.Is64BitProcess ? Usize.FromI64Unchecked(Interlocked.Add(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(Interlocked.Add(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "nands" two native-sized unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Nand(ref nuint location, nuint value) => Environment.Is64BitProcess ? Usize.FromI64Unchecked(Nand(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(Nand(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "ands" two native-sized unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint And(ref nuint location, nuint value) => Environment.Is64BitProcess ? Usize.FromI64Unchecked(And(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(And(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "ors" two native-sized unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Or(ref nuint location, nuint value) => Environment.Is64BitProcess ? Usize.FromI64Unchecked(Or(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(Or(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));

        /// <summary>
        ///     Bitwise "xors" two native-sized unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Xor(ref nuint location, nuint value) => Environment.Is64BitProcess ? Usize.FromI64Unchecked(Xor(ref Unsafe.As<nuint, long>(ref location), I64.FromUsizeUnchecked(value))) : Usize.FromI32Unchecked(Xor(ref Unsafe.As<nuint, int>(ref location), I32.FromUsizeUnchecked(value)));

        /// <summary>
        ///     Sets an 8-bit unsigned integer to a specified value
        ///     and returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="location">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <returns>The original value of <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Exchange(ref byte location, byte value)
        {
#if NET9_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            fixed (byte* pinnedLocation = &location)
            {
                ref var pinnedRef = ref Unsafe.AsRef<byte>(pinnedLocation);
                var offset = UnsafeHelpers.OpportunisticMisalignment(ref pinnedRef, Usize.FromI32Unchecked(4));
                ref var alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref pinnedRef, Isize.FromUsizeUnchecked(offset)));
                var bitOffset = I32.FromUsizeUnchecked((BitConverter.IsLittleEndian ? offset : Usize.FromI32Unchecked(3).WrappingSub(offset)).WrappingMul(Usize.FromI32Unchecked(8)));
                ThrowHelpers.ThrowIfNotNaturallyAligned(Usize.FromI32Unchecked(bitOffset), Usize.FromI32Unchecked(8), ExceptionArgument.location);
                var mask = ~(U32.FromU8Unchecked(byte.MaxValue) << bitOffset);
                var shiftedValue = U32.FromU8Unchecked(value) << bitOffset;
                var originalValue = alignedRef;
                uint newValue;
                while (true)
                {
                    newValue = (originalValue & mask) | shiftedValue;

                    if (originalValue == (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)))
                        break;
                }

                return U8.FromU32Unchecked(originalValue >> bitOffset);
            }
#endif
        }

        /// <summary>
        ///     Compares two 8-bit unsigned integers for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced.
        /// </param>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CompareExchange(ref byte location, byte value, byte comparand)
        {
#if NET9_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            fixed (byte* pinnedLocation = &location)
            {
                ref var pinnedRef = ref Unsafe.AsRef<byte>(pinnedLocation);
                var offset = UnsafeHelpers.OpportunisticMisalignment(ref pinnedRef, Usize.FromI32Unchecked(4));
                ref var alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref pinnedRef, Isize.FromUsizeUnchecked(offset)));
                var bitOffset = I32.FromUsizeUnchecked((BitConverter.IsLittleEndian ? offset : Usize.FromI32Unchecked(3).WrappingSub(offset)).WrappingMul(Usize.FromI32Unchecked(8)));
                ThrowHelpers.ThrowIfNotNaturallyAligned(Usize.FromI32Unchecked(bitOffset), Usize.FromI32Unchecked(8), ExceptionArgument.location);
                var mask = ~(U32.FromU8Unchecked(byte.MaxValue) << bitOffset);
                var shiftedValue = U32.FromU8Unchecked(value) << bitOffset;
                var shiftedComparand = U32.FromU8Unchecked(comparand) << bitOffset;
                var originalValue = alignedRef;
                uint fullComparand, newValue;
                while (true)
                {
                    var otherMemory = originalValue & mask;
                    fullComparand = otherMemory | shiftedComparand;
                    newValue = otherMemory | shiftedValue;

                    if (originalValue == (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)))
                        break;
                }

                return U8.FromU32Unchecked(originalValue >> bitOffset);
            }
#endif
        }

        /// <summary>
        ///     Adds two 8-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum
        ///     of the two values is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Add(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = U8.FromI32Unchecked(I32.FromU8Unchecked(current).WrappingAdd(I32.FromU8Unchecked(value)));
                var oldValue = CompareExchange(ref location, newValue, current);
                if (I32.FromU8Unchecked(oldValue) == I32.FromU8Unchecked(current))
                    return newValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Returns an 8-bit unsigned value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The 8-bit unsigned value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Read(ref byte location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "nands" two 8-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Nand(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = ~(I32.FromU8Unchecked(current) & I32.FromU8Unchecked(value));
                var oldValue = CompareExchange(ref location, U8.FromI32Unchecked(newValue), current);
                if (I32.FromU8Unchecked(oldValue) == I32.FromU8Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ands" two 8-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte And(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU8Unchecked(current) & I32.FromU8Unchecked(value);
                var oldValue = CompareExchange(ref location, U8.FromI32Unchecked(newValue), current);
                if (I32.FromU8Unchecked(oldValue) == I32.FromU8Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ors" two 8-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Or(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU8Unchecked(current) | I32.FromU8Unchecked(value);
                var oldValue = CompareExchange(ref location, U8.FromI32Unchecked(newValue), current);
                if (I32.FromU8Unchecked(oldValue) == I32.FromU8Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "xors" two 8-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Xor(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU8Unchecked(current) ^ I32.FromU8Unchecked(value);
                var oldValue = CompareExchange(ref location, U8.FromI32Unchecked(newValue), current);
                if (I32.FromU8Unchecked(oldValue) == I32.FromU8Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Sets a 16-bit signed integer to a specified value and
        ///     returns the original value, as an atomic operation.
        /// </summary>
        /// <param name="location">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <returns>The original value of <paramref name="location" />.</returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 On net8.0 and earlier, <paramref name="location" /> must be naturally aligned to 2 bytes
        ///                 because the fallback operates on the naturally aligned 4-byte word that contains the value.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 On net9.0 and later the operation forwards to
        ///                 <see cref="Interlocked" />, which does not validate alignment.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     Thrown on net8.0 and earlier when <paramref name="location" /> is not naturally
        ///     aligned to 2 bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Exchange(ref ushort location, ushort value)
        {
#if NET9_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            fixed (ushort* pinnedLocation = &location)
            {
                ref var pinnedRef = ref Unsafe.AsRef<ushort>(pinnedLocation);
                var offset = UnsafeHelpers.OpportunisticMisalignment(ref pinnedRef, Usize.FromI32Unchecked(4));
                ref var alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref pinnedRef, Isize.FromUsizeUnchecked(offset)));
                var bitOffset = I32.FromUsizeUnchecked((BitConverter.IsLittleEndian ? offset : Usize.FromI32Unchecked(2).WrappingSub(offset)).WrappingMul(Usize.FromI32Unchecked(8)));
                ThrowHelpers.ThrowIfNotNaturallyAligned(Usize.FromI32Unchecked(bitOffset), Usize.FromI32Unchecked(16), ExceptionArgument.location);
                var mask = ~(U32.FromU16Unchecked(ushort.MaxValue) << bitOffset);
                var shiftedValue = U32.FromU16Unchecked(value) << bitOffset;
                var originalValue = alignedRef;
                uint newValue;
                while (true)
                {
                    newValue = (originalValue & mask) | shiftedValue;

                    if (originalValue == (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)))
                        break;
                }

                return U16.FromU32Unchecked(originalValue >> bitOffset);
            }
#endif
        }

        /// <summary>
        ///     Compares two 16-bit signed integers for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced.
        /// </param>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 On net8.0 and earlier, <paramref name="location" /> must be naturally aligned to 2 bytes
        ///                 because the fallback operates on the naturally aligned 4-byte word that contains the value.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 On net9.0 and later the operation forwards to
        ///                 <see cref="Interlocked" />, which does not validate alignment.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     Thrown on net8.0 and earlier when <paramref name="location" /> is not naturally
        ///     aligned to 2 bytes.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CompareExchange(ref ushort location, ushort value, ushort comparand)
        {
#if NET9_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            fixed (ushort* pinnedLocation = &location)
            {
                ref var pinnedRef = ref Unsafe.AsRef<ushort>(pinnedLocation);
                var offset = UnsafeHelpers.OpportunisticMisalignment(ref pinnedRef, Usize.FromI32Unchecked(4));
                ref var alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref pinnedRef, Isize.FromUsizeUnchecked(offset)));
                var bitOffset = I32.FromUsizeUnchecked((BitConverter.IsLittleEndian ? offset : Usize.FromI32Unchecked(2).WrappingSub(offset)).WrappingMul(Usize.FromI32Unchecked(8)));
                ThrowHelpers.ThrowIfNotNaturallyAligned(Usize.FromI32Unchecked(bitOffset), Usize.FromI32Unchecked(16), ExceptionArgument.location);
                var mask = ~(U32.FromU16Unchecked(ushort.MaxValue) << bitOffset);
                var shiftedValue = U32.FromU16Unchecked(value) << bitOffset;
                var shiftedComparand = U32.FromU16Unchecked(comparand) << bitOffset;
                var originalValue = alignedRef;
                uint fullComparand, newValue;
                while (true)
                {
                    var otherMemory = originalValue & mask;
                    fullComparand = otherMemory | shiftedComparand;
                    newValue = otherMemory | shiftedValue;

                    if (originalValue == (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)))
                        break;
                }

                return U16.FromU32Unchecked(originalValue >> bitOffset);
            }
#endif
        }

        /// <summary>
        ///     Adds two 16-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum
        ///     of the two values is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Add(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = U16.FromI32Unchecked(I32.FromU16Unchecked(current).WrappingAdd(I32.FromU16Unchecked(value)));
                var oldValue = CompareExchange(ref location, newValue, current);
                if (I32.FromU16Unchecked(oldValue) == I32.FromU16Unchecked(current))
                    return newValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Returns a 16-bit unsigned value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The 16-bit unsigned value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Read(ref ushort location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "nands" two 16-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Nand(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = ~(I32.FromU16Unchecked(current) & I32.FromU16Unchecked(value));
                var oldValue = CompareExchange(ref location, U16.FromI32Unchecked(newValue), current);
                if (I32.FromU16Unchecked(oldValue) == I32.FromU16Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ands" two 16-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort And(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU16Unchecked(current) & I32.FromU16Unchecked(value);
                var oldValue = CompareExchange(ref location, U16.FromI32Unchecked(newValue), current);
                if (I32.FromU16Unchecked(oldValue) == I32.FromU16Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ors" two 16-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Or(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU16Unchecked(current) | I32.FromU16Unchecked(value);
                var oldValue = CompareExchange(ref location, U16.FromI32Unchecked(newValue), current);
                if (I32.FromU16Unchecked(oldValue) == I32.FromU16Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "xors" two 16-bit unsigned integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Xor(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = I32.FromU16Unchecked(current) ^ I32.FromU16Unchecked(value);
                var oldValue = CompareExchange(ref location, U16.FromI32Unchecked(newValue), current);
                if (I32.FromU16Unchecked(oldValue) == I32.FromU16Unchecked(current))
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Returns a 32-bit signed value, loaded as an atomic operation.
        /// </summary>
        /// <param name="location">The 32-bit value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read(ref int location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "nands" two 32-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Nand(ref int location, int value)
        {
            var current = location;
            while (true)
            {
                var newValue = ~(current & value);
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ands" two 32-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int And(ref int location, int value)
        {
#if NET5_0_OR_GREATER
            return Interlocked.And(ref location, value);
#else
            var current = location;
            while (true)
            {
                var newValue = current & value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
#endif
        }

        /// <summary>
        ///     Bitwise "ors" two 32-bit signed integers and replaces the
        ///     first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Or(ref int location, int value)
        {
#if NET5_0_OR_GREATER
            return Interlocked.Or(ref location, value);
#else
            var current = location;
            while (true)
            {
                var newValue = current | value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
#endif
        }

        /// <summary>
        ///     Bitwise "xors" two 32-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Xor(ref int location, int value)
        {
            var current = location;
            while (true)
            {
                var newValue = current ^ value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Compares two 32-bit unsigned integers for equality and, if
        ///     they are equal, replaces the first one, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly replaced.
        /// </param>
        /// <param name="value">
        ///     The value that replaces the destination value if the comparison results in equality.
        /// </param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CompareExchange(ref uint location, uint value, uint comparand)
        {
#if NET5_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            return U32.FromI32Unchecked(Interlocked.CompareExchange(ref Unsafe.As<uint, int>(ref location), I32.FromU32Unchecked(value), I32.FromU32Unchecked(comparand)));
#endif
        }

        /// <summary>
        ///     Bitwise "nands" two 64-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Nand(ref long location, long value)
        {
            var current = location;
            while (true)
            {
                var newValue = ~(current & value);
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ands" two 64-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long And(ref long location, long value)
        {
#if NET5_0_OR_GREATER
            return Interlocked.And(ref location, value);
#else
            var current = location;
            while (true)
            {
                var newValue = current & value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
#endif
        }

        /// <summary>
        ///     Bitwise "ors" two 64-bit signed integers and replaces the
        ///     first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Or(ref long location, long value)
        {
#if NET5_0_OR_GREATER
            return Interlocked.Or(ref location, value);
#else
            var current = location;
            while (true)
            {
                var newValue = current | value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
#endif
        }

        /// <summary>
        ///     Bitwise "xors" two 64-bit signed integers and replaces
        ///     the first integer with the result, as an atomic operation.
        /// </summary>
        /// <param name="location">A variable containing the first value to be combined.</param>
        /// <param name="value">The value to be combined with the integer at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Xor(ref long location, long value)
        {
            var current = location;
            while (true)
            {
                var newValue = current ^ value;
                var oldValue = Interlocked.CompareExchange(ref location, newValue, current);
                if (oldValue == current)
                    return oldValue;

                current = oldValue;
            }
        }

        /// <summary>
        ///     Adds two single-precision floating-point values and replaces
        ///     the first value with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the bit pattern of the first value to be added.
        ///     The bit pattern of the sum is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the value at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AddFloat32(ref int location, float value)
        {
            UnsafeSpinWait spinWait = default;
            var currentInt32 = location;
            while (true)
            {
                var newFloat32 = Unsafe.As<int, float>(ref currentInt32) + value;
                var oldInt32 = Interlocked.CompareExchange(ref location, Unsafe.As<float, int>(ref newFloat32), currentInt32);
                if (oldInt32 == currentInt32)
                    return newFloat32;

                currentInt32 = oldInt32;
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Adds two double-precision floating-point values and replaces
        ///     the first value with the sum, as an atomic operation.
        /// </summary>
        /// <param name="location">
        ///     A variable containing the bit pattern of the first value to be added.
        ///     The bit pattern of the sum is stored in <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the value at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AddFloat64(ref long location, double value)
        {
            UnsafeSpinWait spinWait = default;
            var currentInt64 = location;
            while (true)
            {
                var newFloat64 = Unsafe.As<long, double>(ref currentInt64) + value;
                var oldInt64 = Interlocked.CompareExchange(ref location, Unsafe.As<double, long>(ref newFloat64), currentInt64);
                if (oldInt64 == currentInt64)
                    return newFloat64;

                currentInt64 = oldInt64;
                spinWait.SpinOnce(-1);
            }
        }
    }
}