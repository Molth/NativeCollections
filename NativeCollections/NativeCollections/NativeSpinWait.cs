using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
#if !NET5_0_OR_GREATER
using System;
#endif

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
#if NET5_0_OR_GREATER
        /// <summary>
        ///     Spin wait
        /// </summary>
        private SpinWait _spinWait;
#else
        /// <summary>
        ///     Count
        /// </summary>
        private int _count;
#endif

        /// <summary>
        ///     Count
        /// </summary>
        public int Count
        {
            get
            {
#if NET5_0_OR_GREATER
                return _spinWait.Count;
#else
                return _count;
#endif
            }
        }

        /// <summary>
        ///     Next spin will yield
        /// </summary>
        public bool NextSpinWillYield
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if NET5_0_OR_GREATER
                return _spinWait.NextSpinWillYield;
#else
                return _count >= 10 || Environment.ProcessorCount == 1;
#endif
            }
        }

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
#if NET5_0_OR_GREATER
            _spinWait.Reset();
#else
            _count = 0;
#endif
        }

        /// <summary>
        ///     Spin once
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce()
        {
#if NET5_0_OR_GREATER
            _spinWait.SpinOnce(-1);
#else
            if ((_count >= 10 && (_count - 10) % 2 == 0) || Environment.ProcessorCount == 1)
            {
                var yieldsSoFar = _count >= 10 ? (_count - 10) / 2 : _count;
                if (yieldsSoFar % 5 == 4)
                    Thread.Sleep(0);
                else
                    Thread.Yield();
            }
            else
            {
                var iterations = Environment.ProcessorCount / 2;
                if (_count <= 30 && 1 << _count < iterations)
                    iterations = 1 << _count;
                Thread.SpinWait(iterations);
            }

            _count = _count == int.MaxValue ? 10 : _count + 1;
#endif
        }

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
            if (sleepThreshold < -1)
                sleepThreshold = -1;
            if ((_count >= 10 && ((_count >= sleepThreshold && sleepThreshold >= 0) || (_count - 10) % 2 == 0)) || Environment.ProcessorCount == 1)
            {
                if (_count >= sleepThreshold && sleepThreshold >= 0)
                    Thread.Sleep(1);
                else if ((_count >= 10 ? (_count - 10) / 2 : _count) % 5 == 4)
                    Thread.Sleep(0);
                else
                    Thread.Yield();
            }
            else
            {
                var iterations = Environment.ProcessorCount / 2;
                if (_count <= 30 && 1 << _count < iterations)
                    iterations = 1 << _count;
                Thread.SpinWait(iterations);
            }

            _count = _count == int.MaxValue ? 10 : _count + 1;
#endif
        }

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="other">Other</param>
        /// <returns>Equals</returns>
        public bool Equals(NativeSpinWait other) => other == this;

        /// <summary>
        ///     Equals
        /// </summary>
        /// <param name="obj">object</param>
        /// <returns>Equals</returns>
        public override bool Equals(object? obj) => obj is NativeSpinWait nativeSpinWait && nativeSpinWait == this;

        /// <summary>
        ///     Get hashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => Unsafe.As<NativeSpinWait, int>(ref this);

        /// <summary>
        ///     To string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() => "NativeSpinWait";

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