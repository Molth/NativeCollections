#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Provides extension methods for string builders.
    /// </summary>
    public static class UnsafeStringExtensions
    {
        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to append to.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <returns>
        ///     <see langword="true" /> if the formatted value was appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(ref this UnsafeString builder, [InterpolatedStringHandlerArgument("builder")] ref UnsafeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());

        /// <summary>
        ///     Appends the interpolated string produced by the specified handler to this instance.
        /// </summary>
        /// <param name="builder">The string builder to append to.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <param name="handler">The interpolated string handler to append.</param>
        /// <returns>
        ///     <see langword="true" /> if the formatted value was appended; otherwise, <see langword="false" />.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(ref this UnsafeString builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref UnsafeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());
    }
}
#endif