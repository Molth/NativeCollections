// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides a property that indicates whether the current instance
    ///     has been successfully allocated or initialized.
    /// </summary>
    public interface IIsCreated
    {
        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        bool IsCreated { get; }
    }
}