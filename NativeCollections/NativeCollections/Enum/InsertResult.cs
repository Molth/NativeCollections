// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Insert result
    /// </summary>
    public enum InsertResult
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     Success
        /// </summary>
        Success,

        /// <summary>
        ///     Already exists
        /// </summary>
        AlreadyExists,

        /// <summary>
        ///     Overwritten
        /// </summary>
        Overwritten,

        /// <summary>
        ///     Insufficient capacity
        /// </summary>
        InsufficientCapacity
    }
}