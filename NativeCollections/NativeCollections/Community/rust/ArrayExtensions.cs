using System.Runtime.CompilerServices;
using NativeCollections;

// ReSharper disable All

namespace rust
{
    internal static unsafe class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* get_unchecked<T>(this NativeArray<T> array, nuint index) where T : unmanaged => UnsafeHelpers.Add<T>(array.Buffer, (nint)index);
    }
}