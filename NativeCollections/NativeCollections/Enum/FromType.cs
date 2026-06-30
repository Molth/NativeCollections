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
        None = 1 << 30,

        /// <summary>
        ///     .NET Standard
        /// </summary>
        /// <remarks>
        ///     This type is a direct rewrite of the corresponding .NET Standard collection types.
        ///     The API surface and runtime behavior are kept as close as possible to the original,
        ///     ensuring seamless substitution and predictable performance.
        ///     Any deviations are explicitly documented and are limited to low-level optimizations
        ///     that leverage native memory management.
        /// </remarks>
        Standard = 1 << 0,

        /// <summary>
        ///     Community
        /// </summary>
        /// <remarks>
        ///     This type is derived from community‑sourced implementations (e.g., open‑source libraries
        ///     or known algorithmic patterns). It has been adapted, integration with native memory primitives.
        ///     The original authors are acknowledged in the accompanying documentation.
        /// </remarks>
        Community = 1 << 1,

        /// <summary>
        ///     C
        /// </summary>
        /// <remarks>
        ///     This type is a cross‑language port from a C implementation.
        ///     The logic, data structures, and memory layout follow the original C design,
        ///     while being re‑implemented in C# with appropriate interop considerations.
        /// </remarks>
        C = 1 << 2,

        /// <summary>
        ///     Rust
        /// </summary>
        /// <remarks>
        ///     This type is a cross-language port from a Rust implementation.
        ///     It strives to mimic the original Rust implementation as closely as possible.
        ///     Although C# cannot enforce ownership checking at compile time, the implementation
        ///     can essentially reproduce the same semantics at the implementation level.
        /// </remarks>
        Rust = 1 << 3,

        /// <summary>
        ///     This type is a thin wrapper around the corresponding .NET Standard types.
        /// </summary>
        /// <remarks>
        ///     This design was chosen for the following reasons:
        ///     <list type="bullet">
        ///         <item>
        ///             To avoid use-after-free bugs,
        ///             introducing hazard pointers (HP) or
        ///             epoch-based reclamation (EBR) would likely be impractical.
        ///         </item>
        ///         <item>
        ///             To keep consistent with the latest behavior of .NET Standard.
        ///         </item>
        ///     </list>
        /// </remarks>
        NotImplemented = 1 << 4
    }
}