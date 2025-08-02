using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS1591
#pragma warning disable CA2208
#pragma warning disable CA2231
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native atomic 64
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public struct NativeAtomic64<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        private long _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomic64(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
            {
                _value = UnsafeHelpers.BitCast<T, byte>(value);
                return;
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                _value = UnsafeHelpers.BitCast<T, short>(value);
                return;
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                _value = UnsafeHelpers.BitCast<T, int>(value);
                return;
            }

            _value = UnsafeHelpers.BitCast<T, long>(value);
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
            var obj = Interlocked.CompareExchange(ref _value, 0, 0);
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)obj);
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)obj);
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)obj);
            return UnsafeHelpers.BitCast<long, T>(obj);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, byte>(value)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, short>(value)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, int>(value)));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, long>(value)));
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, byte>(value), UnsafeHelpers.BitCast<T, byte>(comparand)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, short>(value), UnsafeHelpers.BitCast<T, short>(comparand)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, int>(value), UnsafeHelpers.BitCast<T, int>(comparand)));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, long>(value), UnsafeHelpers.BitCast<T, long>(comparand)));
        }

        /// <summary>
        ///     Adds two values and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Add(T value)
        {
            CheckFloatType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, byte>(value)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, short>(value)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, int>(value)));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, long>(value)));
        }

        /// <summary>
        ///     Subtracts two values and replaces the first integer with the difference, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Subtract(T value)
        {
            CheckFloatType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Add(ref _value, -(long)UnsafeHelpers.BitCast<T, byte>(value)));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Add(ref _value, -(long)UnsafeHelpers.BitCast<T, short>(value)));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Add(ref _value, -(long)UnsafeHelpers.BitCast<T, int>(value)));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.Add(ref _value, -UnsafeHelpers.BitCast<T, long>(value)));
        }

        /// <summary>
        ///     Increments a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Increment()
        {
            CheckFloatType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Increment(ref _value));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Increment(ref _value));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Increment(ref _value));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.Increment(ref _value));
        }

        /// <summary>
        ///     Decrements a specified variable and stores the result, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Decrement()
        {
            CheckFloatType();
            if (Unsafe.SizeOf<T>() == 1)
                return UnsafeHelpers.BitCast<byte, T>((byte)Interlocked.Decrement(ref _value));
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Decrement(ref _value));
            if (Unsafe.SizeOf<T>() == 4)
                return UnsafeHelpers.BitCast<int, T>((int)Interlocked.Decrement(ref _value));
            return UnsafeHelpers.BitCast<long, T>(Interlocked.Decrement(ref _value));
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckType()
        {
            if ((typeof(T).IsPrimitive || typeof(T).IsEnum) && (Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || Unsafe.SizeOf<T>() == 8))
                return;
            ThrowHelpers.ThrowNotSupportedException();
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckFloatType()
        {
            if ((typeof(T).IsPrimitive || typeof(T).IsEnum) && (Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || Unsafe.SizeOf<T>() == 8) && typeof(T) != typeof(float) && typeof(T) != typeof(double))
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
        public readonly override string ToString() => $"NativeAtomic64{typeof(T).Name}";

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeAtomic64<T> Empty => new();
    }
}