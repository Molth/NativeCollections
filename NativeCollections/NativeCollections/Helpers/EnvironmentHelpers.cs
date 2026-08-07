using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides information about, and means to manipulate,
    ///     the current environment and platform.
    /// </summary>
    internal static class EnvironmentHelpers
    {
#if !NET5_0_OR_GREATER
        /// <summary>
        ///     Gets a value indicating whether the current system has only one processor core.
        ///     On single-processor systems, spinning is less effective and may require different
        ///     synchronization strategies, such as yielding the thread more aggressively.
        /// </summary>
        public static bool IsSingleProcessor => Environment.ProcessorCount == 1;
#endif

        /// <summary>
        ///     The number of concurrent writes for which to optimize by default.
        /// </summary>
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount * 4;
    }
}