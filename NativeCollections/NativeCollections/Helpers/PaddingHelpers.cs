// ReSharper disable ALL

using System.Runtime.InteropServices;

namespace NativeCollections
{
    /// <summary>
    ///     Padding helpers
    /// </summary>
    internal static class PaddingHelpers
    {
        /// <summary>
        ///     Catch line size
        /// </summary>
        public const int CACHE_LINE_SIZE = 128;

        /// <summary>
        ///     Padding
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct Padding
        {
            /// <summary>
            ///     Padding byte used for alignment calculation.
            /// </summary>
            private readonly byte _dummy;
        }
    }
}