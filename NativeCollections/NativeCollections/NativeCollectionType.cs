// ReSharper disable ALL

using System;

namespace NativeCollections
{
    /// <summary>
    ///     Native collection type
    /// </summary>
   [Flags] public enum NativeCollectionType
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     .net standard
        /// </summary>
        Standard,

        /// <summary>
        ///     Community
        /// </summary>
        Community,

        /// <summary>
        ///     c
        /// </summary>
        C,

        /// <summary>
        ///     rust
        /// </summary>
        Rust
    }
}