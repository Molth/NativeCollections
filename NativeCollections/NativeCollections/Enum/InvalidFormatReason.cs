// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Invalid format reason
    /// </summary>
    internal enum InvalidFormatReason
    {
        /// <summary>
        ///     Unexpected closing brace
        /// </summary>
        UnexpectedClosingBrace,

        /// <summary>
        ///     Expected ascii digit
        /// </summary>
        ExpectedAsciiDigit,

        /// <summary>
        ///     Unclosed format item
        /// </summary>
        UnclosedFormatItem
    }
}