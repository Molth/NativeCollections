using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable All

namespace crossbeam
{
    /// Performs exponential backoff in spin loops.
    /// <br />
    /// Backing off in spin loops reduces contention and improves overall performance.
    /// <br />
    /// This primitive can execute *YIELD* and *PAUSE* instructions, yield the current thread to the OS
    /// scheduler, and tell when is a good time to block the thread using a different synchronization
    /// mechanism. Each step of the back off procedure takes roughly twice as long as the previous
    /// step.
    [StructLayout(LayoutKind.Sequential)]
    internal struct Backoff
    {
        private const uint SPIN_LIMIT = 6;
        private const uint YIELD_LIMIT = 10;

        private uint step;

        /// Resets the `Backoff`.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void reset() => step = 0;

        /// Backs off in a lock-free loop.
        /// <br />
        /// This method should be used when we need to retry an operation because another thread made
        /// progress.
        /// <br />
        /// The processor may yield using the *YIELD* or *PAUSE* instruction.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void spin()
        {
            var count = 1 << (int)Math.Min(step, SPIN_LIMIT);
            Thread.SpinWait(count);

            if (step <= SPIN_LIMIT)
                step += 1;
        }

        /// Backs off in a blocking loop.
        /// <br />
        /// This method should be used when we need to wait for another thread to make progress.
        /// <br />
        /// The processor may yield using the *YIELD* or *PAUSE* instruction and the current thread
        /// may yield by giving up a timeslice to the OS scheduler.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void snooze()
        {
            if (step <= SPIN_LIMIT)
            {
                Thread.SpinWait(1 << (int)step);
            }
            else
            {
                Thread.Yield();
            }

            if (step <= YIELD_LIMIT)
                step += 1;
        }
    }
}