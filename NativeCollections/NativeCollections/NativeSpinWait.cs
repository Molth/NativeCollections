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
    [NativeCollection]
    public ref struct NativeSpinWait
    {
        /// <summary>
        ///     Count
        /// </summary>
        private int _count;

        /// <summary>
        ///     Count
        /// </summary>
        public int Count => _count;

        /// <summary>
        ///     Next spin will yield
        /// </summary>
        public bool NextSpinWillYield
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count >= 10 || Environment.ProcessorCount == 1;
        }

        /// <summary>
        ///     Reset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _count = 0;

        /// <summary>
        ///     Spin once
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce()
        {
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
        }

        /// <summary>
        ///     Spin once
        /// </summary>
        /// <param name="sleepThreshold">Sleep threshold</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SpinOnce(int sleepThreshold)
        {
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
        }

        /// <summary>
        ///     Empty
        /// </summary>
        public static NativeSpinWait Empty => new();
    }
}