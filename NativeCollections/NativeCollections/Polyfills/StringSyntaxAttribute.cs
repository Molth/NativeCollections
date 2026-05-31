#if !NET7_0_OR_GREATER

// ReSharper disable ALL

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies the syntax used in a string.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    internal sealed class StringSyntaxAttribute : Attribute
    {
        /// <summary>The syntax identifier for strings containing composite formats for string formatting.</summary>
        public const string CompositeFormat = "CompositeFormat";

        /// <summary>The syntax identifier for strings containing date format specifiers.</summary>
        public const string DateOnlyFormat = "DateOnlyFormat";

        /// <summary>The syntax identifier for strings containing date and time format specifiers.</summary>
        public const string DateTimeFormat = "DateTimeFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.Enum" /> format specifiers.</summary>
        public const string EnumFormat = "EnumFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.Guid" /> format specifiers.</summary>
        public const string GuidFormat = "GuidFormat";

        /// <summary>The syntax identifier for strings containing JavaScript Object Notation (JSON).</summary>
        public const string Json = "Json";

        /// <summary>The syntax identifier for strings containing numeric format specifiers.</summary>
        public const string NumericFormat = "NumericFormat";

        /// <summary>The syntax identifier for strings containing regular expressions.</summary>
        public const string Regex = "Regex";

        /// <summary>The syntax identifier for strings containing time format specifiers.</summary>
        public const string TimeOnlyFormat = "TimeOnlyFormat";

        /// <summary>The syntax identifier for strings containing <see cref="T:System.TimeSpan" /> format specifiers.</summary>
        public const string TimeSpanFormat = "TimeSpanFormat";

        /// <summary>The syntax identifier for strings containing URIs.</summary>
        public const string Uri = "Uri";

        /// <summary>The syntax identifier for strings containing XML.</summary>
        public const string Xml = "Xml";

        /// <summary>
        ///     Initializes the <see cref="T:System.Diagnostics.CodeAnalysis.StringSyntaxAttribute" /> with the identifier of
        ///     the syntax used.
        /// </summary>
        /// <param name="syntax">The syntax identifier.</param>
        public StringSyntaxAttribute(string syntax)
        {
            Syntax = syntax;
            Arguments = Array.Empty<object>();
        }

        /// <summary>
        ///     Initializes the <see cref="T:System.Diagnostics.CodeAnalysis.StringSyntaxAttribute" /> with the identifier of
        ///     the syntax used.
        /// </summary>
        /// <param name="syntax">The syntax identifier.</param>
        /// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
        public StringSyntaxAttribute(string syntax, params object?[] arguments)
        {
            Syntax = syntax;
            Arguments = arguments;
        }

        /// <summary>Gets the identifier of the syntax used.</summary>
        public string Syntax { get; }

        /// <summary>Gets the optional arguments associated with the specific syntax employed.</summary>
        public object?[] Arguments { get; }
    }
}
#endif