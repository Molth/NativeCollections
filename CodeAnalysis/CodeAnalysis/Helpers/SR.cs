// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Provides the strings shared by the analyzers and code fixes of this project.
    /// </summary>
    internal static class SR
    {
        /// <summary>
        ///     The prefix of the diagnostics that guard raw pointer usage.
        /// </summary>
        public const string DIAGNOSTIC_ID_PTR = "PTR";

        /// <summary>
        ///     The prefix of the diagnostics that guard the public API shape and overflow-aware arithmetic.
        /// </summary>
        public const string DIAGNOSTIC_ID_DESIGN = "DESIGN";

        /// <summary>
        ///     The category reported for every diagnostic of this project.
        /// </summary>
        public const string CATEGORY_SAFETY = "Safety";

        /// <summary>
        ///     The metadata name of the <c>System.Runtime.CompilerServices.Unsafe</c> helper type.
        /// </summary>
        /// <remarks>
        ///     Matching is done on the resolved symbol, never on syntax, so a
        ///     user-declared type named <c>Unsafe</c> is not mistaken for the BCL helper.
        /// </remarks>
        public const string UNSAFE_TYPE_NAME = "System.Runtime.CompilerServices.Unsafe";

        /// <summary>
        ///     The root namespace of the analyzed library.
        /// </summary>
        /// <remarks>
        ///     Helper exemptions are restricted to this namespace so that
        ///     the same method names in other assemblies are still analyzed.
        /// </remarks>
        public const string NATIVE_COLLECTIONS_NAMESPACE = "NativeCollections";

        /// <summary>
        ///     The name of the wrapper type of <see cref="int" />, which is the type of every integral literal.
        /// </summary>
        public const string INT32_TYPE_NAME = "I32";

        /// <summary>
        ///     The name of the native-sized signed wrapper type of the analyzed library.
        /// </summary>
        /// <remarks>
        ///     The type declares <c>FromPtrUnchecked</c>, which is the only pointer conversion helper of the library.
        /// </remarks>
        public const string ISIZE_TYPE_NAME = "Isize";

        /// <summary>
        ///     The name that the pointer diagnostics use for a value that converts from a pointer.
        /// </summary>
        /// <remarks>
        ///     The analyzed library has no pointer wrapper type, so the name is used
        ///     for the message and for the <c>FromPtrUnchecked</c> helper lookup only.
        /// </remarks>
        public const string PTR_TYPE_NAME = "Ptr";
    }
}