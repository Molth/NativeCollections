using System.Runtime.InteropServices;
using static crossbeam.PaddingHelpers;

namespace crossbeam
{
    [StructLayout(LayoutKind.Explicit, Size = CACHE_LINE_SIZE)]
    public struct CachePadding;
}