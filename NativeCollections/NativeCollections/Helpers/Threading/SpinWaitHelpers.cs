#if !NET5_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides support for spin-based waiting and the thresholds that control when spinning yields the processor.
    /// </summary>
    internal static class SpinWaitHelpers
    {
        /// <summary>
        ///     These constants determine the frequency of yields versus spinning. The numbers may seem
        ///     fairly arbitrary, but were derived with at least some thought in the design document. I
        ///     fully expect they will need to change over time as we gain more experience with performance.
        /// </summary>
        /// <remarks>
        ///     When to switch over to a true yield.
        /// </remarks>
        private const int YIELD_THRESHOLD = 10;

        /// <summary>
        ///     The number of yields between calls to <see cref="Thread.Sleep(int)" />.
        /// </summary>
        private const int SLEEP0_EVERY_HOW_MANY_YIELDS = 5;

        /// <summary>
        ///     The default minimum spin count after which <see cref="Thread.Sleep(int)" /> may be used.
        /// </summary>
        private const int DEFAULT_SLEEP1_THRESHOLD = 20;

        /// <summary>
        ///     Provides support for spin-based waiting.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <see cref="SpinWait" /> encapsulates common spinning logic. On single-processor
        ///         machines, yields are always used instead of busy waits, and on computers with Intel(R)
        ///         processors employing Hyper-Threading technology, it helps to prevent hardware thread
        ///         starvation. SpinWait encapsulates a good mixture of spinning and true yielding.
        ///     </para>
        ///     <para>
        ///         <see cref="SpinWait" /> is a value type, which means that low-level code can utilize
        ///         SpinWait without fear of unnecessary allocation overheads. SpinWait is not generally
        ///         useful for ordinary applications. In most cases, you should use the synchronization
        ///         classes provided by the .NET Framework, such as <see cref="Monitor" />. For most
        ///         purposes where spin waiting is required, however, the <see cref="SpinWait" />
        ///         type should be preferred over the <see cref="Thread.SpinWait" /> method.
        ///     </para>
        ///     <para>
        ///         While <see cref="SpinWait" /> is designed to be used in concurrent applications, it is
        ///         not designed to be used from multiple threads concurrently. SpinWait's members are not
        ///         thread-safe. If multiple threads must spin, each should use its own instance of SpinWait.
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
            public readonly bool NextSpinWillYield => _count >= YIELD_THRESHOLD || EnvironmentHelpers.IsSingleProcessor;

            /// <summary>
            ///     Resets the spin counter.
            /// </summary>
            /// <remarks>
            ///     This makes <see cref="SpinOnce()" /> and <see cref="NextSpinWillYield" /> behave as though no
            ///     calls to <see cref="SpinOnce()" /> had been issued on this instance. If a <see cref="SpinWait" />
            ///     instance is reused many times, it may be useful to reset it to avoid yielding too soon.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => _count = 0;

            /// <summary>
            ///     Performs a single spin.
            /// </summary>
            /// <remarks>
            ///     This is typically called in a loop, and may change in behavior based on the number
            ///     of times a <see cref="SpinOnce()" /> has been called thus far on this instance.
            /// </remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SpinOnce() => SpinOnceCore(DEFAULT_SLEEP1_THRESHOLD);

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
            ///     A minimum spin count after which <code>Thread.Sleep(1)</code> may be used. A value
            ///     of <code>-1</code> may be used to disable the use of <code>Thread.Sleep(1)</code>.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SpinOnceCore(int sleep1Threshold)
            {
                if ((_count >= YIELD_THRESHOLD && ((_count >= sleep1Threshold && sleep1Threshold >= 0) || _count.WrappingSub(YIELD_THRESHOLD) % 2 == 0)) || EnvironmentHelpers.IsSingleProcessor)
                {
                    if (_count >= sleep1Threshold && sleep1Threshold >= 0)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        var yieldsSoFar = _count >= YIELD_THRESHOLD ? _count.WrappingSub(YIELD_THRESHOLD) / 2 : _count;
                        if (yieldsSoFar % SLEEP0_EVERY_HOW_MANY_YIELDS == SLEEP0_EVERY_HOW_MANY_YIELDS.WrappingSub(1))
                            Thread.Sleep(0);
                        else
                            Thread.Yield();
                    }
                }
                else
                {
                    var n = ThreadHelpers.OptimalMaxSpinWaitsPerSpinIteration;
                    if (_count <= 30 && 1 << _count < n)
                        n = 1 << _count;

                    Thread.SpinWait(n);
                }

                _count = _count == int.MaxValue ? YIELD_THRESHOLD : _count.WrappingAdd(1);
            }
        }
    }
}
#endif