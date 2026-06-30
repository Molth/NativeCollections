// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Supports a simple iteration over a generic collection.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public unsafe interface IPtrIterator<T> where T : unmanaged
    {
        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" />
        ///     if the enumerator has passed the end of the collection.
        /// </returns>
        bool MoveNext();

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        void Reset();

        /// <summary>
        ///     Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        T* Current { get; }
    }
}