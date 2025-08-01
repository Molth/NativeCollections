using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

#pragma warning disable CS1591
#pragma warning disable CA2208
#pragma warning disable CA2231
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native atomic
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public struct NativeAtomicIntPtr<T> where T : unmanaged
#if NET7_0_OR_GREATER
        , IBinaryInteger<T>
#endif
    {
        /// <summary>
        ///     Value
        /// </summary>
        private nint _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomicIntPtr(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
            {
                _value = new IntPtr(UnsafeHelpers.BitCast<T, byte>(value));
                return;
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                _value = new IntPtr(UnsafeHelpers.BitCast<T, short>(value));
                return;
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                _value = new IntPtr(UnsafeHelpers.BitCast<T, int>(value));
                return;
            }

            _value = UnsafeHelpers.BitCast<T, nint>(value);
        }

        /// <summary>
        ///     Value
        /// </summary>
        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Read();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Exchange(value);
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read()
        {
            CheckType();
            var obj = Interlocked.CompareExchange(ref _value, new IntPtr(0), new IntPtr(0));
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)obj);
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)obj);
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)obj);
            return UnsafeHelpers.BitCast<nint, T>(obj);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Exchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, byte>(value))));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Exchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, short>(value))));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Exchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, int>(value))));
            return UnsafeHelpers.BitCast<nint, T>(Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, nint>(value)));
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.CompareExchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, byte>(value)), new IntPtr(UnsafeHelpers.BitCast<T, byte>(comparand))));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.CompareExchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, short>(value)), new IntPtr(UnsafeHelpers.BitCast<T, short>(comparand))));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.CompareExchange(ref _value, new IntPtr(UnsafeHelpers.BitCast<T, int>(value)), new IntPtr(UnsafeHelpers.BitCast<T, int>(comparand))));
            return UnsafeHelpers.BitCast<nint, T>(Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, nint>(value), UnsafeHelpers.BitCast<T, nint>(comparand)));
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)InterlockedHelpers.Add(ref _value, UnsafeHelpers.BitCast<T, byte>(value)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)InterlockedHelpers.Add(ref _value, UnsafeHelpers.BitCast<T, short>(value)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)InterlockedHelpers.Add(ref _value, UnsafeHelpers.BitCast<T, int>(value)));
            return UnsafeHelpers.BitCast<nint, T>(InterlockedHelpers.Add(ref _value, UnsafeHelpers.BitCast<T, nint>(value)));
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Subtract(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)InterlockedHelpers.Add(ref _value, -(nint)UnsafeHelpers.BitCast<T, byte>(value)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)InterlockedHelpers.Add(ref _value, -(nint)UnsafeHelpers.BitCast<T, short>(value)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)InterlockedHelpers.Add(ref _value, -(nint)UnsafeHelpers.BitCast<T, int>(value)));
            return UnsafeHelpers.BitCast<nint, T>(InterlockedHelpers.Add(ref _value, -UnsafeHelpers.BitCast<T, nint>(value)));
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Increment()
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)InterlockedHelpers.Increment(ref _value));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)InterlockedHelpers.Increment(ref _value));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)InterlockedHelpers.Increment(ref _value));
            return UnsafeHelpers.BitCast<nint, T>(InterlockedHelpers.Increment(ref _value));
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decrement()
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)InterlockedHelpers.Decrement(ref _value));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)InterlockedHelpers.Decrement(ref _value));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)InterlockedHelpers.Decrement(ref _value));
            return UnsafeHelpers.BitCast<nint, T>(InterlockedHelpers.Decrement(ref _value));
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckType()
        {
            if ((typeof(T).IsPrimitive || typeof(T).IsEnum) && (Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || (Unsafe.SizeOf<nint>() == 8 && Unsafe.SizeOf<T>() == 8)) && typeof(T) != typeof(float) && typeof(T) != typeof(double))
                return;
            ThrowHelpers.ThrowNotSupportedException();
        }

        /// <summary>
        ///     Equals
        /// </summary>
        public readonly override bool Equals(object? obj)
        {
            ThrowHelpers.ThrowCannotCallEqualsException();
            return default;
        }

        /// <summary>
        ///     Get hashCode
        /// </summary>
        public readonly override int GetHashCode()
        {
            ThrowHelpers.ThrowCannotCallGetHashCodeException();
            return default;
        }

        /// <summary>
        ///     To string
        /// </summary>
        public readonly override string ToString() => $"NativeAtomicIntPtr{typeof(T).Name}";

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeAtomicIntPtr<T> Empty => new();
    }
}