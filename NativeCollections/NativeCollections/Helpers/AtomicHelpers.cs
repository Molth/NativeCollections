using System;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Atomic helpers
    /// </summary>
    internal static unsafe class AtomicHelpers
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
        public static void Store(ref object? location, object? value, Ordering order)
        {
            switch (order)
            {
                case Ordering.Relaxed:
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
        /// <returns>The original value in <paramref name="location" />.</returns>
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
        /// <returns>The original value in <paramref name="location" />.</returns>
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
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Update(ref nint location, nint value, delegate* managed<nint, nint, nint> func) => Environment.Is64BitProcess ? (nint)Update(ref Unsafe.As<nint, long>(ref location), value, func) : Update(ref Unsafe.As<nint, int>(ref location), value, func);

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Update(ref nuint location, nuint value, delegate* managed<nuint, nuint, nuint> func) => Environment.Is64BitProcess ? (nuint)Update(ref Unsafe.As<nuint, long>(ref location), value, func) : (nuint)Update(ref Unsafe.As<nuint, int>(ref location), value, func);

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Update<T>(ref byte location, T value, delegate* managed<T, T, T> func) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNotSameSizes<byte, T>();
            var current = location;
            while (true)
            {
                var newValue = func(Unsafe.As<byte, T>(ref current), value);
                var oldValue = InterlockedHelpers.CompareExchange(ref location, Unsafe.As<T, byte>(ref newValue), current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort Update<T>(ref ushort location, T value, delegate* managed<T, T, T> func) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNotSameSizes<ushort, T>();
            var current = location;
            while (true)
            {
                var newValue = func(Unsafe.As<ushort, T>(ref current), value);
                var oldValue = InterlockedHelpers.CompareExchange(ref location, Unsafe.As<T, ushort>(ref newValue), current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Update<T>(ref int location, T value, delegate* managed<T, T, T> func) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNotSameSizes<int, T>();
            var current = location;
            while (true)
            {
                var newValue = func(Unsafe.As<int, T>(ref current), value);
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<T, int>(ref newValue), current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Update<T>(ref long location, T value, delegate* managed<T, T, T> func) where T : unmanaged
        {
            ThrowHelpers.ThrowIfNotSameSizes<long, T>();
            var current = location;
            while (true)
            {
                var newValue = func(Unsafe.As<long, T>(ref current), value);
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<T, long>(ref newValue), current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }
    }
}