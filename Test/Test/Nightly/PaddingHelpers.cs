using System.Runtime.InteropServices;

namespace Examples
{
    [StructLayout(LayoutKind.Explicit, Size = PaddingHelpers.CACHE_LINE_SIZE)]
    internal struct Padding;

    internal static class PaddingHelpers
    {
        public const int CACHE_LINE_SIZE = 128;
    }
}