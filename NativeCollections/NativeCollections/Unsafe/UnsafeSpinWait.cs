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
    [StructLayout(LayoutKind.Sequential)]
    [UnsafeCollection(FromType.Standard)]
    [BindingType(typeof(SpinWait))]
    public struct UnsafeSpinWait : IEquatable<UnsafeSpinWait>
    {
        /// <summary>
        ///     Spin wait
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
        public void SpinOnce(int sleep1Threshold) => _spinWait.SpinOnce(sleep1Threshold);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly bool Equals(UnsafeSpinWait other) => SpanHelpers.Equals(ref Unsafe.AsRef(in this), ref other);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is UnsafeSpinWait other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => NativeHashCode.GetHashCode(this);

        /// <summary>
        ///     Returns the fully qualified type name of this instance.
        /// </summary>
        public readonly override string ToString() => "UnsafeSpinWait";

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(UnsafeSpinWait left, UnsafeSpinWait right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(UnsafeSpinWait left, UnsafeSpinWait right) => !left.Equals(right);

        /// <summary>
        ///     Empty
        /// </summary>
        public static UnsafeSpinWait Empty => default;
    }
}