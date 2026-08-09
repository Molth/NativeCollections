// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Represents an object that can be initialized.
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        ///     Performs initialization of the object.
        /// </summary>
        void Initialize();
    }
}