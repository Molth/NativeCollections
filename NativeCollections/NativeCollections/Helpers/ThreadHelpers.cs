#if !NET5_0_OR_GREATER
// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Creates and controls a thread, sets its priority, and gets its status.
    /// </summary>
    internal static class ThreadHelpers
    {
        /// <summary>
        ///     Gets the optimal maximum number of spin waits per spin iteration.
        ///     This value is used to control the spinning behavior in spinlock implementations,
        ///     balancing CPU consumption and responsiveness.
        /// </summary>
        /// <remarks>
        ///     The value of 7 is a heuristic assumption based on common processor architectures and
        ///     typical workloads. It may not be optimal for all environments; consider profiling and
        ///     adjusting this value according to actual performance metrics if necessary.
        /// </remarks>
        public static int OptimalMaxSpinWaitsPerSpinIteration => 7;
    }
}
#endif