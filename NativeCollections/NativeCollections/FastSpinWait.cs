using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Fast spin wait
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public ref struct FastSpinWait
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
        ///     Empty
        /// </summary>
        public static FastSpinWait Empty => new();
    }
}