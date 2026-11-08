using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides unchecked access helpers for native arrays.
    /// </summary>
    internal static unsafe class ArrayExtensions
    {
        /// <summary>
        ///     Returns a pointer to the element at the specified index without performing bounds checking.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array.</typeparam>
        /// <param name="array">The array to access.</param>
        /// <param name="index">The zero-based index of the element to access.</param>
        /// <returns>A pointer to the element at <paramref name="index" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetUnchecked<T>(this NativeArray<T> array, nuint index) where T : unmanaged => UnsafeHelpers.Add<T>(array.Buffer, Isize.FromUsizeUnchecked(index));
    }
}