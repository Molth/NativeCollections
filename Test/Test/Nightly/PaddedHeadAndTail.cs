using System.Runtime.InteropServices;

namespace Examples
{
    /// <summary>
    ///     Padded head and tail indices, to avoid false sharing between producers and consumers.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3 * PaddingHelpers.CACHE_LINE_SIZE)]
    internal struct PaddedHeadAndTail
    {
        /// <summary>
        ///     Head
        /// </summary>
        [FieldOffset(1 * PaddingHelpers.CACHE_LINE_SIZE)]
        public int Head;

        /// <summary>
        ///     Tail
        /// </summary>
        [FieldOffset(2 * PaddingHelpers.CACHE_LINE_SIZE)]
        public int Tail;
    }
}