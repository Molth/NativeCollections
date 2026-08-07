// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents the possible outcomes of an insertion operation on a collection.
    /// </summary>
    public enum InsertResult
    {
        /// <summary>
        ///     No operation was performed; used as a default or uninitialized state.
        /// </summary>
        None,

        /// <summary>
        ///     The item was successfully inserted into the collection.
        /// </summary>
        Success,

        /// <summary>
        ///     The item could not be inserted because an equivalent element already exists in the collection.
        /// </summary>
        AlreadyExists,

        /// <summary>
        ///     The item was inserted by overwriting an existing element.
        /// </summary>
        Overwritten,

        /// <summary>
        ///     The item could not be inserted because the collection has insufficient remaining capacity.
        /// </summary>
        InsufficientCapacity
    }
}