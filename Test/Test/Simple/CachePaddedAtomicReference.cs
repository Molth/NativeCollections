using System.Runtime.InteropServices;

#pragma warning disable CS9084

namespace Examples
{
    [StructLayout(LayoutKind.Explicit, Size = 2 * PaddingHelpers.CACHE_LINE_SIZE)]
    internal struct CachePaddedAtomicReference
    {
        [FieldOffset(1 * PaddingHelpers.CACHE_LINE_SIZE)]
        public nuint AtomicReference;
    }
}