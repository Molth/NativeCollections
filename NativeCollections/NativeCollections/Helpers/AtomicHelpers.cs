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
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? Load(ref object? location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return Interlocked.CompareExchange(ref location, default, default);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref object? location, object? value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    Interlocked.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Load(ref nint location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return InterlockedHelpers.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref nint location, nint value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    Interlocked.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Load(ref nuint location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return InterlockedHelpers.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref nuint location, nuint value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    InterlockedHelpers.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Load(ref long location, Ordering order)
        {
            if (Environment.Is64BitProcess)
            {
                switch (order)
                {
                    case Ordering.Relaxed:
                        return location;

                    case Ordering.Acquire:
                    case Ordering.AcqRel:
                        return Volatile.Read(ref location);

                    case Ordering.SeqCst:
                        return Interlocked.Read(ref location);

                    case Ordering.Release:
                    default:
                        ThrowHelpers.ThrowNotSupportedException();
                        return default;
                }
            }

            switch (order)
            {
                case Ordering.Relaxed:
                case Ordering.Acquire:
                case Ordering.AcqRel:
                case Ordering.SeqCst:
                    return Interlocked.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref long location, long value, Ordering order)
        {
            if (Environment.Is64BitProcess)
            {
                switch (order)
                {
                    case Ordering.Relaxed:
                        location = value;
                        return;

                    case Ordering.Release:
                    case Ordering.AcqRel:
                        Volatile.Write(ref location, value);
                        return;

                    case Ordering.SeqCst:
                        Interlocked.Exchange(ref location, value);
                        return;

                    case Ordering.Acquire:
                    default:
                        ThrowHelpers.ThrowNotSupportedException();
                        return;
                }
            }

            switch (order)
            {
                case Ordering.Relaxed:
                case Ordering.Release:
                case Ordering.AcqRel:
                case Ordering.SeqCst:
                    Interlocked.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Load(ref int location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return InterlockedHelpers.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref int location, int value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    Interlocked.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Load(ref ushort location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return InterlockedHelpers.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref ushort location, ushort value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    InterlockedHelpers.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

        /// <summary>
        ///     Returns a value, loaded as an atomic operation.
        /// </summary>
        /// <returns>The loaded value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Load(ref byte location, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    return location;

                case Ordering.Acquire:
                case Ordering.AcqRel:
                    return Volatile.Read(ref location);

                case Ordering.SeqCst:
                    return InterlockedHelpers.Read(ref location);

                case Ordering.Release:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return default;
            }
        }

        /// <summary>
        ///     Sets a value to a specified value, as an atomic operation.
        /// </summary>
        /// <exception cref="NotSupportedException">Ordering is not supported.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(ref byte location, byte value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
                    location = value;
                    return;

                case Ordering.Release:
                case Ordering.AcqRel:
                    Volatile.Write(ref location, value);
                    return;

                case Ordering.SeqCst:
                    InterlockedHelpers.Exchange(ref location, value);
                    return;

                case Ordering.Acquire:
                default:
                    ThrowHelpers.ThrowNotSupportedException();
                    return;
            }
        }

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
        public static int AddFloat32(ref int location, float value)
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
                var source = (byte)value;
                return Unsafe.As<byte, T>(ref source);
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
                var source = (byte)value;
                return Unsafe.As<byte, T>(ref source);
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
    }
}