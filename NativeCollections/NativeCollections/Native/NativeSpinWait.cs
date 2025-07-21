using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native spin wait
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [NativeCollection(FromType.None)]
    public struct NativeSpinWait
    {
        /// <summary>
        ///     Spin wait
        /// </summary>
        private SpinWait _spinWait;

        /// <summary>
        ///     Count
        /// </summary>
        public readonly int Count => _spinWait.Count;

        /// <summary>
        ///     Next spin will yield
        /// </summary>
        public readonly bool NextSpinWillYield => _spinWait.NextSpinWillYield;

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _spinWait.Reset();

        /// <summary>
        ///     Spin once
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce() => _spinWait.SpinOnce();

        /// <summary>
        ///     Spin once
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce(int sleepThreshold)
        {
#if NET5_0_OR_GREATER
            _spinWait.SpinOnce(sleepThreshold);
#else
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