using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides the assumed cache line size and the padding type used to align values to a cache line.
    /// </summary>
    internal static class PaddingHelpers
    {
        /// <summary>
        ///     Cache lines are assumed to be N bytes long, depending on the architecture.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>On x86-64, aarch64, and powerpc64, N = 128.</description>
        ///         </item>
        ///         <item>
        ///             <description>On arm, mips, mips64, sparc, and hexagon, N = 32.</description>
        ///         </item>
        ///         <item>
        ///             <description>On m68k, N = 16.</description>
        ///         </item>
        ///         <item>
        ///             <description>On s390x, N = 256.</description>
        ///         </item>
        ///         <item>
        ///             <description>On all others, N = 64.</description>
        ///         </item>
        ///     </list>
        ///     N is a reasonable guess and is not guaranteed to match the actual cache line length
        ///     of the machine on which the program is running. On modern Intel architectures,
        ///     the spatial prefetcher pulls pairs of 64-byte cache lines at a time, so this
        ///     implementation pessimistically assumes that cache lines are 128 bytes long.
        /// </remarks>
        public const int CACHE_LINE_SIZE = 128;

        /// <summary>
        ///     Pads a field to the length of a cache line so that neighboring fields do not share a cache line with it.
        /// </summary>
        /// <remarks>
        ///     In concurrent programming, sometimes it is desirable to make sure commonly accessed pieces of data
        ///     are not placed into the same cache line. Updating an atomic value invalidates the whole cache line
        ///     it belongs to, which makes the next access to the same cache line slower for other CPU cores. Use
        ///     <see cref="CachePadding" /> to ensure updating one piece of data does not invalidate other cached data.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, Size = CACHE_LINE_SIZE)]
        public readonly struct CachePadding
        {
            /// <summary>
            ///     Padding byte used for alignment calculation.
            /// </summary>
            private readonly byte _dummy;
        }
    }
}