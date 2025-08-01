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
            CheckType();
            if (typeof(T) == typeof(float))
            {
                int currentInt32, newInt32;
                float currentFloat32, newFloat32;
                do
                {
                    currentInt32 = (int)Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32);
                    newFloat32 = currentFloat32 + Unsafe.As<T, float>(ref value);
                    newInt32 = UnsafeHelpers.BitCast<float, int>(newFloat32);
                } while (Interlocked.CompareExchange(ref _value, newInt32, currentInt32) != currentInt32);

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (typeof(T) == typeof(double))
            {
                long currentInt64, newInt64;
                double currentFloat64, newFloat64;
                do
                {
                    currentInt64 = Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat64 = UnsafeHelpers.BitCast<long, double>(currentInt64);
                    newFloat64 = currentFloat64 + Unsafe.As<T, double>(ref value);
                    newInt64 = UnsafeHelpers.BitCast<double, long>(newFloat64);
                } while (Interlocked.CompareExchange(ref _value, newInt64, currentInt64) != currentInt64);

                return Unsafe.As<double, T>(ref newFloat64);
            }

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
            CheckType();
            if (typeof(T) == typeof(float))
            {
                int currentInt32, newInt32;
                float currentFloat32, newFloat32;
                do
                {
                    currentInt32 = (int)Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32);
                    newFloat32 = currentFloat32 - Unsafe.As<T, float>(ref value);
                    newInt32 = UnsafeHelpers.BitCast<float, int>(newFloat32);
                } while (Interlocked.CompareExchange(ref _value, newInt32, currentInt32) != currentInt32);

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (typeof(T) == typeof(double))
            {
                long currentInt64, newInt64;
                double currentFloat64, newFloat64;
                do
                {
                    currentInt64 = Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat64 = UnsafeHelpers.BitCast<long, double>(currentInt64);
                    newFloat64 = currentFloat64 - Unsafe.As<T, double>(ref value);
                    newInt64 = UnsafeHelpers.BitCast<double, long>(newFloat64);
                } while (Interlocked.CompareExchange(ref _value, newInt64, currentInt64) != currentInt64);

                return Unsafe.As<double, T>(ref newFloat64);
            }

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
            CheckType();
            if (typeof(T) == typeof(float))
            {
                int currentInt32, newInt32;
                float currentFloat32, newFloat32;
                do
                {
                    currentInt32 = (int)Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32);
                    newFloat32 = currentFloat32 + 1.0f;
                    newInt32 = UnsafeHelpers.BitCast<float, int>(newFloat32);
                } while (Interlocked.CompareExchange(ref _value, newInt32, currentInt32) != currentInt32);

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (typeof(T) == typeof(double))
            {
                long currentInt64, newInt64;
                double currentFloat64, newFloat64;
                do
                {
                    currentInt64 = Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat64 = UnsafeHelpers.BitCast<long, double>(currentInt64);
                    newFloat64 = currentFloat64 + 1.0;
                    newInt64 = UnsafeHelpers.BitCast<double, long>(newFloat64);
                } while (Interlocked.CompareExchange(ref _value, newInt64, currentInt64) != currentInt64);

                return Unsafe.As<double, T>(ref newFloat64);
            }

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
            CheckType();
            if (typeof(T) == typeof(float))
            {
                int currentInt32, newInt32;
                float currentFloat32, newFloat32;
                do
                {
                    currentInt32 = (int)Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32);
                    newFloat32 = currentFloat32 - 1.0f;
                    newInt32 = UnsafeHelpers.BitCast<float, int>(newFloat32);
                } while (Interlocked.CompareExchange(ref _value, newInt32, currentInt32) != currentInt32);

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (typeof(T) == typeof(double))
            {
                long currentInt64, newInt64;
                double currentFloat64, newFloat64;
                do
                {
                    currentInt64 = Interlocked.CompareExchange(ref _value, 0, 0);
                    currentFloat64 = UnsafeHelpers.BitCast<long, double>(currentInt64);
                    newFloat64 = currentFloat64 - 1.0;
                    newInt64 = UnsafeHelpers.BitCast<double, long>(newFloat64);
                } while (Interlocked.CompareExchange(ref _value, newInt64, currentInt64) != currentInt64);

                return Unsafe.As<double, T>(ref newFloat64);
            }

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
            if ((!typeof(T).IsPrimitive && !typeof(T).IsEnum) || (Unsafe.SizeOf<T>() != 1 && Unsafe.SizeOf<T>() != 2 && Unsafe.SizeOf<T>() != 4 && Unsafe.SizeOf<T>() != 8))
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