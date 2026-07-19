using System;
using System.Runtime.CompilerServices;
using System.Threading;
using static crossbeam.Result;

namespace crossbeam
{
    internal static unsafe class AtomicHelpers
    {
        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<nuint> TryUpdate<TClosure>(ref nuint location, ref TClosure closure, delegate* managed<ref TClosure, nuint, Option<nuint>> func)
        {
            if (Environment.Is64BitProcess)
            {
                var result = TryUpdate(ref Unsafe.As<nuint, long>(ref location), ref closure, func);
                return new Result<nuint>(result.is_ok(), (nuint)result.unwrap_unchecked());
            }
            else
            {
                var result = TryUpdate(ref Unsafe.As<nuint, int>(ref location), ref closure, func);
                return new Result<nuint>(result.is_ok(), (nuint)result.unwrap_unchecked());
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint Update<TClosure>(ref nuint location, ref TClosure closure, delegate* managed<ref TClosure, nuint, nuint> func) => Environment.Is64BitProcess ? (nuint)Update(ref Unsafe.As<nuint, long>(ref location), ref closure, func) : (nuint)Update(ref Unsafe.As<nuint, int>(ref location), ref closure, func);

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<int> TryUpdate<TParam, TClosure>(ref int location, ref TClosure closure, delegate* managed<ref TClosure, TParam, Option<TParam>> func) where TParam : unmanaged
        {
            ThrowIfNotSameSizes<int, TParam>();
            var current = location;
            while (true)
            {
                var result = func(ref closure, Unsafe.As<int, TParam>(ref current));
                if (result.is_none())
                    return Err(current);
                var newValue = result.unwrap_unchecked();
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<TParam, int>(ref newValue), current);
                if (oldValue == current)
                    return Ok(oldValue);
                current = oldValue;
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Update<TParam, TClosure>(ref int location, ref TClosure closure, delegate* managed<ref TClosure, TParam, TParam> func) where TParam : unmanaged
        {
            ThrowIfNotSameSizes<int, TParam>();
            var current = location;
            while (true)
            {
                var newValue = func(ref closure, Unsafe.As<int, TParam>(ref current));
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<TParam, int>(ref newValue), current);
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
        public static Result<long> TryUpdate<TParam, TClosure>(ref long location, ref TClosure closure, delegate* managed<ref TClosure, TParam, Option<TParam>> func) where TParam : unmanaged
        {
            ThrowIfNotSameSizes<long, TParam>();
            var current = location;
            while (true)
            {
                var result = func(ref closure, Unsafe.As<long, TParam>(ref current));
                if (result.is_none())
                    return Err(current);
                var newValue = result.unwrap_unchecked();
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<TParam, long>(ref newValue), current);
                if (oldValue == current)
                    return Ok(oldValue);
                current = oldValue;
            }
        }

        /// <summary>
        ///     Fetches the value, and applies a function to it that returns an optional new value.
        /// </summary>
        /// <returns>The original value in <paramref name="location" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Update<TParam, TClosure>(ref long location, ref TClosure closure, delegate* managed<ref TClosure, TParam, TParam> func) where TParam : unmanaged
        {
            ThrowIfNotSameSizes<long, TParam>();
            var current = location;
            while (true)
            {
                var newValue = func(ref closure, Unsafe.As<long, TParam>(ref current));
                var oldValue = Interlocked.CompareExchange(ref location, Unsafe.As<TParam, long>(ref newValue), current);
                if (oldValue == current)
                    return oldValue;
                current = oldValue;
            }
        }

        /// <summary>
        ///     Throws an <see cref="NotSupportedException" /> if the sizes of <typeparamref name="TFrom" /> and
        ///     <typeparamref name="TTo" /> are not the same.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNotSameSizes<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged
        {
            if (Unsafe.SizeOf<TFrom>() != Unsafe.SizeOf<TTo>())
                throw new NotSupportedException("NotSupported_CannotCallBitCast");
        }
    }
}