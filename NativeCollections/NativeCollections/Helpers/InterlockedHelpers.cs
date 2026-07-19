using System;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Interlocked helpers
    /// </summary>
    internal static class InterlockedHelpers
    {
        /// <summary>Returns a reference value, loaded as an atomic operation.</summary>
        /// <param name="location">The reference value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Read(ref object? location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>Returns a native-sized signed value, loaded as an atomic operation.</summary>
        /// <param name="location">The native-sized signed value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Read(ref nint location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>
        ///     Adds two native-sized signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Add(ref nint location, nint value) => Environment.Is64BitProcess ? (nint)Interlocked.Add(ref Unsafe.As<nint, long>(ref location), value) : Interlocked.Add(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Increment(ref nint location) => Environment.Is64BitProcess ? (nint)Interlocked.Increment(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Increment(ref Unsafe.As<nint, int>(ref location));

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Decrement(ref nint location) => Environment.Is64BitProcess ? (nint)Interlocked.Decrement(ref Unsafe.As<nint, long>(ref location)) : Interlocked.Decrement(ref Unsafe.As<nint, int>(ref location));

        /// <summary>
        ///     Bitwise "ands" two native-sized signed integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint And(ref nint location, nint value) => Environment.Is64BitProcess ? (nint)And(ref Unsafe.As<nint, long>(ref location), value) : And(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>
        ///     Bitwise "ors" two native-sized signed integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Or(ref nint location, nint value) => Environment.Is64BitProcess ? (nint)Or(ref Unsafe.As<nint, long>(ref location), value) : Or(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>
        ///     Bitwise "xors" two native-sized signed integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Xor(ref nint location, nint value) => Environment.Is64BitProcess ? (nint)Xor(ref Unsafe.As<nint, long>(ref location), value) : Xor(ref Unsafe.As<nint, int>(ref location), (int)value);

        /// <summary>Returns a native-sized unsigned value, loaded as an atomic operation.</summary>
        /// <param name="location">The native-sized unsigned value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Read(ref nuint location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Sets a native-sized unsigned integer to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value of <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Exchange(ref nuint location, nuint value)
        {
#if NET7_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            return Environment.Is64BitProcess ? (nuint)Interlocked.Exchange(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Interlocked.Exchange(ref Unsafe.As<nuint, int>(ref location), (int)value);
#endif
        }

        /// <summary>
        ///     Compares two native-sized unsigned integers for equality and, if they are equal, replaces the first one, as an
        ///     atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint CompareExchange(ref nuint location, nuint value, nuint comparand)
        {
#if NET7_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            return Environment.Is64BitProcess ? (nuint)Interlocked.CompareExchange(ref Unsafe.As<nuint, long>(ref location), (long)value, (long)comparand) : (nuint)Interlocked.CompareExchange(ref Unsafe.As<nuint, int>(ref location), (int)value, (int)comparand);
#endif
        }

        /// <summary>
        ///     Adds two native-sized unsigned integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Add(ref nuint location, nuint value) => Environment.Is64BitProcess ? (nuint)Interlocked.Add(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Interlocked.Add(ref Unsafe.As<nuint, int>(ref location), (int)value);

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The incremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Increment(ref nuint location) => Environment.Is64BitProcess ? (nuint)Interlocked.Increment(ref Unsafe.As<nuint, long>(ref location)) : (nuint)Interlocked.Increment(ref Unsafe.As<nuint, int>(ref location));

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        /// <returns>The decremented value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Decrement(ref nuint location) => Environment.Is64BitProcess ? (nuint)Interlocked.Decrement(ref Unsafe.As<nuint, long>(ref location)) : (nuint)Interlocked.Decrement(ref Unsafe.As<nuint, int>(ref location));

        /// <summary>
        ///     Bitwise "ands" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint And(ref nuint location, nuint value) => Environment.Is64BitProcess ? (nuint)And(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)And(ref Unsafe.As<nuint, int>(ref location), (int)value);

        /// <summary>
        ///     Bitwise "ors" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Or(ref nuint location, nuint value) => Environment.Is64BitProcess ? (nuint)Or(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Or(ref Unsafe.As<nuint, int>(ref location), (int)value);

        /// <summary>
        ///     Bitwise "xors" two native-sized unsigned integers and replaces the first integer with the result, as an atomic
        ///     operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Xor(ref nuint location, nuint value) => Environment.Is64BitProcess ? (nuint)Xor(ref Unsafe.As<nuint, long>(ref location), (long)value) : (nuint)Xor(ref Unsafe.As<nuint, int>(ref location), (int)value);

        /// <summary>Sets a 8-bit unsigned integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <returns>The original value of <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Exchange(ref byte location, byte value)
        {
#if NET9_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            var offset = UnsafeHelpers.OpportunisticMisalignment(ref location, 4);
            ref var alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref location, (nint)offset));
            var bitOffset = (int)((BitConverter.IsLittleEndian ? offset : 3 - offset) * 8);
            var mask = ~((uint)byte.MaxValue << bitOffset);
            var shiftedValue = (uint)value << bitOffset;
            var originalValue = alignedRef;
            uint newValue;
            do
            {
                newValue = (originalValue & mask) | shiftedValue;
            } while (originalValue != (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)));

            return (byte)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Compares two 8-bit unsigned integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly
        ///     replaced.
        /// </param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CompareExchange(ref byte location, byte value, byte comparand)
        {
#if NET9_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            var offset = UnsafeHelpers.OpportunisticMisalignment(ref location, 4);
            ref var alignedRef = ref Unsafe.As<byte, uint>(ref Unsafe.SubtractByteOffset(ref location, (nint)offset));
            var bitOffset = (int)((BitConverter.IsLittleEndian ? offset : 3 - offset) * 8);
            var mask = ~((uint)byte.MaxValue << bitOffset);
            var shiftedValue = (uint)value << bitOffset;
            var shiftedComparand = (uint)comparand << bitOffset;
            var originalValue = alignedRef;
            uint fullComparand, newValue;
            do
            {
                var otherMemory = originalValue & mask;
                fullComparand = otherMemory | shiftedComparand;
                newValue = otherMemory | shiftedValue;
            } while (originalValue != (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)));

            return (byte)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Adds two 8-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum of the two values is stored in
        ///     <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location" /> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Add(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = current + value;
                var oldValue = CompareExchange(ref location, (byte)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>Returns a 8-bit unsigned value, loaded as an atomic operation.</summary>
        /// <param name="location">The native-sized signed value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Read(ref byte location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "ands" two 8-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte And(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = current & value;
                var oldValue = CompareExchange(ref location, (byte)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ors" two 8-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Or(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = current | value;
                var oldValue = CompareExchange(ref location, (byte)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "xors" two 8-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Xor(ref byte location, byte value)
        {
            var current = location;
            while (true)
            {
                var newValue = current ^ value;
                var oldValue = CompareExchange(ref location, (byte)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>Sets a 16-bit signed integer to a specified value and returns the original value, as an atomic operation.</summary>
        /// <param name="location">The variable to set to the specified value.</param>
        /// <param name="value">The value to which the <paramref name="location" /> parameter is set.</param>
        /// <returns>The original value of <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Exchange(ref ushort location, ushort value)
        {
#if NET9_0_OR_GREATER
            return Interlocked.Exchange(ref location, value);
#else
            var offset = UnsafeHelpers.OpportunisticMisalignment(ref location, 4);
            ref var alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref location, (nint)offset));
            var bitOffset = (int)((BitConverter.IsLittleEndian ? offset : 2 - offset) * 8);
            var mask = ~((uint)ushort.MaxValue << bitOffset);
            var shiftedValue = (uint)value << bitOffset;
            var originalValue = alignedRef;
            uint newValue;
            do
            {
                newValue = (originalValue & mask) | shiftedValue;
            } while (originalValue != (originalValue = CompareExchange(ref alignedRef, newValue, originalValue)));

            return (ushort)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Compares two 16-bit signed integers for equality and, if they are equal, replaces the first value.</summary>
        /// <param name="location">
        ///     The destination, whose value is compared with <paramref name="comparand" /> and possibly
        ///     replaced.
        /// </param>
        /// <param name="value">The value that replaces the destination value if the comparison results in equality.</param>
        /// <param name="comparand">The value that is compared to the value at <paramref name="location" />.</param>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CompareExchange(ref ushort location, ushort value, ushort comparand)
        {
#if NET9_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            var offset = UnsafeHelpers.OpportunisticMisalignment(ref location, 4);
            ref var alignedRef = ref Unsafe.As<ushort, uint>(ref Unsafe.SubtractByteOffset(ref location, (nint)offset));
            var bitOffset = (int)((BitConverter.IsLittleEndian ? offset : 2 - offset) * 8);
            var mask = ~((uint)ushort.MaxValue << bitOffset);
            var shiftedValue = (uint)value << bitOffset;
            var shiftedComparand = (uint)comparand << bitOffset;
            var originalValue = alignedRef;
            uint fullComparand, newValue;
            do
            {
                var otherMemory = originalValue & mask;
                fullComparand = otherMemory | shiftedComparand;
                newValue = otherMemory | shiftedValue;
            } while (originalValue != (originalValue = CompareExchange(ref alignedRef, newValue, fullComparand)));

            return (ushort)(originalValue >> bitOffset);
#endif
        }

        /// <summary>Adds two 16-bit unsigned integers and replaces the first integer with the sum, as an atomic operation.</summary>
        /// <param name="location">
        ///     A variable containing the first value to be added. The sum of the two values is stored in
        ///     <paramref name="location" />.
        /// </param>
        /// <param name="value">The value to be added to the integer at <paramref name="location" />.</param>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        /// <exception cref="NullReferenceException">The address of <paramref name="location" /> is a null pointer.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Add(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = current + value;
                var oldValue = CompareExchange(ref location, (ushort)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>Returns a 16-bit unsigned value, loaded as an atomic operation.</summary>
        /// <param name="location">The native-sized signed value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Read(ref ushort location) => CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "ands" two 16-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort And(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = current & value;
                var oldValue = CompareExchange(ref location, (ushort)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "ors" two 16-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Or(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = current | value;
                var oldValue = CompareExchange(ref location, (ushort)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Bitwise "xors" two 16-bit unsigned integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Xor(ref ushort location, ushort value)
        {
            var current = location;
            while (true)
            {
                var newValue = current ^ value;
                var oldValue = CompareExchange(ref location, (ushort)newValue, current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>Returns a 32-bit signed value, loaded as an atomic operation.</summary>
        /// <param name="location">The 32-bit value to be loaded.</param>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Read(ref int location) => Interlocked.CompareExchange(ref location, default, default);

        /// <summary>
        ///     Bitwise "ands" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
        ///     Bitwise "ors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
        ///     Bitwise "xors" two 32-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
        ///     Compares two 32-bit unsigned integers for equality and, if they are equal, replaces the first one, as an
        ///     atomic operation.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CompareExchange(ref uint location, uint value, uint comparand)
        {
#if NET5_0_OR_GREATER
            return Interlocked.CompareExchange(ref location, value, comparand);
#else
            return (uint)Interlocked.CompareExchange(ref Unsafe.As<uint, int>(ref location), (int)value, (int)comparand);
#endif
        }

        /// <summary>
        ///     Bitwise "ands" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
        ///     Bitwise "ors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
        ///     Bitwise "xors" two 64-bit signed integers and replaces the first integer with the result, as an atomic operation.
        /// </summary>
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
    }
}