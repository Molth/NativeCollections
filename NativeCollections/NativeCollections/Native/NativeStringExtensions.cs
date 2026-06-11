#if NET6_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace NativeCollections
{
    /// <summary>
    ///     Native string extensions
    /// </summary>
    public static class NativeStringExtensions
    {
        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(in this NativeString builder, [InterpolatedStringHandlerArgument("builder")] ref NativeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());

        /// <summary>
        ///     Append formatted
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AppendInterpolated(in this NativeString builder, IFormatProvider? provider, [InterpolatedStringHandlerArgument("builder", "provider")] ref NativeStringInterpolatedStringHandler handler) => handler.TryCopyTo(ref builder.AsRef());
    }
}
#endif