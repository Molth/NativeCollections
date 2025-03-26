// ReSharper disable ALL

using System;

namespace NativeCollections
{
    /// <summary>
    ///     From type
    /// </summary>
    [Flags]
    public enum FromType
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0,

        /// <summary>
        ///     .NET Standard
        /// </summary>
        Standard = 1 << 0,

        /// <summary>
        ///     Community
        /// </summary>
        Community = 1 << 1,

        /// <summary>
        ///     C
        /// </summary>
        C = 1 << 2,

        /// <summary>
        ///     Rust
        /// </summary>
        Rust = 1 << 3
    }
}