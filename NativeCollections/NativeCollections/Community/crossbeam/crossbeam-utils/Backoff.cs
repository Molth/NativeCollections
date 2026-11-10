using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Performs exponential backoff in spin loops.
    /// </summary>
    /// <remarks>
    ///     Backing off in spin loops reduces contention and improves overall performance. This primitive can
    ///     execute <c>YIELD</c> and <c>PAUSE</c> instructions and yield the current thread to the operating
    ///     system scheduler. Each step of the backoff procedure takes roughly twice as long as the previous step.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Backoff
    {
        /// <summary>
        ///     The maximum step used to calculate a processor spin duration.
        ///     <see cref="Snooze" /> spins through this step, then yields the current thread.
        /// </summary>
        private const uint SPIN_LIMIT = 6U;

        /// <summary>
        ///     The highest step from which <see cref="Snooze" /> increments the
        ///     backoff counter; it does not limit calls to <see cref="Thread.Yield" />.
        /// </summary>
        private const uint YIELD_LIMIT = 10U;

        /// <summary>
        ///     The current step of the exponential backoff procedure.
        /// </summary>
        private uint _step;

        /// <summary>
        ///     Resets the backoff procedure.
        /// </summary>
        public void Reset() => _step = 0U;

        /// <summary>
        ///     Backs off in a lock-free loop.
        /// </summary>
        /// <remarks>
        ///     Use this method to retry an operation because another thread made progress.
        ///     The processor may yield using the <c>YIELD</c> or <c>PAUSE</c> instruction.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Spin()
        {
            var count = 1 << I32.FromU32Unchecked(Math.Min(_step, SPIN_LIMIT));
            Thread.SpinWait(count);

            if (_step <= SPIN_LIMIT)
                _step = _step.WrappingAdd(1U);
        }

        /// <summary>
        ///     Backs off in a blocking loop.
        /// </summary>
        /// <remarks>
        ///     Use this method to wait for another thread to make progress. The processor
        ///     may yield using the <c>YIELD</c> or <c>PAUSE</c> instruction, and the current
        ///     thread may yield by giving up a time slice to the operating system scheduler.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Snooze()
        {
            if (_step <= SPIN_LIMIT)
                Thread.SpinWait(1 << I32.FromU32Unchecked(_step));
            else
                Thread.Yield();

            if (_step <= YIELD_LIMIT)
                _step = _step.WrappingAdd(1U);
        }
    }
}