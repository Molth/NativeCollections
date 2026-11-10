using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides support for spin-based waiting.
    /// </summary>
    /// <remarks>
    ///     This type is not thread-safe: the spin count is stored in an instance field,
    ///     so each thread must use its own instance.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [BindingType(typeof(SpinWait))]
    public struct UnsafeSpinWait : IEquatable<UnsafeSpinWait>
    {
        /// <summary>
        ///     The underlying spin-wait state, which stores the number of completed spins.
        /// </summary>
        private
#if NET5_0_OR_GREATER
            SpinWait
#else
            SpinWaitHelpers.SpinWait
#endif
            _spinWait;

        /// <summary>
        ///     Gets the number of times <see cref="SpinOnce()" /> has been called on this instance.
        /// </summary>
        public readonly int Count => _spinWait.Count;

        /// <summary>
        ///     Gets whether the next call to <see cref="SpinOnce()" /> will
        ///     yield the processor, triggering a forced context switch.
        /// </summary>
        /// <value>
        ///     Whether the next call to <see cref="SpinOnce()" /> will
        ///     yield the processor, triggering a forced context switch.
        /// </value>
        /// <remarks>
        ///     On a single-CPU machine, <see cref="SpinOnce()" /> always yields the processor. On machines
        ///     with multiple CPUs, <see cref="SpinOnce()" /> may yield after an unspecified number of calls.
        /// </remarks>
        public readonly bool NextSpinWillYield => _spinWait.NextSpinWillYield;

        /// <summary>
        ///     Resets the spin counter.
        /// </summary>
        /// <remarks>
        ///     This makes <see cref="SpinOnce()" /> and <see cref="NextSpinWillYield" /> behave as though no
        ///     calls to <see cref="SpinOnce()" /> had been issued on this instance. If a <see cref="UnsafeSpinWait" />
        ///     instance is reused many times, it may be useful to reset it to avoid yielding too soon.
        /// </remarks>
        public void Reset() => _spinWait.Reset();

        /// <summary>
        ///     Performs a single spin.
        /// </summary>
        /// <remarks>
        ///     This is typically called in a loop, and may change in behavior based on the number
        ///     of times a <see cref="SpinOnce()" /> has been called thus far on this instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce() => _spinWait.SpinOnce();

        /// <summary>
        ///     Performs a single spin.
        /// </summary>
        /// <param name="sleep1Threshold">
        ///     A minimum spin count after which <code>Thread.Sleep(1)</code> may be used. A value
        ///     of <code>-1</code> may be used to disable the use of <code>Thread.Sleep(1)</code>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="sleep1Threshold" /> is less than -1.
        /// </exception>
        /// <remarks>
        ///     This is typically called in a loop, and may change in behavior based on the number
        ///     of times a <see cref="SpinOnce()" /> has been called thus far on this instance.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce(int sleep1Threshold) => _spinWait.SpinOnce(sleep1Threshold);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool Equals(UnsafeSpinWait other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public readonly override bool Equals(object? obj) => obj is UnsafeSpinWait other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the string representation of this instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public readonly override string ToString() => "UnsafeSpinWait";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator ==(UnsafeSpinWait left, UnsafeSpinWait right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if this instance is not equal to
        ///     the other object; otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator !=(UnsafeSpinWait left, UnsafeSpinWait right) => !left.Equals(right);
    }
}