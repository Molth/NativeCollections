using System;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Atomic helpers
    /// </summary>
    internal static class AtomicHelpers
    {
        /// <summary>
        ///     Adds two 64-bit signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AddFloat64(ref long location, double value)
        {
            var spinWait = new UnsafeSpinWait();
            var currentInt64 = location;
            while (true)
            {
                var newFloat64 = Unsafe.As<long, double>(ref currentInt64) + value;
                var oldInt64 = Interlocked.CompareExchange(ref location, Unsafe.As<double, long>(ref newFloat64), currentInt64);
                if (oldInt64 == currentInt64)
                    return oldInt64;
                currentInt64 = oldInt64;
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Adds two 32-bit signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddFloat32(ref long location, float value)
        {
            var spinWait = new UnsafeSpinWait();
            var currentInt64 = location;
            while (true)
            {
                var currentInt32 = (int)currentInt64;
                var newFloat32 = Unsafe.As<int, float>(ref currentInt32) + value;
                var oldInt64 = Interlocked.CompareExchange(ref location, Unsafe.As<float, int>(ref newFloat32), currentInt64);
                if (oldInt64 == currentInt64)
                    return (int)oldInt64;
                currentInt64 = oldInt64;
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Adds two 32-bit signed integers and replaces the first integer with the sum, as an atomic operation.
        /// </summary>
        /// <returns>The new value stored at <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddFloat(ref int location, float value)
        {
            var spinWait = new UnsafeSpinWait();
            var currentInt32 = location;
            while (true)
            {
                var newFloat32 = Unsafe.As<int, float>(ref currentInt32) + value;
                var oldInt32 = Interlocked.CompareExchange(ref location, Unsafe.As<float, int>(ref newFloat32), currentInt32);
                if (oldInt32 == currentInt32)
                    return oldInt32;
                currentInt32 = oldInt32;
                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Cast to Int64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CastToInt64<T>(T value) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() == 1)
                return Unsafe.As<T, byte>(ref value);

            if (Unsafe.SizeOf<T>() == 2)
                return Unsafe.As<T, short>(ref value);

            if (Unsafe.SizeOf<T>() == 4)
                return Unsafe.As<T, int>(ref value);

            if (Unsafe.SizeOf<T>() == 8)
                return Unsafe.As<T, long>(ref value);

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Cast to 32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CastToInt32<T>(T value) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() == 1)
                return Unsafe.As<T, byte>(ref value);

            if (Unsafe.SizeOf<T>() == 2)
                return Unsafe.As<T, short>(ref value);

            if (Unsafe.SizeOf<T>() == 4)
                return Unsafe.As<T, int>(ref value);

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Cast from Int64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CastFromInt64<T>(long value) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                if (typeof(T) == typeof(bool))
                {
                    var source = (value & 1) != 0;
                    return Unsafe.As<bool, T>(ref source);
                }
                else
                {
                    var source = (byte)value;
                    return Unsafe.As<byte, T>(ref source);
                }
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                var source = (short)value;
                return Unsafe.As<short, T>(ref source);
            }

            if (Unsafe.SizeOf<T>() == 4)
            {
                var source = (int)value;
                return Unsafe.As<int, T>(ref source);
            }

            if (Unsafe.SizeOf<T>() == 8)
                return Unsafe.As<long, T>(ref value);

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Cast from Int32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CastFromInt32<T>(int value) where T : unmanaged
        {
            if (Unsafe.SizeOf<T>() == 1)
            {
                if (typeof(T) == typeof(bool))
                {
                    var source = (value & 1) != 0;
                    return Unsafe.As<bool, T>(ref source);
                }
                else
                {
                    var source = (byte)value;
                    return Unsafe.As<byte, T>(ref source);
                }
            }

            if (Unsafe.SizeOf<T>() == 2)
            {
                var source = (short)value;
                return Unsafe.As<short, T>(ref source);
            }

            if (Unsafe.SizeOf<T>() == 4)
                return Unsafe.As<int, T>(ref value);

            ThrowHelpers.ThrowNotSupportedException();
            return default;
        }

        /// <summary>
        ///     Is supported for target-64
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSupported64<T>() where T : unmanaged => Atomic64Helpers<T>.IsSupported;

        /// <summary>
        ///     Is supported for target-32
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSupported32<T>() where T : unmanaged => Atomic32Helpers<T>.IsSupported;

        /// <summary>
        ///     Atomic helpers
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        private static class Atomic64Helpers<T> where T : unmanaged
        {
            /// <summary>
            ///     Is supported for target-64
            /// </summary>
            public static readonly bool IsSupported = IsSupportedPrivate();

            /// <summary>
            ///     Is supported for target-64
            /// </summary>
            private static bool IsSupportedPrivate()
            {
                if (typeof(T) == typeof(IntPtr) || typeof(T) == typeof(UIntPtr))
                    return true;

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.Single:
                    case TypeCode.UInt32:
                    case TypeCode.Double:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        ///     Atomic helpers
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        private static class Atomic32Helpers<T> where T : unmanaged
        {
            /// <summary>
            ///     Is supported for target-32
            /// </summary>
            public static readonly bool IsSupported = IsSupportedPrivate();

            /// <summary>
            ///     Is supported for target-32
            /// </summary>
            private static bool IsSupportedPrivate()
            {
                if (typeof(T) == typeof(IntPtr) || typeof(T) == typeof(UIntPtr))
                    return !Environment.Is64BitProcess;

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Char:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.Single:
                    case TypeCode.UInt32:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}