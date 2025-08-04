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
    ///     Native atomic 32
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public struct NativeAtomic32<T> where T : unmanaged
    {
        /// <summary>
        ///     Value
        /// </summary>
        private int _value;

        /// <summary>
        ///     Structure
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeAtomic32(T value)
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

            _value = UnsafeHelpers.BitCast<T, int>(value);
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
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((obj & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)obj);
            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)obj);
            return UnsafeHelpers.BitCast<int, T>(obj);
        }

        /// <summary>
        ///     Sets a value to a specified value and returns the original value, as an atomic operation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Exchange(T value)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, byte>(value));
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, short>(value)));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.Exchange(ref _value, UnsafeHelpers.BitCast<T, int>(value)));
        }

        /// <summary>
        ///     Compares two values for equality and, if they are equal, replaces the first value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T CompareExchange(T value, T comparand)
        {
            CheckType();
            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, byte>(value), UnsafeHelpers.BitCast<T, byte>(comparand));
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, short>(value), UnsafeHelpers.BitCast<T, short>(comparand)));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<T, int>(value), UnsafeHelpers.BitCast<T, int>(comparand)));
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
                var spinWait = new NativeSpinWait();
                float newFloat32;
                while (true)
                {
                    var currentInt32 = Interlocked.CompareExchange(ref _value, 0, 0);
                    newFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32) + Unsafe.As<T, float>(ref value);
                    if (Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<float, int>(newFloat32), currentInt32) != currentInt32)
                        spinWait.SpinOnce(-1);
                    else
                        break;
                }

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, byte>(value));
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, short>(value)));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.Add(ref _value, UnsafeHelpers.BitCast<T, int>(value)));
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
                var spinWait = new NativeSpinWait();
                float newFloat32;
                while (true)
                {
                    var currentInt32 = Interlocked.CompareExchange(ref _value, 0, 0);
                    newFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32) - Unsafe.As<T, float>(ref value);
                    if (Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<float, int>(newFloat32), currentInt32) != currentInt32)
                        spinWait.SpinOnce(-1);
                    else
                        break;
                }

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.Add(ref _value, -UnsafeHelpers.BitCast<T, byte>(value));
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Add(ref _value, -UnsafeHelpers.BitCast<T, short>(value)));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.Add(ref _value, -UnsafeHelpers.BitCast<T, int>(value)));
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
                var spinWait = new NativeSpinWait();
                float newFloat32;
                while (true)
                {
                    var currentInt32 = Interlocked.CompareExchange(ref _value, 0, 0);
                    newFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32) + 1f;
                    if (Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<float, int>(newFloat32), currentInt32) != currentInt32)
                        spinWait.SpinOnce(-1);
                    else
                        break;
                }

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.Increment(ref _value);
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Increment(ref _value));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.Increment(ref _value));
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
                var spinWait = new NativeSpinWait();
                float newFloat32;
                while (true)
                {
                    var currentInt32 = Interlocked.CompareExchange(ref _value, 0, 0);
                    newFloat32 = UnsafeHelpers.BitCast<int, float>(currentInt32) - 1f;
                    if (Interlocked.CompareExchange(ref _value, UnsafeHelpers.BitCast<float, int>(newFloat32), currentInt32) != currentInt32)
                        spinWait.SpinOnce(-1);
                    else
                        break;
                }

                return Unsafe.As<float, T>(ref newFloat32);
            }

            if (Unsafe.SizeOf<T>() == 1)
            {
                var newInt32 = Interlocked.Decrement(ref _value);
                return typeof(T) == typeof(bool) ? UnsafeHelpers.BitCast<bool, T>((newInt32 & 1) != 0) : UnsafeHelpers.BitCast<byte, T>((byte)newInt32);
            }

            if (Unsafe.SizeOf<T>() == 2)
                return UnsafeHelpers.BitCast<short, T>((short)Interlocked.Decrement(ref _value));
            return UnsafeHelpers.BitCast<int, T>(Interlocked.Decrement(ref _value));
        }

        /// <summary>
        ///     Check type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckType()
        {
            if ((typeof(T).IsPrimitive || typeof(T).IsEnum) && (Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4))
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
        public readonly override string ToString() => $"NativeAtomic32{typeof(T).Name}";

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeAtomic32<T> Empty => new();
    }
}