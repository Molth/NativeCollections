using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native spin wait
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.Standard)]
    public struct NativeSpinWait : IEquatable<NativeSpinWait>
    {
        /// <summary>
        ///     Spin wait
        /// </summary>
        private SpinWait _spinWait;

        /// <summary>
        ///     Gets the number of times <see cref="SpinOnce()" /> has been called on this instance.
        /// </summary>
        public readonly int Count => _spinWait.Count;

        /// <summary>
        ///     Gets whether the next call to <see cref="SpinOnce()" /> will yield the processor, triggering a
        ///     forced context switch.
        /// </summary>
        /// <value>
        ///     Whether the next call to <see cref="SpinOnce()" /> will yield the processor, triggering a
        ///     forced context switch.
        /// </value>
        /// <remarks>
        ///     On a single-CPU machine, <see cref="SpinOnce()" /> always yields the processor. On machines with
        ///     multiple CPUs, <see cref="SpinOnce()" /> may yield after an unspecified number of calls.
        /// </remarks>
        public readonly bool NextSpinWillYield => _spinWait.NextSpinWillYield;

        /// <summary>
        ///     Resets the spin counter.
        /// </summary>
        /// <remarks>
        ///     This makes <see cref="SpinOnce()" /> and <see cref="NextSpinWillYield" /> behave as though no calls
        ///     to <see cref="SpinOnce()" /> had been issued on this instance. If a <see cref="SpinWait" /> instance
        ///     is reused many times, it may be useful to reset it to avoid yielding too soon.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _spinWait.Reset();

        /// <summary>
        ///     Performs a single spin.
        /// </summary>
        /// <remarks>
        ///     This is typically called in a loop, and may change in behavior based on the number of times a
        ///     <see cref="SpinOnce()" /> has been called thus far on this instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce() => _spinWait.SpinOnce();

        /// <summary>
        ///     Performs a single spin.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <code>Thread.Sleep(1)</code> may be used.
        ///     A value of <code>-1</code> may be used to disable the use of <code>Thread.Sleep(1)</code>.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     This is typically called in a loop, and may change in behavior based on the number of times a
        ///     <see cref="SpinOnce()" /> has been called thus far on this instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce(int sleep1Threshold)
        {
#if NET5_0_OR_GREATER
            _spinWait.SpinOnce(sleep1Threshold);
#else
            ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
            _spinWait.SpinOnce();
#endif
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public readonly bool Equals(NativeSpinWait other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public readonly override bool Equals(object? obj) => obj is NativeSpinWait nativeSpinWait && nativeSpinWait == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => Unsafe.As<NativeSpinWait, int>(ref this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public readonly override string ToString() => "NativeSpinWait";

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Equals</returns>
        public static bool operator ==(NativeSpinWait left, NativeSpinWait right) => Unsafe.As<NativeSpinWait, int>(ref left) == Unsafe.As<NativeSpinWait, int>(ref right);

        /// <summary>
        ///     Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>Not equals</returns>
        public static bool operator !=(NativeSpinWait left, NativeSpinWait right) => Unsafe.As<NativeSpinWait, int>(ref left) != Unsafe.As<NativeSpinWait, int>(ref right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSpinWait Empty => new();
    }
}