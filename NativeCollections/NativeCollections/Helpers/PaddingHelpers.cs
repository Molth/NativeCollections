using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Pads and aligns a value to the length of a cache line.
    /// </summary>
    internal static class PaddingHelpers
    {
        /// Cache lines are assumed to be N bytes long, depending on the architecture:
        /// <br />
        /// * On x86-64, aarch64, and powerpc64, N = 128.
        /// <br />
        /// * On arm, mips, mips64, sparc, and hexagon, N = 32.
        /// <br />
        /// * On m68k, N = 16.
        /// <br />
        /// * On s390x, N = 256.
        /// <br />
        /// * On all others, N = 64.
        /// <remarks>
        ///     Note that N is just a reasonable guess and is not guaranteed to match the actual cache line
        ///     length of the machine the program is running on. On modern Intel architectures, spatial
        ///     prefetcher is pulling pairs of 64-byte cache lines at a time, so we pessimistically assume that
        ///     cache lines are 128 bytes long.
        /// </remarks>
        public const int CACHE_LINE_SIZE = 128;

        /// Pads and aligns a value to the length of a cache line.
        /// <br />
        /// In concurrent programming, sometimes it is desirable to make sure commonly accessed pieces of
        /// data are not placed into the same cache line. Updating an atomic value invalidates the whole
        /// cache line it belongs to, which makes the next access to the same cache line slower for other
        /// CPU cores. Use `CachePadded` to ensure updating one piece of data doesn't invalidate other
        /// cached data.
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