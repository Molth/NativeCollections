#if !NET5_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides internal structures.
    /// </summary>
    internal static class SpinWaitHelpers
    {
        /// <summary>
        ///     These constants determine the frequency of yields versus spinning. The
        ///     numbers may seem fairly arbitrary, but were derived with at least some
        ///     thought in the design document.  I fully expect they will need to change
        ///     over time as we gain more experience with performance.
        /// </summary>
        /// <remarks>
        ///     When to switch over to a true yield.
        /// </remarks>
        private const int YIELD_THRESHOLD = 10;

        /// <remarks>
        ///     After how many yields should we Sleep(0)?
        /// </remarks>
        private const int SLEEP0_EVERY_HOW_MANY_YIELDS = 5;

        /// <remarks>
        ///     After how many yields should we Sleep(1) frequently?
        /// </remarks>
        private const int DEFAULT_SLEEP1_THRESHOLD = 20;

        /// <summary>
        ///     Provides support for spin-based waiting.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <see cref="SpinWait" /> encapsulates common spinning logic. On single-processor machines, yields are
        ///         always used instead of busy waits, and on computers with Intel(R) processors employing Hyper-Threading
        ///         technology, it helps to prevent hardware thread starvation. SpinWait encapsulates a good mixture of
        ///         spinning and true yielding.
        ///     </para>
        ///     <para>
        ///         <see cref="SpinWait" /> is a value type, which means that low-level code can utilize SpinWait without
        ///         fear of unnecessary allocation overheads. SpinWait is not generally useful for ordinary applications.
        ///         In most cases, you should use the synchronization classes provided by the .NET Framework, such as
        ///         <see cref="Monitor" />. For most purposes where spin waiting is required, however,
        ///         the <see cref="SpinWait" /> type should be preferred over the
        ///         <see
        ///             cref="Thread.SpinWait" />
        ///         method.
        ///     </para>
        ///     <para>
        ///         While SpinWait is designed to be used in concurrent applications, it is not designed to be
        ///         used from multiple threads concurrently.  SpinWait's members are not thread-safe.  If multiple
        ///         threads must spin, each should use its own instance of SpinWait.
        ///     </para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct SpinWait
        {
            /// <summary>
            ///     The number of times we've spun already.
            /// </summary>
            private int _count;

            /// <summary>
            ///     Gets the number of times <see cref="SpinOnce()" /> has been called on this instance.
            /// </summary>
            public readonly int Count => _count;

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
            public readonly bool NextSpinWillYield => _count >= YIELD_THRESHOLD || EnvironmentHelpers.IsSingleProcessor;

            /// <summary>
            ///     Resets the spin counter.
            /// </summary>
            /// <remarks>
            ///     This makes <see cref="SpinOnce()" /> and <see cref="NextSpinWillYield" /> behave as though no calls
            ///     to <see cref="SpinOnce()" /> had been issued on this instance. If a <see cref="SpinWait" /> instance
            ///     is reused many times, it may be useful to reset it to avoid yielding too soon.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _count = 0;

            /// <summary>
            ///     Performs a single spin.
            /// </summary>
            /// <remarks>
            ///     This is typically called in a loop, and may change in behavior based on the number of times a
            ///     <see cref="SpinOnce()" /> has been called thus far on this instance.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SpinOnce() => SpinOnceCore(DEFAULT_SLEEP1_THRESHOLD);

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
                ThrowHelpers.ThrowIfLessThan(sleep1Threshold, -1, ExceptionArgument.sleep1Threshold);
                if (sleep1Threshold >= 0 && sleep1Threshold < YIELD_THRESHOLD)
                    sleep1Threshold = YIELD_THRESHOLD;
                SpinOnceCore(sleep1Threshold);
            }

            /// <summary>
            ///     Performs a single spin.
            /// </summary>
            /// <param name="sleep1Threshold">
            ///     A minimum spin count after which <code>Thread.Sleep(1)</code> may be used.
            ///     A value of <code>-1</code> may be used to disable the use of <code>Thread.Sleep(1)</code>.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SpinOnceCore(int sleep1Threshold)
            {
                // (_count - YieldThreshold) % 2 == 0: The purpose of this check is to interleave Thread.Yield/Sleep(0) with
                // Thread.SpinWait. Otherwise, the following issues occur:
                //   - When there are no threads to switch to, Yield and Sleep(0) become no-op and it turns the spin loop into a
                //     busy-spin that may quickly reach the max spin count and cause the thread to enter a wait state, or may
                //     just busy-spin for longer than desired before a Sleep(1). Completing the spin loop too early can cause
                //     excessive context switching if a wait follows, and entering the Sleep(1) stage too early can cause
                //     excessive delays.
                //   - If there are multiple threads doing Yield and Sleep(0) (typically from the same spin loop due to
                //     contention), they may switch between one another, delaying work that can make progress.
                if ((_count >= YIELD_THRESHOLD && ((_count >= sleep1Threshold && sleep1Threshold >= 0) || (_count - YIELD_THRESHOLD) % 2 == 0)) || EnvironmentHelpers.IsSingleProcessor)
                {
                    //
                    // We must yield.
                    //
                    // We prefer to call Thread.Yield first, triggering a SwitchToThread. This
                    // unfortunately doesn't consider all runnable threads on all OS SKUs. In
                    // some cases, it may only consult the runnable threads whose ideal processor
                    // is the one currently executing code. Thus we occasionally issue a call to
                    // Sleep(0), which considers all runnable threads at equal priority. Even this
                    // is insufficient since we may be spin waiting for lower priority threads to
                    // execute; we therefore must call Sleep(1) once in a while too, which considers
                    // all runnable threads, regardless of ideal processor and priority, but may
                    // remove the thread from the scheduler's queue for 10+ms, if the system is
                    // configured to use the (default) coarse-grained system timer.
                    //

                    if (_count >= sleep1Threshold && sleep1Threshold >= 0)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        var yieldsSoFar = _count >= YIELD_THRESHOLD ? (_count - YIELD_THRESHOLD) / 2 : _count;
                        if (yieldsSoFar % SLEEP0_EVERY_HOW_MANY_YIELDS == SLEEP0_EVERY_HOW_MANY_YIELDS - 1)
                        {
                            Thread.Sleep(0);
                        }
                        else
                        {
                            Thread.Yield();
                        }
                    }
                }
                else
                {
                    //
                    // Otherwise, we will spin.
                    //
                    // We do this using the CLR's SpinWait API, which is just a busy loop that
                    // issues YIELD/PAUSE instructions to ensure multi-threaded CPUs can react
                    // intelligently to avoid starving. (These are NOOPs on other CPUs.) We
                    // choose a number for the loop iteration count such that each successive
                    // call spins for longer, to reduce cache contention.  We cap the total
                    // number of spins we are willing to tolerate to reduce delay to the caller,
                    // since we expect most callers will eventually block anyway.
                    //
                    // Also, cap the maximum spin count to a value such that many thousands of CPU cycles would not be wasted doing
                    // the equivalent of YieldProcessor(), as at that point SwitchToThread/Sleep(0) are more likely to be able to
                    // allow other useful work to run. Long YieldProcessor() loops can help to reduce contention, but Sleep(1) is
                    // usually better for that.
                    var n = ThreadHelpers.OptimalMaxSpinWaitsPerSpinIteration;
                    if (_count <= 30 && 1 << _count < n)
                    {
                        n = 1 << _count;
                    }

                    Thread.SpinWait(n);
                }

                // Finally, increment our spin counter.
                _count = _count == int.MaxValue ? YIELD_THRESHOLD : _count + 1;
            }
        }
    }
}
#endif