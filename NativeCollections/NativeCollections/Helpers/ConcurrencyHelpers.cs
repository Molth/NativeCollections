using System;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Concurrency helpers
    /// </summary>
    internal static class ConcurrencyHelpers
    {
        /// <summary>
        ///     The number of concurrent writes for which to optimize by default.
        /// </summary>
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount * 4;
    }
}