using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable All

namespace crossbeam
{
    internal static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T get_unchecked<T>(this T[] array, nuint index) => ref Unsafe.Add<T>(ref MemoryMarshal.GetArrayDataReference(array), (nint)index);
    }
}